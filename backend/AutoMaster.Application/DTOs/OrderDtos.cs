namespace AutoMaster.Application.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    Guid VehicleId,
    string VehicleMakeModel,
    string LicensePlate,
    Guid? MechanicId,
    string? MechanicName,
    string ServiceDescription,
    string Status,
    string Priority,
    decimal LaborCost,
    decimal PartsCost,
    decimal TotalCost,
    string? Note,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt);

public record OrderTimelineDto(Guid Id, string Status, string? Note, string? UserName, DateTime CreatedAt);

public record OrderDetailDto(OrderDto Order, List<OrderTimelineDto> Timeline);

public record CreateOrderRequest(
    string CustomerName,
    string CustomerPhone,
    string VehicleMake,
    string VehicleModel,
    string LicensePlate,
    string? VIN,
    int Mileage,
    string ServiceDescription,
    Guid? MechanicId,
    string Priority,
    decimal LaborCost,
    decimal PartsCost,
    string? Note);

public record UpdateOrderRequest(
    string ServiceDescription,
    Guid? MechanicId,
    string Priority,
    decimal LaborCost,
    decimal PartsCost,
    string? Note);

public record UpdateOrderStatusRequest(string Status, string? Note);

public record KanbanColumnDto(string Status, List<OrderDto> Orders);
