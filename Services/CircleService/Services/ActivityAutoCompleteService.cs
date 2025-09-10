using CircleService.Models;
using CircleService.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 活动自动完成服务实现
/// </summary>
public class ActivityAutoCompleteService : IActivityAutoCompleteService
{
    private readonly IActivityRepository _activityRepository;
    private readonly IActivityParticipantRepository _participantRepository;

    public ActivityAutoCompleteService(
        IActivityRepository activityRepository,
        IActivityParticipantRepository participantRepository)
    {
        _activityRepository = activityRepository;
        _participantRepository = participantRepository;
    }

    public async Task<int> HandlePostCreatedAsync(int userId, int circleId, int postId)
    {
        try
        {
            // 查找该圈子内所有发帖类型的活动
            var postActivities = await _activityRepository.GetActivitiesByTypeAsync(circleId, ActivityType.PostCreation);
            
            int completedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var activity in postActivities)
            {
                // 检查活动是否在有效期内
                if (now < activity.StartTime || now > activity.EndTime)
                    continue;

                // 检查用户是否已参与该活动
                var participant = await _participantRepository.GetByIdAsync(activity.ActivityId, userId);
                if (participant == null)
                {
                    // 用户未参与，自动加入活动
                    participant = new ActivityParticipant
                    {
                        ActivityId = activity.ActivityId,
                        UserId = userId,
                        JoinTime = now,
                        Status = ParticipantStatus.Completed, // 发帖即完成
                        RewardStatus = RewardStatus.NotClaimed
                    };
                    await _participantRepository.AddAsync(participant);
                    completedCount++;
                }
                else if (participant.Status == ParticipantStatus.InProgress)
                {
                    // 用户已参与但未完成，标记为已完成
                    participant.Status = ParticipantStatus.Completed;
                    await _participantRepository.UpdateAsync(participant);
                    completedCount++;
                }
            }

            return completedCount;
        }
        catch (Exception ex)
        {
            // 记录日志但不抛出异常，避免影响主业务流程
            Console.WriteLine($"处理发帖活动自动完成时出错: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> HandlePostLikedAsync(int userId, int circleId, int postId)
    {
        try
        {
            // 查找该圈子内所有点赞类型的活动
            var likeActivities = await _activityRepository.GetActivitiesByTypeAsync(circleId, ActivityType.LikePost);
            
            int completedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var activity in likeActivities)
            {
                // 检查活动是否在有效期内
                if (now < activity.StartTime || now > activity.EndTime)
                    continue;

                // 检查用户是否已参与该活动
                var participant = await _participantRepository.GetByIdAsync(activity.ActivityId, userId);
                if (participant == null)
                {
                    // 用户未参与，自动加入活动
                    participant = new ActivityParticipant
                    {
                        ActivityId = activity.ActivityId,
                        UserId = userId,
                        JoinTime = now,
                        Status = ParticipantStatus.Completed, // 点赞即完成
                        RewardStatus = RewardStatus.NotClaimed
                    };
                    await _participantRepository.AddAsync(participant);
                    completedCount++;
                }
                else if (participant.Status == ParticipantStatus.InProgress)
                {
                    // 用户已参与但未完成，标记为已完成
                    participant.Status = ParticipantStatus.Completed;
                    await _participantRepository.UpdateAsync(participant);
                    completedCount++;
                }
            }

            return completedCount;
        }
        catch (Exception ex)
        {
            // 记录日志但不抛出异常，避免影响主业务流程
            Console.WriteLine($"处理点赞活动自动完成时出错: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> HandleCheckInAsync(int userId, int circleId)
    {
        try
        {
            // 查找该圈子内所有签到类型的活动
            var checkInActivities = await _activityRepository.GetActivitiesByTypeAsync(circleId, ActivityType.CheckIn);
            
            int completedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var activity in checkInActivities)
            {
                // 检查活动是否在有效期内
                if (now < activity.StartTime || now > activity.EndTime)
                    continue;

                // 检查用户是否已参与该活动
                var participant = await _participantRepository.GetByIdAsync(activity.ActivityId, userId);
                if (participant == null)
                {
                    // 用户未参与，自动加入活动
                    participant = new ActivityParticipant
                    {
                        ActivityId = activity.ActivityId,
                        UserId = userId,
                        JoinTime = now,
                        Status = ParticipantStatus.Completed, // 签到即完成
                        RewardStatus = RewardStatus.NotClaimed
                    };
                    await _participantRepository.AddAsync(participant);
                    completedCount++;
                }
                else if (participant.Status == ParticipantStatus.InProgress)
                {
                    // 用户已参与但未完成，标记为已完成
                    participant.Status = ParticipantStatus.Completed;
                    await _participantRepository.UpdateAsync(participant);
                    completedCount++;
                }
            }

            return completedCount;
        }
        catch (Exception ex)
        {
            // 记录日志但不抛出异常，避免影响主业务流程
            Console.WriteLine($"处理签到活动自动完成时出错: {ex.Message}");
            return 0;
        }
    }
}
