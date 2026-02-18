# ONI Tool System Reference

This document describes how tools (Dig, Mop, Build, etc.) work in Oxygen Not Included at the code level. It covers the full interaction model, class hierarchy, per-tool behavior, the filter system, priority system, and all associated UI/audio feedback.

## Interaction Model Overview

The core loop is: **select tool → drag area → release to apply**. There is no confirmation step.

1. Player selects a tool via toolbar toggle or hotkey
2. The game switches to the tool's overlay (if any) and shows tool-specific UI (priority bar, filter menu)
3. Player clicks and drags on the map to define an area
4. On mouse release (Box/Line mode) or continuously during drag (Brush mode), the tool processes every cell in the region
5. Right-click or Escape deactivates the tool and returns to SelectTool

## Class Hierarchy

```
InterfaceTool                          (base: cursor, hover, click handling)
├── SelectTool                         (default tool, click to inspect entities)
├── MoveToLocationTool                 (single click, sends dupe to location)
├── StampTool                          (blueprint/stamp placement)
├── PrebuildTool                       (pre-build visualization)
├── BrushTool                          (circular brush with radius, sandbox tools)
│   ├── SandboxBrushTool
│   ├── SandboxSprinkleTool
│   ├── SandboxClearFloorTool
│   ├── SandboxCritterTool
│   ├── SandboxDestroyerTool
│   ├── SandboxFOWTool
│   ├── SandboxHeatTool
│   └── SandboxStressTool
├── FloodTool                          (flood fill from click point)
│   └── SandboxFloodTool
├── SandboxSampleTool                  (point sample)
├── SandboxSpawnerTool                 (point spawn)
├── SandboxStoryTraitTool              (point placement)
└── DragTool                           (adds drag mechanics: Box/Brush/Line)
    ├── DigTool
    ├── DisinfectTool
    ├── ClearTool (Sweep)
    ├── MopTool
    ├── AttackTool
    ├── CaptureTool
    ├── HarvestTool
    ├── CopySettingsTool
    ├── BuildTool                      (Brush mode, standard buildings)
    ├── PlaceTool                      (Brush mode, single-place items)
    ├── DebugTool
    ├── BaseUtilityBuildTool           (Brush mode, path-tracing for conduits)
    │   ├── UtilityBuildTool           (pipes, conveyors)
    │   └── WireBuildTool              (wires, logic)
    └── FilteredDragTool               (adds layer filter menu)
        ├── CancelTool
        ├── DeconstructTool
        ├── PrioritizeTool
        ├── EmptyPipeTool
        └── DisconnectTool             (Line mode, max 2 cells)
```

## Drag Modes

DragTool defines three modes via the `Mode` enum:

| Mode | Behavior | When Applied |
|------|----------|--------------|
| **Box** | Click-drag defines a rectangle. All cells processed on release. | Mouse up |
| **Brush** | Each cell the cursor passes over is processed immediately. Interpolates gaps between frames. | Continuously during drag |
| **Line** | Like Box but constrained to a single axis, with optional max length. | Mouse up |

Holding **Shift** (`Action.DragStraight`) during any Box drag snaps it to a straight horizontal or vertical line.

### Box Mode (default for most tools)

1. `OnLeftClickDown`: Records `downPos`, hides cell visualizer, shows area visualizer
2. `OnMouseMove`: Resizes area visualizer, updates dimension text (`"{width} x {height} = {area}"`), plays drag sound with `tileCount` FMOD parameter
3. `OnLeftClickUp`: Iterates every cell in the rectangle. For each cell where `Grid.IsValidCell(cell) && Grid.IsVisible(cell)`, calls `OnDragTool(cell, distFromOrigin)` where `distFromOrigin` is the Manhattan distance from the click origin. Then plays confirm sound and calls `OnDragComplete()`.

#### The `distFromOrigin` Parameter

`distFromOrigin` is the Manhattan distance (`|cellX - originX| + |cellY - originY|`) from the drag start point. In Brush mode, it is always 0.

**Only one tool uses it:** DigTool passes it through `MarkCellDigAction` as an animation delay. Each dig marker plays a "ScaleUp" animation delayed by `distFromOrigin * 0.02` seconds, creating a visual ripple effect emanating from the click origin. Every other tool ignores it entirely. It has no gameplay effect.

### Brush Mode (build tools)

Each frame during drag, the tool processes cells the cursor passes over. `AddDragPoints()` interpolates between the previous and current cursor positions to fill gaps. During drag, shows length text (`UI.TOOLS.TOOL_LENGTH_FMT`).

### Line Mode (DisconnectTool only)

Same rendering as Box but axis-constrained. DisconnectTool sets `lineModeMaxLength = 2`.

## Toolbar Layout

The toolbar is managed by `ToolMenu`, organized as `rows` containing `ToolCollection` groups. Each collection currently contains exactly one tool (the architecture supports multi-tool collections with sub-menus, but none exist in practice).

### Visual Layout

```
┌─────────────────────────────────────────────────────────┐
│  [DIG] [CANCEL] [DECON] [PRIOR]  ┌Disinfect┐ ┌Attack ┐ ┌Capture ┐ ┌EmptyPipe ┐ │
│   (G)    (C)      (X)     (P)    │  (I)    │ │  (T)  │ │  (N)   │ │ (Insert) │ │
│          large icons              ├─────────┤ ├───────┤ ├────────┤ ├──────────┤ │
│                                   │ Sweep   │ │ Mop   │ │Harvest │ │Disconnect│ │
│                                   │  (K)    │ │  (M)  │ │  (Y)   │ │          │ │
│                                   └─────────┘ └───────┘ └────────┘ └──────────┘ │
└─────────────────────────────────────────────────────────┘
```

