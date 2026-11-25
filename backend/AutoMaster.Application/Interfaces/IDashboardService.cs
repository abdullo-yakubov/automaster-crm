using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<List<RevenuePointDto>> GetRevenueAsync(int days);
    Task<List<ServiceDistributionDto>> GetServicesDistributionAsync();
}
