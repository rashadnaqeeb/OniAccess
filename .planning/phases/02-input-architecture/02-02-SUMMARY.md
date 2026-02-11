---
phase: 02-input-architecture
plan: 02
subsystem: input
tags: [world-handler, help-handler, vanilla-mode, handler-stack, context-detection, phase1-migration]

# Dependency graph
requires:
  - phase: 02-01
    provides: "IAccessHandler interface, HandlerStack, ModInputRouter, KeyPoller, ContextDetector, InputArchPatches"
  - phase: 01-02
    provides: "SpeechPipeline with SpeakInterrupt, SpeakQueued, SetEnabled"
  - phase: 01-03
    provides: "VanillaMode toggle (Phase 1 version), InputInterceptor, HotkeyRegistry (all now replaced)"
provides:
  - "WorldHandler: default IAccessHandler for world view with selective claim, F12 opens help"
  - "HelpHandler: full-capture IAccessHandler presenting navigable help list with Up/Down/Escape navigation"
  - "VanillaMode redesigned: HandlerStack.DeactivateAll on OFF, ContextDetector.DetectAndActivate on ON"
  - "Mod.cs migrated: KeyPoller + WorldHandler replace InputInterceptor + HotkeyRegistry"
  - "Phase 1 input files deleted: HotkeyRegistry.cs, InputInterceptor.cs, AccessContext.cs"
affects: [03-menu-navigation, 04-world-navigation, 05-entity-inspection]

# Tech tracking
tech-stack:
  added: []
  patterns: [navigable-help-list, handler-pushed-from-handler, full-disable-sequence, speech-before-disable]

key-files:
  created:
    - OniAccess/Input/WorldHandler.cs
    - OniAccess/Input/HelpHandler.cs
  modified:
    - OniAccess/Toggle/VanillaMode.cs
    - OniAccess/Mod.cs
    - OniAccess/OniAccessStrings.cs
    - OniAccess/Patches/InputPatches.cs
  deleted:
    - OniAccess/Input/HotkeyRegistry.cs
    - OniAccess/Input/InputInterceptor.cs
    - OniAccess/Input/AccessContext.cs

key-decisions:
  - "WorldHandler pushes HelpHandler directly with its own HelpEntries -- handler-to-handler composition via HandlerStack"
  - "VanillaMode OFF speaks confirmation before deactivating handlers and disabling speech -- order prevents silent shutdown"
  - "VanillaMode ON sets IsEnabled flag first, then enables speech, then speaks, then detects state -- order ensures all systems ready before detection"
  - "InputPatches.cs comment updated to reflect Phase 2 architecture rather than Phase 1 MonoBehaviour approach"

patterns-established:
  - "Navigable help list: HelpHandler receives parent handler's HelpEntries, presents them one-at-a-time with Up/Down wrapping navigation"
  - "Handler-from-handler: WorldHandler.HandleUnboundKey(F12) pushes HelpHandler onto the stack -- handlers can compose by pushing new handlers"
  - "Full disable sequence: speak -> deactivate handlers -> disable speech -> set flag (order matters for confirmation speech)"
  - "Full enable sequence: set flag -> enable speech -> speak -> detect state (order matters for state detection)"

# Metrics
duration: 5min
completed: 2026-02-11
---

# Phase 2 Plan 2: Handlers, Migration, and Cleanup Summary

**WorldHandler and HelpHandler implementing navigable F12 help, VanillaMode redesigned for full handler-stack lifecycle, Mod.cs migrated to KeyPoller/WorldHandler, Phase 1 input files deleted**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-11T15:27:13Z
- **Completed:** 2026-02-11T15:32:50Z
- **Tasks:** 2
- **Files created:** 2
- **Files modified:** 4
- **Files deleted:** 3

## Accomplishments
- WorldHandler as the always-present default handler for world view with selective claim (CapturesAllInput=false), F12 pushes HelpHandler
- HelpHandler as a full-capture navigable help list: Up/Down arrows step through entries with wrapping, Escape or F12 pops back to previous handler
- VanillaMode completely redesigned: OFF speaks confirmation then calls HandlerStack.DeactivateAll() then disables speech; ON sets flag, enables speech, speaks confirmation, calls ContextDetector.DetectAndActivate()
- Mod.cs entry point migrated from Phase 1 (InputInterceptor + RegisterHotkeys + SpeakContextHelp) to Phase 2 (KeyPoller + WorldHandler push)
- All Phase 1 input files deleted (HotkeyRegistry.cs, InputInterceptor.cs, AccessContext.cs) with zero remaining references in the codebase
- Full compilation verification against game DLLs with zero warnings and zero errors
- All 5 key_links from plan must_haves verified via grep

