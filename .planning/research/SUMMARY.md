# Project Research Summary

**Project:** ONI Accessibility Mod (OniAccess)
**Domain:** Game Accessibility Modding - Screen Reader Support for Oxygen Not Included
**Researched:** 2026-02-11
**Confidence:** HIGH

## Executive Summary

The ONI accessibility mod is a greenfield screen reader accessibility layer for Oxygen Not Included, following the pioneering model of FactorioAccess. This domain is characterized by deep technical integration (Harmony patching of game internals), speech synthesis via screen reader abstraction (Tolk), and transformation of visual-spatial gameplay into navigable audio experiences. The recommended approach is a layered pipeline architecture with five core components: Speech Output (Tolk wrapper), Game State Reading (ONI component inspection), Announcement Pipeline (formatting and deduplication), Navigation/Cursor (keyboard-driven world exploration), and Input Handling (context-sensitive hotkey routing). The project is feasible but complex, estimated at 10+ weeks for table stakes features.

The critical success factors are: (1) preventing speech spam through aggressive deduplication and priority systems, (2) making ONI's 15 overlapping tile data layers manageable through a query-on-demand overlay system, and (3) designing for Harmony patch fragility with lazy reflection and dynamic type resolution. The primary risk is information overload — ONI has far more simultaneous data layers than any accessibility-modded game to date. The mitigation is a user-controlled layer system where overlay hotkeys (F1=oxygen, F2=power, etc.) toggle which data appears in cursor readout, matching the game's existing overlay paradigm to preserve compatibility with wiki guides and tutorials.

This is unprecedented work. While FactorioAccess provides architectural patterns (centralized cursor, state-driven speech, hierarchical description), ONI introduces entirely new challenges: autonomous duplicant management, multi-network conduit stacking, atmospheric sonification, and vertical world navigation. The mod will be the first to implement atmospheric data as continuous audio (three-tone gas mapping) and the first to make a colony simulation game with 3-30 autonomous agents accessible to blind players.

## Key Findings

### Recommended Stack

**Runtime:** Unity 2020.3 LTS with Mono runtime (NOT IL2CPP — critical for Harmony patching). ONI build 707956. Target framework net472, C# 9.0. Windows x64 only (Tolk is Windows-specific).

**Core technologies:**
- **KMod.UserMod2** — ONI's built-in mod entry point (ONLY supported mechanism). `OnLoad(Harmony harmony)` is the initialization hook. APIVersion 2 required for Harmony 2 compatibility.
- **Harmony 2.x** — Runtime IL patching via game-bundled `0Harmony.dll`. Do NOT bundle your own Harmony. Prefix/Postfix/Transpiler patterns. Lazy FieldRef initialization mandatory to avoid 4+ second startup lag.
- **Tolk (P/Invoke)** — Screen reader abstraction library supporting NVDA, JAWS, Window-Eyes, SuperNova, System Access, SAPI 5.3 fallback. LGPLv3. Project is inactive but API is stable. Already implemented in `Speech.cs` with rich text filtering and sprite tag conversion.
- **Newtonsoft.Json** — Settings persistence (ships with ONI). Store settings in mod directory, not game save files.
- **Unity references** — `UnityEngine.CoreModule`, `UnityEngine.UI`, `Unity.TextMeshPro`, `UnityEngine.InputLegacyModule` for game API access. All via ONI's `Managed/` directory with `Private="false"` to prevent copying.

**Critical dependency:** Never cache game state. ONI's continuous simulation (200ms/1000ms ticks) updates hundreds of values per frame. Always read from live game objects at point of speech output. Cached data gives blind players wrong information, which is worse than no information.

**Version compatibility:** Harmony patches are fragile. Use dynamic type resolution (`AccessTools.TypeByName`) for DLC-specific types. Use lazy FieldRef initialization. Prefer Postfix over Prefix. Wrap all patches in try-catch. Test after every game update.

### Expected Features

