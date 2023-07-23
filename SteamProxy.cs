using System.Net;
using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.Models;
using TryInventories.Settings;
using TryInventories.WebShareApi;
using TryInventories.WebShareApi.Endpoints;

namespace TryInventories;

public class SteamProxy : IHostedService
{
    private readonly AppOptions _appOptions;
    private readonly List<ProxyEntry> _proxyPool;
    private readonly WebShareClient _webShareClient;
    private int _currentProxyIndex;
    private HttpClient _proxyClient;

    public SteamProxy(IOptions<AppOptions> options)
    {
        _appOptions = options.Value;

        _proxyPool = new List<ProxyEntry>();
        _currentProxyIndex = 0;

        _webShareClient = new WebShareClient(_appOptions.SelfRotatedProxySettings.WebShareApiKey);
        _proxyClient = new HttpClient();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Init();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Init()
    {
        if (_appOptions.SelfRotatedProxySettings != null)
        {
        }

        if (!_appOptions.SelfRotatedProxy)
        {
            Log.Information("Rotation will be handled by WebShare!");
            var proxy = new WebProxy(_appOptions.AutoRotatedProxySettings.ProxyHost,
                _appOptions.AutoRotatedProxySettings.ProxyPort);

            if (_appOptions.AutoRotatedProxySettings.UseAuthorization)
            {
                Log.Information("Using authorization for proxied requests.");

                proxy.Credentials = new NetworkCredential
                {
                    UserName = _appOptions.AutoRotatedProxySettings.AuthorizationCredentials.Username,
                    Password = _appOptions.AutoRotatedProxySettings.AuthorizationCredentials.Password
                };
            }

            var clientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                PreAuthenticate = true,
                UseCookies = false
            };

            _proxyClient = new HttpClient(clientHandler);
            Log.Information("Ready for requests!");
        }
        else
        {
            Log.Information("Rotation will be handled by TryInventories!");

            try
            {
                var profileData = new UserInfoEndpointMessage().Call<ProfileResponse>(_webShareClient).Result;
                Log.Information("The API key belongs to the following user: ({id}) {first} {last} -> {email}! \n" +
                                "This API key will be used for using WebShare's API services which is required by the program to work correctly.",
                    profileData.Id,
                    profileData.FirstName, profileData.LastName, profileData.Email);
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Failed to retrieve profile details! It mainly caused by the wrong API key. Please check it!");
                return;
            }

            LoadPoolAsync(100, "direct").Wait();

            if (_appOptions.ShuffleProxyList)
            {
                Log.Information("Shuffle proxies...");
                ShufflePool();
            }

            Log.Information("For initialization we rotate to the first proxy!");
            RotateProxy(true);
            Log.Information("Initialization completed!");
        }
    }

    private void ShufflePool()
    {
        var random = new Random();
        for (var i = _proxyPool.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (_proxyPool[i], _proxyPool[j]) = (_proxyPool[j], _proxyPool[i]);
        }
    }

    /// <summary>
    ///     Loads the proxy pool from WebShare into the program's pool cache.
    /// </summary>
    /// <param name="chunkSize">Amount of proxies per request.</param>
    /// <param name="mode">Mode of the proxy: backbone, direct...</param>
    /// <returns></returns>
    private async Task LoadPoolAsync(int chunkSize, string mode)
    {
        // Clear pool to prevent duplicates
        _proxyPool.Clear();

        bool hasNext;
        var page = 1;

        Log.Information("Started loading proxy list with page size of {pageSize} and mode {mode}!", chunkSize, mode);

        do
        {
            Log.Information("Page - {page} | Chunk size - {chunkSize}/req", page, chunkSize);
            var rsp = await new ProxyListEndpointMessage
            {
                Mode = mode,
                Page = page,
                PageSize = chunkSize
            }.Call<ProxyListResponse>(_webShareClient);

            Log.Information("Chunk retrieved!");

            CacheChunk(rsp.Results);
            Log.Information("Cached {count} proxies! {actual}/{total}", rsp.Results.Count, _proxyPool.Count, rsp.Count);

            hasNext = rsp.Next != null;
            page++;
        } while (hasNext);

        Log.Information("All proxy retrieved! Proxy count: {finalCount}", _proxyPool.Count);
    }

    private void CacheChunk(IEnumerable<ProxyEntry> toAdd)
    {
        _proxyPool.AddRange(toAdd);
    }

    public async Task<HttpResponseMessage> SendAutoRotatedProxiedMessage(HttpRequestMessage req)
    {
        var maxRetry = _appOptions.AutoRotatedProxySettings.MaxRetryPerRequest;
        var delay = _appOptions.AutoRotatedProxySettings.RetryDelay;
        var attempt = 0;

        HttpResponseMessage rsp;
        do
        {
            var cloneReq = new HttpRequestMessage
            {
                Content = req.Content,
                Method = req.Method,
                Version = req.Version,
                RequestUri = req.RequestUri,
                VersionPolicy = req.VersionPolicy
            };

            rsp = await _proxyClient.SendAsync(cloneReq);

            try
            {
                rsp.EnsureSuccessStatusCode();
                break;
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "Proxied call did not indicated success! Retrying call in {delay} second(s)...",
                    delay / 1000);
                attempt++;

                await Task.Delay(delay);
            }
        } while (attempt < maxRetry);

        if (attempt == maxRetry)
            Log.Error(
                "We have reached the maximum allowed retry count for this request! Returning last response message...");

        return rsp;
    }

    public async Task<HttpResponseMessage> SendSelfRotatedProxiedMessage(HttpRequestMessage req)
    {
        var maxRotates = _appOptions.SelfRotatedProxySettings.MaxRotatePerRequest;
        var rotated = 0;

        HttpResponseMessage rsp;
        do
        {
            var cloneReq = new HttpRequestMessage
            {
                Content = req.Content,
                Method = req.Method,
                Version = req.Version,
                RequestUri = req.RequestUri,
                VersionPolicy = req.VersionPolicy
            };

            rsp = await _proxyClient.SendAsync(cloneReq);

            try
            {
                rsp.EnsureSuccessStatusCode();
                break;
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "Proxied call did not indicated success! Rotating proxy and retrying...");

                RotateProxy();
                rotated++;
            }
        } while (rotated < maxRotates);

        if (rotated == maxRotates)
            Log.Error(
                "We have reached the maximum allowed rotate count for a request! Returning last response message...");

        return rsp;
    }

    private void RotateProxy(bool init = false)
    {
        if (!init)
            _currentProxyIndex++;

        if (_currentProxyIndex >= _proxyPool.Count)
        {
            Log.Information("Reaching end of the proxy list. Resetting to 0...");
            _currentProxyIndex = 0;
        }

        var selectedProxy = _proxyPool[_currentProxyIndex];
        Log.Information("New proxy picked -> ID: {id} | {location} => {fullAddress} | Valid? {valid}",
            selectedProxy.Id, $"{selectedProxy.CountryCode} ({selectedProxy.CityName})",
            $"{selectedProxy.ProxyAddress}:{selectedProxy.Port}", selectedProxy.Valid);

        var proxy = new WebProxy
        {
            Address = new Uri($"http://{selectedProxy.ProxyAddress}:{selectedProxy.Port}"),
            Credentials = new NetworkCredential(selectedProxy.Username, selectedProxy.Password)
        };

        _proxyClient.Dispose();

        _proxyClient = new HttpClient(new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true
        });

        Log.Information("Proxy successfully set!");
    }
}