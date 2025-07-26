using CircleService.Data;
using CircleService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 用户活动参与记录仓储的实现类，负责与数据库进行实际的数据交互。
/// </summary>
public class ActivityParticipantRepository : IActivityParticipantRepository
{
    private readonly AppDbContext _context;

    public ActivityParticipantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityParticipant?> GetByIdAsync(int activityId, int userId)
    {
        return await _context.ActivityParticipants.FindAsync(activityId, userId);
    }

    public async Task<IEnumerable<ActivityParticipant>> GetByUserIdAsync(int userId)
    {
        return await _context.ActivityParticipants
                             .Where(ap => ap.UserId == userId)
                             .ToListAsync();
    }

    public async Task AddAsync(ActivityParticipant participant)
    {
        await _context.ActivityParticipants.AddAsync(participant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ActivityParticipant participant)
    {
        _context.ActivityParticipants.Update(participant);
        await _context.SaveChangesAsync();
    }
} 