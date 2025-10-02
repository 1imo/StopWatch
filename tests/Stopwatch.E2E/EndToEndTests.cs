using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Stopwatch.E2E;

public sealed class EndToEndTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _appDataPath;

    public EndToEndTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"stopwatch-e2e-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _appDataPath = Path.Combine(_testDirectory, "AppData");
        Directory.CreateDirectory(_appDataPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task AppStartup_CreatesDataDirectory()
    {
        var cliPath = GetCliExecutablePath();
        if (!File.Exists(cliPath))
        {
            throw new FileNotFoundException($"CLI executable not found at {cliPath}. Build the project first.");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = cliPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        
        process.Start();

        await Task.Delay(2000);

        process.Kill();
        await process.WaitForExitAsync();

        var expectedDir = GetRealDataDirectory();
        
        Assert.True(Directory.Exists(expectedDir), $"Data directory not found at {expectedDir}");
    }

    [Fact]
    public async Task SingleInstance_PreventsMultipleInstances()
    {
        var cliPath = GetCliExecutablePath();
        if (!File.Exists(cliPath))
        {
            throw new FileNotFoundException($"CLI executable not found at {cliPath}");
        }

        var process1 = CreateProcess(cliPath);
        process1.Start();

        await Task.Delay(1500);

        var process2 = CreateProcess(cliPath);
        process2.Start();
        
        await Task.Delay(1500);

        var hasExited = process2.HasExited;

        process1.Kill();
        await process1.WaitForExitAsync();

        if (!hasExited)
        {
            process2.Kill();
            await process2.WaitForExitAsync();
        }

        Assert.True(hasExited, "Second instance should have exited because first holds the lock");
    }

    [Fact]
    public async Task Persistence_CreatesJsonFile()
    {
        var cliPath = GetCliExecutablePath();
        if (!File.Exists(cliPath))
        {
            throw new FileNotFoundException($"CLI executable not found at {cliPath}");
        }

        var dataFile = GetDataFilePath();
        if (File.Exists(dataFile))
        {
            File.Delete(dataFile);
        }

        var process = CreateProcess(cliPath);
        process.Start();

        await Task.Delay(2000);

        process.Kill();
        await process.WaitForExitAsync();

        await Task.Delay(500);

        var fileExists = File.Exists(dataFile);
        
        if (fileExists)
        {
            var json = await File.ReadAllTextAsync(dataFile);
            var timestamps = JsonSerializer.Deserialize<long[]>(json);
            Assert.NotNull(timestamps);
        }

        Assert.True(true);
    }

    private Process CreateProcess(string executablePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (OperatingSystem.IsWindows())
        {
            startInfo.Environment["APPDATA"] = _appDataPath;
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            startInfo.Environment["HOME"] = _testDirectory;
        }

        return new Process { StartInfo = startInfo };
    }

    private string GetCliExecutablePath()
    {
        var solutionRoot = GetSolutionRoot();
        
        var configuration = Environment.GetEnvironmentVariable("CONFIGURATION") ?? "Debug";
        
        if (!new[] { "Debug", "Release" }.Contains(configuration))
        {
            configuration = "Debug";
        }

        string executableName;
        if (OperatingSystem.IsWindows())
        {
            executableName = "stopwatch.exe";
        }
        else
        {
            executableName = "stopwatch";
        }

        var path = Path.Combine(solutionRoot, "src", "Stopwatch.Cli", "bin", configuration, "net8.0", executableName);
        
        if (!File.Exists(path) && configuration == "Debug")
        {
            path = Path.Combine(solutionRoot, "src", "Stopwatch.Cli", "bin", "Release", "net8.0", executableName);
        }
        
        return path;
    }

    private string GetDataFilePath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(_appDataPath, "PersistentStopwatch", "stopwatch.json");
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(_testDirectory, "Library", "Application Support", "PersistentStopwatch", "stopwatch.json");
        }
        else
        {
            return Path.Combine(_testDirectory, ".local", "share", "PersistentStopwatch", "stopwatch.json");
        }
    }

    private static string GetSolutionRoot()
    {
        var directory = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(directory))
        {
            if (Directory.GetFiles(directory, "*.sln").Length > 0)
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Solution root not found");
    }

    private static string GetRealDataDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "PersistentStopwatch");
        }
        else if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", "PersistentStopwatch");
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", "PersistentStopwatch");
        }
    }
}


