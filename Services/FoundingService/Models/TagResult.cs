/*
重构代码：
*/
using System.Text.Json.Serialization;

namespace MyBackend.Models;

public class TagResult
{
    [JsonPropertyName("tag_id")]
    public int TagId { get; set; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("last_quote")]
    public DateTime? LastQuote { get; set; }
}