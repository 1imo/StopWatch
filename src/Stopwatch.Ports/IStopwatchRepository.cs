namespace Stopwatch.Ports;

public interface IStopwatchRepository
{
    Task<IReadOnlyList<DateTimeOffset>> LoadAsync();
    Task SaveAsync(IReadOnlyList<DateTimeOffset> timestamps);
}


