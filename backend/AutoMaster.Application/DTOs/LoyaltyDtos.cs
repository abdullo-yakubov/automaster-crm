namespace AutoMaster.Application.DTOs;

public record LoyaltyStatsDto(
    int TotalMembers,
    int VipCount,
    int PointsAwarded,
    int PointsRedeemed);

public record LoyaltyTopCustomerDto(
    Guid CustomerId,
    string FullName,
    string Status,
    int LoyaltyPoints,
    decimal TotalSpent);

public record AwardPointsRequest(Guid CustomerId, int Points, string? Note);

public record RedeemPointsRequest(Guid CustomerId, int Points, string? Note);
