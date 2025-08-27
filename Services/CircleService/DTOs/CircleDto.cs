namespace CircleService.DTOs;

/// <summary>
/// 用于传输圈子详细信息的数据传输对象。
/// </summary>
public class CircleDto
{
    /// <summary>
    /// 圈子ID
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// 圈子名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 圈子描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 圈主的用户ID
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 圈子创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 圈子成员数量
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// 圈子分类标签
    /// </summary>
    public string? Categories { get; set; }

    /// <summary>
    /// 圈子头像图片URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 圈子背景图片URL
    /// </summary>
    public string? BannerUrl { get; set; }
} 