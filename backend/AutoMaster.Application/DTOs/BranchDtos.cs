namespace AutoMaster.Application.DTOs;

public record BranchDto(
    Guid Id,
    string Name,
    string Address,
    string Phone,
    string Type,
    bool IsActive,
    int OrdersCount,
    decimal Revenue,
    double Nps);

public record CreateBranchRequest(string Name, string Address, string Phone, string Type);

public record UpdateBranchRequest(string Name, string Address, string Phone, string Type, bool IsActive);

public record NetworkKpiDto(
    int BranchesCount,
    decimal TotalRevenue,
    int TotalOrders,
    double AverageNps,
    List<BranchDto> Branches);
