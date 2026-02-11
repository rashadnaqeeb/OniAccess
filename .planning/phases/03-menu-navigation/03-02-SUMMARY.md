---
phase: 03-menu-navigation
plan: 02
subsystem: input
tags: [menu-handler, main-menu, pause-menu, options, confirm-dialog, colony-summary, context-detector]

# Dependency graph
requires:
  - phase: 03-menu-navigation/01
    provides: "ScreenHandler, BaseMenuHandler, WidgetInfo, TypeAheadSearch, ContextDetector registry, composable help entries"
  - phase: 02-input-architecture
    provides: "IAccessHandler, HandlerStack, ModInputRouter, KeyPoller"
  - phase: 01-foundation
    provides: "SpeechPipeline, TextFilter, OniAccessStrings, Tolk integration"
provides:
  - "MainMenuHandler for MainMenu (buttonParent traversal, Button_ResumeGame)"
  - "PauseMenuHandler for PauseScreen (KButtonMenu buttons array pattern)"
  - "ConfirmDialogHandler for ConfirmDialogScreen (message + OK/Cancel buttons)"
  - "OptionsMenuHandler for OptionsMenuScreen, AudioOptionsScreen, GraphicsOptionsScreen, GameOptionsScreen (sliders, toggles, buttons)"
  - "ColonySummaryHandler for RetiredColonyInfoScreen (two-view: explorer and detail, MENU-09)"
  - "ContextDetector.RegisterMenuHandlers with 8 screen-to-handler registrations"
affects: [03-03, 03-04]

# Tech tracking
tech-stack:
  added: []
  patterns: [buttonParent-traversal, kbuttonmenu-buttons-array, options-widget-discovery, two-view-handler, mouse-only-filter]

key-files:
  created:
    - OniAccess/Input/Handlers/MainMenuHandler.cs
    - OniAccess/Input/Handlers/PauseMenuHandler.cs
    - OniAccess/Input/Handlers/ConfirmDialogHandler.cs
    - OniAccess/Input/Handlers/OptionsMenuHandler.cs
    - OniAccess/Input/Handlers/ColonySummaryHandler.cs
  modified:
    - OniAccess/Input/ContextDetector.cs
    - OniAccess/OniAccessStrings.cs
    - OniAccess/Mod.cs

key-decisions:
  - "MainMenu uses buttonParent field traversal (not KButtonMenu.buttons) because MainMenu inherits KScreen directly"
  - "OptionsMenuHandler is a single handler for all 4 options screens, using screen type name to determine display name and widget discovery strategy"
  - "ConfirmDialogHandler dynamically extracts title from titleText field with fallback to generic 'Confirm'"
  - "ColonySummaryHandler tracks _inColonyDetail boolean for two-view navigation with Escape interception in detail view"
  - "RegisterMenuHandlers called from Mod.OnLoad for centralized handler registration"
  - "Options sub-screens use AccessTools.TypeByName for runtime type resolution (compile-time types not available)"

patterns-established:
  - "buttonParent traversal: for screens that create buttons dynamically via MakeButton, walk buttonParent transform children for KButton/LocText pairs"
  - "KButtonMenu buttons array: for screens inheriting KButtonMenu/KModalButtonMenu, access buttons IList and buttonObjects array via Traverse"
  - "Options widget discovery: GetComponentsInChildren<KSlider/KToggle/KButton> with mouse-only filter (drag, resize, close, scrollbar)"
  - "Two-view handler: track view state with boolean, rediscover widgets on view transition, intercept Escape for back-navigation"
  - "Handler registration: centralized RegisterMenuHandlers method called from Mod.OnLoad"

# Metrics
duration: 6min
completed: 2026-02-11
---

# Phase 3 Plan 02: Basic Screen Handlers Summary

**Five concrete handlers (MainMenu, PauseMenu, ConfirmDialog, Options, ColonySummary) registered in ContextDetector for keyboard navigation of all entry-point screens**

## Performance

