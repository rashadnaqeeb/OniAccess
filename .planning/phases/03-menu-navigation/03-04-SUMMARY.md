---
phase: 03-menu-navigation
plan: 04
subsystem: input
tags: [duplicant-select, save-load, screen-handler, character-container, context-detector]

# Dependency graph
requires:
  - phase: 03-menu-navigation-01
    provides: "ScreenHandler, BaseMenuHandler, WidgetInfo, ContextDetector registry, TypeAheadSearch, KeyPoller"
  - phase: 01-foundation
    provides: "SpeechPipeline, TextFilter, OniAccessStrings, Tolk integration"
provides:
  - "DuplicantSelectHandler for MinionSelectScreen (initial colony start and Printing Pod)"
  - "SaveLoadHandler for LoadScreen with two-level colony/save drill-down"
  - "ContextDetector registrations for MinionSelectScreen and LoadScreen"
  - "Tab/Shift+Tab slot switching for 3 duplicant slots with wrap sound"
  - "Full trait info upfront (name + effect + tooltip description)"
  - "Reroll with automatic re-announcement of new dupe name, interests, traits"
  - "Escape-based back navigation in save view without closing screen"
affects: [04-entity-inspection, 07-duplicant-management]

# Tech tracking
tech-stack:
  added: []
  patterns: [multi-slot-tab-navigation, two-level-drill-down, composite-trait-labels, reroll-announce, pending-transition-flag]

key-files:
  created:
    - OniAccess/Input/Handlers/DuplicantSelectHandler.cs
    - OniAccess/Input/Handlers/SaveLoadHandler.cs
  modified:
    - OniAccess/Input/ContextDetector.cs
    - OniAccess/OniAccessStrings.cs

key-decisions:
  - "Traits combine LocText.text (name + effect) AND ToolTip description into one composite label for full info upfront"
  - "After reroll, speak name + interests + traits but stop at attributes (user navigates to read those)"
  - "LooksLikeAttribute heuristic detects attribute-style text starting with +/- digit to stop reroll announcement"
  - "SaveLoadHandler uses _inColonySaveView boolean and _pendingViewTransition flag for async drill-down"
  - "Escape consumed in save view to go back to colony list instead of closing screen"
  - "Added ClusterCategorySelectionScreen and ColonyDestinationSelectScreen registrations from plan 03-03 to complete Phase 3 registry"

patterns-established:
  - "Multi-slot tab navigation: Tab/Shift+Tab switches slots, rediscovers widgets per slot"
  - "Two-level drill-down: boolean view state tracks which level, transitions via ActivateCurrentWidget"
  - "Composite label building: combine multiple text sources (LocText + ToolTip) during widget discovery"
  - "Pending transition flag pattern: set flag on click, check on next key event for async UI updates"

# Metrics
duration: 7min
completed: 2026-02-11
---

# Phase 3 Plan 04: Duplicant Selection and Save/Load Handlers Summary

**DuplicantSelectHandler with 3-slot tab navigation and full trait readout, SaveLoadHandler with colony/save drill-down, completing all Phase 3 screen handler registrations**

## Performance

- **Duration:** 7 min
- **Started:** 2026-02-11T19:56:42Z
- **Completed:** 2026-02-11T20:03:39Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- DuplicantSelectHandler navigates 3 duplicant slots via Tab/Shift+Tab, reading attributes one per arrow press, traits with full info (name + effect + description), and supporting reroll with automatic re-announcement
- SaveLoadHandler provides two-level navigation: colony list with name/cycle/date, drill into individual saves with auto-save/newest prefixes, Escape returns to colony list
- All 13 Phase 3 screen types registered in ContextDetector across 9 handler classes (WorldGenHandler added concurrently by plan 03-03)

## Task Commits

Each task was committed atomically:

1. **Task 1: DuplicantSelectHandler for MinionSelectScreen** - `c544778` (feat)
2. **Task 2: SaveLoadHandler and ContextDetector registration** - `bfaedb6` (feat)

## Files Created/Modified
- `OniAccess/Input/Handlers/DuplicantSelectHandler.cs` - MinionSelectScreen handler with 3-slot tab navigation, trait/attribute discovery, reroll handling
- `OniAccess/Input/Handlers/SaveLoadHandler.cs` - LoadScreen handler with two-level colony list and save entry navigation
- `OniAccess/Input/ContextDetector.cs` - Added MinionSelectScreen, LoadScreen, ClusterCategorySelectionScreen, ColonyDestinationSelectScreen registrations
- `OniAccess/OniAccessStrings.cs` - Added DUPLICANT_SELECT, SAVE_LOAD, SWITCH_DUPE_SLOT LocStrings

## Decisions Made
- **Trait composite labels:** Traits combine LocText.text (which has "Mole Hands, +2 Digging") with ToolTip description ("Moves through tiles faster") into one spoken label. If tooltip exceeds 120 chars, truncated to first sentence. Shift+I re-reads the same info (no hidden detail).
- **Reroll announcement scope:** After reroll, speak name + interests + traits automatically. Stop at attributes (detected by LooksLikeAttribute heuristic checking for +/- digit prefix). User navigates attributes manually per locked decision.
- **SaveLoadHandler async transition:** Uses _pendingViewTransition flag for cases where colonyViewRoot isn't immediately active after colony click. Checks on next HandleUnboundKey or HandleKeyDown.
- **Colony setup registrations added:** ClusterCategorySelectionScreen and ColonyDestinationSelectScreen registrations were missing from ContextDetector (their handler files existed from plan 03-03 but weren't registered). Added them to complete the Phase 3 registry.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added ClusterCategorySelectionScreen and ColonyDestinationSelectScreen registrations**
- **Found during:** Task 2 (ContextDetector registration)
- **Issue:** ColonySetupHandler file existed from plan 03-03 but was never registered in ContextDetector. The new game flow (Main Menu -> Game Mode -> Asteroid Selection -> Duplicant Selection) would be broken without these registrations.
- **Fix:** Added Register calls for ClusterCategorySelectionScreen (via AccessTools.TypeByName) and ColonyDestinationSelectScreen in RegisterMenuHandlers
- **Files modified:** OniAccess/Input/ContextDetector.cs
- **Verification:** Build succeeds, 12 of 13 expected screen types registered
- **Committed in:** bfaedb6 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical functionality)
**Impact on plan:** Registration was necessary to complete the new game flow. No scope creep -- the handler code already existed.

**Note:** WorldGenScreen handler was created and registered by concurrent plan 03-03 execution, resolving the gap identified during this plan's analysis.

## Issues Encountered
None -- both tasks built clean on first attempt.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All Phase 3 menu navigation handlers are complete (DuplicantSelectHandler and SaveLoadHandler are the final two)
- The full new game flow is accessible: Main Menu -> New Game -> Game Mode -> Asteroid Selection -> Duplicant Selection
- Save/load screen is navigable with two-level hierarchy
- WorldGenScreen handler is the only remaining Phase 3 gap (world generation progress speech)
- Phase 4 (Entity Inspection) can begin when Phase 3 is verified in-game

## Self-Check: PASSED

All 4 created/modified files verified present on disk.
Both task commit hashes verified in git log (c544778, bfaedb6).

---
*Phase: 03-menu-navigation*
*Completed: 2026-02-11*
