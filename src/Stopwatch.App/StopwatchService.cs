using Stopwatch.Domain;
using Stopwatch.Ports;

namespace Stopwatch.App;

public sealed class StopwatchService : IStopwatchService
{
    private readonly IStopwatchRepository _repository;
    private readonly ITimeProvider _timeProvider;
    private StopwatchState _state;

    public StopwatchService(IStopwatchRepository repository, ITimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _state = new StopwatchState();
    }

    public async Task InitializeAsync()
    {
        var timestamps = await _repository.LoadAsync();
        _state = StopwatchState.FromTimestamps(timestamps);
    }

    public async Task StartAsync()
    {
        _state.Start(_timeProvider.Now);
        await _repository.SaveAsync(_state.ToTimestamps());
    }

    public async Task StopAsync()
    {
        _state.Stop(_timeProvider.Now);
        await _repository.SaveAsync(_state.ToTimestamps());
    }

    public async Task ClearAsync()
    {
        _state.Clear();
        await _repository.SaveAsync(_state.ToTimestamps());
    }

    public Task<TimeSpan> GetElapsedAsync()
    {
        var elapsed = _state.CalculateTotalElapsed(_timeProvider.Now);
        return Task.FromResult(elapsed);
    }

    public Task<bool> IsRunningAsync()
    {
        return Task.FromResult(_state.IsRunning);
    }

    public async Task UpdateRunningStateAsync()
    {
        if (_state.IsRunning)
        {
            var timestamps = _state.ToTimestamps().ToList();
            timestamps.Add(_timeProvider.Now);
            await _repository.SaveAsync(timestamps);
        }
    }
}


