using CircleService.Data;
using CircleService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 圈子成员仓储的实现类，负责与数据库进行实际的圈子成员数据交互。
/// </summary>
public class CircleMemberRepository : ICircleMemberRepository
{
    private readonly AppDbContext _context;

    public CircleMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CircleMember?> GetByIdAsync(int circleId, int userId)
    {
        // 对于复合主键，需要使用 FindAsync 并按顺序传入键值
        return await _context.CircleMembers.FindAsync(circleId, userId);
    }

    public async Task<IEnumerable<CircleMember>> GetMembersByCircleIdAsync(int circleId)
    {
        return await _context.CircleMembers
                             .Where(cm => cm.CircleId == circleId)
                             .ToListAsync();
    }

    public async Task AddAsync(CircleMember member)
    {
        await _context.CircleMembers.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CircleMember member)
    {
        _context.CircleMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(CircleMember member)
    {
        _context.CircleMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> GetMemberCountsByCircleIdsAsync(IEnumerable<int> circleIds)
    {
        if (circleIds == null || !circleIds.Any())
        {
            return new Dictionary<int, int>();
        }

        return await _context.CircleMembers
                             .Where(cm => circleIds.Contains(cm.CircleId) && cm.Status == CircleMemberStatus.Approved)
                             .GroupBy(cm => cm.CircleId)
                             .Select(g => new { CircleId = g.Key, MemberCount = g.Count() })
                             .ToDictionaryAsync(x => x.CircleId, x => x.MemberCount);
    }
} 