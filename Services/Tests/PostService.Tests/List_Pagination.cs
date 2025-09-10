using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using PostService.DTOs;
using UserAuthService.DTOs;
using Xunit;

namespace PostService.Tests;

public class List_Pagination
{
    private readonly HttpClient _auth;
    private readonly HttpClient _post;

    public List_Pagination()
    {
        _auth = new WebApplicationFactory<UserAuthService.Program>().CreateClient();
        _post = new WebApplicationFactory<Program>().CreateClient();
    }

    [Fact(DisplayName = "帖子列表-滚动加载(lastId,num)")]
    public async Task List_Scroll()
    {
        // 1. 登录获取 token
        var loginReq = new LoginRequest { Identifier = "testuser", Password = "TestPass123" };
        var loginResp = await _auth.PostAsJsonAsync("/api/auth/login", loginReq);
        loginResp.EnsureSuccessStatusCode();
        var token = JObject.Parse(await loginResp.Content.ReadAsStringAsync())["token"]?.ToString();
        token.Should().NotBeNullOrEmpty();
        _post.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. 先发 5 篇（标题不同，便于断言）
        for (int i = 0; i < 5; i++)
        {
            var pubReq = new PostPublishRequest
            {
                Title = $"标题_{i}",
                Content = $"内容_{i}",
                CircleId = 1,
                Tags = new List<long>() // 可留空或填已有标签
            };
            var resp = await _post.PostAsJsonAsync("/api/posts/publish", pubReq);
            resp.EnsureSuccessStatusCode();
        }

        // 3. 第一次列表(不传 lastId)，取 3 篇
        var list1 = await _post.GetAsync("/api/posts/list?num=3");
        list1.EnsureSuccessStatusCode();
        var json1 = JObject.Parse(await list1.Content.ReadAsStringAsync());
        json1["code"]?.Value<int>().Should().Be(200);
        var arr1 = (JArray?)json1["data"];
        arr1.Should().NotBeNull();
        arr1!.Count.Should().Be(3);

        long lastId = arr1.Last()?["postId"]?.Value<long>() ?? 0;
        lastId.Should().BeGreaterThan(0);

        // 4. 第二次列表(传 lastId)，继续取后续 3 篇（实际上剩 2 篇）
        var list2 = await _post.GetAsync($"/api/posts/list?lastId={lastId}&num=3");
        list2.EnsureSuccessStatusCode();
        var json2 = JObject.Parse(await list2.Content.ReadAsStringAsync());
        json2["code"]?.Value<int>().Should().Be(200);
        var arr2 = (JArray?)json2["data"];
        arr2.Should().NotBeNull();
        arr2!.Count.Should().BeGreaterThan(0);

        // 5. 两页不应有重复
        var ids1 = arr1.Select(x => x?["postId"]?.Value<long>() ?? 0).ToHashSet();
        var ids2 = arr2.Select(x => x?["postId"]?.Value<long>() ?? 0).ToHashSet();
        ids1.Intersect(ids2).Count().Should().Be(0);
    }
}
