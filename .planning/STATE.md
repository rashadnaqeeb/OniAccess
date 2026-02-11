# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-11)

**Core value:** A blind player can play a full colony with an experience designed for audio, not a translation of the visual interface.
**Current focus:** Phase 3 - Menu Navigation

## Current Position

Phase: 3 of 12 (Menu Navigation)
Plan: 4 of 4 in current phase
Status: Plan 03-04 complete (duplicant selection + save/load handlers). Phase 3 all plans complete.
Last activity: 2026-02-11 -- Plan 03-04 (duplicant selection + save/load) executed: 2 created, 2 modified, builds clean

Progress: [###.......] 30%

## Performance Metrics

**Velocity:**
- Total plans completed: 8
- Average duration: 5min
- Total execution time: 0.74 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01-foundation | 3 | 9min | 3min |
| 02-input-architecture | 2 | 12min | 6min |
| 03-menu-navigation | 3 | 23min | 7min |

**Recent Trend:**
- Last 5 plans: 02-02 (5min), 03-01 (10min), 03-02 (6min), 03-04 (7min)
- Trend: handler plans stable at 6-7min, infrastructure plans at 10min

*Updated after each plan completion*

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Roadmap]: Observe-before-act ordering -- entity inspection immediately follows world navigation so player understands what they find before overlays and colony management
- [Roadmap]: META requirements distributed to Phase 1 (foundation) and Phase 2 (help system)
- [Roadmap]: Duplicant management and entity inspection split into separate phases for depth
- [Roadmap revised]: Entity Inspection moved from Phase 8 to Phase 4 -- after navigating to entities, player should inspect them before diving into overlays and colony management
- [Roadmap revised]: Duplicant Management moved from Phase 8 to Phase 7 -- must control workforce before issuing construction or area tool orders
- [Roadmap revised]: Area Tools split out of Building & Construction into new Phase 8 -- area tools (dig, mop, sweep, harvest, capture, attack, disinfect, empty pipe, etc.) interact with the existing world, distinct from placing new structures
- [Roadmap revised]: BUILD-08 (cancel orders) and BUILD-10 (tool commands) moved to Phase 8 (Area Tools); new AREA-01 through AREA-08 requirements added for full area tool coverage
- [01-01]: LocString entries in STRINGS namespace for full localization support from day one
- [01-01]: Tolk_Silence for Stop() instead of empty string Tolk_Output -- cleaner API usage
- [01-01]: OnSpeechOutput callback as internal Action<string> for test capture hook point
- [01-01]: RegisterForTranslation called in OnLoad to enable community translations
- [01-02]: Sprite replacement appends trailing space for clean word separation, normalized by whitespace step
- [01-02]: Queue drained immediately on SpeakQueued -- Tolk handles pacing internally
- [01-02]: AlertHistory uses injectable GetTime delegate for testability without Unity runtime
- [01-02]: Interrupt speech does NOT go to alert history -- navigation is ephemeral
- [01-03]: MonoBehaviour Update() approach for input interception instead of Harmony KInputHandler patches -- simpler for Phase 1
- [01-03]: HotkeyModifier as Flags enum independent from ONI's Modifier enum
- [01-03]: Frame dedup via Time.frameCount dictionary prevents multiple fires per frame per binding
- [01-03]: Toggle off temporarily re-enables SpeechPipeline to speak confirmation before disabling
- [verify-01]: Always use System.Action (not Action) -- ONI's Assembly-CSharp defines its own Action type that shadows System.Action
- [verify-01]: Always fully qualify UnityEngine.Input when in OniAccess.Input namespace -- bare Input resolves to the namespace
- [verify-01]: UnityEngine.InputLegacyModule.dll must be referenced in csproj -- UnityEngine.Input is forwarded there
- [verify-01]: Every plan must compile against game DLLs (ONI_MANAGED) before being marked complete -- no unverified code
- [verify-01]: Stripped AlertHistory, Announcement, SpeechPriority, SpeakQueued -- premature infrastructure for Phase 7 built without understanding ONI's notification system. Design alert speech when we get there.
- [verify-01]: No speculative infrastructure -- only build what the current phase needs
- [02-01]: AccessTools.TypeByName for internal InputInit class -- TargetMethod pattern resolves internal types at runtime for Harmony patching
- [02-01]: ContextDetector.DetectAndActivate is a no-op in Phase 2 -- concrete handlers are Phase 3, no speculative infrastructure
- [02-01]: KeyPoller handles Ctrl+Shift+F12 toggle directly -- must work when mod is off, outside the handler system
- [02-01]: Mouse and zoom actions always pass through in full-capture mode -- 6 Action types checked to prevent blocking clicks
- [02-02]: WorldHandler pushes HelpHandler directly with own HelpEntries -- handler-to-handler composition via HandlerStack
- [02-02]: VanillaMode OFF speaks confirmation before deactivating handlers and disabling speech -- order prevents silent shutdown
- [02-02]: VanillaMode ON sets flag first, enables speech, speaks, then detects state -- order ensures all systems ready before detection
- [02-02]: Phase 1 input files (HotkeyRegistry, InputInterceptor, AccessContext) fully deleted with zero remaining references
- [03-01]: Two-layer hierarchy: ScreenHandler (infrastructure) + BaseMenuHandler (1D nav) -- Phase 8 grid handlers extend ScreenHandler directly
- [03-01]: ContextDetector uses ScreenHandler.Screen for deactivation matching -- works for any handler type, not just BaseMenuHandler
- [03-01]: TypeAheadSearch HandleKey uses bool ctrlHeld/altHeld instead of KeyboardManager.KeyModifiers -- no dependency on unavailable type
- [03-01]: Added UnityEngine.UI.dll and FMODUnity.dll to csproj -- required for KSlider/KToggle base types and KFMOD sounds
- [03-01]: DetectAndActivate uses Harmony Traverse to access private KScreenManager.screenStack
- [03-02]: MainMenu uses buttonParent traversal (not KButtonMenu.buttons) because MainMenu inherits KScreen directly
- [03-02]: Single OptionsMenuHandler for all 4 options screens, using screen type name for display name and discovery strategy
- [03-02]: ColonySummaryHandler tracks _inColonyDetail boolean for two-view navigation with Escape interception
- [03-02]: RegisterMenuHandlers called from Mod.OnLoad for centralized handler registration
- [03-02]: AccessTools.TypeByName for options sub-screens and RetiredColonyInfoScreen (compile-time types not available)
- [03-04]: Traits combine LocText.text (name + effect) AND ToolTip description into one composite label for full info upfront
- [03-04]: After reroll, speak name + interests + traits but stop at attributes (LooksLikeAttribute heuristic with +/- digit prefix)
- [03-04]: SaveLoadHandler uses _inColonySaveView boolean and _pendingViewTransition flag for async two-level drill-down
- [03-04]: Escape consumed in save view to go back to colony list instead of closing screen

### Pending Todos

None yet.

### Blockers/Concerns

- Speech.cs already exists with Tolk integration -- Phase 1 should build on it, not rewrite
- Research flags Phase 5 (Overlays) and Phase 4 (Entity Inspection) as needing deeper research before planning
- 100+ building side screen variants in Phase 4 will need per-screen adapter pattern

## Session Continuity

Last session: 2026-02-11
Stopped at: Completed 03-04-PLAN.md (duplicant selection + save/load handlers). Phase 3 plans all complete. Ready for Phase 4 (Entity Inspection).
Resume file: None
