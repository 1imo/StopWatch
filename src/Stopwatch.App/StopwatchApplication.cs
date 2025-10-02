using System.Text;
using Stopwatch.Ports;

namespace Stopwatch.App;

public sealed class StopwatchApplication
{
    private readonly IStopwatchService _stopwatchService;
    private readonly IConsole _console;
    private readonly ICommandParser _commandParser;
    private readonly CommandHandler _commandHandler;
    private readonly StringBuilder _inputBuffer = new();

    public StopwatchApplication(
        IStopwatchService stopwatchService,
        IConsole console,
        ICommandParser commandParser)
    {
        _stopwatchService = stopwatchService;
        _console = console;
        _commandParser = commandParser;
        _commandHandler = new CommandHandler(stopwatchService, console);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _console.Write("Commands: start, stop, clear, quit\n\n");
        
        var running = true;

        while (!cancellationToken.IsCancellationRequested && running)
        {
            var elapsed = await _stopwatchService.GetElapsedAsync();
            var formatted = FormatElapsed(elapsed);
            
            _console.Write($"\r{formatted}  > {_inputBuffer}  ");

            if (_console.KeyAvailable)
            {
                var key = _console.ReadKey();

                if (key.Key == ConsoleKey.Enter)
                {
                    var command = _commandParser.Parse(_inputBuffer.ToString());
                    _inputBuffer.Clear();
                    
                    _console.Write("\n");

                    var shouldContinue = await _commandHandler.HandleAsync(command);
                    if (!shouldContinue)
                    {
                        running = false;
                    }
                }
                else if (key.Key == ConsoleKey.Backspace && _inputBuffer.Length > 0)
                {
                    _inputBuffer.Length--;
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    _inputBuffer.Append(key.KeyChar);
                }
            }

            await _stopwatchService.UpdateRunningStateAsync();

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _console.Write("\n");
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var days = (int)elapsed.TotalDays;
        var hours = elapsed.Hours;
        var minutes = elapsed.Minutes;
        var seconds = elapsed.Seconds;

        return $"{days:D2}:{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}


