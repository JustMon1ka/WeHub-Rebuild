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

        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _authClient = authFactory.CreateClient();

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
            loginResponse.EnsureSuccessStatusCode();

            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(loginContent);

            var token = json["data"]?.ToString(); // 🔥 关键修改：从 data 中读取 token
            token.Should().NotBeNullOrEmpty($"登录失败: {identifier}");
            _output.WriteLine($"✅ 使用 {identifier} 登录成功，Token: {token}");

            lastToken = token;
        }

        // Step4: 获取当前用户信息
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lastToken);
        _output.WriteLine($"{_authClient.DefaultRequestHeaders}");
        var meResponse = await _authClient.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();

        var meJson = JObject.Parse(await meResponse.Content.ReadAsStringAsync());
        var userId = meJson["data"]?["id"]?.ToString(); // 🔥 关键修改：从 data 中读取 id
        userId.Should().NotBeNullOrEmpty();
        _output.WriteLine($"✅ 获取当前用户信息成功，用户ID: {userId}");

        // Step5: 注销该用户（模拟 UserDataService）
        _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lastToken);
        var deleteResponse = await _dataClient.DeleteAsync($"/api/users/{userId}/delete");
        deleteResponse.EnsureSuccessStatusCode();
        var deleteResult = await deleteResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"✅ 注销成功，响应: {deleteResult}");
    }
}
