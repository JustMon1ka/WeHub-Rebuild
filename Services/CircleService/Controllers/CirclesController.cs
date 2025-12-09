using CircleService.DTOs;
using CircleService.Models;
using CircleService.Services;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CircleService.Controllers;

/// <summary>
/// 圈子相关的API控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 默认需要认证
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

    /// <summary>
    /// 从JWT Claims中获取当前用户ID
    /// </summary>
    /// <returns>用户ID，如果获取失败则返回null</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    // --- Circle Management Endpoints ---

    /// <summary>
    /// 获取所有圈子，支持按名称搜索、分类筛选，或查询某个用户加入的圈子
    /// </summary>
    /// <param name="name">可选的搜索名称</param>
    /// <param name="category">可选的分类标签，用于筛选圈子</param>
    /// <param name="joinedBy">可选的用户ID，用于查询该用户已加入的圈子</param>
    [HttpGet]
    [AllowAnonymous] // 公共接口，无需认证
    public async Task<IActionResult> GetAllCircles([FromQuery] string? name = null, [FromQuery] string? category = null, [FromQuery] int? joinedBy = null)
    {
        var circles = await _circleService.GetAllCirclesAsync(name, category, joinedBy);
        return Ok(BaseHttpResponse<object>.Success(circles));
    }

    /// <summary>
    /// 获取所有圈子分类标签列表，用于分类筛选下拉菜单
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous] // 公共接口，无需认证
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
    [AllowAnonymous] // 公共接口，无需认证
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
        var ownerId = GetCurrentUserId();
        if (ownerId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var newCircle = await _circleService.CreateCircleAsync(createCircleDto, ownerId.Value);
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
        var deleterId = GetCurrentUserId();
        if (deleterId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }
        
        var result = await _circleService.DeleteCircleAsync(id, deleterId.Value);
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
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var result = await _memberService.LeaveCircleAsync(id, userId.Value);

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
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var response = await _memberService.ApplyToJoinCircleAsync(circleId, userId.Value);
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
    /// <param name="sortByPoints">是否按积分排序 (0=不排序, 1=按积分降序排序)</param>
    [HttpGet("{circleId}/members")]
    [AllowAnonymous] // 公共接口，无需认证
    public async Task<IActionResult> GetMembers(int circleId, [FromQuery] int sortByPoints = 0)
    {
        var members = await _memberService.GetCircleMembersAsync(circleId, sortByPoints == 1);
        return Ok(BaseHttpResponse<object>.Success(members));
    }
    
    /// <summary>
    /// (圈主)审批用户的入圈申请
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="targetUserId">被审批用户的ID</param>
    /// <param name="approve">是否通过申请</param>
    /// <param name="role">指定用户角色（0=普通成员，1=管理员，可选）</param>
    [HttpPut("{circleId}/applications/{targetUserId}")]
    public async Task<IActionResult> ApproveApplication(int circleId, int targetUserId, [FromQuery] bool approve, [FromQuery] int? role = null)
    {
        var approverId = GetCurrentUserId();
        if (approverId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        // 转换角色参数
        CircleMemberRole? memberRole = null;
        if (role.HasValue)
        {
            memberRole = (CircleMemberRole)role.Value;
        }

        var response = await _memberService.ApproveJoinApplicationAsync(circleId, targetUserId, approverId.Value, approve, memberRole);
        if (!response.Success)
        {
            return BadRequest(BaseHttpResponse.Fail(400, response.ErrorMessage!));
        }
        return Ok(BaseHttpResponse<object>.Success(null, "审批操作成功。"));
    }

    /// <summary>
    /// 获取圈子的申请列表（待审批和已处理）
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpGet("{circleId}/applications")]
    public async Task<IActionResult> GetApplications(int circleId)
    {
        var requesterId = GetCurrentUserId();
        if (requesterId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var applications = await _memberService.GetApplicationsAsync(circleId, requesterId.Value);
        if (applications == null)
        {
            return NotFound(BaseHttpResponse.Fail(404, "圈子不存在或权限不足"));
        }

        return Ok(BaseHttpResponse<object>.Success(applications, "获取申请列表成功"));
    }

    /// <summary>
    /// (圈主)将成员移出圈子
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="targetUserId">被移除用户的ID</param>
    [HttpDelete("{circleId}/members/{targetUserId}")]
    public async Task<IActionResult> RemoveMember(int circleId, int targetUserId)
    {
        var removerId = GetCurrentUserId();
        if (removerId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }
        
        var response = await _memberService.RemoveMemberAsync(circleId, targetUserId, removerId.Value);
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
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var memberDetails = await _memberService.GetMemberDetailsAsync(circleId, userId.Value);
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
    /// 获取活动详情
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="activityId">活动ID</param>
    [HttpGet("{circleId}/activities/{activityId}")]
    public async Task<IActionResult> GetActivityById(int circleId, int activityId)
    {
        var activity = await _activityService.GetActivityByIdAsync(activityId);
        if (activity == null)
        {
            return NotFound(BaseHttpResponse.Fail(404, "活动不存在"));
        }
        
        // 验证活动是否属于指定圈子
        if (activity.CircleId != circleId)
        {
            return BadRequest(BaseHttpResponse.Fail(400, "活动不属于指定圈子"));
        }
        
        return Ok(BaseHttpResponse<object>.Success(activity));
    }

    /// <summary>
    /// 在一个圈子中创建新活动
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="createActivityDto">创建活动所需的数据</param>
    [HttpPost("{circleId}/activities")]
    public async Task<IActionResult> CreateActivity(int circleId, [FromBody] CreateActivityDto createActivityDto)
    {
        var creatorId = GetCurrentUserId();
        if (creatorId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var response = await _activityService.CreateActivityAsync(circleId, createActivityDto, creatorId.Value);
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
        var modifierId = GetCurrentUserId();
        if (modifierId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }

        var response = await _activityService.UpdateActivityAsync(activityId, updateActivityDto, modifierId.Value);
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
        var deleterId = GetCurrentUserId();
        if (deleterId == null)
        {
            return Unauthorized(BaseHttpResponse.Fail(401, "用户身份验证失败"));
        }
        
        var response = await _activityService.DeleteActivityAsync(activityId, deleterId.Value);
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

    /// <summary>
    /// 上传活动封面图片
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    /// <param name="activityId">活动ID</param>
    /// <param name="file">图片文件</param>
    [HttpPost("{circleId}/activities/{activityId}/cover")]
    public async Task<IActionResult> UploadActivityCover(int circleId, int activityId, IFormFile file)
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
            return BadRequest(BaseHttpResponse.Fail(400, "活动封面图片文件大小不能超过10MB"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _circleService.UploadActivityImageAsync(activityId, stream, file.FileName, file.ContentType);
            
            if (result == null)
            {
                return NotFound(BaseHttpResponse.Fail(404, $"活动ID {activityId} 不存在或FileBrowser上传失败。请检查：1.活动是否存在 2.FileBrowser服务是否正常 3.网络连接是否正常"));
            }

            return Ok(BaseHttpResponse<object>.Success(result, "活动封面图片上传成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"上传失败: {ex.Message}"));
        }
    }

    // --- 帖子相关接口 ---

    /// <summary>
    /// 获取指定圈子内的所有帖子ID列表
    /// </summary>
    /// <param name="circleId">圈子ID</param>
    [HttpGet("{circleId}/posts")]
    public async Task<IActionResult> GetPostIdsByCircleId(int circleId)
    {
        try
        {
            var result = await _circleService.GetPostIdsByCircleIdAsync(circleId);
            return Ok(BaseHttpResponse<object>.Success(result, "获取圈子帖子ID列表成功"));
        }
        catch (ArgumentException ex)
        {
            return NotFound(BaseHttpResponse.Fail(404, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"获取圈子帖子ID列表失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取所有帖子的ID列表（用于验证数据库连接性和POST表存在性）
    /// </summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetAllPostIds()
    {
        try
        {
            var result = await _circleService.GetPostIdsByCircleIdAsync();
            return Ok(BaseHttpResponse<object>.Success(result, "获取所有帖子ID列表成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, BaseHttpResponse.Fail(500, $"获取所有帖子ID列表失败: {ex.Message}"));
        }
    }

    [HttpPost("{circleId}/members/join")]
    public async Task<IActionResult> JoinCircleAsync(int circleId)
    {
        var userId = GetUserIdFromToken();

        await _circleMemberService.JoinCircleAsync(circleId, userId);

        return Ok(new { message = "Join request processed." });
    }

} 