using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class ChatProtectionConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    // INFO: Accepted values are "block", "kick", "ban" and "custom"
    [JsonPropertyName("action")]
    public string Action { get; set; } = "block";

    [JsonPropertyName("kick_command")]
    public string KickCommand { get; set; } = "css_kick #{userid} \"Spam/Inappropriate content\"";

    [JsonPropertyName("ban_command")]
    public string BanCommand { get; set; } = "css_ban #{userid} 60 \"Spam/Inappropriate content\"";

    [JsonPropertyName("custom_command")]
    public string CustomCommand { get; set; } = "";

    [JsonPropertyName("notify_player")]
    public bool NotifyPlayer { get; set; } = true;

    [JsonPropertyName("notify_admins")]
    public bool NotifyAdmins { get; set; } = true;

    [JsonPropertyName("log_violations")]
    public bool LogViolations { get; set; } = true;
}
