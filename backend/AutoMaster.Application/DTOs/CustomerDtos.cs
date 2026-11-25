namespace AutoMaster.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string FullName,
    string Phone,
    string? Email,
    DateOnly? Birthday,
    string Status,
    int LoyaltyPoints,
    decimal TotalSpent,
    string? Note,
    DateTime CreatedAt,
    DateTime? LastVisit,
    int VehicleCount);

public record CustomerDetailDto(CustomerDto Customer, List<OrderDto> Orders, List<VehicleDto> Vehicles);

public record CreateCustomerRequest(
    string FullName,
    string Phone,
    string? Email,
    DateOnly? Birthday,
    string? Status,
    string? Note,
    string? VehicleMake,
    string? VehicleModel,
    string? LicensePlate);

public record UpdateCustomerRequest(
    string FullName,
    string Phone,
    string? Email,
    DateOnly? Birthday,
    string Status,
    string? Note);
