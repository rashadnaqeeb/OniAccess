---
phase: 02-input-architecture
plan: 01
subsystem: input
tags: [iinputhandler, kinputhandler, handler-stack, harmony-patches, kscreen-lifecycle, context-detection, key-polling, priority-dispatch]

# Dependency graph
requires:
  - phase: 01-01
    provides: "SpeechEngine with Tolk P/Invoke"
  - phase: 01-02
    provides: "SpeechPipeline with SpeakInterrupt and SetEnabled"
  - phase: 01-03
    provides: "VanillaMode toggle, InputInterceptor MonoBehaviour, HotkeyRegistry (Phase 1 approach)"
provides:
  - "IAccessHandler: interface for context-sensitive input handlers with DisplayName, CapturesAllInput, HandleKeyDown/Up, HandleUnboundKey, HelpEntries, OnActivate/OnDeactivate"
  - "HelpEntry: data class for F12 navigable help list entries (KeyName + Description)"
  - "HandlerStack: static handler stack with Push/Pop/Replace/DeactivateAll/Clear and proper lifecycle callbacks"
  - "ModInputRouter: IInputHandler registered at priority 50 in ONI's KInputHandler tree with full-capture and selective-claim modes"
  - "KeyPoller: MonoBehaviour polling F12 and arrow keys (unbound in ONI) and forwarding to active handler"
  - "ContextDetector: skeleton for KScreen lifecycle hooks (Phase 3 adds screen-to-handler mappings)"
  - "InputArchPatches: Harmony patches for InputInit.Awake (router registration), KScreen.Activate/Deactivate (context detection)"
affects: [03-menu-navigation, 04-world-navigation, 05-entity-inspection]

# Tech tracking
tech-stack:
  added: [iinputhandler-integration, kinputhandler-priority-dispatch, harmony-targetmethod-accesstools]
  patterns: [handler-stack-with-lifecycle, full-capture-vs-selective-claim, unbound-key-polling, internal-type-harmony-patching]

key-files:
  created:
    - OniAccess/Input/IAccessHandler.cs
    - OniAccess/Input/HelpEntry.cs
    - OniAccess/Input/HandlerStack.cs
    - OniAccess/Input/ModInputRouter.cs
    - OniAccess/Input/KeyPoller.cs
    - OniAccess/Input/ContextDetector.cs
    - OniAccess/Patches/InputArchPatches.cs
  modified:
    - OniAccess/Util/LogHelper.cs

key-decisions:
  - "AccessTools.TypeByName for internal InputInit class -- cannot use typeof() on internal types, so TargetMethod pattern with AccessTools resolves the type at runtime"
  - "ContextDetector.DetectAndActivate is a no-op in Phase 2 -- concrete handlers (WorldHandler etc.) are Phase 3, so Phase 2 skeleton only logs"
  - "KeyPoller handles Ctrl+Shift+F12 toggle directly instead of going through HotkeyRegistry -- must work even when mod is off, outside the handler system"
  - "Mouse and zoom actions always pass through in full-capture mode -- IsMouseOrZoomAction checks 6 Action types to prevent blocking mouse clicks per pitfall #6"

patterns-established:
  - "Handler stack lifecycle: Push calls OnActivate, Pop calls OnDeactivate then reactivates exposed handler, Replace combines both"
  - "Full-capture mode: handler processes first, then all non-mouse keyboard events consumed to prevent game interaction"
  - "Selective-claim mode: handler uses e.TryConsume(Action) to claim specific keys, everything else passes through"
  - "Unbound key polling: MonoBehaviour polls keys with no ONI Action binding (F12, arrows) and delegates to active handler via HandleUnboundKey"
  - "Internal type patching: use [HarmonyPatch] + TargetMethod() + AccessTools.TypeByName instead of typeof() for internal game types"

# Metrics
duration: 7min
completed: 2026-02-11
---

# Phase 2 Plan 1: Input Architecture Summary

**IInputHandler-based ModInputRouter at priority 50 in ONI's KInputHandler tree with handler stack, full-capture/selective-claim modes, unbound key polling, and KScreen lifecycle context detection**

## Performance

- **Duration:** 7 min
- **Started:** 2026-02-11T15:16:30Z
- **Completed:** 2026-02-11T15:23:36Z
- **Tasks:** 2
- **Files created:** 7
- **Files modified:** 1

## Accomplishments
- IAccessHandler interface defining the full handler contract: DisplayName, CapturesAllInput, HelpEntries, HandleKeyDown/Up, HandleUnboundKey, OnActivate, OnDeactivate
- HandlerStack managing handler lifecycle with Push/Pop/Replace/DeactivateAll/Clear and proper OnActivate/OnDeactivate callback sequencing
- ModInputRouter as a single IInputHandler registered at priority 50 (above PlayerController at 20, KScreenManager at 10) with two input modes: full-capture for menus and selective-claim for world view
- KeyPoller MonoBehaviour polling unbound keys (F12 without modifiers, arrow keys) that ONI generates no KButtonEvent for, plus Ctrl+Shift+F12 toggle that works even when mod is off
- ContextDetector skeleton receiving screen lifecycle events from Harmony patches, ready for Phase 3 to add screen-to-handler mappings
- Harmony patches: InputInit.Awake postfix for idempotent router registration, KScreen.Activate postfix and KScreen.Deactivate prefix for context detection
- Full compilation verification against game DLLs with zero warnings and zero errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Create IAccessHandler interface, HelpEntry, and HandlerStack** - `8ac2f87` (feat)
2. **Task 2: Create ModInputRouter, KeyPoller, ContextDetector, and Harmony patches** - `1fe538b` (feat)

