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
    public string Name { get; set; }

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
} 