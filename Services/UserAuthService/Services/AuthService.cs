using Microsoft.EntityFrameworkCore;
using UserAuthService.Data;
using UserAuthService.DTOs;
using UserAuthService.Models;
using UserAuthService.Repositories;

namespace UserAuthService.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly JwtService _jwtService;
        private readonly IUserRepository _userRepo;

        public AuthService(IUserRepository userRepo, JwtService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepo.ExistsByUsernameOrEmailAsync(request.Username, request.Email))
                return (false, "Username or Email already exists.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = hashedPassword,
                Status = 0
            };

            await _userRepo.AddUserAsync(user);
            await _userRepo.SaveChangesAsync();

            return (true, "Registration successful.");
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            return _jwtService.GenerateToken(user);
        }
    }
}