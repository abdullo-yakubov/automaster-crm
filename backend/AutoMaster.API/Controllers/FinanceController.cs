using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,Accountant")]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;

    public FinanceController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet("api/finance/stats")]
    public async Task<IActionResult> Stats() => Ok(await _financeService.GetStatsAsync());

    [HttpGet("api/finance/transactions")]
    public async Task<IActionResult> GetTransactions() => Ok(await _financeService.GetTransactionsAsync());

    [HttpPost("api/finance/transactions")]
    public async Task<IActionResult> CreateTransaction(CreateTransactionRequest request) =>
        Ok(await _financeService.CreateTransactionAsync(request));

    [HttpGet("api/finance/chart")]
    public async Task<IActionResult> Chart([FromQuery] int months = 6) =>
        Ok(await _financeService.GetChartAsync(months));

    [HttpGet("api/finance/expenses-structure")]
    public async Task<IActionResult> ExpensesStructure() =>
        Ok(await _financeService.GetExpensesStructureAsync());
}
