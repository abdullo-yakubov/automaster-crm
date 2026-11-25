using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize(Roles = "Admin,Manager")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVehicles([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var (items, total) = await _vehicleService.GetVehiclesAsync(search, page, limit);
        return Ok(new { items, total, page, limit });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _vehicleService.GetVehicleByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateVehicleRequest request)
    {
        var result = await _vehicleService.CreateVehicleAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleRequest request) =>
        Ok(await _vehicleService.UpdateVehicleAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _vehicleService.DeleteVehicleAsync(id);
        return NoContent();
    }
}
