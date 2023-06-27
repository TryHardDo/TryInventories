using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TryInventories.Settings;

namespace TryInventories.Controllers;

[ApiController]
[Route("inventory")]
public class InventoryApiController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiController> _logger;
    private readonly AppOptions _options;

    public InventoryApiController(ILogger<InventoryApiController> logger, IOptions<AppOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        var proxy = new WebProxy(_options.ProxyHost, _options.ProxyPort);

        if (_options.UseAuthorization)
            proxy.Credentials = new NetworkCredential
            {
                UserName = _options.ProxyAccess.Username,
                Password = _options.ProxyAccess.Password
            };

        var clientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            PreAuthenticate = true,
            UseCookies = false
        };

        _httpClient = new HttpClient(clientHandler);
    }

    [HttpGet("{steamId}")]
    public async Task<ActionResult<object>> GetInventory(string apiKey, string steamId, int appId = 440,
        int contextId = 2, string? startAssetId = null)
    {
        if (string.IsNullOrEmpty(apiKey) || _options.AccessKey != apiKey)
            return Unauthorized($"Parameter {nameof(apiKey)} is invalid!");

        if (string.IsNullOrEmpty(steamId))
            return BadRequest($"Parameter {nameof(steamId)} is invalid!");

        var uriBuilder =
            new StringBuilder(
                $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count=2000");

        if (startAssetId != null) uriBuilder.Append($"&start_assetid={startAssetId}");

        const int maxRetry = 5;
        var retryCooldown = 500;
        for (var retry = 1; maxRetry > retry; retry++)
            try
            {
                _logger.LogInformation(
                    "Attempting to retrieve inventory for user {steamId}... Attempt: {retry}", steamId, retry);

                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uriBuilder.ToString()));
                request.Headers.Add("Referer", $"https://steamcommunity.com/profiles/{steamId}/inventory");
                request.Headers.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var responseStr = await response.Content.ReadFromJsonAsync<object>() ??
                                  throw new Exception("The response content was null!");
                _logger.LogInformation("Got inventory for user {steamId} from Steam!", steamId);

                return Json(responseStr);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve inventory for {steamId}!", steamId);
                retryCooldown *= 2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while tried to retrieve {steamId}'s inventory!",
                    steamId);
                break;
            }
            finally
            {
                await Task.Delay(retryCooldown);
            }

        return StatusCode(500);
    }
}