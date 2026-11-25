using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IFinanceService _financeService;

    public AdminController(IAdminService adminService, IOrderService orderService,
        ICustomerService customerService, IFinanceService financeService)
    {
        _adminService = adminService;
        _orderService = orderService;
        _customerService = customerService;
        _financeService = financeService;
    }

    [HttpGet("api/admin/users")]
    public async Task<IActionResult> GetUsers() => Ok(await _adminService.GetUsersAsync());

    [HttpPost("api/admin/users")]
    public async Task<IActionResult> CreateUser(CreateAdminUserRequest request) => Ok(await _adminService.CreateUserAsync(request));

    [HttpPut("api/admin/users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateAdminUserRequest request) => Ok(await _adminService.UpdateUserAsync(id, request));

    [HttpPatch("api/admin/users/{id:guid}/block")]
    public async Task<IActionResult> ToggleBlock(Guid id) => Ok(await _adminService.ToggleBlockAsync(id));

    [HttpGet("api/admin/network-stats")]
    public async Task<IActionResult> NetworkStats() => Ok(await _adminService.GetNetworkStatsAsync());

    [HttpPost("api/admin/reset-demo")]
    public async Task<IActionResult> ResetDemo()
    {
        await _adminService.ResetDemoDataAsync();
        return NoContent();
    }

    [HttpGet("api/admin/orders")]
    public async Task<IActionResult> AllOrders([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var (items, total) = await _orderService.GetOrdersAsync(status, search, page, limit);
        return Ok(new { items, total, page, limit });
    }

    [HttpGet("api/admin/customers")]
    public async Task<IActionResult> AllCustomers([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var (items, total) = await _customerService.GetCustomersAsync(search, status, page, limit);
        return Ok(new { items, total, page, limit });
    }

    [HttpGet("api/admin/finance-overview")]
    public async Task<IActionResult> FinanceOverview() => Ok(await _financeService.GetStatsAsync());
}
