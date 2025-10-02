using Stopwatch.Ports;

namespace Stopwatch.Adapters.Singleton;

public sealed class FileSingleInstanceProvider : ISingleInstanceProvider
{
    private readonly string _lockFilePath;

    public FileSingleInstanceProvider(string lockFilePath)
    {
        _lockFilePath = lockFilePath;

        var directory = Path.GetDirectoryName(_lockFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stream = new FileStream(
                _lockFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                1,
                FileOptions.DeleteOnClose);

            return Task.FromResult<IDisposable>(new SingleInstanceLock(stream));
        }
        catch (IOException)
        {
            throw new InvalidOperationException("Another instance is already running.");
        }
    }

    private sealed class SingleInstanceLock : IDisposable
    {
        private FileStream? _stream;

        public SingleInstanceLock(FileStream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}


