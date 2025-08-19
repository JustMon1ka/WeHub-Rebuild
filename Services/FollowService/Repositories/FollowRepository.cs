using Microsoft.EntityFrameworkCore;
using FollowService.Data;
using FollowService.Models;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == userId);
        }

        public async Task<int> GetFollowerCountAsync(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FolloweeId == userId);
        }

        public async Task<List<Follow>> GetFollowingListAsync(int userId, int page, int pageSize)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId)
                .OrderBy(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Follow>> GetFollowerListAsync(int userId, int page, int pageSize)
        {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId)
                .OrderBy(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}