using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;

namespace PostService.Repositories
{
    public class FavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task ToggleFavoriteAsync(int userId, int postId)
        {
            var existing = await _context.Set<Favorite>()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PostId == postId);

            if (existing != null)
            {
                _context.Set<Favorite>().Remove(existing);
            }
            else
            {
                _context.Set<Favorite>().Add(new Favorite { UserId = userId, PostId = postId });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<Favorite>> GetFavoritesAsync(int userId)
        {
            return await _context.Set<Favorite>()
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }
    }
}
