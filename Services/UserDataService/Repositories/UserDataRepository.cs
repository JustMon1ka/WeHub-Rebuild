using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;
using UserDataService.Data;

namespace UserDataService.Repositories
{
    public interface IUserDataRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task DeleteUserAsync(User user);
    }
    
    public class UserDataRepository : IUserDataRepository
    {
        private readonly AppDbContext _db;

        public UserDataRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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

            await _db.SaveChangesAsync();
        }
    }
}