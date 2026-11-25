using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IScheduleService
{
    Task<List<BookingDto>> GetBookingsAsync(DateOnly date);
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request);
    Task<List<SlotDto>> GetSlotsAsync(DateOnly date);
    Task DeleteBookingAsync(Guid id);
}
