using System.Text.Json;
using Stopwatch.Ports;

namespace Stopwatch.Adapters.FileSystem;

public sealed class FileSystemStopwatchRepository : IStopwatchRepository
{
    private readonly string _filePath;
    private readonly ILockProvider _lockProvider;

    public FileSystemStopwatchRepository(string filePath, ILockProvider lockProvider)
    {
        _filePath = filePath;
        _lockProvider = lockProvider;

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<IReadOnlyList<DateTimeOffset>> LoadAsync()
    {
        using var fileLock = await _lockProvider.AcquireAsync(CancellationToken.None);

        if (!File.Exists(_filePath))
        {
            return Array.Empty<DateTimeOffset>();
        }

        await using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        var timestamps = await JsonSerializer.DeserializeAsync<long[]>(stream);

        if (timestamps == null || timestamps.Length == 0)
        {
            return Array.Empty<DateTimeOffset>();
        }

        return timestamps.Select(ts => DateTimeOffset.FromUnixTimeSeconds(ts)).ToList();
    }

    public async Task SaveAsync(IReadOnlyList<DateTimeOffset> timestamps)
    {
        using var fileLock = await _lockProvider.AcquireAsync(CancellationToken.None);

        var timestampsUnix = timestamps.Select(ts => ts.ToUnixTimeSeconds()).ToArray();

        var tempPath = _filePath + ".tmp";

        await using (var stream = new FileStream(
            tempPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, timestampsUnix);
            await stream.FlushAsync();
        }

        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }

        File.Move(tempPath, _filePath);
    }
}