- **Duration:** 6 min
- **Started:** 2026-02-11T19:56:08Z
- **Completed:** 2026-02-11T20:02:36Z
- **Tasks:** 3
- **Files modified:** 8

## Accomplishments
- MainMenuHandler navigates MainMenu buttons via buttonParent traversal with Button_ResumeGame support
- PauseMenuHandler navigates PauseScreen via KButtonMenu.buttons array pattern
- ConfirmDialogHandler shows dialog message as readable Label, then OK/Cancel buttons
- OptionsMenuHandler handles top-level options menu plus Audio/Graphics/Game sub-screens with slider, toggle, and button discovery
- ColonySummaryHandler provides two-view navigation (explorer colony list and detail stats/achievements) for RetiredColonyInfoScreen (MENU-09)
- ContextDetector.RegisterMenuHandlers registers 8 screen types, called from Mod.OnLoad

## Task Commits

Each task was committed atomically:

1. **Task 1: MainMenuHandler, PauseMenuHandler, and ConfirmDialogHandler** - `5616c1f` (feat)
2. **Task 2: OptionsMenuHandler and ContextDetector registration** - `04f9768` (feat)
3. **Task 3: ColonySummaryHandler for RetiredColonyInfoScreen** - `8cc8399` (feat)

## Files Created/Modified
- `OniAccess/Input/Handlers/MainMenuHandler.cs` - Main menu button navigation via buttonParent and Button_ResumeGame
- `OniAccess/Input/Handlers/PauseMenuHandler.cs` - Pause menu navigation via KButtonMenu.buttons array
- `OniAccess/Input/Handlers/ConfirmDialogHandler.cs` - Confirmation dialog with message text + OK/Cancel buttons
- `OniAccess/Input/Handlers/OptionsMenuHandler.cs` - Options screens with slider/toggle/button discovery and mouse-only filtering
- `OniAccess/Input/Handlers/ColonySummaryHandler.cs` - Colony summary with two-view navigation (explorer and detail)
- `OniAccess/Input/ContextDetector.cs` - RegisterMenuHandlers with 8 screen-to-handler mappings
- `OniAccess/OniAccessStrings.cs` - Added 8 handler display name LocStrings
- `OniAccess/Mod.cs` - Calls RegisterMenuHandlers during initialization

## Decisions Made
- **MainMenu buttonParent traversal:** MainMenu inherits KScreen directly (not KButtonMenu), so widget discovery walks the buttonParent Transform field for KButton children with LocText labels. Button_ResumeGame is a separate serialized field checked first.
- **Single OptionsMenuHandler for all options screens:** Rather than creating separate handlers for Audio/Graphics/Game options, a single handler uses screen type name to determine display name and discovery strategy (buttons array for top-level, GetComponentsInChildren for sub-screens).
- **ConfirmDialogHandler dynamic title:** Extracts title from titleText field via Traverse with fallback to generic "Confirm" LocString, adapting to whichever dialog is shown.
- **ColonySummaryHandler two-view state:** Boolean `_inColonyDetail` tracks current view. Escape is intercepted via HandleKeyDown + TryConsume(Action.Escape) in detail view to return to explorer instead of closing the screen.
- **Centralized registration in Mod.OnLoad:** RegisterMenuHandlers is called during mod initialization, before any screens activate. AccessTools.TypeByName used for options sub-screens and RetiredColonyInfoScreen where compile-time types are not directly available.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All five handler classes compile and inherit BaseMenuHandler correctly
- ContextDetector registry is populated with 8 screen-to-handler mappings for all basic screens
- Widget discovery patterns validated: buttonParent for MainMenu, buttons array for KButtonMenu derivatives, GetComponentsInChildren for options sub-screens
- Plans 03-03 (colony setup/duplicant selection) and 03-04 (save/load/worldgen) can add their handlers and registrations to the same infrastructure

## Self-Check: PASSED

All 8 created/modified files verified present on disk.
All 3 task commit hashes verified in git log.

---
*Phase: 03-menu-navigation*
*Completed: 2026-02-11*
