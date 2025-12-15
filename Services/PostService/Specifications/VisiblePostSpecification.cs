// Refactoring with `Specification Pattern` - Start
using System.Linq;
using Models;

namespace PostService.Specifications;

/// <summary>
/// 规格: 帖子必须是“可见”的(未删除且未隐藏)。
/// 如果以后可见性的规则发生变化(例如增加审核状态、屏蔽用户),
/// 只需要修改此处的实现即可。
/// </summary>
public sealed class VisiblePostSpecification : IPostSpecification
{
    public IQueryable<Post> Apply(IQueryable<Post> query)
    {
        // 原有逻辑: p.IsDeleted == 0 && p.IsHidden == 0
        return query.Where(p => p.IsDeleted == 0 && p.IsHidden == 0);
    }
}
// Refactoring with `Specification Pattern` - End
