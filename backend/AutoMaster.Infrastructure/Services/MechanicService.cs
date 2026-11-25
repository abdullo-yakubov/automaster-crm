using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class MechanicService : IMechanicService
{
    private readonly AppDbContext _db;
    private const int MaxActiveOrders = 5;

    public MechanicService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MechanicDto>> GetMechanicsAsync()
    {
        var mechanics = await _db.Mechanics.Include(m => m.Orders).ToListAsync();
        return mechanics.Select(ToDto).ToList();
    }

    public async Task<MechanicKpiDto> GetMechanicKpiAsync(Guid id)
    {
        var m = await _db.Mechanics.Include(x => x.Orders)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Мастер не найден");

        var completed = m.Orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var revenue = completed.Sum(o => o.TotalCost);
        var avgTicket = completed.Count > 0 ? revenue / completed.Count : 0;
        var activeOrders = m.Orders.Count(o => o.Status != OrderStatus.Completed);
        var workload = Math.Min(100.0, activeOrders * 100.0 / MaxActiveOrders);

        return new MechanicKpiDto(m.Id, m.FullName, completed.Count, revenue, avgTicket, m.Rating, workload);
    }

    public async Task<MechanicDto> CreateMechanicAsync(CreateMechanicRequest request)
    {
        var mechanic = new Mechanic
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Specialization = Enum.Parse<MechanicSpecialization>(request.Specialization, true),
            Level = Enum.Parse<MechanicLevel>(request.Level, true),
            Salary = request.Salary
        };
        _db.Mechanics.Add(mechanic);
        await _db.SaveChangesAsync();
        return ToDto(mechanic);
    }

    public async Task<MechanicDto> UpdateMechanicAsync(Guid id, UpdateMechanicRequest request)
    {
        var mechanic = await _db.Mechanics.Include(m => m.Orders)
            .FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException("Мастер не найден");
        mechanic.FullName = request.FullName;
        mechanic.Phone = request.Phone;
        mechanic.Specialization = Enum.Parse<MechanicSpecialization>(request.Specialization, true);
        mechanic.Level = Enum.Parse<MechanicLevel>(request.Level, true);
        mechanic.Salary = request.Salary;
        mechanic.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        return ToDto(mechanic);
    }

    public async Task DeleteMechanicAsync(Guid id)
    {
        var mechanic = await _db.Mechanics.FindAsync(id) ?? throw new KeyNotFoundException("Мастер не найден");
        _db.Mechanics.Remove(mechanic);
        await _db.SaveChangesAsync();
    }

    public async Task<List<TopMechanicDto>> GetTopMechanicsAsync(int limit)
    {
        var mechanics = await _db.Mechanics.Include(m => m.Orders).ToListAsync();
        return mechanics
            .Select(m =>
            {
                var completed = m.Orders.Where(o => o.Status == OrderStatus.Completed).ToList();
                return new TopMechanicDto(m.Id, m.FullName, completed.Count, completed.Sum(o => o.TotalCost), m.Rating);
            })
            .OrderByDescending(m => m.Revenue)
            .Take(limit)
            .ToList();
    }

    private static MechanicDto ToDto(Mechanic m)
    {
        var completed = m.Orders.Count(o => o.Status == OrderStatus.Completed);
        var revenue = m.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalCost);
        var activeOrders = m.Orders.Count(o => o.Status != OrderStatus.Completed);
        var workload = Math.Min(100.0, activeOrders * 100.0 / MaxActiveOrders);
        return new MechanicDto(m.Id, m.FullName, m.Phone, m.Specialization.ToString(), m.Level.ToString(),
            m.Salary, m.IsActive, m.Rating, completed, revenue, workload);
    }
}
