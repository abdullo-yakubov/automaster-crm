using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet("api/parts")]
    public async Task<IActionResult> GetParts([FromQuery] string? filter, [FromQuery] string? search) =>
        Ok(await _warehouseService.GetPartsAsync(filter, search));

    [HttpPost("api/parts")]
    public async Task<IActionResult> Create(CreatePartRequest request) => Ok(await _warehouseService.CreatePartAsync(request));

    [HttpPut("api/parts/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePartRequest request) => Ok(await _warehouseService.UpdatePartAsync(id, request));

    [HttpDelete("api/parts/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _warehouseService.DeletePartAsync(id);
        return NoContent();
    }

    [HttpGet("api/parts/low-stock")]
    public async Task<IActionResult> LowStock() => Ok(await _warehouseService.GetLowStockAsync());

    [HttpPatch("api/parts/{id:guid}/quantity")]
    public async Task<IActionResult> UpdateQuantity(Guid id, UpdatePartQuantityRequest request) =>
        Ok(await _warehouseService.UpdateQuantityAsync(id, request));

    [HttpGet("api/warehouse/stats")]
    public async Task<IActionResult> Stats() => Ok(await _warehouseService.GetStatsAsync());
}
