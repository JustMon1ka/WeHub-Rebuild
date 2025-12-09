namespace CircleService.Events;

using CircleService.Models;

public interface ICircleSubject
{
    void Register(ICircleObserver observer);
    void Unregister(ICircleObserver observer);
    void NotifyCircleCreated(Circle circle);
}
