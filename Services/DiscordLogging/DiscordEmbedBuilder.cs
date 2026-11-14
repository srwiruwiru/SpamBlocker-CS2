using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Services.DiscordLogging;

public sealed class DiscordEmbedBuilder(IStringLocalizer localizer, DiscordLoggingConfig config)
{
    private const int DefaultEmbedColor = 0xFF0000;
    private const string HexColorPrefix = "#";
    private const string ServerNamePlaceholder = "{0}";
    private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    private const string SteamProfileUrl = "https://steamcommunity.com/profiles/";
    private const string TitleIcon = "🚨";
    private const string PlayerIcon = "🎯";
    private const string ReasonIcon = "📝";
    private const string DetectedContentIcon = "🔍";

    private static readonly string[] ReadyTags = ["[Ready]", "[No Ready]", "[READY]", "[NOT READY]"];
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string BuildPayloadJson(ViolationReport report, string serverName)
    {
        var payload = new
        {
            content = BuildRoleMention(),
            embeds = new[] { BuildEmbed(report, serverName) }
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private object BuildEmbed(ViolationReport report, string serverName)
    {
        return new
        {
            title = $"{TitleIcon} {GetText("discord_violation_title")}",
            description = GetText("discord_violation_description").Replace(ServerNamePlaceholder, serverName),
            color = ParseEmbedColor(),
            fields = BuildFields(report),
            footer = new { text = GetText("discord_footer_text") },
            timestamp = report.Timestamp.ToString(TimestampFormat)
        };
    }

    private object[] BuildFields(ViolationReport report)
    {
        var playerValue = $"**{GetText("discord_name")}** {FormatPlayerName(report)}\n" +
                          $"**{GetText("discord_steamid")}** {report.SteamId}";

        return
        [
            new { name = $"{PlayerIcon} {GetText("discord_player")}", value = playerValue, inline = false },
            new { name = $"{ReasonIcon} {GetText("discord_reason")}", value = GetText(BlockReasonUtils.GetLangKey(report.Result.Reason)), inline = true },
            new { name = $"{DetectedContentIcon} {GetText("discord_detected_content")}", value = $"`{report.Result.DetectedContent}`", inline = false }
        ];
    }

    private string FormatPlayerName(ViolationReport report)
    {
        var playerName = RemoveReadyTags(report.PlayerName);
        return config.IncludeSteamProfile ? $"[{playerName}]({SteamProfileUrl}{report.SteamId})" : playerName;
    }

    private string BuildRoleMention()
    {
        return string.IsNullOrEmpty(config.MentionRoleId) ? "" : $"<@&{config.MentionRoleId}>";
    }

    private int ParseEmbedColor()
    {
        var hex = config.EmbedColor.StartsWith(HexColorPrefix) ? config.EmbedColor[HexColorPrefix.Length..] : config.EmbedColor;
        return int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var color) ? color : DefaultEmbedColor;
    }

    private string GetText(string key)
    {
        return localizer[key].Value;
    }

    private static string RemoveReadyTags(string playerName)
    {
        foreach (var tag in ReadyTags)
            playerName = playerName.Replace(tag, "");

        return playerName.Trim();
    }
}
