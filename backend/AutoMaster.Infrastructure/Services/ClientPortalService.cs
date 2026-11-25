using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class ClientPortalService : IClientPortalService
{
    private const int ServiceIntervalKm = 10000;
    private readonly AppDbContext _db;

    public ClientPortalService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClientPortalDto> GetForUserAsync(Guid userId)
    {
        var customerId = await _db.Customers.Where(c => c.UserId == userId).Select(c => (Guid?)c.Id).FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Учётная запись не привязана к карточке клиента");
        return await GetForCustomerAsync(customerId);
    }

    public async Task<ClientPortalDto> GetForCustomerAsync(Guid customerId)
    {
        var c = await _db.Customers
            .AsNoTracking()
            .Include(x => x.Vehicles)
            .Include(x => x.Orders).ThenInclude(o => o.Vehicle)
            .Include(x => x.Orders).ThenInclude(o => o.Mechanic)
            .FirstOrDefaultAsync(x => x.Id == customerId)
            ?? throw new KeyNotFoundException("Клиент не найден");

        var history = c.Orders.OrderByDescending(o => o.CreatedAt).Select(ToDto).ToList();
        var active = c.Orders.Where(o => o.Status != OrderStatus.Completed).OrderByDescending(o => o.CreatedAt).Select(ToDto).FirstOrDefault();
        var vehicles = c.Vehicles.Select(v => new PortalVehicleDto(v.Id, $"{v.Make} {v.Model}", v.LicensePlate, v.Year, v.Mileage, v.VIN)).ToList();

        var notifications = new List<PortalNotificationDto>();
        if (active is not null)
            notifications.Add(new PortalNotificationDto(
                active.Status == nameof(OrderStatus.Ready) ? $"🚗 Ваш автомобиль готов! Заказ {active.OrderNumber}" : $"🔧 Заказ {active.OrderNumber} в работе",
                active.Status == nameof(OrderStatus.Ready) ? "success" : "info"));

        foreach (var v in c.Vehicles)
        {
            var toNext = ServiceIntervalKm - v.Mileage % ServiceIntervalKm;
            if (toNext <= 2000)
                notifications.Add(new PortalNotificationDto($"📅 {v.Make} {v.Model}: плановое ТО через ~{toNext:N0} км", "warning"));
        }

        var lastVisit = c.Orders.Select(o => (DateTime?)o.CreatedAt).Max();
        if (lastVisit is not null && (DateTime.UtcNow - lastVisit.Value).TotalDays > 180)
            notifications.Add(new PortalNotificationDto("🕒 С последнего визита прошло больше 6 месяцев — рекомендуем диагностику", "warning"));

        if (c.LoyaltyPoints > 0)
            notifications.Add(new PortalNotificationDto($"⭐ У вас {c.LoyaltyPoints} баллов — их можно списать на скидку", "success"));

        return new ClientPortalDto(c.Id, c.FullName, c.Phone, c.Status.ToString(), c.LoyaltyPoints, c.Orders.Count,
            c.TotalSpent, active, vehicles, history, notifications);
    }

    private static PortalOrderDto ToDto(Order o) =>
        new(o.Id, o.OrderNumber, $"{o.Vehicle?.Make} {o.Vehicle?.Model}".Trim(), o.ServiceDescription, o.Status.ToString(),
            o.Mechanic?.FullName, o.TotalCost, o.CreatedAt, o.CompletedAt);
}
