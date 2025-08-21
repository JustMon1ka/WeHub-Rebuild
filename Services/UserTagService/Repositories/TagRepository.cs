using Microsoft.EntityFrameworkCore;
using UserTagService.Data;
using UserTagService.Models;

namespace UserTagService.Repositories;

public interface IUserTagRepository
{
    Task<List<int>> GetTagsByUserIdAsync(int userId);
    Task ReplaceUserTagsAsync(int userId, List<int> tagIds);
    Task SaveChangesAsync();
    Task AddUserTagAsync(int userId, int tagId);
    Task DeleteUserTagAsync(int userId, int tagId);

}

public class UserTagRepository : IUserTagRepository
{
    private readonly AppDbContext _db;

    public UserTagRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<int>> GetTagsByUserIdAsync(int userId)
    {
        return await _db.UserTags
            .Where(t => t.UserId == userId)
            .Select(t => t.TagId)
            .ToListAsync();
    }

    public async Task ReplaceUserTagsAsync(int userId, List<int> tagIds)
    {
        var existing = await _db.UserTags.Where(t => t.UserId == userId).ToListAsync();
        _db.UserTags.RemoveRange(existing);

        var newTags = tagIds.Select(tagId => new UserTag
        {
            UserId = userId,
            TagId = tagId
        });

        await _db.UserTags.AddRangeAsync(newTags);
    }
    
    public async Task AddUserTagAsync(int userId, int tagId)
    {
        var exists = await _db.UserTags.AnyAsync(t => t.UserId == userId && t.TagId == tagId);
        if (!exists)
        {
            await _db.UserTags.AddAsync(new UserTag
            {
                UserId = userId,
                TagId = tagId
            });
        }
    }

    public async Task DeleteUserTagAsync(int userId, int tagId)
    {
        var tag = await _db.UserTags.FirstOrDefaultAsync(t => t.UserId == userId && t.TagId == tagId);
        if (tag != null)
        {
            _db.UserTags.Remove(tag);
        }
    }


    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}