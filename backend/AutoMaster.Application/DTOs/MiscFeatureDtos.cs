namespace AutoMaster.Application.DTOs;

// ===== Voice =====
public record VoiceCommandRequest(string Transcript);

/// <summary>Распознанная команда. Route — куда перейти во frontend, PrefillService — данные для формы заказа.</summary>
public record VoiceCommandResultDto(string Intent, bool Recognized, string Message, string? Route, string? PrefillService);

public record VoiceHistoryItemDto(Guid Id, string Transcript, string Intent, string Result, DateTime CreatedAt);

// ===== Client portal =====
public record PortalOrderDto(Guid Id, string OrderNumber, string Vehicle, string Service, string Status, string? MechanicName, decimal Total, DateTime CreatedAt, DateTime? CompletedAt);

public record PortalVehicleDto(Guid Id, string Title, string LicensePlate, int Year, int Mileage, string? VIN);

public record PortalNotificationDto(string Text, string Kind);

public record ClientPortalDto(
    Guid CustomerId,
    string FullName,
    string Phone,
    string Status,
    int LoyaltyPoints,
    int Visits,
    decimal TotalSpent,
    PortalOrderDto? ActiveOrder,
    List<PortalVehicleDto> Vehicles,
    List<PortalOrderDto> History,
    List<PortalNotificationDto> Notifications);

// ===== QR =====
public record SignedQrDto(string Type, string Payload, string Label, DateTime ExpiresAt);

public record OrderQrRequest(Guid OrderId);

public record PromoQrRequest(string Code);

public record QrVerifyRequest(string Payload);

public record QrVerificationDto(bool IsValid, string? Type, Dictionary<string, string>? Data, string? Reason);

// ===== BI =====
public record SaveWidgetsRequest(List<string> Widgets);

public record LowStockPartDto(Guid Id, string Name, int Quantity, int MinQuantity);

public record SegmentCountDto(string Segment, int Count);

public record BiSummaryDto(
    decimal RevenueThisMonth,
    decimal RevenueLastMonth,
    double RevenueChangePercent,
    List<string> WeekdayLabels,
    List<int> OrdersByWeekday,
    MechanicPerformanceDto? TopMechanic,
    List<LowStockPartDto> LowStock,
    List<PopularServiceDto> TopServices,
    decimal Income,
    decimal Expense,
    int MarginPercent,
    List<SegmentCountDto> Segments,
    int ActiveOrders,
    int SlaViolations,
    int SlaHours);
