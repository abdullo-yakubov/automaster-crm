using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface ISettingsService
{
    Task<SettingsDto> GetSettingsAsync();
    Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsRequest request);
    Task<DataExportDto> ExportDataAsync();
    Task ImportDataAsync(DataExportDto data);
}
