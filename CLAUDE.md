# Rules for CounterStrikeSharp plugins
General rules for any CS2 plugin written in C# with CounterStrikeSharp. The .NET and API versions live in the `.csproj` and in `[MinimumApiVersion]`.

Build: `dotnet build <Project>.csproj -c Release`

## Principles
1. **Simple over clever.** Between two solutions, the one that is understood in a single read wins.
2. **One responsibility per file and per class.** If describing it needs the word "and", split it.
3. **No repeated code.** The second time something similar shows up, extract it into a reusable function or type.
4. **No spaghetti.** Linear flow, early returns and no state mutated from several places.
5. **YAGNI.** No code is written for cases that do not exist today.

## Existing code
- These rules apply to new code and to the code a change touches. Code that does not follow them yet is left alone.
- Bringing old code up to these rules is only done when explicitly requested, in its own `refactor` commits, never mixed with a feature or a fix.
- `FeatureService` and `Utils.Logger` are the target names. Where the project does not have them yet, use its existing equivalent and do not create them unless asked.

## Files
- Maximum **300 lines** per file. When a file gets close to the limit, split it by responsibility, not by size.
- One type per file, and the file is named after the type. Only exception: the config files, which are named after their role (see below).
- File-scoped namespace that mirrors the folder: `namespace <Plugin>.Services;`.
- Respect the project's existing formatting (indentation, BOM, line endings, `using` order). Never reformat a file that is not part of the change.

## Structure
| Location | Contains | Does not contain |
|---|---|---|
| `<Plugin>.cs` | Entry point: creates the features enabled in the config, wires them to the lifecycle and delegates | Feature logic |
| `Configs/` | Base config and one file per section | Logic |
| `Models/` | `record`s, enums and constants shared between features | Game access |
| `Managers/` | Shared plugin state (players, teams, match, timers) queried by several features | Logic of a single feature |
| `Commands/` | One command per class (`Admin/`, `Player/`): validates the caller and delegates to managers or services | Business logic |
| `Services/` | One feature per service. If it needs several files: `Services/<Feature>/` | Code of another feature |
| `Utils/` | Static, pure, stateless helpers (except the `Logger`) | Feature state |
| `lang/` | One JSON file per language with every player-facing text | Keys missing in another language |

## Configuration
- The config is the only way to enable or disable features. No console commands are created unless explicitly requested.
- The config is split by type so every group of options is easy to find:
  - **Base config** (`Configs/BaseConfig.cs`, class `BaseConfig`): holds the list of sections, plugin-wide standalone options, `EnableDebug` and `ConfigVersion`. It never declares a section class.
  - **Sections** (`Configs/<Section>.cs`, class `<Section>Config`): every group of configurable options lives in its own file, one section per file.
- Plain value lists (`List<string>` of words, domains, IPs) are regular options and stay in the config they belong to. What always moves out of the base config is a class that groups configurable options.
- Order inside `BaseConfig`: sections first, then plugin-wide options, and always last `EnableDebug` followed by `ConfigVersion`.

```csharp
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace <Plugin>.Configs;

public sealed class BaseConfig : BasePluginConfig
{
    [JsonPropertyName("Chat")]
    public ChatConfig Chat { get; set; } = new();

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}
```

- Always accessed as `Config.<Section>.<Option>` (`Config.Chat.Enabled`).
- Every property has `[JsonPropertyName]` and an explicit default value.
- New JSON keys are `PascalCase` (`"ConfigVersion"`, `"Output"`).
- Existing keys keep their name: renaming one breaks the configs already deployed on servers, so it is only done when explicitly requested.
- Adding, renaming or removing options bumps `ConfigVersion`.

## Localization
- No player-facing text is written in the code: every message goes through `Localizer` with a key from `lang/`.
- A new key is added to **every** language file in the same commit. If a translation is unknown, the English text is used and marked for review in the commit body.
- Log messages are not localized: they are always in English.

## Functions
- The name is verb + object and says exactly what it does: `ShowTracersOnlyToShooter`, `RestoreOriginalSkies`. If it needs "And", it is two functions.
- `Try…` returns `bool` with `out`; `Find…` returns `null` when nothing is found. Booleans start with `Is`, `Has`, `Can` or `Should`.
- Short: ideally under 20 lines, never more than 40. A single level of abstraction per function.
- Maximum 3 parameters; if more are needed, group them in a `record`.
- No `bool` parameters that change behavior: create two functions, each with its own name.
- Early returns (guard clauses) and at most 2 levels of nesting.
- No hidden effects: a `Get…`, `Find…` or `Is…` function never modifies state.

## Naming
- Identifiers, log messages and comments in English.
- `PascalCase` for types, methods, properties and constants; `_camelCase` for private fields; `camelCase` for locals and parameters.
- No abbreviations or generic names (`flag`, `num`, `data`, `tmp`, `obj`, `Helper`).
- `Manager` is never a name on its own; it is only allowed as the suffix of something specific (`FilterManager`).
- No magic numbers or strings: `private const int FireBulletsMessageId = 452;`.

## Comments
The code explains itself. Before commenting, try renaming or extracting a function. Only comment what the code cannot express, always with one of these **Better Comments** tags, written exactly as shown (uppercase, trailing colon):

| Tag | Use |
|---|---|
| `// TODO:` | Concrete pending work |
| `// FIXME:` | Something that works wrong and must be fixed |
| `// BUG:` | Known bug with no fix yet |
| `// HACK:` | Temporary or inelegant workaround, with the reason |
| `// NOTE:` | The why behind a non-obvious decision (engine, protocol, offsets) |
| `// INFO:` | Useful context or external reference |
| `// IDEA:` | Possible improvement, no commitment |

