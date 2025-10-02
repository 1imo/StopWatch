namespace Stopwatch.Domain;

public sealed class StopwatchState
{
    private readonly List<TimestampSpan> _spans = new();

    public IReadOnlyList<TimestampSpan> Spans => _spans.AsReadOnly();

    public bool IsRunning => _spans.Count > 0 && _spans[^1].IsRunning;

    public static StopwatchState FromTimestamps(IReadOnlyList<DateTimeOffset> timestamps)
    {
        var state = new StopwatchState();

        for (int i = 0; i < timestamps.Count; i += 2)
        {
            var start = timestamps[i];
            var stop = i + 1 < timestamps.Count ? timestamps[i + 1] : (DateTimeOffset?)null;
            state._spans.Add(new TimestampSpan(start, stop));
        }

        return state;
    }

    public IReadOnlyList<DateTimeOffset> ToTimestamps()
    {
        var result = new List<DateTimeOffset>();

        foreach (var span in _spans)
        {
            result.Add(span.Start);
            if (span.Stop.HasValue)
            {
                result.Add(span.Stop.Value);
            }
        }

        return result;
    }

    public void Start(DateTimeOffset now)
    {
        if (IsRunning)
        {
            return;
        }

        _spans.Add(new TimestampSpan(now, null));
    }

    public void Stop(DateTimeOffset now)
    {
        if (!IsRunning)
        {
            return;
        }

        var lastSpan = _spans[^1];
        _spans[^1] = lastSpan with { Stop = now };
    }

    public void Clear()
    {
        _spans.Clear();
    }

    public TimeSpan CalculateTotalElapsed(DateTimeOffset now)
    {
        var total = TimeSpan.Zero;

        foreach (var span in _spans)
        {
            var end = span.Stop ?? (span.IsRunning ? now : span.Start);
            total += end - span.Start;
        }

        return total;
    }

    public void UpdateRunningTimestamp(DateTimeOffset now)
    {
        if (!IsRunning)
        {
            return;
        }

        var lastSpan = _spans[^1];
        _spans[^1] = lastSpan with { Stop = now };
        _spans[^1] = _spans[^1] with { Stop = null };
    }
}


