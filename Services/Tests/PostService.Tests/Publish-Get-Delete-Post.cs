using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostService.DTOs;
using UserAuthService.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace PostService.Tests;

public class Publish_Get_Delete_Post
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _authClient;
    private readonly HttpClient _dataClient;
    private readonly HttpClient _postClient;

    public Publish_Get_Delete_Post(ITestOutputHelper output)
    {
        _output = output;

        // 启动 UserAuthService
        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _authClient = authFactory.CreateClient();
        
        // 启动 UserDataService
        var dataFactory = new WebApplicationFactory<UserDataService.Program>();
        _dataClient = dataFactory.CreateClient();

        // 启动 PostService
        var postFactory = new WebApplicationFactory<PostService.Program>();
        _postClient = postFactory.CreateClient();
    }

    [Fact(DisplayName = "注册 - 登录 - 发帖 - 获取帖子 - 非发布者删除帖子 - 发布者删除帖子 - 注销账号")]
    public async Task Publish_Get_Delete()
    {
        RegisterRequest sender = new RegisterRequest{
            Username = "testuser_sender",
            Password = "TestPass123!",
            Email = "testuser_sender@example.com",
            Phone = "1234567899"
        };

        RegisterRequest other = new RegisterRequest
        {
            Username = "testuser_other",
            Password = "TestPass1234",
            Email = "testuser_other@example.com",
            Phone = "1234567890"
        };
        
        // Step1. 注册两个用户
        var registerSenderResponse = await _authClient.PostAsJsonAsync("/api/auth/register", sender);
        registerSenderResponse.EnsureSuccessStatusCode();
        var registerOtherResponse = await _authClient.PostAsJsonAsync("/api/auth/register", other);
        registerOtherResponse.EnsureSuccessStatusCode();
        _output.WriteLine("Step1. 注册成功");
        
        // Step2. 登录两个用户
        LoginRequest s = new LoginRequest
        {
            Identifier = sender.Username,
            Password = sender.Password
        };

        LoginRequest o = new LoginRequest
        {
            Identifier = other.Username,
            Password = other.Password
        };

        var loginSenderResponse = await _authClient.PostAsJsonAsync("/api/auth/login", s);
        loginSenderResponse.EnsureSuccessStatusCode();
        var senderContent = await loginSenderResponse.Content.ReadAsStringAsync();
        var senderJson = JObject.Parse(senderContent);
        var senderToken = senderJson["token"]?.ToString();
        senderToken.Should().NotBeNullOrEmpty("发送者登录失败");
        
        var loginOtherResponse = await _authClient.PostAsJsonAsync("/api/auth/login", o);
        loginOtherResponse.EnsureSuccessStatusCode();
        var otherContent = await loginOtherResponse.Content.ReadAsStringAsync();
        var otherJson = JObject.Parse(otherContent);
        var otherToken = otherJson["token"]?.ToString();
        otherToken.Should().NotBeNullOrEmpty("其他人登录失败");
        
        _output.WriteLine("Step2. 登陆成功");
        
        // Step3. sender发帖
        PostPublishRequest post = new PostPublishRequest
        {
            Content = "今天是上分的好日子"
        };
        _postClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderToken);
        var publishResponse = await _postClient.PostAsJsonAsync("/api/posts/publish", post);
        publishResponse.EnsureSuccessStatusCode();
        var publishContent = await publishResponse.Content.ReadAsStringAsync();
        var publishJson = JObject.Parse(publishContent);
        var postPublishResult = publishJson["data"]?.ToObject<PostPublishResponse>();
        postPublishResult.Should().NotBeNull();
        _output.WriteLine($"{postPublishResult}");
        _output.WriteLine($"Step3. 发帖成功: \nPostId: {postPublishResult!.PostId}, CreatedAt: {postPublishResult.CreatedAt}");
        
        // Step4. 通过GET /api/posts尝试获取帖子内容
        _postClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer");
        var getPostByArrayResponse = await _postClient.GetAsync($"api/posts?ids={postPublishResult!.PostId}");
        getPostByArrayResponse.EnsureSuccessStatusCode();
        var getPostByArrayContent = await getPostByArrayResponse.Content.ReadAsStringAsync();
        var getPostByArrayJson = JObject.Parse(getPostByArrayContent);
        var dataArray = getPostByArrayJson["data"]?.ToString();
        dataArray.Should().NotBeNull("响应中应包含 data 字段");
        var postList = JsonConvert.DeserializeObject<List<PostResponse>>(dataArray!);
        postList.Should().NotBeNull();
        postList.Should().HaveCount(1);
        postList[0].PostId.Should().Be(postPublishResult.PostId);
        string postJson = JsonConvert.SerializeObject(postList[0], Formatting.Indented);
        _output.WriteLine("Step4. 获取到的帖子对象内容如下：");
        _output.WriteLine(postJson);
        
        // Step5. 通过GET /api/posts/<post_id>尝试获取帖子内容
        var getPostByIdResponse = await _postClient.GetAsync($"api/posts/{postPublishResult!.PostId}");
        getPostByIdResponse.EnsureSuccessStatusCode();
        var getPostByIdContent = await getPostByIdResponse.Content.ReadAsStringAsync();
        var getPostByIdJson = JObject.Parse(getPostByIdContent);
        var getPostByIdResult = getPostByIdJson["data"]?.ToObject<PostResponse>();
        getPostByIdResult.Should().NotBeNull();
        _output.WriteLine("Step5. 获取帖子对象内容如下：");
        _output.WriteLine(JsonConvert.SerializeObject(getPostByIdResult, Formatting.Indented));
        
        // Step6. other尝试删除帖子
        _postClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var otherDeleteResponse = await _postClient.DeleteAsync($"api/posts?post_id={postPublishResult!.PostId}");
        otherDeleteResponse.EnsureSuccessStatusCode();
        var otherDeleteContent = await otherDeleteResponse.Content.ReadAsStringAsync();
        var otherDeleteJson = JObject.Parse(otherDeleteContent);
        _output.WriteLine($"Step6. 删除结果: {otherDeleteJson.ToString()}");
        ((int?)otherDeleteJson["code"]).Should().Be(403);
        ((string?)otherDeleteJson["msg"]).Should().Contain("无权限");
        
        // Step7. sender尝试删除帖子
        _postClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderToken);
        var senderDeleteResponse = await _postClient.DeleteAsync($"api/posts?post_id={postPublishResult!.PostId}");
        senderDeleteResponse.EnsureSuccessStatusCode();
        var senderDeleteContent = await senderDeleteResponse.Content.ReadAsStringAsync();
        var senderDeleteJson = JObject.Parse(senderDeleteContent);
        _output.WriteLine($"Step7. 删除结果: {senderDeleteJson.ToString()}");
        ((int?)senderDeleteJson["code"]).Should().Be(200);
        
        // Step8. 注销两个账号
        _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderToken);
        var deleteSenderResponse = await _dataClient.DeleteAsync("/api/user/delete");
        deleteSenderResponse.EnsureSuccessStatusCode();
        _dataClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var deleteOtherResponse = await _dataClient.DeleteAsync("api/user/delete");
        deleteOtherResponse.EnsureSuccessStatusCode();
        _output.WriteLine("Step8. 注销成功");
    }
}