namespace Stopwatch.Ports;

public interface IStopwatchService
{
    Task StartAsync();
    Task StopAsync();
    Task ClearAsync();
    Task<TimeSpan> GetElapsedAsync();
    Task<bool> IsRunningAsync();
    Task UpdateRunningStateAsync();
}


