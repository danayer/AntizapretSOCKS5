using System.Text.Json.Serialization;

namespace ConfigEditor;

public class AppConfig
{
    [JsonPropertyName("logLevel")]
    public string LogLevel { get; set; } = "None";

    [JsonPropertyName("proxies")]
    public List<ProxyEntry> Proxies { get; set; } = new();
}

public class ProxyEntry
{
    [JsonPropertyName("appNames")]
    public List<string> AppNames { get; set; } = new();

    [JsonPropertyName("socks5ProxyEndpoint")]
    public string Socks5ProxyEndpoint { get; set; } = "socks-local.antizapret:8118";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("password")]
    public string Password { get; set; } = "";

    [JsonPropertyName("supportedProtocols")]
    public List<string> SupportedProtocols { get; set; } = new() { "TCP", "UDP" };

    public override string ToString()
    {
        var apps = string.Join(", ", AppNames);
        var protocols = string.Join(", ", SupportedProtocols);
        return $"[{apps}] → {Socks5ProxyEndpoint} ({protocols})";
    }
}
