using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _db;

    public SettingsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SettingsDto> GetSettingsAsync()
    {
        var settings = await GetOrCreateAsync();
        return ToDto(settings);
    }

    public async Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        var settings = await GetOrCreateAsync();
        settings.ServiceName = request.ServiceName;
        settings.Address = request.Address;
        settings.Phone = request.Phone;
        settings.Currency = request.Currency;
        settings.Theme = request.Theme;
        settings.Language = request.Language;
        settings.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(settings);
    }

    public async Task<DataExportDto> ExportDataAsync()
    {
        return new DataExportDto(
            DateTime.UtcNow,
            await _db.Customers.ToListAsync(),
            await _db.Vehicles.ToListAsync(),
            await _db.Orders.ToListAsync(),
            await _db.Mechanics.ToListAsync(),
            await _db.Parts.ToListAsync(),
            await _db.Transactions.ToListAsync(),
            await GetOrCreateAsync());
    }

    public Task ImportDataAsync(DataExportDto data)
    {
        // v1: импорт полного дампа выходит за рамки текущего этапа — данные принимаются, но не перезаписываются
        return Task.CompletedTask;
    }

    private async Task<AppSettings> GetOrCreateAsync()
    {
        var settings = await _db.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings();
            _db.AppSettings.Add(settings);
            await _db.SaveChangesAsync();
        }
        return settings;
    }

    private static SettingsDto ToDto(AppSettings s) =>
        new(s.Id, s.ServiceName, s.Address, s.Phone, s.Currency, s.Theme, s.Language);
}