Based on FactorioAccess's 36-feature catalog and ONI's unique systems, categorized by necessity:

**Must have (table stakes) — 12 features:**
1. **Speech Output Pipeline** — Tolk integration with rich text filtering, interrupt/queue modes, screen reader detection
2. **Menu Navigation** — All 20+ menu screens keyboard-accessible with speech (main menu, options, new game, load, pause, mod manager)
3. **Grid Cursor with Tile Readout** — Arrow-key world navigation announcing element, temperature, buildings, state
4. **Building Construction** — Navigate 15 build categories, place buildings with rotation, cancel/deconstruct, pipe/wire routing
5. **Entity Inspection** — 100+ building-specific side screens with status, settings, contents, automation
6. **Duplicant Status and Management** — Query any duplicant's activity, location, health, stress, traits, skills. Follow/track duplicants.
7. **Priority System Access** — Navigate duplicant-by-chore priority grid, set per-building priorities
8. **Research Tree Navigation** — Browse technologies, view prerequisites/rewards, queue research
9. **Notification/Alert Awareness** — Surface game alerts as speech, cycle through active alerts
10. **Overlay Data as Speech** — Query 15 overlay layers (oxygen, power, plumbing, temperature, etc.) at cursor position
11. **Scanner/Entity Finder** — Hierarchical browser: category -> type -> instance sorted by distance. Jump cursor to entity.
12. **Save/Load Access** — Save game, load saves, access auto-saves through keyboard

Without these 12, the game is unplayable by blind users. Every screen reader game mod that achieved adoption includes all table stakes features.

**Should have (differentiators) — 14 features:**
1. **Atmospheric Sonification** — THREE-TONE SYSTEM: oxygen (clean), CO2 (rumble), foreign gas (alert), vacuum (silence). Pressure as volume. Temperature texture overlay. Novel audio design with no precedent.
2. **Room Recognition** — Announce room type and status on entry. Report requirements and missing items for upgrades.
3. **Schedule Editor** — Navigate 24-slot schedule grid per duplicant
4. **Colony Management Screens** — Vitals, Consumables, Reports with accessible table navigation
5. **Fast Travel/Bookmarks** — Named location teleportation
6. **Skills and Attributes** — Browse skill trees, assign points
7. **Automation System** — Configure sensors, logic gates, trace automation networks
8. **Critter and Ranching** — Ranch management, critter status, egg tracking
9. **Vanilla/Accessibility Toggle** — Single hotkey to silence mod and restore mouse control for sighted helpers
10. **Context-Sensitive Help** — In-game control reference for current screen
11. **Multi-Network Conduit Awareness** — Query 5 overlapping networks (liquid/gas/power/automation/conveyor) per tile
12. **Duplicant Selection at Printing Pod** — Compare candidate traits/attributes every 3 cycles
13. **Rocketry and Starmap (DLC)** — Full Spaced Out accessibility
14. **Speech Capture Testing** — Automated test framework for speech output regression prevention

These elevate the mod from "technically usable" to "genuinely enjoyable." Atmospheric sonification (D-1) would be groundbreaking — the first environmental awareness system using continuous audio encoding instead of speech.

**Defer (anti-features — deliberately NOT build):**
- Visual screen description ("panel on the left with 5 tabs")
- OCR/image recognition (have direct API access)
- Item count announcements ("item 3 of 10" — verbose)
- Navigation hints on every action ("Press Enter" — interrupts flow)
- Complete atmospheric simulation in speech (overwhelming)
- Cross-platform support (Tolk is Windows-only)
- Game simplification (blind players want full complexity)

### Architecture Approach

**Layered pipeline architecture** with clean component boundaries and bottom-up build order:

