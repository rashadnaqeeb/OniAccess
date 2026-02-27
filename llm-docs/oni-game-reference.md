# Oxygen Not Included — Game Reference for LLM/Mod Development

This document provides a reference summary of ONI's controls, space model, mechanics, screens, and modding ecosystem as they relate to building the OniAccess screen reader mod. It synthesizes web research, the codebase, and the existing `docs/oni_accessibility_audit.md`.

For a complete screen-by-screen accessibility audit (every panel, tab, and interactive element), see `docs/oni_accessibility_audit.md`. This file focuses on the structural and architectural facts that help an LLM reason about the game.

---

## 1. Control Scheme

### 1.1 Primary Input Methods

ONI is mouse-first. The player pans the camera, clicks to select entities, clicks-drags to place buildings and draw tools, and right-clicks to deselect/cancel. Most management screens are opened by clicking toolbar buttons; keyboard shortcuts are secondary but comprehensive.

The player never directly controls duplicants. All gameplay is indirect: issue orders (dig, build, assign priorities) and duplicants execute them autonomously based on their stats and priority settings.

### 1.2 Camera Controls

| Action | Key/Input |
|--------|-----------|
| Pan Up/Down/Left/Right | W/S/A/D |
| Zoom In/Out | Mouse scroll wheel |
| Camera home (Printing Pod) | H |
| Save camera position 1-10 | Ctrl+1-0 |
| Recall camera position 1-10 | Shift+1-0 |

### 1.3 World Interaction Tools

| Action | Key |
|--------|-----|
| Dig | G |
| Mop | M |
| Clear (debris) | K |
| Disinfect | I |
| Attack critter | T |
| Capture critter | N |
| Harvest plant | Y |
| Empty pipe | Insert |
| Prioritize (sub-priority brush) | P |
| Cancel orders | C |
| Deconstruct | X |
| Copy building | B |
| Rotate building | O |
| Drag straight | Shift (hold while placing) |

### 1.4 Management Screen Hotkeys

| Screen | Key |
|--------|-----|
| Priorities | L |
| Consumables | F |
| Vitals | V |
| Research | R |
| Daily Reports | E |
| Codex/Database | U |
| Skills | J |
| Schedule | . (period) |
| Starmap | Z |

### 1.5 Build Menu Hotkeys

Build menu categories are opened by number keys 1-0, minus, and equals (categories 1-12 respectively). Two additional categories use Shift+minus and Shift+equals.

### 1.6 Overlay Hotkeys

| Overlay | Key |
|---------|-----|
| Oxygen | F1 |
| Power | F2 |
| Temperature | F3 |
| Materials | F4 |
| Light | F5 |
| Liquid Plumbing | F6 |
| Gas Plumbing | F7 |
| Decor | F8 |
| Germs | F9 |
| Farming | F10 |
| Rooms | F11 |
| Exosuit | Shift+F1 |
| Automation | Shift+F2 |
| Conveyor | Shift+F3 |
| Radiation (Spaced Out DLC) | Shift+F4 |

### 1.7 Game Speed

| Action | Key |
|--------|-----|
| Toggle Pause | Space |
| Cycle speed | Tab |
| Speed Up | Numpad + |
| Slow Down | Numpad - |

### 1.8 Building Controls (when a building is selected)

| Action | Key |
|--------|-----|
| Toggle Open (doors) | / |
| Toggle Enabled | Enter |
| Building Utility 1 | \ |
| Building Utility 2 | [ |
| Building Utility 3 | ] |

### 1.9 Mouse Interaction Summary

- **Left-click on world cell**: opens cell detail panel (element, temperature, germs, hardness)
- **Left-click on building**: opens building detail panel (tabbed: Status, Info, Contents, Errands, Config)
- **Left-click on duplicant**: opens duplicant detail panel (tabbed: Status, Bio, Needs, Health, Errands, Equipment)
- **Left-click on critter/plant**: opens respective detail panel
- **Right-click**: deselects/cancels current tool
- **Click-drag**: area selection for tools (dig, mop, sweep, etc.) and pipe/wire drawing
- **Middle-mouse drag**: alternative camera pan

