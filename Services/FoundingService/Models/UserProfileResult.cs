/*
重构代码：
*/
using System.Text.Json.Serialization;

namespace MyBackend.Models;

public class UserProfileResult
{
    // 使用 [JsonPropertyName] 确保如果直接返回给前端，字段名符合原接口习惯 (user_id)
    // 同时也方便 Service 层进行属性映射
    
    [JsonPropertyName("user_id")]
    public int user_id { get; set; }

    [JsonPropertyName("username")]
    public string username { get; set; } = "";

    [JsonPropertyName("nickname")]
    public string nickname { get; set; } = "";

    [JsonPropertyName("avatar_url")]
    public string? avatar_url { get; set; }
}