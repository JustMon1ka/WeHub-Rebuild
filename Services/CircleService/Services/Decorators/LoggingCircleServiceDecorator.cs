using Microsoft.Extensions.Logging;
using CircleService.Models;

namespace CircleService.Services.Decorators;

public class LoggingCircleServiceDecorator : CircleServiceDecorator
{
    private readonly ILogger<LoggingCircleServiceDecorator> _logger;

    public LoggingCircleServiceDecorator(
        ICircleService inner,
        ILogger<LoggingCircleServiceDecorator> logger
    ) : base(inner)
    {
        _logger = logger;
    }

    public override async Task<Circle> CreateCircleAsync(CreateCircleDto dto, int ownerId)
    {
        _logger.LogInformation($"[Circle] CreateCircle start. Owner={ownerId}, Name={dto.Name}");

        var result = await base.CreateCircleAsync(dto, ownerId);

        _logger.LogInformation($"[Circle] CreateCircle success. CircleId={result.CircleId}");

        return result;
    }

    public override async Task<Circle?> GetCircleByIdAsync(int circleId)
    {
        _logger.LogInformation($"[Circle] GetCircleById({circleId})");

        return await base.GetCircleByIdAsync(circleId);
    }
}
