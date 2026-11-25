namespace AutoMaster.Admin.Models;

public record CurrentUser(string Id, string Email, string FullName, string Role, string? Phone, bool IsActive);

public record LoginResponse(string AccessToken, string RefreshToken, CurrentUser User);

public record AdminUser(string Id, string Email, string FullName, string Role, string? Phone, bool IsActive, DateTime CreatedAt);

public record CreateAdminUserRequest(string Email, string Password, string FullName, string Role, string? Phone);

public record UpdateAdminUserRequest(string FullName, string Role, string? Phone);

public record AdminNetworkStats(int UsersCount, int OrdersCount, int CustomersCount, decimal TotalRevenue);

public record OrderItem(string Id, string OrderNumber, string CustomerName, string VehicleMakeModel,
    string LicensePlate, string Status, decimal TotalCost, DateTime CreatedAt);

public record PagedResult<T>(List<T> Items, int Total, int Page, int Limit);

public record CustomerItem(string Id, string FullName, string Phone, string Status, decimal TotalSpent, int LoyaltyPoints);

public record BranchItem(string Id, string Name, string Address, string Phone, string Type, bool IsActive,
    int OrdersCount, decimal Revenue, double Nps);

public record FinanceStats(decimal RevenueThisMonth, decimal ExpensesThisMonth, decimal Profit, double MarginPercent);

public record AuditLogItem(string Id, DateTime CreatedAt, string UserName, string Action, string ObjectType, string ObjectId, string? Details);

public class AppConfigModel
{
    public string Id { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Currency { get; set; } = "TJS";
    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "ru";
}
