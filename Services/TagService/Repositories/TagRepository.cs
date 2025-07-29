using Microsoft.EntityFrameworkCore;
using TagService.Data;
using TagService.Models;

namespace TagService.Repositories
{
    public interface ITagRepository
    {
        Task<Tag?> GetByNameAsync(string name);
        Task<Tag?> GetByIdAsync(long id);
        Task<List<Tag>> GetByIdsAsync(List<long> ids);
        Task<Tag> AddAsync(Tag tag);
        Task<Tag> UpdateAsync(Tag tag);
        Task<List<Tag>> GetPopularTagsAsync(int count);
    }

    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _db;

        public TagRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _db.Tags.FirstOrDefaultAsync(t => t.TagName == name);
        }

        public async Task<Tag?> GetByIdAsync(long id)
        {
            return await _db.Tags.FindAsync(id);
        }

        public async Task<List<Tag>> GetByIdsAsync(List<long> ids)
        {
            return await _db.Tags.Where(t => ids.Contains(t.TagId)).ToListAsync();
        }

        public async Task<Tag> AddAsync(Tag tag)
        {
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return tag;
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            _db.Tags.Update(tag);
            await _db.SaveChangesAsync();
            return tag;
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