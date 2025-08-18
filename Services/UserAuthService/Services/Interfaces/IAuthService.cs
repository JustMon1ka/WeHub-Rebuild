namespace UserAuthService.Services.Interfaces;

using UserAuthService.DTOs;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message)> SendEmailCodeAsync(string email);
    Task<string?> LoginByEmailCodeAsync(string email, string code);
}