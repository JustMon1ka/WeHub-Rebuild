using Microsoft.EntityFrameworkCore;
using UserAuthService.Data;
using UserAuthService.Models;

namespace UserAuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email)
        {
            var count = await _context.Users
                .Where(u => u.Username == username || u.Email == email)
                .CountAsync();
            return count > 0;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}