---

## 2. Space Model: World Structure

### 2.1 The Tile Grid

ONI's world is a 2D side-view tile grid. Every position in the world is a discrete integer cell. A typical asteroid is approximately 256 tiles wide and 385 tiles tall (some are 384x384). Spaced Out DLC asteroids are smaller (~128x256) but there are multiple planetoids.

Each cell is a self-contained simulation unit with the following tracked properties:
- **Element**: the material occupying the cell (Oxygen, Water, Granite, Vacuum, etc.)
- **State**: solid, liquid, or gas
- **Mass**: kilograms of the element
- **Temperature**: degrees Celsius (also displayed in Kelvin or Fahrenheit per player setting)
- **Thermal conductivity**: how fast heat transfers to neighbors
- **Germs**: type (Food Poisoning, Slimelung, Zombie Spores, Radiation, Pollen) and count
- **Radiation level**: rads per cycle (Spaced Out DLC)
- **Decor value**: contribution to nearby duplicant morale
- **Hardness**: for solid tiles, determines dig time (ranges from Very Soft to Impenetrable)

Cells can also contain placed buildings, plants, critters, and debris (loose items on the ground).

### 2.2 World Layers

The world has multiple Z-depth layers that stack on the same grid positions:

1. **Background**: dirt/rock background wall tiles. Provides insulation.
2. **Solid tiles**: the main terrain layer (mineable rock, constructed floors/walls)
3. **Buildings**: placed structures occupy one or more cells
4. **Pipe networks**: liquid pipes and gas pipes occupy cells but are separate from the tile layer
5. **Wire networks**: power wires occupy cells
6. **Automation wires**: logic signal wires
7. **Conveyor rails**: item transport rail network
8. **Duplicants/creatures**: entities that move through traversable cells

Multiple networks can share cells (a cell can have a liquid pipe, gas pipe, automation wire, and power wire all in the same tile). Building placement respects which layers it occupies.

### 2.3 Biomes

Worlds are divided into biomes, each with characteristic materials, temperatures, and critters. Common biomes:
- **Temperate**: starting area, comfortable for duplicants, oxygen and algae present
- **Caustic**: caustic biome with chlorine gas and bleach stone
- **Frozen/Tundra**: extreme cold, ice, wolframite
- **Magma**: extreme heat, igneous rock, magma
- **Swamp**: polluted water, slimelung germs, prickle flowers
- **Barren**: minimal resources
- **Forest**: carbon dioxide, pincha pepper plants, lumber
- **Oil Biome**: crude oil, natural gas, petroleum, deep underground
- **Space**: the outer rim of the asteroid, vacuum, meteor showers
- **Rust**: rust biome (some asteroids), iron ore, rust
- **Ocean**: salt water

### 2.4 World Generation

Worlds are procedurally generated from a seed. The seed determines which biomes appear, where geysers spawn, terrain shape, and natural resource distribution. During new game setup the player sees "World Traits" — modifiers applied to that seed's world such as Metal Rich, Frozen Core, Volatile Geyser, Geoactive, etc.

### 2.5 Overlays

Overlays are visual filters toggled by F-keys (see section 1.6). Each overlay changes the rendering of every cell to highlight a specific property. For screen reader purposes: when an overlay is active, the tile cursor in OniAccess uses an `OverlayProfile` to select what data to read from each cell. Each profile corresponds to one overlay mode.

Overlay profiles defined in OniAccess: None (default/visual), Oxygen, Power, Temperature, Decor, Germs, Light, Radiation, Plumbing (liquid pipes), Ventilation (gas pipes), Conveyor, Automation.

---

## 3. Core Mechanics

### 3.1 Duplicants

Duplicants are the player's workforce — AI-controlled characters who execute orders. The player never directly moves them. They are printed from the Printing Pod at colony start and every 3 cycles thereafter.

