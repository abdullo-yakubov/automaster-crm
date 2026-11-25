using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IMechanicService
{
    Task<List<MechanicDto>> GetMechanicsAsync();
    Task<MechanicKpiDto> GetMechanicKpiAsync(Guid id);
    Task<MechanicDto> CreateMechanicAsync(CreateMechanicRequest request);
    Task<MechanicDto> UpdateMechanicAsync(Guid id, UpdateMechanicRequest request);
    Task DeleteMechanicAsync(Guid id);
    Task<List<TopMechanicDto>> GetTopMechanicsAsync(int limit);
}
