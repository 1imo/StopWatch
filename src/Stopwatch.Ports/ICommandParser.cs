namespace Stopwatch.Ports;

public interface ICommandParser
{
    StopwatchCommand Parse(string? input);
}

public enum StopwatchCommand
{
    Unknown,
    Start,
    Stop,
    Clear,
    Quit
}


