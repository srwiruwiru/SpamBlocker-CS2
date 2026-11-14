using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

using SpamBlocker.Configs;
using SpamBlocker.Managers.Filters;
using SpamBlocker.Models;

namespace SpamBlocker.Managers;

public sealed class FilterManager(BaseConfig config)
{
    private readonly IContentFilter[] _contentFilters =
    [
        new WordFilter(config.WordFilter),
        new IpFilter(config.IpFilter),
        new UrlFilter(config.UrlFilter)
    ];

    private readonly NameBlacklistFilter _nameBlacklistFilter = new(config.NameProtection.NameBlacklist);

    public FilterResult CheckContent(string content, CCSPlayerController player)
    {
        if (string.IsNullOrWhiteSpace(content) || CanBypassFilters(player))
            return FilterResult.Allowed;

        foreach (var filter in _contentFilters)
        {
            var result = filter.Check(content);
            if (result.IsBlocked)
                return result;
        }

        return FilterResult.Allowed;
    }

    public FilterResult CheckPlayerName(string playerName, CCSPlayerController player)
    {
        if (string.IsNullOrWhiteSpace(playerName) || CanBypassFilters(player))
            return FilterResult.Allowed;

        var result = _nameBlacklistFilter.Check(playerName);
        return result.IsBlocked ? result : CheckContent(playerName, player);
    }

    private bool CanBypassFilters(CCSPlayerController player)
    {
        return config.Settings.AdminBypass && AdminManager.PlayerHasPermissions(player, config.Settings.AdminBypassPermission);
    }
}
