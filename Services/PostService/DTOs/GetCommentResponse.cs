namespace PostService.DTOs;

public class GetCommentResponse
{
    public CommentRequest.CommentType Type { get; set; }
    public long Id { get; set; }
    public long? TargetId { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public long Likes { get; set; }
    public string? PostTitle { get; set; }
}