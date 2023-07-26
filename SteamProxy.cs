using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.Models;
using TryInventories.SettingModels;
using TryInventories.WebShareApi;
using TryInventories.WebShareApi.Endpoints;

namespace TryInventories;

public class SteamProxy
{
    private readonly Settings _appSettings;
    private readonly WebShareClient _webShareClient;

    public SteamProxy(IOptions<Settings> appSettings)
    {
        _appSettings = appSettings.Value;
        _webShareClient = new WebShareClient(_appSettings.InternalRotationSettings.WebShareApiKey);
        ProxyClient = new ProxyClient(new ProxyPool(), _appSettings.InternalRotationSettings.RotationThreshold);
    }

    public ProxyClient ProxyClient { get; private set; }

    public void Init()
    {
        if (!_appSettings.AcceptTermsOfUse)
        {
            Log.Warning("DISCLAIMER:\n" +
                        "By accepting the Terms of Use and using this software, you acknowledge that I, the creator/developer, cannot be held responsible for any damages, losses,\n" +
                        "or issues that may arise from the use of this software. The software is provided \"as is,\" without any warranties, and users assume full responsibility for its use.\n" +
                        "Users are encouraged to review and understand this disclaimer before proceeding with the software.\n\n" +
                        "If you agree with these then change \"{fieldName}\" to true in {configFile} to continue using this program.", "AcceptTermsOfUse", "appsettings.json");

            Environment.Exit(0);
        }

        // Todo: Re-implement external rotation option!
        if (_appSettings.ProxyMode == Mode.External)
            Log.Warning(
                "External proxy rotation feature is under rework and not available for use right now. Internal rotation mode will be used!");
        else
            Log.Information("Using {mode} rotation mode. The proxy rotation will be handled by TryInventories!",
                _appSettings.ProxyMode);

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
            if (_appSettings.ShuffleProxyPool) pool.ShufflePool();

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