using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class IpFilterConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("whitelist_ip_ports")]
    public List<string> WhitelistIpPorts { get; set; } = ["0.0.0.0:27060", "127.0.0.1:27015"];

    [JsonPropertyName("whitelist_ips")]
    public List<string> WhitelistIps { get; set; } = ["0.0.0.0"];
}
