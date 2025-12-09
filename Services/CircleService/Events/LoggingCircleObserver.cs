using Microsoft.Extensions.Logging;
using CircleService.Models;

namespace CircleService.Events;

public class LoggingCircleObserver : ICircleObserver
{
    private readonly ILogger<LoggingCircleObserver> _logger;

    public LoggingCircleObserver(ILogger<LoggingCircleObserver> logger)
    {
        _logger = logger;
    }

    public void OnCircleCreated(Circle circle)
    {
        _logger.LogInformation($"[Observer] Circle created: {circle.CircleId} - {circle.Name}");
    }
}
