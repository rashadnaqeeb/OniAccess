# Requirements: Oni-Access

**Defined:** 2026-02-11
**Core Value:** A blind player can play a full colony with an experience designed for audio, not a translation of the visual interface.

## v1 Requirements

### Foundation

- [ ] **FOUND-01**: Mod loads via KMod.UserMod2, initializes Tolk, and speaks confirmation on game launch
- [ ] **FOUND-02**: Speech output works with NVDA, JAWS, and SAPI fallback
- [ ] **FOUND-03**: Rich text tags and sprite tags are stripped/converted before speech
- [ ] **FOUND-04**: Vanilla mode toggle disables all accessibility speech and input without breaking game state

### Menu Navigation

- [ ] **MENU-01**: User can navigate main menu (all buttons announced, keyboard operable)
- [ ] **MENU-02**: User can navigate options screens (graphics, audio, game, controls, accounts)
- [ ] **MENU-03**: User can select game mode (Survival, No Sweat, etc.)
- [ ] **MENU-04**: User can browse and select asteroid/cluster with traits and details announced
- [ ] **MENU-05**: User can customize world settings (disease, hunger, morale, stress, etc.)
- [ ] **MENU-06**: User can select starting duplicants (reroll, compare attributes/traits/interests)
- [ ] **MENU-07**: User can save and load games including auto-saves
- [ ] **MENU-08**: User can navigate pause menu
- [ ] **MENU-09**: User can navigate colony summary screen

### World Navigation

- [ ] **NAV-01**: User can move cursor tile-by-tile with arrow keys (horizontal and vertical)
- [ ] **NAV-02**: Cursor announces tile contents: element, temperature, building, and key state
- [ ] **NAV-03**: User can query cursor coordinates on demand
- [ ] **NAV-04**: User can scan for entities by category (buildings, duplicants, critters, geysers) and jump to them
- [ ] **NAV-05**: User can save named bookmarks and teleport cursor to them
- [ ] **NAV-06**: User can follow a duplicant (cursor tracks their movement)
- [ ] **NAV-07**: Cursor respects world boundaries and announces edges

### Building & Construction

- [ ] **BUILD-01**: User can navigate 15 build categories and browse buildings within each
- [ ] **BUILD-02**: Building details announced: name, description, material cost, power usage
- [ ] **BUILD-03**: User can place buildings at cursor with rotation/orientation
- [ ] **BUILD-04**: Placement validity announced (valid, blocked, missing materials, overlap)
- [ ] **BUILD-05**: User can route pipes and wires tile-by-tile with connection feedback
- [ ] **BUILD-06**: User can drag-to-build for tiles, pipes, and wires
- [ ] **BUILD-07**: User can deconstruct buildings
- [ ] **BUILD-08**: User can cancel pending build/dig orders
- [ ] **BUILD-09**: User can set build priority on placement
- [ ] **BUILD-10**: User can use dig, mop, sweep, and other tool commands

### Entity Inspection

- [ ] **INSP-01**: User can inspect any building and hear status, contents, and active errands
- [ ] **INSP-02**: User can navigate building side screens to modify settings (filters, thresholds, assignments)
- [ ] **INSP-03**: User can inspect duplicant detail panels (status, bio, needs, health, errands, equipment)
- [ ] **INSP-04**: User can inspect critters (species, tame/wild, happiness, egg progress)
- [ ] **INSP-05**: User can inspect plants (growth stage, conditions, yield)
- [ ] **INSP-06**: User can inspect geysers/vents (output type, eruption cycle, dormancy)
- [ ] **INSP-07**: User can inspect tile/cell details (element, mass, temperature, germs)

### Duplicant Management

