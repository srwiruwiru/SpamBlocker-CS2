using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class UrlFilterConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    // INFO: Accepted values are "whitelist" and "blacklist"
    [JsonPropertyName("filter_mode")]
    public string FilterMode { get; set; } = "blacklist";

    [JsonPropertyName("whitelist")]
    public List<string> Whitelist { get; set; } = ["steamcommunity.com", "github.com", "google.com"];

    [JsonPropertyName("blacklist")]
    public List<string> Blacklist { get; set; } = ["kick.com", "twitch.tv"];

    [JsonPropertyName("allowed_protocols")]
    public List<string> AllowedProtocols { get; set; } = ["http", "https", "steam"];
}
