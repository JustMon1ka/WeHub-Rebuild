using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using TagService.DTOs;
using UserAuthService.DTOs;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

namespace TagService.Test;

public class AddGetTag
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _auth;
    private readonly HttpClient _tag;

    public AddGetTag(ITestOutputHelper output)
    {
        _output = output;
        
        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _auth = authFactory.CreateClient();

        var tagFactory = new WebApplicationFactory<TagService.Program>();
        _tag = tagFactory.CreateClient();
    }

    [Fact(DisplayName = "登录 - 添加tag - 获取tag")]
    public async Task LoginAddGet()
    {
        // Step1. 登录
        var user = new LoginRequest
        {
            Identifier = "testuser",
            Password = "TestPass123"
        };
        var loginResponse = await _auth.PostAsJsonAsync("/api/auth/login", user);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginJson = JObject.Parse(loginContent);
        var token = loginJson["token"]?.ToString();
        token.Should().NotBeNullOrEmpty();
        _output.WriteLine($"Step1. 登录成功，token：{token}");
        _tag.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Step2. 添加第一个标签
        var tag1Req = new TagAddRequest { TagName = "科技" };
        var tag1Response = await _tag.PostAsJsonAsync("/api/tags/add", tag1Req);
        tag1Response.EnsureSuccessStatusCode();
        var tag1Json = JObject.Parse(await tag1Response.Content.ReadAsStringAsync());
        tag1Json["data"]?["tagId"].Should().NotBeNull();
        var tag1Id = tag1Json["data"]?["tagId"]!.Value<long>();
        _output.WriteLine($"Step2. 添加标签1成功，ID: {tag1Id}");

        // Step3. 添加第二个标签
        var tag2Req = new TagAddRequest { TagName = "生活" };
        var tag2Response = await _tag.PostAsJsonAsync("/api/tags/add", tag2Req);
        tag2Response.EnsureSuccessStatusCode();
        var tag2Json = JObject.Parse(await tag2Response.Content.ReadAsStringAsync());
        tag2Json["data"]?["tagId"].Should().NotBeNull();
        var tag2Id = tag2Json["data"]?["tagId"]!.Value<long>();
        _output.WriteLine($"Step3. 添加标签2成功，ID: {tag2Id}");

        // Step4. 批量获取标签
        var getResponse = await _tag.GetAsync($"/api/tags?ids={tag1Id},{tag2Id}");
        getResponse.EnsureSuccessStatusCode();
        var getJson = JObject.Parse(await getResponse.Content.ReadAsStringAsync());
        _output.WriteLine("Step4. 获取标签返回：" + getJson.ToString());

        var data = getJson["data"] as JArray;
        data.Should().NotBeNull();
        data!.Count.Should().BeGreaterThanOrEqualTo(2);

        var names = data.Select(d => d["tagName"]?.ToString()).ToList();
        names.Should().Contain(new[] { "科技", "生活" });
    }
}