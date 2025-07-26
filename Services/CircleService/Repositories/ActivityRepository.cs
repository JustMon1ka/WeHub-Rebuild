using CircleService.Data;
using CircleService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 活动仓储的实现类，负责与数据库进行实际的活动数据交互。
/// </summary>
public class ActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public ActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Activity?> GetByIdAsync(int id)
    {
        return await _context.Activities.FindAsync(id);
    }

    public async Task<IEnumerable<Activity>> GetByCircleIdAsync(int circleId)
    {
        return await _context.Activities
                             .Where(a => a.CircleId == circleId)
                             .ToListAsync();
    }

    public async Task AddAsync(Activity activity)
    {
        await _context.Activities.AddAsync(activity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Activity activity)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }
} 