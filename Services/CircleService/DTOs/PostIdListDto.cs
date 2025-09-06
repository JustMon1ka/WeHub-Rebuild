namespace CircleService.DTOs;

/// <summary>
/// 用于返回圈子内帖子ID列表的数据传输对象
/// </summary>
public class PostIdListDto
{
    /// <summary>
    /// 圈子ID
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// 帖子ID列表
    /// </summary>
    public required IEnumerable<int> PostIds { get; set; }

    /// <summary>
    /// 帖子总数
    /// </summary>
    public int TotalCount { get; set; }
}
