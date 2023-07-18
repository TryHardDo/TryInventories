using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Serilog;

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

        HttpResponseMessage? response = null;
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

            try
            {
                response = await _client.SendAsync(reqMsg, _cancellationToken.Token);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to invoke API endpoint \"{endpoint}\"! Attempt: {attempt} | Headers: {headers}",
                    reqMsg.RequestUri,
                    attempt, _client.DefaultRequestHeaders);

                await Task.Delay(delay);
                attempt++;
                delay += 1000;

                continue;
            }

            break;
        } while (attempt <= max);

        return response ?? throw new InvalidOperationException("Failed to execute API request with multiple retries!");
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