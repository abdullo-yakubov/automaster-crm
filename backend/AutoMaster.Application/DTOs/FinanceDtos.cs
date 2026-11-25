namespace AutoMaster.Application.DTOs;

public record TransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    string Description,
    string Category,
    DateOnly Date);

public record CreateTransactionRequest(
    string Type,
    decimal Amount,
    string Description,
    string Category,
    DateOnly Date);

public record FinanceStatsDto(
    decimal RevenueThisMonth,
    decimal ExpensesThisMonth,
    decimal Profit,
    double MarginPercent);

public record FinanceChartPointDto(string Month, decimal Income, decimal Expense);

public record ExpenseCategoryDto(string Category, decimal Amount);
