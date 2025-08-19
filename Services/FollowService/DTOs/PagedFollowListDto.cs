using System.Collections.Generic;

namespace FollowService.DTOs
{
    public class PagedFollowListDto
    {
        public int TotalCount { get; set; } // 总记录数
        public int Page { get; set; }       // 当前页
        public int PageSize { get; set; }   // 每页大小
        public List<FollowDto> Items { get; set; } = new List<FollowDto>(); // 分页数据
    }
}