using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using SpamBlocker.Managers;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Services.NameProtection;

public sealed class NameProtectionService(
    FeatureContext context,
    FilterManager filterManager,
    ViolationReporter violationReporter) : FeatureService(context)
{
    private const float NameCheckDelaySeconds = 2.0f;
    private const float PlayerEventReapplyDelaySeconds = 2.0f;
    private const float PlayerNotificationDelaySeconds = 0.5f;
    private const int MinRandomSuffix = 1000;
    private const int MaxRandomSuffix = 9999;
    private const string SuffixSeparator = "_";

    private static readonly float[] RoundStartReapplyDelays = [1.0f, 3.0f, 5.0f];
    private static readonly float[] RoundEndReapplyDelays = [1.0f, 3.0f];

    private readonly HashSet<int> _renamedSlots = [];

    protected override string LogCategory => "NameProtection";

    public void CheckExistingPlayers()
    {
        foreach (var player in PlayerUtils.GetValidPlayers())
        {
            if (string.IsNullOrEmpty(player.PlayerName))
                continue;

            var result = filterManager.CheckContent(player.PlayerName, player);
            if (result.IsBlocked)
                HandleViolation(player, result);
        }
    }

    protected override void OnEnable()
    {
        HookGameEvent<EventPlayerConnectFull>(OnPlayerConnectFull);
        HookGameEvent<EventRoundStart>(OnRoundStart);
        HookGameEvent<EventRoundEnd>(OnRoundEnd);
        HookGameEvent<EventPlayerSpawn>(OnPlayerSpawn);
        HookGameEvent<EventPlayerDeath>(OnPlayerDeath);
        HookClientDisconnect(playerSlot => _renamedSlots.Remove(playerSlot));
    }

    protected override void OnDisable()
    {
        _renamedSlots.Clear();
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (PlayerUtils.IsValidPlayer(@event.Userid))
        {
            var playerSlot = @event.Userid!.Slot;
            RunAfter(NameCheckDelaySeconds, () => CheckPlayerName(playerSlot));
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        ScheduleNameReapply(RoundStartReapplyDelays);
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        ScheduleNameReapply(RoundEndReapplyDelays);
        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (PlayerUtils.IsValidPlayer(@event.Userid))
            RunAfter(PlayerEventReapplyDelaySeconds, ReapplyReplacementNames);

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (PlayerUtils.IsValidPlayer(@event.Userid))
            RunAfter(PlayerEventReapplyDelaySeconds, ReapplyReplacementNames);

        return HookResult.Continue;
    }

    private void CheckPlayerName(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (!PlayerUtils.HasValidName(player))
            return;

        var result = filterManager.CheckPlayerName(player!.PlayerName, player);
        if (result.IsBlocked)
            HandleViolation(player, result);
    }

    private void HandleViolation(CCSPlayerController player, FilterResult result)
    {
        Logger.LogDebug(LogCategory, $"Name violation by {player.PlayerName}: {result.Reason}");
        ReplacePlayerName(player, result);
        violationReporter.Report(player, ViolationSource.Name, result);
    }

    private void ReplacePlayerName(CCSPlayerController player, FilterResult result)
    {
        if (!Config.NameProtection.Enabled)
            return;

        var newName = BuildReplacementName(player);
        if (string.IsNullOrEmpty(newName))
        {
            Logger.LogError(LogCategory, $"Cannot rename {player.PlayerName}: the replacement name is empty");
            return;
        }

        RenameOnNextFrame(player.Slot, newName);
        _renamedSlots.Add(player.Slot);
        NotifyRename(player.Slot, new ViolationNotice(Plugin.Localizer["admin_name_change"], newName, result));
    }

    private void NotifyRename(int playerSlot, ViolationNotice notice)
    {
        if (Config.NameProtection.NotifyPlayer)
            RunAfter(PlayerNotificationDelaySeconds, () => NotifyPlayerOfRename(playerSlot));

        if (Config.NameProtection.NotifyAdmins)
            NotificationUtils.NotifyAdminsOfViolation(Plugin, notice);
    }

    private void NotifyPlayerOfRename(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player is { IsValid: true })
            NotificationUtils.NotifyPlayer(player, Plugin.Localizer["name_changed"], Plugin);
    }

    private void ScheduleNameReapply(float[] delaysSeconds)
    {
        foreach (var delaySeconds in delaysSeconds)
            RunAfter(delaySeconds, ReapplyReplacementNames);
    }

    private void ReapplyReplacementNames()
    {
        foreach (var playerSlot in _renamedSlots.ToList())
        {
            var player = Utilities.GetPlayerFromSlot(playerSlot);
            if (!PlayerUtils.IsValidPlayer(player))
            {
                _renamedSlots.Remove(playerSlot);
                continue;
            }

            var expectedName = BuildReplacementName(player!);
            if (player!.PlayerName != expectedName)
                RenameOnNextFrame(playerSlot, expectedName);
        }
    }

    private void RenameOnNextFrame(int playerSlot, string newName)
    {
        RunNextFrame(() => RenamePlayer(playerSlot, newName));
    }

    private void RenamePlayer(int playerSlot, string newName)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player is not { IsValid: true })
            return;

        try
        {
            PlayerUtils.RenamePlayer(player, newName);
            Logger.LogDebug(LogCategory, $"Player in slot {playerSlot} renamed to '{newName}'");
        }
        catch (Exception exception)
        {
            Logger.LogError(LogCategory, $"Error renaming player: {exception.Message}");
        }
    }

    private string BuildReplacementName(CCSPlayerController player)
    {
        var nameProtection = Config.NameProtection;
        if (!nameProtection.AddUserIdSuffix)
            return nameProtection.ReplacementName;

        var suffix = player.UserId?.ToString() ?? Random.Shared.Next(MinRandomSuffix, MaxRandomSuffix).ToString();
        return nameProtection.ReplacementName + SuffixSeparator + suffix;
    }
}
