using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize(Roles = "Admin")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches() => Ok(await _branchService.GetBranchesAsync());

    [HttpGet("network-kpi")]
    public async Task<IActionResult> GetNetworkKpi() => Ok(await _branchService.GetNetworkKpiAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateBranchRequest request) => Ok(await _branchService.CreateBranchAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBranchRequest request) => Ok(await _branchService.UpdateBranchAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _branchService.DeleteBranchAsync(id);
        return NoContent();
    }
}
