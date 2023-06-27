using System.Text.Json;

namespace TryInventories.Settings
{
    public class ProxyAccess
    {
        public string Username { get; set; } = "username";
        public string Password { get; set; } = "password";

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class AppOptions
    {
        public const string Settings = "Settings";

        public string ProxyHost { get; set; } = "http://something.com";
        public int ProxyPort { get; set; } = 8080;
        public bool UseAuthorization { get; set; } = true;
        public ProxyAccess ProxyAccess { get; set; } = new ProxyAccess();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
