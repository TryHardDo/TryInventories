using Serilog;

namespace TryInventories.Services;

public class ProxyUpdater : IHostedService
{
    private readonly SteamProxy _steamProxy;
    private Timer? _timer;

    public ProxyUpdater(SteamProxy steamProxy)
    {
        _steamProxy = steamProxy;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartScheduledProxyPoolRefresh();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopScheduler();
        return Task.CompletedTask;
    }

    private void StartScheduledProxyPoolRefresh()
    {
        var syncInterval = TimeSpan.FromMinutes(30);

        _timer = new Timer(_ =>
        {
            Log.Information("Automatic proxy pool sync initiated...");

            var pool = _steamProxy.LoadPoolAsync(100, "direct").Result;
            _steamProxy.ProxyClient.SwapPool(new ProxyPool(pool));

            Log.Information("Proxy pool synced!");
        }, null, syncInterval, syncInterval);
    }

    private void StopScheduler()
    {
        if (_timer == null) return;

        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
    }
}