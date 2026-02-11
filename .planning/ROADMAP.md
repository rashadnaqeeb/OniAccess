# Roadmap: Oni-Access

## Overview

Oni-Access makes Oxygen Not Included playable by blind users through screen reader speech output. The roadmap follows an "observe before act" principle: first establish speech and mod loading, then fix the input architecture to properly intercept and route keys, then enable reading game state (menus, world, entity details, overlays, colony data), then manage duplicants (workforce control before construction), then interact with the existing world via area tools, and only then build new structures. DLC content comes last after the base game is fully accessible.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Foundation** - Mod loads, speaks, and establishes architectural patterns for all subsequent work *(completed 2026-02-11)*
- [x] **Phase 2: Input Architecture** - Replace flat hotkey registry with proper input handler system that intercepts keys before the game sees them *(completed 2026-02-11)*
- [ ] **Phase 3: Menu Navigation** - Player can navigate all menus to start and manage games
- [ ] **Phase 4: World Navigation** - Player can explore the colony world tile-by-tile and find entities
- [ ] **Phase 5: Entity Inspection** - Player can inspect any entity in the world for detailed status and settings
- [ ] **Phase 6: Overlays & Environment** - Player can query environmental data layers and room information at cursor
- [ ] **Phase 7: Colony Management & Notifications** - Player can read colony-wide screens and stay aware of alerts
- [ ] **Phase 8: Duplicant Management** - Player can manage duplicant priorities, schedules, skills, and assignments
- [ ] **Phase 9: Area Tools** - Player can use area-selection tools to modify the existing world
- [ ] **Phase 10: Building & Construction** - Player can place, configure, and remove buildings and infrastructure
- [ ] **Phase 11: Ranching & Automation** - Player can manage critters and configure automation networks
- [ ] **Phase 12: DLC Content** - Player can access Spaced Out rocketry, starmap, multi-asteroid, and Bionic Booster Pack

## Phase Details

### Phase 1: Foundation
**Goal**: The mod loads reliably, speaks through the user's screen reader, and establishes the architectural patterns (text sourcing, announcement format, testing, hotkey discipline) that every subsequent phase depends on
**Depends on**: Nothing (first phase)
**Requirements**: FOUND-01, FOUND-02, FOUND-03, FOUND-04, META-02, META-03, META-04, META-05
**Success Criteria** (what must be TRUE):
  1. Game launches with mod loaded and user hears "Oni-Access loaded" through their screen reader (NVDA, JAWS, or SAPI)
  2. User can toggle the mod off with a hotkey and game returns to normal behavior with no errors
  3. Speech output contains no raw rich text tags or sprite codes -- all output is clean readable text
  4. A speech capture test can verify that specific game actions produce expected speech output
  5. All speech strings that have a game-data equivalent use STRINGS/LocText, not hardcoded English
**Plans**: 3 plans

Plans:
- [x] 01-01-PLAN.md -- Project scaffolding, mod entry point, and Tolk initialization
- [x] 01-02-PLAN.md -- Speech pipeline with text filtering, announcement infrastructure, and alert history buffer
- [x] 01-03-PLAN.md -- Vanilla mode toggle, hotkey system, input interception, and speech capture testing

### Phase 2: Input Architecture
**Goal**: Replace the flat HotkeyRegistry and InputInterceptor MonoBehaviour with a proper input handler system -- either a state machine or an IInputHandler stack -- that intercepts keys before ONI processes them, routes input based on game state (world map vs menu vs build mode), and lets each game context own its own key handling
**Depends on**: Phase 1
**Requirements**: INPUT-01 (input handler architecture), INPUT-02 (game key interception), INPUT-03 (state-aware routing), INPUT-04 (VanillaMode disables everything), INPUT-05 (context-aware help)
**Success Criteria** (what must be TRUE):
  1. Input handlers can register/deregister themselves based on game state (e.g., a menu handler activates when a menu opens, deactivates when it closes)
  2. Keys claimed by a handler do NOT reach ONI's own input system -- the game never sees them
  3. Unclaimed keys pass through to the game normally
  4. The same physical key can do different things in different states (e.g., arrows navigate a menu list vs. move the world cursor) without any central enum or registry knowing about all states
  5. VanillaMode toggle (Ctrl+Shift+F12) disables the ENTIRE mod -- all input handlers deactivate, all keys pass through to the game, speech stops. Only the toggle hotkey itself remains active. It must not just mute speech while still intercepting keys.
  6. F12 help queries the currently active handler for its key list -- it does NOT dump a global registry. Each handler owns its own help text. If no handler is active, F12 reports only the global keys (toggle).
  7. Game context detection mechanism is explicitly designed -- how the system detects state transitions (menu open/close, build mode enter/exit, tool change) and activates/deactivates handlers accordingly. This is the spine every later phase plugs into.
