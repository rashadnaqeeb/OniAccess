# Features Research: Screen Reader Accessibility for ONI

**Research date:** 2026-02-11
**Scope:** Features dimension for ONI accessibility modding
**Sources:** FactorioAccess mod (36-feature catalog in docs/fa-features.md), ONI UI audit (docs/oni_accessibility_audit.md), ONI accessibility analysis (docs/oni-accessibility-analysis.md), FactorioAccess architecture lessons (docs/fa-architecture-lessons.md, docs/oni-accessibility-lessons.md), blind gaming community patterns
**Downstream consumer:** Requirements definition

---

## Prior Art: What Exists

### FactorioAccess (Primary Reference)

FactorioAccess is the gold standard for screen reader accessibility in a complex strategy/simulation game. It makes Factorio fully playable by blind players through 36 interconnected feature systems. Key capabilities:

- **Speech pipeline**: Game state -> information layer -> message assembly -> screen reader output via external launcher bridge
- **Cursor navigation**: Tile-by-tile keyboard movement replacing mouse, with coordinate and content announcement
- **Scanner system**: Hierarchical entity browser (category -> subcategory -> instance sorted by distance) turning spatial exploration into list navigation with teleportation
- **Accessible UI framework**: KeyGraph directed graph navigation for all menus, with WASD movement, Tab sections, search, and a central Router managing a UI stack
- **Building and construction**: Keyboard placement, rotation, drag-to-build, build lock (auto-build while walking)
- **Spatial audio**: Stereo pan for direction, low-pass filter for behind, volume for distance
- **Sonification**: Continuous audio encoding of streaming data (health as pan, inserters as tones) while reserving speech for discrete events
- **Entity inspection**: Dynamic tab system per entity type, two-level tab navigation for complex entities
- **Alert system**: Proactive problem detection, alert cycling, warning scanning for factory problems
- **Fast travel**: Bookmark-based teleportation with named waypoints
- **Audio rulers**: Invisible alignment guides that play sounds when crossed for grid-based building
- **Vanilla mode**: Toggle between accessibility and sighted play, allowing helpers to use the same save
- **Settings**: Per-feature opt-out for all sonification and audio systems
- **Help system**: Context-sensitive documentation accessible from any screen (Shift+/)
- **Localization**: All text through localization keys, never hardcoded strings
- **Testing**: Speech capture framework for automated regression testing of announcements

### Civilization Accessibility

Civilization games have limited native accessibility. Community efforts include:

- **Civ VI screen magnification mods**: UI scaling, not screen reader support. No functional blind play.
- **Civ V strategic view**: Text-heavy mode used by some low-vision players, but not screen reader compatible.
- **No Civilization screen reader mod exists** that approaches FactorioAccess's depth. The turn-based nature theoretically makes Civ more feasible, but nobody has built it.

### Other Strategy Game Accessibility Efforts

- **Quentin C's Playroom / AudioGames.net community**: The blind gaming community has long-standing forums and communities. Their primary concern is that mainstream games are almost entirely inaccessible. The games they play are purpose-built audio games (MUDs, audio RPGs, accessible card games), not modded visual games.
- **The Last of Us Part II**: Landmark AAA accessibility, but focused on motor/visual/hearing within a linear game. Not comparable to open-ended simulation.
- **Hearthstone accessibility overlay (unofficial)**: Read card text and board state via OCR and screen readers. Fragile, broke on updates.
- **Stardew Access mod**: A Harmony/C# mod for Stardew Valley providing screen reader accessibility. Simpler game (farming sim) but validates the Harmony + TTS approach. Uses tile scanning, menu reading, and keyboard navigation.
- **A Hero's Call / Swamp / Manamon**: Purpose-built accessible RPGs from the blind gaming community. Not mods, but establish expectations: keyboard-only, full speech output, audio cues for spatial information, no visual dependency.

---

## Blind Gaming Community Expectations

Based on community patterns from AudioGames.net, AppleVis gaming discussions, FactorioAccess user feedback, and accessible game design literature:

### Non-negotiable expectations (users will not try the mod without these)

