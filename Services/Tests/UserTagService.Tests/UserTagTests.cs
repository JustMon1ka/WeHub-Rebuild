using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using UserAuthService.DTOs;
using UserTagService.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace UserTagService.Tests;

public class UserTagTests
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _authClient;
    private readonly HttpClient _tagClient;

    public UserTagTests(ITestOutputHelper output)
    {
        _output = output;
        _authClient = new WebApplicationFactory<UserAuthService.Program>().CreateClient();
        _tagClient = new WebApplicationFactory<UserTagService.Program>().CreateClient();
    }

    [Fact(DisplayName = "添加并获取用户标签")]
    public async Task AddAndGetUserTags_Should_Work()
    {
        // 注册用户
        var registerRequest = new RegisterRequest
        {
            Username = "tagtest_user",
            Password = "Pass123!",
            Email = "tagtest@example.com",
            Phone = "12300000000"
        };

        // 登录用户
        var loginResp = await _authClient.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Identifier = registerRequest.Username,
            Password = registerRequest.Password
        });
        loginResp.EnsureSuccessStatusCode();
        var token = JObject.Parse(await loginResp.Content.ReadAsStringAsync())["data"]?.ToString();
        token.Should().NotBeNullOrEmpty();
        _tagClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _output.WriteLine("✅ 登录成功");

        // 获取当前用户信息
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResp = await _authClient.GetAsync("/api/auth/me");
        var meJson = JObject.Parse(await meResp.Content.ReadAsStringAsync());
        var userId = meJson["data"]?["id"]?.ToString();
        userId.Should().NotBeNullOrEmpty();
        _output.WriteLine("✅ 获取用户信息");

        // 添加用户标签
        var updateTags = new UpdateUserTagRequest
        {
            Tags = new List<int> { 78, 79, 81 }
        };

        var putResp = await _tagClient.PutAsJsonAsync($"/api/users/{userId}/tags", updateTags);
        putResp.EnsureSuccessStatusCode();
        _output.WriteLine("✅ 标签更新成功");

        // 查询用户标签
        var getResp = await _tagClient.GetAsync($"/api/users/{userId}/tags");
        getResp.EnsureSuccessStatusCode();

        var json = JObject.Parse(await getResp.Content.ReadAsStringAsync());
        var tags = json["data"]?["tags"]?.ToObject<List<int>>();

        tags.Should().NotBeNull();
        tags.Should().Contain(78);
        tags.Should().Contain(79);
        _output.WriteLine($"✅ 查询标签成功: {string.Join(", ", tags!)}");
        
        var addResponse = await _tagClient.PostAsync($"/api/users/{userId}/tags/101", null);
        var putContent = await addResponse.Content.ReadAsStringAsync();
        if (!addResponse.IsSuccessStatusCode)
        {
            _output.WriteLine($"❌ 标签更新失败：状态码 {(int)addResponse.StatusCode}");
            _output.WriteLine($"响应内容：{putContent}");
        }
        else
        {
            _output.WriteLine($"✅ 标签更新成功");
        }
        addResponse.EnsureSuccessStatusCode();

        var checkResp1 = await _tagClient.GetAsync($"/api/users/{userId}/tags");
        checkResp1.EnsureSuccessStatusCode();
        var tagsJson1 = JObject.Parse(await checkResp1.Content.ReadAsStringAsync());
        var tags1 = tagsJson1["data"]?["tags"]?.ToObject<List<int>>();
        tags1.Should().Contain(101);
        _output.WriteLine("✅ 添加后标签中包含 101");
        
        var delResponse = await _tagClient.DeleteAsync($"/api/users/{userId}/tags/101");
        delResponse.EnsureSuccessStatusCode();
        _output.WriteLine("✅ 删除标签成功");
        
        var checkResp2 = await _tagClient.GetAsync($"/api/users/{userId}/tags");
        checkResp2.EnsureSuccessStatusCode();
        var tagsJson2 = JObject.Parse(await checkResp2.Content.ReadAsStringAsync());
        var tags2 = tagsJson2["data"]?["tags"]?.ToObject<List<int>>();
        tags2.Should().NotContain(101);
        _output.WriteLine("✅ 删除后标签中不再包含 101");

    }
}
