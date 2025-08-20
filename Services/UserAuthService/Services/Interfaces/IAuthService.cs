namespace UserAuthService.Services.Interfaces;

using UserAuthService.DTOs;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
    Task<string?> RefreshTokenAsync(string username);
    Task<(bool Success, string Message)> SendEmailCodeAsync(string email);
    Task<(bool Success, string Message, string? data)> LoginByEmailCodeAsync(string email, string code);
}