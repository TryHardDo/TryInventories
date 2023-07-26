using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.SettingModels;

namespace TryInventories.Services;

public class ProxyUpdater : IHostedService
{
    private readonly Settings _settings;
    private readonly SteamProxy _steamProxy;
    private Timer? _timer;

    public ProxyUpdater(SteamProxy steamProxy, IOptions<Settings> appSettings)
    {
        _steamProxy = steamProxy;
        _settings = appSettings.Value;
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
        var syncInterval = TimeSpan.FromMinutes(_settings.InternalRotationSettings.PoolSyncInterval);

        if (_settings.InternalRotationSettings.DoScheduledPoolSync)
            _timer = new Timer(_ =>
            {
                Log.Information("Automatic proxy pool sync initiated...");

                var proxies = _steamProxy.LoadPoolAsync(100, "direct").Result;
                var pool = new ProxyPool(proxies);

                if (_settings.ShuffleProxyPool) pool.ShufflePool();

                _steamProxy.ProxyClient.SwapPool(pool);

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