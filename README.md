# Persistent Stopwatch

A cross-platform terminal stopwatch application built with .NET 8.0 that persists timing data across sessions.

## Features

- Cross-platform support (Windows, macOS, Linux)
- Persistent state using JSON file storage
- Single-instance enforcement
- Commands: `start`, `stop`, `clear`, `quit`
- Display format: `DD:HH:MM:SS`
- Atomic file writes for crash safety
- Hexagonal architecture with DDD principles

## Architecture

The application follows a hexagonal (ports and adapters) architecture:

- **Domain**: Core business logic, entities, and value objects
- **Ports**: Interfaces defining contracts
- **Adapters**: Platform-specific implementations (filesystem, console, singleton)
- **Application**: Use-cases and orchestration
- **CLI**: Composition root and entry point

## Prerequisites

- .NET 8.0 SDK or later

## Build

Build the entire solution:

```bash
dotnet build
```

Build in Release mode:

```bash
dotnet build -c Release
```

## Run

Run the application directly:

```bash
dotnet run --project src/Stopwatch.Cli/Stopwatch.Cli.csproj
```

## Test

Run all tests:

```bash
dotnet test
```

Run specific test projects:

```bash
dotnet test tests/Stopwatch.Unit/Stopwatch.Unit.csproj
dotnet test tests/Stopwatch.Integration/Stopwatch.Integration.csproj
dotnet test tests/Stopwatch.E2E/Stopwatch.E2E.csproj
```

## Publish

Create self-contained single-file executables for each platform:

### Windows (x64)

```bash
dotnet publish src/Stopwatch.Cli/Stopwatch.Cli.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64
```

### Linux (x64)

```bash
dotnet publish src/Stopwatch.Cli/Stopwatch.Cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64
```

### macOS (x64)

```bash
dotnet publish src/Stopwatch.Cli/Stopwatch.Cli.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64
```

### macOS (ARM64)

```bash
dotnet publish src/Stopwatch.Cli/Stopwatch.Cli.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64
```

## Usage

Once the application is running, use the following commands:

- `start` - Start or resume the stopwatch
- `stop` - Pause the stopwatch
- `clear` - Reset the stopwatch and clear all persisted data
- `quit` - Exit the application

The elapsed time is displayed in the format `DD:HH:MM:SS` and updates every second when the stopwatch is running.

## Data Storage

The application stores its state in platform-specific locations:

- **Windows**: `%APPDATA%\PersistentStopwatch\stopwatch.json`
- **macOS**: `~/Library/Application Support/PersistentStopwatch/stopwatch.json`
- **Linux**: `~/.local/share/PersistentStopwatch/stopwatch.json`

The data is stored as an array of Unix epoch timestamps representing start/stop pairs.

## Single Instance

The application enforces single-instance execution. If you try to run multiple instances simultaneously, the second instance will display an error message and exit.

## CI/CD

The repository includes a GitHub Actions workflow that builds and tests the application on Windows, macOS, and Linux.

## Project Structure

```
.
├── src/
│   ├── Stopwatch.Domain/          # Core entities and business logic
│   ├── Stopwatch.Ports/           # Interface definitions
│   ├── Stopwatch.Adapters.FileSystem/  # File-based persistence
│   ├── Stopwatch.Adapters.Console/     # Console I/O
│   ├── Stopwatch.Adapters.Singleton/   # Single-instance enforcement
│   ├── Stopwatch.App/             # Application use-cases
│   └── Stopwatch.Cli/             # Executable entry point
├── tests/
│   ├── Stopwatch.Unit/            # Unit tests
│   ├── Stopwatch.Integration/     # Integration tests
│   └── Stopwatch.E2E/             # End-to-end tests
└── PersistentStopwatch.sln
```

## License

This project is provided as-is for demonstration purposes.


