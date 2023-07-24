using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.Models;
using TryInventories.Settings;
using TryInventories.WebShareApi;
using TryInventories.WebShareApi.Endpoints;

namespace TryInventories;

public class SteamProxy
{
    private readonly AppOptions _appOptions;
    private readonly WebShareClient _webShareClient;

    public SteamProxy(IOptions<AppOptions> options, IHostApplicationLifetime applicationLifetime)
    {
        _appOptions = options.Value;
        _webShareClient = new WebShareClient(_appOptions.SelfRotatedProxySettings.WebShareApiKey);
        ProxyClient = new ProxyClient(new ProxyPool());
    }

    public ProxyClient ProxyClient { get; private set; }

    public void Init()
    {
        if (!_appOptions.SelfRotatedProxy)
            Log.Warning(
                "WebShare based proxy rotation feature is under rework and not available for use right now. Self rotated mode will be used!");
        else
            Log.Information("The proxy rotation will be handled by TryInventories!");

        try
        {
            var profileData = GetProfileData().Result;
            if (profileData == null)
                Log.Warning("Profile data can't be retrieved!");
            else
                Log.Information("The API key belongs to the following user: ({id}) {first} {last} -> {email}! \n" +
                                "This API key will be used for using WebShare's API services which is required by the program to work correctly.",
                    profileData.Id,
                    profileData.FirstName, profileData.LastName, profileData.Email);

            var proxies = LoadPoolAsync(100, "direct").Result;

            var pool = new ProxyPool(proxies);
            if (_appOptions.ShuffleProxyList) pool.ShufflePool();

            ProxyClient = new ProxyClient(pool);
        }
        catch (Exception ex)
        {
            Log.Warning(ex,
                "Failed to retrieve WebShare proxy list! The main causer of the issue is the wrong or unset API key. In this state the software uses the original IP as default!");
        }

        Log.Information("Initialization completed!");
    }

    private async Task<ProfileResponse?> GetProfileData()
    {
        return await new UserInfoEndpointMessage().Call<ProfileResponse>(_webShareClient);
    }

    public async Task<List<ProxyEntry>> LoadPoolAsync(int chunkSize, string mode)
    {
        var loadCache = new List<ProxyEntry>();
        bool hasNext;
        var page = 1;

        Log.Information("Started loading proxy list with page size of {pageSize} and mode {mode}!", chunkSize, mode);

        do
        {
            Log.Information("Page - | {page} | Chunk size - {chunkSize}/req", page, chunkSize);
            var rsp = await new ProxyListEndpointMessage
            {
                Mode = mode,
                Page = page,
                PageSize = chunkSize
            }.Call<ProxyListResponse>(_webShareClient);

            Log.Information("Chunk arrived!");

            loadCache.AddRange(rsp.Results);
            Log.Information("Cached {count} proxies! {actual}/{total}", rsp.Results.Count, loadCache.Count, rsp.Count);

            hasNext = rsp.Next != null;
            page++;
        } while (hasNext);

        Log.Information("All proxy retrieved! Proxy count: {finalCount}", loadCache.Count);

        return loadCache;
    }
}