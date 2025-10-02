using Stopwatch.Ports;

namespace Stopwatch.Unit.Fakes;

public sealed class FakeConsole : IConsole
{
    private readonly Queue<ConsoleKeyInfo> _keyQueue = new();
    private readonly List<string> _writes = new();

    public IReadOnlyList<string> Writes => _writes;

    public void Write(string text)
    {
        _writes.Add(text);
    }

    public bool KeyAvailable => _keyQueue.Count > 0;

    public ConsoleKeyInfo ReadKey()
    {
        return _keyQueue.Dequeue();
    }

    public void EnqueueKey(ConsoleKeyInfo key)
    {
        _keyQueue.Enqueue(key);
    }
}


