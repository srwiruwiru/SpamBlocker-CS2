using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers.Filters;

public sealed class NameBlacklistFilter(NameBlacklistConfig config)
{
    private const string LogCategory = "NameBlacklistFilter";

    private readonly StringComparison _comparison = config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    public FilterResult Check(string playerName)
    {
        if (!config.Enabled)
            return FilterResult.Allowed;

        if (playerName.Length < config.MinNameLength)
            return Block(BlockReason.NameTooShort, playerName);

        if (playerName.Length > config.MaxNameLength)
            return Block(BlockReason.NameTooLong, playerName);

        var blacklistedName = FindBlacklistedName(playerName.Trim());
        return blacklistedName == null ? FilterResult.Allowed : Block(BlockReason.BlacklistedName, blacklistedName);
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

    private static FilterResult Block(BlockReason reason, string detectedContent)
    {
        Logger.LogDebug(LogCategory, $"'{detectedContent}' blocked: {reason}");
        return FilterResult.Blocked(ViolationType.BlacklistedName, reason, detectedContent);
    }
}
