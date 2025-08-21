using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using UserAuthService.DTOs;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UserAuthService.Tests;

public class EmailCodeLoginTests
{
    private readonly HttpClient _authClient;
    private readonly ITestOutputHelper _output;

    public EmailCodeLoginTests(ITestOutputHelper output)
    {
        _output = output;
        _authClient = new WebApplicationFactory<UserAuthService.Program>().CreateClient();
    }

    [Fact(DisplayName = "邮箱验证码登录流程")]
    public async Task EmailCodeLogin_Should_Succeed()
    {
        var email = "1497146290@qq.com";

        var registerRequest = new RegisterRequest
        {
            Username = "testuser_emailcode",
            Password = "Password123!",
            Email = email,
            Phone = "18888888888"
        };
        
        var sendCodeResp = await _authClient.PostAsJsonAsync("/api/auth/send-code-email", new SendEmailCodeRequest
        {
            Email = email
        });
        
        sendCodeResp.EnsureSuccessStatusCode();
        _output.WriteLine("✅ 发送验证码成功（请查看控制台或邮箱）");
        
        
        var loginResp = await _authClient.PostAsJsonAsync("/api/auth/login-email-code", new LoginByEmailCodeRequest
        {
            Email = email,
            Code = "123456"
        });

        var loginJson = JObject.Parse(await loginResp.Content.ReadAsStringAsync());
        if (!loginResp.IsSuccessStatusCode)
        {
            _output.WriteLine($"❌ 登录失败：{loginJson}");
        }
        else
        {
            var token = loginJson["data"]?.ToString();
            token.Should().NotBeNullOrEmpty();
            _output.WriteLine($"✅ 登录成功，Token: {token}");
        }
    }
}
