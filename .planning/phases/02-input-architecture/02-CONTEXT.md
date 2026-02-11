# Phase 2: Input Architecture - Context

**Gathered:** 2026-02-11
**Status:** Ready for planning

<domain>
## Phase Boundary

Replace the flat HotkeyRegistry and InputInterceptor MonoBehaviour with a proper input handler system that intercepts keys before ONI processes them, routes input based on game state (world map vs menu vs build mode), and lets each game context own its own key handling. Redesign VanillaMode to disable all mod input handling, not just speech. Add context-sensitive F12 help that queries the active handler.

</domain>

<decisions>
## Implementation Decisions

### Mode announcements (handler convention for Phase 3+)
- Always announce context switches -- every handler activation should speak its name
- Name first, vary early: "Build menu" not "Menu, build"
- Mode announcements interrupt current speech
- Returning to world view is also announced
- Sequencing concern for Phase 3: when a menu opens, the mode announcement interrupts, then the focused-item announcement must queue after it (e.g., "Main menu" then "New game"). The input architecture should not block this sequencing. Capture this as a Phase 3 constraint.

### VanillaMode redesign
- Toggle OFF: speak "Oni-Access off" then full disable -- all input handlers deactivate, all keys pass through to game, speech stops. Only Ctrl+Shift+F12 remains active.
- Toggle ON: speak "Oni-Access on" only -- no state dump, no context announcement. Immediately detect current game state and activate appropriate handler.
- No background work when mod is off -- full stop. No passive state tracking.
- Toggle key stays Ctrl+Shift+F12

### Help system (F12)
- F12 opens a navigable list (arrow keys to step through entries), not a single speech dump
- Show only the active handler's keys -- no global keys mixed in
- No override information (don't mention what the game used the key for)
- When mod is on, there is always an active handler (context detection ensures this), so F12 always has content

### Key claim behavior
- Selective claim by default: each handler declares which keys it wants, everything else passes through to the game
- Full capture for menus: menu handlers block ALL keyboard input to prevent accidental game actions while navigating
- Silent handling: when the mod claims a key, it just handles it -- no feedback about the key being "stolen" from the game
- No passthrough modifier: VanillaMode is the escape hatch for full game access
- WASD/arrow key decisions are Phase 4 (world navigation), not Phase 2

### Claude's Discretion
- Handler architecture approach (IInputHandler stack vs Harmony hooks on KScreen.OnKeyDown vs state machine vs hybrid). Research will determine what works best with ONI's existing input system.
- Context detection mechanism (Harmony patches on OnActivate/OnDeactivate, KScreenManager polling, ToolMenu events, or combination)
- How the help mode handler itself works (it's a handler that takes over input to let arrows navigate the help list)
- Migration strategy from Phase 1's HotkeyRegistry/InputInterceptor to the new system

</decisions>

<specifics>
## Specific Ideas

- FactorioAccess EventManager priority chain (TEST > UI > WORLD) is the right mental model -- handlers should have priority ordering so UI eats keys before world
- ONI already has KScreen.OnKeyDown(KButtonEvent) with e.Consumed pattern -- explore hooking INTO this rather than building a parallel system
- KScreen lifecycle (OnActivate/OnDeactivate) provides natural handler activation/deactivation signals
- The "announce then queue" sequencing for menus (mode name interrupts, then focused item queues) must work by Phase 3 -- note this in Phase 3 requirements
- Arrow keys are completely unbound in ONI, safe for mod use in all contexts

</specifics>

<deferred>
## Deferred Ideas

- WASD camera panning behavior when cursor is active -- Phase 4 (World Navigation) decision
- Actual mode announcement speech text for specific screens -- Phase 3+ handlers implement this
- Menu-specific full-capture key lists -- Phase 3 (Menu Navigation) defines which keys menus capture
- Speech sequencing for "container then focused item" -- Phase 3 constraint

</deferred>

---

*Phase: 02-input-architecture*
*Context gathered: 2026-02-11*