1. **Works with their screen reader**: NVDA and JAWS support are mandatory. SAPI fallback for users without a full screen reader.
2. **Keyboard-only operation**: Every action achievable without mouse. No "click this button" paths.
3. **Speech output for all navigation**: Every menu, every button, every state change announced.
4. **Interrupt behavior**: New speech cancels old speech (standard screen reader pattern). Users control reading pace.
5. **No visual dependency**: The mod must never require seeing the screen to proceed. Every workflow must be completable purely through audio.

### Strong expectations (users will leave quickly without these)

6. **Concise announcements**: Experienced screen reader users read at 300-500 WPM. Verbose descriptions are hostile. Name, state, value -- nothing more.
7. **Consistent navigation patterns**: Same keys do the same things everywhere. If arrows navigate lists, they navigate ALL lists.
8. **Context-sensitive information**: Show what is relevant NOW, not everything available. Detail on demand.
9. **Error recovery**: If something goes wrong (wrong building placed, wrong screen opened), there must be a clear way back. Undo, cancel, escape.
10. **Hotkey discoverability**: A help system or key listing accessible in-game. Users cannot read documentation while playing.

### Appreciated but not expected (users will be delighted by these)

11. **Spatial audio**: Directional/distance information through sound, not just speech.
12. **Customization**: Adjustable verbosity, toggleable features, keybinding remapping.
13. **Tutorial/onboarding**: Purpose-built accessible tutorial, not just "figure it out."
14. **Active community responsiveness**: Updates when the base game updates. Broken mods are abandoned mods.

---

## Feature Catalog: Table Stakes vs. Differentiators vs. Anti-Features

### Table Stakes (Must Have or Users Leave)

These are features without which the mod has no viable user base. Every screen reader game mod that has achieved adoption includes all of these.

---

#### TS-1. Speech Output Pipeline

**What:** Game state transformed to concise text and sent to screen reader via Tolk (NVDA, JAWS, SAPI fallback). Rich text stripped, sprite tags converted to words.
**Why table stakes:** Without speech, nothing works. This is the foundation.
**Complexity:** Low-medium. Speech.cs already exists with Tolk P/Invoke, rich text filtering, sprite tag conversion.
**Dependencies:** None (foundation for everything).
**Precedent:** FactorioAccess speech pipeline, Stardew Access TTS.

---

#### TS-2. Menu Navigation (All Menus)

**What:** Keyboard navigation of main menu, options, new game setup, colony destination selection, load game, mod manager, pause menu. Every button, every dropdown, every slider announced and operable.
**Why table stakes:** Players cannot even start the game without this.
**Complexity:** Medium. ONI has 20+ menu screens with varied layouts. Must intercept Unity/Klei UI framework (KScreen, KButton, LocText, KSlider). Colony destination selection is the most complex (multi-panel with asteroid details, traits, difficulty settings).
**Dependencies:** TS-1 (speech).
**Precedent:** FactorioAccess accessible UI framework. Stardew Access menu reading.

---

#### TS-3. Grid Cursor with Tile Readout

**What:** Arrow-key cursor movement on the game world grid. Each tile announces: element (granite, oxygen, vacuum), temperature, building if present, and basic state. Coordinates spoken on demand.
**Why table stakes:** This is how the player perceives the world. Without it, the game is a void.
**Complexity:** Medium. ONI tiles have 10+ overlapping data layers. The base readout must be selective (element + building + key status), with detail available on demand. Vertical axis matters (gravity, gas layering). Must handle cursor at world boundaries.
**Dependencies:** TS-1 (speech).
**Precedent:** FactorioAccess cursor system (horizontal only). ONI adds vertical axis, making this an adapted concept.

---

#### TS-4. Building Construction (Build Menu + Placement + Deconstruction)

**What:** Navigate the build menu by category (15 categories) and item, place buildings on the grid with rotation/orientation, cancel/deconstruct buildings. Pipe and wire routing tile-by-tile.
**Why table stakes:** Building is the core gameplay loop. A player who cannot build has nothing to do.
**Complexity:** High. 15 build categories with subcategories. Building placement must confirm validity (material availability, placement rules, overlap). Pipe/wire routing needs to announce connections and direction. Drag-to-build for wires/pipes/tiles is essential.
**Dependencies:** TS-1 (speech), TS-3 (cursor).
**Precedent:** FactorioAccess building and construction. Direct map but ONI's category hierarchy is deeper.

