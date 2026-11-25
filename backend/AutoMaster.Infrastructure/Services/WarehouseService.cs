using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _db;

    public WarehouseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PartDto>> GetPartsAsync(string? filter, string? search)
    {
        var query = _db.Parts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));

        var parts = await query.OrderBy(p => p.Name).ToListAsync();
        var dtos = parts.Select(ToDto);

        if (filter == "low") dtos = dtos.Where(d => d.Status == "Мало");
        else if (filter == "ok") dtos = dtos.Where(d => d.Status == "ОК");

        return dtos.ToList();
    }

    public async Task<PartDto> CreatePartAsync(CreatePartRequest request)
    {
        var part = new Part
        {
            Name = request.Name,
            Sku = request.Sku,
            Category = Enum.Parse<PartCategory>(request.Category, true),
            Quantity = request.Quantity,
            MinQuantity = request.MinQuantity,
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice
        };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
        return ToDto(part);
    }

    public async Task<PartDto> UpdatePartAsync(Guid id, UpdatePartRequest request)
    {
        var part = await _db.Parts.FindAsync(id) ?? throw new KeyNotFoundException("Запчасть не найдена");
        part.Name = request.Name;
        part.Sku = request.Sku;
        part.Category = Enum.Parse<PartCategory>(request.Category, true);
        part.Quantity = request.Quantity;
        part.MinQuantity = request.MinQuantity;
        part.PurchasePrice = request.PurchasePrice;
        part.SalePrice = request.SalePrice;
        part.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(part);
    }

    public async Task DeletePartAsync(Guid id)
    {
        var part = await _db.Parts.FindAsync(id) ?? throw new KeyNotFoundException("Запчасть не найдена");
        _db.Parts.Remove(part);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PartDto>> GetLowStockAsync()
    {
        var parts = await _db.Parts.Where(p => p.Quantity <= p.MinQuantity).ToListAsync();
        return parts.Select(ToDto).ToList();
    }

    public async Task<PartDto> UpdateQuantityAsync(Guid id, UpdatePartQuantityRequest request)
    {
        var part = await _db.Parts.FindAsync(id) ?? throw new KeyNotFoundException("Запчасть не найдена");
        part.Quantity = request.Quantity;
        part.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(part);
    }

    public async Task<WarehouseStatsDto> GetStatsAsync()
    {
        var parts = await _db.Parts.ToListAsync();
        var totalItems = parts.Count;
        var lowStock = parts.Count(p => p.Quantity <= p.MinQuantity);
        var totalValue = parts.Sum(p => p.PurchasePrice * p.Quantity);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthlyTurnover = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt >= monthStart)
            .SumAsync(o => o.PartsCost);

        return new WarehouseStatsDto(totalItems, lowStock, totalValue, monthlyTurnover);
    }

    private static PartDto ToDto(Part p)
    {
        var margin = p.PurchasePrice > 0 ? (double)((p.SalePrice - p.PurchasePrice) / p.PurchasePrice * 100) : 0;
        var status = p.Quantity == 0 ? "Нет" : p.Quantity <= p.MinQuantity ? "Мало" : "ОК";
        return new PartDto(p.Id, p.Name, p.Sku, p.Category.ToString(), p.Quantity, p.MinQuantity,
            p.PurchasePrice, p.SalePrice, Math.Round(margin, 1), status);
    }
}
