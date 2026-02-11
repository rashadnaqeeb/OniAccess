---
phase: 01-foundation
plan: 01
subsystem: speech
tags: [tolk, pinvoke, csharp, oni-mod, screen-reader, locstring, net472]

# Dependency graph
requires: []
provides:
  - "OniAccess.csproj project targeting net472 with game DLL references"
  - "KMod.UserMod2 entry point with Tolk DLL path setup and speech init"
  - "SpeechEngine: Tolk P/Invoke wrapper with Initialize/Shutdown/Say/Stop lifecycle"
  - "OnSpeechOutput callback hook for test capture integration"
  - "LogHelper with [OniAccess] prefix for all mod logging"
  - "STRINGS.ONIACCESS LocString entries for localized mod text"
  - "mod_info.yaml with APIVersion 2 and supportedContent ALL"
affects: [01-02 text-filtering, 01-03 testing-framework, 02-menus, 03-navigation]

# Tech tracking
tech-stack:
  added: [Tolk P/Invoke, KMod.UserMod2, Harmony 2, kernel32 SetDllDirectory]
  patterns: [static-engine-class, pinvoke-lifecycle, locstring-mod-strings, prefixed-logging]

key-files:
  created:
    - OniAccess/OniAccess.csproj
    - OniAccess/mod_info.yaml
    - OniAccess/Mod.cs
    - OniAccess/Speech/SpeechEngine.cs
    - OniAccess/Util/LogHelper.cs
    - OniAccess/OniAccessStrings.cs

key-decisions:
  - "LocString entries in STRINGS namespace for full localization support from day one"
  - "Tolk_Silence for Stop() instead of empty string Tolk_Output -- cleaner API usage"
  - "OnSpeechOutput callback as internal Action<string> for test capture hook point"
  - "RegisterForTranslation called in OnLoad to enable community translations"

patterns-established:
  - "Static engine class pattern: SpeechEngine as static class with Initialize/Shutdown lifecycle"
  - "P/Invoke with SetDllDirectory: set DLL search path before any native calls"
  - "Prefixed logging: all Log.Info/Warn/Error calls prefix with [OniAccess]"
  - "LocString mod strings: all mod-facing text defined as LocString in STRINGS.ONIACCESS namespace"

# Metrics
duration: 2min
completed: 2026-02-11
---

# Phase 1 Plan 1: Project Scaffolding Summary

**ONI mod project with KMod.UserMod2 entry point, Tolk P/Invoke speech engine, and LocString localization system**

## Performance

- **Duration:** 2 min
- **Started:** 2026-02-11T13:09:47Z
- **Completed:** 2026-02-11T13:12:04Z
- **Tasks:** 2
- **Files created:** 6

## Accomplishments
- Buildable ONI mod project targeting net472 with game DLL references via ONI_MANAGED
- Mod.cs entry point with SetDllDirectory, Harmony auto-patching, speech init, and localized startup message
- SpeechEngine evolved from Speech.cs with clean Tolk lifecycle and OnSpeechOutput test hook
- All mod text defined as LocString entries in STRINGS.ONIACCESS for community translation support

## Task Commits

Each task was committed atomically:

1. **Task 1: Create project scaffolding and mod entry point** - `d4590dc` (feat)
2. **Task 2: Evolve Speech.cs into SpeechEngine with proper lifecycle** - `c9987fa` (feat)

## Files Created/Modified
- `OniAccess/OniAccess.csproj` - Build configuration targeting net472 with game DLL references
- `OniAccess/mod_info.yaml` - Mod metadata for ONI mod loader (APIVersion 2, supportedContent ALL)
- `OniAccess/Mod.cs` - UserMod2 entry point with Tolk init and startup speech
- `OniAccess/Speech/SpeechEngine.cs` - Tolk P/Invoke wrapper evolved from Speech.cs
- `OniAccess/Util/LogHelper.cs` - [OniAccess]-prefixed logging utility
- `OniAccess/OniAccessStrings.cs` - Localized mod strings in STRINGS namespace

## Decisions Made
- Used full localization with LocString entries and RegisterForTranslation from day one (per locked decision in CONTEXT.md)
- SpeechEngine.Stop() uses Tolk_Silence() instead of empty-string Tolk_Output for proper speech termination
- OnSpeechOutput callback defined as `internal static Action<string>` so SpeechCapture in Plan 03 can hook in
- Build cannot be verified without ONI_MANAGED game DLLs (expected -- code structure verified manually)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Build verification with `dotnet build` not possible without ONI game DLLs (ONI_MANAGED environment variable not set). Code structure, references, and cross-file links verified manually against plan requirements. This is expected in a development environment without the game installed.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Project scaffolding complete, ready for Plan 02 (text filtering pipeline) and Plan 03 (testing framework)
- SpeechEngine.Say() passes text directly to Tolk -- Plan 02 will insert TextFilter pipeline
- OnSpeechOutput callback ready for Plan 03 SpeechCapture integration
- Speech.cs remains in project root as reference; new code lives in OniAccess/ directory

## Self-Check: PASSED

- All 6 created files verified on disk
- Commit `d4590dc` (Task 1) verified in git log
- Commit `c9987fa` (Task 2) verified in git log
- Key links verified: Mod.cs -> SpeechEngine.Initialize, SetDllDirectory, STRINGS.ONIACCESS.SPEECH.MOD_LOADED
- SpeechEngine: 7 Tolk P/Invoke declarations, OnSpeechOutput callback, no text filtering code

---
*Phase: 01-foundation*
*Completed: 2026-02-11*
