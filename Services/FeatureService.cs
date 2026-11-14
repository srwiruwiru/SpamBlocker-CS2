using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

using SpamBlocker.Configs;
using SpamBlocker.Utils;

namespace SpamBlocker.Services;

public abstract class FeatureService(FeatureContext context)
{
    private readonly List<Action> _unhookActions = [];

    public bool IsActive { get; private set; }

    protected BasePlugin Plugin { get; } = context.Plugin;
    protected BaseConfig Config { get; } = context.Config;
    protected abstract string LogCategory { get; }

    public void Enable()
    {
        if (IsActive) return;

        IsActive = true;
        OnEnable();
        Logger.LogDebug(LogCategory, "Enabled");
    }

    public void Disable()
    {
        if (!IsActive) return;

        IsActive = false;
        ReleaseResources();
        Logger.LogDebug(LogCategory, "Disabled");
    }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() { }

    protected void HookGameEvent<T>(BasePlugin.GameEventHandler<T> handler) where T : GameEvent
    {
        BasePlugin.GameEventHandler<T> guardedHandler = (@event, info) => RunGuarded(() => handler(@event, info));
        Plugin.RegisterEventHandler(guardedHandler);
        _unhookActions.Add(() => Plugin.DeregisterEventHandler(guardedHandler));
    }

    protected void HookCommandListener(string command, CommandInfo.CommandListenerCallback handler)
    {
        CommandInfo.CommandListenerCallback guardedHandler = (player, info) => RunGuarded(() => handler(player, info));
        Plugin.AddCommandListener(command, guardedHandler);
        _unhookActions.Add(() => Plugin.RemoveCommandListener(command, guardedHandler, HookMode.Pre));
    }

    protected void HookClientDisconnect(Action<int> handler)
    {
        Listeners.OnClientDisconnect listener = playerSlot => RunGuarded(() => handler(playerSlot));
        Plugin.RegisterListener(listener);
        _unhookActions.Add(() => Plugin.RemoveListener(listener));
    }

    protected void RunNextFrame(Action callback)
    {
        Server.NextFrame(() => RunGuarded(callback));
    }

    protected void RunAfter(float delaySeconds, Action callback)
    {
        Plugin.AddTimer(delaySeconds, () => RunGuarded(callback));
    }

    private HookResult RunGuarded(Func<HookResult> handler)
    {
        if (!IsActive)
            return HookResult.Continue;

        try
        {
            return handler();
        }
        catch (Exception exception)
        {
            DisableOnError(exception);
            return HookResult.Continue;
        }
    }

    private void RunGuarded(Action callback)
    {
        if (!IsActive)
            return;

        try
        {
            callback();
        }
        catch (Exception exception)
        {
            DisableOnError(exception);
        }
    }

    private void DisableOnError(Exception exception)
    {
        if (!IsActive)
            return;

        IsActive = false;
        Logger.LogError(LogCategory, $"Disabled after an unexpected error: {exception}");

        // NOTE: Deferred because unhooking from inside a running hook invalidates the engine's callback list
        Server.NextFrame(ReleaseResources);
    }

    private void ReleaseResources()
    {
        OnDisable();

        foreach (var unhook in _unhookActions)
            unhook();

        _unhookActions.Clear();
    }
}
