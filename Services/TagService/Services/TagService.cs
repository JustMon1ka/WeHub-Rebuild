using TagService.DTOs;
using TagService.Models;
using TagService.Repositories;

namespace TagService.Services
{
    public interface ITagService
    {
        Task<TagAddResponse> AddTagAsync(string tagName);
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

        public async Task<TagAddResponse> AddTagAsync(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentException("标签名不能为空");

            var tag = await _tagRepo.GetByNameAsync(tagName);

            if (tag == null)
            {
                tag = new Tag
                {
                    TagName = tagName,
                    Count = 1,
                    LastQuote = DateTime.UtcNow
                };
                tag = await _tagRepo.AddAsync(tag);
            }
            else
            {
                tag.Count += 1;
                tag.LastQuote = DateTime.UtcNow;
                await _tagRepo.UpdateAsync(tag);
            }

            return new TagAddResponse { TagId = tag.TagId };
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