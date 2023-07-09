using System.Text.Json;
using System.Text.Json.Serialization;

namespace TryInventories.Models;

public record ProxyEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password,
    [property: JsonPropertyName("proxy_address")]
    string ProxyAddress,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("last_verification")]
    DateTime LastVerification,
    [property: JsonPropertyName("country_code")]
    string CountryCode,
    [property: JsonPropertyName("city_name")]
    string CityName,
    [property: JsonPropertyName("created_at")]
    DateTime CreatedAt)

{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}