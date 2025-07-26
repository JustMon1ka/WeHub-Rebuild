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
    public string Title { get; set; }

    /// <summary>
    /// 活动详细描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 活动奖励说明
    /// </summary>
    public string? Reward { get; set; }

    /// <summary>
    /// 活动开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 活动结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
} 