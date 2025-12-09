namespace CircleService.Builders;

using CircleService.Models;

public interface ICircleBuilder
{
    ICircleBuilder WithName(string name);
    ICircleBuilder WithDescription(string description);
    ICircleBuilder WithCategories(string categories);
    ICircleBuilder WithOwnerId(int ownerId);
    Circle Build();
}

public class CircleBuilder : ICircleBuilder
{
    private string? _name;
    private string? _description;
    private string? _categories;
    private int _ownerId;

    public ICircleBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ICircleBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ICircleBuilder WithCategories(string categories)
    {
        _categories = categories;
        return this;
    }

    public ICircleBuilder WithOwnerId(int ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public Circle Build()
    {
        return new Circle
        {
            Name = _name ?? throw new Exception("Circle.Name is required"),
            Description = _description ?? "",
            Categories = _categories ?? throw new Exception("Circle.Categories is required"),
            OwnerId = _ownerId
        };
    }
}