```csharp
// NOTE: Deferred because unhooking from inside a running hook invalidates the engine's callback list
Server.NextFrame(Stop);
```

- Commented-out code is forbidden: that is what git is for.
- No XML comments (`///`), banners or section separators.

## Features and CounterStrikeSharp
- Every feature inherits from a common base (`FeatureService`) that centralizes enabling, disabling and self-disabling on error. It is not reinvented in each service.
- When disabled (plugin unload or reload), a feature **restores** everything it changed: ConVars, entities, hooks and listeners.
- Never keep an entity between frames: keep its handle (`EntityHandle.Raw`), then resolve it and check `IsValid` when using it.
- Deferred callbacks (`Server.NextFrame`, `AddTimer`) check that the feature is still active before acting.
- No exception escapes a hook: it is caught at the edge, the feature disables itself and it is logged **only once**.
- Never unregister a hook from inside its own execution; defer it with `Server.NextFrame`.
- In high-frequency hooks (sounds, shots, movement): no LINQ, no closures, no avoidable allocations and no per-call logs.
- The game API is only touched from the main thread. After an `await` (HTTP, database, files), return with `Server.NextFrame` before using players, entities or ConVars.
- Never block the main thread: no `.Result`, `.Wait()` or `Thread.Sleep`. Slow work (webhooks, queries) runs async and its exceptions are caught and logged.
- Prefer the CounterStrikeSharp API over custom signatures or offsets. If there is no alternative, isolate them in a single file.
- Every log goes through `Utils.Logger`, never through `Console` directly.
- Third-party code (especially decompiled code) is only used to understand behavior: it is rewritten from scratch and never copied.

## Logger
`Utils/Logger.cs` is always this exact file, only replacing `<Plugin>`. It is the only place allowed to write to `Console`.

```csharp
using <Plugin>.Configs;

namespace <Plugin>.Utils;

public static class Logger
{
    private const string PluginName = "<Plugin>";
    public static BaseConfig? Config { get; set; }

    public static void LogInfo(string category, string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{PluginName} - {category}] {message}");
        Console.ResetColor();
    }

    public static void LogWarning(string category, string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{PluginName} - {category}] {message}");
        Console.ResetColor();
    }

    public static void LogError(string category, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{PluginName} - {category}] {message}");
        Console.ResetColor();
    }

    public static void LogDebug(string category, string message)
    {
        if (Config?.EnableDebug != true) return;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[{PluginName} - {category}] {message}");
        Console.ResetColor();
    }
}
```

- `Logger.Config` is assigned in `OnConfigParsed`, so `LogDebug` follows `EnableDebug`.
- `category` is the feature or area that logs (`"Core"`, `"ChatProtection"`), kept in a `private const string LogCategory` of each class.

## Commits
- [Conventional Commits](https://www.conventionalcommits.org) format: `type(scope): description`.

| Type | Use |
|---|---|
| `feat` | New feature or extension of an existing one |
| `fix` | Bug fix |
| `refactor` | Code change with no behavior change |
| `perf` | Performance improvement |
| `style` | Formatting only, no logic changes |
| `docs` | Documentation and rules |
| `build` | `.csproj`, dependencies and build |
| `ci` | GitHub Actions workflows |
| `chore` | Maintenance that fits none of the above |

- The scope is the affected feature or area, in kebab-case: `feat(black-sky):`, `fix(own-bullet-tracers):`, `refactor(config):`. `core` is the scope of the plugin core (entry point, common base, logger): `fix(core):`.
- Description in English, imperative, lowercase, no trailing period and at most 72 characters: `feat(black-sky): restore original sky on unload`. If the why is not obvious, explain it in the body.
- A commit is a single logical change. **It never mixes changes from two features**: when working on two at once, each one gets its own commits. Shared code (common base, base config) goes in its own commit before the features that use it.
- Commit as the work progresses, every time a coherent unit is finished, without waiting for the end of the task. Every commit builds on its own.
- Never commit `bin/`, `obj/`, `.superpowers/`, secrets or local configs.
- **Plans and specs (`docs/superpowers/`) are never committed without explicit authorization**, every time. Write them, present them and wait for the yes before committing.
- **Pushing always requires explicit confirmation**, every time. Never `push --force` and never rewrite already published commits.
- A change that breaks compatibility for server owners is marked with `!` and a `BREAKING CHANGE:` footer: `feat(config)!: move debug toggle to root EnableDebug`.

## Versioning
- [Semantic Versioning](https://semver.org): `MAJOR.MINOR.PATCH`. `ModuleVersion` in `<Plugin>.cs` is the only place that holds the version and always matches the latest release.
- The version is decided from the commits since the last release, from the server owner's point of view:

| Bump | When |
|---|---|
| `MAJOR` | Anything a server owner must react to: a config key removed, renamed or with a new meaning, a removed command or permission, a higher minimum .NET or CounterStrikeSharp version, a behavior change of an existing option |
| `MINOR` | New backwards-compatible features or options whose default keeps the previous behavior |
| `PATCH` | Fixes and internal changes that need no action from the server owner |

- The version is only bumped when preparing a release, in its own commit: `chore(release): bump version to 2.0.0`. Feature and fix commits never touch it.
- Releases are named `<Plugin> vX.Y.Z`, keep the project's existing tag convention, and list breaking changes first. Creating a release publishes it, so it always requires explicit confirmation.

## Before considering a change done
- [ ] Builds with no errors or warnings.
- [ ] No file exceeds 300 lines.
- [ ] No duplicated logic and no unnecessary comments.
- [ ] The formatting of the touched files is the same as before.
- [ ] The feature is enabled from the config and restores the original state when disabled.
- [ ] Every new player-facing text exists in all the language files.
- [ ] Changes are in separate commits per feature, with the `type(scope): description` format.
