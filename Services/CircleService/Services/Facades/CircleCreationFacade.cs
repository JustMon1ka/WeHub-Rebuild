using CircleService.Builders;
using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;

namespace CircleService.Services;

public interface ICircleCreationFacade
{
    Task<Circle> CreateNewCircleAsync(CreateCircleDto dto, int ownerId);
}

public class CircleCreationFacade : ICircleCreationFacade
{
    private readonly ICircleRepository _circleRepository;
    private readonly ICircleMemberRepository _circleMemberRepository;
    private readonly IActivityService _activityService;

    public CircleCreationFacade(
        ICircleRepository circleRepository,
        ICircleMemberRepository circleMemberRepository)
    {
        _circleRepository = circleRepository;
        _circleMemberRepository = circleMemberRepository;
    }

    public async Task<Circle> CreateNewCircleAsync(CreateCircleDto dto, int ownerId)
    {
        // 1. 用 Builder 构建 Circle 实体
        var circle = new CircleBuilder()
            .WithName(dto.Name)
            .WithDescription(dto.Description)
            .WithCategories(string.Join(",", dto.Categories))
            .WithOwnerId(ownerId)
            .Build();

        // 2. 保存 Circle
        await _circleRepository.CreateCircleAsync(circle);

        // 3. 创建圈主成员记录
        await _circleMemberRepository.AddAsync(new CircleMember
        {
            CircleId = circle.CircleId,
            UserId = ownerId,
            Role = CircleMemberRole.Admin
        });

        // 4. 可选：设置默认头像（目前不动）
        // 之后可以扩展 mediaService.SetDefaultAvatar(circle.Id)

        return circle;
    }
}
