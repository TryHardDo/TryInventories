using System.Text.Json;
using System.Text.Json.Serialization;

namespace TryInventories.Models;

public record ProxyListResponse(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("next")] string? Next,
    [property: JsonPropertyName("previous")]
    object Previous,
    [property: JsonPropertyName("results")]
    IReadOnlyList<ProxyEntry> Results)
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}