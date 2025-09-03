namespace PostService.DTOs;

public class CommentResponse
{
    public CommentRequest.CommentType Type { get; set; }
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
}