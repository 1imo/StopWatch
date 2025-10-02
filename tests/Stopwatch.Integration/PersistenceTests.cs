using Stopwatch.Adapters.FileSystem;
using Stopwatch.Adapters.Singleton;
using Stopwatch.App;
using Xunit;

namespace Stopwatch.Integration;

public sealed class PersistenceTests : IDisposable
{
    private readonly string _testDirectory;

    public PersistenceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"stopwatch-persist-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task StartStopRestart_AggregatesTimeAcrossSessions()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var timeProvider = new FakeTimeProvider();

        var session1Repository = new FileSystemStopwatchRepository(filePath, lockProvider);
        var session1Service = new StopwatchService(session1Repository, timeProvider);
        await session1Service.InitializeAsync();

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:00:00Z");
        await session1Service.StartAsync();

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:00:30Z");
        await session1Service.StopAsync();

        var elapsed1 = await session1Service.GetElapsedAsync();
        Assert.Equal(TimeSpan.FromSeconds(30), elapsed1);

        var session2Repository = new FileSystemStopwatchRepository(filePath, lockProvider);
        var session2Service = new StopwatchService(session2Repository, timeProvider);
        await session2Service.InitializeAsync();

        var elapsedAfterReopen = await session2Service.GetElapsedAsync();
        Assert.Equal(TimeSpan.FromSeconds(30), elapsedAfterReopen);

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:01:00Z");
        await session2Service.StartAsync();

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:01:45Z");
        await session2Service.StopAsync();

        var elapsed2 = await session2Service.GetElapsedAsync();
        Assert.Equal(TimeSpan.FromSeconds(75), elapsed2);
    }

    [Fact]
    public async Task StartAndCrash_DoesNotLoseAllData()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var timeProvider = new FakeTimeProvider();

        var session1Repository = new FileSystemStopwatchRepository(filePath, lockProvider);
        var session1Service = new StopwatchService(session1Repository, timeProvider);
        await session1Service.InitializeAsync();

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:00:00Z");
        await session1Service.StartAsync();

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:00:20Z");
        await session1Service.UpdateRunningStateAsync();

        var session2Repository = new FileSystemStopwatchRepository(filePath, lockProvider);
        var session2Service = new StopwatchService(session2Repository, timeProvider);
        await session2Service.InitializeAsync();

        timeProvider.Now = DateTimeOffset.Parse("2025-01-01T10:00:20Z");
        var elapsed = await session2Service.GetElapsedAsync();

        Assert.True(elapsed >= TimeSpan.FromSeconds(10));
    }

    private sealed class FakeTimeProvider : Stopwatch.Ports.ITimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
    }
}


