namespace AutoMaster.Application.DTOs;

public record BookingDto(
    Guid Id,
    string CustomerName,
    string Phone,
    DateOnly Date,
    string Time,
    string Service,
    string Status);

public record CreateBookingRequest(
    string CustomerName,
    string Phone,
    DateOnly Date,
    string Time,
    string Service);

public record SlotDto(string Time, bool IsFree);
