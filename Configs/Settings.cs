using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class SettingsConfig
{
    [JsonPropertyName("AdminBypass")]
    public bool AdminBypass { get; set; } = false;

    [JsonPropertyName("AdminBypassPermission")]
    public string AdminBypassPermission { get; set; } = "@css/root";

    [JsonPropertyName("DebugMode")]
    public bool DebugMode { get; set; } = false;
}
