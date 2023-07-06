using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TryInventories.Models;
using TryInventories.Settings;

namespace TryInventories;

public class SteamProxy
{
    private readonly AppOptions _appOptions;
    private readonly ILogger<SteamProxy> _logger;

    private readonly List<ProxyEntry> _proxyEntries;

    private readonly HttpClient _proxyLoaderClient;
    private int _currentProxyIndex;
    private HttpClient _proxyClient;

    public SteamProxy(ILogger<SteamProxy> logger, IOptions<AppOptions> options)
    {
        _logger = logger;
        _appOptions = options.Value;

        _proxyEntries = new List<ProxyEntry>();
        _currentProxyIndex = 0;

        _proxyLoaderClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Token", _appOptions.WebShareApiKey)
            }
        };

        _proxyClient = new HttpClient();
    }

    public void Init()
    {
        _logger.LogInformation("Getting WebShare profile details...");
        var profileData = GetWebShareProfileDetails().Result;
        _logger.LogInformation("Using services as ({id}) {first} {last} -> {email}!", profileData.Id,
            profileData.FirstName, profileData.LastName, profileData.Email);

        _logger.LogInformation("Loading proxies...");
        LoadProxies().Wait();

        _logger.LogInformation("For initialization we rotate to the first proxy!");
        RotateProxy(true);
    }

    public async Task LoadProxies()
    {
        var reqString = "https://proxy.webshare.io/api/v2/proxy/list/?mode=direct&page=1&page_size=100";
        var retries = 0;
        const int max = 3;
        const int cooldown = 3000;

        while (reqString != null)
        {
            retries++;

            _logger.LogInformation("Retrieving proxy chunk...");

            using var reqMsg = new HttpRequestMessage(HttpMethod.Get, reqString);
            var rsp = await _proxyLoaderClient.SendAsync(reqMsg);

            _logger.LogInformation("Request was sent!");

            try
            {
                rsp.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Failed to retrieve a proxy list chunk from WebShare! Status: {status}",
                    rsp.StatusCode);

                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Api credentials are incorrect!");
                    return;
                }

                if (max < retries)
                {
                    _logger.LogInformation("(Attempt: {retries}) Retrying in {cooldown} seconds...", retries,
                        cooldown / 1000);

                    await Task.Delay(cooldown);
                    continue;
                }

                _logger.LogError("Failed to retrieve proxy list after multiple retries!");
            }

            _logger.LogInformation("Response arrived! Status: {status}", rsp.StatusCode);

            var content = await rsp.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<ProxyListResponse>(content) ??
                       throw new JsonException("Failed to deserialize the returned proxies from WebShare's API!");

            _logger.LogInformation("Content deserialized!");

            reqString = json.Next;
            _proxyEntries.AddRange(json.Results);
        }

        _logger.LogInformation("All proxy has been loaded into cache! We have {proxyCount} proxies total.",
            _proxyEntries.Count);
    }

    public async Task RefreshProxyList()
    {
        const string reqString = "https://proxy.webshare.io/api/v2/proxy/list/refresh/";
        var reqMsg = new HttpRequestMessage(HttpMethod.Post, reqString);
        var rsp = await _proxyLoaderClient.SendAsync(reqMsg);

        rsp.EnsureSuccessStatusCode();
    }

    public async Task<ProfileResponse> GetWebShareProfileDetails()
    {
        const string reqString = "https://proxy.webshare.io/api/v2/profile/";
        var reqMsg = new HttpRequestMessage(HttpMethod.Get, reqString);
        var rsp = await _proxyLoaderClient.SendAsync(reqMsg);

        rsp.EnsureSuccessStatusCode();

        var content = await rsp.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<ProfileResponse>(content) ??
                   throw new JsonException("Failed to deserialize the response from WebShare API!");

        return json;
    }

    public async Task<HttpResponseMessage> SendSelfRotatedProxiedMessage(HttpRequestMessage req)
    {
        const int maxRotates = 10;
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
                _logger.LogWarning(ex, "Proxied call did not indicated success! Rotating proxy and retrying...");

                RotateProxy();
                rotated++;
            }
        } while (rotated < maxRotates);

        if (rotated == maxRotates)
            _logger.LogError(
                "We have reached the maximum allowed rotate count for a request! Returning last response message...");

        return rsp;
    }

    private void RotateProxy(bool init = false)
    {
        if (!init)
            _currentProxyIndex++;

        if (_currentProxyIndex >= _proxyEntries.Count)
        {
            _logger.LogInformation("Reaching end of the proxy list. Resetting to 0...");
            _currentProxyIndex = 0;
        }

        var selectedProxy = _proxyEntries[_currentProxyIndex];
        _logger.LogInformation("New proxy picked -> ID: {id} | {location} => {fullAddress} | Valid? {valid}",
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

        _logger.LogInformation("Proxy successfully set!");
    }
}