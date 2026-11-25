using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("api/settings")]
    public async Task<IActionResult> Get() => Ok(await _settingsService.GetSettingsAsync());

    [HttpPut("api/settings")]
    public async Task<IActionResult> Update(UpdateSettingsRequest request) => Ok(await _settingsService.UpdateSettingsAsync(request));

    [HttpGet("api/data/export")]
    public async Task<IActionResult> Export() => Ok(await _settingsService.ExportDataAsync());

    [HttpPost("api/data/import")]
    public async Task<IActionResult> Import(DataExportDto data)
    {
        await _settingsService.ImportDataAsync(data);
        return NoContent();
    }
}
