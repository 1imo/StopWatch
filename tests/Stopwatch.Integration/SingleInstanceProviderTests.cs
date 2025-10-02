using Stopwatch.Adapters.Singleton;
using Xunit;

namespace Stopwatch.Integration;

public sealed class SingleInstanceProviderTests : IDisposable
{
    private readonly string _testDirectory;

    public SingleInstanceProviderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"stopwatch-singleton-test-{Guid.NewGuid()}");
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
    public async Task AcquireAsync_FirstInstance_Succeeds()
    {
        var lockPath = Path.Combine(_testDirectory, "instance.lock");
        var provider = new FileSingleInstanceProvider(lockPath);

        var lockHandle = await provider.AcquireAsync(CancellationToken.None);

        Assert.NotNull(lockHandle);
        lockHandle.Dispose();
    }

    [Fact]
    public async Task AcquireAsync_SecondInstance_ThrowsException()
    {
        var lockPath = Path.Combine(_testDirectory, "instance.lock");
        var provider1 = new FileSingleInstanceProvider(lockPath);
        var provider2 = new FileSingleInstanceProvider(lockPath);

        var lock1 = await provider1.AcquireAsync(CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await provider2.AcquireAsync(CancellationToken.None));

        lock1.Dispose();
    }

    [Fact]
    public async Task AcquireAsync_AfterDispose_AllowsNewAcquisition()
    {
        var lockPath = Path.Combine(_testDirectory, "instance.lock");
        var provider1 = new FileSingleInstanceProvider(lockPath);
        var provider2 = new FileSingleInstanceProvider(lockPath);

        var lock1 = await provider1.AcquireAsync(CancellationToken.None);
        lock1.Dispose();

        var lock2 = await provider2.AcquireAsync(CancellationToken.None);

        Assert.NotNull(lock2);
        lock2.Dispose();
    }
}


