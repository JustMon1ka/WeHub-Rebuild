using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 活动服务的实现类，负责处理活动相关的业务逻辑。
/// </summary>
public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ICircleRepository _circleRepository;
    private readonly ICircleMemberRepository _memberRepository;
    private readonly IActivityParticipantRepository _participantRepository;

    public ActivityService(IActivityRepository activityRepository, ICircleRepository circleRepository, ICircleMemberRepository memberRepository, IActivityParticipantRepository participantRepository)
    {
        _activityRepository = activityRepository;
        _circleRepository = circleRepository;
        _memberRepository = memberRepository;
        _participantRepository = participantRepository;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByCircleIdAsync(int circleId)
    {
        var activities = await _activityRepository.GetByCircleIdAsync(circleId);
        return activities.Select(MapToActivityDto);
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(int activityId)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId);
        return activity != null ? await MapToActivityDtoAsync(activity) : null;
    }

    public async Task<ServiceResponse<ActivityDto>> CreateActivityAsync(int circleId, CreateActivityDto createActivityDto, int creatorId)
    {
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null)
        {
            return ServiceResponse<ActivityDto>.Fail("圈子不存在。");
        }

        // 权限验证：只有圈主或管理员可以创建活动
        // TODO: 临时注释掉权限验证，方便测试，后续需要恢复
        /*
        var member = await _memberRepository.GetByIdAsync(circleId, creatorId);
        if (member == null || member.Status != CircleMemberStatus.Approved || 
            (member.Role != CircleMemberRole.Admin && circle.OwnerId != creatorId))
        {
            return ServiceResponse<ActivityDto>.Fail("权限不足，只有圈主或管理员可以创建活动。");
        }
        */

        var activity = new Activity
        {
            CircleId = circleId,
            Title = createActivityDto.Title,
            Description = createActivityDto.Description,
            RewardDescription = createActivityDto.RewardDescription,
            RewardPoints = createActivityDto.RewardPoints,
            ActivityType = createActivityDto.ActivityType,
            ActivityUrl = createActivityDto.ActivityUrl,
            StartTime = createActivityDto.StartTime,
            EndTime = createActivityDto.EndTime
        };

        await _activityRepository.AddAsync(activity);
        
        var activityDto = MapToActivityDto(activity);
        return ServiceResponse<ActivityDto>.Succeed(activityDto);
    }

    public async Task<ServiceResponse> UpdateActivityAsync(int activityId, UpdateActivityDto updateActivityDto, int modifierId)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null)
        {
            return ServiceResponse.Fail("活动不存在。");
        }

        var circle = await _circleRepository.GetByIdAsync(activity.CircleId);
        if (circle == null)
        {
            return ServiceResponse.Fail("圈子不存在。");
        }

        // 权限验证：只有圈主或管理员可以修改活动
        // TODO: 临时注释掉权限验证，方便测试，后续需要恢复
        /*
        var member = await _memberRepository.GetByIdAsync(activity.CircleId, modifierId);
        if (member == null || member.Status != CircleMemberStatus.Approved || 
            (member.Role != CircleMemberRole.Admin && circle.OwnerId != modifierId))
        {
            return ServiceResponse.Fail("权限不足，只有圈主或管理员可以修改活动。");
        }
        */

        activity.Title = updateActivityDto.Title;
        activity.Description = updateActivityDto.Description;
        activity.RewardDescription = updateActivityDto.RewardDescription;
        activity.RewardPoints = updateActivityDto.RewardPoints;
        activity.ActivityType = updateActivityDto.ActivityType;
        activity.ActivityUrl = updateActivityDto.ActivityUrl;
        activity.StartTime = updateActivityDto.StartTime;
        activity.EndTime = updateActivityDto.EndTime;

        await _activityRepository.UpdateAsync(activity);
        return ServiceResponse.Succeed();
    }

    public async Task<ServiceResponse> DeleteActivityAsync(int activityId, int deleterId)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null)
        {
            return ServiceResponse.Fail("活动不存在。");
        }

        var circle = await _circleRepository.GetByIdAsync(activity.CircleId);
        if (circle == null || circle.OwnerId != deleterId)
        {
            // 简化权限判断：只有圈主可以删除活动
            return ServiceResponse.Fail("权限不足，只有圈主可以删除活动。");
        }
        
        await _activityRepository.DeleteAsync(activityId);
        return ServiceResponse.Succeed();
    }
    
    private async Task<ActivityDto> MapToActivityDtoAsync(Activity activity)
    {
        // 获取活动参与人数
        var participants = await _participantRepository.GetByActivityIdAsync(activity.ActivityId);
        var participantCount = participants.Count();

        return new ActivityDto
        {
            ActivityId = activity.ActivityId,
            CircleId = activity.CircleId,
            Title = activity.Title,
            Description = activity.Description,
            RewardDescription = activity.RewardDescription,
            RewardPoints = activity.RewardPoints,
            ActivityType = activity.ActivityType,
            ActivityUrl = activity.ActivityUrl,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime,
            ParticipantCount = participantCount
        };
    }

    private ActivityDto MapToActivityDto(Activity activity)
    {
        return new ActivityDto
        {
            ActivityId = activity.ActivityId,
            CircleId = activity.CircleId,
            Title = activity.Title,
            Description = activity.Description,
            RewardDescription = activity.RewardDescription,
            RewardPoints = activity.RewardPoints,
            ActivityType = activity.ActivityType,
            ActivityUrl = activity.ActivityUrl,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime,
            ParticipantCount = 0 // 默认值，用于不需要参与人数的场景
        };
    }
} 