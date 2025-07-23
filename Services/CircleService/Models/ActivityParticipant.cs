using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

/// <summary>
/// 用户参与活动的状态
/// </summary>
public enum ParticipantStatus
{
    InProgress,
    Completed
}

/// <summary>
/// 活动奖励的领取状态
/// </summary>
public enum RewardStatus
{
    NotClaimed,
    Claimed
}

/// <summary>
/// 用户参与活动记录表
/// </summary>
[Table("ACTIVITY_PARTICIPANTS")]
public class ActivityParticipant
{
    /// <summary>
    /// 活动ID (复合主键)
    /// </summary>
    [Key]
    [Column("ACTIVITY_ID", Order = 0)]
    public int ActivityId { get; set; }

    /// <summary>
    /// 用户ID (复合主键)
    /// </summary>
    [Key]
    [Column("USER_ID", Order = 1)]
    public int UserId { get; set; }

    /// <summary>
    /// 加入活动的时间
    /// </summary>
    [Column("JOIN_TIME")]
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 参与状态 (进行中/已完成)
    /// </summary>
    [Column("STATUS")]
    public ParticipantStatus Status { get; set; } = ParticipantStatus.InProgress;

    /// <summary>
    /// 奖励状态 (未领取/已领取)
    /// </summary>
    [Column("REWARD_STATUS")]
    public RewardStatus RewardStatus { get; set; } = RewardStatus.NotClaimed;
} 