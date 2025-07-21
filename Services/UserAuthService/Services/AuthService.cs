using Microsoft.EntityFrameworkCore;
using Models;
using UserAuthService.Data;
using UserAuthService.DTOs;
using UserAuthService.Repositories;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Services;
public class AuthService : IAuthService
{
    private readonly JwtService _jwtService;
    private readonly IUserAuthRepository _userAuthRepo;

    public AuthService(IUserAuthRepository userAuthRepo, JwtService jwtService)
    {
        _userAuthRepo = userAuthRepo;
        _jwtService = jwtService;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        if (await _userAuthRepo.ExistsByUsernameOrEmailOrPhoneAsync(request.Username, request.Email, request.Phone))
            return (false, "Username or Email or Phone already exists.");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = hashedPassword,
            Status = 1
        };

        await _userAuthRepo.AddUserAsync(user);
        await _userAuthRepo.SaveChangesAsync();

        return (true, "Registration successful.");
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await _userAuthRepo.GetByIdentifierAsync(request.Identifier);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return _jwtService.GenerateToken(user);
    }
}
