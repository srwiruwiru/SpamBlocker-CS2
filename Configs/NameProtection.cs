using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class NameProtectionConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("replacement_name")]
    public string ReplacementName { get; set; } = "Player";

    [JsonPropertyName("add_userid_suffix")]
    public bool AddUserIdSuffix { get; set; } = true;

    [JsonPropertyName("name_blacklist")]
    public NameBlacklistConfig NameBlacklist { get; set; } = new();

    [JsonPropertyName("notify_player")]
    public bool NotifyPlayer { get; set; } = true;

    [JsonPropertyName("notify_admins")]
    public bool NotifyAdmins { get; set; } = true;
}