- [ ] **DUPE-01**: User can query any duplicant's current activity, location, stress, and health
- [ ] **DUPE-02**: User can navigate and modify the priority grid (duplicant x chore type, values 1-9)
- [ ] **DUPE-03**: User can navigate and edit 24-slot schedule grid per duplicant/group
- [ ] **DUPE-04**: User can browse and assign duplicant skills with prerequisites and morale costs announced
- [ ] **DUPE-05**: User can compare and select duplicant candidates at the Printing Pod (every 3 cycles)
- [ ] **DUPE-06**: User can view duplicant attributes, traits, and interests

### Colony Management

- [ ] **COL-01**: User can navigate Vitals screen (all duplicant health/stress overview)
- [ ] **COL-02**: User can navigate Consumables screen (food permissions per duplicant)
- [ ] **COL-03**: User can navigate Reports screen (colony statistics over time)
- [ ] **COL-04**: User can browse research tree as sorted lists (available, locked, completed)
- [ ] **COL-05**: User can queue and track research with prerequisites and costs announced
- [ ] **COL-06**: User can cycle through active notifications/alerts with severity
- [ ] **COL-07**: Alert deduplication prevents repeated announcement of same issue

### Overlays & Environment

- [ ] **ENV-01**: User can toggle speech overlays that add layer data to tile readout
- [ ] **ENV-02**: Oxygen overlay: gas type, mass, pressure at cursor
- [ ] **ENV-03**: Power overlay: wattage, circuit status, wire type at cursor
- [ ] **ENV-04**: Temperature overlay: exact temperature at cursor
- [ ] **ENV-05**: Liquid plumbing overlay: pipe contents, flow, connections at cursor
- [ ] **ENV-06**: Gas plumbing overlay: pipe contents, flow, connections at cursor
- [ ] **ENV-07**: Additional overlays accessible: materials, light, decor, germs, farming, exosuit, automation, conveyor, radiation
- [ ] **ENV-08**: Room type and status announced when cursor enters a recognized room
- [ ] **ENV-09**: Room requirements and missing items available on demand
- [ ] **ENV-10**: User can query conduit networks at cursor (liquid pipe, gas pipe, power wire, automation wire, conveyor rail)

### Automation

- [ ] **AUTO-01**: User can connect automation wires between buildings
- [ ] **AUTO-02**: User can configure sensor thresholds (temperature, atmo, timer, etc.)
- [ ] **AUTO-03**: User can configure logic gates
- [ ] **AUTO-04**: User can read automation signal state at cursor

### Critter & Ranching

- [ ] **RANCH-01**: User can find and inspect critters with status details
- [ ] **RANCH-02**: User can track egg production and population counts
- [ ] **RANCH-03**: User can manage ranching stable assignments

### DLC Content

- [ ] **DLC-01**: User can build and configure rockets (Spaced Out)
- [ ] **DLC-02**: User can navigate the starmap and select destinations
- [ ] **DLC-03**: User can manage multiple asteroid colonies and switch between them
- [ ] **DLC-04**: User can access Bionic Booster Pack buildings, duplicants, and systems

### Meta

- [ ] **META-01**: Context-sensitive help available from any screen (controls and current context)
- [ ] **META-02**: Speech capture testing framework for automated regression testing
- [ ] **META-03**: All game text sourced from STRINGS namespace and LocText (no hardcoded text unless unavoidable)
- [ ] **META-04**: Announcements follow concise format: name, state, value only
- [ ] **META-05**: Hotkey overwrites documented with original function noted

## v2 Requirements

### Atmospheric Sonification

- **SON-01**: Three-tone gas system (oxygen tone, CO2 rumble, foreign gas alert, vacuum silence)
- **SON-02**: Pressure encoded as volume
- **SON-03**: Temperature texture overlay (warm crackle, cold shimmer)
- **SON-04**: Focused gas tracking mode (isolate one gas type)
- **SON-05**: Sweep pattern for horizontal atmospheric scanning

### Polish & Accessibility Refinement

- **POL-01**: Adjustable verbosity levels
- **POL-02**: Configurable speech rate preferences
- **POL-03**: Custom keybinding remapping for mod controls
- **POL-04**: Accessible tutorial/onboarding experience

