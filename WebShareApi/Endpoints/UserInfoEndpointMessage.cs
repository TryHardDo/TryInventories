namespace TryInventories.WebShareApi.Endpoints;

public class UserInfoEndpointMessage : WebShareEndpoint
{
    public override string EndpointUrl => "https://proxy.webshare.io/api/v2/profile/";
    public override HttpMethod Method => HttpMethod.Get;
    public override Dictionary<string, string>? QueryParams => null;
    public override object? Body => null;
}