using CircleService.DTOs;
using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 圈子相关的API控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CirclesController : ControllerBase
{
    private readonly ICircleService _circleService;
    private readonly ICircleMemberService _memberService;
    private readonly IActivityService _activityService;

    public CirclesController(
        ICircleService circleService, 
        ICircleMemberService memberService,
        IActivityService activityService)
    {
        _circleService = circleService;
        _memberService = memberService;
        _activityService = activityService;
    }

    // --- Circle Management Endpoints ---

    /// <summary>
    /// 获取所有圈子，支持按名称搜索、分类筛选，或查询某个用户加入的圈子
    /// </summary>
    /// <param name="name">可选的搜索名称</param>
    /// <param name="category">可选的分类标签，用于筛选圈子</param>
    /// <param name="joinedBy">可选的用户ID，用于查询该用户已加入的圈子</param>
    [HttpGet]
    public async Task<IActionResult> GetAllCircles([FromQuery] string? name = null, [FromQuery] string? category = null, [FromQuery] int? joinedBy = null)
    {
        var circles = await _circleService.GetAllCirclesAsync(name, category, joinedBy);
        return Ok(BaseHttpResponse<object>.Success(circles));
    }

    /// <summary>
    /// 获取所有圈子分类标签列表，用于分类筛选下拉菜单
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _circleService.GetAllCategoriesAsync();
        return Ok(BaseHttpResponse<object>.Success(categories));
    }

    /// <summary>
    /// 根据ID获取圈子详情
    /// </summary>
    /// <param name="id">圈子ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCircleById(int id)
    {
        var circle = await _circleService.GetCircleByIdAsync(id);
        if (circle == null)
        {
            return NotFound(BaseHttpResponse.Fail(404, "圈子不存在"));
        }
        
        // 获取圈子成员信息
        var members = await _memberService.GetCircleMembersAsync(id);
        
        // 构建包含圈子信息和成员列表的响应数据
        var response = new 
        {
            Circle = circle,
            Members = members
        };
        
        return Ok(BaseHttpResponse<object>.Success(response));
    }

    /// <summary>
    /// 创建一个新圈子
    /// </summary>
    /// <param name="createCircleDto">创建圈子所需的数据</param>
    [HttpPost]
    public async Task<IActionResult> CreateCircle([FromBody] CreateCircleDto createCircleDto)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var ownerId = 1; // TODO: 实际应从用户认证信息中获取

        var newCircle = await _circleService.CreateCircleAsync(createCircleDto, ownerId);
        return CreatedAtAction(nameof(GetCircleById), new { id = newCircle.CircleId }, BaseHttpResponse<object>.Success(newCircle, "圈子创建成功"));
    }

    /// <summary>
    /// 更新一个圈子
    /// </summary>
    /// <param name="id">要更新的圈子ID</param>
    /// <param name="updateCircleDto">更新所需的数据</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCircle(int id, [FromBody] UpdateCircleDto updateCircleDto)
    {
        var result = await _circleService.UpdateCircleAsync(id, updateCircleDto);
        if (!result)
        {
            return NotFound(BaseHttpResponse.Fail(404, "圈子不存在或更新失败"));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "圈子更新成功"));
    }

    /// <summary>
    /// 删除一个圈子
    /// </summary>
    /// <param name="id">要删除的圈子ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCircle(int id)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var deleterId = 1; // TODO: 实际应从用户认证信息中获取
        
        var result = await _circleService.DeleteCircleAsync(id, deleterId);
        if (!result)
        {
            return NotFound(BaseHttpResponse.Fail(404, "圈子不存在或无权删除"));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "圈子删除成功"));
    }

    // --- Circle Member Management Endpoints ---

    /// <summary>
    /// 用户主动退出一个圈子
    /// </summary>
    /// <param name="id">要退出的圈子ID</param>
    [HttpDelete("{id}/membership")]
    public async Task<IActionResult> LeaveCircle(int id)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var userId = 2; // TODO: 实际应从用户认证信息中获取

        var result = await _memberService.LeaveCircleAsync(id, userId);

        if (!result.Success)
        {
            // 根据不同的失败原因返回不同的状态码可能会更友好
            // 但为保持简单，这里统一返回400 Bad Request
            return BadRequest(BaseHttpResponse.Fail(400, result.ErrorMessage));
        }

        return Ok(BaseHttpResponse<object>.Success(null, "成功退出圈子"));
    }

    /// <summary>
    /// 申请加入一个圈子
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpPost("{circleId}/join")]
    public async Task<IActionResult> ApplyToJoin(int circleId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var userId = 2; // TODO: 实际应从用户认证信息中获取

        var response = await _memberService.ApplyToJoinCircleAsync(circleId, userId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "入圈申请已提交，等待审核。"));
    }

    /// <summary>
    /// 获取一个圈子的所有成员
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpGet("{circleId}/members")]
    public async Task<IActionResult> GetMembers(int circleId)
    {
        var members = await _memberService.GetCircleMembersAsync(circleId);
        return Ok(BaseHttpResponse<object>.Success(members));
    }
    
    /// <summary>
    /// (圈主)审批用户的入圈申请
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="targetUserId">被审批用户的ID</param>
    /// <param name="approve">是否通过申请</param>
    [HttpPut("{circleId}/applications/{targetUserId}")]
    public async Task<IActionResult> ApproveApplication(int circleId, int targetUserId, [FromQuery] bool approve)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var approverId = 1; // TODO: 实际应从用户认证信息中获取

        var response = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId, approve);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "审批操作成功。"));
    }

    /// <summary>
    /// (圈主)将成员移出圈子
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="targetUserId">被移除用户的ID</param>
    [HttpDelete("{circleId}/members/{targetUserId}")]
    public async Task<IActionResult> RemoveMember(int circleId, int targetUserId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var removerId = 1; // TODO: 实际应从用户认证信息中获取
        
        var response = await _memberService.RemoveMemberAsync(circleId, targetUserId, removerId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "成员移除成功。"));
    }

    // --- Points and Leaderboard Endpoints ---

    /// <summary>
    /// 获取圈子积分排行榜
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpGet("{circleId}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(int circleId)
    {
        var leaderboard = await _memberService.GetLeaderboardAsync(circleId);
        return Ok(BaseHttpResponse<object>.Success(leaderboard));
    }

    /// <summary>
    /// 获取当前用户在圈子内的积分详情
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpGet("{circleId}/points")]
    public async Task<IActionResult> GetMyPoints(int circleId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var userId = 2; // TODO: 实际应从用户认证信息中获取

        var memberDetails = await _memberService.GetMemberDetailsAsync(circleId, userId);
        if (memberDetails == null)
        {
            return NotFound(BaseHttpResponse.Fail(404, "您不是该圈子成员或查询出错。"));
        }
        return Ok(BaseHttpResponse<object>.Success(memberDetails));
    }

    // --- Activity Management Endpoints ---

    /// <summary>
    /// 获取一个圈子的所有活动
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpGet("{circleId}/activities")]
    public async Task<IActionResult> GetActivities(int circleId)
    {
        var activities = await _activityService.GetActivitiesByCircleIdAsync(circleId);
        return Ok(BaseHttpResponse<object>.Success(activities));
    }

    /// <summary>
    /// 在一个圈子中创建新活动
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="createActivityDto">创建活动所需的数据</param>
    [HttpPost("{circleId}/activities")]
    public async Task<IActionResult> CreateActivity(int circleId, [FromBody] CreateActivityDto createActivityDto)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替，后续衔接时需要更改
        var creatorId = 1; // TODO: 实际应从用户认证信息中获取

        var response = await _activityService.CreateActivityAsync(circleId, createActivityDto, creatorId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(response.Data, "活动创建成功。"));
    }

    /// <summary>
    /// 更新一个活动
    /// </summary>
    /// <param name="circleId">圈子ID (用于路由，但业务逻辑不一定需要)</param>
    /// <param name="activityId">活动ID</param>
    /// <param name="updateActivityDto">更新活动所需的数据</param>
    [HttpPut("{circleId}/activities/{activityId}")]
    public async Task<IActionResult> UpdateActivity(int circleId, int activityId, [FromBody] UpdateActivityDto updateActivityDto)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var modifierId = 1; // TODO: 实际应从用户认证信息中获取

        var response = await _activityService.UpdateActivityAsync(activityId, updateActivityDto, modifierId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "活动更新成功。"));
    }

    /// <summary>
    /// 删除一个活动
    /// </summary>
    /// <param name="circleId">圈子ID (用于路由)</param>
    /// <param name="activityId">活动ID</param>
    [HttpDelete("{circleId}/activities/{activityId}")]
    public async Task<IActionResult> DeleteActivity(int circleId, int activityId)
    {
        // 假设从JWT Token中获取当前用户ID，这里暂时用一个硬编码代替
        var deleterId = 1; // TODO: 实际应从用户认证信息中获取
        
        var response = await _activityService.DeleteActivityAsync(activityId, deleterId);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "活动删除成功。"));
    }

    // --- 图片上传相关接口 ---

    /// <summary>
    /// 上传圈子头像
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="file">图片文件</param>
    [HttpPost("{circleId}/avatar")]
    public async Task<IActionResult> UploadAvatar(int circleId, IFormFile file)
    {
        // 临时调试：检查圈子是否存在
        var circle = await _circleService.GetCircleByIdAsync(circleId);
        if (circle == null)
        {
            return NotFound(BaseHttpResponse.Fail(404, $"圈子ID {circleId} 不存在"));
        }
        
        Console.WriteLine($"DEBUG: 找到圈子 {circleId}, 名称: {circle.Name}");
        
        if (file == null || file.Length == 0)
        {
            return BadRequest(BaseHttpResponse.Fail(400, "请选择要上传的图片文件"));
        }

        // 验证文件类型
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(BaseHttpResponse.Fail(400, "只支持 JPG、PNG、GIF 格式的图片"));
        }

        // 验证文件大小（限制为5MB）
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(BaseHttpResponse.Fail(400, "图片文件大小不能超过5MB"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _circleService.UploadAvatarAsync(circleId, stream, file.FileName, file.ContentType);
            
            if (result == null)
            {
                return NotFound(BaseHttpResponse.Fail(404, $"圈子ID {circleId} 不存在或FileBrowser上传失败。请检查：1.圈子是否存在 2.FileBrowser服务是否正常 3.网络连接是否正常"));
            }

            return Ok(BaseHttpResponse<object>.Success(result, "头像上传成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"上传失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 上传圈子背景图
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="file">图片文件</param>
    [HttpPost("{circleId}/banner")]
    public async Task<IActionResult> UploadBanner(int circleId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(BaseHttpResponse.Fail(400, "请选择要上传的图片文件"));
        }

        // 验证文件类型
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(BaseHttpResponse.Fail(400, "只支持 JPG、PNG、GIF 格式的图片"));
        }

        // 验证文件大小（限制为10MB）
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(BaseHttpResponse.Fail(400, "背景图片文件大小不能超过10MB"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _circleService.UploadBannerAsync(circleId, stream, file.FileName, file.ContentType);
            
            if (result == null)
            {
                return NotFound(BaseHttpResponse.Fail(404, $"圈子ID {circleId} 不存在或FileBrowser上传失败。请检查：1.圈子是否存在 2.FileBrowser服务是否正常 3.网络连接是否正常"));
            }

            return Ok(BaseHttpResponse<object>.Success(result, "背景图上传成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"上传失败: {ex.Message}"));
        }
    }
} 