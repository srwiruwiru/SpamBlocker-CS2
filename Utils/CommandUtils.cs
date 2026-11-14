using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace SpamBlocker.Utils;

public static class CommandUtils
{
    private const string LogCategory = "Commands";
    private const string UserIdPlaceholder = "{userid}";
    private const string SteamIdPlaceholder = "{steamid}";
    private const string NamePlaceholder = "{name}";
    private const string SlotPlaceholder = "{slot}";

    private static readonly string[] CommandPrefixes = ["!", "@", "/", "."];

    public static void ExecuteCommand(string commandTemplate, CCSPlayerController player)
    {
        if (string.IsNullOrEmpty(commandTemplate) || !player.IsValid)
            return;

        try
        {
            var command = SubstitutePlaceholders(commandTemplate, player);
            Logger.LogDebug(LogCategory, $"Executing command: {command}");
            Server.ExecuteCommand(command);
        }
        catch (Exception exception)
        {
            Logger.LogError(LogCategory, $"Error executing command '{commandTemplate}': {exception.Message}");
        }
    }

    public static bool IsCommand(string message)
    {
        foreach (var prefix in CommandPrefixes)
        {
            if (message.StartsWith(prefix, StringComparison.Ordinal))
                return true;
        }

        return false;
    }


    private static string SubstitutePlaceholders(string template, CCSPlayerController player)
    {
        return template
            .Replace(UserIdPlaceholder, player.UserId?.ToString() ?? "")
            .Replace(SteamIdPlaceholder, player.AuthorizedSteamID?.SteamId64.ToString() ?? "")
            .Replace(NamePlaceholder, player.PlayerName ?? "")
            .Replace(SlotPlaceholder, player.Slot.ToString());
    }
}
