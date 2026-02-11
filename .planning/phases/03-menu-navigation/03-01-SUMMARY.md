---
phase: 03-menu-navigation
plan: 01
subsystem: input
tags: [screen-handler, menu-navigation, type-ahead-search, widget-info, context-detector]

# Dependency graph
requires:
  - phase: 02-input-architecture
    provides: "IAccessHandler, HandlerStack, ModInputRouter, KeyPoller, ContextDetector skeleton, HelpHandler, WorldHandler"
  - phase: 01-foundation
    provides: "SpeechPipeline, TextFilter, OniAccessStrings, Tolk integration"
provides:
  - "ScreenHandler abstract base class (IAccessHandler + ISearchable) for all screen handlers"
  - "BaseMenuHandler 1D list navigation layer extending ScreenHandler"
  - "WidgetInfo/WidgetType for widget metadata"
  - "ISearchable interface for type-ahead search integration"
  - "TypeAheadSearch ported with SpeechPipeline integration"
  - "ContextDetector registry with Register<T> and push/pop lifecycle"
  - "KeyPoller extended with Home, End, Tab, Return"
  - "ModInputRouter Escape pass-through for screen close"
  - "Composable help entry lists (CommonHelpEntries, ListNavHelpEntries)"
affects: [03-02, 03-03, 03-04, 07-duplicant-management, 08-area-tools]

# Tech tracking
tech-stack:
  added: [UnityEngine.UI.dll, FMODUnity.dll]
  patterns: [two-layer-handler-hierarchy, screen-handler-registry, composable-help-entries, widget-info-model]

key-files:
  created:
    - OniAccess/Input/ScreenHandler.cs
    - OniAccess/Input/BaseMenuHandler.cs
    - OniAccess/Input/WidgetInfo.cs
    - OniAccess/Input/ISearchable.cs
    - OniAccess/Input/TypeAheadSearch.cs
  modified:
    - OniAccess/Input/KeyPoller.cs
    - OniAccess/Input/ContextDetector.cs
    - OniAccess/Input/ModInputRouter.cs
    - OniAccess/OniAccessStrings.cs
    - OniAccess/OniAccess.csproj

key-decisions:
  - "Two-layer hierarchy: ScreenHandler (infrastructure) + BaseMenuHandler (1D nav) allows future 2D grid handlers to extend ScreenHandler directly"
  - "ContextDetector uses ScreenHandler.Screen (not BaseMenuHandler.Screen) for deactivation matching -- works for any handler type"
  - "TypeAheadSearch HandleKey signature changed from KeyboardManager.KeyModifiers to bool ctrlHeld/altHeld -- no dependency on unavailable type"
  - "Added UnityEngine.UI.dll and FMODUnity.dll references for KSlider/KToggle base types and KFMOD sounds"
  - "DetectAndActivate uses Harmony Traverse to access private KScreenManager.screenStack"

patterns-established:
  - "Two-layer handler hierarchy: ScreenHandler provides common infrastructure, BaseMenuHandler adds 1D navigation"
  - "Composable help lists: subclasses compose CommonHelpEntries + ListNavHelpEntries + screen-specific"
  - "ContextDetector registry pattern: Register<TScreen>(factory), OnScreenActivated pushes, OnScreenDeactivating pops"
  - "Widget discovery pattern: abstract DiscoverWidgets populates _widgets list, base class handles navigation/speech"

# Metrics
duration: 10min
completed: 2026-02-11
---

# Phase 3 Plan 01: Screen Handler Infrastructure Summary

**Two-layer ScreenHandler/BaseMenuHandler class hierarchy with TypeAheadSearch, ContextDetector registry, and extended KeyPoller for menu navigation**

## Performance

- **Duration:** 10 min
- **Started:** 2026-02-11T19:43:32Z
- **Completed:** 2026-02-11T19:53:04Z
- **Tasks:** 3
- **Files modified:** 10

## Accomplishments
- ScreenHandler abstract base provides F12 help, Shift+I tooltip, A-Z search, screen entry speech, and widget lifecycle for all screen handlers
- BaseMenuHandler extends ScreenHandler with 1D list navigation: arrow wrap-around, Home/End, Enter activation, Left/Right adjustment, Tab stubs
- TypeAheadSearch ported from repo root with SpeechPipeline integration, removing all external dependencies
- ContextDetector now has a type-safe registry with push/pop handler lifecycle matching via ScreenHandler.Screen
- KeyPoller polls 9 keys (F12, arrows, Home, End, Tab, Return) and ModInputRouter passes Escape through for screen close

## Task Commits

Each task was committed atomically:

