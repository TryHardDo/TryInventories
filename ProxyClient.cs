using System.Net;
using Serilog;

namespace TryInventories;

public class ProxyClient : IDisposable
{
    public ProxyClient(ProxyPool pool, int maxRotates = 10, int timeout = 5)
    {
        Pool = pool;
        Client = GetNewClient();
        MaxRotates = maxRotates;
        Timeout = timeout;
    }

    public ProxyPool Pool { get; private set; }
    private HttpClient Client { get; set; }
    private int MaxRotates { get; }
    private int Timeout { get; }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
    {
        HttpResponseMessage rsp;
        var rotations = 1;

        do
        {
            using var cloneReq = new HttpRequestMessage
            {
                Content = requestMessage.Content,
                Method = requestMessage.Method,
                Version = requestMessage.Version,
                RequestUri = requestMessage.RequestUri,
                VersionPolicy = requestMessage.VersionPolicy
            };

            rsp = await Client.SendAsync(cloneReq);

            try
            {
                rsp.EnsureSuccessStatusCode();
                break;
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case HttpRequestException:
                        Log.Warning(ex, "Call did not indicate success.");
                        break;
                    case TaskCanceledException:
                        Log.Warning(ex, "The request was canceled due to timeout limitation!");
                        break;
                    default:
                        Log.Error(ex, "An unexpected error happened during proxied call to Steam!");
                        return rsp;
                }

                if (Pool.GetSelected() == null)
                {
                    Log.Warning(
                        "There is no valid proxy pool set for handling requests! The execution won't be retried since it was sent trough the original IP!");
                    return rsp;
                }

                Log.Information("Picking new proxy from the pool...");

                RotateProxyClient();
                rotations++;
            }
        } while (rotations <= MaxRotates);

        if (rotations == MaxRotates)
            Log.Error(
                "We have reached the maximum allowed rotate count for a request! Returning last response message...");

        return rsp;
    }

    public void SwapPool(ProxyPool pool)
    {
        Pool = pool;
        Client.Dispose();

        Client = GetNewClient();
    }

    private void RotateProxyClient()
    {
        Pool.Rotate();
        Client.Dispose();

        Client = GetNewClient();
    }

    private WebProxy? GetWebProxy()
    {
        var currentProxy = Pool.GetSelected();
        if (currentProxy == null) return null;

        return new WebProxy
        {
            Address = new Uri($"http://{currentProxy.ProxyAddress}:{currentProxy.Port}"),
            Credentials = new NetworkCredential(currentProxy.Username, currentProxy.Password)
        };
    }

    private HttpClient GetNewClient()
    {
        var proxy = GetWebProxy();
        return new HttpClient(new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = proxy != null
        }, true)
        {
            Timeout = TimeSpan.FromSeconds(Timeout)
        };
    }

    ~ProxyClient()
    {
        Dispose();
    }
}