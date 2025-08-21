using Microsoft.EntityFrameworkCore;
using Models;
using UserAuthService.Data;

namespace UserAuthService.Repositories
{
    public interface IUserAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsByUsernameOrEmailOrPhoneAsync(string username, string email,string phone);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetByIdentifierAsync(string identifier);
    }

    public class UserAuthRepository : IUserAuthRepository
    {
        private readonly AppDbContext _context;

        public UserAuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> ExistsByUsernameOrEmailOrPhoneAsync(string username, string email, string phone)
        {
            var count = await _context.Users
                .Where(u => u.Username == username || u.Email == email || u.Phone == phone)
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
        
        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == identifier ||
                    u.Email == identifier ||
                    u.Phone == identifier);
        }
    }
}