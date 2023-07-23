using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.Settings;

namespace TryInventories.Controllers;

[ApiController]
[Route("inventory")]
public class InventoryApiController : Controller
{
    private readonly AppOptions _options;
    private readonly SteamProxy _steamProxy;

    public InventoryApiController(IOptions<AppOptions> options, SteamProxy steam)
    {
        _options = options.Value;
        _steamProxy = steam;
    }

    [HttpGet("{steamId}/{appId}/{contextId}")]
    public async Task<ActionResult<object>> GetInventory(string steamId, int appId = 440,
        int contextId = 2, string? apiKey = null, [FromQuery(Name = "start_assetid")] string? startAssetId = null)
    {
        if ((string.IsNullOrEmpty(apiKey) || _options.AccessKey != apiKey) && !string.IsNullOrEmpty(_options.AccessKey))
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

            // Using method based on settings
            using var response = _options.SelfRotatedProxy
                ? await _steamProxy.SendSelfRotatedProxiedMessage(request)
                : await _steamProxy.SendAutoRotatedProxiedMessage(request);

            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadFromJsonAsync<object>() ??
                              throw new Exception("The response content was null!");
            Log.Information("Got inventory for user {steamId} from Steam! Forwarding response...", steamId);

            return Json(responseStr);
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Failed to retrieve inventory for {steamId}!", steamId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while tried to retrieve {steamId}'s inventory!",
                steamId);
        }

        return StatusCode(500);
    }
}