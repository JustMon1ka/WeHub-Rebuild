using UserTagService.DTOs;
using UserTagService.Repositories;

namespace UserTagService.Services;

public interface IUserTagService
{
    Task<List<int>> GetUserTagsAsync(int userId);
    Task<(bool Success, string Message)> UpdateUserTagsAsync(int userId, List<int> tags);
    Task<(bool Success, string Message)> AddUserTagAsync(int userId, int tagId);
    Task<(bool Success, string Message)> DeleteUserTagAsync(int userId, int tagId);

}

public class TagService : IUserTagService
{
    private readonly IUserTagRepository _repo;

    public TagService(IUserTagRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<int>> GetUserTagsAsync(int userId)
    {
        return await _repo.GetTagsByUserIdAsync(userId);
    }

    public async Task<(bool Success, string Message)> UpdateUserTagsAsync(int userId, List<int> tags)
    {
        await _repo.ReplaceUserTagsAsync(userId, tags);
        await _repo.SaveChangesAsync();
        return (true, "Tags updated successfully");
    }
    
    public async Task<(bool Success, string Message)> AddUserTagAsync(int userId, int tagId)
    {
        await _repo.AddUserTagAsync(userId, tagId);
        await _repo.SaveChangesAsync();
        return (true, "Tag added successfully");
    }

    public async Task<(bool Success, string Message)> DeleteUserTagAsync(int userId, int tagId)
    {
        await _repo.DeleteUserTagAsync(userId, tagId);
        await _repo.SaveChangesAsync();
        return (true, "Tag deleted successfully");
    }

}