using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _db;

    public VehicleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<VehicleDto> Items, int Total)> GetVehiclesAsync(string? search, int page, int limit)
    {
        var query = _db.Vehicles.Include(v => v.Customer).Include(v => v.Orders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(v => v.Make.ToLower().Contains(s) || v.Model.ToLower().Contains(s) ||
                                      (v.VIN != null && v.VIN.ToLower().Contains(s)) ||
                                      v.LicensePlate.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .Select(v => ToDto(v)).ToListAsync();

        return (items, total);
    }

    public async Task<VehicleDto> GetVehicleByIdAsync(Guid id)
    {
        var v = await _db.Vehicles.Include(x => x.Customer).Include(x => x.Orders)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Автомобиль не найден");
        return ToDto(v);
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleRequest request)
    {
        if (await _db.Vehicles.AnyAsync(v => v.LicensePlate == request.LicensePlate.ToUpper()))
            throw new InvalidOperationException("Автомобиль с таким гос. номером уже существует");

        Guid customerId;
        if (request.CustomerId.HasValue)
        {
            customerId = request.CustomerId.Value;
        }
        else
        {
            var customer = new Customer { FullName = request.OwnerName ?? "Без имени", Phone = "+992000000000" };
            _db.Customers.Add(customer);
            customerId = customer.Id;
        }

        var vehicle = new Vehicle
        {
            CustomerId = customerId,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            LicensePlate = request.LicensePlate.ToUpper(),
            VIN = request.VIN,
            Mileage = request.Mileage
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        await _db.Entry(vehicle).Reference(v => v.Customer).LoadAsync();
        return ToDto(vehicle);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request)
    {
        var vehicle = await _db.Vehicles.Include(v => v.Customer).Include(v => v.Orders)
            .FirstOrDefaultAsync(v => v.Id == id) ?? throw new KeyNotFoundException("Автомобиль не найден");
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.LicensePlate = request.LicensePlate.ToUpper();
        vehicle.VIN = request.VIN;
        vehicle.Mileage = request.Mileage;
        await _db.SaveChangesAsync();
        return ToDto(vehicle);
    }

    public async Task DeleteVehicleAsync(Guid id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id) ?? throw new KeyNotFoundException("Автомобиль не найден");
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
    }

    private static VehicleDto ToDto(Vehicle v) => new(
        v.Id, v.CustomerId, v.Customer?.FullName ?? "", v.Make, v.Model, v.Year, v.LicensePlate, v.VIN,
        v.Mileage, v.Orders.Count, v.CreatedAt);
}
