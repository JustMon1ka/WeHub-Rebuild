namespace PostService.DTOs;

public class PostResponse
{
    public long PostId { get; set; }        // 帖子ID
    public long UserId { get; set; }        // 用户ID
    public string Title { get; set; } = string.Empty;   // 帖子标题
    public string Content { get; set; } = string.Empty; // 帖子内容
    public List<string> Tags { get; set; } = new();     // 标签列表
    public DateTime CreatedAt { get; set; }             // 创建时间
    public int Views { get; set; }                      // 浏览量
    public int Likes { get; set; }                      // 点赞数
    public long CircleId { get; set; }                  // 所属圈子Id
}