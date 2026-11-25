namespace AutoMaster.Application.DTOs;

public record ServiceBoxDto(
    Guid Id,
    string Name,
    string Specialization,
    bool IsActive,
    Guid? OrderId,
    string? OrderNumber,
    string? CustomerName,
    string? Vehicle,
    string? Service,
    string? MechanicName,
    DateTime? OccupiedSince,
    int ProgressPercent);

public record QueuedOrderDto(Guid Id, string OrderNumber, string CustomerName, string Vehicle, string Service, string Status, string Priority);

public record HourlyLoadDto(int Hour, int Actual, int Forecast);

public record DigitalTwinDto(
    List<ServiceBoxDto> Boxes,
    List<QueuedOrderDto> Queue,
    int TotalBoxes,
    int BusyBoxes,
    int LoadPercent,
    double AverageRepairHours,
    int ThroughputPerDay,
    List<HourlyLoadDto> HourlyLoad,
    List<string> Insights);

public record AssignBoxRequest(Guid OrderId);

public record CreateServiceBoxRequest(string Name, string Specialization);
