using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CircleService.Models;

/// <summary>
/// 帖子表
/// </summary>
[Table("POST", Schema = "WEIBO_DEV")]
public class Post
{
    /// <summary>
    /// 帖子ID (主键)
    /// </summary>
    [Key]
    [Column("POST_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PostId { get; set; }

    /// <summary>
    /// 发布者用户ID
    /// </summary>
    [Column("USER_ID")]
    public int? UserId { get; set; }

    /// <summary>
    /// 帖子内容
    /// </summary>
    [Column("CONTENT")]
    public string? Content { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    [Column("IS_DELETED")]
    public int? IsDeleted { get; set; }

    /// <summary>
    /// 是否已隐藏
    /// </summary>
    [Column("IS_HIDDEN")]
    public int? IsHidden { get; set; }

    /// <summary>
    /// 浏览量
    /// </summary>
    [Column("VIEWS")]
    public int? Views { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    [Column("LIKES")]
    public int? Likes { get; set; }

    /// <summary>
    /// 点踩数
    /// </summary>
    [Column("DISLIKES")]
    public int? Dislikes { get; set; }

    /// <summary>
    /// 帖子标题
    /// </summary>
    [Column("TITLE")]
    public string? Title { get; set; }

    /// <summary>
    /// 所属圈子ID（默认为100000，不可为null）
    /// </summary>
    [Required]
    [Column("CIRCLE_ID")]
    public int CircleId { get; set; } = 100000;

    /// <summary>
    /// 搜索文本
    /// </summary>
    [Column("SEARCH_TEXT")]
    public string? SearchText { get; set; }
}
