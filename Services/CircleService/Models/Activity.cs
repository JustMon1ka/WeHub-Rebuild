using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

/// <summary>
/// 活动类型枚举
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// 手动完成（需要用户主动点击完成）
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// 发帖活动（用户发帖后自动完成）
    /// </summary>
    PostCreation = 1,
    
    /// <summary>
    /// 签到活动（用户签到后自动完成）
    /// </summary>
    CheckIn = 2,
    
    /// <summary>
    /// 点赞活动（用户点赞后自动完成）
    /// </summary>
    LikePost = 3
}

/// <summary>
/// 圈子活动表
/// </summary>
[Table("ACTIVITIES")]
public class Activity
{
    /// <summary>
    /// 活动ID (主键)
    /// </summary>
    [Key]
    [Column("ACTIVITY_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivityId { get; set; }

    /// <summary>
    /// 所属圈子的ID
    /// </summary>
    [Required]
    [Column("CIRCLE_ID")]
    public int CircleId { get; set; }

    /// <summary>
    /// 活动标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("TITLE")]
    public required string Title { get; set; }

    /// <summary>
    /// 活动详细描述
    /// </summary>
    [Column("DESCRIPTION")]
    public string? Description { get; set; }
    
    /// <summary>
    /// 活动奖励描述
    /// </summary>
    [Column("REWARD_DESCRIPTION")]
    public string? RewardDescription { get; set; }

    /// <summary>
    /// 活动奖励点数
    /// </summary>
    [Column("REWARD_POINTS")]
    public int RewardPoints { get; set; } = 100;

    /// <summary>
    /// 活动类型
    /// </summary>
    [Column("ACTIVITY_TYPE")]
    public ActivityType ActivityType { get; set; } = ActivityType.Manual;

    /// <summary>
    /// 活动封面图片URL
    /// </summary>
    [Column("ACTIVITY_URL")]
    public string? ActivityUrl { get; set; }

    /// <summary>
    /// 活动开始时间
    /// </summary>
    [Column("START_TIME")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 活动结束时间
    /// </summary>
    [Column("END_TIME")]
    public DateTime EndTime { get; set; }
} 