---

#### TS-5. Entity Inspection (Building Detail Panels)

**What:** Inspect any building/entity to get status, settings, contents, connections. Navigate the building's side screen to view and modify settings (e.g., set temperature threshold on Thermo Sensor, assign duplicant to building, set material filter).
**Why table stakes:** Every building in ONI has a complex side screen. Without access to these, the player cannot configure anything.
**Complexity:** Very high. ONI has 100+ side screen variants. Each building type has different panels: status, material delivery, filters, automation, priority, duplicant assignment, and building-specific controls. Must handle dynamic content that updates in real-time.
**Dependencies:** TS-1 (speech), TS-3 (cursor).
**Precedent:** FactorioAccess entity inspection with dynamic tabs. Significant adaptation needed for ONI's side screen system.

---

#### TS-6. Duplicant Status and Management

**What:** Query any duplicant's current activity, location, health, stress, attributes, traits, skills, and active effects. Follow a duplicant (move cursor to track them). Select duplicants from a list.
**Why table stakes:** Duplicants are the colony's workforce. A player who cannot monitor them cannot play. ONI has 3-30+ duplicants, each an autonomous agent.
**Complexity:** High. Duplicant data is spread across multiple components (MinionIdentity, MinionResume, Klei.AI attributes/effects/traits). Must aggregate meaningfully and present concisely. "Meep: Mining granite at (42, 15). Stress 12%. Hungry."
**Dependencies:** TS-1 (speech), TS-3 (cursor).
**Precedent:** No direct FactorioAccess equivalent (Factorio has no autonomous agents). Entirely new invention.

---

#### TS-7. Priority System Access

**What:** Navigate and modify the duplicant priority grid (duplicant-by-chore-type matrix with 1-9 priority values). Set per-building priorities. Understand which duplicants will do which tasks.
**Why table stakes:** Priority management is how players control what gets done. Without it, the colony runs on default priorities (poorly).
**Complexity:** High. The priority screen is a complex 2D grid with ~20 chore types per duplicant across all duplicants. Must be navigable as rows (by duplicant) and columns (by chore type). Per-building priority override must be accessible from building inspection.
**Dependencies:** TS-1 (speech), TS-6 (duplicant management).
**Precedent:** No direct equivalent. The schedule editor in FactorioAccess (24-slot time block) is conceptually similar but simpler.

---

#### TS-8. Research Tree Navigation

**What:** Browse available technologies, view prerequisites and rewards, queue research. Track research progress.
**Why table stakes:** Research unlocks new buildings and systems. Without it, players are stuck with starting technology.
**Complexity:** Medium. ONI's research tree is a directed graph but navigable as sorted lists (available, locked, completed). Each tech has prerequisites, costs, and unlocks. Queue management needed.
**Dependencies:** TS-1 (speech).
**Precedent:** FactorioAccess research menu. Direct map with minor adaptation.

---

#### TS-9. Notification/Alert Awareness

