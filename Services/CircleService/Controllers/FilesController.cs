using System.Net.Mime;
using System.Web;
using CircleService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CircleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/webp", "image/avif", "image/svg+xml"
        };

        private readonly IFileBrowserClient _fileBrowserClient;
        private readonly FileBrowserOptions _options;

        public FilesController(IFileBrowserClient fileBrowserClient, IOptions<FileBrowserOptions> options)
        {
            _fileBrowserClient = fileBrowserClient;
            _options = options.Value;
        }

        /// <summary>
        /// 代理拉取文件服务器上的图片，解决浏览器直接访问401问题。
        /// 示例：/api/files/proxy?u={urlencodedOriginalUrl}
        /// </summary>
        [HttpGet("proxy")]
        public async Task<IActionResult> Proxy([FromQuery(Name = "u")] string? encodedUrl)
        {
            if (string.IsNullOrWhiteSpace(encodedUrl))
            {
                return BadRequest("missing parameter: u");
            }

            // 解码原始URL
            var originalUrl = HttpUtility.UrlDecode(encodedUrl);
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                return BadRequest("invalid parameter: u");
            }

            // 白名单校验：必须以 FileBrowser.BaseUrl 开头，且路径限制在 /api/preview/big/uploads/
            if (!originalUrl.StartsWith(_options.BaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("url not allowed");
            }

            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
            {
                return BadRequest("invalid url");
            }

            if (!uri.AbsolutePath.StartsWith("/api/preview/big/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("path not allowed");
            }

            // 调用文件客户端下载
            var stream = await _fileBrowserClient.DownloadFileAsync(originalUrl);
            if (stream == null)
            {
                return NotFound();
            }

            // 根据扩展名或默认类型设置 Content-Type
            var contentType = GetContentTypeByExtension(originalUrl) ?? MediaTypeNames.Application.Octet;

            // 建议缓存：可根据需要调整，这里给出较保守的策略
            Response.Headers["Cache-Control"] = "public, max-age=86400"; // 1天

            return File(stream, contentType);
        }

        private static string? GetContentTypeByExtension(string url)
        {
            var ext = Path.GetExtension(new Uri(url).AbsolutePath).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".avif" => "image/avif",
                ".svg" => "image/svg+xml",
                _ => null
            };
        }
    }
}


