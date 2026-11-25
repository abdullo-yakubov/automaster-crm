using System.Security.Cryptography;
using System.Text;
using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);

    // Хэш-заглушка: BCrypt проверяется даже для несуществующего email,
    // чтобы по времени ответа нельзя было узнать, зарегистрирован ли адрес.
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing", 12);

    private readonly AppDbContext _db;
    private readonly JwtTokenGenerator _jwt;

    public AuthService(AppDbContext db, JwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);

        if (user is null)
        {
            BCrypt.Net.BCrypt.Verify(password ?? string.Empty, DummyHash);
            throw new UnauthorizedAccessException("Неверный email или пароль");
        }

        if (user.LockoutEndsAt is not null && user.LockoutEndsAt > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Слишком много неудачных попыток. Попробуйте позже.");

        if (!BCrypt.Net.BCrypt.Verify(password ?? string.Empty, user.PasswordHash) || !user.IsActive)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEndsAt = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
            }
            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Неверный email или пароль");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndsAt = null;

        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();
        user.RefreshToken = HashToken(refresh);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.Add(RefreshLifetime);
        await _db.SaveChangesAsync();

        return new LoginResponse(access, refresh, ToDto(user));
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == hash);
        if (user is null || !user.IsActive || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token недействителен");

        // Ротация: старый токен сразу перестаёт работать.
        var access = _jwt.GenerateAccessToken(user);
        var newRefresh = _jwt.GenerateRefreshToken();
        user.RefreshToken = HashToken(newRefresh);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.Add(RefreshLifetime);
        await _db.SaveChangesAsync();

        return new LoginResponse(access, newRefresh, ToDto(user));
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await _db.SaveChangesAsync();
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Пользователь не найден");
        return ToDto(user);
    }

    /// <summary>В БД хранится только SHA-256 от refresh-токена.</summary>
    public static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static UserDto ToDto(Domain.Entities.User u) =>
        new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.Phone, u.IsActive);
}
