using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface ILoyaltyService
{
    Task<LoyaltyStatsDto> GetStatsAsync();
    Task<List<LoyaltyTopCustomerDto>> GetTopAsync(int limit);
    Task AwardAsync(AwardPointsRequest request);
    Task RedeemAsync(RedeemPointsRequest request);
}
