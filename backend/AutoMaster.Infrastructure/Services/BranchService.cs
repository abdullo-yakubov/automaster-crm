using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly AppDbContext _db;
    private static readonly Random Rng = new();

    public BranchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BranchDto>> GetBranchesAsync()
    {
        var branches = await _db.Branches.ToListAsync();
        var orders = await _db.Orders.ToListAsync();
        return branches.Select(b => ToDto(b, orders)).ToList();
    }

    public async Task<BranchDto> CreateBranchAsync(CreateBranchRequest request)
    {
        var branch = new Branch
        {
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            Type = Enum.Parse<BranchType>(request.Type, true)
        };
        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();
        return ToDto(branch, new List<Order>());
    }

    public async Task<BranchDto> UpdateBranchAsync(Guid id, UpdateBranchRequest request)
    {
        var branch = await _db.Branches.FindAsync(id) ?? throw new KeyNotFoundException("Филиал не найден");
        branch.Name = request.Name;
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.Type = Enum.Parse<BranchType>(request.Type, true);
        branch.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        var orders = await _db.Orders.ToListAsync();
        return ToDto(branch, orders);
    }

    public async Task DeleteBranchAsync(Guid id)
    {
        var branch = await _db.Branches.FindAsync(id) ?? throw new KeyNotFoundException("Филиал не найден");
        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync();
    }

    public async Task<NetworkKpiDto> GetNetworkKpiAsync()
    {
        var branches = await GetBranchesAsync();
        var totalRevenue = branches.Sum(b => b.Revenue);
        var totalOrders = branches.Sum(b => b.OrdersCount);
        var avgNps = branches.Count > 0 ? branches.Average(b => b.Nps) : 0;
        return new NetworkKpiDto(branches.Count, totalRevenue, totalOrders, avgNps, branches);
    }

    private static BranchDto ToDto(Branch b, List<Order> allOrders)
    {
        var branchOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var revenue = branchOrders.Sum(o => o.TotalCost);
        var nps = 70 + Rng.Next(0, 25);
        return new BranchDto(b.Id, b.Name, b.Address, b.Phone, b.Type.ToString(), b.IsActive,
            branchOrders.Count, revenue, nps);
    }
}
