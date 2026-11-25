namespace AutoMaster.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string AccessToken, string RefreshToken, UserDto User);

public record AuthResponseDto(string AccessToken, UserDto User);

public record UserDto(Guid Id, string Email, string FullName, string Role, string? Phone, bool IsActive);
