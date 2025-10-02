using Stopwatch.Ports;

namespace Stopwatch.Unit.Fakes;

public sealed class FakeTimeProvider : ITimeProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

    public void Advance(TimeSpan duration)
    {
        Now += duration;
    }
}