**Attributes** (11 total, numeric values that improve with practice):
- Athletics (move speed, carrying capacity)
- Cooking (food quality)
- Creativity (decor quality of art/sculptures)
- Digging (dig speed)
- Machinery (building efficiency, machine operation speed)
- Medicine (medical treatment quality)
- Science (research speed)
- Strength (hauling speed)
- Husbandry (critter grooming and ranching)
- Tinkering (farm station speed)
- Art (sculpture quality)

**Traits**: each duplicant has 0-3 positive traits and 0-3 negative traits. Traits give attribute bonuses/penalties or add behavioral restrictions (e.g., a duplicant with the "Bottomless Stomach" negative trait consumes 50% more calories; "Trypophobia" prevents them from working near certain critters). Some negative traits permanently disable certain ChoreGroups (errand types).

**Interests**: 1-3 interests per duplicant. An interest grants +1 morale for skills in that category and increases XP gain speed in related activities.

**Stress**: a 0-100% meter. High stress accumulates when morale need exceeds morale supply, when in dark/cold/hot conditions, or during emergencies. At 100% stress the duplicant enters a "stress response" (Destructive, Ugly Crier, Binge Eater, or Vomiter) until stress drops below 60%.

**Morale**: the net of morale supply (food quality, decor, room bonuses, recreation, expectations) minus morale need (from assigned skills). High morale reduces stress; low morale increases it.

**Needs**: duplicants have automatic biological needs — they need to sleep (restores stamina), eat (restores calories), and use the bathroom (empties bladder). These are managed via the Schedule system. Personal needs override work orders unless Red Alert is active.

**Skills**: earned by spending Skill Points (awarded passively through activity and attributable via the Skills screen). Each skill unlocks new errand types or improves existing ones. Skills increase the morale need (higher-tier skills increase it more). Examples: Improved Digging, Hard Digging, Super-Hard Digging, Advanced Research, Field Research, Astronomy, Grilling, Artistry, Critter Ranching, Rocket Piloting.

**Schedule blocks**: each duplicant follows a 24-segment daily schedule (one segment = 25 real seconds, one cycle = 600s). Segments are typed: Work (performs assigned tasks), Bathtime (bathroom visit), Downtime (recreation, eating, bathroom), Bedtime (sleep). Players create named schedules and assign duplicants to them.

**Bionic Duplicants** (Bionic Booster Pack DLC): an alternate duplicant type that requires power banks instead of food, has an internal oxygen supply, and has different morale sources. Managed with additional columns in Vitals (Power Banks) and Consumables (Power Banks, Oxygen Canisters).

### 3.2 Resources

Resources exist as:
- **Tiles**: solid materials in the world grid
- **Debris**: loose items on the floor (dropped when mined/deconstructed)
- **Storage**: inside buildings (Refrigerators, Storage Bins, etc.)
- **Pipe contents**: liquids/gases flowing through pipe networks

Key resource categories:
- **Food/Edibles**: calorie-providing items, each with a food quality value (Very Poor to Gourmet). Food quality affects morale.
- **Raw materials**: ore, minerals, metals, refined metals, plastics
- **Gases**: Oxygen, Carbon Dioxide, Natural Gas, Hydrogen, Chlorine, Polluted Oxygen, etc. (each an element with distinct properties)
- **Liquids**: Water, Polluted Water, Salt Water, Crude Oil, Petroleum, Brine, Liquid Oxygen, etc.
- **Biological**: plant seeds, critter eggs, medicine
- **Energy**: power in watts, managed through circuits

### 3.3 Oxygen and Atmosphere

Gases diffuse through open cells and equalize pressure. Oxygen must be produced (algae-based buildings, electrolyzers, sublimation stations) and distributed. Carbon dioxide sinks to the bottom; Hydrogen rises to the top. Polluted Oxygen can carry germs. Vacuum causes suffocation. The game tracks per-cell gas amounts.

