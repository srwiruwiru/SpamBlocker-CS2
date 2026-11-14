using System.Text.RegularExpressions;

using SpamBlocker.Configs;
using SpamBlocker.Models;
using SpamBlocker.Utils;

namespace SpamBlocker.Managers.Filters;

public sealed class IpFilter(IpFilterConfig config) : IContentFilter
{
    private const string LogCategory = "IpFilter";
    private const char PortSeparator = ':';

    public FilterResult Check(string content)
    {
        if (!config.Enabled)
            return FilterResult.Allowed;

        var ipPortMatches = RegexUtils.IpPortRegex.Matches(content);
        var blockedIpPort = FindBlockedIpPort(ipPortMatches);
        if (blockedIpPort != null)
            return Block(BlockReason.IpPortNotWhitelisted, blockedIpPort);

        var blockedIp = FindBlockedIp(content, ipPortMatches);
        return blockedIp == null ? FilterResult.Allowed : Block(BlockReason.IpNotWhitelisted, blockedIp);
    }

    private string? FindBlockedIpPort(MatchCollection ipPortMatches)
    {
        foreach (Match match in ipPortMatches)
        {
            var ip = match.Value.Split(PortSeparator)[0];
            if (ValidationUtils.IsValidIpAddress(ip) && !IsWhitelisted(match.Value, config.WhitelistIpPorts))
                return match.Value;
        }

        return null;
    }

    private string? FindBlockedIp(string content, MatchCollection ipPortMatches)
    {
        foreach (Match match in RegexUtils.IpRegex.Matches(content))
        {
            var ip = match.Value;
            if (IsPartOfIpPort(ip, ipPortMatches) || !ValidationUtils.IsValidIpAddress(ip))
                continue;

            if (!IsWhitelisted(ip, config.WhitelistIps))
                return ip;
        }

        return null;
    }

    private static bool IsPartOfIpPort(string ip, MatchCollection ipPortMatches)
    {
        foreach (Match match in ipPortMatches)
        {
            if (match.Value.StartsWith(ip + PortSeparator, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static bool IsWhitelisted(string value, List<string> whitelist)
    {
        foreach (var entry in whitelist)
        {
            if (string.Equals(entry.Trim(), value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static FilterResult Block(BlockReason reason, string detectedContent)
    {
        Logger.LogDebug(LogCategory, $"'{detectedContent}' blocked: {reason}");
        return FilterResult.Blocked(ViolationType.BlockedIp, reason, detectedContent);
    }
}
