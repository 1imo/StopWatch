using Stopwatch.Ports;

namespace Stopwatch.Adapters.Singleton;

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}


