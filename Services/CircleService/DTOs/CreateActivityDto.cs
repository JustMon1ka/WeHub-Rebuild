using System.ComponentModel.DataAnnotations;

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
    public string Title { get; set; }

    /// <summary>
    /// 活动详细描述 (可选)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 活动奖励说明 (可选)
    /// </summary>
    [StringLength(200, ErrorMessage = "奖励说明长度不能超过200个字符。")]
    public string? Reward { get; set; }

    /// <summary>
    /// 活动开始时间
    /// </summary>
    [Required(ErrorMessage = "活动开始时间是必填项。")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 活动结束时间
    /// </summary>
    [Required(ErrorMessage = "活动结束时间是必填项。")]
    public DateTime EndTime { get; set; }
} 