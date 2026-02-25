# ONI Inspection Panel Systems

How the game displays information when entities are selected with the select tool.

## Selection Flow

1. Player clicks a tile with `SelectTool`
2. If multiple selectables exist at that cell, OniAccess opens `EntityPickerHandler` for the user to choose
3. `SelectTool.Select(KSelectable)` fires the global `GameHashes.SelectObject` event (-1503271301)
4. `DetailsScreen` (singleton) receives the event and calls `Refresh(GameObject)`
5. `Refresh` populates the title bar, validates tabs, validates side screens, and triggers `UserMenuScreen`

## DetailsScreen Layout

The details screen is a persistent singleton (`DetailsScreen.Instance`). It doesn't open/close like a `KScreen` — it shows/hides its existing instance. This means `KScreen.Activate`/`Deactivate` lifecycle patches won't catch it.

### Title Bar
- Entity name (editable for dupes and rockets via `EditableTitleBar`)
- Portrait/icon (building sprite, dupe face, element icon)
- Skill subtitle for dupes
- Codex entry button (opens in-game encyclopedia)
- Pin Resource button

### Main Tabs (DetailTabHeader)

Four tabs, each backed by a `TargetPanel` subclass. Tabs auto-hide when their panel returns `IsValidForTarget = false`.

| Tab ID | Panel Class | Shows For | Content |
|--------|------------|-----------|---------|
| SIMPLEINFO | `SimpleInfoScreen` | Everything (always valid) | Status items, storage contents, vitals, effects, stress, fertility, world traits, process conditions |
| PERSONALITY | `MinionPersonalityPanel` | Dupes only (`MinionIdentity`) | Traits, aptitudes, attributes, equipment, amenities, skills/resume |
| BUILDINGCHORES | `BuildingChoresPanel` | Entities with chores (not dupes) | Assigned chores with performer info |
| DETAILS | `AdditionalDetailsPanel` | Everything (always valid) | Element properties, thermal data, disease info, energy/circuit info, immune system (dupes) |

### Side Screen Tabs

Orthogonal to the main tabs. Four categories, each can host multiple side screens:

| Tab | Purpose |
|-----|---------|
| Config | Default — entity-specific controls |
| Errands | Dupe task assignments |
| Material | Deconstruction/reconstruction materials |
| Blueprints | Building facades, dupe outfits |

### User Menu (Right-Click Actions)

`UserMenuScreen` sits alongside the details panel. When a target is selected, the game fires event 493375141 on the entity — components respond by registering buttons (deconstruct, cancel, enable/disable, etc.). Buttons are sorted by priority constants (DECONSTRUCT_PRIORITY=0 through AUTODISINFECT_PRIORITY=10).

## Main Tab Details

### SimpleInfoScreen (SIMPLEINFO)

The most information-dense tab. Contains collapsible sections:

**Status Items** — Dynamic list from `KSelectable.GetStatusItemGroup()`. Each entry has icon, text, tooltip, color (by severity: bad/warning/good/info), and optional click callback. Refreshes on add/remove events.

**Storage Contents** — Shows items stored in the entity's `IStorage` components.
- Single items: name (max 15 chars), temperature, mass, click-to-select
- Multiple items of same type: collapsible row with total mass, expands to individual items
- Status icons inline: frozen/refrigerated, fresh/stale/spoiled, spiced, diseased
- Click on any stored item calls `SelectTool.Instance.Select(item)` to inspect it

**Vitals** (dupes/creatures) — Health, stamina, morale, calories, etc. via `Amounts` component.

**Effects/Requirements** — Active gameplay effects on the entity.

**Stress Breakdown** (dupes) — Sources of stress gain/loss.

**Fertility** (creatures) — Breeding rates and egg info.

**World Traits** — Biome characteristics (when inspecting planetoid info).

**Process Conditions** — Equipment readiness checklist.

### AdditionalDetailsPanel (DETAILS)

Element and environmental info:

- **Properties**: Mass, temperature, specific heat, thermal conductivity, melting/boiling points
- **Disease Source**: How disease spreads to this entity
- **Current Germs**: Disease count, growth/decay rates, environmental factors
- **Immune System** (dupes): Contraction probability by disease, trait-based immunity
- **Energy Overview**: Circuit status, watts generated/consumed, connected generators/consumers

### MinionPersonalityPanel (PERSONALITY)

Dupe-specific:
- Personality traits and aptitudes
- Current attributes and modifiers
- Equipment worn
- Amenity assignments
- Skill tree / resume progress

### BuildingChoresPanel (BUILDINGCHORES)

Shows chores assigned to a building with which dupe/robot is performing each.

## Side Screens (82 Player-Visible)

