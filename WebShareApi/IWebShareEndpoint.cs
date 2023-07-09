namespace TryInventories.WebShareApi;

public interface IWebShareEndpoint
{
    HttpMethod GetMethod();
    string GetUri();
    object? GetBody();
    object? GetQueryParams();
}