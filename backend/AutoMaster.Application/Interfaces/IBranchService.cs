using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IBranchService
{
    Task<List<BranchDto>> GetBranchesAsync();
    Task<BranchDto> CreateBranchAsync(CreateBranchRequest request);
    Task<BranchDto> UpdateBranchAsync(Guid id, UpdateBranchRequest request);
    Task DeleteBranchAsync(Guid id);
    Task<NetworkKpiDto> GetNetworkKpiAsync();
}
