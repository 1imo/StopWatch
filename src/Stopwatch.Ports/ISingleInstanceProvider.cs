namespace Stopwatch.Ports;

public interface ISingleInstanceProvider
{
    Task<IDisposable> AcquireAsync(CancellationToken cancellationToken);
}