**Plans**: 2 plans

Plans:
- [ ] 02-01-PLAN.md -- Core input infrastructure: IAccessHandler interface, ModInputRouter (IInputHandler at priority 50), HandlerStack, KeyPoller for unbound keys, ContextDetector, Harmony patches for InputInit.Awake and KScreen lifecycle
- [ ] 02-02-PLAN.md -- Handlers and migration: WorldHandler, HelpHandler (navigable F12 list), VanillaMode redesign (full handler stack disable/enable), Mod.cs migration, delete old Phase 1 input files

### Phase 3: Menu Navigation
**Goal**: A blind player can start a new colony from scratch -- navigating main menu, configuring game settings, selecting an asteroid, customizing the world, picking starting duplicants, and managing saves -- entirely through keyboard and speech
**Depends on**: Phase 2
**Requirements**: MENU-01, MENU-02, MENU-03, MENU-04, MENU-05, MENU-06, MENU-07, MENU-08, MENU-09, META-01
**Success Criteria** (what must be TRUE):
  1. User can launch game, hear main menu options, and select any menu item with keyboard
  2. User can configure a new game (select game mode, pick asteroid with traits read aloud, customize world settings) and start the colony
  3. User can select starting duplicants by hearing each candidate's attributes, traits, and interests, then reroll or confirm
  4. User can save the game, browse existing saves and auto-saves, and load any save
  5. User can access context-sensitive help from any menu to hear available controls
**Plans**: TBD

**System note**: Plan 03-01 builds a generic KScreen/widget accessibility layer that reads buttons, lists, toggles, sliders, labels from ANY KScreen and provides keyboard navigation within screens. This system is reused by Phases 7, 8, and 12 for their management screens -- they configure it, not rebuild it.

Plans:
- [ ] 03-01: KScreen accessibility layer -- generic widget reader and keyboard navigation for any KScreen subclass
- [ ] 03-02: Apply to main menu, pause menu, and options screens
- [ ] 03-03: New game flow -- game mode, asteroid selection, world customization
- [ ] 03-04: Duplicant selection at colony start and Printing Pod
- [ ] 03-05: Save/load system and colony summary screen

### Phase 4: World Navigation
**Goal**: A blind player can explore their colony world -- moving a cursor tile-by-tile, hearing what occupies each tile, finding specific entities by searching, bookmarking locations, and tracking duplicants -- all without needing to see the screen
**Depends on**: Phase 3
**Requirements**: NAV-01, NAV-02, NAV-03, NAV-04, NAV-05, NAV-06, NAV-07
**Success Criteria** (what must be TRUE):
  1. User can move cursor with arrow keys and hear tile contents (element, temperature, building, key state) on every step
  2. User can search for entities by category (buildings, duplicants, critters, geysers) and jump cursor directly to any result
  3. User can save named bookmarks at cursor position and teleport back to them later
  4. User can follow a duplicant so the cursor tracks their movement automatically
  5. Cursor announces world boundaries when reaching edges and reports coordinates on demand
  6. Tile readout is a composable hub -- base readout (element, temperature, building) is built-in, but the system accepts pluggable data providers that later phases (Phase 6 overlays, etc.) register into. Moving the cursor queries all active providers and assembles the speech output.
**Plans**: TBD

**System note**: Plan 04-01 designs the tile readout as a pluggable composition system, not a hardcoded string builder. Phase 6 overlay extractors register as readout providers -- they don't retrofit the cursor. The readout hub is the central point all world-state speech flows through.

Plans:
- [ ] 04-01: Grid cursor movement, composable tile readout hub (pluggable data providers), and coordinate queries
- [ ] 04-02: Entity scanner -- category browsing, search, and jump-to
- [ ] 04-03: Bookmarks, duplicant follow mode, and boundary handling

