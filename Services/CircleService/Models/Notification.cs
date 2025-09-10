using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

/// <summary>
/// 通知类型
/// </summary>
public enum NotificationType
{
    Comment,
    Like,
    Follow,
    System
}

/// <summary>
/// 通知表
/// </summary>
[Table("NOTIFICATIONS")]
public class Notification
{
    /// <summary>
    /// 通知ID (主键)
    /// </summary>
    [Key]
    [Column("NOTIFICATION_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationId { get; set; }

    /// <summary>
    /// 接收通知的用户ID
    /// </summary>
    [Required]
    [Column("USER_ID")]
    public int UserId { get; set; }

    /// <summary>
    /// 通知类型 (评论、点赞、关注、系统)
    /// </summary>
    [Required]
    [Column("TYPE")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// 关联对象ID (例如: 帖子ID, 评论ID)
    /// </summary>
    [Column("RELATED_ID")]
    public int? RelatedId { get; set; }

    /// <summary>
    /// 通知内容
    /// </summary>
    [Required]
    [Column("CONTENT")]
    public string Content { get; set; }

    /// <summary>
    /// 触发通知的用户ID (例如: 点赞你的人)
    /// </summary>
    [Column("SENDER_ID")]
    public int? SenderId { get; set; }

    /// <summary>
    /// 通知创建时间
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已读
    /// </summary>
    [Column("IS_READ")]
    public bool IsRead { get; set; } = false;
} 