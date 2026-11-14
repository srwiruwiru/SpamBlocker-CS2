using System.Text.Json.Serialization;

namespace SpamBlocker.Configs;

public sealed class DiscordLoggingConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("webhook_url")]
    public string WebhookUrl { get; set; } = "";

    [JsonPropertyName("embed_color")]
    public string EmbedColor { get; set; } = "#ff0000";

    [JsonPropertyName("mention_role_id")]
    public string MentionRoleId { get; set; } = "";

    [JsonPropertyName("server_name")]
    public string ServerName { get; set; } = "SpamBlocker";

    [JsonPropertyName("include_steam_profile")]
    public bool IncludeSteamProfile { get; set; } = true;

    [JsonPropertyName("log_word_violations")]
    public bool LogWordViolations { get; set; } = true;

    [JsonPropertyName("log_url_violations")]
    public bool LogUrlViolations { get; set; } = true;

    [JsonPropertyName("log_ip_violations")]
    public bool LogIpViolations { get; set; } = true;

    [JsonPropertyName("log_name_violations")]
    public bool LogNameViolations { get; set; } = true;
}
