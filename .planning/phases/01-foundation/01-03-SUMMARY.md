---
phase: 01-foundation
plan: 03
subsystem: input
tags: [hotkey-registry, vanilla-mode, input-interception, speech-capture, harmony, unity-monobehaviour, context-sensitive]

# Dependency graph
requires:
  - phase: 01-01
    provides: "SpeechEngine with Tolk P/Invoke and OnSpeechOutput callback hook"
  - phase: 01-02
    provides: "SpeechPipeline with SpeakInterrupt, SpeakQueued, SetEnabled, TextFilter"
provides:
  - "VanillaMode: mod on/off toggle with speech confirmation and SpeechPipeline.SetEnabled"
  - "AccessContext enum: 5 game contexts (Always, Global, WorldView, MenuOpen, BuildMode)"
  - "HotkeyRegistry: context-sensitive binding registration with TryHandle, frame dedup, help text generation"
  - "HotkeyBinding: key+modifier+context+description+handler+originalFunction"
  - "InputInterceptor: MonoBehaviour bridging Unity Input to HotkeyRegistry"
  - "SpeechCapture: Start/Stop/ContainsText/LastSpeechWas test assertion framework"
  - "Game_OnDestroy_Patch: Harmony postfix ensuring Tolk shutdown on game exit"
  - "Phase 1 hotkeys registered: Ctrl+Shift+F12 (toggle), F12 (context help)"
affects: [02-menus, 03-navigation, 04-entity-inspection, 06-notifications]

# Tech tracking
tech-stack:
  added: [unity-input-system, monobehaviour-update, harmony-lifecycle-patch]
  patterns: [context-sensitive-hotkeys, vanilla-mode-toggle, speech-capture-hook, frame-dedup]

key-files:
  created:
    - OniAccess/Input/AccessContext.cs
    - OniAccess/Input/HotkeyRegistry.cs
    - OniAccess/Input/InputInterceptor.cs
    - OniAccess/Toggle/VanillaMode.cs
    - OniAccess/Patches/InputPatches.cs
    - OniAccess/Testing/SpeechCapture.cs
  modified:
    - OniAccess/Mod.cs
    - OniAccess/OniAccessStrings.cs

key-decisions:
  - "MonoBehaviour Update() approach for Phase 1 input interception instead of Harmony KInputHandler patches -- simpler, avoids ONI input priority complexity"
  - "HotkeyModifier as Flags enum (Ctrl=1, Shift=2, Alt=4) for clean combination matching"
  - "Frame dedup via Time.frameCount dictionary prevents multiple fires per frame per binding"
  - "Toggle off speaks confirmation before disabling pipeline -- temporarily re-enables SpeechPipeline to dispatch the message"

patterns-established:
  - "Context-sensitive hotkey pattern: register with AccessContext, TryHandle checks VanillaMode + context match"
  - "MonoBehaviour input bridge: persistent GameObject with DontDestroyOnLoad for frame-by-frame input polling"
  - "Speech capture hook: subscribe to SpeechEngine.OnSpeechOutput for test assertions"
  - "Lifecycle Harmony patch: Game.OnDestroy postfix for native resource cleanup"

# Metrics
duration: 3min
completed: 2026-02-11
---

# Phase 1 Plan 3: Input & Toggle Summary

**VanillaMode toggle with Ctrl+Shift+F12, context-sensitive HotkeyRegistry with frame dedup and help text, InputInterceptor MonoBehaviour, and SpeechCapture test framework**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-11T13:21:06Z
- **Completed:** 2026-02-11T13:24:33Z
- **Tasks:** 2
- **Files created:** 6
- **Files modified:** 2

## Accomplishments
- VanillaMode toggle that speaks "Oni-Access off"/"Oni-Access on" and gates all speech via SpeechPipeline.SetEnabled
- HotkeyRegistry with context-sensitive binding, modifier matching, frame dedup, and help text generation from registered bindings
- InputInterceptor MonoBehaviour polling Unity Input.GetKeyDown in Update() and routing to HotkeyRegistry.TryHandle
- SpeechCapture test framework hooking SpeechEngine.OnSpeechOutput with ContainsText, LastSpeechWas, Count, and LastSpeech assertion helpers
- Game.OnDestroy Harmony postfix ensuring clean Tolk shutdown
- Phase 1 hotkeys registered: Ctrl+Shift+F12 (Always context toggle) and F12 (Global context help)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create VanillaMode toggle and context-sensitive hotkey system** - `778d397` (feat)
2. **Task 2: Create InputInterceptor, Harmony patches, and SpeechCapture** - `fa8dd14` (feat)

## Files Created/Modified
- `OniAccess/Input/AccessContext.cs` - Enum with 5 game contexts for context-sensitive hotkey matching
- `OniAccess/Input/HotkeyRegistry.cs` - Static registry with Register, Unregister, TryHandle, GetHelpText, frame dedup
- `OniAccess/Input/InputInterceptor.cs` - MonoBehaviour polling F12 key with modifier detection, routing to HotkeyRegistry
- `OniAccess/Toggle/VanillaMode.cs` - Static toggle class with IsEnabled property and SpeechPipeline integration
- `OniAccess/Patches/InputPatches.cs` - Harmony postfix on Game.OnDestroy for SpeechEngine.Shutdown
- `OniAccess/Testing/SpeechCapture.cs` - Speech capture buffer with Start/Stop/assertion helpers hooking OnSpeechOutput
- `OniAccess/Mod.cs` - Added RegisterHotkeys, InputInterceptor GameObject creation, SpeakContextHelp handler
- `OniAccess/OniAccessStrings.cs` - Added HELP_HEADER and NO_COMMANDS LocString constants

## Decisions Made
- Used MonoBehaviour Update() for input polling instead of Harmony patching ONI's KInputHandler -- simpler for Phase 1 where only F12 key is needed, future phases can migrate if needed
- Created HotkeyModifier as a Flags enum rather than using ONI's Modifier enum -- keeps mod input system independent from game internals
- Frame dedup uses Dictionary<int, int> tracking binding index to last frame count -- prevents multiple fires per frame per binding (pitfall #3 from research)
- Toggle off message uses temporary re-enable of SpeechPipeline -- ensures the "Oni-Access off" message reaches Tolk before pipeline is disabled

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Build verification with `dotnet build` not possible without ONI game DLLs (expected, same as Plans 01 and 02). Code structure, cross-file references, and all key_links from the plan's must_haves verified manually.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Phase 1 foundation complete: all 3 plans delivered
- HotkeyRegistry ready for Phase 2+ to register navigation, menu, and feature hotkeys
- InputInterceptor will need expansion to check additional keys as they are registered (currently only checks F12)
- SpeechCapture ready for integration tests in future phases
- VanillaMode cleanly gates all mod behavior -- future Harmony patches should check VanillaMode.IsEnabled

## Self-Check: PASSED

- All 6 created files verified on disk
- Commit `778d397` (Task 1) verified in git log
- Commit `fa8dd14` (Task 2) verified in git log
- Key link verified: VanillaMode -> SpeechPipeline.SetEnabled (3 calls)
- Key link verified: Mod.cs -> InputInterceptor.AddComponent
- Key link verified: InputInterceptor -> HotkeyRegistry.TryHandle (1 call)
- Key link verified: SpeechCapture -> SpeechEngine.OnSpeechOutput (+= and -=)

---
*Phase: 01-foundation*
*Completed: 2026-02-11*
