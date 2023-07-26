using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.SettingModels;

namespace TryInventories.Controllers;

[ApiController]
[Route("inventory")]
public class InventoryApiController : Controller
{
    private readonly Settings _options;
    private readonly SteamProxy _steamProxy;

    public InventoryApiController(IOptions<Settings> options, SteamProxy steam)
    {
        _options = options.Value;
        _steamProxy = steam;
    }

    [HttpGet("{steamId}/{appId}/{contextId}")]
    public async Task<ActionResult> GetInventory(string steamId, int appId = 440,
        int contextId = 2, string? apiKey = null, [FromQuery(Name = "start_assetid")] string? startAssetId = null)
    {
        if ((string.IsNullOrEmpty(apiKey) || _options.AccessToken != apiKey) &&
            !string.IsNullOrEmpty(_options.AccessToken))
            return Unauthorized($"Parameter {nameof(apiKey)} is invalid!");

        if (string.IsNullOrEmpty(steamId))
            return BadRequest($"Parameter {nameof(steamId)} is invalid!");

        var uriBuilder =
            new StringBuilder(
                $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count=2000");

        if (startAssetId != null) uriBuilder.Append($"&start_assetid={startAssetId}");

        try
        {
            Log.Information(
                "Retrieving inventory for user {steamId} from Steam...", steamId);

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uriBuilder.ToString()));
            request.Headers.Add("Referer", $"https://steamcommunity.com/profiles/{steamId}/inventory");
            request.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Todo: Re-implement the conditional request strategy when it is ready
            using var response = await _steamProxy.ProxyClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            var responseStr = await response.Content.ReadFromJsonAsync<object>() ??
                              throw new Exception("The response content was null!");
            Log.Information("Got inventory for user {steamId} from Steam! Forwarding response...", steamId);

            return Ok(responseStr);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while tried to retrieve {steamId}'s inventory!",
                steamId);
        }

        return StatusCode(500);
    }
}