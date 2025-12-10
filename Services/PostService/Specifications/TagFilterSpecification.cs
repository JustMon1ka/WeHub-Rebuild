using System.Linq;
using Models;

namespace PostService.Specifications;

/// <summary>
/// 规格: 若提供 tagName,则只保留带有该标签的帖子;
/// 若 tagName 为空或仅为空白,则不对查询做任何修改。
/// </summary>
public sealed class TagFilterSpecification : IPostSpecification
{
    private readonly string? _tagName;

    public TagFilterSpecification(string? tagName)
    {
        _tagName = tagName;
    }

    public IQueryable<Post> Apply(IQueryable<Post> query)
    {
        if (string.IsNullOrWhiteSpace(_tagName))
        {
            // 不需要过滤标签时,直接返回原查询
            return query;
        }

        return query.Where(p =>
            p.PostTags != null &&
            p.PostTags.Any(pt => pt.Tag != null && pt.Tag.TagName == _tagName));
    }
}
