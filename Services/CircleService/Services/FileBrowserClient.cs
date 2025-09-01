using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CircleService.Services
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
            _client.Timeout = TimeSpan.FromSeconds(30); // 设置30秒超时
        }

        private async Task<bool> LoginAsync()
        {
            var loginUrl = $"{_options.BaseUrl}/api/login";
            
            // 处理空用户名密码的情况
            var loginData = new
            {
                username = string.IsNullOrEmpty(_options.Username) ? "" : _options.Username,
                password = string.IsNullOrEmpty(_options.Password) ? "" : _options.Password
            };
            
            Console.WriteLine($"DEBUG: 尝试登录FileBrowser, URL: {loginUrl}");
            Console.WriteLine($"DEBUG: 登录数据: {JsonSerializer.Serialize(loginData)}");
            
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync(loginUrl, content);
                Console.WriteLine($"DEBUG: 登录响应状态码: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: 登录失败, 错误内容: {errorContent}");
                    return false;
                }

                var newToken = await response.Content.ReadAsStringAsync();
                _authToken = newToken;
                Console.WriteLine($"DEBUG: 登录成功, 获取到token: {newToken.Substring(0, Math.Min(20, newToken.Length))}...");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: 登录异常: {ex.Message}");
                return false;
            }
        }

        private async Task<HttpResponseMessage> SendWithAutoLoginAsync(Func<HttpRequestMessage> requestFactory)
        {
            async Task<HttpResponseMessage> SendAsync()
            {
                var request = requestFactory();
                Console.WriteLine($"DEBUG: 请求URL: {request.RequestUri}");
                Console.WriteLine($"DEBUG: 请求方法: {request.Method}");
                
                if (_authToken != null)
                {
                    // 1. x-auth 头
                    request.Headers.Add("x-auth", _authToken);
                    Console.WriteLine($"DEBUG: 添加x-auth头: {_authToken.Substring(0, Math.Min(10, _authToken.Length))}...");

                    // 2. Cookie 里也带上 auth=<token>
                    request.Headers.Add("Cookie", $"auth={_authToken}");
                    Console.WriteLine($"DEBUG: 添加Cookie认证");
                }
                else
                {
                    Console.WriteLine($"DEBUG: 没有认证token，使用匿名请求");
                }
                
                Console.WriteLine($"DEBUG: 开始发送请求...");
                var response = await _client.SendAsync(request);
                Console.WriteLine($"DEBUG: 请求完成，状态码: {response.StatusCode}");
                return response;
            }

            var response = await SendAsync();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"DEBUG: 认证失败，尝试重新登录...");
                response.Dispose();
                if (await LoginAsync())
                {
                    Console.WriteLine($"DEBUG: 重新登录成功，重试请求...");
                    response = await SendAsync();
                }
                else
                {
                    Console.WriteLine($"DEBUG: 重新登录失败");
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> UploadFileAsync(string storedName, string contentType, Stream stream)
        {
            Console.WriteLine($"DEBUG: 开始上传文件: {storedName}");
            
            // 先尝试登录
            Console.WriteLine($"DEBUG: 检查是否需要登录...");
            if (_authToken == null)
            {
                Console.WriteLine($"DEBUG: 当前没有token，尝试登录...");
                var loginSuccess = await LoginAsync();
                Console.WriteLine($"DEBUG: 登录结果: {(loginSuccess ? "成功" : "失败")}");
            }
            else
            {
                Console.WriteLine($"DEBUG: 已有token: {_authToken.Substring(0, Math.Min(10, _authToken.Length))}...");
            }
            
            // 先把流读成字节数组，方便多次重建
            var payload = await ReadAllBytesAsync(stream);
            Console.WriteLine($"DEBUG: 文件大小: {payload.Length} 字节");

            // 调用通用的发送方法，每次都会用新的请求
            try
            {
                Console.WriteLine($"DEBUG: 准备发送上传请求...");
                var response = await SendWithAutoLoginAsync(() => {
                    var uploadUrl = $"{_options.BaseUrl}/api/resources/uploads/{storedName}?override=true&mkdir=true";
                    Console.WriteLine($"DEBUG: 上传URL: {uploadUrl}");
                    
                    var req = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                    {
                        Content = new ByteArrayContent(payload)
                    };
                    req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    Console.WriteLine($"DEBUG: 请求已创建，准备发送...");
                    return req;
                });
                
                Console.WriteLine($"DEBUG: 上传响应状态码: {response.StatusCode}");
                return response;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"DEBUG: 上传超时: {ex.Message}");
                throw;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"DEBUG: 网络请求异常: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: 上传异常: {ex.Message}");
                throw;
            }
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

        public string GetAuthenticatedImageUrl(string storedName)
        {
            // 生成带时间戳的URL，用于缓存破坏
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return $"{_options.BaseUrl}/api/resources/uploads/{storedName}?t={timestamp}";
        }
    }
}
