# PauseScreen Handler Bug: No Speech After Loading a Game

## The Bug

After loading a game from the pause menu's Load option, pressing Escape opens the pause menu and our PauseMenuHandler pushes correctly with 9 widgets. But arrow keys produce no speech. The handler is active, navigation is happening internally, but the user hears nothing and concludes the handler is broken.

**Works:** Start game -> Escape -> pause menu works (speech, arrows, everything)
**Breaks:** Start game -> Escape -> pause menu works -> Load Game -> pick save -> game loads -> Escape -> handler pushes, no speech on any key

## Root Cause

`Game.OnDestroy` fires during scene transitions (loading a save), not just on application quit. Our `Game_OnDestroy_Patch` called `SpeechEngine.Shutdown()`, which called `Tolk_Unload()` and set `_initialized = false`, `_available = false`. After the new scene loaded, every `SpeechEngine.Say()` call hit the `if (!_available) return` guard and silently returned.

The handler stack, KeyPoller (DontDestroyOnLoad), ModInputRouter, and widget discovery all survived the scene transition correctly. Only speech was dead.

## The Fix

Guard `Game_OnDestroy_Patch` so it only shuts down speech on actual application quit, not scene transitions. Since `Application.isQuitting` isn't available in this Unity build (2022.3.62f2 with .NET 4.7.2), the patch body was emptied entirely. Tolk cleanup is handled by OS process exit.

## What the Log Showed (run 3, stack trace + frame count)

### Pre-load (working correctly)
- Frame 469: `OnShow(true)` -> handler pushed, speech works
- Frame 496: `OnShow(false)` via `PauseScreen.OnKeyDown` -> user pressed Escape to close (27 frames later)
- Frame 804: `OnShow(true)` -> handler pushed again, speech works
- Frame 839: `OnShow(false)` via `PauseScreen.OnKeyDown` -> user pressed Escape to close (35 frames later)

### Scene transition
- `Game.OnDestroy` fires -> `SpeechEngine.Shutdown()` -> Tolk unloaded, `_available = false`
- New scene loads, new PauseScreen created
- Frame 3437: `OnShow(false)` from `PauseScreen.OnPrefabInit` (expected, harmless)

### Post-load (broken)
- Frame 3538: `OnShow(true)` -> handler pushed, 9 widgets discovered, `OnActivate()` tries to speak but `SpeechEngine.Say()` returns early
- User presses arrows -> `NavigateNext()` -> `SpeakCurrentWidget()` -> `SpeechPipeline.SpeakInterrupt()` -> `SpeechEngine.Say()` -> `!_available` -> no speech
- Frame 3577: `OnShow(false)` via `PauseScreen.OnKeyDown` -> user pressed Escape to close after hearing nothing (39 frames later)

### Key evidence
All three `OnShow(false)` calls during gameplay had identical stack traces through `PauseScreen.OnKeyDown` -> `KScreenManager.OnKeyDown` -> normal input dispatch chain. The frame gaps (27, 35, 39) were all user-initiated Escape presses. There was never a spurious second Escape event.

## What Survived Scene Reload

- **KeyPoller** -- `DontDestroyOnLoad`, runs throughout. Proved by stale handler detection in log.
- **ModInputRouter** -- Registered once in `InputInit.Awake` (which only fires once). The KInputHandler tree persists across scenes since InputInit uses DontDestroyOnLoad.
- **HandlerStack** -- Static class, persists. Stale handlers from old scene cleaned up by KeyPoller.
- **VanillaMode.IsEnabled** -- Static bool, stays true.
- **ContextDetector registry** -- Static dictionary, populated once in `Mod.OnLoad`.

## What Did NOT Survive

- **SpeechEngine state** -- `Game_OnDestroy_Patch` called `Shutdown()`, setting `_initialized = false` and `_available = false`. `Initialize()` is only called once in `Mod.OnLoad`.

## Wrong Theories (Don't Repeat)

1. **Held Escape key timing** -- User says it reliably works before loading, reliably breaks after. Not a timing issue.
2. **HandleKeyDown consuming Escape** -- Was actually CAUSING the close in tests where it was present.
3. **Frame guard on HandleKeyDown** -- Didn't help because the issue was never about the close.
4. **Harmony prefix on PauseScreen.OnKeyDown with frame guard** -- The prefix never fired. Irrelevant to the actual bug.
5. **Same-frame input dispatch / stale events** -- The close events were all user-initiated Escape presses, not spurious events.
6. **ModInputRouter lost after scene reload** -- InputInit.Awake only fires once; the input handler tree persists. ModInputRouter was never lost.
7. **Handler not responding to input** -- The handler was receiving Tick() calls and processing keys. Navigation was happening. Speech was dead.

## Previous Fixes That ARE Correct

1. **DiscoverWidgets Field fix** (commit 6575fc5): Changed `Traverse.Property("text")` to `Traverse.Field("text")` for ButtonInfo.text.
2. **Widget invalidation detection** (commit 6575fc5): Added check in BaseMenuHandler.Tick() for destroyed widget references after RefreshButtons.
3. **OnShow patch instead of Show patch** (commit 9612154): PauseScreen overrides OnShow, not Show.

## Key Files

- `OniAccess/Patches/GameLifecyclePatches.cs` -- Game_OnDestroy_Patch (the fix)
- `OniAccess/Speech/SpeechEngine.cs` -- Initialize/Shutdown/Say with _available guard
- `OniAccess/Speech/SpeechPipeline.cs` -- SpeakInterrupt/SpeakQueued -> SpeechEngine.Say
- `OniAccess/Input/KeyPoller.cs` -- DontDestroyOnLoad MonoBehaviour driving Tick()
- `OniAccess/Mod.cs` -- OnLoad: SpeechEngine.Initialize(), KeyPoller creation, handler registration
