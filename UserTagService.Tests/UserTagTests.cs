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
        var regResp = await _authClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        regResp.EnsureSuccessStatusCode();
        _output.WriteLine("✅ 注册成功");

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
            Tags = new List<int> { 1, 2, 3 }
        };

        var putResp = await _tagClient.PutAsJsonAsync($"/api/users/{userId}/tags", updateTags);
        putResp.EnsureSuccessStatusCode();
        _output.WriteLine("✅ 标签更新成功");

        // 查询用户标签
        var getResp = await _tagClient.GetAsync($"/api/users/{userId}/tags");
        getResp.EnsureSuccessStatusCode();

        var json = JObject.Parse(await getResp.Content.ReadAsStringAsync());
        var tags = json["data"]?["tags"]?.ToObject<List<string>>();

        tags.Should().NotBeNull();
        tags.Should().Contain("1");
        tags.Should().Contain("2");

        _output.WriteLine($"✅ 查询标签成功: {string.Join(", ", tags!)}");
    }
}
