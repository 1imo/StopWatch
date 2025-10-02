using Stopwatch.Adapters.FileSystem;
using Xunit;

namespace Stopwatch.Integration;

public sealed class FileSystemRepositoryTests : IDisposable
{
    private readonly string _testDirectory;

    public FileSystemRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"stopwatch-test-{Guid.NewGuid()}");
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
    public async Task LoadAsync_WithNonExistentFile_ReturnsEmptyList()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var repository = new FileSystemStopwatchRepository(filePath, lockProvider);

        var timestamps = await repository.LoadAsync();

        Assert.Empty(timestamps);
    }

    [Fact]
    public async Task SaveAsync_CreatesFile()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var repository = new FileSystemStopwatchRepository(filePath, lockProvider);

        var now = DateTimeOffset.UtcNow;
        await repository.SaveAsync(new List<DateTimeOffset> { now });

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ReturnsCorrectData()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var repository = new FileSystemStopwatchRepository(filePath, lockProvider);

        var start = DateTimeOffset.UtcNow;
        var stop = start.AddSeconds(30);
        await repository.SaveAsync(new List<DateTimeOffset> { start, stop });

        var loaded = await repository.LoadAsync();

        Assert.Equal(2, loaded.Count);
        Assert.Equal(start.ToUnixTimeSeconds(), loaded[0].ToUnixTimeSeconds());
        Assert.Equal(stop.ToUnixTimeSeconds(), loaded[1].ToUnixTimeSeconds());
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var repository = new FileSystemStopwatchRepository(filePath, lockProvider);

        var first = DateTimeOffset.UtcNow;
        await repository.SaveAsync(new List<DateTimeOffset> { first });

        var second = first.AddSeconds(60);
        await repository.SaveAsync(new List<DateTimeOffset> { second });

        var loaded = await repository.LoadAsync();

        Assert.Single(loaded);
        Assert.Equal(second.ToUnixTimeSeconds(), loaded[0].ToUnixTimeSeconds());
    }

    [Fact]
    public async Task SaveAsync_WithEmptyList_CreatesEmptyFile()
    {
        var filePath = Path.Combine(_testDirectory, "stopwatch.json");
        var lockPath = Path.Combine(_testDirectory, "stopwatch.lock");
        var lockProvider = new FileLockProvider(lockPath);
        var repository = new FileSystemStopwatchRepository(filePath, lockProvider);

        await repository.SaveAsync(Array.Empty<DateTimeOffset>());

        var loaded = await repository.LoadAsync();
        Assert.Empty(loaded);
    }
}


