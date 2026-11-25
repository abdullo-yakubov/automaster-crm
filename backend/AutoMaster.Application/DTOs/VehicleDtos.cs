namespace AutoMaster.Application.DTOs;

public record VehicleDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Make,
    string Model,
    int Year,
    string LicensePlate,
    string? VIN,
    int Mileage,
    int VisitCount,
    DateTime CreatedAt);

public record CreateVehicleRequest(
    Guid? CustomerId,
    string? OwnerName,
    string Make,
    string Model,
    int Year,
    string LicensePlate,
    string? VIN,
    int Mileage);

public record UpdateVehicleRequest(
    string Make,
    string Model,
    int Year,
    string LicensePlate,
    string? VIN,
    int Mileage);
