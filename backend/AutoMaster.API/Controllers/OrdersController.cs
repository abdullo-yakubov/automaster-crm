using System.Security.Claims;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetOrders([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var (items, total) = await _orderService.GetOrdersAsync(status, search, page, limit);
        return Ok(new { items, total, page, limit });
    }

    [HttpGet("kanban")]
    [Authorize(Roles = "Admin,Manager,Mechanic")]
    public async Task<IActionResult> GetKanban() => Ok(await _orderService.GetKanbanAsync());

    [HttpGet("export/csv")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<IActionResult> ExportCsv()
    {
        var bytes = await _orderService.ExportCsvAsync();
        return File(bytes, "text/csv", $"orders-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Mechanic")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _orderService.GetOrderByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, UpdateOrderRequest request) =>
        Ok(await _orderService.UpdateOrderAsync(id, request));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Mechanic")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request) =>
        Ok(await _orderService.UpdateStatusAsync(id, request, CurrentUserId));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
