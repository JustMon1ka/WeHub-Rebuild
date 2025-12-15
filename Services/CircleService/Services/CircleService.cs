using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using CircleService.Builders;
using CircleService.Events;

namespace CircleService.Services;

/// <summary>
/// 圈子服务的实现类，负责处理圈子相关的业务逻辑。
/// </summary>
public class CircleService : ICircleService
{
    private readonly ICircleRepository _circleRepository;
    private readonly ICircleMemberRepository _memberRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IFileBrowserClient _fileBrowserClient;
    //Refactoring classes under
    private readonly ICircleCreationFacade _circleCreationFacade;
    private readonly ICircleMemberRepository _circleMemberRepository;
     private readonly ICircleSubject _subject;


    public CircleService(ICircleRepository circleRepository, ICircleMemberRepository memberRepository, IActivityRepository activityRepository, IFileBrowserClient fileBrowserClient, ICircleCreationFacade circleCreationFacade, ICircleMemberRepository circleMemberRepository, ICircleSubject subject)
    {
        _circleRepository = circleRepository;
        _memberRepository = memberRepository;
        _activityRepository = activityRepository;
        _fileBrowserClient = fileBrowserClient;
        _subject = subject;
    }

    public async Task<CircleDto?> GetCircleByIdAsync(int id)
    {
        var circle = await _circleRepository.GetByIdAsync(id);
        if (circle == null)
        {
            return null;
        }
        // 对于单个圈子，直接查询其成员数
        var members = await _memberRepository.GetMembersByCircleIdAsync(id);
        var memberCount = members.Count(m => m.Status == CircleMemberStatus.Approved);

        return MapToCircleDto(circle, memberCount);
    }

    public async Task<IEnumerable<CircleDto>> GetAllCirclesAsync(string? name = null, string? category = null, int? userId = null)
    {
        var circles = await _circleRepository.GetAllAsync(name, category, userId);
        if (!circles.Any())
        {
            return Enumerable.Empty<CircleDto>();
        }

        var circleIds = circles.Select(c => c.CircleId);
        var memberCounts = await _memberRepository.GetMemberCountsByCircleIdsAsync(circleIds);

        return circles.Select(c => MapToCircleDto(c, memberCounts.GetValueOrDefault(c.CircleId, 0)));
    }
    
    //Refactored Function with builder and Facade
    public async Task<CircleDto> CreateCircleAsync(CreateCircleDto createCircleDto, int ownerId)
    {
        // 1. 使用 Facade 完成创建圈子完整流程
        var circle = await _circleCreationFacade.CreateNewCircleAsync(createCircleDto, ownerId);

        // 2. 获取成员数量（用于构建 DTO）
        var memberCount = await _circleMemberRepository.GetMemberCountAsync(circle.Id);
        
        // 通知观察者圈子已创建
        _subject.NotifyCircleCreated(circle);

        // 3. 返回 CircleDto（你项目已有的 DTO 构建函数）
        return MapToCircleDto(circle, memberCount);
    }


    public async Task<bool> UpdateCircleAsync(int id, UpdateCircleDto updateCircleDto)
    {
        var circle = await _circleRepository.GetByIdAsync(id);
        if (circle == null)
        {
            return false;
        }

        circle.Name = updateCircleDto.Name;
        circle.Description = updateCircleDto.Description;
        circle.Categories = updateCircleDto.Categories;
        // 注意：头像和背景图通过专门的图片上传接口更新，这里不更新

        await _circleRepository.UpdateAsync(circle);
        return true;
    }

    public async Task<bool> DeleteCircleAsync(int id, int deleterId)
    {
        var circle = await _circleRepository.GetByIdAsync(id);
        if (circle == null)
        {
            // 圈子不存在
            return false;
        }

        if (circle.OwnerId != deleterId)
        {
            // 如果删除者不是圈主，则无权删除
            return false;
        }

        await _circleRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<string>> GetAllCategoriesAsync()
    {
        var circles = await _circleRepository.GetAllAsync();
        
        // 收集所有非空的分类字符串
        var allCategoriesRaw = circles
            .Where(c => !string.IsNullOrWhiteSpace(c.Categories))
            .Select(c => c.Categories!)
            .ToList();

        // 解析分类标签（假设用逗号分隔）
        var categories = new HashSet<string>();
        foreach (var categoryString in allCategoriesRaw)
        {
            var tags = categoryString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(tag => tag.Trim())
                                   .Where(tag => !string.IsNullOrWhiteSpace(tag));
            
            foreach (var tag in tags)
            {
                categories.Add(tag);
            }
        }

        return categories.OrderBy(c => c).ToList();
    }

    public async Task<ImageUploadResponseDto?> UploadAvatarAsync(int circleId, Stream fileStream, string fileName, string contentType)
    {
        // 检查圈子是否存在
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null)
        {
            return null;
        }

        // 生成唯一的文件名
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"circles/{circleId}/avatar_{DateTime.UtcNow:yyyyMMddHHmmss}_{GenerateRandomString(8)}{fileExtension}";

        // 上传文件到FileBrowser
        var response = await _fileBrowserClient.UploadFileAsync(uniqueFileName, contentType, fileStream);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 构建图片URL 
        var imageUrl = $"http://120.26.118.70:5001/api/preview/big/uploads/{uniqueFileName}?inline=true&key={timestamp}";

        // 更新数据库中的头像URL
        circle.AvatarUrl = imageUrl;
        await _circleRepository.UpdateAsync(circle);

        // 返回带预览功能的URL给前端 - 使用公开访问路径避免401认证，添加inline参数直接返回图片
     
        var previewUrl = $"http://120.26.118.70:5001/api/preview/big/uploads/{uniqueFileName}?inline=true&key={timestamp}";

        return new ImageUploadResponseDto
        {
            ImageUrl = previewUrl, // 返回预览URL
            FileName = fileName,
            FileSize = fileStream.Length,
            ContentType = contentType
        };
    }

