using System.Globalization;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    /// <summary>Бонус мастеру — процент от выручки по завершённым заказам.</summary>
    public const decimal BonusRate = 0.03m;

    private static readonly CultureInfo Ru = new("ru-RU");
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PopularServiceDto>> GetPopularServicesAsync(int top = 10)
    {
        top = Math.Clamp(top, 1, 50);
        var rows = await _db.Orders
            .GroupBy(o => o.ServiceDescription)
            .Select(g => new
            {
                Service = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(o => o.Status == OrderStatus.Completed ? o.TotalCost : 0m)
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync();
        return rows.Select(r => new PopularServiceDto(r.Service, r.Count, r.Revenue)).ToList();
    }

    public async Task<List<MechanicPerformanceDto>> GetMechanicPerformanceAsync()
    {
        var stats = await _db.Mechanics
            .Where(m => m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.FullName,
                m.Level,
                m.Rating,
                m.Salary,
                Total = m.Orders.Count,
                Completed = m.Orders.Count(o => o.Status == OrderStatus.Completed),
                Revenue = m.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalCost)
            })
            .ToListAsync();

        return stats
            .Select(m => new MechanicPerformanceDto(
                m.Id, m.FullName, m.Level.ToString(), m.Total, m.Completed, m.Revenue, m.Rating, m.Salary,
                Math.Round(m.Revenue * BonusRate, 2)))
            .OrderByDescending(m => m.Revenue)
            .ToList();
    }

    public async Task<HeatmapDto> GetHeatmapAsync(int days = 90)
    {
        days = Math.Clamp(days, 7, 365);
        var since = DateTime.UtcNow.AddDays(-days);
        var created = await _db.Orders.Where(o => o.CreatedAt >= since).Select(o => o.CreatedAt).ToListAsync();

        // Пн..Сб, рабочие часы 08..18
        var dayOrder = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        var hours = Enumerable.Range(8, 11).ToList();
        var values = dayOrder.Select(_ => hours.Select(_ => 0).ToList()).ToList();

        foreach (var utc in created)
        {
            var local = TimeZoneHelper.ToLocal(utc);
            var di = Array.IndexOf(dayOrder, local.DayOfWeek);
            var hi = hours.IndexOf(local.Hour);
            if (di >= 0 && hi >= 0) values[di][hi]++;
        }

        var labels = dayOrder.Select(d => Ru.DateTimeFormat.GetAbbreviatedDayName(d)).ToList();
        var max = values.SelectMany(v => v).DefaultIfEmpty(0).Max();
        return new HeatmapDto(labels, hours, values, max);
    }

    public async Task<ForecastDto> GetForecastAsync()
    {
        var nowLocal = TimeZoneHelper.ToLocal(DateTime.UtcNow);
        var firstMonth = new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-5);
        var sinceUtc = TimeZoneHelper.ToUtc(firstMonth);

        var completed = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt != null && o.CompletedAt >= sinceUtc)
            .Select(o => new { o.CompletedAt, o.TotalCost })
            .ToListAsync();

        var byMonth = completed
            .GroupBy(o => { var l = TimeZoneHelper.ToLocal(o.CompletedAt!.Value); return new DateTime(l.Year, l.Month, 1); })
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalCost));

        var history = Enumerable.Range(0, 6)
            .Select(i => firstMonth.AddMonths(i))
            .Select(m => new MonthlyRevenueDto(MonthLabel(m), byMonth.GetValueOrDefault(m)))
            .ToList();

        // Линейная регрессия по месяцам, где есть данные (текущий месяц — неполный, его масштабируем).
        var points = new List<(double X, double Y)>();
        for (var i = 0; i < history.Count; i++)
        {
            var y = (double)history[i].Revenue;
            if (i == history.Count - 1)
            {
                var daysInMonth = DateTime.DaysInMonth(nowLocal.Year, nowLocal.Month);
                y = y / Math.Max(1, nowLocal.Day) * daysInMonth;
            }
            if (y > 0) points.Add((i, y));
        }

        double slope = 0, intercept = points.Count > 0 ? points.Average(p => p.Y) : 0;
        if (points.Count >= 2)
        {
            var mx = points.Average(p => p.X);
            var my = points.Average(p => p.Y);
            var den = points.Sum(p => (p.X - mx) * (p.X - mx));
            slope = den == 0 ? 0 : points.Sum(p => (p.X - mx) * (p.Y - my)) / den;
            intercept = my - slope * mx;
        }

        var forecast = new List<ForecastPointDto>();
        for (var i = 1; i <= 6; i++)
        {
            var x = history.Count - 1 + i;
            var expected = (decimal)Math.Max(0, intercept + slope * x);
            expected = Math.Round(expected, 0);
            forecast.Add(new ForecastPointDto(
                MonthLabel(firstMonth.AddMonths(5 + i)),
                expected,
                Math.Round(expected * 1.15m, 0),
                Math.Round(expected * 0.85m, 0)));
        }

        var baseline = points.Count > 0 ? points.Average(p => p.Y) : 0;
        var growth = baseline > 0 ? Math.Round(slope / baseline * 100, 1) : 0;
        var next = forecast[0].Expected;
        var total = forecast.Sum(f => f.Expected);

        var insights = new List<string>();
        if (points.Count < 2)
        {
            insights.Add("Недостаточно истории (нужно минимум 2 месяца с выручкой) — прогноз построен по среднему значению.");
        }
        else
        {
            insights.Add($"Прогноз выручки на следующий месяц: {next:N0} TJS.");
            insights.Add(growth >= 0
                ? $"Среднемесячный рост: +{growth}% по тренду последних месяцев."
                : $"Тренд снижается: {growth}% в месяц — стоит запустить маркетинговую кампанию.");
        }
        insights.Add($"Суммарный прогноз на 6 месяцев: {total:N0} TJS (±15%).");

        return new ForecastDto(history, forecast, growth, next, total, insights);
    }

    private static string MonthLabel(DateTime m) => m.ToString("MMM yy", Ru);
}

/// <summary>Душанбе — UTC+5 без перехода на летнее время.</summary>
public static class TimeZoneHelper
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(5);
    public static DateTime ToLocal(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Unspecified) + Offset;
    public static DateTime ToUtc(DateTime local) => DateTime.SpecifyKind(local - Offset, DateTimeKind.Utc);
}
