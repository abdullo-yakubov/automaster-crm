using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class FinanceService : IFinanceService
{
    private readonly AppDbContext _db;

    public FinanceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FinanceStatsDto> GetStatsAsync()
    {
        var monthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthStartUtc = DateTime.SpecifyKind(monthStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var orderRevenue = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt >= monthStartUtc)
            .SumAsync(o => o.TotalCost);

        var txIncome = await _db.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date >= monthStart)
            .SumAsync(t => t.Amount);

        var expenses = await _db.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= monthStart)
            .SumAsync(t => t.Amount);

        var revenue = orderRevenue + txIncome;
        var profit = revenue - expenses;
        var margin = revenue > 0 ? (double)(profit / revenue * 100) : 0;

        return new FinanceStatsDto(revenue, expenses, profit, Math.Round(margin, 1));
    }

    public async Task<List<TransactionDto>> GetTransactionsAsync()
    {
        var txs = await _db.Transactions.OrderByDescending(t => t.Date).ToListAsync();
        return txs.Select(ToDto).ToList();
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var tx = new Transaction
        {
            Type = Enum.Parse<TransactionType>(request.Type, true),
            Amount = request.Amount,
            Description = request.Description,
            Category = request.Category,
            Date = request.Date
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return ToDto(tx);
    }

    public async Task<List<FinanceChartPointDto>> GetChartAsync(int months)
    {
        var result = new List<FinanceChartPointDto>();
        var now = DateTime.UtcNow;

        for (int i = months - 1; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var start = new DateOnly(monthDate.Year, monthDate.Month, 1);
            var end = start.AddMonths(1);
            var startUtc = DateTime.SpecifyKind(start.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(end.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

            var income = await _db.Transactions
                .Where(t => t.Type == TransactionType.Income && t.Date >= start && t.Date < end)
                .SumAsync(t => t.Amount);
            var orderIncome = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed
                    && o.CompletedAt >= startUtc
                    && o.CompletedAt < endUtc)
                .SumAsync(o => o.TotalCost);
            var expense = await _db.Transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date >= start && t.Date < end)
                .SumAsync(t => t.Amount);

            result.Add(new FinanceChartPointDto(start.ToString("yyyy-MM"), income + orderIncome, expense));
        }

        return result;
    }

    public async Task<List<ExpenseCategoryDto>> GetExpensesStructureAsync()
    {
        var groups = await _db.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new ExpenseCategoryDto(g.Key, g.Sum(t => t.Amount)))
            .ToListAsync();
        return groups;
    }

    private static TransactionDto ToDto(Transaction t) =>
        new(t.Id, t.Type.ToString(), t.Amount, t.Description, t.Category, t.Date);
}
