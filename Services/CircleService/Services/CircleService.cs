using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CircleService.Services;

/// <summary>
/// 圈子服务的实现类，负责处理圈子相关的业务逻辑。
/// </summary>
public class CircleService : ICircleService
{
    private readonly ICircleRepository _circleRepository;
    private readonly ICircleMemberRepository _memberRepository;

    private readonly IFileBrowserClient _fileBrowserClient;

    public CircleService(ICircleRepository circleRepository, ICircleMemberRepository memberRepository, IFileBrowserClient fileBrowserClient)
    {
        _circleRepository = circleRepository;
        _memberRepository = memberRepository;
        _fileBrowserClient = fileBrowserClient;
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

    public async Task<CircleDto> CreateCircleAsync(CreateCircleDto createCircleDto, int ownerId)
    {
        var circle = new Circle
        {
            Name = createCircleDto.Name,
            Description = createCircleDto.Description,
            Categories = createCircleDto.Categories,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            AvatarUrl = null,  // 新创建的圈子默认没有头像
            BannerUrl = null   // 新创建的圈子默认没有背景图
        };

        await _circleRepository.AddAsync(circle);
        // 新创建的圈子成员数为1（即圈主自己）
        return MapToCircleDto(circle, 1);
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

        // 构建图片URL
        var imageUrl = $"http://120.26.118.70:5001/api/resources/{uniqueFileName}";

        // 更新数据库中的头像URL
        circle.AvatarUrl = imageUrl;
        await _circleRepository.UpdateAsync(circle);

        return new ImageUploadResponseDto
        {
            ImageUrl = imageUrl,
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

        // 构建图片URL
        var imageUrl = $"http://120.26.118.70:5001/api/resources/{uniqueFileName}";

        // 更新数据库中的背景图URL
        circle.BannerUrl = imageUrl;
        await _circleRepository.UpdateAsync(circle);

        return new ImageUploadResponseDto
        {
            ImageUrl = imageUrl,
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
} 