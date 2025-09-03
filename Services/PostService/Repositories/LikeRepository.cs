using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;

namespace PostService.Repositories
{
    public class LikeRepository
    {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task ToggleLikeAsync(int userId, Like like)
        {
            var existing = await _context.Set<Like>()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TargetId == like.TargetId);

            if (existing != null)
            {
                _context.Set<Like>().Remove(existing);
            }
            else
            {
                like.UserId = userId;
                _context.Set<Like>().Add(like);
            }
            await _context.SaveChangesAsync();
        }
    }
}
