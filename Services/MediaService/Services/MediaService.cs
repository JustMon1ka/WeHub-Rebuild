using MediaService.DTOs;
using MediaService.Models;
using MediaService.Repositories;

namespace MediaService.Services
{
    public interface IMediaService
    {
        Task<MediaUploadResponse> UploadAsync(IFormFile file);
        Task<MediaInfo?> GetMediaInfoAsync(string mediaId);
    }
    
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _repo;
        private readonly string _fbBase;
        private readonly string _fbUploadPath;
        private readonly IFileBrowserClient _fileBrowser;

        public MediaService(IMediaRepository repo, IConfiguration cfg, IFileBrowserClient fileBrowser)
        {
            _repo           = repo;
            _fileBrowser = fileBrowser;
            _fbBase         = cfg["FileBrowser:BaseUrl"]!;
            _fbUploadPath   = cfg["FileBrowser:UploadPath"]!;
        }

        public async Task<MediaUploadResponse> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("file is required");

            // 文件大小/格式校验
            const long MaxBytes = 50L * 1024 * 1024;
            if (file.Length > MaxBytes)
                throw new ArgumentOutOfRangeException("file too large");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".avi" };
            if (Array.IndexOf(allowed, ext) < 0)
                throw new ArgumentException("invalid format");

            // 生成 ID、拼文件名/类型/URL
            var mediaId    = Guid.NewGuid().ToString("N");
            var storedName = $"{mediaId}{ext}";
            var mediaType  = DetermineType(file.ContentType);
            var mediaUrl   = $"{_fbBase}/api/raw/{_fbUploadPath}/{storedName}";

            // 上传到 FileBrowser
            await using var stream = file.OpenReadStream();
            var ok = await _fileBrowser.UploadFileAsync(storedName, mediaType, stream);
            if (!ok.IsSuccessStatusCode) throw new InvalidOperationException($"upload to FileBrowser failed: {ok.ReasonPhrase}");

            // 入库
            var entity = new Media
            {
                MediaId   = mediaId,
                MediaType = mediaType,
                MediaUrl  = mediaUrl
            };
            await _repo.InsertMediaAsync(entity);

            return new MediaUploadResponse { FileId = mediaId };
        }

        public async Task<MediaInfo?> GetMediaInfoAsync(string mediaId)
        {
            var media = await _repo.GetByIdAsync(mediaId);
            if (media == null) return null;

            // 映射为标准 MIME 类型
            var mediaType = media.MediaType.ToLower() switch
            {
                "image" => "image/jpeg",
                "video" => "video/mp4",
                "audio" => "audio/mpeg",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            return new MediaInfo
            {
                Url = media.MediaUrl,
                MediaType = mediaType
            };
        }

        private static string DetermineType(string contentType)
        {
            if (contentType.StartsWith("image/")) return "image";
            if (contentType.StartsWith("video/")) return "video";
            if (contentType.StartsWith("audio/")) return "audio";
            return "other";
        }
    }
}
