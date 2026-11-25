using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;
    private static readonly string[] AllSlots = Enumerable.Range(8, 10).Select(h => $"{h:D2}:00").ToArray();

    public ScheduleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BookingDto>> GetBookingsAsync(DateOnly date)
    {
        var bookings = await _db.Bookings
            .Where(b => b.Date == date && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.Time)
            .ToListAsync();
        return bookings.Select(ToDto).ToList();
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        var time = TimeOnly.Parse(request.Time);
        var booking = new Booking
        {
            CustomerName = request.CustomerName,
            Phone = request.Phone,
            Date = request.Date,
            Time = time,
            Service = request.Service,
            Status = BookingStatus.Confirmed
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return ToDto(booking);
    }

    public async Task<List<SlotDto>> GetSlotsAsync(DateOnly date)
    {
        var taken = await _db.Bookings
            .Where(b => b.Date == date && b.Status != BookingStatus.Cancelled)
            .Select(b => b.Time)
            .ToListAsync();

        var takenStrings = taken.Select(t => t.ToString("HH:mm")).ToHashSet();
        return AllSlots.Select(s => new SlotDto(s, !takenStrings.Contains(s))).ToList();
    }

    public async Task DeleteBookingAsync(Guid id)
    {
        var booking = await _db.Bookings.FindAsync(id) ?? throw new KeyNotFoundException("Запись не найдена");
        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    private static BookingDto ToDto(Booking b) =>
        new(b.Id, b.CustomerName, b.Phone, b.Date, b.Time.ToString("HH:mm"), b.Service, b.Status.ToString());
}
