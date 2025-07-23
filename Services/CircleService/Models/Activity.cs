using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

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
    public string Title { get; set; }

    /// <summary>
    /// 活动详细描述
    /// </summary>
    [Column("DESCRIPTION")]
    public string? Description { get; set; }
    
    /// <summary>
    /// 活动奖励说明
    /// </summary>
    [Column("REWARD")]
    public string? Reward { get; set; }

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