Duplicants breathe 100g of Oxygen per second. If Oxygen falls below a threshold, they gain "Suffocating" status and rapidly lose health.

### 3.4 Temperature

Every cell and entity has a temperature. Heat transfers through thermal conduction between adjacent cells and through radiated/convective exchange with contained gases. Buildings generate and absorb heat. Extreme temperatures cause duplicant stress effects ("Too Hot", "Too Cold") and can damage buildings. Temperature management (heating, cooling via air conditioning, thermal insulation) is a major gameplay challenge.

### 3.5 Power

Buildings consume or generate Watts. Buildings are connected via power wires into circuits. Each circuit has a maximum capacity; overload breaks the circuit. Power sources: manual generators (duplicant-operated), coal/hydrogen/natural gas/petroleum generators, steam turbines, solar panels. Storage: batteries. Power management is monitored via the Power Overlay.

### 3.6 Plumbing

Two separate fluid networks:
- **Liquid pipes**: move water, polluted water, crude oil, petroleum, etc.
- **Gas pipes**: move oxygen, carbon dioxide, natural gas, polluted oxygen, etc.

Each network has: pumps (move fluid), vents (release fluid to environment), bridges (cross pipe networks), valves (flow control), filters (split by element type), reservoirs. Pipes have limited throughput (kg/s). Liquid plumbing is also used for toilets, sinks, showers, and irrigation.

### 3.7 Research

Technology unlocks new buildings and capabilities. Research requires producing Research Points by assigning duplicants to Research Stations (early), Super Computers (advanced), and Virtual Planetariums (interstellar). There are three point types: Novice, Advanced, and Interstellar. The research tree has approximately 10 tiers, with hundreds of tech nodes. Research is queued from the Research screen.

### 3.8 Skills

Separate from research. Skills are assigned to individual duplicants from the Skills screen using Skill Points. Unlike research (colony-wide), skills are per-duplicant. Each skill comes with a morale cost increase that must be weighed against the benefit.

### 3.9 Food

Duplicants consume approximately 1000 kcal per cycle. Food is produced by growing plants, ranching critters, or cooking. Food quality (1-6 scale) determines morale modifier: higher quality food provides higher morale bonus. Food spoils without refrigeration.

### 3.10 Disease and Germs

Germs exist on cells, in fluids, and on duplicants. Types: Food Poisoning, Slimelung, Zombie Spores, Pollen, Sunburn, and Radiation Sickness (DLC). Germs spread via fluid contact and air exposure. Duplicants can be infected, causing sickness with HP loss and other penalties. Disease management involves hand washing (sinks), ore scrubbers, disinfectant, and germ sensor-based automation.

### 3.11 Schedules

The Schedule screen (key: `.`) manages duplicant daily time allocation. The 24-block cycle timeline can be painted with four block types:
- **Work**: performs assigned tasks
- **Bathtime**: toilet visit (required ~once per cycle)
- **Downtime**: eating, recreation, optional toilet
- **Bedtime**: sleep (restores Stamina)

Multiple named schedules can exist. Duplicants are assigned to one schedule each. Day/night in the game corresponds to 525 seconds daytime (surface-light hours) and 75 seconds nighttime; the in-game clock represents this 24-block cycle.

### 3.12 Priorities (Errands/Chores)

Duplicants perform Errands. Each errand belongs to a ChoreGroup (errand category). Priorities are set per duplicant per ChoreGroup in the Priorities screen (key: L).

Priority levels: Disabled (0), Very Low (1), Low (2), Standard (3), High (4), Very High (5).

ChoreGroups (user-prioritizable errand categories):
- Life Support, Farming, Ranching, Cooking, Combat, Art, Research, Rocketry, Suit Wearing, Operating, Medicine, Supplying, Tidying, Hauling, Building, Digging, Toggle, Storage

Personal needs (eating, sleeping, bathroom) override priority settings unless Red Alert is active. Red Alert forces all work, ignoring personal needs.

