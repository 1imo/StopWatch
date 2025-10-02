using Stopwatch.App;
using Stopwatch.Unit.Fakes;
using Xunit;

namespace Stopwatch.Unit;

public sealed class StopwatchServiceTests
{
    [Fact]
    public async Task StartAsync_PersistsTimestamp()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        await service.StartAsync();

        var timestamps = repository.GetTimestamps();
        Assert.Single(timestamps);
        Assert.Equal(timeProvider.Now, timestamps[0]);
    }

    [Fact]
    public async Task StopAsync_PersistsStopTimestamp()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        await service.StartAsync();
        timeProvider.Advance(TimeSpan.FromSeconds(15));
        await service.StopAsync();

        var timestamps = repository.GetTimestamps();
        Assert.Equal(2, timestamps.Count);
    }

    [Fact]
    public async Task GetElapsedAsync_ReturnsTotalDuration()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        await service.StartAsync();
        timeProvider.Advance(TimeSpan.FromSeconds(30));
        await service.StopAsync();

        var elapsed = await service.GetElapsedAsync();

        Assert.Equal(TimeSpan.FromSeconds(30), elapsed);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllTimestamps()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        await service.StartAsync();
        await service.StopAsync();
        await service.ClearAsync();

        var timestamps = repository.GetTimestamps();
        Assert.Empty(timestamps);
    }

    [Fact]
    public async Task IsRunningAsync_ReturnsTrueWhenRunning()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        await service.StartAsync();

        var isRunning = await service.IsRunningAsync();
        Assert.True(isRunning);
    }

    [Fact]
    public async Task IsRunningAsync_ReturnsFalseWhenStopped()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        await service.StartAsync();
        await service.StopAsync();

        var isRunning = await service.IsRunningAsync();
        Assert.False(isRunning);
    }

    [Fact]
    public async Task InitializeAsync_LoadsExistingState()
    {
        var repository = new InMemoryStopwatchRepository();
        var timeProvider = new FakeTimeProvider();
        var baseTime = timeProvider.Now;

        await repository.SaveAsync(new List<DateTimeOffset>
        {
            baseTime,
            baseTime.AddSeconds(20)
        });

        var service = new StopwatchService(repository, timeProvider);
        await service.InitializeAsync();

        var elapsed = await service.GetElapsedAsync();
        Assert.Equal(TimeSpan.FromSeconds(20), elapsed);
    }
}