1. **Task 1: WidgetInfo, ISearchable, and TypeAheadSearch port** - `856bc12` (feat)
2. **Task 2: ScreenHandler and BaseMenuHandler class hierarchy** - `6d6e7f1` (feat)
3. **Task 3: KeyPoller extensions, ContextDetector registry, and ModInputRouter Escape pass-through** - `f0ea0e7` (feat)

## Files Created/Modified
- `OniAccess/Input/ScreenHandler.cs` - Abstract base for all screen handlers (IAccessHandler + ISearchable)
- `OniAccess/Input/BaseMenuHandler.cs` - 1D list navigation layer extending ScreenHandler
- `OniAccess/Input/WidgetInfo.cs` - WidgetType enum and WidgetInfo class for widget metadata
- `OniAccess/Input/ISearchable.cs` - Interface for type-ahead search integration
- `OniAccess/Input/TypeAheadSearch.cs` - Type-ahead search with word-start matching and SpeechPipeline
- `OniAccess/Input/KeyPoller.cs` - Extended with Home, End, Tab, Return poll keys
- `OniAccess/Input/ContextDetector.cs` - Rewritten with registry mechanism and push/pop lifecycle
- `OniAccess/Input/ModInputRouter.cs` - Escape added to pass-through actions
- `OniAccess/OniAccessStrings.cs` - Added 7 help LocStrings for navigation
- `OniAccess/OniAccess.csproj` - Added UnityEngine.UI and FMODUnity DLL references

## Decisions Made
- **Two-layer hierarchy split:** ScreenHandler holds all common infrastructure (F12, tooltip, search, speech, widgets). BaseMenuHandler adds only 1D navigation. This allows Phase 8 grid handlers to extend ScreenHandler directly without inheriting irrelevant 1D navigation methods.
- **ScreenHandler.Screen for deactivation matching:** ContextDetector.OnScreenDeactivating checks `ScreenHandler.Screen` (not BaseMenuHandler), so it works for any handler that extends ScreenHandler, including future grid handlers.
- **TypeAheadSearch HandleKey signature:** Changed from `KeyboardManager.KeyModifiers` to `bool ctrlHeld, bool altHeld` because KeyboardManager.KeyModifiers is from another project and unavailable in ONI.
- **New DLL references:** Added UnityEngine.UI.dll (required for KSlider/KToggle which inherit Unity's Slider/Toggle) and FMODUnity.dll (required for KFMOD.PlayUISound EventReference parameter type).
- **Traverse for screenStack:** KScreenManager.screenStack is a private field, so DetectAndActivate uses Harmony Traverse with try/catch for safe access.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added UnityEngine.UI.dll and FMODUnity.dll references to csproj**
- **Found during:** Task 2 (ScreenHandler and BaseMenuHandler)
- **Issue:** KSlider/KToggle inherit from Unity's Slider/Toggle (in UnityEngine.UI.dll), and KFMOD.PlayUISound requires FMODUnity.dll. Build failed without these references.
- **Fix:** Added both DLL references to OniAccess.csproj with HintPath via ONI_MANAGED
- **Files modified:** OniAccess/OniAccess.csproj
- **Verification:** Build succeeds with zero errors
- **Committed in:** 6d6e7f1 (Task 2 commit)

**2. [Rule 1 - Bug] Fixed private screenStack access in ContextDetector.DetectAndActivate**
- **Found during:** Task 3 (ContextDetector registry)
- **Issue:** Plan referenced `KScreenManager.Instance.screenStack` as if public, but the field is private
- **Fix:** Used HarmonyLib.Traverse.Create().Field<List<KScreen>>("screenStack") with try/catch
- **Files modified:** OniAccess/Input/ContextDetector.cs
- **Verification:** Build succeeds with zero errors
- **Committed in:** f0ea0e7 (Task 3 commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 bug)
**Impact on plan:** Both fixes were necessary for compilation. No scope creep.

## Issues Encountered
None beyond the deviations documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- The handler infrastructure is ready for concrete screen handlers (Plans 02-04)
- A list-based handler can be created by extending BaseMenuHandler and implementing only DiscoverWidgets, DisplayName, and HelpEntries
- ContextDetector.Register<TScreen> is ready for handler registration in concrete handler plans
- All 9 plan verification checks pass (clean build, ScreenHandler implements IAccessHandler + ISearchable, BaseMenuHandler extends ScreenHandler, composable help lists are static, TypeAheadSearch uses SpeechPipeline, KeyPoller has 9 poll keys, ContextDetector has _registry and Register, deactivation uses ScreenHandler.Screen, ModInputRouter passes Escape)

## Self-Check: PASSED

All 10 created/modified files verified present on disk.
All 3 task commit hashes verified in git log.

---
*Phase: 03-menu-navigation*
*Completed: 2026-02-11*
