using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using UserAuthService.DTOs;
using Xunit;
using Xunit.Abstractions;

namespace MediaService.Test;

public class Upload_Download
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _auth;
    private readonly HttpClient _media;

    public Upload_Download(ITestOutputHelper output)
    {
        _output = output;
        
        var authFactory = new WebApplicationFactory<UserAuthService.Program>();
        _auth = authFactory.CreateClient();

        var mediaFactory = new WebApplicationFactory<MediaService.Program>();
        _media = mediaFactory.CreateClient();
    }

    [Fact(DisplayName = "登录 - 上传文件 - 获取文件")]
    public async Task Upload_File_Should_Exist()
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

        // Step2 上传文件
        _media.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var imagePath = Path.Combine("Resources", "test_image.jpeg");
        _output.WriteLine($"读取测试图片路径: {imagePath}");
        using var imageContent = new ByteArrayContent(await File.ReadAllBytesAsync(imagePath));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var formData = new MultipartFormDataContent
        {
            { imageContent, "file", "test_image.jpeg" }
        };

        var uploadResponse = await _media.PostAsync("/api/media/upload", formData);
        uploadResponse.EnsureSuccessStatusCode();
        var uploadContent = await uploadResponse.Content.ReadAsStringAsync();
        var uploadJson = JObject.Parse(uploadContent);
        _output.WriteLine($"相应数据：{uploadContent}");
        uploadJson["code"]?.ToString().Should().Be("200");
        var fileId = uploadJson["data"]?["fileId"]?.ToString();
        fileId.Should().NotBeNullOrEmpty();
        _output.WriteLine($"Step2. 上传成功，fileId：{fileId}");

        // Step3. 下载文件
        var downloadResponse = await _media.GetAsync($"/api/media/{fileId}");
        downloadResponse.EnsureSuccessStatusCode();
        downloadResponse.Content.Headers.ContentType?.MediaType.Should().StartWith("image/");
        var imageBytes = await downloadResponse.Content.ReadAsByteArrayAsync();

        imageBytes.Should().NotBeNull();
        imageBytes.Length.Should().BeGreaterThan(0);
        _output.WriteLine($"Step3. 下载成功，字节数：{imageBytes.Length}");

    }
}