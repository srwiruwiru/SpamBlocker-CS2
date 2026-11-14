using SpamBlocker.Models;

namespace SpamBlocker.Services.ChatProtection;

public sealed class ViolationCooldown
{
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromSeconds(5);

    private readonly Dictionary<int, (ViolationSource Source, DateTime Time)> _lastViolations = [];

    public bool IsInCooldown(int playerSlot, ViolationSource source)
    {
        if (!_lastViolations.TryGetValue(playerSlot, out var lastViolation))
            return false;

        return lastViolation.Source == source && DateTime.UtcNow - lastViolation.Time < CooldownDuration;
    }

    public void StartCooldown(int playerSlot, ViolationSource source)
    {
        _lastViolations[playerSlot] = (source, DateTime.UtcNow);
    }

    public void RemovePlayer(int playerSlot)
    {
        _lastViolations.Remove(playerSlot);
    }

    public void Clear()
    {
        _lastViolations.Clear();
    }
}
