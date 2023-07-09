using System.Text.Json;

namespace TryInventories.Settings;

public class ProxyAccess
{
    public string Username { get; set; } = "username";
    public string Password { get; set; } = "password";

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class SelfRotatedProxySettings
{
    public string WebShareApiKey { get; set; } = string.Empty;
    public int MaxRotatePerRequest { get; set; } = 20;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class AutoRotatedProxySettings
{
    public string ProxyHost { get; set; } = "p.webshare.io";
    public int ProxyPort { get; set; } = 80;
    public bool UseAuthorization { get; set; } = true;
    public ProxyAccess AuthorizationCredentials { get; set; } = new();
    public int MaxRetryPerRequest { get; set; } = 10;
    public int RetryDelay { get; set; } = 1000;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class AppOptions
{
    public const string Settings = "Settings";

    public bool SelfRotatedProxy { get; set; } = true;
    public bool ShuffleProxyList { get; set; } = true;
    public SelfRotatedProxySettings SelfRotatedProxySettings { get; set; } = new();
    public AutoRotatedProxySettings AutoRotatedProxySettings { get; set; } = new();
    public string AccessKey { get; set; } = "place_some_api_key_here";

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}