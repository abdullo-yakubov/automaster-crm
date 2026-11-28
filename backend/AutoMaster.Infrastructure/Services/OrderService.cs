using System.Text;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private static readonly OrderStatus[] KanbanStatuses =
    {
        OrderStatus.Intake, OrderStatus.Diagnosis, OrderStatus.Estimate, OrderStatus.Approved,
        OrderStatus.InProgress, OrderStatus.Waiting, OrderStatus.Ready, OrderStatus.Completed
    };

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<OrderDto> Items, int Total)> GetOrdersAsync(string? status, string? search, int page, int limit)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var st))
            query = query.Where(o => o.Status == st);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(s) ||
                                      o.Customer!.FullName.ToLower().Contains(s) ||
                                      o.Customer.Phone.Contains(s) ||
                                      o.Vehicle!.LicensePlate.ToLower().Contains(s) ||
                                      (o.Mechanic != null && o.Mechanic.FullName.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * limit).Take(limit)
            .Select(o => ToDto(o)).ToListAsync();

        return (items, total);
    }

    public async Task<OrderDetailDto> GetOrderByIdAsync(Guid id)
    {
        var order = await BaseQuery().FirstOrDefaultAsync(o => o.Id == id) ?? throw new KeyNotFoundException("Заказ не найден");
        var timeline = await _db.OrderTimelineEntries
            .Where(t => t.OrderId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Join(_db.Users, t => t.UserId, u => u.Id, (t, u) => new OrderTimelineDto(t.Id, t.Status, t.Note, u.FullName, t.CreatedAt))
            .ToListAsync();

        return new OrderDetailDto(ToDto(order), timeline);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, Guid currentUserId)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Phone == request.CustomerPhone);
        if (customer is null)
        {
            customer = new Customer { FullName = request.CustomerName, Phone = request.CustomerPhone };
            _db.Customers.Add(customer);
        }

        var plate = request.LicensePlate.ToUpper();
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == plate);
        if (vehicle is null)
        {
            vehicle = new Vehicle
            {
                CustomerId = customer.Id,
                Make = request.VehicleMake,
                Model = request.VehicleModel,
                LicensePlate = plate,
                VIN = request.VIN,
                Mileage = request.Mileage,
                Year = DateTime.UtcNow.Year
            };
            _db.Vehicles.Add(vehicle);
        }
        else
        {
            vehicle.Mileage = request.Mileage;
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyMMdd}-{(await _db.Orders.CountAsync() + 1):D4}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            MechanicId = request.MechanicId,
            ServiceDescription = request.ServiceDescription,
            Priority = Enum.Parse<OrderPriority>(request.Priority, true),
            LaborCost = request.LaborCost,
            PartsCost = request.PartsCost,
            TotalCost = request.LaborCost + request.PartsCost,
            Note = request.Note
        };
        _db.Orders.Add(order);
        _db.OrderTimelineEntries.Add(new OrderTimelineEntry
        {
            OrderId = order.Id, Status = order.Status.ToString(), Note = "Заказ создан", UserId = currentUserId
        });

        await _db.SaveChangesAsync();
        var saved = await BaseQuery().FirstAsync(o => o.Id == order.Id);
        return ToDto(saved);
    }

    public async Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderRequest request)
    {
        var order = await _db.Orders.FindAsync(id) ?? throw new KeyNotFoundException("Заказ не найден");
        order.ServiceDescription = request.ServiceDescription;
        order.MechanicId = request.MechanicId;
        order.Priority = Enum.Parse<OrderPriority>(request.Priority, true);
        order.LaborCost = request.LaborCost;
        order.PartsCost = request.PartsCost;
        order.TotalCost = request.LaborCost + request.PartsCost;
        order.Note = request.Note;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var saved = await BaseQuery().FirstAsync(o => o.Id == id);
        return ToDto(saved);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, Guid currentUserId)
    {
        var order = await _db.Orders.Include(o => o.Customer).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Заказ не найден");

        var newStatus = Enum.Parse<OrderStatus>(request.Status, true);
        var wasCompleted = order.CompletedAt is not null;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Completed && !wasCompleted)
        {
            order.CompletedAt = DateTime.UtcNow;
            ApplyLoyaltyAndSpend(order.Customer!, order.TotalCost);
        }

        _db.OrderTimelineEntries.Add(new OrderTimelineEntry
        {
            OrderId = order.Id, Status = newStatus.ToString(), Note = request.Note, UserId = currentUserId
        });

        await _db.SaveChangesAsync();
        var saved = await BaseQuery().FirstAsync(o => o.Id == id);
        return ToDto(saved);
    }

    public async Task DeleteOrderAsync(Guid id)
    {
        var order = await _db.Orders.FindAsync(id) ?? throw new KeyNotFoundException("Заказ не найден");
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();
    }

    public async Task<List<KanbanColumnDto>> GetKanbanAsync()
    {
        var orders = await BaseQuery().Where(o => o.Status != OrderStatus.Completed).ToListAsync();
        return KanbanStatuses.Select(s => new KanbanColumnDto(
            s.ToString(),
            orders.Where(o => o.Status == s).Select(ToDto).ToList()
        )).ToList();
    }

    public async Task<byte[]> ExportCsvAsync()
    {
        var orders = await BaseQuery().OrderByDescending(o => o.CreatedAt).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("OrderNumber,Customer,Phone,Vehicle,LicensePlate,Mechanic,Status,Total,CreatedAt");
        foreach (var o in orders)
        {
            sb.AppendLine($"{o.OrderNumber},{o.Customer!.FullName},{o.Customer.Phone},\"{o.Vehicle!.Make} {o.Vehicle.Model}\",{o.Vehicle.LicensePlate},{o.Mechanic?.FullName},{o.Status},{o.TotalCost},{o.CreatedAt:yyyy-MM-dd}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static void ApplyLoyaltyAndSpend(Customer customer, decimal orderTotal)
    {
        customer.TotalSpent += orderTotal;
        customer.LoyaltyPoints += (int)(orderTotal / 100);
        customer.Status = customer.TotalSpent >= 20000 ? CustomerStatus.Corporate
            : customer.TotalSpent >= 5000 ? CustomerStatus.Vip
            : customer.Status;
    }

    private IQueryable<Order> BaseQuery() =>
        _db.Orders.Include(o => o.Customer).Include(o => o.Vehicle).Include(o => o.Mechanic);

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.OrderNumber, o.CustomerId, o.Customer?.FullName ?? "", o.Customer?.Phone ?? "",
        o.VehicleId, $"{o.Vehicle?.Make} {o.Vehicle?.Model}", o.Vehicle?.LicensePlate ?? "",
        o.MechanicId, o.Mechanic?.FullName, o.ServiceDescription, o.Status.ToString(), o.Priority.ToString(),
        o.LaborCost, o.PartsCost, o.TotalCost, o.Note, o.CreatedAt, o.UpdatedAt, o.CompletedAt);
}
