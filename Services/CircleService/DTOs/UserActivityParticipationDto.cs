using CircleService.Models;

namespace CircleService.DTOs;

/// <summary>
/// 用户活动参与状态DTO
/// </summary>
public class UserActivityParticipationDto
{
    /// <summary>
    /// 活动ID
    /// </summary>
    public int ActivityId { get; set; }

    /// <summary>
    /// 圈子ID
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    public required string ActivityTitle { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 参与状态
    /// </summary>
    public ParticipantStatus Status { get; set; }

    /// <summary>
    /// 奖励状态
    /// </summary>
    public RewardStatus RewardStatus { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    public DateTime JoinTime { get; set; }

    /// <summary>
    /// 活动奖励点数
    /// </summary>
    public int RewardPoints { get; set; }

    /// <summary>
    /// 活动类型
    /// </summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>
    /// 活动开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 活动结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
}