Side screens are `SideScreenContent` subclasses. Each implements:
- `IsValidForTarget(GameObject)` — component check to decide if this screen applies
- `SetTarget(GameObject)` — populate with entity data
- `GetTitle()` — header text
- `GetSideScreenSortOrder()` — display priority

During `DetailsScreen.Refresh`, every registered side screen prefab is checked against the target. Valid ones are instantiated (once, then reused) and shown; invalid ones are hidden.

### Class Hierarchy

87 concrete classes total. 83 extend `SideScreenContent` directly (plus 1 abstract base `SingleItemSelectionSideScreenBase`). 5 extend intermediate classes:

- `ReceptacleSideScreen` → `IncubatorSideScreen`, `PlanterSideScreen`, `SpecialCargoBayClusterSideScreen`
- `SingleItemSelectionSideScreenBase` → `FilterSideScreen`, `SingleItemSelectionSideScreen`

The `sideScreens` list on `DetailsScreen` is `[SerializeField]` — populated in the Unity editor, not in code. Each concrete class is a separate prefab. The parent `ReceptacleSideScreen` and its children coexist: the parent validates for generic `SingleEntityReceptacle` targets that aren't `PlantablePlot`, `EggIncubator`, or `SpecialCargoBayClusterReceptacle`; the children each claim their specific type.

### Never Shown (5 classes)

| Class | Reason |
|-------|--------|
| `NoConfigSideScreen` | `IsValidForTarget` returns `false` unconditionally |
| `ConditionListSideScreen` | `IsValidForTarget` returns `false` unconditionally |
| `HabitatModuleSideScreen` | `IsValidForTarget` returns `false` unconditionally |
| `RoleStationSideScreen` | `IsValidForTarget` returns `false` unconditionally |
| `AutoPlumberSideScreen` | Requires `DebugHandler.InstantBuildMode` — dev-only |

### Side Screens by Entity Type

**Buildings (general)**
- `ButtonMenuSideScreen` — Generic button menu (requires `ISidescreenButtonControl`)
- `AutomatableSideScreen` — Manual vs automation toggle
- `SingleSliderSideScreen` — Single slider control
- `SingleCheckboxSideScreen` — Single checkbox toggle
- `TreeFilterableSideScreen` — Hierarchical item filtering (storage bins, etc.)
- `FlatTagFilterSideScreen` — Tag-based item filtering
- `CapacityControlSideScreen` — Capacity slider + number input
- `ActiveRangeSideScreen` — Activation/deactivation thresholds
- `PlayerControlledToggleSideScreen` — Manual on/off toggle
- `CheckboxListGroupSideScreen` — Grouped checkbox lists (requires `ICheckboxListGroupControl`)
- `DispenserSideScreen` — Dispensing filter/controls
- `TagFilterScreen` — Tag-based filtering

**Doors**
- `DoorToggleSideScreen` — Open/close/auto toggle
- `SealedDoorSideScreen` — Sealed door controls
- `AccessControlSideScreen` — Per-dupe/robot access permissions

**Power**
- `ThresholdSwitchSideScreen` — Threshold value configuration (batteries, etc.)
- `TemperatureSwitchSideScreen` — Temperature threshold
- `HighEnergyParticleDirectionSideScreen` — HEP beam direction

**Pipes/Valves**
- `ValveSideScreen` — Flow control
- `LimitValveSideScreen` — Limit valve thresholds

**Logic/Automation**
- `CounterSideScreen` — Logic counter
- `TimerSideScreen` — Timer configuration
- `TimeRangeSideScreen` — Time-of-day ranges
- `LogicBitSelectorSideScreen` — Bit selection
- `LogicBroadcastChannelSideScreen` — Broadcast channels
- `CritterSensorSideScreen` — Critter sensor config
- `CometDetectorSideScreen` — Comet detection target
- `ClusterLocationFilterSideScreen` — Location sensor filter
- `AlarmSideScreen` — Alarm notification settings

**Fabrication/Production**
- `ComplexFabricatorSideScreen` — Recipe selection and queue management
- `ReceptacleSideScreen` — Generic single-entity receptacle (excludes planters, incubators, special cargo)
- `PlanterSideScreen` — Plant/seed receptacle (extends `ReceptacleSideScreen`)
- `IncubatorSideScreen` — Egg incubator (extends `ReceptacleSideScreen`)
- `SpecialCargoBayClusterSideScreen` — Special cargo receptacle (extends `ReceptacleSideScreen`)
- `ConfigureConsumerSideScreen` — Consumption preferences
- `FilterSideScreen` — Single-item filter selection (extends `SingleItemSelectionSideScreenBase`)
- `SingleItemSelectionSideScreen` — Single-item selection (extends `SingleItemSelectionSideScreenBase`)

**Assignments**
- `AssignableSideScreen` — Assign item to specific dupe
- `MinionTodoSideScreen` — Dupe priority/task queue
- `OwnablesSidescreen` — Ownable item management (dupes)
- `BionicSideScreen` — Bionic dupe upgrades/battery

