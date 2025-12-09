using System.Diagnostics;

namespace CircleService.Services.Decorators;

public class TimingCircleServiceDecorator : CircleServiceDecorator
{
    public TimingCircleServiceDecorator(ICircleService inner)
        : base(inner) { }

    public override async Task<IEnumerable<Circle>> GetAllCirclesAsync()
    {
        var sw = Stopwatch.StartNew();

        var result = await base.GetAllCirclesAsync();

        sw.Stop();
        Console.WriteLine($"[Timing] GetAllCirclesAsync took {sw.ElapsedMilliseconds} ms");

        return result;
    }
}