- **Large icons** (first 4): Dig, Cancel, Deconstruct, Prioritize -- each gets `toolIconLargePrefab`
- **Small icons** (remaining 8): arranged in a **two-row grid**, alternating top/bottom:
  - **Top row**: Disinfect, Attack, Capture, Empty Pipe
  - **Bottom row**: Sweep, Mop, Harvest, Disconnect
- When small tool count is even (it is: 8), the bottom row gets `padding.left = 26` and top row gets `padding.right = 26`, creating a staggered/offset visual effect
- Sandbox tools (12 total) occupy a separate row, toggled via Shift+S, all using `sandboxToolIconPrefab`

### Collection/Toggle Behavior

Each tool is its own `ToolCollection` with a single `KToggle` button. Pressing a hotkey:
- If the tool is not selected: selects it
- If the tool is already selected: deactivates it (returns to SelectTool)

The architecture supports multi-tool collections (a group toggle that opens a sub-menu of individual tools), using `collectionIconPrefab` for groups of < 5 and `Prefab_collectionContainerWindow` for groups of >= 5. No current tools use this.

## Complete Tool List

### Standard Toolbar Tools

Created in `ToolMenu.CreateBasicTools()`. Displayed in two rows: first 4 large-icon, rest small-icon.

| Tool | Hotkey | Drag Mode | Has Filters | Has Priority | Overlay | Number Keys |
|------|--------|-----------|-------------|--------------|---------|-------------|
| Dig | G | Box | No | Yes | None | No |
| Cancel | C | Box | Yes (layer + dig/sweep) | No | None | No |
| Deconstruct | X | Box | Yes (layer) | Yes | None | No |
| Prioritize | P | Box | Yes (errand type) | Yes (with diagram) | Priorities | Yes |
| Disinfect | I | Box | No | Yes | Disease | Yes |
| Sweep | K | Box | No | Yes | None | Yes |
| Attack | T | Box | No | Yes | None | No |
| Mop | M | Box | No | Yes | None | Yes |
| Capture | N | Box | No | Yes | None | No |
| Harvest | Y | Box | Own menu (harvest/don't) | Yes | Harvest | No |
| Empty Pipe | Insert | Box | Yes (conduit type) | Yes | None | No |
| Disconnect | (none) | Line (max 2) | Yes (layer, no backwall) | No | None | No |

"Number Keys" means the tool sets `interceptNumberKeysForPriority = true`, allowing keys 1-9 to change priority and 0 for emergency priority. Tools without this still show the priority bar but require clicking the priority buttons.

### Build Tools (activated from build menu)

| Tool | Drag Mode | Notes |
|------|-----------|-------|
| BuildTool | Brush | Places buildings cell-by-cell. Rotate with O. Red/white ghost preview. See [Building Rotation](#building-rotation). |
| UtilityBuildTool | Brush (path) | Traces connected pipe/conveyor segments. Path built on release. See [Utility Build Path Tracing](#utility-build-path-tracing). |
| WireBuildTool | Brush (path) | Same as UtilityBuildTool but for power wires and logic. Forces Power overlay. See [Utility Build Path Tracing](#utility-build-path-tracing). |
| PlaceTool | Brush | Single placement, auto-deactivates after placing. |
| CopySettingsTool | Box | Activated from entity user menu, not toolbar. Copies settings to matching buildings in drag area. |

### Non-Toolbar Tools

| Tool | Type | Behavior |
|------|------|----------|
| SelectTool | InterfaceTool | Default tool. Single click to inspect entities. Always `tools[0]`. |
| MoveToLocationTool | InterfaceTool | Single click to send a movable entity to a location. |
| StampTool | InterfaceTool | Blueprint/stamp placement. |

### Sandbox Tools (toggled via Shift+S)

All extend BrushTool (circular radius) or FloodTool/InterfaceTool:

| Tool | Hotkey Action | Parent |
|------|---------------|--------|
| Brush | SandboxBrush | BrushTool |
| Sprinkle | SandboxSprinkle | BrushTool |
| Flood | SandboxFlood | FloodTool |
| Sample | SandboxSample | InterfaceTool |
| Heat Gun | SandboxHeatGun | BrushTool |
| Stress | SandboxStressTool | BrushTool |
| Spawner | SandboxSpawnEntity | InterfaceTool |
| Clear Floor | SandboxClearFloor | BrushTool |
| Destroy | SandboxDestroy | BrushTool |
| Reveal (FOW) | SandboxReveal | BrushTool |
| Critter | SandboxCritterTool | BrushTool |
| Story Trait | SandboxStoryTraitTool | InterfaceTool |

## Per-Tool Cell Validity

Each tool overrides `OnDragTool(cell, distFromOrigin)` (or `OnDragComplete` for entity-based tools) and is fully responsible for deciding whether a cell is actionable. The base class only pre-filters with `Grid.IsValidCell(cell) && Grid.IsVisible(cell)`.

### DigTool

Delegates to `MarkCellDigAction.Dig()` which calls `DigTool.PlaceDig(cell)`:

1. `Grid.Solid[cell]` -- must be solid
2. `!Grid.Foundation[cell]` -- must not be a built tile
3. `Grid.Objects[cell, 7] == null` -- no existing dig order at object layer 7
4. No `Constructable` component at any object layer (can't dig something being built)

Also calls `Uproot(cell)` first, which checks object layers 1 and 5 for `IDigActionEntity` (plants) and marks them for dig.

**Already-marked handling:** If a dig order already exists, returns the existing placer and updates its priority.

### MopTool

1. `!Grid.Solid[cell]` -- must not be solid
2. `Grid.Element[cell].IsLiquid` -- must contain a liquid
3. `Grid.Solid[Grid.CellBelow(cell)]` -- must have a solid floor below
4. `Grid.Mass[cell] <= 150kg` -- liquid mass must not exceed threshold
5. `Grid.Objects[cell, 8] == null` -- no existing mop order at layer 8

**Per-cell feedback:** If liquid present but floor/mass checks fail, spawns a PopFX with reason text:
- `UI.TOOLS.MOP.TOO_MUCH_LIQUID` if mass > 150kg
- `UI.TOOLS.MOP.NOT_ON_FLOOR` if no solid floor below

### DisinfectTool

Iterates all 45 object layers at the cell. For each object:
1. Has `Disinfectable` component
2. `DiseaseCount > 0` on its `PrimaryElement`

**Already-marked:** `MarkForDisinfect` guards with `!isMarkedForDisinfect`, silently skipped.

### ClearTool (Sweep)

Checks `Grid.Objects[cell, 3]` (Pickupable layer), walks the linked list:
1. Not null
2. Not a duplicant (`!HasTag(GameTags.BaseMinion)`)
3. `pickupable.Clearable.isClearable`

### AttackTool

Does NOT use `OnDragTool` per-cell. Overrides `OnDragComplete` instead. After box drag, iterates all `Components.FactionAlignments.Items` globally, checks if each creature's position falls within the drag rectangle and faction disposition is not `Assist` (friendly).

### CaptureTool

Same pattern as AttackTool -- `OnDragComplete`, iterates `Components.Capturables.Items` within bounds. Checks `item.allowCapture`.

**Feedback:** If not capturable, spawns PopFX with `UI.TOOLS.CAPTURE.NOT_CAPTURABLE`.

### HarvestTool

Looks for `HarvestDesignatable` component at the cell. Reads its parameter menu to determine mode (harvest when ready vs. do not harvest). Sets the harvest designation accordingly.

### BuildTool

The most complex validation chain. Uses `BuildingDef.IsValidPlaceLocation` + `IsValidBuildLocation`:

1. `Grid.IsValidBuildingCell(cell)` -- within world bounds
2. World restriction check
3. `BuildLocationRule`-specific checks (see full list below)
4. `IsAreaClear` -- for each placement offset cell: valid building cell, same world, not neutronium, not occupied by another building (with special cases for bridges, conduits, wires), uprootable plants can be displaced

**Ghost preview:** Every mouse move, the visualizer is tinted red (invalid) or white (valid) based on `IsValidPlaceLocation`.

**Already-occupied:** Checks for replacement candidates via `GetReplacementCandidate(cell)` with `Replaceable` component and `CanReplace` compatibility.

**OnePerWorld:** If the building has this flag, auto-deactivates the tool after placing.

#### BuildLocationRule (all 20 values)

Each building has a `BuildLocationRule` that determines where it can be placed. The rule is checked in `BuildingDef.IsValidBuildLocation()` and `IsValidPlaceLocation()`. When placement fails, a `fail_reason` string is produced for the hover text.

| Rule | Meaning | Fail Reason |
|------|---------|-------------|
| `Anywhere` | No restrictions | (generic only) |
| `OnFloor` | Solid tiles below every cell of building width | "Must be built on solid ground" |
| `OnCeiling` | Solid tiles above every cell of building width | "Must be built on solid ground" (reuses floor string) |
| `OnWall` | Solid tiles adjacent on one side for full height | "Must be built against a wall" |
| `InCorner` | Ceiling AND wall (both must pass) | "Must be built in a corner" |
| `WallFloor` | Floor AND wall (both must pass) | "Must be built in a corner on the ground" |
| `OnFloorOverSpace` | Floor + all cells in Space zone | "Must be built on the surface in space" |
| `OnFoundationRotatable` | Like OnFloor but rotated foundation (e.g., auto-miner on ceiling) | "Must be built on solid ground" |
| `OnBackWall` | Non-solid cells with back wall objects (drywall) behind them | "Must be built against a back wall" |
| `OnRocketEnvelope` | Foundation tiles tagged `RocketEnvelopeTile` | "Must be built on the interior wall of a rocket" |
| `BelowRocketCeiling` | 35 tiles clearance below world top (checked in IsValidPlaceLocation only) | "Must be placed further from the edge of space" |
| `BuildingAttachPoint` | On a matching attachment point from another building (rocket modules, etc.) | "Must be built overlapping a {tag}" |
| `OnFloorOrBuildingAttachPoint` | Floor OR attachment point (validated at runtime, not build time) | "Must be built on solid ground or overlapping an {tag}" |
| `Tile` | Building IS a tile. Checks for NotInTiles conflicts on wire/backwall layers | "Obstructed by Heavi-Watt Wire" / "Obstructed by back wall" |
| `NotInTiles` | Cannot overlap tiles or doors. Mutual exclusion with Tile rule | "Cannot be built inside tile" |
| `Conduit` | Always valid placement. Skips occupied check in IsAreaClear (can overlay) | (generic only) |
| `LogicBridge` | Logic wire bridge. Skips occupied check. Checks logic port overlap | "Automation ports cannot overlap" |
| `WireBridge` | Power wire bridge. Skips occupied check. Checks wire connector overlap | "Power connectors cannot overlap" |
| `HighWattBridgeTile` | Heavi-Watt joint plate. Must satisfy BOTH tile AND high-watt bridge checks | Both tile and wire errors possible |
| `NoLiquidConduitAtOrigin` | Bidirectional: blocks liquid conduits at this cell AND this building can't go where liquid conduits are | "Obstructed by a building" |

**Notable quirks:**
- OnCeiling reuses the "solid ground" fail string despite being a ceiling check (the game defines but never uses a "Must be built on the ceiling" string)
- Conduit, LogicBridge, and WireBridge skip the normal "occupied" check, allowing them to overlay other buildings
- HighWattBridgeTile is the most restrictive rule: it must satisfy two independent validation methods simultaneously
- NoLiquidConduitAtOrigin is the only rule that restricts OTHER buildings' placement (bidirectional exclusion)
- OnFloorOrBuildingAttachPoint passes build-time validation unconditionally; the real enforcement is at runtime via `RequiresFoundation` walking the attachment chain

### Filtered Tools (Cancel, Deconstruct, EmptyPipe, Disconnect)

Use `FilteredDragTool.GetFilterLayerFromGameObject()` to map each object to a filter layer, then check `IsActiveLayer()` against the current filter selection. See Filter System section below.

### CancelTool (special)

Iterates all 45 object layers per cell, gets filter layer from each object, fires cancel event (hash `2127324410`). Also calls `AttackTool.MarkForAttack(min, max, mark: false)` and `CaptureTool.MarkForCapture(min, max, mark: false)` in `OnDragComplete` to cancel attack/capture orders.

### Summary Table

| Tool | Validation | Per-Cell Feedback | Already-Marked |
|------|-----------|-------------------|----------------|
| Dig | Solid, not foundation, no constructable | None | Updates priority |
| Mop | Liquid, floor below, mass <= 150kg | PopFX with reason | Skips, updates priority |
| Disinfect | Disinfectable + diseased | None | Silently skipped |
| Sweep | Pickupable, clearable, not dupe | None | Presumably guarded |
| Attack | Entity in bounds, hostile faction | None | Re-marks |
| Capture | Entity in bounds, capturable | PopFX "NOT_CAPTURABLE" | Re-marks |
| Harvest | HarvestDesignatable at cell | None | Sets state regardless |
| Build | IsValidPlaceLocation + BuildLocationRule | Red/white ghost tint | Replacement check |
| Deconstruct | FilteredDragTool layer filter | None | Fires event |
| Cancel | FilteredDragTool layer filter | None | Fires cancel event |
| Empty Pipe | FilteredDragTool layer filter | None | Fires event |
| Disconnect | FilteredDragTool layer filter, line max 2 | None | Fires event |

## Filter System

### ToolParameterMenu

`ToolParameterMenu` is a generic radio-button widget. It takes a `Dictionary<string, ToggleState>` and creates one `MultiToggle` per entry. Toggle states are `On`, `Off`, or `Disabled` (grayed out, unclickable). Only one filter can be `On` at a time (radio-button semantics via `ChangeToSetting()`).

Labels are resolved from `STRINGS.UI.TOOLS.FILTERLAYERS.<KEY>.NAME`. Tooltips from `STRINGS.UI.TOOLS.FILTERLAYERS.<KEY>.TOOLTIP`.

Two separate `ToolParameterMenu` instances exist in the game:
1. `ToolMenu.Instance.toolParameterMenu` -- used by tool filters
2. One created dynamically by `OverlayLegend` -- used for overlay legend filters (temperature modes, material types, disease pipe types)

### FilteredDragTool

Extends DragTool. Manages two separate filter dictionaries:
- `filterTargets` -- used when a relevant overlay is active (gets locked to a single filter)
- `overlayFilterTargets` -- used in normal mode (all filters available)
- `currentFilterTargets` -- pointer to whichever is currently active

**Default filters** (from `GetDefaultFilters()`):

| Key | Default State | Display Name |
|-----|---------------|--------------|
| ALL | On | "All" |
| WIRES | Off | "Power Wires" |
| LIQUIDCONDUIT | Off | "Liquid Pipes" |
| GASCONDUIT | Off | "Gas Pipes" |
| SOLIDCONDUIT | Off | "Conveyor Rails" |
| BUILDINGS | Off | "Buildings" |
| LOGIC | Off | "Automation" |
| BACKWALL | Off | "Background Buildings" |

When `ALL` is `On`, `IsActiveLayer()` returns true for everything. Otherwise, only the specifically enabled filter passes.

### Overlay-Aware Auto-Switching

When the player changes overlays while a FilteredDragTool is active, `OnOverlayChanged()` fires:

| Overlay | Forced Filter | All Others |
|---------|---------------|------------|
| Power | WIRES forced On | All Disabled |
| Liquid Conduits | LIQUIDCONDUIT forced On | All Disabled |
| Gas Conduits | GASCONDUIT forced On | All Disabled |
| Solid Conveyor | SOLIDCONDUIT forced On | All Disabled |
| Logic | LOGIC forced On | All Disabled |
| Any other | Normal mode restored | All available |

The overlay state and normal state are tracked independently, so switching back to a non-matching overlay restores the previous filter selection.

### Per-Tool Filter Customization

**CancelTool** -- adds two extra filters beyond defaults:
- CLEANANDCLEAR ("Sweep & Mop Orders") -- Off
- DIGPLACER ("Dig Orders") -- Off

**DeconstructTool** -- uses default filters exactly.

**PrioritizeTool** -- completely different filter set (errand types, not building layers):

| Key | Display Name | Matches |
|-----|-------------|---------|
| ALL | "All" | Everything |
| CONSTRUCTION | "Construction" | `Constructable` or `Deconstructable` (marked) |
| DIG | "Digging" | `Diggable` |
| CLEAN | "Cleaning" | `Clearable`, `Moppable`, `StorageLocker` |
| OPERATE | "Duties" | Everything else |

**EmptyPipeTool** -- reduced set (only pipe types):

| Key | Display Name |
|-----|-------------|
| ALL | "All" |
| LIQUIDCONDUIT | "Liquid Pipes" |
| GASCONDUIT | "Gas Pipes" |
| SOLIDCONDUIT | "Conveyor Rails" |

**DisconnectTool** -- like defaults but no BACKWALL (can't disconnect background buildings).

### HarvestTool's Custom Menu

HarvestTool extends DragTool directly (not FilteredDragTool) but still uses ToolParameterMenu as a mode toggle rather than a filter:

| Key | Display Name | Meaning |
|-----|-------------|---------|
| HARVEST_WHEN_READY | "Enable Harvest" | Mark plants for auto-harvest |
| DO_NOT_HARVEST | "Disable Harvest" | Prevent harvesting |

No overlay auto-switching since it's not a FilteredDragTool.

### Object-to-Filter Mapping

`GetFilterLayerFromGameObject()` maps game objects to filter strings:

| Component Found | Filter Layer |
|----------------|-------------|
| BuildingComplete | Maps via ObjectLayer (see below) |
| BuildingUnderConstruction | Maps via ObjectLayer (see below) |
| Clearable or Moppable | "CleanAndClear" |
| Diggable | "DigPlacer" |
| None of the above | "Default" |

`GetFilterLayerFromObjectLayer()`:

| ObjectLayer | Filter Layer |
|-------------|-------------|
| Building, Gantry | "Buildings" |
| Wire, WireConnectors | "Wires" |
| LiquidConduit, LiquidConduitConnection | "LiquidPipes" |
| GasConduit, GasConduitConnection | "GasPipes" |
| SolidConduit, SolidConduitConnection | "SolidConduits" |
| FoundationTile | "Tiles" |
| LogicGate, LogicWire | "Logic" |
| Backwall | "BackWall" |
| Anything else | "Default" |

## Priority System

### PriorityClass Hierarchy

```
idle (-1) < basic (0) < high (1) < personalNeeds (2) < topPriority (3) < compulsory (4)
```

`PrioritySetting` is a struct with `priority_class` and `priority_value` (1-9). Comparison is class-first, then value. Any `topPriority` task outranks all `basic` and `high` tasks regardless of numeric value.

Default priority everywhere: `basic, 5`.

### Two Independent Priority Screens

1. **ToolMenu.Instance.PriorityScreen** -- used by most action tools (Dig, Deconstruct, Mop, Sweep, Attack, Capture, Harvest, EmptyPipe, Prioritize)
2. **MaterialSelectionPanel.PriorityScreen** -- embedded in the build menu's material selection panel. Used by BuildTool and BaseUtilityBuildTool.

These track separate state. Changing priority on the toolbar does not affect build priority, and vice versa.

### PriorityScreen UI Layout

- 9 numbered `PriorityButton` instances (values 1-9), created from `buttonPrefab_basic`
- 1 emergency button (`button_emergency`), hardcoded to `topPriority, 1`
- 1 hidden high-priority toggle (`button_toggleHigh`, `SetActive(false)` by default) -- when on, switches all 9 buttons from `basic` to `high` class
- 1 button opening the full duplicant priorities management screen
- 1 diagram (hidden by default, shown only by PrioritizeTool at 1.35x scale)

### Number Key Shortcuts

Implemented in `DragTool.HandlePriortyKeysDown()` (note: game typo in method name). Maps `Action.Plan1`-`Plan9` to basic priority 1-9, `Action.Plan10` to emergency (`topPriority, 1`). Always targets `ToolMenu.Instance.PriorityScreen`.

**Only these tools intercept number keys for priority:**
- ClearTool (Sweep)
- DisinfectTool
- MopTool
- PrioritizeTool

All other tools that show the priority bar (Dig, Deconstruct, Harvest, Attack, Capture, EmptyPipe) do NOT intercept number keys. The user must click priority buttons instead, or number keys fall through to their default game bindings.

### How Each Tool Applies Priority

**Pattern A: ToolMenu PriorityScreen** -- tool reads `ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority()` at the moment of applying, sets it on the `Prioritizable` component of each affected object:
- DigTool, DeconstructTool, ClearTool, MopTool, HarvestTool, AttackTool, CaptureTool, EmptyPipeTool, PrioritizeTool

**Pattern B: Build menu PriorityScreen** -- BuildTool reads from `BuildMenu.Instance.GetBuildingPriority()` or `PlanScreen.Instance.GetBuildingPriority()`, which both delegate to `MaterialSelectionPanel.PriorityScreen`:
- BuildTool, BaseUtilityBuildTool (UtilityBuildTool, WireBuildTool)

Priority is applied at drag/place time, not stored on the tool. Each cell/entity gets whatever the relevant PriorityScreen returns at that moment.

## Tool Activation and Lifecycle

### PlayerController

Singleton that owns the `InterfaceTool[] tools` array and tracks `activeTool`.

- `tools[0]` is always SelectTool (the default)
- `ActivateTool(tool)`: deactivates old tool, enables new tool's GameObject, calls `tool.ActivateTool()`, updates hover
- `DeactivateTool()`: disables tool's GameObject, calls `tool.DeactivateTool(new_tool)`
- `ToolDeactivated(tool)`: fallback -- if no tool active after deactivation, activates SelectTool
- Input routing: forwards mouse/key events to `activeTool` after checking if cursor is over UI

### ToolMenu

Singleton KScreen organizing tools into `ToolCollection` groups displayed as toggles.

**Tool selection:**
- Pressing a hotkey selects the collection and activates its first tool
- Re-pressing the same hotkey deactivates the tool (returns to SelectTool)
- Escape deactivates the current tool
- Right-click (if not dragging) deactivates the current tool

**Overlay conflict:** If the player manually changes overlay while a tool with a specific viewMode is active, the tool is deactivated.

### InterfaceTool Lifecycle

**Activation** (`ActivateTool()`):
1. Calls `OnActivateTool()` (virtual, tool-specific setup)
2. Calls `OnMouseMove()` with current cursor position
3. Fires `Game.Instance.Trigger(1174281782, this)` -- the `ActiveToolChanged` event

**Deactivation** (`DeactivateTool(new_tool)`):
1. Calls `OnDeactivateTool(new_tool)` (virtual, tool-specific cleanup)
2. If reverting to SelectTool and an overlay was tool-activated, reverts overlay to None

### Per-Tool Overlay Switching

| Tool | Forced Overlay |
|------|---------------|
| DisinfectTool | Disease |
| HarvestTool | Harvest |
| PrioritizeTool | Priorities |
| SandboxHeatTool | Temperature |
| WireBuildTool | Power |
| BuildTool | Per-building (`def.ViewMode`) |
| BaseUtilityBuildTool | Per-building (`def.ViewMode`) |
| All others | None (no overlay change) |

The overlay switch is bidirectional: tool activation forces an overlay, and external overlay changes can deactivate a tool if they conflict with its viewMode.

## Audio Feedback

### Activation/Deactivation Sounds

| Event | Sound |
|-------|-------|
| Select tool from toolbar | `UISounds.Sound.ClickObject` |
| Deactivate tool (Escape/right-click/re-click) | `GetDeactivateSound()` |
| Default deactivate | `"Tile_Cancel"` |
| BuildTool/PlaceTool deactivate | `"HUD_Click_Deselect"` |

### Drag Sounds

Played during drag when area size changes, with `tileCount` FMOD parameter encoding the number of cells:

| Tool | Sound |
|------|-------|
| Default (most tools) | `"Tile_Drag"` |
| CancelTool, DisconnectTool | `"Tile_Drag_NegativeTool"` |
| Sandbox tools | Tool-specific |

### Confirm Sounds

Played on mouse release after a drag completes:

| Tool | Sound |
|------|-------|
| Default (most tools) | `"Tile_Confirm"` |
| CancelTool | `"Tile_Confirm_NegativeTool"` |
| DisconnectTool | `"OutletDisconnected"` |
| BuildTool | `"Place_Building_{AudioSize}"` with `tileCount` parameter |

### Other Sounds

| Event | Sound |
|-------|-------|
| Rotate building | `"HUD_Rotate"` |
| Priority change | `PlayPriorityConfirmSound()` with FMOD parameter encoding priority value |
| SelectTool click empty | `"Select_empty"` |
| SelectTool click entity | `"Select_full"` with `selection` cycle parameter |
| Sandbox toggle | `"SandboxTool_Toggle_On"` / `"SandboxTool_Toggle_Off"` |
| MoveToLocation success/failure | `"HUD_Click"` / `"Negative"` |

## Hover Text and Tooltips

Each tool has a `HoverTextConfiguration` component that draws a base hover card: tool name title, left-click action description, right-click "back" instruction, and mouse icons. Most tools also have a dedicated hover text card component that adds tool-specific per-cell information.

### SelectToolHoverTextCard

The most complex card. Shows different information based on the active overlay:

**Always shown (per hovered object):**
- Object name (with element material prefix for buildings, e.g., "GRANITE MANUAL GENERATOR")
- Status items (up to 10): warnings and status messages, with warning icons for bad/threatening statuses
- Temperature (from PrimaryElement, not shown during construction)

**Always shown (background cell/tile):**
- Element name (e.g., "OXYGEN", "SANDSTONE", "VACUUM")
- Disease (germ type + count, always shown in disease overlay, otherwise only if germs present)
- Material category (e.g., "Raw Mineral", not shown for vacuum)
- Mass (formatted with unit)
- Temperature (not shown for vacuum or zero specific heat)
- Exposed-to-Space indicator
- Buried/Entombed item indicator
- Off-gassing/sublimation info: emission rate, emitted element, whether blocked or over-pressure

**Overlay-specific sections:**
- **Temperature (HeatFlow mode):** Thermal comfort for a standard duplicant, formatted as DTU/s ("Neutral" / "Cooling" / "Heating")
- **Decor:** Total decor value, itemized positive providers (sorted highest first), light decor bonus, itemized negative providers
- **Room:** Room type name (or "No Room"), effect description, assignment, detail string (size, criteria), requirements met/unmet. If no room: reason why (cavity too big, etc.)
- **Light:** Light intensity (lux) + qualitative description ("Bright", "Dim")
- **Radiation:** Radiation level (rads) + qualitative description, shielding/absorption percentage
- **Logic:** Per-object logic port info: port name, building name, active/inactive state descriptions with signal highlighting, connection status
- **Disease:** Disease name + germ count per object (including stored items in Storage)

**Fog of war:** If the cell is not visible, shows warning icon + "Unknown" instead of all the above.

### DigToolHoverTextCard

- Element name (e.g., "SANDSTONE")
- Material category (e.g., "Raw Mineral")
- Mass (formatted with unit)
- Hardness string (e.g., "Very Soft", "Impenetrable" via `GameUtil.GetHardnessString`)

Only shown if the cell is visible and contains a diggable solid. Otherwise just tool title and instructions.

### BuildToolHoverTextCard

- Building name in tool title (e.g., "Build Manual Generator")
- Rotate hint if the building is rotatable (shows the rotate key binding)
- Placement validity error (the `fail_reason` string from BuildLocationRule)
- Room requirement error if the building needs a room but isn't in one
- Remaining resources string (how many more of this building you can afford)
- **In Logic overlay:** Logic port states for any ports at the hovered cell
- **In Power overlay:** Potential wattage consumed on the circuit (existing load + this building), colored red if it would exceed the safe limit

### MopToolHoverTextCard

- Liquid element name (e.g., "WATER")
- Material category (e.g., "Liquid")
- Mass (formatted with unit)

Only shown if the cell is visible and contains a liquid. Otherwise just tool title and instructions.

### AttackToolHoverTextCard

- Name of the first attackable creature under the cursor (proper name in uppercase)

No additional details (health, species, threat level).

### HarvestToolHoverTextCard

- Active filter name if a filter is enabled (not "ALL"), appended to tool title

No per-plant information (species, growth stage, yield).

### EmptyPipeToolHoverTextCard

- Active filter name if a filter is enabled (not "ALL"), appended to tool title

No pipe contents or conduit type information.

### PlaceToolHoverTextCard

- Placeable name in tool title (e.g., "Place Painting")
- Placement validity error if location is invalid (the reason string)

## Visual Feedback

### Area Visualizer (Box/Line Drag)

During drag in Box/Line mode:
- Single-cell cursor visualizer is hidden
- `areaVisualizer` sprite scales to cover the drag rectangle (default 50% white tint)
- Dimension text overlay shows `"{width} x {height} = {area}"`

### Tool Effect Display Plane

`ToolMenu.RefreshToolDisplayPlaneColor()` runs every frame while a non-SelectTool is active. Calls `activeTool.GetOverlayColorData()` to get per-cell color data and renders it onto a texture plane overlaying the game grid, showing which cells are affected.

### BuildTool Ghost

The build ghost/preview is a visualizer object that follows the cursor:
- **White** (valid placement)
- **Red** (invalid placement)
- Updated every mouse move via `BuildingDef.IsValidPlaceLocation()`
- `GridCompositor.ToggleMajor` enabled during build for grid visibility

### PopFX Feedback

Some tools spawn floating text popups for per-cell rejection:
- MopTool: "Too much liquid" or "Not on floor"
- CaptureTool: "Not capturable"
- BuildTool: "No material" when resources unavailable

## CopySettingsTool

Activated from an entity's user menu (not the toolbar), via the "Copy Building Settings" button (hotkey `Action.BuildingUtility1`).

1. Stores a `sourceGameObject` reference
2. Uses Box/Brush drag to sweep over target buildings
3. For each cell, finds the building on the same ObjectLayer as the source
4. Verifies `copyGroupTag` compatibility (buildings must be same type or same group)
5. Fires event `-905833192` on the target with the source as data
6. Individual building components subscribe to this event to copy their own settings
7. Shows PopFX with `UI.COPIED_SETTINGS` text

## Building Rotation

### Orientation Enum

| Value | Int | Meaning |
|-------|-----|---------|
| `Neutral` | 0 | Default, no rotation |
| `R90` | 1 | 90 degrees clockwise |
| `R180` | 2 | 180 degrees |
| `R270` | 3 | 270 degrees clockwise |
| `FlipH` | 5 | Horizontal flip (mirror left-right) |
| `FlipV` | 6 | Vertical flip (mirror top-bottom) |

Each orientation transforms a cell offset `(x, y)` via `Rotatable.GetRotatedCellOffset()`:

| Orientation | Transform |
|-------------|-----------|
| Neutral | `(x, y)` |
| R90 | `(y, -x)` |
| R180 | `(-x, -y)` |
| R270 | `(-y, x)` |
| FlipH | `(-x, y)` |
| FlipV | `(x, -y)` |

This affects the building's footprint, port positions (conduit I/O, power I/O), and foundation checks.

### PermittedRotations

Each building has a `PermittedRotations` value controlling which orientations it can cycle through:

| Value | Cycle | Orientations |
|-------|-------|-------------|
| `Unrotatable` | None | Only Neutral (most buildings default to this) |
| `R90` | Toggle | Neutral <-> R90 (e.g., doors: horizontal vs vertical) |
| `R360` | Full cycle | Neutral -> R90 -> R180 -> R270 -> back (e.g., auto-miner) |
| `FlipH` | Toggle | Neutral <-> FlipH (e.g., most machines: face left or right) |
| `FlipV` | Toggle | Neutral <-> FlipV (e.g., farm tile: floor or ceiling) |

**FlipH is the most common rotation type** -- used by Air Conditioner, Bottle Emptier, Canvas, Checkpoint, CO2 Scrubber, Desalinator, Flush Toilet, Gantry, and many decorative items.

### BuildTool Rotation Flow

1. Player presses O (`Action.RotateBuilding`)
2. `BuildTool.OnKeyDown()` calls `TryRotate()`
3. `TryRotate()` gets the `Rotatable` component on the visualizer, calls `Rotate()` which advances to the next orientation in the cycle
4. Stores result in `buildingOrientation`, refreshes visualization
5. On tool deactivation, `buildingOrientation` resets to `Orientation.Neutral`

Buildings with `PermittedRotations.Unrotatable` do not get a `Rotatable` component, so `CanRotate()` returns false and the O key does nothing.

### Key APIs

- `BuildTool.Instance.GetBuildingOrientation` -- current orientation
- `BuildTool.Instance.CanRotate()` -- whether rotation is possible
- `BuildTool.Instance.GetPermittedRotations()` -- nullable PermittedRotations
- `BuildTool.Instance.TryRotate()` -- advance to next orientation

## Utility Build Path Tracing

`BaseUtilityBuildTool` (parent of `UtilityBuildTool` and `WireBuildTool`) uses Brush mode to trace a path of connected pipe/wire segments. There is **no pathfinding or auto-routing** -- the path strictly follows the cursor cell by cell.

### Path Tracking During Drag

The path is stored as `List<PathNode>` where each node has `cell`, `valid` flag, and `visualizer` GameObject.

**`OnDragTool(cell, distFromOrigin)` logic:**
- **Duplicate cell:** Ignored (same as last cell in path)
- **Backtrack:** If the new cell matches the second-to-last cell, the last node is removed and its visualizer destroyed. This lets players "undo" by dragging backward.
- **New cell:** Appended if not already anywhere in the path (prevents loops). Validity checked via `CheckValidPathPiece()`.
- **Gap interpolation:** The base `DragTool.AddDragPoints()` interpolates at 0.25 cell intervals to avoid skipping cells when the mouse moves fast.

### Cell Validity

`CheckValidPathPiece(cell)` checks:
- For `NotInTiles` buildings: rejects cells with tiles or doors
- Rejects cells already occupied on the same ObjectLayer/TileLayer (unless the existing object has `KAnimGraphTileVisualizer`, meaning it is an existing utility segment that can be connected through)

### Connection Detection

As each cell is added, `CheckForConnection()` checks if the cell coincides with a building's input/output port:
- Logic wires: checks `LogicPorts` input/output cells
- Power wires: checks `Building.GetPowerInputCell()` / `GetPowerOutputCell()`
- Liquid/Gas pipes: checks `Building.GetUtilityInputCell()` / `GetUtilityOutputCell()`, plus `ElementFilter.GetFilteredCell()` for secondary outputs

Connection plays "OutletConnected" sound. Backtracking away from a connection plays "OutletDisconnected".

### Path Visualization

A `VisUpdater()` coroutine runs every frame while dragging:
1. Temporarily applies path connections to the conduit manager
2. Creates a preview sprite per node using `def.BuildingPreview`
3. Queries `conduitMgr.GetVisualizerString(cell)` for the correct connection animation (straight, corner, T-junction, cross)
4. Tints each node white (valid) or red (invalid)
5. Restores the conduit manager's original state

The single-cell cursor visualizer is hidden once the path has 2+ nodes. A text display shows path length during drag.

### What Happens on Release

`BuildPath()` iterates every node:
- **Empty cell:** Places a construction errand (or instant-builds in sandbox mode). Shows "no material" popup if resources insufficient.
- **Existing utility segment:** Merges new connections into the existing segment (this is how you draw over existing pipes to add branch connections).
- **Different material/type:** Places a replacement construction order on `def.ReplacementLayer`.
- Sets priority from `BuildMenu` or `PlanScreen`.

### UtilityBuildTool vs WireBuildTool

Both inherit from `BaseUtilityBuildTool`. The key difference is in `ApplyPathToConduitSystem()`:
- **UtilityBuildTool** (pipes/conveyors): Checks `conduitMgr.CanAddConnection()` before adding. Shows PopFX error if it fails.
- **WireBuildTool** (power/logic): Adds connections unconditionally (wire networks are less constrained).

Only cardinal-adjacent cells get logical connections. If the cursor jumps diagonally or skips a cell, that segment becomes a gap in the network even though visualizers exist for each cell.

## Key Source Files

| File | Purpose |
|------|---------|
| `InterfaceTool.cs` | Base class: cursor, hover, click/key handling, overlay switching |
| `DragTool.cs` | Drag mechanics: Box/Brush/Line modes, area visualizer, priority key interception |
| `FilteredDragTool.cs` | Layer filter infrastructure, overlay-aware auto-switching |
| `BrushTool.cs` | Circular brush with radius (sandbox tools) |
| `ToolMenu.cs` | Toolbar UI, tool collections, hotkey routing, tool selection |
| `ToolParameterMenu.cs` | Generic radio-button filter menu widget |
| `PlayerController.cs` | Tool array management, input routing, active tool tracking |
| `PriorityScreen.cs` | Priority UI (1-9 + emergency), PriorityClass enum |
| `PrioritySetting.cs` | Priority struct (class + value), comparison |
| `Prioritizable.cs` | Component on prioritizable entities, ref-counting, top-priority tracking |
| `BuildTool.cs` | Building placement, ghost preview, rotation, material checking |
| `BuildingDef.cs` | Build validation (IsValidPlaceLocation, IsValidBuildLocation, BuildLocationRule) |
| `CopySettingsTool.cs` | Settings copying between buildings |
| `HoverTextConfiguration.cs` | Tool hover card with name and instructions |
| `SelectToolHoverTextCard.cs` | Detailed per-cell/entity hover info for SelectTool |
| `BuildToolHoverTextCard.cs` | Placement errors, circuit info, resource remaining |
| `DigToolHoverTextCard.cs` | Element, mass, hardness info for dig targets |
| `BaseUtilityBuildTool.cs` | Path tracing for pipe/wire building |
| `Rotatable.cs` | Building rotation component, orientation transforms |
| `BuildLocationRule.cs` | Enum of 20 placement rules |

## Hookable Events

| Event Hash | Name | Fired By | Data |
|------------|------|----------|------|
| 1174281782 | ActiveToolChanged | `InterfaceTool.ActivateTool()` | The activated InterfaceTool |
| 2127324410 | (Cancel) | `CancelTool.OnDragTool()` | null |
| -905833192 | (CopySettings) | `CopyBuildingSettings.ApplyCopy()` | Source GameObject |
| -790448070 | (Deconstruct) | `DeconstructTool.OnDragTool()` | null |
