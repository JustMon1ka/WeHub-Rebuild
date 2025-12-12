using Microsoft.Extensions.Options;
using CircleService.Services;

namespace CircleService.Services.Adapters;

// Refactored with Adapter Pattern (Additional Design Pattern)
// FileBrowserStorageAdapter适配器：将FileBrowserClient适配为IFileStorageAdapter接口
// 这样可以在不修改CircleService的情况下，轻松切换到其他存储服务（如AWS S3、Azure Blob等）
/// <summary>
/// FileBrowser存储服务的适配器实现
/// </summary>
public class FileBrowserStorageAdapter : IFileStorageAdapter
{
    private readonly IFileBrowserClient _fileBrowserClient;
    private readonly FileBrowserOptions _options;

    public FileBrowserStorageAdapter(IFileBrowserClient fileBrowserClient, IOptions<FileBrowserOptions> options)
    {
        _fileBrowserClient = fileBrowserClient;
        _options = options.Value;
    }

    public async Task<bool> UploadFileAsync(string storedName, string contentType, Stream stream)
    {
        try
        {
            var response = await _fileBrowserClient.UploadFileAsync(storedName, contentType, stream);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public string GetFileUrl(string storedName)
    {
        // 生成带时间戳的URL，用于缓存破坏
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return $"{_options.BaseUrl}/api/preview/big/uploads/{storedName}?inline=true&key={timestamp}";
    }

    public async Task<Stream?> DownloadFileAsync(string storedName)
    {
        var url = GetFileUrl(storedName);
        return await _fileBrowserClient.DownloadFileAsync(url);
    }
}

