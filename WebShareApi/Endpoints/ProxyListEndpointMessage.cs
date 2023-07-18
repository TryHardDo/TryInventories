namespace TryInventories.WebShareApi.Endpoints;

public class ProxyListEndpointMessage : WebShareEndpoint
{
    public string Mode { get; init; } = "direct";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;

    public override string EndpointUrl => "https://proxy.webshare.io/api/v2/proxy/list/";
    public override HttpMethod Method => HttpMethod.Get;

    public override Dictionary<string, string>? QueryParams => new()
    {
        { "mode", Mode },
        { "page", Page.ToString() },
        { "page_size", PageSize.ToString() }
    };

    public override object? Body => null;
}