using System.Text.RegularExpressions;

using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers.Filters;

public sealed class UrlFilter(UrlFilterConfig config) : IContentFilter
{
    private const string LogCategory = "UrlFilter";
    private const string WhitelistMode = "whitelist";

    public FilterResult Check(string content)
    {
        if (!config.Enabled)
            return FilterResult.Allowed;

        foreach (Match match in RegexUtils.UrlRegex.Matches(content))
        {
            var result = CheckUrl(match.Value);
            if (result.IsBlocked)
                return result;
        }

        return FilterResult.Allowed;
    }

    private FilterResult CheckUrl(string url)
    {
        var domain = ValidationUtils.FindDomain(url);
        if (domain == null)
            return FilterResult.Allowed;

        if (!ValidationUtils.IsAllowedProtocol(url, config.AllowedProtocols))
            return Block(BlockReason.ProtocolNotAllowed, url);

        if (IsWhitelistMode())
            return ValidationUtils.IsInDomainList(domain, config.Whitelist) ? FilterResult.Allowed : Block(BlockReason.DomainNotWhitelisted, url);

        return ValidationUtils.IsInDomainList(domain, config.Blacklist) ? Block(BlockReason.DomainBlacklisted, url) : FilterResult.Allowed;
    }

    private bool IsWhitelistMode()
    {
        return string.Equals(config.FilterMode, WhitelistMode, StringComparison.OrdinalIgnoreCase);
    }

    private static FilterResult Block(BlockReason reason, string url)
    {
        Logger.LogDebug(LogCategory, $"'{url}' blocked: {reason}");
        return FilterResult.Blocked(ViolationType.BlockedUrl, reason, url);
    }
}