## Task Commits

Each task was committed atomically:

1. **Task 1: Create WorldHandler, HelpHandler, and update strings** - `9452b9b` (feat)
2. **Task 2: Redesign VanillaMode, migrate Mod.cs, delete old Phase 1 input files** - `facd8b0` (feat)

## Files Created/Modified
- `OniAccess/Input/WorldHandler.cs` - Default IAccessHandler for world view with selective claim, F12 pushes HelpHandler with own HelpEntries
- `OniAccess/Input/HelpHandler.cs` - Full-capture IAccessHandler presenting navigable help list with Up/Down wrapping, Escape/F12 close
- `OniAccess/Toggle/VanillaMode.cs` - Redesigned toggle: OFF calls HandlerStack.DeactivateAll, ON calls ContextDetector.DetectAndActivate
- `OniAccess/Mod.cs` - Migrated from InputInterceptor+HotkeyRegistry to KeyPoller+WorldHandler, removed RegisterHotkeys/SpeakContextHelp
- `OniAccess/OniAccessStrings.cs` - Added HANDLERS (WORLD_VIEW, HELP) and HELP (NAVIGATE, CLOSE) LocString classes
- `OniAccess/Patches/InputPatches.cs` - Updated comment to reference Phase 2 architecture instead of Phase 1

## Decisions Made
- WorldHandler pushes HelpHandler directly with its own HelpEntries, establishing handler-to-handler composition pattern via HandlerStack
- VanillaMode OFF sequence is speak -> deactivate -> disable speech -> set flag, ensuring the confirmation message is spoken before the pipeline shuts down
- VanillaMode ON sequence is set flag -> enable speech -> speak -> detect state, ensuring all systems are initialized before context detection runs
- InputPatches.cs comment updated to correctly describe Phase 2 architecture rather than the now-deleted Phase 1 MonoBehaviour approach

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Updated InputPatches.cs stale comment**
- **Found during:** Task 2 (deletion of Phase 1 files)
- **Issue:** InputPatches.cs comment referenced "MonoBehaviour approach for input (InputInterceptor)" which was being deleted
- **Fix:** Updated comment to reference Phase 2 architecture (InputArchPatches.cs for ModInputRouter registration and KScreen context detection)
- **Files modified:** OniAccess/Patches/InputPatches.cs
- **Verification:** Build passes, comment accurately describes current architecture
- **Committed in:** facd8b0 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 bug - stale comment)
**Impact on plan:** Minor documentation fix. No scope creep. All planned artifacts delivered.

## Issues Encountered
None beyond the auto-fixed deviation above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Input architecture fully operational end-to-end: KeyPoller polls unbound keys, ModInputRouter handles KButtonEvents, HandlerStack manages lifecycle, WorldHandler and HelpHandler provide concrete behavior
- Phase 3 (Menu Navigation) can implement menu-specific IAccessHandler classes that get pushed onto the stack when KScreens activate
- ContextDetector.DetectAndActivate() is ready for Phase 3 to add screen-type-to-handler mappings
- ContextDetector.OnScreenActivated/OnScreenDeactivating are ready for Phase 3 to add push/pop logic
- VanillaMode is Phase 2 complete: full disable/enable cycle works with handler stack
- No Phase 1 input code remains -- clean slate for Phase 3

## Self-Check: PASSED

- All 2 created files verified on disk (WorldHandler.cs, HelpHandler.cs)
- All 4 modified files verified on disk (VanillaMode.cs, Mod.cs, OniAccessStrings.cs, InputPatches.cs)
- All 3 deleted files confirmed absent (HotkeyRegistry.cs, InputInterceptor.cs, AccessContext.cs)
- Commit `9452b9b` (Task 1) verified in git log
- Commit `facd8b0` (Task 2) verified in git log
- Key link verified: WorldHandler -> HelpHandler via HandlerStack.Push(new HelpHandler(HelpEntries))
- Key link verified: HelpHandler -> HandlerStack via HandlerStack.Pop()
- Key link verified: VanillaMode -> HandlerStack via HandlerStack.DeactivateAll()
- Key link verified: VanillaMode -> ContextDetector via ContextDetector.DetectAndActivate()
- Key link verified: Mod.cs -> KeyPoller via go.AddComponent<KeyPoller>()
- Grep for old types (HotkeyRegistry, InputInterceptor, AccessContext, HotkeyModifier, HotkeyBinding): zero results
- Build verification: dotnet build OniAccess.csproj succeeds with 0 warnings, 0 errors

---
*Phase: 02-input-architecture*
*Completed: 2026-02-11*
