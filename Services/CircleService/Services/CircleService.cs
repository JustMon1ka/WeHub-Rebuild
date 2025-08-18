using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 圈子服务的实现类，负责处理圈子相关的业务逻辑。
/// </summary>
public class CircleService : ICircleService
{
    private readonly ICircleRepository _circleRepository;
    private readonly ICircleMemberRepository _memberRepository;

    public CircleService(ICircleRepository circleRepository, ICircleMemberRepository memberRepository)
    {
        _circleRepository = circleRepository;
        _memberRepository = memberRepository;
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

    public async Task<IEnumerable<CircleDto>> GetAllCirclesAsync(string? name = null, int? userId = null)
    {
        var circles = await _circleRepository.GetAllAsync(name, userId);
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
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
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
            OwnerId = circle.OwnerId,
            CreatedAt = circle.CreatedAt,
            MemberCount = memberCount
        };
    }
} 