### Phase 5: Entity Inspection
**Goal**: A blind player can inspect any entity in the world -- buildings with their status and settings side screens, critters with tame/wild status, plants with growth conditions, geysers with eruption cycles, and raw tile details -- to understand what is happening and why
**Depends on**: Phases 3, 4
**Requirements**: INSP-01, INSP-02, INSP-03, INSP-04, INSP-05, INSP-06, INSP-07
**Success Criteria** (what must be TRUE):
  1. User can inspect any building and hear its status, contents, and active errands
  2. User can navigate building side screens to modify settings (filters, thresholds, assignments)
  3. User can inspect a duplicant and hear full detail panel (status, bio, needs, health, errands, equipment)
  4. User can inspect critters (species, tame/wild, happiness, egg progress) and plants (growth stage, conditions, yield)
  5. User can inspect geysers/vents (output type, eruption cycle, dormancy) and raw tile cells (element, mass, temperature, germs)
**Plans**: TBD

**System note**: DetailsScreen is a KScreen subclass. Plan 05-01 builds on Phase 3's KScreen/widget reader for navigating tabs, status items, and side screen controls -- it does NOT rebuild screen-reading infrastructure. It adds entity-specific data extraction on top. Plan 05-02 builds a pluggable side screen adapter framework for the 100+ building side screen variants. Reused by Phases 11 and 12.

Plans:
- [ ] 05-01: DetailsScreen reader -- generic framework for reading entity detail panels, status items, and tab navigation
- [ ] 05-02: Side screen adapter system -- pluggable adapters for building side screen variants (filters, thresholds, assignments, etc.)
- [ ] 05-03: Core entity readouts -- buildings (status, contents, errands), duplicants (bio, needs, health, equipment)
- [ ] 05-04: Secondary entity readouts -- critters, plants, geysers, and raw tile cells

### Phase 6: Overlays & Environment
**Goal**: A blind player can query the environmental data layers that are critical to colony survival -- oxygen levels, power networks, temperatures, pipe contents, room status -- by toggling speech overlays that enrich the cursor tile readout
**Depends on**: Phase 4
**Requirements**: ENV-01, ENV-02, ENV-03, ENV-04, ENV-05, ENV-06, ENV-07, ENV-08, ENV-09, ENV-10
**Success Criteria** (what must be TRUE):
  1. User can toggle speech overlays on/off and the cursor readout includes active overlay data alongside base tile info
  2. User can query oxygen overlay (gas type, mass, pressure), temperature overlay (exact temperature), and power overlay (wattage, circuit status) at cursor position
  3. User can query liquid and gas plumbing overlays (pipe contents, flow direction, connections) at cursor position
  4. User can access additional overlays (materials, light, decor, germs, farming, exosuit, automation, conveyor, radiation) at cursor
  5. When cursor enters a recognized room, user hears room type and status, and can query room requirements and missing items on demand
**Plans**: TBD

**System note**: Plan 06-01 builds a generic overlay data reader framework that hooks into OverlayModes.Mode, extracts data at cursor position for any active overlay, and integrates with the cursor readout from Phase 4. Per-overlay plans register data extractors into this framework -- each overlay is a pluggable extractor, not a hardcoded branch.

Plans:
- [ ] 06-01: Overlay data reader framework -- hooks into OverlayModes, pluggable per-overlay extractors, cursor readout integration
- [ ] 06-02: Core overlay extractors -- oxygen, temperature, power
- [ ] 06-03: Plumbing overlay extractors -- liquid pipes, gas pipes, conduit networks
- [ ] 06-04: Secondary overlay extractors -- materials, light, decor, germs, farming, exosuit, conveyor, radiation
- [ ] 06-05: Room recognition -- type, requirements, and missing items

