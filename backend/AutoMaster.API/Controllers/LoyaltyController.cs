using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpGet("api/loyalty/stats")]
    public async Task<IActionResult> Stats() => Ok(await _loyaltyService.GetStatsAsync());

    [HttpGet("api/loyalty/top")]
    public async Task<IActionResult> Top([FromQuery] int limit = 10) =>
        Ok(await _loyaltyService.GetTopAsync(limit));

    [HttpPost("api/loyalty/award")]
    public async Task<IActionResult> Award(AwardPointsRequest request)
    {
        await _loyaltyService.AwardAsync(request);
        return NoContent();
    }

    [HttpPost("api/loyalty/redeem")]
    public async Task<IActionResult> Redeem(RedeemPointsRequest request)
    {
        await _loyaltyService.RedeemAsync(request);
        return NoContent();
    }
}