**What:** Surface game notifications as speech. "Building lacks material." "Duplicant idle." "Flooding detected." Cycle through active alerts with a hotkey.
**Why table stakes:** ONI communicates problems through visual notification banners. Blind players miss all of them. Missing alerts means the colony dies silently.
**Complexity:** Medium. ONI has a notification system (Notification class, NotificationManager). Must hook into it and present alerts with priority/severity ordering. Deduplication needed (don't announce "low oxygen" 50 times).
**Dependencies:** TS-1 (speech).
**Precedent:** FactorioAccess alert system. Direct map.

---

#### TS-10. Overlay Data as Speech

**What:** Toggle speech overlays that add layer-specific data to tile readout. Oxygen overlay: announce gas type, mass, pressure. Power overlay: announce wattage, circuit status. Temperature overlay: announce exact temps. At minimum: oxygen, power, plumbing, ventilation, temperature.
**Why table stakes:** ONI's 15 overlays are how sighted players understand their colony's invisible systems (gas flow, power distribution, temperature gradients). Without overlay data, blind players cannot diagnose problems.
**Complexity:** Very high. 15 overlay modes, each extracting different data from tile state. Must be combinable (query multiple overlays at cursor position). Some overlays require network tracing (power circuits, pipe networks). Must not overwhelm with data -- query-on-demand primary, continuous secondary.
**Dependencies:** TS-1 (speech), TS-3 (cursor).
**Precedent:** No direct FactorioAccess equivalent. The ALT-mode info in Factorio is simpler (single overlay). This is ONI's biggest accessibility challenge and the biggest new invention.

---

#### TS-11. Scanner/Entity Finder

**What:** Hierarchical entity browser: select a category (buildings, duplicants, critters, geysers), subcategory (by type), instance (sorted by distance). Jump cursor to selected entity.
**Why table stakes:** Players need to find things without visually scanning the screen. A colony with 200 buildings is unnavigable without a find system.
**Complexity:** Medium-high. Must categorize all ONI entity types meaningfully. Distance sorting needs vertical-aware distance calculation. Categories must be useful (by function, by status, by problem).
**Dependencies:** TS-1 (speech), TS-3 (cursor).
**Precedent:** FactorioAccess scanner system. Core concept maps directly; categories need ONI-specific design.

---

#### TS-12. Save/Load Access

**What:** Save the game, load saves, access auto-saves. All through keyboard with speech feedback.
**Why table stakes:** If the player cannot save, they lose all progress.
**Complexity:** Low. Save/load are standard menu screens.
**Dependencies:** TS-2 (menu navigation).
**Precedent:** FactorioAccess save/load. Direct map.

---

### Differentiators (Competitive Advantage / Quality of Life)

These features elevate the mod from "technically usable" to "genuinely enjoyable." No precedent mod has all of these. They are what would make ONI accessibility remarkable rather than merely functional.

---

#### D-1. Atmospheric Sonification

**What:** Continuous audio layer encoding atmospheric data without speech. Three-tone system: oxygen (clean tone), carbon dioxide (low rumble), foreign gas (alert tone), vacuum (silence). Pressure encoded as volume (louder = higher pressure). Optional temperature texture overlay (warm crackle, cold shimmer). Focused modes for tracking specific gases.
**Why differentiating:** Transforms gas management from "query every tile" to "sweep and listen." Enables intuitive environmental awareness that speech alone cannot achieve. This would be the first atmospheric sonification system in any game accessibility mod.
**Complexity:** Very high. Requires real-time audio synthesis or carefully designed audio samples with dynamic parameter control. Must integrate with Unity's audio system. Performance-critical (runs every frame). Extensive user testing needed for tone design.
**Dependencies:** TS-3 (cursor), TS-10 (overlay data for the underlying gas queries).
**Precedent:** FactorioAccess has spatial audio and sonification (inserter tones, combat radar, health pan), but nothing comparable to atmospheric mapping. The ONI accessibility analysis doc designed this system in detail.

---

#### D-2. Room Recognition and Reporting

**What:** Announce room type and status when cursor enters a recognized room. "Barracks: 4/6 beds, missing Comfy Bed for upgrade to Bedroom." Report room requirements and what is missing.
**Why differentiating:** ONI's room system grants bonuses (morale, skill points, efficiency) for correctly configured rooms. Sighted players see room overlays. This makes room planning accessible without visual overlays.
**Complexity:** Medium. ONI has a Room system with RoomTypes database. Can query room membership at any tile and compare against requirements. The data is there; the challenge is concise presentation.
**Dependencies:** TS-3 (cursor), TS-10 (overlay system).
**Precedent:** No equivalent in any accessibility mod.

---

#### D-3. Schedule Editor Access

**What:** Navigate and edit the 24-time-slot schedule grid for each duplicant. Assign blocks (sleep, work, bathtime, downtime) and manage schedule groups.
**Why differentiating:** Schedule management is important for mid-game efficiency but not strictly necessary for survival (default schedule works). Access to it separates "playing the game" from "mastering the game."
**Complexity:** Medium. A 24-column grid per duplicant, with schedule group management. Standard 2D grid navigation pattern.
**Dependencies:** TS-1 (speech), TS-6 (duplicant management).
**Precedent:** No direct equivalent. FactorioAccess has no time management.

---

#### D-4. Colony Management Screens (Vitals, Consumables, Reports)

**What:** Accessible navigation of the colony-wide management screens: Vitals (all duplicant health/stress at a glance), Consumables (food permissions per duplicant), Reports (colony statistics over time), Colony Summary.
**Why differentiating:** These screens are how experienced players monitor colony-wide trends. Without them the game is playable but the player is flying blind on macro trends.
**Complexity:** Medium-high. These are complex table/grid UIs with sorting, filtering, and per-duplicant rows. Must present tabular data accessibly (row-by-row with column headers).
**Dependencies:** TS-1 (speech), TS-6 (duplicant management).
**Precedent:** No direct equivalent. FactorioAccess has production statistics but not per-agent management screens.

---

#### D-5. Fast Travel / Bookmarks

**What:** Save named locations, teleport cursor to them. Quick-jump to key colony areas (main base, power plant, farm, etc.).
**Why differentiating:** Navigation efficiency. Without bookmarks, finding a specific area means scanning or using the entity finder. With bookmarks, experienced players can jump instantly.
**Complexity:** Low. Bookmark list with save/load/teleport. Simple state management.
**Dependencies:** TS-3 (cursor).
**Precedent:** FactorioAccess fast travel. Direct map.

---

#### D-6. Skills and Attribute Tree Navigation

**What:** Browse duplicant skills, view requirements and effects, assign skill points. Navigate the skill tree structure.
**Why differentiating:** Skill assignment is how duplicants specialize. Default skills work for early game but mid-to-late game requires deliberate skill planning.
**Complexity:** Medium. Skill tree is a directed graph but small per duplicant. Must show prerequisites, morale requirements, and effects clearly.
**Dependencies:** TS-1 (speech), TS-6 (duplicant management).
**Precedent:** No direct equivalent in FactorioAccess.

---

#### D-7. Automation System Access

**What:** Connect automation wires, configure logic gates, set sensor thresholds (temperature sensor ranges, atmo sensor ranges, timer durations). Read automation network state.
**Why differentiating:** Automation is what separates functional colonies from optimized ones. It is mid-to-late game but transformative when available.
**Complexity:** High. Sensor configuration involves sliders with precise ranges. Logic gate state requires understanding signal flow. Automation overlay data needed to trace connections.
**Dependencies:** TS-4 (building/wiring), TS-5 (entity inspection), TS-10 (overlay data).
**Precedent:** FactorioAccess circuit networks. Concept maps but ONI's automation is simpler (binary signals vs. combinators).

---

#### D-8. Critter and Ranching Management

**What:** Find critters, view their status (happiness, egg progress, wild/tame), manage ranching stables, track egg production and critter populations.
**Why differentiating:** Ranching is an entire mid-game subsystem that provides food and resources. Not needed for survival but important for colony optimization.
**Complexity:** Medium. Critter data accessible through standard entity inspection. Ranch management requires tracking room/critter associations and population counts.
**Dependencies:** TS-5 (entity inspection), TS-11 (scanner), D-2 (room recognition).
**Precedent:** No equivalent in FactorioAccess (Factorio has enemy biters, not ranch animals).

---

#### D-9. Vanilla/Accessibility Mode Toggle

**What:** Single hotkey to toggle between full accessibility mode and vanilla mouse play. Silences speech, hides keyboard cursor, re-enables mouse. Preserves all state.
**Why differentiating:** Enables sighted helpers to use the same save. Enables partially sighted players to switch modes. Enables streamers to demonstrate both modes. Enables developers to test visually.
**Complexity:** Low. Must design all Harmony patches to be toggleable without breaking game state. Whitelist essential hooks (state tracking) while suppressing speech and keyboard overlay.
**Dependencies:** All features (must be designed into the architecture from the start).
**Precedent:** FactorioAccess vanilla mode. Direct map.

---

#### D-10. Context-Sensitive Help System

**What:** Press a key from any screen to get help text describing available controls and current context. "You are in the Build menu. Left/Right: switch category. Up/Down: browse items. Enter: place. Escape: cancel."
**Why differentiating:** Blind players cannot read tooltips, watch tutorials, or reference keyboard overlays. In-game help is the only reliable way to learn the mod.
**Complexity:** Medium. Must write help text for every context. Content creation is the bottleneck, not code.
**Dependencies:** TS-1 (speech), TS-2 (menu navigation).
**Precedent:** FactorioAccess help system (Shift+/). Direct map in concept.

---

#### D-11. Multi-Network Conduit Awareness

**What:** At any tile, query which conduit networks pass through it. "Liquid pipe: polluted water, 4.2 kg. Gas pipe: oxygen, 1.8 kg. Power wire: 1.2 kW, Heavy-Watt Wire." Trace pipe/wire networks to see where they connect.
**Why differentiating:** ONI has 5 overlapping conduit networks (liquid pipe, gas pipe, power wire, automation wire, conveyor rail) potentially on the same tile. Sighted players see these through overlays. This is essential for debugging plumbing and power issues.
**Complexity:** High. Must query 5 independent network systems per tile. Network tracing (follow a pipe to see where it goes) is complex. Must present without overwhelming.
**Dependencies:** TS-3 (cursor), TS-10 (overlay data).
**Precedent:** FactorioAccess has transport belt, pipe, electrical, and circuit network inspection. Concept maps but ONI has more overlapping networks on same tiles.

---

#### D-12. Duplicant Selection at Printing Pod

**What:** When new duplicants are offered (every 3 cycles), compare their traits, attributes, and interests. Select or reject. "Meep: +7 Athletics, Diver's Lungs (no oxygen drain swimming), Flatulent (emits natural gas)."
**Why differentiating:** Choosing the right duplicants is a key strategic decision. Without accessible comparison, players must accept random duplicants.
**Complexity:** Medium. The Printing Pod's "Immigration" screen has a comparison layout. Must present 3 candidates with their stats in a navigable format.
**Dependencies:** TS-1 (speech), TS-6 (duplicant management).
**Precedent:** No equivalent in FactorioAccess.

---

#### D-13. Rocketry and Starmap Navigation (DLC)

**What:** Build and launch rockets, navigate the starmap, manage multi-asteroid colonies, assign duplicants to rocket missions. Full Spaced Out DLC accessibility.
**Why differentiating:** Spaced Out is the major DLC and adds an entire new dimension. Without it, DLC owners cannot access half their content.
**Complexity:** Very high. Starmap is a separate navigational domain. Multi-asteroid management means switching context between colonies. Rocket construction and module management. New UI screens throughout.
**Dependencies:** Nearly all table stakes features, plus DLC-specific systems.
**Precedent:** No equivalent in FactorioAccess (Factorio 2.0 Space Age was not supported when the project docs were created).

---

#### D-14. Speech Capture Testing Framework

**What:** Automated testing infrastructure that captures all speech output during test scenarios and asserts correct announcements. "When inspecting an electrolyzer with no power, speech includes 'no power'."
**Why differentiating:** Prevents regressions. A code change that silently breaks what blind players hear is invisible to sighted developers. Automated speech testing is the only reliable guard.
**Complexity:** Low-medium. Requires a capture mode in the speech system and test harness integration.
**Dependencies:** TS-1 (speech).
**Precedent:** FactorioAccess speech capture testing. Direct map.

---

### Anti-Features (Things to Deliberately NOT Build)

These are features that might seem necessary but should be consciously avoided. They would waste development effort, create maintenance burden, or actively harm the user experience.

---

#### AF-1. Visual Screen Description

**Do not** attempt to describe the visual layout of the screen. "There is a panel on the left with 5 tabs. The first tab shows..." This is a visual translation, not an accessible experience.
**Why anti-feature:** Blind players do not need to know what the screen looks like. They need to know what they can DO and what the current STATE is. FactorioAccess's core insight: reimagine the interaction model around audio, do not describe the visual.
**Instead:** Announce the content and available actions. "Electrolyzer: Active, producing Oxygen. 120W. Priority 5. Settings available."

---

#### AF-2. OCR/Image Recognition

**Do not** attempt to read screen content through OCR or image analysis.
**Why anti-feature:** Fragile, slow, inaccurate, and unnecessary. We have direct access to game state through Harmony patches and reflection. Game data is always more reliable than pixel reading.
**Instead:** Query game objects directly through C# APIs.

---

#### AF-3. Item Count Announcements

**Do not** announce "item 3 of 10" or "button 2 of 5" as part of standard navigation.
**Why anti-feature:** Experienced screen reader users navigate by content, not position. Counts add verbal overhead to every navigation step and slow experienced users. The project's design principles explicitly exclude this.
**Instead:** Announce the item name/state. List position available on demand if needed.

---

#### AF-4. Navigation Hints on Every Action

**Do not** announce "Press Enter to select" or "Use arrow keys to navigate" after every item.
**Why anti-feature:** Experienced screen reader users know standard patterns. Hints at 300+ WPM speech are a constant interruption. The project's design principles explicitly exclude this.
**Instead:** Provide hints only for non-standard controls, and only after a brief delay (2-3 seconds of inactivity).

---

#### AF-5. Type Suffixes When Obvious

**Do not** announce "Coal Generator button" or "Meep duplicant."
**Why anti-feature:** Context makes the type clear. In the build menu, everything is a building. In the duplicant list, everything is a duplicant. The suffix is noise.
**Instead:** "Coal Generator" in the build menu. "Meep" in the duplicant list.

---

#### AF-6. Complete Atmospheric Simulation in Speech

**Do not** try to speak the atmospheric state of every tile the cursor crosses. "Oxygen 1.8 kg 32 degrees. Oxygen 1.7 kg 31 degrees. Oxygen 1.9 kg 33 degrees."
**Why anti-feature:** Speech is serial. Continuous per-tile atmospheric readout is overwhelming and unusable. Sonification handles continuous environmental data; speech handles discrete queries.
**Instead:** Atmospheric data available on demand (press a key to query). Sonification layer for continuous awareness (D-1). Or overlay mode that adds atmospheric info to cursor readout only when explicitly toggled.

---

#### AF-7. Full Pixel-Perfect Spatial Layout

**Do not** attempt to preserve the exact spatial relationships between UI elements (e.g., "this panel is 3 pixels to the right of that one").
**Why anti-feature:** Spatial layout is a visual concept. The accessible experience should be a logical navigation structure (list, tree, grid) not a pixel map.
**Instead:** Navigate by logical structure: categories, items, tabs, fields.

---

#### AF-8. Cross-Platform Support

**Do not** attempt to support macOS or Linux in the initial release.
**Why anti-feature:** Tolk is Windows-only. ONI's blind player base is on Windows (screen reader market share: NVDA and JAWS are Windows-only). Cross-platform would require an entirely different speech bridge and testing infrastructure.
**Instead:** Windows-only with NVDA/JAWS/SAPI. Revisit cross-platform only if demand materializes.

---

#### AF-9. Multiplayer Support

**Do not** build multiplayer accessibility.
**Why anti-feature:** ONI is single-player only. No multiplayer exists to support.

---

#### AF-10. Game Simplification

**Do not** simplify game mechanics, remove complexity, or auto-manage systems for blind players.
**Why anti-feature:** The goal is accessibility, not a different game. Blind players want the same challenge and depth. FactorioAccess preserves Factorio's full complexity; ONI accessibility should do the same.
**Instead:** Provide full access to all mechanics. If a mechanic is hard to use accessibly, design a better interface for it, do not remove it.

---

## ONI-Specific Features with No Precedent

These features are required by ONI's unique design and have no equivalent in any existing accessibility mod. They represent the primary engineering and design challenges.

| Feature | Why Unprecedented | Complexity | Dependencies |
|---------|-------------------|------------|--------------|
| **Overlay speech system** (TS-10) | ONI has 15 data overlays on every tile. No other accessible game has overlapping data layers at this density. | Very High | TS-1, TS-3 |
| **Autonomous agent management** (TS-6, TS-7) | Duplicants are AI agents with needs, skills, and priorities. Factorio has no equivalent. Managing 3-30+ autonomous workers through audio is uncharted. | High | TS-1 |
| **Atmospheric sonification** (D-1) | Three-tone gas mapping with pressure-as-volume is a novel audio design. No precedent in any game mod. | Very High | TS-3, TS-10 |
| **Multi-network conduit stacking** (D-11) | 5 independent networks overlapping on single tiles. Factorio has networks but they do not stack this densely. | High | TS-3, TS-10 |
| **Room recognition** (D-2) | ONI's room bonus system is unique. No other accessible game has spatial room detection with requirements reporting. | Medium | TS-3, TS-10 |
| **Vertical world navigation** | ONI is side-view with gravity. FactorioAccess is top-down. Vertical cursor movement, gas layering (CO2 sinks, hydrogen rises), and vertical building relationships are new problems. | Medium | TS-3 |
| **Printing Pod selection** (D-12) | Comparing candidate duplicants with traits/attributes is a unique ONI mechanic. | Medium | TS-1, TS-6 |

---

## Dependency Map

```
TS-1 Speech Pipeline (foundation)
 |
 +-- TS-2 Menu Navigation
 |    +-- TS-12 Save/Load
 |    +-- D-10 Help System
 |
 +-- TS-3 Grid Cursor
 |    +-- TS-4 Building Construction
 |    |    +-- D-7 Automation
 |    |
 |    +-- TS-5 Entity Inspection
 |    |    +-- D-7 Automation
 |    |    +-- D-8 Critter Management
 |    |
 |    +-- TS-10 Overlay Data
 |    |    +-- D-1 Atmospheric Sonification
 |    |    +-- D-2 Room Recognition
 |    |    +-- D-11 Multi-Network Conduit
 |    |
 |    +-- TS-11 Scanner/Entity Finder
 |    +-- D-5 Fast Travel/Bookmarks
 |
 +-- TS-6 Duplicant Management
 |    +-- TS-7 Priority System
 |    +-- D-3 Schedule Editor
 |    +-- D-4 Colony Management Screens
 |    +-- D-6 Skills/Attributes
 |    +-- D-12 Printing Pod Selection
 |
 +-- TS-8 Research Tree
 +-- TS-9 Notifications/Alerts
 +-- D-9 Vanilla Mode Toggle (cross-cutting)
 +-- D-14 Speech Capture Testing (cross-cutting)
```

---

## Complexity Summary

| Complexity | Table Stakes | Differentiators |
|------------|-------------|-----------------|
| Low | TS-1 (speech), TS-12 (save/load) | D-5 (bookmarks), D-9 (vanilla toggle), D-14 (testing) |
| Medium | TS-2 (menus), TS-3 (cursor), TS-8 (research), TS-9 (alerts) | D-2 (rooms), D-3 (schedules), D-6 (skills), D-10 (help), D-12 (printing pod) |
| High | TS-4 (building), TS-6 (duplicants), TS-7 (priorities), TS-11 (scanner) | D-4 (colony screens), D-7 (automation), D-8 (critters), D-11 (conduits) |
| Very High | TS-5 (entity inspection -- 100+ side screens), TS-10 (overlay system -- 15 layers) | D-1 (sonification), D-13 (DLC rocketry) |

---

## Confidence and Limitations

**High confidence:**
- FactorioAccess feature analysis is based on the project's own 36-feature catalog (docs/fa-features.md) and architecture analysis (docs/fa-architecture-lessons.md, docs/oni-accessibility-lessons.md).
- ONI UI requirements are based on the project's comprehensive accessibility audit (docs/oni_accessibility_audit.md) and full analysis (docs/oni-accessibility-analysis.md).
- Table stakes vs. differentiator categorization is grounded in community patterns and the "what makes a mod usable vs. what makes it good" distinction.

**Medium confidence:**
- Blind gaming community expectations are synthesized from documented patterns (FactorioAccess community, AudioGames.net conventions, accessible game design principles). Direct user interviews were not conducted.
- Civilization and other strategy game accessibility efforts are summarized from known community work. There may be smaller efforts not captured here.

**Low confidence / gaps:**
- Stardew Access mod features are referenced at high level; detailed feature comparison not performed.
- Complexity estimates for ONI-specific features (especially atmospheric sonification and overlay system) are architectural guesses. Actual complexity will depend on ONI's internal APIs and Unity's audio capabilities, which require implementation-phase discovery.
- DLC (Spaced Out, Bionic Booster Pack) features are estimated at very high complexity but actual scope depends on how much DLC-specific UI diverges from the base game.

---

*Research completed 2026-02-11. This document feeds into requirements definition.*
