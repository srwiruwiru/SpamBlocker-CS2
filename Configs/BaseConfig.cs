using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace SpamBlocker.Configs;

public sealed class BaseConfig : BasePluginConfig
{
    [JsonPropertyName("WordFilter")]
    public WordFilterConfig WordFilter { get; set; } = new();

    [JsonPropertyName("UrlFilter")]
    public UrlFilterConfig UrlFilter { get; set; } = new();

    [JsonPropertyName("IpFilter")]
    public IpFilterConfig IpFilter { get; set; } = new();

    [JsonPropertyName("NameProtection")]
    public NameProtectionConfig NameProtection { get; set; } = new();

    [JsonPropertyName("ChatProtection")]
    public ChatProtectionConfig ChatProtection { get; set; } = new();

    [JsonPropertyName("DiscordLogging")]
    public DiscordLoggingConfig DiscordLogging { get; set; } = new();

    [JsonPropertyName("Settings")]
    public SettingsConfig Settings { get; set; } = new();

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 2;
}
