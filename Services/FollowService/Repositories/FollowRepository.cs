using Microsoft.EntityFrameworkCore;
using FollowService.Data;
using FollowService.Models;
using System.Threading.Tasks;

namespace FollowService.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly FollowDbContext _context;

        public FollowRepository(FollowDbContext context)
        {
            _context = context;
        }

        public async Task<Follow> CreateFollowAsync(Follow follow)
        {
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task<Follow> GetFollowAsync(int followerId, int followeeId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }

        public async Task<bool> DeleteFollowAsync(int followerId, int followeeId)
        {
            var follow = await GetFollowAsync(followerId, followeeId);
            if (follow == null)
            {
                return false;
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}