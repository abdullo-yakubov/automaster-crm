using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var ordersToday = await _db.Orders.CountAsync(o => DateOnly.FromDateTime(o.CreatedAt) == today);
        var ordersYesterday = await _db.Orders.CountAsync(o => DateOnly.FromDateTime(o.CreatedAt) == yesterday);
        var changePercent = ordersYesterday == 0 ? 0 : (ordersToday - ordersYesterday) * 100.0 / ordersYesterday;

        var revenueToday = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt != null && DateOnly.FromDateTime(o.CompletedAt.Value) == today)
            .SumAsync(o => o.TotalCost);

        var activeOrders = await _db.Orders.CountAsync(o => o.Status != OrderStatus.Completed);
        var totalCustomers = await _db.Customers.CountAsync();

        var completedOrders = await _db.Orders.Where(o => o.Status == OrderStatus.Completed).ToListAsync();
        var avgTicket = completedOrders.Count > 0 ? completedOrders.Average(o => o.TotalCost) : 0;

        return new DashboardStatsDto(ordersToday, changePercent, revenueToday, activeOrders, totalCustomers, avgTicket);
    }

    public async Task<List<RevenuePointDto>> GetRevenueAsync(int days)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(days - 1));
        var orders = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt != null && DateOnly.FromDateTime(o.CompletedAt.Value) >= since)
            .ToListAsync();

        var grouped = orders.GroupBy(o => DateOnly.FromDateTime(o.CompletedAt!.Value))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalCost));

        var result = new List<RevenuePointDto>();
        for (var d = since; d <= DateOnly.FromDateTime(DateTime.UtcNow); d = d.AddDays(1))
            result.Add(new RevenuePointDto(d, grouped.GetValueOrDefault(d)));

        return result;
    }

    public async Task<List<ServiceDistributionDto>> GetServicesDistributionAsync()
    {
        var orders = await _db.Orders.Select(o => o.ServiceDescription).ToListAsync();
        return orders
            .GroupBy(s => s)
            .Select(g => new ServiceDistributionDto(g.Key, g.Count()))
            .OrderByDescending(s => s.Count)
            .Take(10)
            .ToList();
    }
}
