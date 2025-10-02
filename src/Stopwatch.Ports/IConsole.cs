namespace Stopwatch.Ports;

public interface IConsole
{
    void Write(string text);
    bool KeyAvailable { get; }
    ConsoleKeyInfo ReadKey();
}


