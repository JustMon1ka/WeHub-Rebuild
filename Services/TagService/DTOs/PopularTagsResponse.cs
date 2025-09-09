namespace TagService.DTOs;

public class PopularTagsResponse
{
    public long TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public long Count { get; set; }
}