### Phase 7: Colony Management & Notifications
**Goal**: A blind player can monitor colony health through management screens (vitals, consumables, reports) and the research system, and stays informed of problems through a speech-based notification system that surfaces alerts without overwhelming
**Depends on**: Phases 3, 4
**Requirements**: COL-01, COL-02, COL-03, COL-04, COL-05, COL-06, COL-07
**Success Criteria** (what must be TRUE):
  1. User can open the Vitals screen and hear each duplicant's health, stress, and key status indicators
  2. User can navigate Consumables screen to manage food permissions per duplicant
  3. User can browse Reports screen to hear colony statistics across cycles
  4. User can browse available research as sorted lists, queue research, and hear prerequisites and costs
  5. User can cycle through active notifications with severity announced, and repeated alerts for the same issue are deduplicated
**Plans**: TBD

**System note**: Vitals, Consumables, and Reports are KScreens -- they use the generic KScreen reader from Phase 3, not new screen-reading infrastructure. Research tree may need custom navigation since it's a tree, not a list. Notification system hooks ONI's NotificationManager and designs alert-specific speech (queuing, dedup, history) based on the actual game API -- no premature infrastructure exists.

Plans:
- [ ] 07-01: Management screen configurations -- Vitals, Consumables, Reports using Phase 3 KScreen reader
- [ ] 07-02: Research tree navigation and queue management
- [ ] 07-03: Notification system -- hook ONI's NotificationManager, design alert speech with dedup and history

### Phase 8: Duplicant Management
**Goal**: A blind player can manage their duplicants as individuals -- querying status, adjusting the priority grid, editing schedules, assigning skills, and selecting new duplicants from the Printing Pod -- with enough information to make strategic workforce decisions
**Depends on**: Phases 3, 4, 7
**Requirements**: DUPE-01, DUPE-02, DUPE-03, DUPE-04, DUPE-05, DUPE-06
**Success Criteria** (what must be TRUE):
  1. User can query any duplicant's current activity, location, stress, and health status
  2. User can navigate the priority grid (duplicant x chore type) and set values 1-9 for any cell
  3. User can navigate and edit the 24-slot schedule grid per duplicant or group
  4. User can browse the skill tree, hear prerequisites and morale costs, and assign skills
  5. User can compare and select duplicant candidates at the Printing Pod every 3 cycles, hearing attributes, traits, and interests
**Plans**: TBD

**System note**: Priority grid and schedule editor are specialized 2D grid KScreens -- they use the Phase 3 KScreen reader for basic widget access but need custom grid navigation on top. Skill tree needs tree-walking navigation. Duplicant status queries use the Phase 5 DetailsScreen reader.

Plans:
- [ ] 08-01: 2D grid navigation system for priority and schedule screens (built on Phase 3 KScreen reader)
- [ ] 08-02: Priority grid and schedule editor configurations
- [ ] 08-03: Skill tree navigation and Printing Pod candidate comparison

### Phase 9: Area Tools
**Goal**: A blind player can use all area-selection tools to interact with and modify the existing world -- digging, mopping, sweeping, harvesting, capturing critters, attacking, disinfecting, emptying pipes, and other drag-over-area commands -- with clear feedback about what is being targeted and ordered
**Depends on**: Phases 4, 8
**Requirements**: AREA-01, AREA-02, AREA-03, AREA-04, AREA-05, AREA-06, AREA-07, AREA-08, BUILD-08, BUILD-10
**Success Criteria** (what must be TRUE):
  1. User can select any area tool (dig, mop, sweep, harvest, capture, attack, disinfect, empty pipe, toggle, relocate) and hear which tool is active
  2. User can drag-select a rectangular area at cursor and hear a summary of affected tiles/entities before confirming
  3. User can cancel pending orders for any tool type in a selected area
  4. User can set priority on area tool orders and hear the current priority level
  5. User can use the prioritize tool to change priority of existing errands in an area
**Plans**: TBD

**System note**: All area tools inherit DragTool, which inherits InterfaceTool. Plan 09-01 builds the framework at the InterfaceTool level (tool activation, deactivation, feedback patterns) and adds DragTool-specific features (area selection, tile summary, confirmation, priority) on top. Phase 10's BuildTool also inherits InterfaceTool -- it reuses the InterfaceTool-level patterns from this framework and only adds placement-specific behavior. Designing at InterfaceTool level prevents Phase 10 from rebuilding tool lifecycle management.

