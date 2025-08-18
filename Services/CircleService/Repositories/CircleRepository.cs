using CircleService.Data;
using CircleService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleService.Repositories;

/// <summary>
/// 圈子仓储的实现类，负责与数据库进行实际的圈子数据交互。
/// </summary>
public class CircleRepository : ICircleRepository
{
    private readonly AppDbContext _context;

    public CircleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Circle?> GetByIdAsync(int id)
    {
        return await _context.Circles.FindAsync(id);
    }

    public async Task<IEnumerable<Circle>> GetAllAsync(string? name = null, int? userId = null)
    {
        var query = _context.Circles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (userId.HasValue)
        {
            query = query.Where(c => _context.CircleMembers.Any(cm => cm.CircleId == c.CircleId && cm.UserId == userId.Value));
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(Circle circle)
    {
        await _context.Circles.AddAsync(circle);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Circle circle)
    {
        _context.Circles.Update(circle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var circle = await _context.Circles.FindAsync(id);
        if (circle != null)
        {
            _context.Circles.Remove(circle);
            await _context.SaveChangesAsync();
        }
    }
} 