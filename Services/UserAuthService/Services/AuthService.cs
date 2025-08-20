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
    private readonly IEmailService _emailService;

    public AuthService(IUserAuthRepository userAuthRepo, JwtService jwtService,IEmailService emailService)
    {
        _userAuthRepo = userAuthRepo;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        if (await _userAuthRepo.ExistsByUsernameOrEmailOrPhoneAsync(request.Username, request.Email, request.Phone))
            return (false, "Username or Email or Phone already exists.");

        if (!_emailService.ValidateCode(request.Email, request.Code))
        {
            return (false, "Invalid code.");
        }
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

        return _jwtService.GenerateToken(user, true);
    }

    public async Task<string?> RefreshTokenAsync(string username)
    {
        var user = await _userAuthRepo.GetByUsernameAsync(username);
        if (user == null) return null;
        return _jwtService.GenerateToken(user, false);
    }
    
    public async Task<(bool Success, string Message)> SendEmailCodeAsync(string email)
    {
        var code = new Random().Next(100000, 999999).ToString();
        _emailService.SaveCode(email, code);
        await _emailService.SendEmailAsync(email, "验证码登录", $"你的验证码是：{code}");

        return (true, "The verification code has been sent.");
    }
    
    public async Task<(bool Success, string Message, string? data)> LoginByEmailCodeAsync(string email, string code)
    {
        if (!_emailService.ValidateCode(email, code)) return (false, "Invalid code.", null);
        var user = await _userAuthRepo.GetByIdentifierAsync(email);
        if (user == null)
            return (false, "Email not registered", null);
        return (true, "Login Succuss", _jwtService.GenerateToken(user, true));
    }
}
