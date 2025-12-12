namespace CircleService.Services.Adapters;

// Refactored with Adapter Pattern (Additional Design Pattern)
// 使用适配器模式重构：适配不同的文件存储服务（FileBrowser、AWS S3、Azure Blob等），
// 使CircleService可以统一使用不同的存储后端，而无需修改业务逻辑
/// <summary>
/// 文件存储适配器接口，定义统一的文件存储操作
/// </summary>
public interface IFileStorageAdapter
{
    /// <summary>
    /// 上传文件到存储服务
    /// </summary>
    /// <param name="storedName">存储的文件名（包含路径）</param>
    /// <param name="contentType">文件内容类型</param>
    /// <param name="stream">文件流</param>
    /// <returns>上传是否成功</returns>
    Task<bool> UploadFileAsync(string storedName, string contentType, Stream stream);

    /// <summary>
    /// 获取文件的访问URL
    /// </summary>
    /// <param name="storedName">存储的文件名</param>
    /// <returns>文件的访问URL</returns>
    string GetFileUrl(string storedName);

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="storedName">存储的文件名</param>
    /// <returns>文件流，如果文件不存在则返回null</returns>
    Task<Stream?> DownloadFileAsync(string storedName);
}

