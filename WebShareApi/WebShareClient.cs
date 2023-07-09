using System.Net.Http.Headers;
using System.Text.Json;

namespace TryInventories.WebShareApi;

public class WebShareClient
{
    private readonly HttpClient _webShareClient;

    public WebShareClient(string webShareApiKey)
    {
        _webShareClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Token", webShareApiKey)
            },

            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private static string ParseQueryParams(object queryParams)
    {
        var properties = queryParams.GetType().GetProperties();
        var dict = properties.ToDictionary(k => k.Name, v => v.GetValue(queryParams)?.ToString() ?? "");

        var str = new FormUrlEncodedContent(dict).ReadAsStringAsync().Result;

        return str;
    }

    private static string StringifyBody(object body)
    {
        return JsonSerializer.Serialize(body);
    }

    private static T ParseContent<T>(string content)
    {
        var json = JsonSerializer.Deserialize<T>(content);

        return json ?? throw new JsonException("Failed to deserialize the content input!");
    }

    public async Task<T> InvokeApiAsync<T>(IWebShareEndpoint request)
    {
        var req = new HttpRequestMessage(request.GetMethod(), request.GetUri());

        var body = request.GetBody();
        if (body != null) req.Content = new StringContent(StringifyBody(body));

        var qp = request.GetQueryParams();
        if (qp != null)
        {
            var ub = new UriBuilder(request.GetUri())
            {
                Query = ParseQueryParams(qp)
            };

            req.RequestUri = new Uri(ub.ToString());
        }

        var rsp = await _webShareClient.SendAsync(req);

        try
        {
            rsp.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("There was an error while we invoked a WebShare endpoint.", ex);
        }

        var rspContent = await rsp.Content.ReadAsStringAsync();
        var json = ParseContent<T>(rspContent);

        return json;
    }
}