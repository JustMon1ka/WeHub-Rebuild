namespace CircleService.Events;

using CircleService.Models;

public class CircleEventSubject : ICircleSubject
{
    private readonly List<ICircleObserver> _observers = new();

    public void Register(ICircleObserver observer)
    {
        _observers.Add(observer);
    }

    public void Unregister(ICircleObserver observer)
    {
        _observers.Remove(observer);
    }

    public void NotifyCircleCreated(Circle circle)
    {
        foreach (var observer in _observers)
        {
            observer.OnCircleCreated(circle);
        }
    }
}
