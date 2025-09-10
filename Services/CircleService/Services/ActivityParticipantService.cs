using CircleService.DTOs;
using CircleService.Models;
using CircleService.Repositories;
using System;
using System.Threading.Tasks;

namespace CircleService.Services;

/// <summary>
/// 用户参与活动服务的实现类，负责处理相关的业务逻辑。
/// </summary>
public class ActivityParticipantService : IActivityParticipantService
{
    private readonly IActivityParticipantRepository _participantRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ICircleMemberRepository _memberRepository;

    public ActivityParticipantService(
        IActivityParticipantRepository participantRepository,
        IActivityRepository activityRepository,
        ICircleMemberRepository memberRepository)
    {
        _participantRepository = participantRepository;
        _activityRepository = activityRepository;
        _memberRepository = memberRepository;
    }

    public async Task<ServiceResponse> JoinActivityAsync(int activityId, int userId)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null || DateTime.UtcNow > activity.EndTime)
        {
            return ServiceResponse.Fail("活动不存在或已结束。");
        }

        var participant = await _participantRepository.GetByIdAsync(activityId, userId);
        if (participant != null)
        {
            return ServiceResponse.Fail("您已参加此活动。");
        }

        var newParticipant = new ActivityParticipant
        {
            ActivityId = activityId,
            UserId = userId
        };

        await _participantRepository.AddAsync(newParticipant);
        return ServiceResponse.Succeed();
    }

    public async Task<ServiceResponse> CompleteActivityTaskAsync(int activityId, int userId)
    {
        var participant = await _participantRepository.GetByIdAsync(activityId, userId);
        if (participant == null)
        {
            return ServiceResponse.Fail("您尚未参加此活动。");
        }

        if (participant.Status == ParticipantStatus.Completed)
        {
            return ServiceResponse.Fail("您已完成此活动任务。");
        }

        participant.Status = ParticipantStatus.Completed;
        await _participantRepository.UpdateAsync(participant);
        return ServiceResponse.Succeed();
    }

    public async Task<ServiceResponse<RewardDto>> ClaimRewardAsync(int activityId, int userId)
    {
        var participant = await _participantRepository.GetByIdAsync(activityId, userId);
        if (participant == null || participant.Status != ParticipantStatus.Completed)
        {
            return ServiceResponse<RewardDto>.Fail("您尚未完成此活动，无法领取奖励。");
        }

        if (participant.RewardStatus == RewardStatus.Claimed)
        {
            return ServiceResponse<RewardDto>.Fail("您已领取过此活动的奖励。");
        }

        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null)
        {
            // 理论上不应该发生，因为能参与就说明活动存在
            return ServiceResponse<RewardDto>.Fail("活动数据异常。");
        }

        // 使用活动配置的奖励点数
        var pointsToAward = activity.RewardPoints;
        
        var member = await _memberRepository.GetByIdAsync(activity.CircleId, userId);
        if (member != null)
        {
            member.Points += pointsToAward;
            await _memberRepository.UpdateAsync(member);
        }

        participant.RewardStatus = RewardStatus.Claimed;
        await _participantRepository.UpdateAsync(participant);

        var rewardDto = new RewardDto
        {
            RewardDescription = activity.RewardDescription ?? "恭喜您完成活动！",
            PointsAwarded = pointsToAward
        };

        return ServiceResponse<RewardDto>.Succeed(rewardDto);
    }

    public async Task<IEnumerable<UserActivityParticipationDto>> GetUserActivityParticipationsAsync(int userId)
    {
        var participations = await _participantRepository.GetByUserIdAsync(userId);
        var result = new List<UserActivityParticipationDto>();

        foreach (var participation in participations)
        {
            var activity = await _activityRepository.GetByIdAsync(participation.ActivityId);
            if (activity != null)
            {
                result.Add(new UserActivityParticipationDto
                {
                    ActivityId = participation.ActivityId,
                    CircleId = activity.CircleId,
                    ActivityTitle = activity.Title,
                    UserId = participation.UserId,
                    Status = participation.Status,
                    RewardStatus = participation.RewardStatus,
                    JoinTime = participation.JoinTime,
                    RewardPoints = activity.RewardPoints,
                    ActivityType = activity.ActivityType,
                    StartTime = activity.StartTime,
                    EndTime = activity.EndTime
                });
            }
        }

        return result;
    }

    public async Task<UserActivityParticipationDto?> GetUserActivityParticipationAsync(int activityId, int userId)
    {
        var participation = await _participantRepository.GetByIdAsync(activityId, userId);
        if (participation == null)
        {
            return null;
        }

        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null)
        {
            return null;
        }

        return new UserActivityParticipationDto
        {
            ActivityId = participation.ActivityId,
            CircleId = activity.CircleId,
            ActivityTitle = activity.Title,
            UserId = participation.UserId,
            Status = participation.Status,
            RewardStatus = participation.RewardStatus,
            JoinTime = participation.JoinTime,
            RewardPoints = activity.RewardPoints,
            ActivityType = activity.ActivityType,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime
        };
    }
} 