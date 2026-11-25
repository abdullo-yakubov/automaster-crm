using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMaster.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class BookingsController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public BookingsController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("api/bookings")]
    public async Task<IActionResult> GetBookings([FromQuery] DateOnly date) =>
        Ok(await _scheduleService.GetBookingsAsync(date));

    [HttpPost("api/bookings")]
    public async Task<IActionResult> Create(CreateBookingRequest request) =>
        Ok(await _scheduleService.CreateBookingAsync(request));

    [HttpGet("api/bookings/slots")]
    public async Task<IActionResult> GetSlots([FromQuery] DateOnly date) =>
        Ok(await _scheduleService.GetSlotsAsync(date));

    [HttpDelete("api/bookings/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _scheduleService.DeleteBookingAsync(id);
        return NoContent();
    }
}
