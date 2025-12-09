namespace CircleService.Events;

using CircleService.Models;

public interface ICircleObserver
{
    void OnCircleCreated(Circle circle);
}
