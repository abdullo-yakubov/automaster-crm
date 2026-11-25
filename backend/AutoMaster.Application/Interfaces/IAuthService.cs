using AutoMaster.Application.DTOs;

namespace AutoMaster.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task<LoginResponse> RefreshAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
}
