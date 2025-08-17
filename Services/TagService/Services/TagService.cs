using TagService.DTOs;
using Models;
using TagService.Repositories;

namespace TagService.Services
{
    public interface ITagService
    {
        Task<List<TagAddResponse>> AddTagsAsync(List<string> tagNames);
        Task<List<TagsGetResponse>> GetTagsAsync(List<long> ids);
        Task<List<PopularTagsResponse>> GetPopularTagsAsync();
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepo;

        public TagService(ITagRepository tagRepo)
        {
            _tagRepo = tagRepo;
        }

        public async Task<List<TagAddResponse>> AddTagsAsync(List<string> tagNames)
        {
            var distinctNames = tagNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct()
                .ToList();

            if (distinctNames.Count == 0)
                throw new ArgumentException("标签名不能为空");

            // 一次性查询数据库已有的标签
            var existingTags = await _tagRepo.GetByNamesAsync(distinctNames);
            var existingDict = existingTags.ToDictionary(t => t.TagName, t => t);

            var now = DateTime.UtcNow;
            var result = new List<TagAddResponse>();
            var newTags = new List<Tag>();

            foreach (var name in distinctNames)
            {
                if (existingDict.TryGetValue(name, out var tag))
                {
                    // 已存在 -> 只更新计数
                    tag.Count += 1;
                    tag.LastQuote = now;
                }
                else
                {
                    // 不存在 -> 准备新增
                    var newTag = new Tag
                    {
                        TagName = name,
                        Count = 1,
                        LastQuote = now
                    };
                    newTags.Add(newTag);
                }
            }

            // 批量插入新标签（一次 SaveChanges）
            if (newTags.Count > 0)
                await _tagRepo.AddRangeAsync(newTags);

            // 批量更新已有标签（一次 SaveChanges）
            if (existingTags.Count > 0)
                await _tagRepo.UpdateRangeAsync(existingTags);

            // 返回所有标签的 ID
            foreach (var t in existingTags.Concat(newTags))
                result.Add(new TagAddResponse { TagId = t.TagId });

            return result;
        }

        public async Task<List<TagsGetResponse>> GetTagsAsync(List<long> ids)
        {
            var tags = await _tagRepo.GetByIdsAsync(ids);
            return tags.Select(t => new TagsGetResponse
            {
                TagId = t.TagId,
                TagName = t.TagName
            }).ToList();
        }

        public async Task<List<PopularTagsResponse>> GetPopularTagsAsync()
        {
            var tags = await _tagRepo.GetPopularTagsAsync(10);
            return tags.Select(t => new PopularTagsResponse
            {
                TagId = t.TagId,
                TagName = t.TagName,
                Count = t.Count
            }).ToList();
        }
    }
}