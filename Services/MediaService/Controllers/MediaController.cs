using DTOs;
using MediaService.DTOs;
using MediaService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaService.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IFileBrowserClient _fileBrowserClient;

    public MediaController(IMediaService mediaService, IFileBrowserClient fileBrowserClient)
    {
        _mediaService = mediaService;
        _fileBrowserClient = fileBrowserClient;
    }

    /// <summary>
    /// 4.2.2 POST /api/media/upload
    /// 上传图片/视频，返回 file_id
    /// </summary>
    [HttpPost("upload")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<BaseHttpResponse<MediaUploadResponse>> UploadMedia([FromForm] MediaUploadRequest request)
    {
        try
        {
            var result = await _mediaService.UploadAsync(request.File);
            return BaseHttpResponse<MediaUploadResponse>.Success(new MediaUploadResponse
            {
                FileId = result.FileId
            });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BaseHttpResponse<MediaUploadResponse>.Fail(400, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BaseHttpResponse<MediaUploadResponse>.Fail(400, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BaseHttpResponse<MediaUploadResponse>.Fail(500, ex.Message);
        }
        catch (Exception ex)
        {
            return BaseHttpResponse<MediaUploadResponse>.Fail(500, "服务器异常: " + ex.Message);
        }
    }
    
    /// <summary>
    /// 4.2.3 GET /api/media/{file_id}
    /// 根据 file_id 获取媒体资源
    /// </summary>
    [HttpGet("{file_id}")]
    public async Task<ActionResult<BaseHttpResponse<string>>> GetMedia([FromRoute] string file_id)
    {
        try
        {
            var mediaInfo = await _mediaService.GetMediaInfoAsync(file_id);
            if (mediaInfo == null)
                return NotFound(BaseHttpResponse<object?>.Fail(404, "找不到文件"));

            var stream = await _fileBrowserClient.DownloadFileAsync(mediaInfo.Url);
            if (stream == null)
                return NotFound(BaseHttpResponse<object?>.Fail(404, "远程资源请求失败"));

            return File(stream, mediaInfo.MediaType, fileDownloadName: file_id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse<object?>.Fail(500, "服务器异常: " + ex.Message));
        }
    }
    
}