## Out of Scope

| Feature | Reason |
|---------|--------|
| Visual accessibility (colorblind, high contrast) | This mod is specifically for screen reader users |
| Cross-platform (macOS, Linux) | Tolk is Windows-only; screen reader market is Windows |
| Multiplayer support | ONI is single-player |
| Game simplification / auto-management | Goal is full access to game complexity, not a simpler game |
| OCR / image recognition | Direct game state access via Harmony is always more reliable |
| Visual screen description | Accessible experience, not visual translation |
| Item count announcements | Design principle: concise, no "3 of 10" |
| Navigation hints on every action | Design principle: experienced SR users know patterns |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| FOUND-01 | — | Pending |
| FOUND-02 | — | Pending |
| FOUND-03 | — | Pending |
| FOUND-04 | — | Pending |
| MENU-01 | — | Pending |
| MENU-02 | — | Pending |
| MENU-03 | — | Pending |
| MENU-04 | — | Pending |
| MENU-05 | — | Pending |
| MENU-06 | — | Pending |
| MENU-07 | — | Pending |
| MENU-08 | — | Pending |
| MENU-09 | — | Pending |
| NAV-01 | — | Pending |
| NAV-02 | — | Pending |
| NAV-03 | — | Pending |
| NAV-04 | — | Pending |
| NAV-05 | — | Pending |
| NAV-06 | — | Pending |
| NAV-07 | — | Pending |
| BUILD-01 | — | Pending |
| BUILD-02 | — | Pending |
| BUILD-03 | — | Pending |
| BUILD-04 | — | Pending |
| BUILD-05 | — | Pending |
| BUILD-06 | — | Pending |
| BUILD-07 | — | Pending |
| BUILD-08 | — | Pending |
| BUILD-09 | — | Pending |
| BUILD-10 | — | Pending |
| INSP-01 | — | Pending |
| INSP-02 | — | Pending |
| INSP-03 | — | Pending |
| INSP-04 | — | Pending |
| INSP-05 | — | Pending |
| INSP-06 | — | Pending |
| INSP-07 | — | Pending |
| DUPE-01 | — | Pending |
| DUPE-02 | — | Pending |
| DUPE-03 | — | Pending |
| DUPE-04 | — | Pending |
| DUPE-05 | — | Pending |
| DUPE-06 | — | Pending |
| COL-01 | — | Pending |
| COL-02 | — | Pending |
| COL-03 | — | Pending |
| COL-04 | — | Pending |
| COL-05 | — | Pending |
| COL-06 | — | Pending |
| COL-07 | — | Pending |
| ENV-01 | — | Pending |
| ENV-02 | — | Pending |
| ENV-03 | — | Pending |
| ENV-04 | — | Pending |
| ENV-05 | — | Pending |
| ENV-06 | — | Pending |
| ENV-07 | — | Pending |
| ENV-08 | — | Pending |
| ENV-09 | — | Pending |
| ENV-10 | — | Pending |
| AUTO-01 | — | Pending |
| AUTO-02 | — | Pending |
| AUTO-03 | — | Pending |
| AUTO-04 | — | Pending |
| RANCH-01 | — | Pending |
| RANCH-02 | — | Pending |
| RANCH-03 | — | Pending |
| DLC-01 | — | Pending |
| DLC-02 | — | Pending |
| DLC-03 | — | Pending |
| DLC-04 | — | Pending |
| META-01 | — | Pending |
| META-02 | — | Pending |
| META-03 | — | Pending |
| META-04 | — | Pending |
| META-05 | — | Pending |

**Coverage:**
- v1 requirements: 72 total
- Mapped to phases: 0
- Unmapped: 72 (pending roadmap creation)

---
*Requirements defined: 2026-02-11*
*Last updated: 2026-02-11 after initial definition*
