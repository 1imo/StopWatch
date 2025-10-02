using Stopwatch.Ports;

namespace Stopwatch.Adapters.FileSystem;

public sealed class FileLockProvider : ILockProvider
{
    private readonly string _lockFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileLockProvider(string lockFilePath)
    {
        _lockFilePath = lockFilePath;

        var directory = Path.GetDirectoryName(_lockFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new LockHandle(_semaphore);
    }

    private sealed class LockHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public LockHandle(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Release();
                _disposed = true;
            }
        }
    }
}


