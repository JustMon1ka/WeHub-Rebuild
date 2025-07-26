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

    public CircleService(ICircleRepository circleRepository)
    {
        _circleRepository = circleRepository;
    }

    public async Task<CircleDto?> GetCircleByIdAsync(int id)
    {
        var circle = await _circleRepository.GetByIdAsync(id);
        if (circle == null)
        {
            return null;
        }
        return MapToCircleDto(circle);
    }

    public async Task<IEnumerable<CircleDto>> GetAllCirclesAsync()
    {
        var circles = await _circleRepository.GetAllAsync();
        return circles.Select(MapToCircleDto);
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
        return MapToCircleDto(circle);
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

    public async Task<bool> DeleteCircleAsync(int id)
    {
        var circle = await _circleRepository.GetByIdAsync(id);
        if (circle == null)
        {
            return false;
        }

        await _circleRepository.DeleteAsync(id);
        return true;
    }

    /// <summary>
    /// 将Circle模型映射到CircleDto。
    /// </summary>
    private CircleDto MapToCircleDto(Circle circle)
    {
        return new CircleDto
        {
            CircleId = circle.CircleId,
            Name = circle.Name,
            Description = circle.Description,
            OwnerId = circle.OwnerId,
            CreatedAt = circle.CreatedAt
        };
    }
} 