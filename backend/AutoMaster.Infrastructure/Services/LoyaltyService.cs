using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly AppDbContext _db;

    public LoyaltyService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LoyaltyStatsDto> GetStatsAsync()
    {
        var totalMembers = await _db.Customers.CountAsync();
        var vipCount = await _db.Customers.CountAsync(c => c.Status == CustomerStatus.Vip || c.Status == CustomerStatus.Corporate);
        var awarded = await _db.LoyaltyTransactions.Where(t => t.Type == LoyaltyTransactionType.Award).SumAsync(t => t.Points);
        var redeemed = await _db.LoyaltyTransactions.Where(t => t.Type == LoyaltyTransactionType.Redeem).SumAsync(t => t.Points);

        return new LoyaltyStatsDto(totalMembers, vipCount, awarded, redeemed);
    }

    public async Task<List<LoyaltyTopCustomerDto>> GetTopAsync(int limit)
    {
        var customers = await _db.Customers
            .OrderByDescending(c => c.LoyaltyPoints)
            .Take(limit)
            .ToListAsync();

        return customers
            .Select(c => new LoyaltyTopCustomerDto(c.Id, c.FullName, c.Status.ToString(), c.LoyaltyPoints, c.TotalSpent))
            .ToList();
    }

    public async Task AwardAsync(AwardPointsRequest request)
    {
        var customer = await _db.Customers.FindAsync(request.CustomerId) ?? throw new KeyNotFoundException("Клиент не найден");
        customer.LoyaltyPoints += request.Points;
        _db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            CustomerId = customer.Id,
            Type = LoyaltyTransactionType.Award,
            Points = request.Points,
            Note = request.Note
        });
        await _db.SaveChangesAsync();
    }

    public async Task RedeemAsync(RedeemPointsRequest request)
    {
        var customer = await _db.Customers.FindAsync(request.CustomerId) ?? throw new KeyNotFoundException("Клиент не найден");
        if (customer.LoyaltyPoints < request.Points)
            throw new InvalidOperationException("Недостаточно баллов");

        customer.LoyaltyPoints -= request.Points;
        _db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            CustomerId = customer.Id,
            Type = LoyaltyTransactionType.Redeem,
            Points = request.Points,
            Note = request.Note
        });
        await _db.SaveChangesAsync();
    }
}
