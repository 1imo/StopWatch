namespace Stopwatch.Domain;

public sealed record TimestampSpan(DateTimeOffset Start, DateTimeOffset? Stop)
{
    public TimeSpan Duration => (Stop ?? DateTimeOffset.UtcNow) - Start;

    public bool IsRunning => Stop == null;
}


