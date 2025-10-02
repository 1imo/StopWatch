using Stopwatch.Ports;

namespace Stopwatch.Adapters.Console;

public sealed class SystemConsole : IConsole
{
    public void Write(string text)
    {
        System.Console.Write(text);
    }

    public bool KeyAvailable => System.Console.KeyAvailable;

    public ConsoleKeyInfo ReadKey()
    {
        return System.Console.ReadKey(intercept: true);
    }
}


