using CircleService.Models;

namespace CircleService.Events;

public class RecommendationCircleObserver : ICircleObserver
{
    public void OnCircleCreated(Circle circle)
    {
        Console.WriteLine($"[Observer] Recommendation updated for new circle: {circle.Name}");
    }
}
