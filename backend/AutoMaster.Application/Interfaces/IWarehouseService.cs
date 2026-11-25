using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IWarehouseService
{
    Task<List<PartDto>> GetPartsAsync(string? filter, string? search);
    Task<PartDto> CreatePartAsync(CreatePartRequest request);
    Task<PartDto> UpdatePartAsync(Guid id, UpdatePartRequest request);
    Task DeletePartAsync(Guid id);
    Task<List<PartDto>> GetLowStockAsync();
    Task<PartDto> UpdateQuantityAsync(Guid id, UpdatePartQuantityRequest request);
    Task<WarehouseStatsDto> GetStatsAsync();
}
