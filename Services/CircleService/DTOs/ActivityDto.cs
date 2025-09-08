using CircleService.Models;

namespace CircleService.DTOs;

/// <summary>
/// 用于传输活动详细信息的数据传输对象。
/// </summary>
public class ActivityDto
{
    /// <summary>
    /// 活动ID
    /// </summary>
    public int ActivityId { get; set; }

    /// <summary>
    /// 所属圈子的ID
    /// </summary>
    public int CircleId { get; set; }
    
    /// <summary>
    /// 活动标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 活动详细描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 活动奖励描述
    /// </summary>
    public string? RewardDescription { get; set; }

    /// <summary>
    /// 活动奖励点数
    /// </summary>
    public int RewardPoints { get; set; }

    /// <summary>
    /// 活动类型
    /// </summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>
    /// 活动封面图片URL
    /// </summary>
    public string? ActivityUrl { get; set; }

    /// <summary>
    /// 活动开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 活动结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 活动参与总人数
    /// </summary>
    public int ParticipantCount { get; set; }
} 