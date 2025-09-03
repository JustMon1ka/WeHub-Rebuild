namespace PostService.DTOs;

public class CommentRequest
{
    public enum CommentType
    {
        Comment,
        Reply
    }
    
    public CommentType Type { get; set; }
    public long TargetId { get; set; }
    public string? Content { get; set; }
}