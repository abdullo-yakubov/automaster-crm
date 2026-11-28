using AutoMaster.Application.DTOs;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using AutoMaster.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AutoMaster.Tests;

public class OrderCompletionTests
{
    [Fact]
    public async Task Completing_same_order_twice_credits_customer_once()
    {
        await using var db = CreateDatabase();
        var user = new User { Email = "tester@example.invalid", FullName = "Tester", PasswordHash = "test" };
        var customer = new Customer { FullName = "Customer", Phone = "+992900000000" };
        var vehicle = new Vehicle { CustomerId = customer.Id, Customer = customer, Make = "Test", Model = "Car", LicensePlate = "TEST-01" };
        var order = new Order { OrderNumber = "TEST-001", CustomerId = customer.Id, Customer = customer,
            VehicleId = vehicle.Id, Vehicle = vehicle, TotalCost = 250m, Status = OrderStatus.Ready };
        db.AddRange(user, customer, vehicle, order);
        await db.SaveChangesAsync();

        var service = new OrderService(db);
        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Completed", null), user.Id);
        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Completed", null), user.Id);

        Assert.Equal(250m, customer.TotalSpent);
        Assert.Equal(2, customer.LoyaltyPoints);
        Assert.NotNull(order.CompletedAt);
    }

    [Fact]
    public async Task Reopening_then_completing_does_not_credit_again()
    {
        await using var db = CreateDatabase();
        var user = new User { Email = "tester@example.invalid", FullName = "Tester", PasswordHash = "test" };
        var customer = new Customer { FullName = "Customer", Phone = "+992900000000" };
        var vehicle = new Vehicle { CustomerId = customer.Id, Customer = customer, Make = "Test", Model = "Car", LicensePlate = "TEST-01" };
        var order = new Order { OrderNumber = "TEST-002", CustomerId = customer.Id, Customer = customer,
            VehicleId = vehicle.Id, Vehicle = vehicle, TotalCost = 250m, Status = OrderStatus.Ready };
        db.AddRange(user, customer, vehicle, order);
        await db.SaveChangesAsync();

        var service = new OrderService(db);
        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Completed", null), user.Id);
        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Ready", null), user.Id);
        await service.UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest("Completed", null), user.Id);

        Assert.Equal(250m, customer.TotalSpent);
        Assert.Equal(2, customer.LoyaltyPoints);
    }

    private static AppDbContext CreateDatabase() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
