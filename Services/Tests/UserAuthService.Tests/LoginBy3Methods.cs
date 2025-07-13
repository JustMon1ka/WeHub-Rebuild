using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using UserAuthService.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace UserAuthService.Tests;

public class LoginBy3Methods
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _authClient;
    private readonly HttpClient _dataClient;

    public LoginBy3Methods(ITestOutputHelper output)
    {
        _output = output;

        // 启动 UserAuthService
        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _authClient = authFactory.CreateClient();

        // 启动 UserDataService
        var dataFactory = new WebApplicationFactory<UserDataService.Program>();
        _dataClient = dataFactory.CreateClient();
    }

    [Fact(DisplayName = "注册用户 -> 用户名登录 -> 手机号登录 -> 邮箱登录 -> 注销用户")]
    public async Task Register_Should_Create_User()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "testuser_login3",
            Password = "TestPass123!",
            Email = "testuser_register@example.com",
            Phone = "1234567899"
        };

        // Step1: 注册用户
        var registerResponse = await _authClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var content = await registerResponse.Content.ReadAsStringAsync();
        content.Should().Contain("successful");
        _output.WriteLine("✅ 注册成功");

        // 三种登录方式测试
        var identifiers = new[] { registerRequest.Username, registerRequest.Email, registerRequest.Phone };
        string? lastToken = null;

        foreach (var identifier in identifiers)
        {
            var loginRequest = new LoginRequest
            {
                Identifier = identifier,
                Password = registerRequest.Password
            };

            var loginResponse = await _authClient.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            loginResponse.EnsureSuccessStatusCode();
            var json = JObject.Parse(loginContent);
            var token = json["token"]?.ToString();
            token.Should().NotBeNullOrEmpty($"登录失败: {identifier}");
            _output.WriteLine($"✅ 使用 {identifier} 登录成功，Token: {token}");
            lastToken = token; // 保存最后一个可用的 token
        }

        // Step4: 获取当前用户信息
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lastToken);
        var meResponse = await _authClient.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();
        var meJson = JObject.Parse(await meResponse.Content.ReadAsStringAsync());
        var userId = meJson["id"]?.ToString();
        userId.Should().NotBeNullOrEmpty();
        _output.WriteLine($"✅ 获取当前用户信息成功，用户ID: {userId}");

        // Step5: 注销该用户（使用 UserDataService）
        _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lastToken);
        var deleteResponse = await _dataClient.DeleteAsync("/api/user/delete");
        deleteResponse.EnsureSuccessStatusCode();
        var deleteResult = await deleteResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"✅ 注销成功，响应: {deleteResult}");
    }
}