**Major components:**
1. **Speech Output Layer** (`SpeechEngine`, `SpeechQueue`, `Announcement`) — Single output sink. Wraps Tolk. Handles priority (Critical/High/Normal/Low), deduplication (200ms window, category-based), interrupt semantics. All speech funnels through this.
2. **Game State Reader Layer** (`CellInspector`, `BuildingInspector`, `DuplicantInspector`, `WorldInspector`, `UIInspector`) — Read-only game state access. Static utility classes. Return structured data (info records), not strings. No caching (always read fresh).
3. **Announcement Pipeline** (`Formatter`, `AnnouncementRouter`, `ChangeDetector`) — Transforms game data to concise text. Uses ONI's `STRINGS` for localization. Change detection (delta announcements). Category-based dedup. Verbosity levels (Compact/Normal/Detailed).
4. **Navigation/Cursor System** (`GridCursor`, `MenuCursor`, `CursorManager`, `SearchSystem`) — Dual cursor model: grid cursor for world, menu cursor for UI. Only one active at time. Announce-on-move pattern. World-aware (multi-asteroid DLC support). Search/jump-to for entities.
5. **Input Handling Layer** (`AccessKeyBindings`, `InputRouter`) — Context-sensitive hotkey routing. Use ONI's `Action` system. Respect `KButtonEvent.Consumed` flag. Avoid screen reader conflicts (no Insert/CapsLock). Repeatable arrow keys with debounce.
6. **Harmony Patch Layer** (thin patches) — Extract events/data, delegate to mod core. Prefer Postfix. 1-5 lines per patch. Organized by concern: UI, GameState, Input, Sim, Tooltip, Build. Lazy FieldRef initialization. Dynamic type resolution for DLC.

**Data flow:** User key press -> InputRouter -> Cursor.Move() -> StateInspector.GetInfo() -> Formatter.Format() -> AnnouncementRouter -> SpeechQueue -> SpeechEngine -> Tolk -> Screen reader

