# OniAccess - Claude Code Instructions

OniAccess is an accessibility mod for Oxygen Not Included that makes the game playable for blind users. It uses Harmony patches to hook into the game's UI and provides speech output as the sole interface — there is no visual fallback. Every decision should be weighed against the fact that if something fails silently or speaks stale data, the player has no way to know.

## Build

Always use the build script, never `dotnet build` directly. The project references game assemblies via the `ONI_MANAGED` environment variable which the build script sets automatically.

```
powershell -ExecutionPolicy Bypass -File build.ps1
```

The script builds the DLL, deploys it to the game's local mods directory, and patches mods.json to keep the mod enabled.

LSP diagnostics about missing types (KScreen, KButton, KToggle, etc.) are expected; the language server cannot resolve game assembly references. Ignore these; use the build script to verify compilation.

When a build fails on a type or method signature, look it up in `ONI-Decompiled/` before guessing at fixes.

## Project Structure

- `OniAccess/` - mod source code (C#, .NET Framework 4.7.2, Harmony patches)
- `ONI-Decompiled/` - decompiled game source for reference (read-only, not part of build)
- `docs/` - design documentation
- `docs/CODEBASE_INDEX.md` - complete namespace reference for decompiled ONI source
- `.planning/` - project planning files

## Code Style

- Harmony patch classes: `GameType_MethodName_Patch` (e.g., `KScreen_Activate_Patch`)
- All speech goes through `SpeechPipeline`, never call `SpeechEngine.Say()` directly
- All logging goes through `Log.Info/Debug/Warn/Error`, never use `Debug.Log` directly

## Test

```
powershell -ExecutionPolicy Bypass -File test.ps1
```

Builds and runs the offline test suite (`OniAccess.Tests`). Tests run without the game. All new tests must work offline — never add tests that require launching the game. Don't test individual screen handlers.

Test systems where bugs hide: algorithmic logic, state machines with multiple transitions, non-obvious side effects (e.g., Pop reactivating the handler underneath), exception safety, and time-dependent behavior. Don't write tests for code you can verify by reading: null guards, flag checks, list indexing, cleanup methods, property accessors. TextFilter-style regression suites are the exception — keep full coverage when the code is a chain of replacements where any change can break unrelated cases.

## Project Rules

### Reuse game data, avoid hardcoding
Use the game's localized text (`STRINGS` namespace, `LocText` components), UI state, and entity data wherever possible. Hardcoded text becomes stale across game updates and blocks translation. Only hardcode when no game data source exists.

### Never cache game state
Do not copy game data into mod-side dictionaries, lists, or string fields for later use. Always re-query the game when you need a value. A sighted player can see when the screen contradicts itself; a blind player trusts speech absolutely. Stale data is worse than no data. The only acceptable "cache" is holding a reference to a live Unity component (e.g., a `KSlider` or `LocText`) and reading its properties at speech time.

### Game strings first, OniAccessStrings.cs second
Before creating a new `LocString` in `OniAccessStrings.cs`, search the game's `STRINGS` namespace (see `docs/CODEBASE_INDEX.md` and `ONI-Decompiled/`) for existing localized text that conveys the same meaning. The game already has strings for common labels like "Embark", "Close", "Cancel", etc. Only add to `OniAccessStrings.cs` when no game string exists or the mod needs text with no game equivalent (e.g., screen reader instructions, mod-specific labels). Every mod-authored string is a translation burden and a divergence from the game's own wording.

### No inline string literals
All user-facing text must come from a `LocString` reference, either the game's `STRINGS` namespace or `STRINGS.ONIACCESS` in `OniAccessStrings.cs`. Never inline string literals for text that gets spoken or displayed.

### Concise announcements
**These rules apply to mod-authored text only; never alter, truncate, or reword game text.** Users are experienced screen reader users. Strip fluff, never strip information.
- No positional item counts ("3 of 10") — the screen reader already tracks list position
- No navigation hints ("press Enter to select") unless unusual controls, and on a delay
- No redundant context ("You are now in...")
- No type suffixes when obvious ("Lumber button")
- DO include all gameplay-relevant details (traits, difficulty, descriptions). Concise means no fluff, not less information
- The sooner a message's varying part appears, the faster the user can keep going. Put the distinguishing word first.
  - WRONG: "cursor anchored" / "cursor unanchored" - user must listen through "cursor" before hearing the difference.
  - CORRECT: "anchored cursor" / "unanchored cursor" - first syllable already differs.
- Avoid emdash. Screen readers announce it as "dash" which breaks the flow of speech

### Conscious hotkey management
ONI has extensive hotkeys. Many are useless to blind players and can be overwritten. But every overwrite is a deliberate decision; document what the original hotkey did and why it's being replaced. See `docs/hotkey-reference.md` for the complete ONI key binding map, safe keys, and screen reader keys to avoid.

### No silent failures
This mod runs on Harmony patches and reflection. Both fail in ways that produce no visible error unless we log it. A swallowed exception in a patch means the feature silently stops working and the user has no idea why. **Every catch block must log via `Log.Warn` or `Log.Error`.** Never write an empty catch, never catch-and-return-default without logging. If something fails, the player log must say what and where. A logged failure is actionable; a silent one is invisible.

## Architecture Gotchas
- **Edit discipline** - always Read the exact lines immediately before editing. Never compose old_string from memory or earlier reads; tab depth is easy to miscount. Working tree files use CRLF on Windows (`core.autocrlf=true`, `.gitattributes: * text=auto`); the Edit tool matches bytes exactly, so stale reads will fail on line endings too
- New screen handlers must be registered in `ContextDetector.RegisterMenuHandlers()` or they will never activate
- Key detection goes in `Tick()` via `UnityEngine.Input.GetKeyDown()`. `HandleKeyDown()` is only for Escape interception through KButtonEvent
- `UnityEngine.Input` must be fully qualified inside the `OniAccess.Input` namespace. Bare `Input` resolves to the namespace, not the Unity class

## Game Log

The Unity player log is at `C:\Users\rasha\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`. Lines prefixed with `[OniAccess]` are mod debug output.

## Common LLM Antipatterns

### Comments referring to what changed
Comments should describe the current state, not the change history. Consider whether a comment is needed at all.

**WRONG**: `// Removed the old UI system. Now x does y.`
**WRONG**: `// Changed to use controllers. Now handles force_close`
**CORRECT**: `// Can be closed with the controller`

### Redundant null checks
Before adding a null check, consider whether nullability has already been established by a caller or earlier in the method. Don't re-check what's already guaranteed.

### Defensive coding
Excessive validation hides bugs. Let code crash to find edge cases. (The flip side: when you *do* catch, always log — see "No silent failures" above.)

**WRONG** — null-checking every intermediate step and silently returning empty:
```csharp
if (entity == null) return new List();
var controller = entity.GetControlBehavior();
if (controller == null) return new List();
```

**CORRECT** — only guard what's legitimately nullable:
```csharp
var controller = entity.GetControlBehavior();
foreach (var section in controller.Sections) {
	var slot = section.GetSlot(i);
	if (slot.Value != null) { // Only check what's expected to be null
		// process slot
	}
}
```

Validate at public API boundaries and UI entry points. Trust private callers.

### Null-conditional operator abuse
Do **NOT** use `?.` to avoid thinking about whether something *should* be null. Only use it where null is a legitimate, expected state.

**WRONG**: `var name = entity?.GetController()?.Sections?.FirstOrDefault()?.Name ?? "default";`
**CORRECT**: `var name = entity.GetController().Sections.FirstOrDefault()?.Name ?? "default";`

Only guard after `FirstOrDefault()` which legitimately returns null. Everything before it should crash if broken.

### Padding and false balance
Don't invent concerns to appear thorough. If there are no problems, say "no issues." Don't present two options as equally valid out of fairness when one is clearly better — just recommend the better one.
