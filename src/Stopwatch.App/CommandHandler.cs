using Stopwatch.Ports;

namespace Stopwatch.App;

public sealed class CommandHandler
{
    private readonly IStopwatchService _stopwatchService;

    public CommandHandler(IStopwatchService stopwatchService, IConsole console)
    {
        _stopwatchService = stopwatchService;
    }

    public async Task<bool> HandleAsync(StopwatchCommand command)
    {
        switch (command)
        {
            case StopwatchCommand.Start:
                await _stopwatchService.StartAsync();
                return true;

            case StopwatchCommand.Stop:
                await _stopwatchService.StopAsync();
                return true;

            case StopwatchCommand.Clear:
                await _stopwatchService.ClearAsync();
                return true;

            case StopwatchCommand.Quit:
                return false;

            case StopwatchCommand.Unknown:
                return true;

            default:
                return true;
        }
    }
}


