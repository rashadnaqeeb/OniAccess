# OniAccess - Claude Code Instructions

## Build

Always use the build script, never `dotnet build` directly. The project references game assemblies via the `ONI_MANAGED` environment variable which the build script sets automatically.

```
powershell -ExecutionPolicy Bypass -File build.ps1
```

The script builds the DLL, deploys it to the game's local mods directory, and patches mods.json to keep the mod enabled.

LSP diagnostics about missing types (KScreen, KButton, KToggle, etc.) are expected; the language server cannot resolve game assembly references. Ignore these; use the build script to verify compilation.

## Project Structure

- `OniAccess/` - mod source code (C#, .NET Framework 4.7.2, Harmony patches)
- `ONI-Decompiled/` - decompiled game source for reference (read-only, not part of build)
- `docs/` - design documentation
- `docs/CODEBASE_INDEX.md` - complete namespace reference for decompiled ONI source
- `.planning/` - project planning files

## Code Style
- **Tabs for indentation**, never spaces
- **K&R braces** - opening brace on the same line, not Allman style. `} else {` not `}\nelse\n{`
- **Edit discipline** - always Read the exact lines immediately before editing. Never compose old_string from memory or earlier reads; tab depth is easy to miscount
- Harmony patch classes: `GameType_MethodName_Patch` (e.g., `KScreen_Activate_Patch`)
- All speech goes through `SpeechPipeline`, never call `SpeechEngine.Say()` directly
- All logging goes through `Log.Info/Debug/Warn/Error`, never use `Debug.Log` directly

## Test

```
powershell -ExecutionPolicy Bypass -File test.ps1
```

Builds and runs the offline test suite (`OniAccess.Tests`). Tests run without the game. All new tests must work offline — never add tests that require launching the game. Test base classes and major systems (e.g., handler stack, input routing, speech pipeline), not individual screen handlers.

## Project Rules

### Reuse game data, avoid hardcoding
Use the game's localized text (`STRINGS` namespace, `LocText` components), UI state, and entity data wherever possible. Hardcoded text becomes stale across game updates and blocks translation. Only hardcode when no game data source exists. Avoid caching for the same reason; stale data is worse than a re-query.

### Game strings first, OniAccessStrings.cs second
Before creating a new `LocString` in `OniAccessStrings.cs`, search the game's `STRINGS` namespace (see `docs/CODEBASE_INDEX.md` and `ONI-Decompiled/`) for existing localized text that conveys the same meaning. The game already has strings for common labels like "Embark", "Close", "Cancel", etc. Only add to `OniAccessStrings.cs` when no game string exists or the mod needs text with no game equivalent (e.g., screen reader instructions, mod-specific labels). Every mod-authored string is a translation burden and a divergence from the game's own wording.

### No inline string literals
All user-facing text must come from a `LocString` reference, either the game's `STRINGS` namespace or `STRINGS.ONIACCESS` in `OniAccessStrings.cs`. Never inline string literals for text that gets spoken or displayed.

### Concise announcements
Users are experienced screen reader users. Strip fluff, never strip information. These rules apply to mod-authored text only; never alter, truncate, or reword game text. Ordering rules (variation-first, below) apply to both.
- No item counts ("3 of 10")
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

## Architecture Gotchas
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
Excessive validation hides bugs. Let code crash to find edge cases.

**WRONG**:
```csharp
public List ProcessSignals(Entity entity) {
	if (entity == null) return new List();
	var controller = entity.GetControlBehavior();
	if (controller == null) return new List();
	var sections = controller.Sections;
	if (sections == null) return new List();
	foreach (var section in sections) {
		int count = section?.FiltersCount ?? 0;
		for (int i = 0; i < count; i++) {
			var slot = section.GetSlot(i);
			if (slot?.Value != null) {
				// process slot
			}
		}
	}
}
```

This hides bugs. Silent empty returns mask null entities, missing controllers, and missing data. `section?.FiltersCount ?? 0` and `slot?.Value` silently skip unexpected nulls.

**CORRECT**:
```csharp
public List ProcessSignals(Entity entity) {
	var controller = entity.GetControlBehavior();
	foreach (var section in controller.Sections) {
		for (int i = 0; i < section.FiltersCount; i++) {
			var slot = section.GetSlot(i);
			if (slot.Value != null) { // Only check what's expected to be null
				// process slot
			}
		}
	}
}
```

**When to validate:** public API boundaries, UI entry points, legitimately nullable values.
**When NOT to validate:** private/internal methods, properties that should always exist, returning empty/default silently.

### Null-conditional operator abuse
Do **NOT** use `?.` to avoid thinking about whether something *should* be null. Only use it where null is a legitimate, expected state.

**WRONG**: `var name = entity?.GetController()?.Sections?.FirstOrDefault()?.Name ?? "default";`
**CORRECT**: `var name = entity.GetController().Sections.FirstOrDefault()?.Name ?? "default";`

Only guard after `FirstOrDefault()` which legitimately returns null. Everything before it should crash if broken.

### Access modifiers as validation boundaries
Public methods are your contract with the outside world; validate there. Private and internal methods are your own territory; trust the caller. If you're adding null checks in a `private` method, the real fix is ensuring the calling code never passes null.