**Rockets**
- `CommandModuleSideScreen` — Launch condition checklist
- `ClusterDestinationSideScreen` — Destination/landing selection
- `AssignPilotAndCrewSideScreen` — Crew assignment
- `LaunchButtonSideScreen` — Launch state
- `LaunchPadSideScreen` — Launch pad config
- `CargoModuleSideScreen` — Cargo collection progress
- `RocketModuleSideScreen` — Module assembly/reorder
- `RocketInteriorSectionSideScreen` — Interior config
- `RocketRestrictionSideScreen` — Movement restrictions
- `ModuleFlightUtilitySideScreen` — Flight utility control
- `HarvestModuleSideScreen` — Harvest module
- `SummonCrewSideScreen` — Summon crew to rocket
- `SelfDestructButtonSideScreen` — Self-destruct (rocket in space only)
- `TemporalTearSideScreen` — Temporal tear management (rocket at tear location)

**Creatures/Biology**
- `LureSideScreen` — Critter lure settings
- `GeneShufflerSideScreen` — Gene shuffler
- `GeneticAnalysisStationSideScreen` — Analyzed organisms
- `RelatedEntitiesSideScreen` — Related entities list

**Cosmetics/Lore**
- `ArtableSelectionSideScreen` — Art variant selection (paintings, sculptures)
- `ArtifactAnalysisSideScreen` — Artifact analysis results
- `LoreBearerSideScreen` — Artifact lore text
- `PixelPackSideScreen` — Pixel pack display config
- `MonumentSideScreen` — Monument customization

**Research/Science**
- `ResearchSideScreen` — Research center / nuclear research center
- `TelescopeSideScreen` — Telescope controls
- `GeoTunerSideScreen` — Geyser tuning controls

**Special/Misc**
- `MissileSelectionSideScreen` — Missile type selection
- `RailGunSideScreen` — Rail gun targeting
- `SuitLockerSideScreen` — Suit assignment
- `TelepadSideScreen` — Telepad/printing pod config
- `WarpPortalSideScreen` — Warp portal config
- `ClusterGridWorldSideScreen` — Asteroid info on starmap
- `RemoteWorkTerminalSidescreen` — Remote work terminal controls
- `PrinterceptorSideScreen` — Hijacked HQ (event-specific)
- `BaseGameImpactorImperativeSideScreen` — Impactor defense (event-specific)
- `ProgressBarSideScreen` — Generic progress display (requires `IProgressBar`)

**Interface-Driven Generic Patterns** (interface on the building → side screen)

| Interface | Side Screen |
|-----------|-------------|
| `ISidescreenButtonControl` | `ButtonMenuSideScreen` |
| `ISingleSlider` | `SingleSliderSideScreen` |
| `ISingleCheckboxSidescreen` | `SingleCheckboxSideScreen` |
| `IIntSlider` | `IntSliderSideScreen` |
| `IDualSlider` | `DualSliderSideScreen` |
| `IMultiSlider` | `MultiSliderSideScreen` |
| `INToggle` | `NToggleSideScreen` |
| `IFewOptionSidescreen` | `FewOptionSideScreen` |
| `IProgressBar` | `ProgressBarSideScreen` |
| `IActivationRangeTarget` | `ActiveRangeSideScreen` |
| `IUserControlledCapacity` | `CapacityControlSideScreen` |
| `ICheckboxListGroupControl` | `CheckboxListGroupSideScreen` |
| `IConfigurableConsumer` | `ConfigureConsumerSideScreen` |

## What Different Entities Show

### Duplicant
- **Tabs**: SIMPLEINFO, PERSONALITY, DETAILS (no BUILDINGCHORES)
- **SIMPLEINFO**: Status items, vitals (health/stamina/morale/calories), storage (inventory), stress breakdown, effects
- **PERSONALITY**: Traits, aptitudes, attributes, equipment, skills
- **DETAILS**: Disease/immune system, element properties
- **Side screens**: AssignableSideScreen, MinionTodoSideScreen, BionicSideScreen (if bionic)
- **User menu**: Follow cam, move to, priority

### Building (completed)
- **Tabs**: SIMPLEINFO, BUILDINGCHORES, DETAILS (no PERSONALITY)
- **SIMPLEINFO**: Status items, storage contents (if storage building), effects/requirements
- **BUILDINGCHORES**: Assigned chores with performers
- **DETAILS**: Element properties, thermal data, energy/circuit info
- **Side screens**: Varies enormously by building type — see list above. Many buildings implement generic interfaces (ISingleSlider, ISidescreenButtonControl, etc.)
- **User menu**: Deconstruct, disable, priority, copy settings, etc.

