using System.Globalization;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class BiService : IBiService
{
    /// <summary>Заказ считается нарушившим SLA, если он открыт дольше этого срока.</summary>
    public const int SlaHours = 48;

    private static readonly string[] DefaultWidgets = { "revenue_card", "orders_chart", "mechanics_kpi_widget", "warehouse_alert" };
    private readonly AppDbContext _db;
    private readonly IAnalyticsService _analytics;

    public BiService(AppDbContext db, IAnalyticsService analytics)
    {
        _db = db;
        _analytics = analytics;
    }

    public async Task<List<string>> GetWidgetsAsync(Guid userId)
    {
        var dash = await _db.UserDashboards.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId);
        return dash?.Widgets ?? DefaultWidgets.ToList();
    }

    public async Task<List<string>> SaveWidgetsAsync(Guid userId, List<string> widgets)
    {
        var clean = widgets.Where(w => IBiService.AvailableWidgets.Contains(w)).Distinct().ToList();
        var dash = await _db.UserDashboards.FirstOrDefaultAsync(d => d.UserId == userId);
        if (dash is null)
        {
            dash = new UserDashboard { UserId = userId };
            _db.UserDashboards.Add(dash);
        }
        dash.Widgets = clean;
        dash.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return clean;
    }

    public async Task<BiSummaryDto> GetSummaryAsync()
    {
        var nowLocal = TimeZoneHelper.ToLocal(DateTime.UtcNow);
        var monthStart = TimeZoneHelper.ToUtc(new DateTime(nowLocal.Year, nowLocal.Month, 1));
        var prevStart = TimeZoneHelper.ToUtc(new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-1));

        var completed = _db.Orders.Where(o => o.Status == OrderStatus.Completed && o.CompletedAt != null);
        var thisMonth = await completed.Where(o => o.CompletedAt >= monthStart).SumAsync(o => (decimal?)o.TotalCost) ?? 0;
        var lastMonth = await completed.Where(o => o.CompletedAt >= prevStart && o.CompletedAt < monthStart).SumAsync(o => (decimal?)o.TotalCost) ?? 0;
        var change = lastMonth == 0 ? 0 : Math.Round((double)((thisMonth - lastMonth) / lastMonth * 100), 1);

        // Заказы за последние 7 дней по дням
        var weekStartLocal = nowLocal.Date.AddDays(-6);
        var weekCreated = await _db.Orders.Where(o => o.CreatedAt >= TimeZoneHelper.ToUtc(weekStartLocal)).Select(o => o.CreatedAt).ToListAsync();
        var ru = new CultureInfo("ru-RU");
        var labels = new List<string>();
        var counts = new List<int>();
        for (var i = 0; i < 7; i++)
        {
            var day = weekStartLocal.AddDays(i);
            labels.Add(ru.DateTimeFormat.GetAbbreviatedDayName(day.DayOfWeek));
            counts.Add(weekCreated.Count(c => TimeZoneHelper.ToLocal(c).Date == day));
        }

        var mechanics = await _analytics.GetMechanicPerformanceAsync();
        var topServices = await _analytics.GetPopularServicesAsync(5);

        var lowStock = await _db.Parts.Where(p => p.Quantity <= p.MinQuantity)
            .OrderBy(p => p.Quantity).Take(10)
            .Select(p => new LowStockPartDto(p.Id, p.Name, p.Quantity, p.MinQuantity)).ToListAsync();

        var income = await _db.Transactions.Where(t => t.Type == TransactionType.Income).SumAsync(t => (decimal?)t.Amount) ?? 0;
        var expense = await _db.Transactions.Where(t => t.Type == TransactionType.Expense).SumAsync(t => (decimal?)t.Amount) ?? 0;
        var margin = income > 0 ? (int)Math.Round((income - expense) / income * 100) : 0;

        var now = DateTime.UtcNow;
        var segments = new List<SegmentCountDto>
        {
            new("VIP", await _db.Customers.CountAsync(c => c.Status == CustomerStatus.Vip)),
            new("Постоянные", await _db.Customers.CountAsync(c => c.Orders.Count >= 2)),
            new("Новые (30 дн.)", await _db.Customers.CountAsync(c => c.CreatedAt >= now.AddDays(-30))),
            new("Спящие (90+ дн.)", await _db.Customers.CountAsync(c => c.Orders.Any() && !c.Orders.Any(o => o.CreatedAt >= now.AddDays(-90)))),
        };

        var active = await _db.Orders.CountAsync(o => o.Status != OrderStatus.Completed);
        var slaBorder = now.AddHours(-SlaHours);
        var violations = await _db.Orders.CountAsync(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Ready && o.CreatedAt < slaBorder);

        return new BiSummaryDto(thisMonth, lastMonth, change, labels, counts, mechanics.FirstOrDefault(), lowStock, topServices,
            income, expense, margin, segments, active, violations, SlaHours);
    }
}
