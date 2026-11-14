using System.Text;

using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Services.DiscordLogging;

public sealed class DiscordLoggingService(FeatureContext context) : FeatureService(context)
{
    private const string JsonMediaType = "application/json";

    private readonly DiscordEmbedBuilder _embedBuilder = new(context.Plugin.Localizer, context.Config.DiscordLogging);
    private HttpClient? _httpClient;

    protected override string LogCategory => "DiscordLogging";

    public bool ShouldEnable()
    {
        return Config.DiscordLogging.Enabled && !string.IsNullOrEmpty(Config.DiscordLogging.WebhookUrl);
    }

    public async Task SendViolationAsync(ViolationReport report, string serverName)
    {
        var httpClient = _httpClient;
        if (httpClient == null || !ShouldLogSource(report.Source)) return;

        try
        {
            var json = _embedBuilder.BuildPayloadJson(report, serverName);
            using var content = new StringContent(json, Encoding.UTF8, JsonMediaType);
            using var response = await httpClient.PostAsync(Config.DiscordLogging.WebhookUrl, content);
            if (response.IsSuccessStatusCode) return;

            var error = await response.Content.ReadAsStringAsync();
            Logger.LogError(LogCategory, $"Webhook failed: {response.StatusCode} - {error}");
        }
        catch (Exception exception)
        {
            Logger.LogError(LogCategory, $"Error sending violation to Discord: {exception.Message}");
        }
    }

    protected override void OnEnable()
    {
        _httpClient = new HttpClient();
    }

    protected override void OnDisable()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }

    private bool ShouldLogSource(ViolationSource source)
    {
        return source switch
        {
            ViolationSource.Chat => Config.DiscordLogging.LogWordViolations,
            // BUG: Team chat violations follow LogNameViolations instead of LogWordViolations
            ViolationSource.TeamChat => Config.DiscordLogging.LogNameViolations,
            ViolationSource.Name => Config.DiscordLogging.LogNameViolations,
            _ => true
        };
    }
}
