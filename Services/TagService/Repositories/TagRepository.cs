using Microsoft.EntityFrameworkCore;
using TagService.Data;
using Models;

namespace TagService.Repositories
{
    public interface ITagRepository
    {
        Task<List<Tag>> GetByNamesAsync(List<string> tagNames);
        Task AddRangeAsync(List<Tag> tags);
        Task<Tag?> GetByIdAsync(long id);
        Task<List<Tag>> GetByIdsAsync(List<long> ids);
        Task UpdateRangeAsync(List<Tag> tags);
        Task<List<Tag>> GetPopularTagsAsync(int count);
    }

    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _db;

        public TagRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Tag>> GetByNamesAsync(List<string> tagNames)
        {
            return await _db.Tags
                .Where(t => tagNames.Contains(t.TagName))
                .ToListAsync();
        }

        public async Task<Tag?> GetByIdAsync(long id)
        {
            return await _db.Tags.FindAsync(id);
        }

        public async Task<List<Tag>> GetByIdsAsync(List<long> ids)
        {
            return await _db.Tags.Where(t => ids.Contains(t.TagId)).ToListAsync();
        }

        public async Task AddRangeAsync(List<Tag> tags)
        {
            _db.Tags.AddRange(tags);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(List<Tag> tags)
        {
            _db.Tags.UpdateRange(tags);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 获取热门标签，按 Count 权重 + 时间衰减排序
        /// </summary>
        public async Task<List<Tag>> GetPopularTagsAsync(int count)
        {
            var now = DateTime.UtcNow;
            return await _db.Tags
                .OrderByDescending(t =>
                    t.Count * 0.7 +
                    (t.LastQuote.HasValue ? (1.0 / (1 + (now - t.LastQuote.Value).TotalDays)) * 50 : 0)
                )
                .Take(count)
                .ToListAsync();
        }
    }
}