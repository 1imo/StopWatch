using Stopwatch.Ports;

namespace Stopwatch.Adapters.Console;

public sealed class CommandParser : ICommandParser
{
    public StopwatchCommand Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return StopwatchCommand.Unknown;
        }

        return input.Trim().ToLowerInvariant() switch
        {
            "start" => StopwatchCommand.Start,
            "stop" => StopwatchCommand.Stop,
            "clear" => StopwatchCommand.Clear,
            "quit" => StopwatchCommand.Quit,
            _ => StopwatchCommand.Unknown
        };
    }
}


