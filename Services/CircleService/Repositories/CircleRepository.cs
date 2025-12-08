using CircleService.Data;
using CircleService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

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

    public async Task<IEnumerable<Circle>> GetAllAsync(string? name = null, string? category = null, int? userId = null)
    {
        var query = _context.Circles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Categories != null && c.Categories.Contains(category));
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

    public async Task<IEnumerable<int>> GetPostIdsByCircleIdAsync(int? circleId = null)
    {
        try
        {
            Console.WriteLine($"DEBUG: 开始查询帖子，circleId: {circleId}");
            Console.WriteLine($"DEBUG: 数据库上下文类型: {_context.GetType().Name}");
            
            // 检查Posts DbSet是否存在
            var postsDbSet = _context.Posts;
            Console.WriteLine($"DEBUG: Posts DbSet类型: {postsDbSet.GetType().Name}");
            
            // 从POST表中查询帖子ID列表
            var query = _context.Posts.AsQueryable();
            Console.WriteLine($"DEBUG: 查询构建完成");

            // 如果提供了circleId，则按圈子筛选
            if (circleId.HasValue)
            {
                query = query.Where(p => p.CircleId == circleId.Value);
                Console.WriteLine($"DEBUG: 添加圈子筛选条件: {circleId.Value}");
            }

            // 只返回未删除且未隐藏的帖子（Oracle中0=false, 1=true）
            var postIds = await query
                .Where(p => (p.IsDeleted == null || p.IsDeleted == 0) && (p.IsHidden == null || p.IsHidden == 0))
                .Select(p => p.PostId)
                .ToListAsync();

            Console.WriteLine($"DEBUG: 查询完成，返回 {postIds.Count} 个帖子ID");
            return postIds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: 查询帖子时发生异常: {ex.Message}");
            Console.WriteLine($"DEBUG: 异常类型: {ex.GetType().Name}");
            Console.WriteLine($"DEBUG: 堆栈跟踪: {ex.StackTrace}");
            throw;
        }
    }

    public Task<bool> ExistsAsync(long circleId)
    {
        throw new NotImplementedException();
    }
} 