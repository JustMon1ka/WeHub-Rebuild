using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using UserAuthService.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<UserAuthService.Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<UserAuthService.Program> factory)
    {
        _client = factory.CreateClient();
    }

    private readonly RegisterRequest _registerRequest = new RegisterRequest
    {
        Username = "testuser",
        Password = "TestPass123!",
        Email = "testuser@example.com",
        Phone = "1234567890"
    };

    private readonly LoginRequest _loginRequest = new LoginRequest
    {
        Username = "testuser",
        Password = "TestPass123!"
    };

    [Fact(DisplayName = "1. 注册用户应返回成功")]
    public async Task Register_Should_Create_User()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", _registerRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("successful");
    }

    [Fact(DisplayName = "2. 登录应返回有效Token")]
    public async Task Login_Should_Return_JwtToken_For_Valid_Credentials()
    {
        // 确保用户已存在
        await _client.PostAsJsonAsync("/api/auth/register", _registerRequest);

        var response = await _client.PostAsJsonAsync("/api/auth/login", _loginRequest);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(jsonString);
        var token = json["token"]?.ToString();

        token.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "3. 获取当前用户信息应返回用户名")]
    public async Task Me_Should_Return_User_Info_With_Valid_Token()
    {
        // 注册 & 登录获取 token
        await _client.PostAsJsonAsync("/api/auth/register", _registerRequest);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", _loginRequest);
        var jsonString = await loginResponse.Content.ReadAsStringAsync();
        var json = JObject.Parse(jsonString);
        var token = json["token"]?.ToString();
        token.Should().NotBeNullOrEmpty();

        // 设置 Authorization
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();

        var meContent = await meResponse.Content.ReadAsStringAsync();
        meContent.Should().Contain("testuser");
    }
}
