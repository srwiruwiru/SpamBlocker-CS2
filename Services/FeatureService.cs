using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;

using SpamBlocker.Configs;
using SpamBlocker.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace SpamBlocker.Services;

public abstract class FeatureService(BasePlugin plugin, BaseConfig config)
{
    private readonly List<Action> _unhookActions = [];

    public bool IsActive { get; private set; }

    protected BasePlugin Plugin { get; } = plugin;
    protected BaseConfig Config { get; } = config;
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
        Plugin.RegisterEventHandler(handler);
        _unhookActions.Add(() => Plugin.DeregisterEventHandler(handler));
    }

    protected void HookCommandListener(string command, CommandInfo.CommandListenerCallback handler)
    {
        Plugin.AddCommandListener(command, handler);
        _unhookActions.Add(() => Plugin.RemoveCommandListener(command, handler, HookMode.Pre));
    }

    protected void HookClientDisconnect(Action<int> handler)
    {
        Listeners.OnClientDisconnect listener = playerSlot => handler(playerSlot);
        Plugin.RegisterListener(listener);
        _unhookActions.Add(() => Plugin.RemoveListener(listener));
    }

    protected void RunNextFrame(Action callback)
    {
        Server.NextFrame(callback);
    }

    protected void RunAfter(float delaySeconds, Action callback)
    {
        _ = new Timer(delaySeconds, callback);
    }

    private void ReleaseResources()
    {
        OnDisable();

        foreach (var unhook in _unhookActions)
            unhook();

        _unhookActions.Clear();
    }
}
