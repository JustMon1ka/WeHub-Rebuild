using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagService.Models;

[Table("TAG")]
public class Tag
{
    [Key]
    [Column("TAG_ID")]
    public long TagId { get; set; }

    [Column("TAG_NAME")] 
    public string TagName { get; set; } = string.Empty;
    
    [Column("COUNT")]
    public long Count { get; set; }
    
    [Column("LAST_QUOTE")]
    public DateTime? LastQuote { get; set; }
}