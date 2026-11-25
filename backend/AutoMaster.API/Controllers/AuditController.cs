using System.Security.Claims;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("api/audit/logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLogs([FromQuery] Guid? userId, [FromQuery] string? objectType,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        Ok(await _auditService.GetLogsAsync(userId, objectType, from, to));

    [HttpPost("api/audit/mystery-shopper")]
    public async Task<IActionResult> SaveMysteryAudit(SaveMysteryAuditRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _auditService.SaveMysteryAuditAsync(userId, request));
    }

    [HttpGet("api/audit/mystery-shopper")]
    public async Task<IActionResult> GetMysteryAudits() => Ok(await _auditService.GetMysteryAuditsAsync());
}
