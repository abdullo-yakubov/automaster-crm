using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class OperationsController : ControllerBase
{
    private readonly IOperationsService _operationsService;

    public OperationsController(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    [HttpGet("api/checklists/{type}")]
    public async Task<IActionResult> GetChecklist(string type) => Ok(await _operationsService.GetChecklistAsync(type));

    [HttpPatch("api/checklists/{type}/{itemId:guid}")]
    public async Task<IActionResult> ToggleItem(string type, Guid itemId) => Ok(await _operationsService.ToggleChecklistItemAsync(type, itemId));

    [HttpPost("api/checklists/items")]
    public async Task<IActionResult> AddItem(AddChecklistItemRequest request) => Ok(await _operationsService.AddChecklistItemAsync(request));

    [HttpGet("api/iot/sensors")]
    public async Task<IActionResult> GetSensors() => Ok(await _operationsService.GetSensorsAsync());

    [HttpGet("api/orders/sla-violations")]
    public async Task<IActionResult> GetSlaViolations() => Ok(await _operationsService.GetSlaViolationsAsync());
}
