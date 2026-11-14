using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using SpamBlocker.Managers;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Services.ChatProtection;

public sealed class ChatProtectionService(
    FeatureContext context,
    FilterManager filterManager,
    ViolationReporter violationReporter) : FeatureService(context)
{
    private const string PublicChatCommand = "say";
    private const string TeamChatCommand = "say_team";
    private const int MessageArgumentIndex = 1;
    private const float PunishmentDelaySeconds = 0.5f;
    private const string KickAction = "kick";
    private const string BanAction = "ban";
    private const string CustomAction = "custom";

    private readonly ViolationCooldown _cooldown = new();

    protected override string LogCategory => "ChatProtection";

    protected override void OnEnable()
    {
        HookCommandListener(PublicChatCommand, OnPublicChat);
        HookCommandListener(TeamChatCommand, OnTeamChat);
        HookClientDisconnect(_cooldown.RemovePlayer);
    }

    protected override void OnDisable()
    {
        _cooldown.Clear();
    }

    private HookResult OnPublicChat(CCSPlayerController? player, CommandInfo info)
    {
        return FilterChatMessage(player, info, ViolationSource.Chat);
    }

    private HookResult OnTeamChat(CCSPlayerController? player, CommandInfo info)
    {
        return FilterChatMessage(player, info, ViolationSource.TeamChat);
    }

    private HookResult FilterChatMessage(CCSPlayerController? player, CommandInfo info, ViolationSource source)
    {
        var message = info.GetArg(MessageArgumentIndex);
        if (!PlayerUtils.IsValidAuthorizedPlayer(player) || message.Length == 0 || CommandUtils.IsCommand(message))
            return HookResult.Continue;

        var result = filterManager.CheckContent(message, player!);
        if (!result.IsBlocked)
            return HookResult.Continue;

        Logger.LogDebug(LogCategory, $"{source} message from {player!.PlayerName} blocked: {result.Reason}");
        if (_cooldown.IsInCooldown(player.Slot, source))
            return HookResult.Handled;

        _cooldown.StartCooldown(player.Slot, source);
        HandleViolation(player, result, source);
        return HookResult.Handled;
    }

    private void HandleViolation(CCSPlayerController player, FilterResult result, ViolationSource source)
    {
        if (Config.ChatProtection.NotifyPlayer)
            NotificationUtils.NotifyPlayer(player, Plugin.Localizer["chat_message_blocked"], Plugin);

        if (Config.ChatProtection.NotifyAdmins)
            NotifyAdmins(player, result, source);

        violationReporter.Report(player, source, result);
        SchedulePunishment(player);
    }

    private void NotifyAdmins(CCSPlayerController player, FilterResult result, ViolationSource source)
    {
        var chatTypeKey = source == ViolationSource.TeamChat ? "team_chat" : "public_chat";
        var actionText = Plugin.Localizer["admin_blocked_message", Plugin.Localizer[chatTypeKey]];
        NotificationUtils.NotifyAdminsOfViolation(Plugin, new ViolationNotice(actionText, player.PlayerName, result));
    }

    private void SchedulePunishment(CCSPlayerController player)
    {
        var command = FindPunishmentCommand();
        if (command == null)
            return;

        RunAfter(PunishmentDelaySeconds, () =>
        {
            if (player.IsValid)
                CommandUtils.ExecuteCommand(command, player);
        });
    }

    private string? FindPunishmentCommand()
    {
        var chatProtection = Config.ChatProtection;
        return chatProtection.Action.ToLowerInvariant() switch
        {
            KickAction => chatProtection.KickCommand,
            BanAction => chatProtection.BanCommand,
            CustomAction when !string.IsNullOrEmpty(chatProtection.CustomCommand) => chatProtection.CustomCommand,
            _ => null
        };
    }
}
