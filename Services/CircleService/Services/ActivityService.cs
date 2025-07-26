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

    public ActivityService(IActivityRepository activityRepository, ICircleRepository circleRepository)
    {
        _activityRepository = activityRepository;
        _circleRepository = circleRepository;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByCircleIdAsync(int circleId)
    {
        var activities = await _activityRepository.GetByCircleIdAsync(circleId);
        return activities.Select(MapToActivityDto);
    }

    public async Task<ServiceResponse<ActivityDto>> CreateActivityAsync(int circleId, CreateActivityDto createActivityDto, int creatorId)
    {
        var circle = await _circleRepository.GetByIdAsync(circleId);
        if (circle == null || circle.OwnerId != creatorId)
        {
            // 简化权限判断：只有圈主可以创建活动
            return ServiceResponse<ActivityDto>.Fail("权限不足，只有圈主可以创建活动。");
        }

        var activity = new Activity
        {
            CircleId = circleId,
            Title = createActivityDto.Title,
            Description = createActivityDto.Description,
            Reward = createActivityDto.Reward,
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
        if (circle == null || circle.OwnerId != modifierId)
        {
            // 简化权限判断：只有圈主可以修改活动
            return ServiceResponse.Fail("权限不足，只有圈主可以修改活动。");
        }

        activity.Title = updateActivityDto.Title;
        activity.Description = updateActivityDto.Description;
        activity.Reward = updateActivityDto.Reward;
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
    
    private ActivityDto MapToActivityDto(Activity activity)
    {
        return new ActivityDto
        {
            ActivityId = activity.ActivityId,
            CircleId = activity.CircleId,
            Title = activity.Title,
            Description = activity.Description,
            Reward = activity.Reward,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime
        };
    }
} 