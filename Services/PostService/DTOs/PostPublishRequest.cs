namespace PostService.DTOs;

public class PostPublishRequest
{
    public long? CircleId { get; set; } // 帖子所属圈子
    public string? Title { get; set; } = string.Empty; // 帖子标题
    public string? Content { get; set; } = string.Empty; // 帖子内容
    public List<long>? Tags { get; set; } = []; // 帖子的Tag集合
}