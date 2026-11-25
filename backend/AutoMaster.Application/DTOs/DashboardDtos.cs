namespace AutoMaster.Application.DTOs;

public record DashboardStatsDto(
    int OrdersToday,
    double OrdersTodayChangePercent,
    decimal RevenueToday,
    int ActiveOrders,
    int TotalCustomers,
    decimal AverageTicket);

public record RevenuePointDto(DateOnly Date, decimal Revenue);

public record ServiceDistributionDto(string Service, int Count);

public record TopMechanicDto(Guid MechanicId, string FullName, int CompletedOrders, decimal Revenue, decimal Rating);
