namespace AutoMaster.Application.DTOs;

public record SettingsDto(
    Guid Id,
    string ServiceName,
    string Address,
    string Phone,
    string Currency,
    string Theme,
    string Language);

public record UpdateSettingsRequest(
    string ServiceName,
    string Address,
    string Phone,
    string Currency,
    string Theme,
    string Language);

public record DataExportDto(
    DateTime ExportedAt,
    object Customers,
    object Vehicles,
    object Orders,
    object Mechanics,
    object Parts,
    object Transactions,
    object Settings);
