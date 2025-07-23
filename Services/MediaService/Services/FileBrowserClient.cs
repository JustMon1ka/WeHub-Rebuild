using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MediaService.Services
{
    public class FileBrowserOptions
    {
        public string BaseUrl { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
    
    public interface IFileBrowserClient
    {
        Task<HttpResponseMessage> UploadFileAsync(string storedName, string contentType, Stream stream);
        Task<Stream?> DownloadFileAsync(string url);
    }

    public class FileBrowserClient : IFileBrowserClient
    {
        private readonly HttpClient _client;
        private readonly FileBrowserOptions _options;
        private string? _authToken;

        public FileBrowserClient(IOptions<FileBrowserOptions> options)
        {
            _options = options.Value;
            _client = new HttpClient();
        }

        private async Task<bool> LoginAsync()
        {
            var loginUrl = $"{_options.BaseUrl}/api/login";
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                username = _options.Username,
                password = _options.Password
            }), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(loginUrl, content);
            if (!response.IsSuccessStatusCode) return false;

            var newToken = await response.Content.ReadAsStringAsync();
            _authToken = newToken;

            return true;
        }

        private async Task<HttpResponseMessage> SendWithAutoLoginAsync(Func<HttpRequestMessage> requestFactory)
        {
            async Task<HttpResponseMessage> SendAsync()
            {
                var request = requestFactory();
                if (_authToken != null)
                {
                    // 1. x-auth 头
                    request.Headers.Add("x-auth", _authToken);

                    // 2. Cookie 里也带上 auth=<token>
                    request.Headers.Add("Cookie", $"auth={_authToken}");
                }
                return await _client.SendAsync(request);
            }

            var response = await SendAsync();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                if (await LoginAsync())
                    response = await SendAsync();
            }

            return response;
        }

        public async Task<HttpResponseMessage> UploadFileAsync(string storedName, string contentType, Stream stream)
        {
            // 先把流读成字节数组，方便多次重建
            var payload = await ReadAllBytesAsync(stream);

            // 调用通用的发送方法，每次都会用新的请求
            return await SendWithAutoLoginAsync(() => {
                var uploadUrl = $"{_options.BaseUrl}/api/resources/uploads/{storedName}?override=true&mkdir=true";
                var req = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = new ByteArrayContent(payload)
                };
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                return req;
            });
        }
        
        private static async Task<byte[]> ReadAllBytesAsync(Stream input)
        {
            if (input.CanSeek) input.Position = 0;
            using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }
        
        public async Task<Stream?> DownloadFileAsync(string url)
        {
            var response = await SendWithAutoLoginAsync(() =>
            {
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                return req;
            });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStreamAsync();
        }
    }
}