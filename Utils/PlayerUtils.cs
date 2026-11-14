using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace SpamBlocker.Utils;

public static class PlayerUtils
{
    private const string UnknownValue = "Unknown";
    private const string PlayerControllerClassName = "CBasePlayerController";
    private const string PlayerNameField = "m_iszPlayerName";

    public static bool IsValidPlayer(CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
    }

    public static bool IsValidAuthorizedPlayer(CCSPlayerController? player)
    {
        return IsValidPlayer(player) && player!.AuthorizedSteamID != null;
    }

    public static bool HasValidName(CCSPlayerController? player)
    {
        return IsValidPlayer(player) && !string.IsNullOrEmpty(player!.PlayerName);
    }

    public static List<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().Where(IsValidPlayer).ToList();
    }

    public static string GetSafePlayerName(CCSPlayerController? player)
    {
        return HasValidName(player) ? player!.PlayerName : UnknownValue;
    }

    public static string GetSafeSteamId(CCSPlayerController? player)
    {
        return IsValidAuthorizedPlayer(player) ? player!.AuthorizedSteamID!.SteamId64.ToString() : UnknownValue;
    }

    public static void RenamePlayer(CCSPlayerController player, string newName)
    {
        player.PlayerName = newName;
        Utilities.SetStateChanged(player, PlayerControllerClassName, PlayerNameField);

        // NOTE: Firing nextlevel_changed makes clients refresh the displayed player name
        new EventNextlevelChanged(true).FireEvent(false);
    }
}
