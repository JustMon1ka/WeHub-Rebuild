using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CircleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IFileBrowserClient _fileBrowserClient;

        public TestController(IFileBrowserClient fileBrowserClient)
        {
            _fileBrowserClient = fileBrowserClient;
        }

        /// <summary>
        /// 测试FileBrowser连接
        /// </summary>
        [HttpGet("filebrowser")]
        public async Task<IActionResult> TestFileBrowser()
        {
            try
            {
                // 创建一个简单的测试文件
                var testContent = "Hello FileBrowser Test";
                var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));
                
                var response = await _fileBrowserClient.UploadFileAsync("test.txt", "text/plain", stream);
                
                return Ok(BaseHttpResponse<object>.Success(new
                {
                    StatusCode = response.StatusCode,
                    IsSuccess = response.IsSuccessStatusCode,
                    Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value)
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseHttpResponse.Fail(500, $"FileBrowser连接测试失败: {ex.Message}"));
            }
        }
    }
}
