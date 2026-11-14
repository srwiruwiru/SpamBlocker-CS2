using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace SpamBlocker.Utils;

public static class PlayerUtils
{
    private const string UnknownValue = "Unknown";

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
}
