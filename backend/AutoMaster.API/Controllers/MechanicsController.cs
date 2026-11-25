using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Route("api/mechanics")]
[Authorize]
public class MechanicsController : ControllerBase
{
    private readonly IMechanicService _mechanicService;

    public MechanicsController(IMechanicService mechanicService)
    {
        _mechanicService = mechanicService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetMechanics() => Ok(await _mechanicService.GetMechanicsAsync());

    [HttpGet("top")]
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public async Task<IActionResult> GetTop([FromQuery] int limit = 10) => Ok(await _mechanicService.GetTopMechanicsAsync(limit));

    [HttpGet("{id:guid}/kpi")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetKpi(Guid id) => Ok(await _mechanicService.GetMechanicKpiAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateMechanicRequest request) => Ok(await _mechanicService.CreateMechanicAsync(request));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateMechanicRequest request) => Ok(await _mechanicService.UpdateMechanicAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mechanicService.DeleteMechanicAsync(id);
        return NoContent();
    }
}
