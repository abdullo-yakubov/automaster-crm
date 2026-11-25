namespace AutoMaster.Application.DTOs;

public record AdminUserDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt);

public record CreateAdminUserRequest(string Email, string Password, string FullName, string Role, string? Phone);

public record UpdateAdminUserRequest(string FullName, string Role, string? Phone);

public record AdminNetworkStatsDto(
    int UsersCount,
    int OrdersCount,
    int CustomersCount,
    decimal TotalRevenue);
