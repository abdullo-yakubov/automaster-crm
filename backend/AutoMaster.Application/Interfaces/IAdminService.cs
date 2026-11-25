using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto> CreateUserAsync(CreateAdminUserRequest request);
    Task<AdminUserDto> UpdateUserAsync(Guid id, UpdateAdminUserRequest request);
    Task<AdminUserDto> ToggleBlockAsync(Guid id);
    Task<AdminNetworkStatsDto> GetNetworkStatsAsync();
    Task ResetDemoDataAsync();
}
