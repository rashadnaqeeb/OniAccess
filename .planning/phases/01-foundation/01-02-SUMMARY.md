---
phase: 01-foundation
plan: 02
subsystem: speech
tags: [regex, text-filter, tdd, speech-pipeline, ring-buffer, dedup, csharp]

# Dependency graph
requires:
  - phase: 01-01
    provides: "SpeechEngine with Tolk P/Invoke, LogHelper, Mod.cs entry point"
provides:
  - "TextFilter.FilterForSpeech: 8-step regex pipeline stripping rich text, sprites, links, hotkeys"
  - "TextFilter.RegisterSprite: sprite-to-text mapping for meaningful sprite conversion"
  - "SpeechPipeline: central dispatch with SpeakInterrupt (navigation) and SpeakQueued (alerts)"
  - "Announcement readonly struct with Text, Priority, Interrupt, Category"
  - "AlertHistory: 100-entry ring buffer with 1-second dedup for Phase 6 navigation"
  - "TextFilterTests: 12 in-game test cases for text filtering validation"
affects: [01-03 testing-framework, 02-menus, 03-navigation, 06-notifications]

# Tech tracking
tech-stack:
  added: [compiled-regex-pipeline, ring-buffer]
  patterns: [speech-pipeline-dispatch, text-filter-chain, queue-dedup-count-suffix, alert-history-buffer]

key-files:
  created:
    - OniAccess/Speech/TextFilter.cs
    - OniAccess/Speech/Announcement.cs
    - OniAccess/Speech/SpeechPipeline.cs
    - OniAccess/Speech/AlertHistory.cs
    - OniAccess/Testing/TextFilterTests.cs
  modified:
    - OniAccess/Mod.cs

key-decisions:
  - "Sprite replacement appends trailing space for clean word separation, normalized by whitespace step"
  - "Queue drained immediately on SpeakQueued -- Tolk handles pacing internally, no per-frame dispatch needed"
  - "AlertHistory uses injectable GetTime delegate for testability without Unity runtime dependency"
  - "Interrupt speech does NOT go to alert history -- navigation is ephemeral"

patterns-established:
  - "Pipeline pattern: all speech flows Caller -> SpeechPipeline -> TextFilter -> SpeechEngine -> Tolk"
  - "No direct SpeechEngine.Say calls from game code -- only SpeechPipeline dispatches"
  - "Queue dedup with count suffix: same-category alerts combine as 'text x2'"
  - "In-game test class pattern: static RunAll returning List<(name, passed, detail)>"

# Metrics
duration: 4min
completed: 2026-02-11
---

# Phase 1 Plan 2: Speech Pipeline Summary

**Speech pipeline with TextFilter regex chain, interrupt/queue dispatch, queue dedup with count suffix, and 100-entry alert history ring buffer**

## Performance

- **Duration:** 4 min
- **Started:** 2026-02-11T13:14:36Z
- **Completed:** 2026-02-11T13:18:19Z
- **Tasks:** 2
- **Files created:** 5
- **Files modified:** 1

## Accomplishments
- TextFilter with 8-step regex pipeline handling all ONI rich text variants (bold, color, size, style, link, sprite, TMP brackets, hotkey placeholders)
- SpeechPipeline as the single dispatch point enforcing interrupt vs queue semantics per locked decisions
- Queue deduplication combining same-category alerts with count suffix ("Broken Wall x2")
- AlertHistory ring buffer (100 entries) with 1-second dedup window ready for Phase 6 navigation
- 12 TDD-style in-game test cases covering all filtering behavior

## Task Commits

Each task was committed atomically:

1. **Task 1: Create TextFilter with TDD -- rich text stripping and sprite conversion** - `57712dd` (feat)
2. **Task 2: Create SpeechPipeline, Announcement, and AlertHistory** - `77ec81c` (feat)

## Files Created/Modified
- `OniAccess/Speech/TextFilter.cs` - 8-step regex pipeline: sprite conversion, link extraction, hotkey stripping, tag removal, TMP cleanup, whitespace normalization
- `OniAccess/Speech/Announcement.cs` - Readonly struct with Text, Priority, Interrupt, Category for speech dispatch
- `OniAccess/Speech/SpeechPipeline.cs` - Central dispatch with SpeakInterrupt, SpeakQueued, Silence, SetEnabled for VanillaMode
- `OniAccess/Speech/AlertHistory.cs` - Ring buffer with AlertEntry class, Record with dedup, GetRecent, Clear
- `OniAccess/Testing/TextFilterTests.cs` - 12 test cases: tag stripping, sprite conversion, link handling, null/empty, plain text preservation
- `OniAccess/Mod.cs` - Updated to use SpeechPipeline.SpeakInterrupt for startup speech, added TextFilter.InitializeDefaults

## Decisions Made
- Sprite replacement text gets a trailing space appended so "warning:Pipe" becomes "warning: Pipe" -- whitespace normalization cleans up any doubles
- Queue is drained immediately on each SpeakQueued call rather than one-per-frame, since Tolk internally handles speech pacing/queuing
- AlertHistory.GetTime is an injectable delegate (defaults to 0f) for testability without requiring Unity runtime -- set to Time.time during mod init
- Interrupt speech is ephemeral and does NOT get recorded in alert history -- only queued alerts are captured for Phase 6 navigation

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Build verification with `dotnet build` not possible without ONI game DLLs (expected, same as Plan 01). Code structure, cross-file references, and regex patterns verified via manual trace of all 12 test cases against the filter pipeline.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Speech pipeline complete: all future phases call SpeechPipeline.SpeakInterrupt or SpeakQueued, never SpeechEngine directly
- TextFilter ready for incremental sprite additions as new ONI sprites are discovered (unrecognized sprites logged)
- AlertHistory buffer in place for Phase 6 to add navigation UI on top
- SpeechPipeline.SetEnabled ready for VanillaMode toggle (Plan 03)
- TextFilterTests ready for Plan 03 to integrate with SpeechCapture testing framework

## Self-Check: PASSED

- All 5 created files verified on disk
- Commit `57712dd` (Task 1) verified in git log
- Commit `77ec81c` (Task 2) verified in git log
- Key links verified: SpeechPipeline -> TextFilter.FilterForSpeech (2 calls)
- Key links verified: SpeechPipeline -> SpeechEngine.Say (interrupt=true and interrupt=false paths)
- Key links verified: SpeechPipeline -> AlertHistory.Record (2 calls in SpeakQueued)
- Key links verified: Mod.cs -> SpeechPipeline.SpeakInterrupt (not SpeechEngine.Say)

---
*Phase: 01-foundation*
*Completed: 2026-02-11*
