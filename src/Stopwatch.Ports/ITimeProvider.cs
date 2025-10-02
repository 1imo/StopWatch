namespace Stopwatch.Ports;

public interface ITimeProvider
{
    DateTimeOffset Now { get; }
}


