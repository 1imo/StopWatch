using Stopwatch.App;
using Stopwatch.Cli;
using Stopwatch.Ports;

var compositionRoot = new CompositionRoot();

var singleInstanceProvider = compositionRoot.Resolve<ISingleInstanceProvider>();

IDisposable? instanceLock = null;

try
{
    instanceLock = await singleInstanceProvider.AcquireAsync(CancellationToken.None);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    return 1;
}

try
{
    var stopwatchService = compositionRoot.Resolve<IStopwatchService>();

    if (stopwatchService is StopwatchService service)
    {
        await service.InitializeAsync();
    }

    var application = compositionRoot.Resolve<StopwatchApplication>();

    using var cts = new CancellationTokenSource();

    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    await application.RunAsync(cts.Token);

    if (stopwatchService is StopwatchService svc && await svc.IsRunningAsync())
    {
        await svc.StopAsync();
    }

    return 0;
}
finally
{
    instanceLock?.Dispose();
}