### 3.13 Rooms

Enclosed spaces (walled with tiles and doors) that meet specific criteria are recognized as room types, granting morale and other bonuses. The Room Overlay (F11) shows room recognition. Key room types: Barracks, Bedroom, Latrine, Washroom, Great Hall, Kitchen, Hospital, Recreation Room, Laboratory, Stable, Nature Reserve. Room size requirements vary (typically 12-64 tiles).

### 3.14 Automation

An electrical logic network separate from power wiring. Logic gates (AND, OR, XOR, NOT, Buffer, Filter, Memory Toggle, Signal Counter) and sensors (pressure, temperature, germ, duplicant motion, clock, critter, weight plate) can be wired together to automate building behavior. Green signal = on/active; Red signal = off/inactive. Visible in the Automation Overlay (Shift+F2).

### 3.15 Rocketry and Starmap

Rockets travel to space destinations revealed via telescope. In the base game the Starmap (key: Z) shows destinations at various distances. In Spaced Out DLC the Starmap is a hex-grid map of multiple planetoids; rockets travel between them carrying duplicants and cargo. Spaced Out rockets are modular (command module + engine + fuel + cargo bays + nosecone) and have interior tile grids.

---

## 4. Game Screens Enumeration

This section cross-references with `docs/oni_accessibility_audit.md`, which documents every screen in detail. Below is the structural overview relevant to the mod.

### 4.1 Main Menu Screens

| Screen | Game Class | Mod Handler |
|--------|-----------|-------------|
| Main Menu | `MainMenu` | `MainMenuHandler` |
| Options Menu | `OptionsMenuScreen` | `OptionsMenuHandler` |
| Load Game | `SaveLoadScreen` | `SaveLoadHandler` |
| Save Screen | `SaveScreen` | `SaveScreenHandler` |
| Colony Summaries | `ColonySummaryScreen` | `ColonySummaryHandler` |
| Mod Manager | `ModsScreen` | `ModsHandler` |
| File Name Dialog | `FileNameDialog` | `FileNameDialogHandler` |
| Translation Handler | (language selector) | `TranslationHandler` |

### 4.2 New Game Setup Screens

| Screen | Notes | Mod Handler |
|--------|-------|-------------|
| Colony Setup / Asteroid Select | Asteroid type, seed, world traits, game settings | `ColonySetupHandler` |
| Duplicant Selection | 3 slots, reroll, traits, attributes, interests | `MinionSelectHandler` |
| World Generation | Loading/progress screen | `WorldGenHandler` |

### 4.3 In-Game HUD Elements

These are always visible during gameplay (not full-screen modals):

- **Management tab buttons** (top-left toolbar): Vitals, Consumables, Schedule, Priorities, Reports, Research, Skills, Starmap — each opens a full-screen management screen
- **Overlay buttons** (top-left, beside management tabs): 15 overlays — each toggles a world rendering mode
- **Resource panel** (top-right): collapsed summary or expanded categorized list
- **Time/cycle display** (top-center-right): cycle number, time-of-day segment, speed controls
- **Build menu** (bottom-left): 14-15 category tabs, each with a scrollable building list
- **Tool commands** (bottom-right): Dig, Cancel, Deconstruct, Prioritize, Mop, Sweep, Capture, Harvest, etc.
- **Alert buttons** (bottom-center): Red Alert, Yellow Alert
- **Notification stack** (left side): scrollable transient notification list
- **Entity detail panel** (right side, context-dependent): appears when something is clicked

### 4.4 Full-Screen Management Screens

These slide in over the game world. They are opened by toolbar buttons or keyboard shortcuts.

