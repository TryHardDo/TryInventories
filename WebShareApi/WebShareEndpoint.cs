namespace TryInventories.WebShareApi;

public abstract class WebShareEndpoint
{
    public abstract string EndpointUrl { get; }
    public abstract HttpMethod Method { get; }
    public abstract Dictionary<string, string>? QueryParams { get; }
    public abstract object? Body { get; }

    public async Task<T> Call<T>(WebShareClient client)
    {
        return await client.Call<T>(this);
    }
}