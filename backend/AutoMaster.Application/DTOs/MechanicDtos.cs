namespace AutoMaster.Application.DTOs;

public record MechanicDto(
    Guid Id,
    string FullName,
    string? Phone,
    string Specialization,
    string Level,
    decimal Salary,
    bool IsActive,
    decimal Rating,
    int CompletedOrders,
    decimal Revenue,
    double WorkloadPercent);

public record MechanicKpiDto(
    Guid MechanicId,
    string FullName,
    int CompletedOrders,
    decimal Revenue,
    decimal AverageTicket,
    decimal Rating,
    double WorkloadPercent);

public record CreateMechanicRequest(
    string FullName,
    string? Phone,
    string Specialization,
    string Level,
    decimal Salary);

public record UpdateMechanicRequest(
    string FullName,
    string? Phone,
    string Specialization,
    string Level,
    decimal Salary,
    bool IsActive);