    public async Task<ImageUploadResponseDto?> UploadBannerAsync(int circleId, Stream fileStream, string fileName, string contentType)
    {
        // 检查圈子是否存在
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null)
        {
            return null;
        }

        // 生成唯一的文件名
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"circles/{circleId}/banner_{DateTime.UtcNow:yyyyMMddHHmmss}_{GenerateRandomString(8)}{fileExtension}";

        // 上传文件到FileBrowser
        var response = await _fileBrowserClient.UploadFileAsync(uniqueFileName, contentType, fileStream);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
    
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 构建图片URL 
        var imageUrl = $"http://120.26.118.70:5001/api/preview/big/uploads/{uniqueFileName}?inline=true&key={timestamp}";

        // 更新数据库中的背景图URL
        circle.BannerUrl = imageUrl;
        await _circleRepository.UpdateAsync(circle);

        var previewUrl = $"http://120.26.118.70:5001/api/preview/big/uploads/{uniqueFileName}?inline=true&key={timestamp}";

        return new ImageUploadResponseDto
        {
            ImageUrl = previewUrl, // 返回预览URL
            FileName = fileName,
            FileSize = fileStream.Length,
            ContentType = contentType
        };
    }

    public async Task<ImageUploadResponseDto?> UploadActivityImageAsync(int activityId, Stream fileStream, string fileName, string contentType)
    {
        // 检查活动是否存在
        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null)
        {
            return null;
        }

        // 生成唯一的文件名
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"activities/{activityId}/cover_{DateTime.UtcNow:yyyyMMddHHmmss}_{GenerateRandomString(8)}{fileExtension}";

        // 上传文件到FileBrowser
        var response = await _fileBrowserClient.UploadFileAsync(uniqueFileName, contentType, fileStream);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 构建图片URL 
        var imageUrl = $"http://120.26.118.70:5001/api/preview/big/uploads/{uniqueFileName}?inline=true&key={timestamp}";

        // 更新数据库中的活动封面URL
        activity.ActivityUrl = imageUrl;
        await _activityRepository.UpdateAsync(activity);

        var previewUrl = $"http://120.26.118.70:5001/api/preview/big/uploads/{uniqueFileName}?inline=true&key={timestamp}";

        return new ImageUploadResponseDto
        {
            ImageUrl = previewUrl, // 返回预览URL
            FileName = fileName,
            FileSize = fileStream.Length,
            ContentType = contentType
        };
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// 将Circle模型映射到CircleDto。
    /// </summary>
    private CircleDto MapToCircleDto(Circle circle, int memberCount = 0)
    {
        return new CircleDto
        {
            CircleId = circle.CircleId,
            Name = circle.Name,
            Description = circle.Description,
            Categories = circle.Categories,
            OwnerId = circle.OwnerId,
            CreatedAt = circle.CreatedAt,
            MemberCount = memberCount,
            AvatarUrl = circle.AvatarUrl,
            BannerUrl = circle.BannerUrl
        };
    }

    public async Task<PostIdListDto> GetPostIdsByCircleIdAsync(int? circleId = null)
    {
        // 如果提供了circleId，检查圈子是否存在
        if (circleId.HasValue)
        {
            var circle = await _circleRepository.GetByIdAsync(circleId.Value);
            if (circle == null)
            {
                throw new ArgumentException($"圈子ID {circleId.Value} 不存在");
            }
        }

        // 获取帖子ID列表
        var postIds = await _circleRepository.GetPostIdsByCircleIdAsync(circleId);

        return new PostIdListDto
        {
            CircleId = circleId ?? 0, // 如果没有指定圈子，设为0表示所有圈子
            PostIds = postIds,
            TotalCount = postIds.Count()
        };
    }
} 
