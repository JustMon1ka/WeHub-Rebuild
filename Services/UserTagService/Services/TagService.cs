using UserTagService.DTOs;
using UserTagService.Repositories;

namespace UserTagService.Services;

public interface IUserTagService
{
    Task<List<int>> GetUserTagsAsync(int userId);
    Task<(bool Success, string Message)> UpdateUserTagsAsync(int userId, List<int> tags);
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
}