namespace MyBackend.Models;

public class RecommendResult
{
    public string Topic { get; set; } = "";
    public int Count { get; set; }
    public string Category { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string Desc { get; set; } = "";
}
