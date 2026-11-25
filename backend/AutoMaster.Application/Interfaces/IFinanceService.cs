using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IFinanceService
{
    Task<FinanceStatsDto> GetStatsAsync();
    Task<List<TransactionDto>> GetTransactionsAsync();
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request);
    Task<List<FinanceChartPointDto>> GetChartAsync(int months);
    Task<List<ExpenseCategoryDto>> GetExpensesStructureAsync();
}
