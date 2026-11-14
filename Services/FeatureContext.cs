using CounterStrikeSharp.API.Core;

using SpamBlocker.Configs;

namespace SpamBlocker.Services;

public sealed record FeatureContext(BasePlugin Plugin, BaseConfig Config);