## Files Created/Modified
- `OniAccess/Input/IAccessHandler.cs` - Interface for mod input handlers with 8 members covering handler contract
- `OniAccess/Input/HelpEntry.cs` - Simple data class for F12 navigable help list entries (KeyName + Description)
- `OniAccess/Input/HandlerStack.cs` - Static handler stack with Push/Pop/Replace/DeactivateAll/Clear and lifecycle callbacks
- `OniAccess/Input/ModInputRouter.cs` - IInputHandler implementation delegating to HandlerStack.ActiveHandler with full-capture and selective-claim modes
- `OniAccess/Input/KeyPoller.cs` - MonoBehaviour polling unbound keys (F12, arrows) and Ctrl+Shift+F12 toggle
- `OniAccess/Input/ContextDetector.cs` - Static class receiving KScreen lifecycle events, Phase 2 skeleton with logging
- `OniAccess/Patches/InputArchPatches.cs` - Harmony patches for InputInit.Awake, KScreen.Activate, KScreen.Deactivate
- `OniAccess/Util/LogHelper.cs` - Added Debug method, fully qualified all UnityEngine.Debug references

## Decisions Made
- Used AccessTools.TypeByName("InputInit") with TargetMethod pattern instead of typeof(InputInit) because InputInit is declared internal in the game DLLs -- Harmony supports this via runtime type resolution
- ContextDetector.DetectAndActivate() is a logging no-op in Phase 2 because concrete handler implementations (WorldHandler, MenuHandler, etc.) are Phase 3 deliverables -- no speculative infrastructure per Phase 1 decision
- KeyPoller handles the Ctrl+Shift+F12 toggle key directly via UnityEngine.Input.GetKeyDown instead of routing through the handler system, because it must function even when the mod is off and all handlers are deactivated
- IsMouseOrZoomAction checks 6 specific Action enum values (MouseLeft, MouseRight, MouseMiddle, ShiftMouseLeft, ZoomIn, ZoomOut) to prevent full-capture mode from blocking mouse interaction

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added Log.Debug method to LogHelper**
- **Found during:** Task 1 (HandlerStack implementation)
- **Issue:** Plan's HandlerStack and ContextDetector call Util.Log.Debug() but LogHelper only had Info/Warn/Error
- **Fix:** Added Debug(string) method to Log class, fully qualified all UnityEngine.Debug references to avoid method name shadowing, removed unused `using UnityEngine` directive
- **Files modified:** OniAccess/Util/LogHelper.cs
- **Verification:** Build passes, all Log.Debug calls resolve correctly
- **Committed in:** 8ac2f87 (Task 1 commit)

**2. [Rule 1 - Bug] Fixed InputInit internal accessibility with AccessTools**
- **Found during:** Task 2 (InputArchPatches implementation)
- **Issue:** Plan specified `[HarmonyPatch(typeof(InputInit), "Awake")]` but InputInit is declared `internal class` in game DLLs, causing CS0122 compilation error
- **Fix:** Replaced typeof-based patch attribute with `[HarmonyPatch]` + `TargetMethod()` using `AccessTools.TypeByName("InputInit")` for runtime type resolution
- **Files modified:** OniAccess/Patches/InputArchPatches.cs
- **Verification:** Build passes with zero errors, Harmony will resolve the type at runtime
- **Committed in:** 1fe538b (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 bug)
**Impact on plan:** Both fixes necessary for compilation. No scope creep. All planned artifacts delivered.

## Issues Encountered
None beyond the auto-fixed deviations above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Input architecture foundation complete: ModInputRouter registered, handler stack operational, unbound key polling active
- Phase 1's InputInterceptor and HotkeyRegistry remain in place -- Plan 02-02 handles migration and cleanup
- Phase 3 (Menu Navigation) can implement concrete IAccessHandler classes (WorldHandler, MenuHandler, HelpHandler) that plug into the stack
- ContextDetector ready for Phase 3 to add screen-type-to-handler mappings in OnScreenActivated/OnScreenDeactivating
- VanillaMode.Toggle() should be updated in 02-02 to call HandlerStack.DeactivateAll() on toggle off and ContextDetector.DetectAndActivate() on toggle on

## Self-Check: PASSED

- All 7 created files verified on disk
- 1 modified file (LogHelper.cs) verified on disk
- Commit `8ac2f87` (Task 1) verified in git log
- Commit `1fe538b` (Task 2) verified in git log
- Key link verified: InputArchPatches -> ModInputRouter via KInputHandler.Add(router, 50)
- Key link verified: ModInputRouter -> HandlerStack.ActiveHandler (2 calls: OnKeyDown, OnKeyUp)
- Key link verified: InputArchPatches -> ContextDetector.OnScreenActivated and OnScreenDeactivating
- Key link verified: KeyPoller -> HandlerStack.ActiveHandler (1 call in Update)
- Build verification: dotnet build OniAccess.csproj succeeds with 0 warnings, 0 errors
