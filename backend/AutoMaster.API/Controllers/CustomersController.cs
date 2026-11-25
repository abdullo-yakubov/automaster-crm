using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Manager")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var (items, total) = await _customerService.GetCustomersAsync(search, status, page, limit);
        return Ok(new { items, total, page, limit });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _customerService.GetCustomerByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        var result = await _customerService.CreateCustomerAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request) =>
        Ok(await _customerService.UpdateCustomerAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}
