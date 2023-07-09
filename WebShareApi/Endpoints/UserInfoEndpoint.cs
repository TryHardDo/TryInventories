namespace TryInventories.WebShareApi.Endpoints;

public class UserInfoEndpoint : IWebShareEndpoint
{
    public HttpMethod GetMethod()
    {
        return HttpMethod.Get;
    }

    public string GetUri()
    {
        return "https://proxy.webshare.io/api/v2/profile/";
    }

    public object? GetBody()
    {
        return null;
    }

    public object? GetQueryParams()
    {
        return null;
    }
}