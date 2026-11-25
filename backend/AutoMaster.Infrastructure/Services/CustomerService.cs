using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<CustomerDto> Items, int Total)> GetCustomersAsync(string? search, string? status, int page, int limit)
    {
        var query = _db.Customers.Include(c => c.Vehicles).Include(c => c.Orders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(s) || c.Phone.Contains(s) ||
                                      c.Vehicles.Any(v => v.LicensePlate.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CustomerStatus>(status, true, out var st))
            query = query.Where(c => c.Status == st);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .Select(c => ToDto(c)).ToListAsync();

        return (items, total);
    }

    public async Task<CustomerDetailDto> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _db.Customers
            .Include(c => c.Vehicles)
            .Include(c => c.Orders).ThenInclude(o => o.Vehicle)
            .Include(c => c.Orders).ThenInclude(o => o.Mechanic)
            .FirstOrDefaultAsync(c => c.Id == id) ?? throw new KeyNotFoundException("Клиент не найден");

        var orders = customer.Orders.Select(o => new OrderDto(
            o.Id, o.OrderNumber, o.CustomerId, customer.FullName, customer.Phone,
            o.VehicleId, $"{o.Vehicle!.Make} {o.Vehicle.Model}", o.Vehicle.LicensePlate,
            o.MechanicId, o.Mechanic?.FullName, o.ServiceDescription, o.Status.ToString(), o.Priority.ToString(),
            o.LaborCost, o.PartsCost, o.TotalCost, o.Note, o.CreatedAt, o.UpdatedAt, o.CompletedAt)).ToList();

        var vehicles = customer.Vehicles.Select(v => new VehicleDto(
            v.Id, v.CustomerId, customer.FullName, v.Make, v.Model, v.Year, v.LicensePlate, v.VIN, v.Mileage,
            customer.Orders.Count(o => o.VehicleId == v.Id), v.CreatedAt)).ToList();

        return new CustomerDetailDto(ToDto(customer), orders, vehicles);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        if (await _db.Customers.AnyAsync(c => c.Phone == request.Phone))
            throw new InvalidOperationException("Клиент с таким телефоном уже существует");

        var status = Enum.TryParse<CustomerStatus>(request.Status, true, out var st) ? st : CustomerStatus.Regular;

        var customer = new Customer
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            Birthday = request.Birthday,
            Status = status,
            Note = request.Note
        };
        _db.Customers.Add(customer);

        if (!string.IsNullOrWhiteSpace(request.VehicleMake) && !string.IsNullOrWhiteSpace(request.LicensePlate))
        {
            _db.Vehicles.Add(new Vehicle
            {
                CustomerId = customer.Id,
                Make = request.VehicleMake,
                Model = request.VehicleModel ?? "",
                LicensePlate = request.LicensePlate.ToUpper(),
                Year = DateTime.UtcNow.Year
            });
        }

        await _db.SaveChangesAsync();
        return ToDto(customer);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _db.Customers.FindAsync(id) ?? throw new KeyNotFoundException("Клиент не найден");
        customer.FullName = request.FullName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Birthday = request.Birthday;
        customer.Status = Enum.Parse<CustomerStatus>(request.Status, true);
        customer.Note = request.Note;
        customer.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(customer);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var customer = await _db.Customers.FindAsync(id) ?? throw new KeyNotFoundException("Клиент не найден");
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
    }

    private static CustomerDto ToDto(Customer c) => new(
        c.Id, c.FullName, c.Phone, c.Email, c.Birthday, c.Status.ToString(), c.LoyaltyPoints, c.TotalSpent,
        c.Note, c.CreatedAt, c.Orders.Count > 0 ? c.Orders.Max(o => o.CreatedAt) : null, c.Vehicles.Count);
}
