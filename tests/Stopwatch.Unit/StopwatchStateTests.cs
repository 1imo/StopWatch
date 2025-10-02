using Stopwatch.Domain;
using Xunit;

namespace Stopwatch.Unit;

public sealed class StopwatchStateTests
{
    [Fact]
    public void NewState_IsNotRunning()
    {
        var state = new StopwatchState();

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void Start_SetsRunningState()
    {
        var state = new StopwatchState();
        var now = DateTimeOffset.UtcNow;

        state.Start(now);

        Assert.True(state.IsRunning);
    }

    [Fact]
    public void Stop_ClearsRunningState()
    {
        var state = new StopwatchState();
        var now = DateTimeOffset.UtcNow;

        state.Start(now);
        state.Stop(now.AddSeconds(10));

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_IsNoOp()
    {
        var state = new StopwatchState();
        var now = DateTimeOffset.UtcNow;

        state.Start(now);
        state.Start(now.AddSeconds(5));

        Assert.Single(state.Spans);
    }

    [Fact]
    public void Stop_WhenNotRunning_IsNoOp()
    {
        var state = new StopwatchState();
        var now = DateTimeOffset.UtcNow;

        state.Stop(now);

        Assert.Empty(state.Spans);
    }

    [Fact]
    public void CalculateTotalElapsed_WithSingleSpan_ReturnsCorrectDuration()
    {
        var state = new StopwatchState();
        var start = DateTimeOffset.UtcNow;

        state.Start(start);
        state.Stop(start.AddSeconds(30));

        var elapsed = state.CalculateTotalElapsed(start.AddSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), elapsed);
    }

    [Fact]
    public void CalculateTotalElapsed_WithMultipleSpans_AggregatesCorrectly()
    {
        var state = new StopwatchState();
        var start = DateTimeOffset.UtcNow;

        state.Start(start);
        state.Stop(start.AddSeconds(10));
        state.Start(start.AddSeconds(20));
        state.Stop(start.AddSeconds(35));

        var elapsed = state.CalculateTotalElapsed(start.AddSeconds(35));

        Assert.Equal(TimeSpan.FromSeconds(25), elapsed);
    }

    [Fact]
    public void CalculateTotalElapsed_WhileRunning_IncludesCurrentTime()
    {
        var state = new StopwatchState();
        var start = DateTimeOffset.UtcNow;

        state.Start(start);

        var elapsed = state.CalculateTotalElapsed(start.AddSeconds(45));

        Assert.Equal(TimeSpan.FromSeconds(45), elapsed);
    }

    [Fact]
    public void FromTimestamps_CreatesCorrectState()
    {
        var start1 = DateTimeOffset.UtcNow;
        var stop1 = start1.AddSeconds(10);
        var start2 = start1.AddSeconds(20);

        var timestamps = new List<DateTimeOffset> { start1, stop1, start2 };

        var state = StopwatchState.FromTimestamps(timestamps);

        Assert.Equal(2, state.Spans.Count);
        Assert.False(state.Spans[0].IsRunning);
        Assert.True(state.Spans[1].IsRunning);
    }

    [Fact]
    public void ToTimestamps_ProducesCorrectOutput()
    {
        var state = new StopwatchState();
        var start = DateTimeOffset.UtcNow;

        state.Start(start);
        state.Stop(start.AddSeconds(10));
        state.Start(start.AddSeconds(20));

        var timestamps = state.ToTimestamps();

        Assert.Equal(3, timestamps.Count);
        Assert.Equal(start, timestamps[0]);
        Assert.Equal(start.AddSeconds(10), timestamps[1]);
        Assert.Equal(start.AddSeconds(20), timestamps[2]);
    }

    [Fact]
    public void Clear_RemovesAllSpans()
    {
        var state = new StopwatchState();
        var start = DateTimeOffset.UtcNow;

        state.Start(start);
        state.Stop(start.AddSeconds(10));
        state.Clear();

        Assert.Empty(state.Spans);
    }
}


