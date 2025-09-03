using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaService.Models;

[Table("MEDIA")]
public class Media
{
    [Key]
    [Column("MEDIA_ID")]
    public string MediaId { get; set; }
    
    [Column("MEDIA_TYPE")]
    public string MediaType { get; set; }  // image, video, audio, other
    
    [Column("MEDIA_URL")]
    public string MediaUrl { get; set; }
}