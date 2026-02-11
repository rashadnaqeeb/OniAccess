# Phase 1: Foundation - Context

**Gathered:** 2026-02-11
**Status:** Ready for planning

<domain>
## Phase Boundary

Mod loads reliably, speaks through the user's screen reader via Tolk, and establishes the architectural patterns (speech formatting, announcement behavior, hotkey registration, text filtering, testing) that every subsequent phase builds on. Speech.cs already exists with Tolk integration -- build on it, don't rewrite.

</domain>

<decisions>
## Implementation Decisions

### Speech style & formatting
- Terse label format: "Copper Ore, 200 kg, 25°C" -- compact, front-load useful information
- One verbosity level for now; tile-specific toggles deferred to Phase 3
- Abbreviated units: "25°C, 200 kg, 500 W" -- screen readers handle common abbreviations well
- Comma-separated compound readouts: "Copper Ore, 200 kg, 25°C, Solid"
- Inline lists without counts: "Requires 400 kg Metal Ore, 1 kg Glass"
- Follow game's temperature unit setting (Celsius/Fahrenheit/Kelvin)
- Use exact game names for elements/materials (Abyssalite, Polluted Oxygen, Neutronium) -- matches wiki and community
- No extra category prefixes in most cases; revisit per-context as needed
- Color-coded text: convert to meaning where possible, but context-specific -- decide per instance as they arise

### Text filtering
- Rich text tags: replace meaningful tags with words (warning icon sprite -> "warning:"), silently strip decorative tags
- All output must be clean readable text with no raw rich text tags or sprite codes

### Localization
- Prefer full localization (create STRINGS entries for mod text too) if practical
- Fallback: use STRINGS/LocText for game-data strings, English for mod-specific text if full localization proves impractical
- Researcher should assess feasibility of full localization approach

### Announcement behavior
- Navigation speech (cursor movement): interrupts previous speech for responsiveness
- Alert/notification speech: queues, plays in order, never dropped
- Duplicate simultaneous alerts: combine with count ("Broken Wall x2")
- Same behavior whether game is paused or unpaused
- No speech logging -- testing framework handles verification
- No earcons/sound cues for now -- speech only, evaluate earcons as future enhancement
- No separate mute function -- mod toggle (on/off) is sufficient

### Alert history
- Create a history buffer for alerts, navigable like a menu via hotkey
- Pressing Enter on an alert jumps to its location
- May overlap with existing in-game notification system -- researcher should investigate
- Full implementation likely Phase 6; Phase 1 establishes buffer infrastructure if practical

### Hotkey conventions
- No blanket modifier key -- each hotkey decided individually based on context and conflicts
- Researcher must map ALL existing game hotkeys to inform per-key decisions
- Context-sensitive: same key can do different things in different game states (arrows navigate world vs. menu)
- Context-aware help command: a hotkey lists available commands for current game state
- When mod overrides a game hotkey, no announcement to user (discussed during development)
- Only the toggle hotkey remains active when mod is off; all other mod hotkeys deactivated

### Hotkey system architecture
- Context-sensitive registration: keys bound per game state, not globally
- Help text generation from hotkey registry (architecture is Claude's discretion)
- Must handle same key in multiple contexts cleanly

### Startup & toggle
- On launch: "Oni-Access version [X] loaded"
- Toggle off: "Oni-Access off" -- speech stops, only toggle hotkey remains active, game behaves normally
- Toggle on: "Oni-Access on" -- brief confirmation, no state readout
- Tolk handles screen reader detection and SAPI fallback natively -- no extra fallback logic needed

### Claude's Discretion
- Hotkey system architecture (state machine, IKeyHandler, or other pattern -- based on ONI's input system)
- Help text registry design (central vs co-located -- must support context-sensitive keys)
- Loading skeleton / progress indicator design
- Exact text filtering rules for which tags carry meaning vs decoration
- Speech pipeline internals (queue management, interrupt handling implementation)
- Alert history buffer architecture (if included in Phase 1)

</decisions>

<specifics>
## Specific Ideas

- "Saying a lot of useful information is OK as long as it's compact and correctly ordered to front-load useful information"
- Alert history should work like a menu -- open with hotkey, navigate with arrows, Enter to jump to location
- Hotkey conflicts should be discussed case-by-case during development, not solved with a blanket modifier
- Build on existing Speech.cs with Tolk integration, don't rewrite

</specifics>

<deferred>
## Deferred Ideas

- Tile-specific verbosity toggles -- Phase 3 (World Navigation)
- Earcon/sound cue system alongside speech -- future enhancement after base works
- Full alert history with navigation and jump-to-location -- Phase 6 (Notifications)
- Per-context category prefixes -- revisit as specific features are built

</deferred>

---

*Phase: 01-foundation*
*Context gathered: 2026-02-11*