**Design philosophy:**
- Accessible experience over visual mimicry (don't describe pixels, present logical structure)
- Reuse game data over hardcoding (use `STRINGS`, `LocText`, component queries)
- Concise announcements (name, state, value — nothing more)
- State-driven, not event-driven (query on demand vs. tracking mutations)
- Anti-defensive coding (validate at boundaries, crash internally for diagnosable errors)

**FactorioAccess patterns adapted:**
- Centralized cursor (grid + menu dual model)
- Announce-on-move (every cursor step triggers readout)
- Hierarchical description (verbosity levels)
- Search/jump-to (hierarchical entity browser)
- Proactive alerts (notification system integration)
- State diffing (change detection for delta announcements)

### Critical Pitfalls

Based on Harmony modding gotchas, FactorioAccess lessons, and ONI-specific hazards:

1. **Stale Data from Caching** — ONI's simulation updates 200ms/1000ms ticks continuously. Cached building status, duplicant positions, or overlay values give blind players wrong info. NEVER cache game state. Always read live objects at speech time. Cache only expensive operations (world scanning) with explicit invalidation.

2. **Harmony Patch Fragility** — Klei updates ONI monthly. Field names change without warning (e.g., `closeButton` vs `CloseButton`). Private field access breaks silently. **Prevention:** Dynamic type resolution (`AccessTools.TypeByName`), lazy FieldRef init (`??=`), null-check AccessTools results, centralize reflection access, version logging, prefer Postfix over Prefix, try-catch all patches.

3. **Speech Spam and Audio Overload** — ONI generates torrents of state changes (duplicants move, errands shift, notifications fire). Naive "announce every change" produces unlistenable output. **Prevention:** 200ms time-based deduplication, category-based dedup (one announcement per category survives), interrupt for user actions only, queue for background alerts, throttle cursor movement (debounce 150-200ms), priority-based suppression during crises, per-feature opt-out.

4. **Multi-Layer Tile Information Overload** — Single ONI tile has 10+ overlapping data: element, temperature, mass, germs, decor, light, building, liquid pipe, gas pipe, automation wire, power wire, conveyor. Reading all on every cursor move is a 5+ second wall of text. **Prevention:** Minimal default readout (element + building only). Layer query system with modifier keys. Togglable continuous overlays (F1=oxygen, F2=power, match game hotkeys). Brevity mode for multi-overlay. Design information hierarchy before coding.

5. **Input Conflict with ONI's Extensive Hotkeys** — ONI uses almost every key (F1-F12 overlays, G/C/X/M/P/B tools, V/F/L/J/R/E/Z/U screens, WASD camera, Tab speed, Space pause, 1-9 priorities). Accessibility mod needs extensive bindings too. **Prevention:** Audit all ONI hotkeys first. Use `e.Consumed` properly. Reserve modifier prefix (Ctrl+Alt). Deliberately overwrite useless-to-blind-players hotkeys with documentation. Priority system (accessibility layer consumes navigation when active).

6. **FieldRef Startup Lag** — Creating 500+ `AccessTools.FieldRefAccess` delegates upfront causes 4+ second game freeze. **Prevention:** Lazy initialization pattern ALWAYS (`_field ??= AccessTools.FieldRefAccess(...)`). Batch per screen (resolve on first screen open). Never initialize in `OnLoad()`.

7. **UI Not Ready When Patch Fires** — Unity's layout system is deferred (end-of-frame). Reading child elements in `OnActivate` Postfix produces nulls or empty lists. **Prevention:** Coroutine delay 1-2 frames (`yield return new WaitForEndOfFrame()`), null-check at UI boundary, never assume child counts.

8. **Hardcoding Text Instead of Game Strings** — Literal English strings break localization and become stale. **Prevention:** Always read `LocText.text`, use `STRINGS` namespace, call `Strings.Get()` for dynamic lookups. Strip rich text (`Speech.FilterRichText`). Hardcode only mod-specific instructions.

9. **Treating ONI Like Static UI** — ONI is a simulation with 3-30 autonomous duplicants making decisions. World changes continuously without player input. **Prevention:** Proactive problem detection (hook status systems), periodic colony pulse summaries, alert queue cycling, duplicant activity tracking, build order confirmation.

10. **Ignoring Autonomous Agents** — Unlike FactorioAccess (player is sole agent), ONI has autonomous workers. Blind players need awareness of duplicant activities without manual queries. **Prevention:** Implement alert system, duplicant follow mode, priority system feedback, status change announcements.

## Implications for Roadmap

Based on research findings, architecture dependencies, and complexity assessment, suggested phase structure:

### Phase 0: Foundation (Week 1)
**Rationale:** Prove mod loads and can speak. Establish architectural patterns before features.
**Delivers:** `UserMod2` entry point, Tolk integration, "ONI Accessibility loaded" announcement through screen reader.
**Addresses:** Speech Output Pipeline (TS-1) foundation, Tolk DLL loading (Pitfall 8), lazy FieldRef pattern (Pitfall 5), modular architecture (Pitfall 9).
**Stack elements:** KMod.UserMod2, Harmony 2.x, Tolk P/Invoke, project scaffolding.
**Avoids:** Monolithic controller by establishing thin orchestrator from day one.

### Phase 1: Read the World (Weeks 2-3)
**Rationale:** Must read game state before anything can be announced. Depends on Phase 0 (speech works).
**Delivers:** `CellInspector`, `BuildingInspector`, `Formatter`, `AnnouncementRouter`. Can read and speak tile data on demand (not yet navigable).
**Implements:** Game State Reader Layer, Announcement Pipeline (basic).
**Addresses:** Table stakes TS-1 (speech pipeline extension).
**Avoids:** Stale data (Pitfall 1) by reading live state only, hardcoding text (Pitfall 7) by using `STRINGS`.

### Phase 2: Navigate the World (Weeks 3-5)
**Rationale:** Navigation is the core interaction. Depends on Phase 1 (can read state).
**Delivers:** Arrow-key grid cursor, speech on every move, `SpeechQueue` with deduplication, `ChangeDetector` for delta announcements. **FIRST GENUINELY USABLE MILESTONE.**
**Addresses:** Grid Cursor with Tile Readout (TS-3), Input Handling layer, deduplication (Pitfall 3).
**Avoids:** Speech spam (Pitfall 4) via 200ms dedup and throttle, input conflicts (Pitfall 5) via `e.Consumed` and debounce.
**Research flag:** Standard keyboard navigation patterns, well-documented. Skip `/gsd:research-phase`.

### Phase 3: Read the UI (Weeks 5-7)
**Rationale:** Players need menu access to start games, configure options. Depends on Phase 2 (navigation patterns established).
**Delivers:** `UIPatches`, `UIInspector`, `MenuCursor`, `CursorManager` context switching, tooltip capture. Navigate build menus, management screens, overlays.
**Addresses:** Menu Navigation (TS-2), dual cursor model, UI frame delay pattern (Pitfall 6).
**Avoids:** UI not ready (Pitfall 6) via coroutine delay, Dropdown issues (Pitfall 12) via `RefreshShownValue()` wrapper.
**Research flag:** Complex screens (ColonyDestinationSelectScreen, ModsScreen) may need screen-specific research during planning.

### Phase 4: Act in the World (Weeks 7-10)
**Rationale:** Players can navigate and see menus; now they need to perform game actions. Depends on Phases 2-3.
**Delivers:** Accessible building placement, `DuplicantInspector`, `WorldInspector`, `SearchSystem`, proactive alerts. Can build, manage, respond to problems.
**Addresses:** Building Construction (TS-4), Entity Inspection (TS-5), Duplicant Management (TS-6), Priority System (TS-7), Alerts (TS-9), Scanner (TS-11).
**Avoids:** Treating ONI like static UI (Pitfall 11) by implementing proactive status system and alert queue.
**Research flag:** Entity inspection side screens (100+ variants) and priority grid likely need deeper research. Consider `/gsd:research-phase` before starting.

### Phase 5: Overlays and Environmental Awareness (Weeks 10-12)
**Rationale:** Core gameplay works; now add the data layers that separate survival from mastery. Depends on all prior phases.
**Delivers:** 15 overlay layer queries (oxygen, power, plumbing, temperature, etc.), layer toggle system, multi-network conduit awareness, room recognition.
**Addresses:** Overlay Data as Speech (TS-10 — VERY HIGH COMPLEXITY), Multi-Network Conduits (D-11), Room Recognition (D-2).
**Avoids:** Multi-layer tile overload (Pitfall 10) via query-on-demand and togglable continuous overlays matching game F1-F12 hotkeys.
**Research flag:** HIGH. This is the most complex system with no precedent. Recommend `/gsd:research-phase` to research overlay API, pipe network tracing, and information hierarchy design.

### Phase 6: Polish and Advanced Features (Weeks 12+)
**Rationale:** Core playability complete; add QoL and customization.
**Delivers:** Configuration system, verbosity modes, save/load announcements, schedule editor, colony management screens, skills/attributes, fast travel/bookmarks, help system, vanilla mode toggle, speech capture testing.
**Addresses:** Research Tree (TS-8), Schedule Editor (D-3), Colony Screens (D-4), Skills (D-6), Bookmarks (D-5), Help (D-10), Vanilla Toggle (D-9), Testing (D-14).
**Avoids:** User frustration by allowing customization of verbosity and feature toggles.
**Research flag:** Standard patterns. Skip research.

### Phase 7 (Optional): Atmospheric Sonification (Weeks 13+)
**Rationale:** Experimental feature. High-risk, high-reward. Depends on Phase 5 (overlay data).
**Delivers:** Three-tone gas mapping (O2=clean, CO2=rumble, foreign=alert), pressure-as-volume, temperature texture. Real-time audio synthesis.
**Addresses:** Atmospheric Sonification (D-1 — VERY HIGH COMPLEXITY, NOVEL DESIGN).
**Research flag:** CRITICAL. Needs dedicated research phase for audio synthesis approaches, Unity audio integration, tone design, performance implications. Absolutely requires `/gsd:research-phase` and user testing with blind players.

### Phase 8 (Optional): DLC Support (Weeks 14+)
**Rationale:** Spaced Out and Bionic Booster Pack add major systems. Base game must work first.
**Delivers:** Multi-asteroid navigation, rocket construction, starmap (separate base/DLC implementations), bionic duplicant screens.
**Addresses:** Rocketry and Starmap (D-13), DLC compatibility (Pitfall 14).
**Avoids:** DLC matrix failures (Pitfall 14) via dynamic type resolution established in Phase 0.
**Research flag:** DLC-specific systems need research. Starmap especially (hex grid vs. distance rings).

### Phase Ordering Rationale

**Bottom-up dependency chain:**
- Speech must work before anything can be announced (Phase 0)
- State reading must work before navigation can report (Phase 1)
- Navigation establishes interaction model for UI (Phase 2 before 3)
- UI access needed before game actions (Phase 3 before 4)
- Basic actions needed before advanced overlays (Phase 4 before 5)

**Risk mitigation:**
- Hardest problems (overlays, sonification) deferred until patterns established
- Table stakes features completed by Phase 4 (core playability)
- Optional/experimental features (sonification, DLC) in separate phases

**User value delivery:**
- Phase 2: Can explore world (limited but functional)
- Phase 4: Can play game (build, manage, survive)
- Phase 5: Can optimize (overlays for troubleshooting)
- Phase 6: Can customize (settings, help)

**Complexity grouping:**
- Phases 0-2: LOW-MEDIUM complexity, well-documented patterns
- Phase 3: MEDIUM complexity, screen-specific adaptation needed
- Phase 4: HIGH complexity, 100+ side screen variants
- Phase 5: VERY HIGH complexity, unprecedented overlay system
- Phase 7: VERY HIGH complexity, novel audio design

### Research Flags

**Phases needing deeper research during planning:**
- **Phase 4 (Entity Inspection)** — 100+ building-specific side screens with dynamic content. Needs enumeration of common patterns and edge cases.
- **Phase 5 (Overlays)** — 15 overlay layers with no accessibility precedent. Needs API research for pipe network tracing, circuit queries, decor calculations. Information hierarchy design critical.
- **Phase 7 (Sonification)** — Novel audio design. Needs research on Unity audio synthesis, FMOD integration, real-time parameter control, tone design with blind user testing.
- **Phase 8 (DLC)** — Starmap variants, multi-asteroid systems. Needs DLC-specific API research.

**Phases with standard patterns (skip research-phase):**
- **Phase 1 (State Reading)** — Component queries, ONI API access. Standard C# patterns.
- **Phase 2 (Navigation)** — Keyboard cursor, announce-on-move. FactorioAccess provides direct precedent.
- **Phase 3 (UI)** — Menu navigation, screen patching. ONI modding community has established patterns.
- **Phase 6 (Polish)** — Configuration, help text, testing. Standard software patterns.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Verified from decompiled ONI source (build 707956), `Assembly-CSharp.csproj`, mod loader internals. Tolk already prototyped. Unity/Harmony integration well-documented. |
| Features | HIGH | Based on FactorioAccess 36-feature catalog (proven model), comprehensive ONI accessibility audit (20+ screens documented), and blind gaming community expectations. Table stakes vs. differentiators validated against successful mods. |
| Architecture | HIGH | Layered pipeline pattern proven by FactorioAccess. Component boundaries clear. Dependency chain logical. Harmony patching patterns established by ONI modding community. Build order validated bottom-up. |
| Pitfalls | HIGH | Harmony fragility documented in ONI modding community. Speech spam is known failure mode in game accessibility. Tolk integration gotchas documented. FactorioAccess anti-patterns provide cautionary lessons. |

**Overall confidence:** HIGH

This is a complex but feasible project. The stack is well-understood (Harmony modding for ONI is established), the architecture has proven precedent (FactorioAccess), and the pitfalls are documented. The primary unknowns are ONI-specific: the overlay system complexity (very high, no precedent) and atmospheric sonification design (experimental, requires user testing).

### Gaps to Address

Areas where research was inconclusive or needs validation during implementation:

- **Overlay API performance:** Unknown if querying 15 overlay layers per cursor movement causes frame drops. Needs profiling in Phase 5. Mitigation: query-on-demand rather than continuous readout.
- **Side screen variance:** 100+ building types with unique side screens. Research enumerated common patterns but edge cases will emerge. Mitigation: design `UIInspector` with per-screen adapter pattern, graceful fallback for unknown screens.
- **DLC type availability:** Which specific types exist in which DLC configurations needs runtime testing. Mitigation: dynamic type resolution established in Phase 0, null-check all DLC types.
- **Screen reader behavior differences:** Whether JAWS vs NVDA handle interrupt semantics differently under Tolk abstraction. Needs testing with both. Mitigation: Tolk should abstract this, but verify during Phase 0.
- **Automation network tracing:** How to efficiently trace automation wire connections across tiles. Game has internal graph but reflection API unknown. Needs investigation in Phase 5.
- **Priority grid performance:** Navigating a 20-column x 30-row priority matrix with speech feedback. Information density needs tuning. Defer to Phase 4 with user testing.
- **Atmospheric sonification audio synthesis:** Unity audio system vs FMOD, real-time parameter control, performance of continuous audio updates. Needs dedicated research phase before Phase 7.

**Validation strategy during planning:**
- Use `/gsd:research-phase` for Phases 4, 5, 7, 8 as needed
- Profile overlay queries in Phase 5 proof-of-concept
- Test with NVDA and JAWS during Phase 0
- Design per-screen adapters in Phase 3 to handle side screen variance in Phase 4
- Community feedback early and often (ONI modding Discord, blind gaming forums)

## Sources

### Primary (HIGH confidence)
- `docs/oni-internals.md` — Decompiled ONI source analysis (build 707956), mod loader internals, Harmony patterns, known gotchas
- `docs/oni_accessibility_audit.md` — Comprehensive audit of 20+ ONI screens with component hierarchy and interaction patterns
- `docs/oni-accessibility-analysis.md` — Feature-by-feature accessibility analysis with design recommendations
- `docs/fa-architecture-lessons.md` — FactorioAccess architecture (36 features, 4183-line control.lua analysis, anti-patterns)
- `docs/oni-accessibility-lessons.md` — Adaptation patterns from FactorioAccess to ONI/C#/Unity/Harmony
- `Speech.cs` (existing project code) — Tolk P/Invoke wrapper with rich text filtering, verified working prototype
- ONI Modding Community patterns (documented in research files) — Harmony best practices, lazy FieldRef, dynamic type resolution

### Secondary (MEDIUM confidence)
- `docs/fa-features.md` — FactorioAccess feature catalog (36 features detailed, community-validated)
- Blind gaming community expectations (synthesized from AudioGames.net patterns, accessible game design principles, FactorioAccess feedback)
- Stardew Access mod (mentioned as validation of Harmony + TTS approach for farming sim)
- Tolk library README — Screen reader API, COM considerations, threading warnings

### Tertiary (LOW confidence)
- Civilization accessibility efforts (limited community work, no comprehensive mod exists)
- Other strategy game accessibility (referenced at high level, detailed comparison not performed)
- Unity Test Framework considerations (testing in modded games inherently challenging, pragmatic manual approach recommended)

---
*Research completed: 2026-02-11*
*Research team: 4 parallel agents (STACK, FEATURES, ARCHITECTURE, PITFALLS) + synthesizer*
*Ready for roadmap: YES*
*Confidence: HIGH*
*Estimated complexity: 10+ weeks for table stakes, 14+ weeks for full feature set*
