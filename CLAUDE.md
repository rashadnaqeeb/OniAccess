# OniAccess - Claude Code Instructions

## Build

Always use the build script, never `dotnet build` directly. The project references game assemblies via the `ONI_MANAGED` environment variable which the build script sets automatically.

```
powershell -ExecutionPolicy Bypass -File build.ps1
```

The script builds the DLL, deploys it to the game's local mods directory, and patches mods.json to keep the mod enabled.

LSP diagnostics about missing types (KScreen, KButton, KToggle, etc.) are expected — the language server cannot resolve game assembly references. Ignore these; use the build script to verify compilation.

## Project Structure

- `OniAccess/` — mod source code (C#, .NET Framework 4.7.2, Harmony patches)
- `ONI-Decompiled/` — decompiled game source for reference (read-only, not part of build)
- `docs/` — design documentation
- `docs/CODEBASE_INDEX.md` — complete namespace reference for decompiled ONI source
- `.planning/` — project planning files

## Code Style
- **Tabs for indentation** — never spaces
- **K&R braces** — opening brace on the same line, not Allman style. `} else {` not `}\nelse\n{`
- **Edit discipline** — always Read the exact lines immediately before editing. Never compose old_string from memory or earlier reads — tab depth is easy to miscount
- Harmony patch classes: `GameType_MethodName_Patch` (e.g., `KScreen_Activate_Patch`)
- All speech goes through `SpeechPipeline` — never call `SpeechEngine.Say()` directly
- All logging goes through `Log.Info/Debug/Warn/Error` — never use `Debug.Log` directly

## Test

```
powershell -ExecutionPolicy Bypass -File test.ps1
```

Builds and runs the offline test suite (`OniAccess.Tests`). Tests run without the game. All new tests must work offline — never add tests that require launching the game. Test base classes and major systems (e.g., handler stack, input routing, speech pipeline), not individual screen handlers.

## Project Rules

### Reuse game data, avoid hardcoding
Use the game's localized text (`STRINGS` namespace, `LocText` components), UI state, and entity data wherever possible. Hardcoded text becomes stale across game updates and blocks translation. Only hardcode when no game data source exists. Avoid caching for the same reason — stale data is worse than a re-query.

### Mod-authored strings go in OniAccessStrings.cs
All text the mod speaks or displays must be a `LocString` field in `OniAccessStrings.cs` under the `STRINGS.ONIACCESS` hierarchy (e.g., `STRINGS.ONIACCESS.HANDLERS.MAIN_MENU`). Never inline string literals for user-facing text.

### Concise announcements
Users are experienced screen reader users. Announce name, state, value — nothing more.
- No item counts ("3 of 10")
- No navigation hints ("press Enter to select") unless unusual controls, and on a delay
- No redundant context ("You are now in...")
- No type suffixes when obvious ("Lumber button")

### Conscious hotkey management
ONI has extensive hotkeys. Many are useless to blind players and can be overwritten. But every overwrite is a deliberate decision — document what the original hotkey did and why it's being replaced. See `docs/hotkey-reference.md` for the complete ONI key binding map, safe keys, and screen reader keys to avoid.

## Architecture Gotchas
- New screen handlers must be registered in `ContextDetector.RegisterMenuHandlers()` or they will never activate
- Key detection goes in `Tick()` via `UnityEngine.Input.GetKeyDown()` — `HandleKeyDown()` is only for Escape interception through KButtonEvent
- `UnityEngine.Input` must be fully qualified inside the `OniAccess.Input` namespace — bare `Input` resolves to the namespace, not the Unity class

## Game Log

The Unity player log is at `C:\Users\rasha\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`. Lines prefixed with `[OniAccess]` are mod debug output.
