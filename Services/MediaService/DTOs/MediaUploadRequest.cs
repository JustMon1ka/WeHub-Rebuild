using System.ComponentModel.DataAnnotations;

namespace MediaService.DTOs;

public class MediaUploadRequest
{
    [Required]
    public IFormFile File { get; set; }
}
