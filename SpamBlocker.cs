using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

using SpamBlocker.Configs;
using SpamBlocker.Managers;
using SpamBlocker.Services;
using SpamBlocker.Services.ChatProtection;
using SpamBlocker.Services.DiscordLogging;
using SpamBlocker.Services.NameProtection;

namespace SpamBlocker;

[MinimumApiVersion(374)]
public class SpamBlocker : BasePlugin, IPluginConfig<BaseConfig>
{
    private const string LogCategory = "Core";

    public override string ModuleName => "SpamBlocker";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Advanced spam and content filtering plugin with Discord integration";

    public BaseConfig Config { get; set; } = new();

    private readonly List<FeatureService> _features = [];
    private NameProtectionService? _nameProtection;

    public void OnConfigParsed(BaseConfig config)
    {
        Config = config;
        Utils.Logger.Config = config;
    }

    public override void Load(bool hotReload)
    {
        CreateFeatures();

        if (hotReload)
            _nameProtection?.ScheduleExistingPlayersCheck();

        Utils.Logger.LogInfo(LogCategory, "Loaded");
    }

    public override void Unload(bool hotReload)
    {
        foreach (var feature in _features)
            feature.Disable();

        _features.Clear();
        _nameProtection = null;
        Utils.Logger.LogInfo(LogCategory, "Unloaded");
    }

    private void CreateFeatures()
    {
        var context = new FeatureContext(this, Config);
        var discordLogging = new DiscordLoggingService(context);
        var violationReporter = new ViolationReporter(Config, discordLogging);
        var filterManager = new FilterManager(Config);
        var chatProtection = new ChatProtectionService(context, filterManager, violationReporter);
        _nameProtection = new NameProtectionService(context, filterManager, violationReporter);
        _features.AddRange([discordLogging, chatProtection, _nameProtection]);

        if (discordLogging.ShouldEnable())
            discordLogging.Enable();

        if (Config.ChatProtection.Enabled)
            chatProtection.Enable();

        if (Config.NameProtection.Enabled)
            _nameProtection.Enable();
    }
}