| Screen | Key | Game Class | Mod Handler | Tabs |
|--------|-----|-----------|-------------|------|
| Vitals | V | `VitalsTableScreen` (extends `TableScreen`) | `VitalsScreenHandler` | Single 2D grid |
| Consumables | F | `ConsumablesTableScreen` (extends `TableScreen`) | `ConsumablesScreenHandler` | Single 2D grid |
| Priorities | L | `JobsTableScreen` (extends `TableScreen`) | `PriorityScreenHandler` | Single 2D grid + toolbar row |
| Schedule | . | `ScheduleScreen` | `ScheduleScreenHandler` | Schedules tab, Duplicants tab |
| Research | R | `ResearchScreen` (extends `KModalScreen`) | `ResearchScreenHandler` | Browse, Queue, Tree |
| Skills | J | `SkillsScreen` (extends `KModalScreen`) | `SkillsScreenHandler` | Duplicants, Skills, Tree |
| Daily Reports | E | `ColonyDiagnosticScreen` | (not yet implemented) | — |
| Starmap | Z | `StarMapScreen` / `ClusterMapScreen` | (not yet implemented) | — |
| Codex/Database | U | `CodexScreen` | (not yet implemented) | — |

**TableScreen subclasses** use `OnShow` lifecycle (patch `OnShow`). **KModalScreen subclasses** default to `Show` lifecycle (patch `Show`; `KModalScreen.OnActivate` calls `OnShow(true)` during prefab init). However, check the decompiled source for each subclass — some (e.g. `SkillsScreen`) only override `OnShow` and must be patched there instead.

### 4.5 Entity Detail Panels

The detail panel opens on the right side when the player left-clicks an entity in the world. In OniAccess this is handled by `DetailsScreenHandler`.

- **Building detail panel** (`DetailsScreen`): tabs include Status, Info, Contents, Errands, Config. Many buildings also open a contextual side screen (fabrication queue, storage filter, sensor settings, etc.).
- **Duplicant detail panel**: same `DetailsScreen` but with duplicant-specific tabs: Status, Bio (attributes/traits), Needs/Morale, Health, Errands, Equipment.
- **Critter/Plant/Geyser panels**: specialized content within the same `DetailsScreen` framework.
- **Cell/tile info**: shown in a simple panel (element, mass, temperature, germs, hardness).

### 4.6 Building Placement Flow

When a building is selected from the build menu:
1. `BuildInfoHandler` reads and speaks the building name, description, size, cost, and power stats.
2. `MaterialPickerHandler` manages material selection for construction.
3. `FacadePickerHandler` manages cosmetic skin selection.
4. The tile cursor (`TileCursor`) shows the placement ghost; `BuildToolSection` speaks cell feedback as the cursor moves.

### 4.7 In-Game Dialogs and Popups

| Dialog | Game Class | Mod Handler |
|--------|-----------|-------------|
| Pause Menu | `PauseMenuScreen` | `PauseMenuHandler` |
| Confirm Dialog | `ConfirmDialogScreen` | `ConfirmDialogHandler` |
| Story/Event Popup | `StoryMessageScreen` | `StoryMessageHandler` |
| Wattson Message | `WattsonMessage` | `WattsonMessageHandler` |
| Key Bindings Screen | `KeyBindingMenuScreen` | `KeyBindingsHandler` |
| Klei Item Drop (care package) | `KleiItemDropScreen` | `KleiItemDropHandler` |
| Video Screen | `VideoScreen` | `VideoScreenHandler` |
| Locker Menu | `LockerMenu` | `LockerMenuHandler` |

### 4.8 Printing Pod (Recurring Event)

Every 3 cycles the Printing Pod offers a choice: 3 duplicants or care packages. This uses `MinionSelectScreen` and is handled by `MinionSelectHandler`. Unlike initial duplicant selection, candidates cannot be rerolled.

---

## 5. Modding Ecosystem

### 5.1 Mod Architecture

ONI uses Harmony (currently Harmony 2.x) for runtime patching. All ONI mods inject into the game by patching game methods at runtime. There is no official modding API; modders decompile game assemblies and patch directly.

Mod entry point: a class inheriting `KMod.UserMod2`, with `OnLoad(Harmony harmony)` override. Harmony patches use `[HarmonyPatch(typeof(TargetClass), "MethodName")]` attributes with `[HarmonyPrefix]`, `[HarmonyPostfix]`, or `[HarmonyTranspiler]` methods.

