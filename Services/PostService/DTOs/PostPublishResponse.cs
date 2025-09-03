namespace PostService.DTOs;

public class PostPublishResponse
{
    public long PostId { get; set; }  // 帖子id
    public string CreatedAt { get; set; } = string.Empty;  // 发布成功的时间
    
}