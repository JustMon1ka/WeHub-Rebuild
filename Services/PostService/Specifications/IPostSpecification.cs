// Refactoring with `Specification Pattern` - Start
using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace PostService.Specifications;

/// <summary>
/// 针对 Post 的规格接口:
/// 把“筛选规则”封装成一个可以作用于 IQueryable&lt;Post&gt; 的对象。
/// </summary>
public interface IPostSpecification
{
    /// <summary>
    /// 将规格应用到给定的查询上,返回附加了当前规格条件之后的新查询。
    /// </summary>
    IQueryable<Post> Apply(IQueryable<Post> query);
}

/// <summary>
/// 用于把多个 IPostSpecification 按顺序组合起来。
/// 注意: 这里不是 GoF 中的组合模式(Composite Pattern),
/// 只是一个简单的规格集合,依次对查询进行过滤。
/// </summary>
public sealed class PostSpecificationCollection : IPostSpecification
{
    private readonly IReadOnlyList<IPostSpecification> _specifications;

    public PostSpecificationCollection(params IPostSpecification[] specifications)
    {
        _specifications = specifications ?? Array.Empty<IPostSpecification>();
    }

    public IQueryable<Post> Apply(IQueryable<Post> query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        foreach (var spec in _specifications)
        {
            query = spec.Apply(query);
        }

        return query;
    }
}
// Refactoring with `Specification Pattern` - End
