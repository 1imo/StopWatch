using Stopwatch.Ports;

namespace Stopwatch.Unit.Fakes;

public sealed class InMemoryStopwatchRepository : IStopwatchRepository
{
    private IReadOnlyList<DateTimeOffset> _timestamps = Array.Empty<DateTimeOffset>();

    public Task<IReadOnlyList<DateTimeOffset>> LoadAsync()
    {
        return Task.FromResult(_timestamps);
    }

    public Task SaveAsync(IReadOnlyList<DateTimeOffset> timestamps)
    {
        _timestamps = timestamps.ToList();
        return Task.CompletedTask;
    }

    public IReadOnlyList<DateTimeOffset> GetTimestamps() => _timestamps;
}


