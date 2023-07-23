using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace TryInventories.WebShareApi;

public class WebShareClient : IDisposable
{
    private readonly CancellationTokenSource _cancellationToken;
    private readonly HttpClient _client;

    public WebShareClient(string webShareApiKey)
    {
        _client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Token", webShareApiKey)
            },
            Timeout = TimeSpan.FromSeconds(30)
        };

        _cancellationToken = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _client.Dispose();
        _cancellationToken.Dispose();
    }

    private async Task<HttpResponseMessage> CallRaw(WebShareEndpoint endpoint)
    {
        const int max = 10;
        var attempt = 0;
        var delay = 1000;

        HttpResponseMessage? response;
        do
        {
            var queryDict = endpoint.QueryParams;

            var queryStr = ParseQueryStr(queryDict);
            var uriStr = endpoint.EndpointUrl;

            if (queryStr != null) uriStr += $"?{queryStr}";

            using var reqMsg = new HttpRequestMessage(endpoint.Method, new Uri(uriStr));

            if (endpoint.Body != null)
            {
                var jsonString = JsonSerializer.Serialize(endpoint.Body);
                reqMsg.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }

            response = await _client.SendAsync(reqMsg, _cancellationToken.Token);
            if (response.IsSuccessStatusCode) break;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new HttpRequestException(
                    $"The response returned {response.StatusCode} which mainly caused by invalid API key. Please check it!");

            await Task.Delay(delay);
            attempt++;
            delay += 1000;
        } while (attempt <= max);

        return response ?? throw new InvalidOperationException("The response was NULL!");
    }

    public async Task<string> Call(WebShareEndpoint endpoint)
    {
        var rawCall = await CallRaw(endpoint);
        var contentStr = await rawCall.Content.ReadAsStringAsync();

        return contentStr;
    }

    public async Task<T> Call<T>(WebShareEndpoint endpoint)
    {
        var rawCall = await CallRaw(endpoint);
        var contentStr = await rawCall.Content.ReadAsStringAsync();
        return ToObj<T>(contentStr);
    }

    private static T ToObj<T>(string jsonString)
    {
        var obj = JsonSerializer.Deserialize<T>(jsonString);
        return obj ?? throw new JsonException($"{nameof(obj)} was null after deserialization!");
    }

    private static string? ParseQueryStr(Dictionary<string, string>? queryDictionary)
    {
        if (queryDictionary == null || queryDictionary.Count == 0) return null;
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kvp in queryDictionary) queryString[kvp.Key] = kvp.Value;

        return queryString.ToString();
    }
}