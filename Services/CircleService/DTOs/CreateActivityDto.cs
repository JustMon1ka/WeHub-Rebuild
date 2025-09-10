using System.ComponentModel.DataAnnotations;
using CircleService.Models;

namespace CircleService.DTOs;

/// <summary>
/// 用于创建新活动的数据传输对象。
/// </summary>
public class CreateActivityDto
{
    /// <summary>
    /// 活动标题
    /// </summary>
    [Required(ErrorMessage = "活动标题是必填项。")]
    [StringLength(200, ErrorMessage = "活动标题长度不能超过200个字符。")]
    public required string Title { get; set; }

    /// <summary>
    /// 活动详细描述 (可选)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 活动奖励描述 (可选)
    /// </summary>
    [StringLength(200, ErrorMessage = "奖励描述长度不能超过200个字符。")]
    public string? RewardDescription { get; set; }

    /// <summary>
    /// 活动奖励点数 (默认100)
    /// </summary>
    [Range(1, 10000, ErrorMessage = "奖励点数必须在1-10000之间。")]
    public int RewardPoints { get; set; } = 100;

    /// <summary>
    /// 活动类型 (默认手动完成)
    /// </summary>
    public ActivityType ActivityType { get; set; } = ActivityType.Manual;

    /// <summary>
    /// 活动封面图片URL (可选)
    /// </summary>
    [StringLength(500, ErrorMessage = "图片URL长度不能超过500个字符。")]
    public string? ActivityUrl { get; set; }

    /// <summary>
    /// 活动开始时间
    /// </summary>
    [Required(ErrorMessage = "活动开始时间是必填项。")]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 活动结束时间 (默认比开始时间晚一个月)
    /// </summary>
    [Required(ErrorMessage = "活动结束时间是必填项。")]
    public DateTime EndTime { get; set; } = DateTime.UtcNow.AddMonths(1);
} 