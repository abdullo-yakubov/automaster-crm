using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Accountant")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("api/reports/daily/pdf")]
    public async Task<IActionResult> DailyPdf([FromQuery] DateTime? date)
    {
        var bytes = await _reportService.GenerateDailyPdfAsync(date ?? DateTime.UtcNow);
        return File(bytes, "application/pdf", $"daily-report-{(date ?? DateTime.UtcNow):yyyyMMdd}.pdf");
    }

    [HttpGet("api/reports/weekly/pdf")]
    public async Task<IActionResult> WeeklyPdf([FromQuery] DateTime? weekEnd)
    {
        var bytes = await _reportService.GenerateWeeklyPdfAsync(weekEnd ?? DateTime.UtcNow);
        return File(bytes, "application/pdf", $"weekly-report-{(weekEnd ?? DateTime.UtcNow):yyyyMMdd}.pdf");
    }
}