Plans:
- [ ] 09-01: InterfaceTool/DragTool accessibility framework -- tool lifecycle at InterfaceTool level, cursor-based area selection and tile summary at DragTool level, confirmation, and priority
- [ ] 09-02: Per-tool configurations -- dig, mop, sweep, harvest, capture, attack, disinfect, empty pipe, toggle, relocate, cancel, and prioritize

### Phase 10: Building & Construction
**Goal**: A blind player can construct their colony -- navigating build categories, placing buildings with rotation, routing pipes and wires, and deconstructing -- with clear feedback about placement validity and material costs
**Depends on**: Phases 4, 6, 7, 9
**Requirements**: BUILD-01, BUILD-02, BUILD-03, BUILD-04, BUILD-05, BUILD-06, BUILD-07, BUILD-09
**Success Criteria** (what must be TRUE):
  1. User can browse 15 build categories and hear building details (name, description, material cost, power usage) for each option
  2. User can place a building at cursor with rotation/orientation and hear whether placement is valid, blocked, or missing materials
  3. User can route pipes and wires tile-by-tile with connection feedback, and drag-to-build for tiles, pipes, and wires
  4. User can deconstruct buildings and set build priority during placement
**Plans**: TBD

Plans:
- [ ] 10-01: Build menu navigation and building detail announcements
- [ ] 10-02: Building placement with rotation, validity feedback, and priority
- [ ] 10-03: Pipe and wire routing with connection feedback and drag-to-build
- [ ] 10-04: Deconstruction and build order management

### Phase 11: Ranching & Automation
**Goal**: A blind player can manage ranching operations (critter tracking, egg production, stable assignments) and configure automation networks (wires, sensors, logic gates, signal state) for advanced colony optimization
**Depends on**: Phases 5, 10
**Requirements**: RANCH-01, RANCH-02, RANCH-03, AUTO-01, AUTO-02, AUTO-03, AUTO-04
**Success Criteria** (what must be TRUE):
  1. User can find and inspect critters with full status details including species, tame/wild, happiness, and egg progress
  2. User can track egg production counts and manage ranching stable assignments
  3. User can connect automation wires between buildings and read automation signal state at cursor
  4. User can configure sensor thresholds (temperature, atmo, timer) and logic gates through their side screens
**Plans**: TBD

Plans:
- [ ] 11-01: Critter management -- finding, status tracking, and egg/population monitoring
- [ ] 11-02: Stable assignments and ranching workflow
- [ ] 11-03: Automation wiring and signal state readout
- [ ] 11-04: Sensor and logic gate configuration

### Phase 12: DLC Content
**Goal**: A blind player can access all Spaced Out and Bionic Booster Pack content -- building rockets, navigating the starmap, managing multiple asteroid colonies, and using bionic systems -- extending full accessibility to every DLC feature
**Depends on**: Phases 3, 5, 8, 10
**Requirements**: DLC-01, DLC-02, DLC-03, DLC-04
**Success Criteria** (what must be TRUE):
  1. User can build and configure rockets using the rocket construction interface with all components announced
  2. User can navigate the starmap, browse destinations, and assign rocket missions
  3. User can manage multiple asteroid colonies and switch between them with full awareness of each colony's state
  4. User can access Bionic Booster Pack buildings, bionic duplicants, and bionic-specific systems
**Plans**: TBD

Plans:
- [ ] 12-01: Rocket construction and configuration
- [ ] 12-02: Starmap navigation and mission management
- [ ] 12-03: Multi-asteroid colony management and switching
- [ ] 12-04: Bionic Booster Pack content integration

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 9 -> 10 -> 11 -> 12

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 3/3 | ✓ Complete | 2026-02-11 |
| 2. Input Architecture | 2/2 | ✓ Complete | 2026-02-11 |
| 3. Menu Navigation | 0/5 | Not started | - |
| 4. World Navigation | 0/3 | Not started | - |
| 5. Entity Inspection | 0/4 | Not started | - |
| 6. Overlays & Environment | 0/5 | Not started | - |
| 7. Colony Management & Notifications | 0/3 | Not started | - |
| 8. Duplicant Management | 0/3 | Not started | - |
| 9. Area Tools | 0/2 | Not started | - |
| 10. Building & Construction | 0/4 | Not started | - |
| 11. Ranching & Automation | 0/4 | Not started | - |
| 12. DLC Content | 0/4 | Not started | - |
