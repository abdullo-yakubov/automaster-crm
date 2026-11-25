using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Manager,Accountant")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await _dashboardService.GetStatsAsync());

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] int days = 7) => Ok(await _dashboardService.GetRevenueAsync(days));

    [HttpGet("services-distribution")]
    public async Task<IActionResult> GetServicesDistribution() => Ok(await _dashboardService.GetServicesDistributionAsync());
}