Key mod infrastructure classes:
- **`KMod.UserMod2`**: base class for all mods; provides `OnLoad` and `OnAllModsLoaded`
- **`KMod.Manager`**: central mod loading manager
- **`KMod.Mod`**: individual mod metadata
- **`KMod.DLLLoader`**: loads mod assemblies

### 5.2 Mod File Structure

A mod lives in a directory containing:
- `mod_info.yaml`: specifies supported content IDs (`VANILLA_ID`, `EXPANSION1_ID`, or `ALL`) and `lastWorkingBuild`
- `mod.yaml`: mod name and description (shown in the Mods screen)
- The compiled `.dll` with the mod code

Local mods path: `C:\Users\[USERNAME]\Documents\Klei\Oxygen Not Included\mods\Local\`
Steam Workshop mods are stored separately and synced via Steam.

### 5.3 DLC Detection

The game ships as one executable with DLC toggled via content flags. Relevant DLC IDs:
- `"EXPANSION1_ID"` — Spaced Out! DLC (multi-planetoid rocketry, radiation, new asteroids)
- `"DLC3_ID"` — Bionic Booster Pack (bionic duplicants, power banks, remote workers)

Runtime check: `Game.IsDlcActiveForCurrentSave("EXPANSION1_ID")` or `DlcManager.FeatureClusterSpaceEnabled()`.

### 5.4 UI Class Hierarchy (relevant to OniAccess)

```
KScreen
├── KModalScreen          (full-screen modal, patch Show())
│   ├── ResearchScreen
│   ├── SkillsScreen
│   └── ... other modals
├── ShowOptimizedKScreen  (toggled by toolbar, patch OnShow())
│   ├── TableScreen
│   │   ├── VitalsTableScreen
│   │   ├── ConsumablesTableScreen
│   │   └── JobsTableScreen (Priorities)
│   └── ScheduleScreen
└── (other KScreen subclasses)
```

**Critical lifecycle distinction**:
- `KModalScreen.OnActivate()` calls `OnShow(true)` directly during prefab initialization. Patching `OnShow` on KModalScreen subclasses would trigger before the game world is ready. **Default to patching `Show()`**, but check the decompiled source — not all subclasses override `Show` (e.g. `SkillsScreen` only overrides `OnShow` and must be patched there).
- `ShowOptimizedKScreen.OnActivate()` does not call `OnShow`. `OnShow` only fires on real ManagementMenu toggles. **Patch `OnShow` for these.**

### 5.5 Steam Workshop

ONI has full Steam Workshop integration. Players subscribe to mods through the Workshop; the game automatically downloads and applies them. The Mods screen (`ModsScreen`) in-game lists all installed mods with enable/disable checkboxes and per-mod options buttons.

### 5.6 OniAccess Mod Conventions

- All patches named: `GameType_MethodName_Patch` (e.g., `ScheduleScreen_OnShow_Patch`)
- All speech routes through `SpeechPipeline.SpeakInterrupt()` or `SpeechPipeline.SpeakQueued()`; never call `SpeechEngine.Say()` directly
- All logging via `Log.Info/Debug/Warn/Error`; never use `Debug.Log`
- New screen handlers must be registered in `ContextDetector.RegisterMenuHandlers()`
- Key detection in `Tick()` via `UnityEngine.Input.GetKeyDown()` (must be fully qualified inside `OniAccess.Input` namespace where `Input` resolves to the namespace, not Unity class)
- `HandleKeyDown()` is only for Escape interception via `KButtonEvent`

---

## 6. Key Data Sources for Speech Output

### 6.1 Game Strings Namespace

All localized text lives in `STRINGS.*` (the `STRINGS` namespace). Always use game strings before adding new ones to `OniAccessStrings.cs`.

Relevant sub-namespaces for management screens:
- `STRINGS.UI.VITALSSCREEN` — Vitals column headers, sickness text
- `STRINGS.UI.TABLESCREENS` — Shared table text (N/A, not available)
- `STRINGS.UI.JOBSSCREEN` — Priority screen labels
- `STRINGS.UI.CONSUMABLESSCREEN` — Consumables screen text
- `STRINGS.RESEARCH` — Research tree text
- `STRINGS.BUILDINGS` — Building names and descriptions
- `STRINGS.DUPLICANTS` — Duplicant traits, modifiers
- `STRINGS.ELEMENTS` — Element names
- `STRINGS.ROOMS` — Room type names and descriptions
- `STRINGS.EQUIPMENT` — Equipment names

### 6.2 Runtime Game Data Objects

Key runtime objects for reading live game state:

- `Db.Get()` — root database accessor; gives access to `Amounts`, `Attributes`, `ChoreGroups`, `ChoreTypes`, `Effects`, `Skills`, `SkillGroups`, `Traits`
- `Db.Get().Amounts.Stress`, `.Calories`, `.HitPoints`, `.Stamina`, `.BionicInternalBattery` — amount definitions
- `Db.Get().Attributes.QualityOfLife`, `.QualityOfLifeExpectation`, `.FoodExpectation` — attribute definitions
- `ClusterManager.Instance.GetWorldIDsSorted()` — all discovered worlds in Spaced Out
- `Components.LiveMinionIdentities.Items` — list of all live duplicants
- `MinionIdentity.GetAmounts()`, `.GetAttributes()`, `.GetComponent<ChoreConsumer>()` — duplicant runtime data
- `Db.Get().ChoreGroups.resources` — all ChoreGroup definitions (filter by `.userPrioritizable`)
- `EdiblesManager.GetAllFoodTypes()` — all food definitions
- `Game.Instance.advancedPersonalPriorities` — Proximity mode toggle state

### 6.3 Speech and Text Filtering

- `SpeechPipeline.SpeakInterrupt(text)` — speaks text, interrupting current speech
- `SpeechPipeline.SpeakQueued(text)` — speaks text after current speech finishes
- `TextFilter.FilterForSpeech(text)` — strips HTML tags, rich text markup, and other visual formatting that would sound bad when spoken (must be applied to game tooltip text before speaking)

---

## 7. Common Screen Reader Interaction Patterns in OniAccess

### 7.1 Table Screens (Vitals, Consumables, Priorities)

These screens use `BaseTableHandler`, a 2D grid navigator. The handler maintains a virtual row/column cursor and speaks cell values on navigation.

Navigation:
- Arrow keys: move cursor
- Home/End: jump to first/last column
- Ctrl+Up/Down: scroll through rows while holding column
- Enter: activate cell (toggle permission, focus duplicant, etc.)
- Sort: configurable

The game's visual `TableScreen` populates UI rows dynamically; OniAccess builds its own virtual row list from live game data instead of reading the visual table.

### 7.2 Screens with Tabs (Research, Skills, Schedule)

These use `BaseScreenHandler` with a `CycleTab()` method. Tab/Shift+Tab cycles between tabs. Each tab is a separate object implementing the tab interface. The handler speaks the tab name when switching.

### 7.3 Tree Navigation (Research Tree, Skills Tree)

Both Research and Skills have a "Tree" tab that navigates a DAG (Directed Acyclic Graph) where nodes are connected by prerequisites. Navigation lets the player move to adjacent nodes (parents/children/siblings in the tree).

### 7.4 Tile Cursor (World Navigation)

`TileCursor` moves a cursor through the world grid. `GlanceComposer` assembles a spoken description of the current cell from multiple `ICellSection` sources (Element, Temperature, Building, Automation, etc.). The active `OverlayProfile` filters which sections to include based on the current overlay mode.

---

*Sources: ONI wiki (oxygennotincluded.wiki.gg), Klei Forums, `docs/oni_accessibility_audit.md`, OniAccess codebase.*
