namespace CircleService.Services.Decorators;

public abstract class CircleServiceDecorator : ICircleService
{
    protected readonly ICircleService _inner;

    protected CircleServiceDecorator(ICircleService inner)
    {
        _inner = inner;
    }

    public virtual Task<Circle> CreateCircleAsync(CreateCircleDto dto, int ownerId)
        => _inner.CreateCircleAsync(dto, ownerId);

    public virtual Task<Circle?> GetCircleByIdAsync(int circleId)
        => _inner.GetCircleByIdAsync(circleId);

    public virtual Task<IEnumerable<Circle>> GetAllCirclesAsync()
        => _inner.GetAllCirclesAsync();
}
