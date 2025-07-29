using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PostService.Models;
using TagService.Models;

namespace Models;

[Table("POSTTAG")]
[PrimaryKey(nameof(PostId), nameof(TagId))]
public class PostTag
{
    [Column("POST_ID", Order = 0)]
    public long PostId { get; set; }

    [Column("TAG_ID", Order = 1)]
    public long TagId { get; set; }

    // 可选：导航属性
    public virtual Post Post { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}