namespace Stopwatch.Ports;

public interface ILockProvider
{
    Task<IDisposable> AcquireAsync(CancellationToken cancellationToken);
}


