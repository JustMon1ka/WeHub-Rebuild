using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;
using UserDataService.Data;

namespace UserDataService.Repositories
{
    public interface IUserDataRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<UserProfile?> GetProfileByIdAsync(int userId);
        Task UpdateUserAsync(User user);
        Task UpdateUserProfileAsync(UserProfile profile);
        Task DeleteUserAsync(User user);
        Task SaveChangesAsync();
        Task<bool> ExistsByUsernameAsync(string username, int excludeUserId);
        Task<bool> ExistsByEmailOrPhoneAsync(string email, string phone, int excludeUserId);
    }
    
    public class UserDataRepository : IUserDataRepository
    {
        private readonly AppDbContext _db;

        public UserDataRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
        
        public async Task<UserProfile?> GetProfileByIdAsync(int userId)
        {
            return await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }
        
        public async Task UpdateUserAsync(User user)
        {
            _db.Users.Update(user);
        }
        
        public async Task UpdateUserProfileAsync(UserProfile profile)
        {
            _db.UserProfiles.Update(profile);
        }

        public async Task DeleteUserAsync(User user)
        {
            // 先删除用户资料
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            if (profile != null)
            {
                _db.UserProfiles.Remove(profile);
            }

            // 删除用户
            _db.Users.Remove(user);
        }
        
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
        
        public async Task<bool> ExistsByUsernameAsync(string username, int excludeUserId)
        {
            var count = await _db.Users
                .Where(u => u.UserId != excludeUserId && u.Username == username)
                .CountAsync();

            return count > 0;
        }
        
        public async Task<bool> ExistsByEmailOrPhoneAsync( string email, string phone, int excludeUserId)
        {
            var count = await _db.Users
                .Where(u => u.UserId != excludeUserId && ( u.Email == email || u.Phone == phone))
                .CountAsync();

            return count > 0;
        }
    }
}