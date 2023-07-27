using System.Text.Json;

namespace TryInventories.SettingModels;

public class InternalRotationSettings
{
    /// <summary>
    ///     The WebShare API key which will be used to send requests to WebShare's API services.
    /// </summary>
    public string WebShareApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     The maximum allowed rotation count. If any retry loop reached this value the method returns the last response from
    ///     the server.
    /// </summary>
    public int RotationThreshold { get; set; } = 20;

    /// <summary>
    ///     Should we sync the proxy pool automatically after <see cref="PoolSyncInterval" /> minutes?
    /// </summary>
    public bool DoScheduledPoolSync { get; set; } = true;

    /// <summary>
    ///     If <see cref="DoScheduledPoolSync" /> is enabled this value defines the interval of the sync rate defined in
    ///     MINUTES.
    /// </summary>
    public int PoolSyncInterval { get; set; } = 30;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class ExternalRotationSettings
{
    /// <summary>
    ///     The host name of the rotated proxy in WebShare.
    /// </summary>
    public string ProxyHost { get; set; } = "p.webshare.io";

    /// <summary>
    ///     The port of the rotated proxy in WebShare.
    /// </summary>
    public int ProxyPort { get; set; } = 80;

    /// <summary>
    ///     Should we use authorization for each request trough rotated proxy? Keep it false if you are using IP authorization.
    /// </summary>
    public bool UseAuthorization { get; set; } = true;

    /// <summary>
    ///     The username for authorization when you are using WebShare's rotated proxy feature.
    ///     You can ignore it if <see cref="UseAuthorization" /> is false.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     The password for authorization when you are using WebShare's rotated proxy feature.
    ///     You can ignore it if <see cref="UseAuthorization" /> is false.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     The maximum retry count trough WebShare rotating proxy. If the attempt count reaches this value the last
    ///     response will be returned.
    /// </summary>
    public int MaxCallRetry { get; set; } = 10;

    /// <summary>
    ///     The delay between each retry call in milliseconds.
    /// </summary>
    public int RetryDelay { get; set; } = 1000;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class Settings
{
    public const string SectionName = "TryInventories";

    /// <summary>
    ///     If it is set to true you accept the terms of use mentioned in the disclaimer at program restart in the console.
    ///     This is false by default and
    ///     the program only keep running if you accept these by setting this value to true.
    /// </summary>
    public bool AcceptTermsOfUse { get; set; } = false;

    /// <summary>
    ///     The mode of the software strategy.
    ///     Internal: Proxy rotation is handled and triggered by the software.
    ///     External: WebShare's rotating proxy feature. The rotation happens regardless of the state
    ///     of the software and it will be triggered by WebShare.
    /// </summary>
    public Mode ProxyMode { get; set; } = Mode.Internal;

    /// <summary>
    ///     Should we shuffle the pool to prevent ordered occurrences for each synchronization?
    ///     If we get the proxy list it usually arrives in the same order each time. After a proxy pool swap (sync)
    ///     the indexer of the rotation returns to the default value (0).
    ///     This usually causes using "exhausted" proxies. In other words you will use the same X proxy each time
    ///     when the software refreshes the proxy pool in Internal strategy and the first few call will return bad response
    ///     if all of the first X proxy was exhausted already.
    ///     I highly recommend using this feature but your choice.
    /// </summary>
    public bool ShuffleProxyPool { get; set; } = true;

    /// <summary>
    ///     Internal rotation strategy related settings.
    /// </summary>
    public InternalRotationSettings InternalRotationSettings { get; set; } = new();

    /// <summary>
    ///     External rotation strategy related settings.
    /// </summary>
    public ExternalRotationSettings ExternalRotationSettings { get; set; } = new();

    /// <summary>
    ///     The access token which will be checked for each request sent to the software. It is useful if you
    ///     are using the app in public mode not on localhost.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public enum Mode
{
    External,
    Internal
}