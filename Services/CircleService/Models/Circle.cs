using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

/// <summary>
/// 圈子表
/// </summary>
[Table("CIRCLES")]
public class Circle
{
    /// <summary>
    /// 圈子ID (主键)
    /// </summary>
    [Key]
    [Column("CIRCLE_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CircleId { get; set; }

    /// <summary>
    /// 圈子名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("NAME")]
    public required string Name { get; set; }

    /// <summary>
    /// 圈子描述
    /// </summary>
    [Column("DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 创建者/圈主的用户ID
    /// </summary>
    [Required]
    [Column("OWNER_ID")]
    public int OwnerId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 圈子分类标签（多个分类用逗号分隔）
    /// </summary>
    [Column("CATEGORIES")]
    public string? Categories { get; set; }

    /// <summary>
    /// 圈子头像图片URL
    /// </summary>
    [Column("AVATAR_URL")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 圈子背景图片URL
    /// </summary>
    [Column("BANNER_URL")]
    public string? BannerUrl { get; set; }
} 