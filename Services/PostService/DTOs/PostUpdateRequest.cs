namespace PostService.DTOs;

public sealed class PostUpdateRequest
{
    public long PostId { get; set; }
    public long? CircleId { get; set; }      // 允许 null（置空圈子）
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public List<long> Tags { get; set; } = new();  // 前端传 tagId 列表
}