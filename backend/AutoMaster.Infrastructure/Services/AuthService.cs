using AutoMaster.Application.DTOs;
using AutoMaster.Application.Interfaces;
using AutoMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtTokenGenerator _jwt;

    public AuthService(AppDbContext db, JwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный email или пароль");

        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();
        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        return new LoginResponse(access, refresh, ToDto(user));
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token недействителен");

        var access = _jwt.GenerateAccessToken(user);
        var newRefresh = _jwt.GenerateRefreshToken();
        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
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

    private static UserDto ToDto(Domain.Entities.User u) =>
        new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.Phone, u.IsActive);
}
