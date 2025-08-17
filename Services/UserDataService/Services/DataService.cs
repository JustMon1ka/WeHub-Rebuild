using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Models;
using UserDataService.DTOs;
using UserDataService.Repositories;
using UserDataService.Models;

namespace UserDataService.Services
{
    public interface IUserDataService
    {
        Task<UserInfoResponse?> GetUserInfoAsync(int userId);
        Task<(bool Success, string Message)> UpdateUserInfoAsync(int userId,UpdateUserInfoRequest request);
        Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserRequest request);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
    }

    public class DataService : IUserDataService
    {
        private readonly IUserDataRepository _repo;

        public DataService(IUserDataRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserInfoResponse?> GetUserInfoAsync(int userId)
        {
            var user = await _repo.GetUserByIdAsync(userId);
            var profile = await _repo.GetProfileByIdAsync(userId);

            if (user == null) return null;

            return new UserInfoResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt,
                Status = user.Status,
                Bio = profile?.Bio,
                Gender = profile?.Gender,
                Birthday = profile?.Birthday ?? default,
                Location = profile?.Location,
                Experience = profile?.Experience ?? 0,
                Level = profile?.Level ?? 0,
                Nickname = profile?.Nickname,
            };
        }

        public async Task<(bool Success, string Message)> UpdateUserInfoAsync(int userId,UpdateUserInfoRequest request)
        {
            var user = await _repo.GetUserByIdAsync(userId);
            var profile = await _repo.GetProfileByIdAsync(userId);

            if (user == null) return (false, "User not found");
            if (profile == null) return (false, "User profile not found");
            if (await _repo.ExistsByUsernameAsync(request.Username, userId))
            {
                return (false, "Username already exists.");
            }

            if (!string.IsNullOrEmpty(request.Username)) user.Username = request.Username;
            if (!string.IsNullOrEmpty(request.Bio)) profile.Bio = request.Bio;
            if (!string.IsNullOrEmpty(request.Gender)) profile.Gender = request.Gender;
            if (!string.IsNullOrEmpty(request.Location)) profile.Location = request.Location;
            if (!string.IsNullOrEmpty(request.Nickname)) profile.Nickname = request.Nickname;
            profile.Birthday = request.Birthday;
            profile.Experience = request.Experience;
            profile.Level = request.Level;

            await _repo.UpdateUserAsync(user);
            await _repo.UpdateUserProfileAsync(profile);
            await _repo.SaveChangesAsync();

            return (true, "User info updated successfully");
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            var user = await _repo.GetUserByIdAsync(userId);
            if (user == null) return (false, "User not found");
            
            if (await _repo.ExistsByUsernameAsync(request.Username, userId))
            {
                return (false, "Username already exists.");
            }
            if (await _repo.ExistsByEmailOrPhoneAsync(request.Email, request.Phone,userId))
            {
                return (false, "Email or phone already exists.");
            }

            user.Username = request.Username;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync();

            return (true, "User updated successfully");
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            var user = await _repo.GetUserByIdAsync(userId);
            if (user == null) return (false, "User not found");

            await _repo.DeleteUserAsync(user);
            await _repo.SaveChangesAsync();

            return (true, "User deleted successfully");
        }
    }
}
