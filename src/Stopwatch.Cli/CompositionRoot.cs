using Microsoft.Extensions.DependencyInjection;
using Stopwatch.Adapters.Console;
using Stopwatch.Adapters.FileSystem;
using Stopwatch.Adapters.Singleton;
using Stopwatch.App;
using Stopwatch.Ports;

namespace Stopwatch.Cli;

public sealed class CompositionRoot
{
    private readonly IServiceProvider _serviceProvider;

    public CompositionRoot()
    {
        var services = new ServiceCollection();

        var appDataPath = GetApplicationDataPath();
        var dataFilePath = Path.Combine(appDataPath, "stopwatch.json");
        var lockFilePath = Path.Combine(appDataPath, "stopwatch.lock");
        var instanceLockPath = Path.Combine(appDataPath, "instance.lock");

        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<ILockProvider>(new FileLockProvider(lockFilePath));
        services.AddSingleton<IStopwatchRepository>(sp =>
            new FileSystemStopwatchRepository(dataFilePath, sp.GetRequiredService<ILockProvider>()));
        services.AddSingleton<IConsole, SystemConsole>();
        services.AddSingleton<ICommandParser, CommandParser>();
        services.AddSingleton<ISingleInstanceProvider>(new FileSingleInstanceProvider(instanceLockPath));
        services.AddSingleton<IStopwatchService, StopwatchService>();
        services.AddSingleton<StopwatchApplication>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public T Resolve<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private static string GetApplicationDataPath()
    {
        var appName = "PersistentStopwatch";

        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, appName);
        }

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", appName);
        }

        if (OperatingSystem.IsLinux())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", appName);
        }

        throw new PlatformNotSupportedException("Unsupported operating system");
    }
}


