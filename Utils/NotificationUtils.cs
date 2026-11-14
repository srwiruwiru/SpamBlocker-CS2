using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

using SpamBlocker.Models;

namespace SpamBlocker.Utils;

public static class NotificationUtils
{
    private const string LogCategory = "Notifications";
    private const string AdminPermission = "@css/admin";

    public static void NotifyPlayer(CCSPlayerController player, string message, BasePlugin plugin)
    {
        if (!player.IsValid)
            return;

        player.PrintToChat($"{plugin.Localizer["prefix"]} {message}");
    }

    public static void NotifyAdminsOfViolation(BasePlugin plugin, ViolationNotice notice)
    {
        var message = $"{notice.ActionText} {plugin.Localizer["violation_by"]} {notice.PlayerName}: " +
                      $"{notice.Result.Reason} ('{notice.Result.DetectedContent}')";
        NotifyAdmins(plugin, message);
    }

    private static void NotifyAdmins(BasePlugin plugin, string message)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (PlayerUtils.IsValidPlayer(player) && AdminManager.PlayerHasPermissions(player, AdminPermission))
                player.PrintToChat($"{plugin.Localizer["prefix"]} {message}");
        }

        Logger.LogInfo(LogCategory, message);
    }
}
