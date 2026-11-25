namespace AutoMaster.Application.DTOs;

public record PopularServiceDto(string Service, int Count, decimal Revenue);

public record MechanicPerformanceDto(
    Guid MechanicId,
    string FullName,
    string Level,
    int TotalOrders,
    int CompletedOrders,
    decimal Revenue,
    decimal Rating,
    decimal Salary,
    decimal Bonus);

/// <summary>Матрица загрузки: Values[day][hour] — количество заказов.</summary>
public record HeatmapDto(List<string> Days, List<int> Hours, List<List<int>> Values, int Max);

public record MonthlyRevenueDto(string Month, decimal Revenue);

public record ForecastPointDto(string Month, decimal Expected, decimal Optimistic, decimal Pessimistic);

public record ForecastDto(
    List<MonthlyRevenueDto> History,
    List<ForecastPointDto> Forecast,
    double MonthlyGrowthPercent,
    decimal NextMonth,
    decimal NextSixMonthsTotal,
    List<string> Insights);
