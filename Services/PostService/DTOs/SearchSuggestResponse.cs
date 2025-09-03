namespace PostService.DTOs;

public class SearchSuggestResponse
{
    public enum KeywordType
    {
        Title,
        Tag,
        Content,
        User,
        Circle,
        Other
    }
    
    public string? Keyword { get; set; }
    public KeywordType? Type { get; set; }
}