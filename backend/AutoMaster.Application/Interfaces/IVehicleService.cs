using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IVehicleService
{
    Task<(List<VehicleDto> Items, int Total)> GetVehiclesAsync(string? search, int page, int limit);
    Task<VehicleDto> GetVehicleByIdAsync(Guid id);
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleRequest request);
    Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request);
    Task DeleteVehicleAsync(Guid id);
}
