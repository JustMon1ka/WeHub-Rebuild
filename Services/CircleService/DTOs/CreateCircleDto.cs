using System.ComponentModel.DataAnnotations;

namespace CircleService.DTOs;

/// <summary>
/// 用于创建新圈子的数据传输对象。
/// </summary>
public class CreateCircleDto
{
    /// <summary>
    /// 圈子名称
    /// </summary>
    [Required(ErrorMessage = "圈子名称是必填项。")]
    [StringLength(100, ErrorMessage = "圈子名称长度不能超过100个字符。")]
    public string Name { get; set; }

    /// <summary>
    /// 圈子描述 (可选)
    /// </summary>
    [StringLength(500, ErrorMessage = "圈子描述长度不能超过500个字符。")]
    public string? Description { get; set; }
} 