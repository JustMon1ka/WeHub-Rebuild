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
    public async Task LoginAddGet_Batch()
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

        // Step2. 一次性添加多个标签
        var tagReq = new TagAddRequest { TagsName = new List<string> { "科技", "生活" } };
        var addResponse = await _tag.PostAsJsonAsync("/api/tags/add", tagReq);
        addResponse.EnsureSuccessStatusCode();
        var addJson = JObject.Parse(await addResponse.Content.ReadAsStringAsync());
        _output.WriteLine("Step2. 批量添加标签返回：" + addJson.ToString());

        var addData = addJson["data"] as JArray;
        addData.Should().NotBeNull();
        addData!.Count.Should().BeGreaterThanOrEqualTo(2);

        var tagIds = addData.Select(d => d["tagId"]!.Value<long>()).ToList();
        _output.WriteLine($"Step2. 标签ID列表：{string.Join(", ", tagIds)}");

        // Step3. 批量获取标签
        var idsParam = string.Join(",", tagIds);
        var getResponse = await _tag.GetAsync($"/api/tags?ids={idsParam}");
        getResponse.EnsureSuccessStatusCode();
        var getJson = JObject.Parse(await getResponse.Content.ReadAsStringAsync());
        _output.WriteLine("Step3. 获取标签返回：" + getJson.ToString());

        var data = getJson["data"] as JArray;
        data.Should().NotBeNull();
        data!.Count.Should().BeGreaterThanOrEqualTo(2);

        var names = data.Select(d => d["tagName"]?.ToString()).ToList();
        names.Should().Contain(new[] { "科技", "生活" });
    }
}