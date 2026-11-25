using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        var users = await _db.Users.OrderBy(u => u.FullName).ToListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<AdminUserDto> CreateUserAsync(CreateAdminUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Пользователь с таким email уже существует");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            FullName = request.FullName,
            Role = Enum.Parse<UserRole>(request.Role, true),
            Phone = request.Phone
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<AdminUserDto> UpdateUserAsync(Guid id, UpdateAdminUserRequest request)
    {
        var user = await _db.Users.FindAsync(id) ?? throw new KeyNotFoundException("Пользователь не найден");
        user.FullName = request.FullName;
        user.Role = Enum.Parse<UserRole>(request.Role, true);
        user.Phone = request.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<AdminUserDto> ToggleBlockAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id) ?? throw new KeyNotFoundException("Пользователь не найден");
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<AdminNetworkStatsDto> GetNetworkStatsAsync()
    {
        var usersCount = await _db.Users.CountAsync();
        var ordersCount = await _db.Orders.CountAsync();
        var customersCount = await _db.Customers.CountAsync();
        var totalRevenue = await _db.Orders.Where(o => o.Status == OrderStatus.Completed).SumAsync(o => o.TotalCost);
        return new AdminNetworkStatsDto(usersCount, ordersCount, customersCount, totalRevenue);
    }

    public async Task ResetDemoDataAsync()
    {
        await DbSeeder.ResetAsync(_db);
    }

    private static AdminUserDto ToDto(User u) => new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.Phone, u.IsActive, u.CreatedAt);
}
