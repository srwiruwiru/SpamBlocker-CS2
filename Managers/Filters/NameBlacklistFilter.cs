using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers.Filters;

public sealed class NameBlacklistFilter(NameBlacklistConfig config)
{
    private const string LogCategory = "NameBlacklistFilter";
    private const string NameTooShortReason = "Name too short";
    private const string NameTooLongReason = "Name too long";
    private const string BlacklistedNameReason = "Blacklisted name detected";

    private readonly StringComparison _comparison = config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    public FilterResult Check(string playerName)
    {
        if (!config.Enabled)
            return FilterResult.Allowed;

        if (playerName.Length < config.MinNameLength)
            return Block(NameTooShortReason, playerName);

        if (playerName.Length > config.MaxNameLength)
            return Block(NameTooLongReason, playerName);

        var blacklistedName = FindBlacklistedName(playerName.Trim());
        return blacklistedName == null ? FilterResult.Allowed : Block(BlacklistedNameReason, blacklistedName);
    }

    private string? FindBlacklistedName(string playerName)
    {
        foreach (var blacklistedName in config.BlacklistedNames)
        {
            if (!string.IsNullOrWhiteSpace(blacklistedName) && IsBlacklistedMatch(playerName, blacklistedName))
                return blacklistedName;
        }

        return null;
    }

    private bool IsBlacklistedMatch(string playerName, string blacklistedName)
    {
        if (config.WholeNameOnly || !config.BlockPartialMatches)
            return string.Equals(playerName, blacklistedName, _comparison);

        if (!playerName.Contains(blacklistedName, _comparison))
            return false;

        if (!config.AllowNumbersSuffix)
            return true;

        var nameWithoutNumbers = RegexUtils.TrailingDigitsRegex.Replace(playerName, "");
        return !string.Equals(nameWithoutNumbers, blacklistedName, _comparison);
    }

    private static FilterResult Block(string reason, string detectedContent)
    {
        Logger.LogDebug(LogCategory, $"'{detectedContent}' blocked: {reason}");
        return FilterResult.Blocked(ViolationType.BlacklistedName, reason, detectedContent);
    }
}
