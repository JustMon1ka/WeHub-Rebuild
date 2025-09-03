namespace PostService.DTOs;

public class SearchResponse
{
    public long PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string Content { get; set; } = string.Empty;
}