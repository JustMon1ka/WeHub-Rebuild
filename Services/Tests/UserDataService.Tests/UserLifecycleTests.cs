using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using UserDataService.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using UserAuthService.DTOs;
using Xunit.Abstractions;

namespace UserDataService.Tests
{
    public class UserLifecycleTests
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _authClient;
    private readonly HttpClient _dataClient;

    public UserLifecycleTests(ITestOutputHelper output)
    {
        _output = output;

        // 启动 UserAuthService
        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _authClient = authFactory.CreateClient();

        // 启动 UserDataService
        var dataFactory = new WebApplicationFactory<UserDataService.Program>();
        _dataClient = dataFactory.CreateClient();
    }

    private readonly RegisterRequest _registerRequest = new RegisterRequest
    {
        Username = "delete_test_user",
        Password = "TestPass123!",
        Email = "delete_test_user@example.com",
        Phone = "13800001111"
    };

    [Fact(DisplayName = "User lifecycle: register → login → get me → delete → login fail")]
    public async Task UserLifecycle_Flow_Should_Work_As_Expected()
    {
        // 1. 注册用户
        var registerResponse = await _authClient.PostAsJsonAsync("/api/auth/register", _registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        _output.WriteLine("✅ 注册成功");

        // 2. 登录获取 Token
        var loginRequest = new LoginRequest
        {
            Identifier = _registerRequest.Username,
            Password = _registerRequest.Password
        };

        var loginResponse = await _authClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginJson = JObject.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginJson["token"]?.ToString();
        token.Should().NotBeNullOrEmpty();
        _output.WriteLine($"✅ 登录成功，Token: {token}");

        // 3. 获取用户信息
        _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _authClient.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();
        var meJson = JObject.Parse(await meResponse.Content.ReadAsStringAsync());
        var userId = meJson["id"]?.ToString();
        var username = meJson["username"]?.ToString();
        userId.Should().NotBeNullOrEmpty();
        _output.WriteLine($"✅ 当前用户ID: {userId}, 用户名: {username}");

        // 4. 注销用户（UserDataService）
        _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var deleteResponse = await _dataClient.DeleteAsync("/api/user/delete");
        deleteResponse.EnsureSuccessStatusCode();
        _output.WriteLine($"✅ 注销响应: {await deleteResponse.Content.ReadAsStringAsync()}");

        // 5. 再次尝试登录
        var retryLogin = await _authClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        retryLogin.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        _output.WriteLine("✅ 注销后再次登录失败");
    }
}

}
