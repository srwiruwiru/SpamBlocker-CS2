using System.Text.RegularExpressions;

using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers.Filters;

public sealed class WordFilter(WordFilterConfig config) : IContentFilter
{
    private const string LogCategory = "WordFilter";

    private readonly StringComparison _comparison = config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    private readonly RegexOptions _regexOptions = config.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

    public FilterResult Check(string content)
    {
        if (!config.Enabled)
            return FilterResult.Allowed;

        foreach (var word in config.BlacklistedWords)
        {
            if (string.IsNullOrWhiteSpace(word) || !ContainsWord(content, word))
                continue;

            Logger.LogDebug(LogCategory, $"Blacklisted word '{word}' detected");
            return FilterResult.Blocked(ViolationType.BlacklistedWord, BlockReason.BlacklistedWord, word);
        }

        return FilterResult.Allowed;
    }

    private bool ContainsWord(string content, string word)
    {
        return config.WholeWordOnly
            ? RegexUtils.IsWholeWordMatch(content, word, _regexOptions)
            : content.Contains(word, _comparison);
    }
}
