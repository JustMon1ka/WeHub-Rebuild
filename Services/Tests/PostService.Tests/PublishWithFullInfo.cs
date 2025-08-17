using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using PostService.DTOs;
using TagService.DTOs;
using UserAuthService.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace PostService.Tests;

public class PublishWithFullInfo
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _authClient;
    private readonly HttpClient _postClient;
    private readonly HttpClient _tagClient;

    public PublishWithFullInfo(ITestOutputHelper output)
    {
        _output = output;

        // 启动 UserAuthService
        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _authClient = authFactory.CreateClient();

        // 启动 PostService
        var postFactory = new WebApplicationFactory<PostService.Program>();
        _postClient = postFactory.CreateClient();
        
        // 启动tagService
        var tagFactory = new WebApplicationFactory<TagService.Program>();
        _tagClient = tagFactory.CreateClient();
    }

    [Fact(DisplayName = "登录 - 上传Tags - 发帖 - 查看获取帖子")]
    public async Task PublishFull()
    {
        // ✅ Step1. 登录
        var loginReq = new LoginRequest
        {
            Identifier = "testuser",
            Password = "TestPass123"
        };
        var loginResp = await _authClient.PostAsJsonAsync("/api/auth/login", loginReq);
        loginResp.EnsureSuccessStatusCode();
        var loginJson = JObject.Parse(await loginResp.Content.ReadAsStringAsync());
        var token = loginJson["token"]?.ToString();
        token.Should().NotBeNullOrEmpty();
        _output.WriteLine($"Step1. 登录成功，Token: {token}");

        _postClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _tagClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // ✅ Step2. 上传 Tags
        var tagReq = new TagAddRequest
        {
            TagsName = new List<string> { "科技", "生活", "学习", "AI" }
        };
        var tagResp = await _tagClient.PostAsJsonAsync("/api/tags/add", tagReq);
        tagResp.EnsureSuccessStatusCode();
        var tagJson = JObject.Parse(await tagResp.Content.ReadAsStringAsync());
        _output.WriteLine("Step2. 添加标签返回：" + tagJson.ToString());

        var tagData = tagJson["data"] as JArray;
        tagData.Should().NotBeNull();
        var tagIds = tagData!.Select(d => d["tagId"]!.Value<long>()).ToList();
        _output.WriteLine($"Step2. 标签ID列表: {string.Join(", ", tagIds)}");

        // ✅ Step3. 发布帖子（带 Tags）
        var publishReq = new PostPublishRequest
        {
            Title = "测试帖子标题2",
            Content = "这是测试帖子的内容2，包含标签。",
            Tags = tagIds
        };
        var postResp = await _postClient.PostAsJsonAsync("/api/posts/publish", publishReq);
        postResp.EnsureSuccessStatusCode();
        var postJson = JObject.Parse(await postResp.Content.ReadAsStringAsync());
        _output.WriteLine("Step3. 发布帖子返回：" + postJson.ToString());

        var postId = postJson["data"]?["postId"]?.Value<long>();
        postId.Should().NotBeNull();
        _output.WriteLine($"Step3. 帖子发布成功，ID: {postId}");

        // ✅ Step4. 获取帖子详情
        var getResp = await _postClient.GetAsync($"/api/posts/{postId}");
        getResp.EnsureSuccessStatusCode();
        var getJson = JObject.Parse(await getResp.Content.ReadAsStringAsync());
        _output.WriteLine("Step4. 获取帖子详情：" + getJson.ToString());

        var data = getJson["data"];
        data.Should().NotBeNull();
        data!["title"]?.ToString().Should().Be("测试帖子标题2");
        data["content"]?.ToString().Should().Contain("测试帖子");

        var tags = data["tags"] as JArray;
        tags.Should().NotBeNull();
        tags!.Count.Should().BeGreaterThanOrEqualTo(3);
        tags.Select(t => t.ToString()).Should().Contain(new[] { "科技", "生活", "学习" });

        _output.WriteLine("✅ 测试完成，帖子包含完整的标签信息。");
    }
}