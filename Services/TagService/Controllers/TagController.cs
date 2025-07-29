using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TagService.DTOs;
using TagService.Services;

namespace TagService.Controllers;

[ApiController]
[Route("/api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }
    
    [HttpPost("add")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<TagAddResponse>> AddTag([FromBody] TagAddRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TagName))
        {
            return BaseHttpResponse<TagAddResponse>.Fail(400, "标签名不能为空");
        }

        try
        {
            var result = await _tagService.AddTagAsync(request.TagName);
            return BaseHttpResponse<TagAddResponse>.Success(result);
        }
        catch (Exception ex)
        {
            // 可以在这里记录日志
            return BaseHttpResponse<TagAddResponse>.Fail(500, $"服务器错误: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<BaseHttpResponse<List<TagsGetResponse>>> GetTags([FromQuery] string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            return BaseHttpResponse<List<TagsGetResponse>>.Fail(400, "ids 参数不能为空");
        }

        try
        {
            var idList = ids.Split(',')
                .Select(idStr => long.TryParse(idStr, out var id) ? id : (long?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (idList.Count == 0)
                return BaseHttpResponse<List<TagsGetResponse>>.Fail(400, "ids 格式错误");

            var tags = await _tagService.GetTagsAsync(idList);
            return BaseHttpResponse<List<TagsGetResponse>>.Success(tags);
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<List<TagsGetResponse>>.Fail(500, $"服务器错误: {ex.Message}");
        }
    }

    [HttpGet("popular")]
    public async Task<BaseHttpResponse<List<PopularTagsResponse>>> GetPopularTags()
    {
        try
        {
            var result = await _tagService.GetPopularTagsAsync();
            return BaseHttpResponse<List<PopularTagsResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<List<PopularTagsResponse>>.Fail(500, $"服务器错误: {ex.Message}");
        }
    }
}