### Building (under construction)
- **Tabs**: SIMPLEINFO, DETAILS
- **SIMPLEINFO**: Status items (awaiting materials, etc.), required materials
- **DETAILS**: Element properties
- **Side screens**: Minimal
- **User menu**: Cancel construction, priority

### Creature
- **Tabs**: SIMPLEINFO, DETAILS (no PERSONALITY, no BUILDINGCHORES)
- **SIMPLEINFO**: Status items, vitals, fertility/breeding info
- **DETAILS**: Element properties, disease
- **Side screens**: Minimal (some creatures may trigger generic screens)
- **User menu**: Wrangle, move to

### Item/Debris (loose material on ground)
- **Tabs**: SIMPLEINFO, DETAILS
- **SIMPLEINFO**: Status items
- **DETAILS**: Mass, temperature, element properties, disease
- **Side screens**: None typically
- **User menu**: Sweep, priority

### Element at Tile (CellSelectionObject — gas/liquid/solid tile)
- **Tabs**: SIMPLEINFO, DETAILS
- **SIMPLEINFO**: Status items
- **DETAILS**: Mass, temperature, specific heat, thermal conductivity, melting/boiling points, disease
- **Side screens**: None
- **User menu**: None (can't interact with raw elements)
- **Special**: Updates every 0.5s while selected, fires refresh event

### Plant
- **Tabs**: SIMPLEINFO, DETAILS
- **SIMPLEINFO**: Status items (growth stage, irrigation needs, etc.), effects
- **DETAILS**: Element properties, temperature
- **Side screens**: Depends on plant type
- **User menu**: Harvest, uproot, priority

## Codex Entry Button

Present in the title bar. Determines the codex ID by checking (in priority order):
1. `CellSelectionObject` → element ID
2. `CodexEntryRedirector` → custom codex ID
3. `BuildingUnderConstruction` → building def prefab ID
4. `CreatureBrain` → prefab ID (minus "BABY" suffix)
5. `PlantableSeed` → seed prefab ID
6. Fallback: extract from `GetProperName()`

Button is only interactive if the codex entry exists in `CodexCache`.

## How Data Flows

All panel info reads live from game components on the entity — no caching:

```
GameObject target = DetailsScreen.Instance.target;

// Common
KSelectable selectable → StatusItemGroup (status items)
PrimaryElement → element type, mass, temperature, disease

// Dupes
MinionIdentity → name, personality
MinionResume → skills, experience
Amounts → health, stamina, morale, calories, etc.

// Buildings
Building → definition, orientation
BuildingComplete → construction materials
Operational → operational status
IStorage[] → stored items

// Creatures
CreatureBrain → species, behavior
WiltCondition → plant health

// Elements
CellSelectionObject → mass, element, temperature, disease, state
```

## Key Event Hashes

| Hash | Name | Trigger | Listener |
|------|------|---------|----------|
| -1503271301 | SelectObject | SelectTool.Select() | DetailsScreen |
| -1514841199 | RefreshUserMenu | CellSelectionObject.UpdateValues() | SimpleInfoScreen, AdditionalDetailsPanel |
| 493375141 | (UserMenu populate) | UserMenuScreen.Refresh() | Entity components register buttons |
| -1697596308 | OnItemSpawned | Storage changes | SimpleInfoScreen storage refresh |
| -1197125120 | OnItemRemoved | Storage changes | SimpleInfoScreen storage refresh |

## Key Source Files

| File | Lines | Purpose |
|------|-------|---------|
| ONI-Decompiled/Assembly-CSharp/SelectTool.cs | 186 | Selection mechanism |
| ONI-Decompiled/Assembly-CSharp/DetailsScreen.cs | 1051 | Main details panel |
| ONI-Decompiled/Assembly-CSharp/DetailTabHeader.cs | 135 | Tab system |
| ONI-Decompiled/Assembly-CSharp/SimpleInfoScreen.cs | 1419 | Info tab (status, storage, vitals) |
| ONI-Decompiled/Assembly-CSharp/AdditionalDetailsPanel.cs | 698 | Details tab (element, disease, energy) |
| ONI-Decompiled/Assembly-CSharp/MinionPersonalityPanel.cs | — | Personality tab (dupes only) |
| ONI-Decompiled/Assembly-CSharp/BuildingChoresPanel.cs | — | Chores tab |
| ONI-Decompiled/Assembly-CSharp/SideScreenContent.cs | 32 | Side screen base class |
| ONI-Decompiled/Assembly-CSharp/TargetPanel.cs | 40 | Tab panel base class |
| ONI-Decompiled/Assembly-CSharp/UserMenu.cs | 119 | Right-click menu data |
| ONI-Decompiled/Assembly-CSharp/UserMenuScreen.cs | 182 | Right-click menu UI |
| ONI-Decompiled/Assembly-CSharp/CellSelectionObject.cs | 291 | Element/tile selection data |
