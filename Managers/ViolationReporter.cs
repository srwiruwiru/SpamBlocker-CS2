using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Services;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers;

public sealed class ViolationReporter(BaseConfig config, DiscordService? discordService)
{
    private const string LogCategory = "Violations";
    private const string HostnameConVar = "hostname";
    private const string ConsoleTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public void Report(CCSPlayerController player, ViolationSource source, FilterResult result)
    {
        var report = new ViolationReport(
            PlayerUtils.GetSafePlayerName(player),
            PlayerUtils.GetSafeSteamId(player),
            source,
            result,
            DateTime.UtcNow);

        if (config.ChatProtection.LogViolations)
            LogToConsole(report);

        SendToDiscord(report);
    }

    private static void LogToConsole(ViolationReport report)
    {
        var time = report.Timestamp.ToLocalTime().ToString(ConsoleTimeFormat);
        Logger.LogInfo(LogCategory,
            $"Violation - Player: {report.PlayerName} (SteamID: {report.SteamId}) | " +
            $"Type: {report.Source} | Reason: {report.Result.Reason} | " +
            $"Content: '{report.Result.DetectedContent}' | Time: {time}");
    }

    private void SendToDiscord(ViolationReport report)
    {
        if (discordService == null) return;

        var serverName = GetServerName();
        Task.Run(() => discordService.SendViolationAsync(report, serverName));
    }

    private string GetServerName()
    {
        var hostname = ConVar.Find(HostnameConVar)?.StringValue;
        return string.IsNullOrEmpty(hostname) ? config.DiscordLogging.ServerName : hostname;
    }
}
