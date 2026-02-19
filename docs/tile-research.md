# OniAccess Tile System Research

## Purpose

This document captures our verified understanding of how tiles work in Oxygen Not Included, based on examination of the decompiled game source (`ONI-Decompiled/Assembly-CSharp/`). All claims are backed by specific code references unless marked with **[native sim]**, meaning the logic runs in native C++ simulation code not available in the C# decompilation.

---

## Grid Architecture

### Coordinate System

The game uses a single flat array indexed by cell integer. Coordinates use a bottom-left origin with Y increasing upward.

**Indexing** (`Grid.cs`):
- `cell = x + y * WidthInCells` (`Grid.XYToCell`)
- `x = cell % WidthInCells` (`Grid.CellColumn`)
- `y = cell / WidthInCells` (`Grid.CellRow`)
- Cell 0 is the bottom-left corner

**Navigation helpers** (`Grid.cs`):
- `Grid.CellAbove(cell)` adds `WidthInCells` (moves up)
- `Grid.CellBelow(cell)` subtracts `WidthInCells` (moves down)
- `Grid.CellRight(cell)` adds 1
- `Grid.CellLeft(cell)` subtracts 1
- `Grid.OffsetCell(cell, CellOffset)` for arbitrary offsets

### Map Dimensions

`Grid.WidthInCells` and `Grid.HeightInCells` are set at runtime from world generation config files, not hardcoded. `Grid.CellCount = WidthInCells * HeightInCells`. The actual default dimensions for each world type are defined in YAML config files outside the decompiled C# source. `Grid.CellSizeInMeters` defines cell size in world units.

Outer edges are filled with neutronium (`SimHashes.Unobtanium`) during world generation. The top 2 rows are reserved as a border (`Grid.TopBorderHeight = 2`), and buildings are excluded from these rows via `Grid.IsValidBuildingCell`.

### Multi-World (Spaced Out DLC)

All worlds share a **single unified grid**. Each cell has a world ID stored in `Grid.WorldIdx` (`byte[]`, one per cell). Worlds are managed by `ClusterManager` and represented as `WorldContainer` objects.

Each `WorldContainer` defines:
- `WorldOffset` (`Vector2I`): position of this world within the unified grid
- `WorldSize` (`Vector2I`): width and height of the world
- `minimumBounds` / `maximumBounds` (`Vector2`): playable coordinate range
- `HiddenYOffset` (`int`): hidden rows at bottom (used for rocket interiors)

Rocket interiors dynamically allocate grid space via `Grid.GetFreeGridSpace()` and register cells to `Grid.WorldIdx` with unique world IDs. This space is freed when the rocket is destroyed.

**Playable bounds**: Use `WorldContainer.minimumBounds` and `WorldContainer.maximumBounds`, minus `Grid.TopBorderHeight` for the top border. Neutronium borders are regular cells filled with the Unobtanium element, not a grid-level concept.

---

## Per-Cell Data Storage

All per-cell data is stored in **parallel arrays** indexed by cell integer. There is no struct-per-cell. All core arrays use unsafe pointers for performance (`Grid.cs`).

### Core Arrays

| Array | Type | Description |
|---|---|---|
| `Grid.elementIdx` | `ushort*` | Index into `ElementLoader.elements` |
| `Grid.mass` | `float*` | Mass in kilograms |
| `Grid.temperature` | `float*` | Temperature in **Kelvin** |
| `Grid.radiation` | `float*` | Radiation level (Spaced Out DLC) |
| `Grid.properties` | `byte*` | Bitflags (gas/liquid impermeable, transparent, etc.) |
| `Grid.strengthInfo` | `byte*` | Structural integrity |
| `Grid.insulation` | `byte*` | Thermal insulation (0-255) |
| `Grid.diseaseIdx` | `byte*` | Disease type index (255 = no disease) |
| `Grid.diseaseCount` | `int*` | Disease population count |
| `Grid.exposedToSunlight` | `byte*` | Sunlight exposure (0-255) |
| `Grid.WorldIdx` | `byte[]` | World ID per cell (255 = invalid) |
| `Grid.Visible` | `byte[]` | Fog of war visibility |
| `Grid.Damage` | `float[]` | Terrain damage |
| `Grid.Decor` | `float[]` | Cached decor value |
| `Grid.BuildMasks` | `BuildFlags[]` | Passability bitmask per cell |
| `Grid.ObjectLayers` | `Dictionary<int, GameObject>[]` | Per-layer object tracking |

### Typed Accessors

The `Grid` class provides indexer structs for safe access:
- `Grid.Element[cell]` returns `Element` object (via `ElementLoader.elements[elementIdx[cell]]`)
- `Grid.ElementIdx[cell]` returns raw `ushort` index
- `Grid.Mass[cell]` returns `float` kg
- `Grid.Temperature[cell]` returns `float` Kelvin
- `Grid.Pressure[cell]` returns `mass[cell] * 101.3f` (computed, not stored)
- `Grid.Radiation[cell]` returns `float`
- `Grid.DiseaseIdx[cell]` returns `byte` (index into `Db.Get().Diseases`, 255 = none)
- `Grid.DiseaseCount[cell]` returns `int`
- `Grid.Objects[cell, layer]` returns `GameObject` at that cell in the specified `ObjectLayer`

### Vacuum Representation

Vacuum is an explicit element with ID `SimHashes.Vacuum`. Detection: `Grid.Element[cell].IsVacuum` or `Grid.Mass[cell] == 0f`. Outer space uses `SimHashes.Void`. Both have `Element.State.Vacuum`.

### Temperature Units

Temperature is stored and computed in **Kelvin** internally. Display conversion happens only in `GameUtil.cs`:
- Celsius: `kelvin - 273.15f`
- Fahrenheit: `kelvin * 1.8f - 459.67f`

### Germs

Germ data is stored in the main grid arrays (`diseaseIdx` and `diseaseCount`), not a separate system. There are **five** disease types registered in `Database/Diseases.cs`:

| Field | Common Name |
|---|---|
| `FoodGerms` | Food poisoning |
| `SlimeGerms` | Slimelung |
| `PollenGerms` | Floral scents |
| `ZombieSpores` | Zombie spores |
| `RadiationPoisoning` | Radiation sickness (Spaced Out DLC, gated by `DlcManager.FeatureRadiationEnabled()`) |

### Radiation

Radiation is stored in the same per-cell array system (`Grid.radiation`, `float*`), not a separate system. It is part of the Spaced Out DLC.

---

## Element Behavior

### Density Sorting

Every element has a `molarMass` field (`Element.cs`, line 37) that determines sorting weight. Higher `molarMass` = denser = sinks. The C# code confirms this in `FallingWater.cs` (lines 444-447), where liquids compare `element2.molarMass > element.molarMass` to determine displacement.

The actual cell-swapping logic that settles elements vertically runs in **[native sim]** code. The C# side provides the data and parameters; the native simulation performs the per-tick element displacement.

Other element mass fields (not to be confused with `molarMass`):
- `Element.maxMass`: Maximum mass a cell of this element can hold
- `Element.defaultValues.mass`: Default creation mass per cell
- `Grid.Mass[cell]`: Actual current mass in a specific cell

### Phase Separation

Elements have a `State` enum (`Element.cs`): `Vacuum=0, Gas=1, Liquid=2, Solid=3`. The behavior that gases sit above liquids regardless of `molarMass` is consistent with all observable C# code (e.g., `FallingWater.cs` only processes liquid falling, `CO2Manager.cs` only processes gas settling). However, the actual phase-priority sorting logic runs in **[native sim]** code.

Element also defines `minHorizontalFlow` and `minVerticalFlow` fields (both initialized to `float.PositiveInfinity`), suggesting the native sim distinguishes horizontal and vertical flow thresholds. The claim that horizontal sorting does not occur is consistent with these defaults but cannot be directly confirmed from C# code alone **[native sim]**.

## Duplicant Occupancy

(`BaseMinionConfig.cs`): Duplicants occupy **two cells** via `MoverLayerOccupier.cellOffsets`:
- `CellOffset(0, 0)`: Body/feet cell
- `CellOffset(0, 1)`: Head cell (one cell above body)

Both cells are registered in `Grid.Objects` under `ObjectLayer.Minion` AND `ObjectLayer.Mover`, pointing to the same duplicant GameObject. `MoverLayerOccupier.Sim200ms()` refreshes these entries every 200ms.

**Duplicants also register on `ObjectLayer.Pickupables`** via their `Pickupable` component (added unconditionally in `BaseMinionConfig`, line 343). `Pickupable.OnSpawn()` calls `OnTagsChanged()` → `UpdateListeners(worldSpace: true)`, which creates an `ObjectLayerListItem` on `ObjectLayer.Pickupables` at the body cell. The `Health` component check in `OnSpawn()` only suppresses faller physics, not layer registration. This means a live duplicant appears on **three layers** at the body cell: `Minion`, `Mover`, and `Pickupables`. Any code traversing `ObjectLayer.Pickupables` must filter out duplicants (check `MinionIdentity` component) to avoid double-reporting.

---

## Building Layers

### Object Layer System

Buildings are tracked via `Grid.Objects[cell, layer]`, which indexes into `Grid.ObjectLayers` — an array of `Dictionary<int, GameObject>`, one dictionary per `ObjectLayer` enum value.

The `ObjectLayer` enum (`ObjectLayer.cs`) defines **49 layers**. Key building-related layers:

| Layer | Purpose |
|---|---|
| `ObjectLayer.Building` | Foreground buildings (machines, furniture, generators) |
| `ObjectLayer.Backwall` | Background buildings (drywall, tempshift plates) |
| `ObjectLayer.FoundationTile` | Constructed tiles, insulated tiles |
| `ObjectLayer.Minion` | Duplicants (via MoverLayerOccupier) |
| `ObjectLayer.Mover` | Duplicants and rovers (via MoverLayerOccupier) |
| `ObjectLayer.Critter` | **Dead code** — nothing writes to this layer |
| `ObjectLayer.Plants` | Plants |
| `ObjectLayer.Pickupables` | Debris, critters, AND duplicants share this layer |
| `ObjectLayer.GasConduit` | Gas pipe buildings |
| `ObjectLayer.GasConduitTile` | Gas pipe tile pieces |
| `ObjectLayer.LiquidConduit` | Liquid pipe buildings |
| `ObjectLayer.LiquidConduitTile` | Liquid pipe tile pieces |
| `ObjectLayer.SolidConduit` | Conveyor rail buildings |
| `ObjectLayer.SolidConduitTile` | Conveyor rail tile pieces |
| `ObjectLayer.Wire` | Electrical wire buildings |
| `ObjectLayer.WireTile` | Electrical wire tile pieces |
| `ObjectLayer.LogicGate` | Automation gate buildings |
| `ObjectLayer.LogicWire` | Automation wire |
| `ObjectLayer.TravelTube` | Transit tube buildings |
| `ObjectLayer.TravelTubeTile` | Transit tube tile pieces |

Each cell can have one object per layer. Different layers are independent, so a cell can simultaneously have a foreground building, a background building, pipes, wires, and automation.

### Passability

Passability is determined by **`Grid.BuildMasks`**, a `BuildFlags[]` bitmask array with one entry per cell. Buildings set and clear these flags when they spawn and despawn.

```
BuildFlags (byte):
  Solid            = 0x01  — blocks all movement
  Foundation       = 0x02  — provides floor support
  Door             = 0x04  — door present
  DupePassable     = 0x08  — duplicants can traverse
  DupeImpassable   = 0x10  — duplicants cannot traverse
  CritterImpassable = 0x20 — critters cannot traverse
  FakeFloor        = 0xC0  — counts as floor (mesh tiles, etc.)
```

Queried via: `Grid.Solid[cell]`, `Grid.DupeImpassable[cell]`, `Grid.CritterImpassable[cell]`, `Grid.HasDoor[cell]`.

### Multi-Tile Buildings

Multi-tile buildings use `BuildingDef.PlacementOffsets` (`CellOffset[]`) to define which cells they occupy relative to their anchor cell. The `Building` class computes actual occupied cells at runtime in `Building.PlacementCells`, accounting for rotation via `Rotatable.GetRotatedCellOffset`.

**The same `GameObject` reference** is stored in `Grid.Objects` for every cell the building occupies. All occupied cells point to the same Building instance.

### Element Under Buildings

Buildings and elements occupy **completely independent data structures**. The element arrays (`Grid.elementIdx`, `Grid.mass`, `Grid.temperature`, etc.) persist regardless of what buildings are present. A cell with a coal generator still has gas element data (the gas exists "behind" the building). Buildings may affect gas/liquid flow through passability flags, but they do not erase or replace the cell's element data.

### Heavy-Watt Wire

Heavy-watt wire configs (`WireHighWattageConfig.cs`) use `BuildLocationRule.NotInTiles`, which prevents placement in cells containing constructed tiles or doors. Standard wire (`BaseWireConfig.cs`) uses `BuildLocationRule.Anywhere`.

### Drywall

Drywall (`ExteriorWallConfig.cs`) also uses `BuildLocationRule.NotInTiles` and occupies `ObjectLayer.Backwall`. It cannot coexist with constructed tiles or doors in the foreground tile layer.

### Building Input/Output Ports

Buildings flag their utility connection points (pipe inputs/outputs, power, logic, radbolts) through **CellOffset fields** on `BuildingDef` and component-level interfaces. All offsets are relative to the building's anchor cell and rotation-adjusted at runtime via `Building.GetRotatedOffset()`.

**Primary port fields** (`BuildingDef.cs`, lines 55-172):

| Field | Type | Default | Purpose |
|---|---|---|---|
| `InputConduitType` | `ConduitType` | None | Which conduit layer the input uses (Gas, Liquid, Solid) |
| `OutputConduitType` | `ConduitType` | None | Which conduit layer the output uses |
| `UtilityInputOffset` | `CellOffset` | (0, 1) | Primary conduit input position |
| `UtilityOutputOffset` | `CellOffset` | (1, 0) | Primary conduit output position |
| `PowerInputOffset` | `CellOffset` | | Electrical input position |
| `PowerOutputOffset` | `CellOffset` | | Electrical output position |
| `HighEnergyParticleInputOffset` | `CellOffset` | | Radbolt input position |
| `HighEnergyParticleOutputOffset` | `CellOffset` | | Radbolt output position |
| `LogicInputPorts` | `List<LogicPorts.Port>` | | Logic input port definitions |
| `LogicOutputPorts` | `List<LogicPorts.Port>` | | Logic output port definitions |

**Logic port definition** (`LogicPorts.cs`, lines 12-62): Each `LogicPorts.Port` struct contains `id` (HashedString), `cellOffset`, `description`, `activeDescription`, `inactiveDescription`, `requiresConnection`, and `spriteType` (Input or Output). Factory methods `Port.InputPort()` and `Port.OutputPort()` construct these.

**Secondary ports** for buildings with multiple conduit connections: Components implement `ISecondaryInput` / `ISecondaryOutput` interfaces (`ISecondaryInput.cs`, `ISecondaryOutput.cs`), each providing `HasSecondaryConduitType(ConduitType)` and `GetSecondaryConduitOffset(ConduitType)`. The concrete implementations `ConduitSecondaryInput` / `ConduitSecondaryOutput` hold a `ConduitPortInfo` struct (`ConduitPortInfo.cs`) with `conduitType` + `offset` fields. `ConduitConsumer` and `ConduitDispenser` check for these interfaces when `useSecondaryInput` / `useSecondaryOutput` flags are set.

**Runtime query methods** (`Building.cs`, lines 262-316):
```csharp
building.GetUtilityInputCell()               // Absolute cell for primary conduit input
building.GetUtilityOutputCell()              // Absolute cell for primary conduit output
building.GetPowerInputCell()                 // Absolute cell for power input
building.GetPowerOutputCell()                // Absolute cell for power output
building.GetHighEnergyParticleInputCell()    // Absolute cell for radbolt input
building.GetHighEnergyParticleOutputCell()   // Absolute cell for radbolt output
```

All methods apply rotation via `GetRotatedOffset()` then resolve to absolute cells with `Grid.OffsetCell(GetBottomLeftCell(), rotatedOffset)`.

**Logic port runtime queries** (`LogicPorts.cs`, lines 403-424):
- `LogicPorts.GetPortCell(HashedString portId)` returns the absolute cell for a named logic port
- `LogicPorts.TryGetPortAtCell(int cell, out Port port, out bool isInput)` reverse-lookups whether a given cell is a logic port

**Example** (`CO2ScrubberConfig.cs`): A 2x2 building with liquid input at bottom-left, liquid output at top-right, and a logic input at bottom-right:
```csharp
obj.InputConduitType = ConduitType.Liquid;
obj.OutputConduitType = ConduitType.Liquid;
obj.UtilityInputOffset = new CellOffset(0, 0);
obj.UtilityOutputOffset = new CellOffset(1, 1);
obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 0));
```

**Multi-port example** (`HabitatModuleSmallConfig.cs`): Rocket modules use multiple secondary ports for different conduit types, each defined as a `ConduitPortInfo` and attached via `ConduitSecondaryInput` / `ConduitSecondaryOutput` components.

### Work Cells (Duplicant Interaction Points)

Every `Workable` defines which cells a duplicant can stand at to operate the building. These are stored as a `CellOffset[]` array managed by an `OffsetTracker`.

**Setting work cells** (`Workable.cs`, lines 590-629):
- `Workable.SetOffsets(CellOffset[] offsets)` stores offsets in a `StandardOffsetTracker`
- `Workable.GetOffsets(int cell)` / `GetOffsets()` returns the offset array
- `Workable` implements `IApproachable`, which exposes `GetCell()` (building's base cell) and `GetOffsets()` (valid approach cells)

**Navigation flow**: When a duplicant is assigned a work errand, the `Navigator` iterates all offset cells, calculates pathfinding cost to each, and picks the cheapest reachable one (`Navigator.cs`, lines 878-892):
```csharp
public int GetNavigationCost(int cell, CellOffset[] offsets)
{
    int num = -1;
    for (int i = 0; i < offsets.Length; i++)
    {
        int cell2 = Grid.OffsetCell(cell, offsets[i]);
        int navigationCost = GetNavigationCost(cell2);
        if (navigationCost != -1 && (num == -1 || navigationCost < num))
            num = navigationCost;
    }
    return num;
}
```

The `CanMoveTo` chore precondition (`ChorePreconditions.cs`, lines 356-380) gates work errands on reachability of at least one offset cell.

**Default offsets**: Buildings that don't call `SetOffsets()` use `OffsetGroups.Use`, a standard group. Many buildings explicitly set a single offset, e.g.:
```csharp
// Bottler.cs, IceKettleWorkable.cs
public CellOffset workCellOffset = new CellOffset(0, 0);
SetOffsets(new CellOffset[1] { workCellOffset });
```

**State machine integration** (`GameStateMachine.cs`, lines 2690-2709): `ApproachSubState` drives the duplicant to the target workable's offset cells, with optional `override_offsets` for dynamic repositioning.

---

## Utility Layers

### Storage Architecture

Each utility type uses its own `ObjectLayer` entries (one for buildings, one for tile pieces) in `Grid.ObjectLayers`. Additionally, each conduit type maintains its own flow simulation via `ConduitFlow`.

**ConduitFlow** (`ConduitFlow.cs`) stores a per-cell `GridNode` array:
```
GridNode:
  conduitIdx     — index into conduit list (-1 = no conduit)
  contents       — ConduitContents struct
```

**ConduitContents** per pipe segment:
```
ConduitContents:
  element        — SimHashes element type
  temperature    — float (Kelvin)
  mass           — float (kg), computed from initial/added/removed
  diseaseIdx     — byte (disease type)
  diseaseCount   — int (germ count)
```

Flow direction is tracked per conduit in `SOAInfo.srcFlowDirections`.

**Utility networks** (`UtilityNetworkManager.cs`) maintain separate `physicalGrid` and `visualGrid` arrays (`UtilityNetworkGridNode[]`), both sized `width * height`. All utility layers share the main grid coordinate system.

There is no unified "what utilities are at this cell" query. Each layer must be checked independently via `Grid.Objects[cell, layer]` for the relevant `ObjectLayer`.

### Bridges

Bridges are **separate building objects** (not flags on pipe segments). Each conduit type has its own bridge class:
- `ConduitBridge` (gas/liquid pipes) — inherits `ConduitBridgeBase`, implements `IBridgedNetworkItem`
- `SolidConduitBridge` (conveyor rails)

Each bridge has `inputCell` and `outputCell` fields, registers with the conduit flow manager, and handles mass transfer between its two connected cells each simulation tick.

---

## Entities

### Per-Cell Entity Tracking

Entities are tracked per-cell via `Grid.Objects[cell, layer]`:
- `ObjectLayer.Minion` for duplicants (direct slot via `MoverLayerOccupier`, not a linked list)
- `ObjectLayer.Plants` for plants (direct slot)
- `ObjectLayer.Pickupables` for debris, critters, AND duplicants (shared linked list)

`ObjectLayer.Critter` exists in the enum but is **dead code** — nothing writes to it.

When **multiple pickupable entities** occupy one cell, they form a **per-cell linked list** via `ObjectLayerListItem`. The head of the list is stored in `Grid.Objects[cell, ObjectLayer.Pickupables]`. Traverse with `objectLayerListItem.nextItem` (accessed via `Pickupable.objectLayerListItem`). Filter by component to distinguish entity types: `MinionIdentity` = duplicant, `CreatureBrain` = critter, otherwise = debris.

### Duplicants

Duplicants occupy two cells (body + head) as described above. Both cells register the same duplicant in `Grid.Objects[cell, ObjectLayer.Minion]`. Position is tracked via `Transform.GetPosition()` and converted to cell via `Grid.PosToCell`. The `Navigator` component caches the current cell in `cachedCell`.

### Critters

**`ObjectLayer.Critter` is dead code.** The enum value exists (`ObjectLayer.cs`, line 45) but nothing in the game ever writes to it. No critter config adds `MoverLayerOccupier` or otherwise registers on `ObjectLayer.Critter`.

Critters register **only on `ObjectLayer.Pickupables`** via their `Pickupable` component (added in `EntityTemplates.ExtendEntityToBasicCreature`, line 415). Multiple critters at one cell are linked via `ObjectLayerListItem` on the Pickupables layer. To find critters specifically, check for `CreatureBrain` component on items in the Pickupables linked list.

### Plants

Plants are **standalone entities**, not part of tile data. They:
- Register in `Grid.Objects[cell, ObjectLayer.Plants]`
- Track their area via `OccupyArea` component
- Register with `GameScenePartitioner.Instance.plants` for spatial queries
- Are listed globally in `Components.Uprootables`
- Growth is managed by the `Growing` state machine component

### Debris

Debris items (`Pickupable` component) are tracked **both globally and per-cell**:
- **Global list**: `Components.Pickupables`
- **Per-cell**: `Grid.Objects[cell, ObjectLayer.Pickupables]` with linked list via `ObjectLayerListItem`
- **Spatial partitioner**: Registered in `GameScenePartitioner.Instance.pickupablesLayer`
- **Cell tracking**: `Pickupable.cachedCell` is kept up-to-date via `OnCellChange()`

Each debris item carries its own element, mass, temperature, and germ data via its `PrimaryElement` component.

---

## Computed Properties

### Decor

Decor has **both a per-cell cache and live computation**:
- `Grid.Decor[cell]` (`float[]`) stores the cached decor value per cell
- Each `DecorProvider` component defines `baseDecor` (value) and `baseRadius` (range)
- `DecorProvider.AddDecor()` accumulates contributions to `Grid.Decor` for affected cells
- For hover display, `GameScenePartitioner` does a spatial query to find all `DecorProvider` objects affecting a cell, then sums their `GetDecorForCell(cell)` contributions
- `DecorMonitor` on duplicants queries `GameUtil.GetDecorAtCell(cell)` every tick for morale calculations

### Light Level

Light is computed per-emitter and accumulated into a per-cell array:
- `Grid.LightCount[cell]` stores accumulated lux per cell
- `Grid.LightIntensity[cell]` stores display intensity
- Each `LightGridManager.LightGridEmitter` defines origin, shape, radius, intensity, and falloff rate
- Emitters compute visible cells via `DiscreteShadowCaster.GetVisibleCells` (raycasted visibility)
- Lux per cell is computed as `intensity / falloff(distance)` and accumulated into the grid array

### Room Assignment

Room assignment is maintained **persistently per cell**, not just when the overlay is active:
- `RoomProber.CellCavityID[cell]` (`HandleVector<int>.Handle[]`, one per cell) provides O(1) lookup
- `RoomProber.GetCavityForCell(cell)` returns a `CavityInfo`
- `CavityInfo` contains: list of cells, bounding box, `Room room` reference (null if not a valid room), lists of buildings/plants/creatures within
- `Room` contains: `CavityInfo cavity`, `RoomType roomType`
- `RoomType` defines criteria, effects, and bonuses

---

## UI Systems

### Hover Tooltip

The hover tooltip is rendered by `SelectToolHoverTextCard.UpdateHoverElements()` using `HoverTextDrawer`.

**For tiles with buildings/objects:**
- Object name via `GameUtil.GetUnitFormattedName()`
- For buildings with materials: formatted as `"{Name} ({Element})"`
- Status items (warnings, disease, temperature)
- Temperature value
- Overlay-specific details when an overlay is active

**For raw tiles (no building):**
- Element name in uppercase (`element.nameUpperCase`)
- Disease info if present (`Grid.DiseaseIdx[cell]`, `Grid.DiseaseCount[cell]`)
- Material category (`ElementLoader.elements[Grid.ElementIdx[cell]].GetMaterialCategoryTag()`)
- Mass (formatted via `HoverTextHelper.MassStringsReadOnly(cell)`)
- Temperature (`Grid.Temperature[cell]`)
- Space exposure status (`CellSelectionObject.IsExposedToSpace(cell)`)
- Buried items (`EntombedItemVisualizer.IsEntombedItem(cell)`)
- Sublimation info if applicable

### Overlay-Specific Tooltip Changes

When an overlay is active, the hover tooltip **changes content**. `SelectToolHoverTextCard` maintains a `modeFilters` dictionary that maps overlay IDs to filter functions determining what objects to show. Overlay-specific tooltip additions:

| Overlay | Additional Tooltip Content |
|---|---|
| Decor | Decor effectors (positive/negative contributors), total decor value |
| Disease | Disease status with special formatting |
| Logic | Port states and signal values |
| Rooms | Room type, effects, criteria |
| Light | Light intensity with description |
| Temperature | Thermal comfort calculations |
| Radiation | Rads and shielding info |

### Click Inspect Panel

Clicking a tile calls `SelectTool.OnLeftClickDown()`, which uses `GetObjectUnderCursor(cycleSelection: true)` to pick a selectable object, then calls `Select()` to open the `DetailsScreen`.

**For raw tiles** (no selectable object): A `CellSelectionObject` is used, which stores:
- `element`, `state`, `Mass`, `temperature`, `diseaseIdx`, `diseaseCount`
- Updated every 0.5 seconds via `UpdateValues()` from `Grid` arrays

The `DetailsScreen` shows tabs: Config, Errands, Material, Blueprints. Content depends on the selected object type. Side screens show component-specific UI panels. `AdditionalDetailsPanel.RefreshDetailsPanel()` handles raw tile display (element, mass, temperature, disease, age).

### Selection Cycling

When multiple selectable objects overlap at the cursor position, **repeated clicks cycle through them**. The `InterfaceTool.GetObjectUnderCursor` method maintains `hitCycleCount`, which increments modulo the number of intersections on each click. There is **no Tab key for cycling** — only repeated left clicks.

The intersection list is sorted via `SortSelectables()` to determine priority order. Sound feedback varies based on cycle position.

### Overlay System Architecture

The game registers **20 overlay modes** in `OverlayScreen.RegisterModes()` (19 gameplay overlays + None). A `Sound` overlay mode class exists in `OverlayModes.cs` but is **not registered** and not player-accessible. Overlay activation is via `OverlayScreen.ToggleOverlay(HashedString newMode)`. Keyboard shortcut bindings are defined in input config files outside the decompiled C# source.

All overlay modes inherit from a base `Mode` class (`OverlayModes.cs`). The system uses two independent mechanisms:

1. **Object layer masking**: Objects are moved to Unity layers `"MaskedOverlay"` or `"MaskedOverlayBG"`, and the camera's culling mask is toggled to show/hide these layers. Only objects explicitly moved by the overlay mode become visible on the overlay layer. Objects not targeted by the mode remain on their `KPrefabID.defaultLayer`.

2. **SimDebugView cell coloring**: The `SimDebugView` system recolors grid cells based on the active `SimViewMode` (e.g., oxygen density, temperature, radiation). This is independent of the object layer system.

When an overlay activates, it typically:
- Enables camera culling for `MaskedOverlay` / `MaskedOverlayBG`
- Moves targeted objects to those layers (with tint/highlight colors)
- Sets a `SimViewMode` that changes how grid cells render
- Updates `SelectTool` layer mask so the player can click overlay objects
- Optionally toggles `GridCompositor.ToggleMinor()` for grid line visibility

When an overlay deactivates, `ResetDisplayValues()` restores each affected object: layer reset to `KPrefabID.defaultLayer`, tint reset to `Color.white`, highlight cleared.

**Objects not targeted by the active overlay mode remain on their default Unity rendering layer.** This means buildings, duplicants, critters, debris, and plants all continue to render normally in the default camera view. The overlay adds its targeted objects on top. The visual effect is that non-targeted objects appear dimmed/grayed because the SimDebugView cell coloring darkens the background.

### Overlay Mode Details

Overlay modes fall into three categories:

**Grid-only modes** set a SimViewMode to color cells but do not manipulate any objects. All default-layer objects (buildings, duplicants, critters, debris, plants) continue rendering normally:

| Mode | ID | SimViewMode | Cell Coloring | Notes |
|---|---|---|---|---|
| None | (invalid) | Normal | Normal rendering | Default view, no overlay |
| Oxygen | "Oxygen" | Oxygen map | Cells colored by O2 density | Adds MaskedOverlay to SelectTool mask |
| Light | "Light" | Light map | Cells colored by lux intensity | Minimal implementation |
| Thermal Conductivity | "ThermalConductivity" | TC map | Cells colored by conductivity | Minimal implementation |
| Priorities | "Priorities" | Priority map | Cells colored by work priority | Stub: no custom Enable/Disable/Update |
| Heat Flow | "HeatFlow" | Heat flow map | Cells colored by heat direction | Stub: no custom Enable/Disable/Update |
| Rooms | "Rooms" | Room map | Cells colored by room type | Legend: all RoomTypes from `Db.Get().RoomTypes` |
| Radiation | "Radiation" | Radiation map | Cells colored by rads | Only custom behavior is audio ambience toggle |

**Temperature mode** is grid-based but has additional complexity:

| Mode | ID | Cell Coloring | Special Features |
|---|---|---|---|
| Temperature | "Temperature" | Post-processing shader + cell coloring | 5 sub-modes: AbsoluteTemperature, RelativeTemperature, HeatFlow, AdaptiveTemperature, StateChange. Uses `Infrared.Instance.SetMode()` and `CameraController.ToggleColouredOverlayView()`. Legend shows 8 temperature ranges (MAXHOT through EXTREMECOLD) plus heat source/sink icons. |

**Object-targeting modes** move specific buildings/entities to overlay layers and apply tinting. Only targeted objects appear on the overlay layer; everything else stays on its default layer:

| Mode | ID | Targeted Objects | Visual Effects |
|---|---|---|---|
| Power | "Power" | Buildings in `OverlayScreen.WireIDs` (wires, generators, consumers, batteries, transformers) | Wires colored by circuit state: unpowered, overloading (> max safe watts), straining (> 75% capacity), safe. Power labels show watts generated (+) or consumed (-). Connected networks highlight on hover. Enables minor grid lines. |
| Liquid Conduits | "LiquidConduit" | Buildings in `OverlayScreen.LiquidVentIDs` (liquid pipes, vents, bridges, pumps) | Pipes tinted by thermal conductivity: normal, insulated (low TC), radiant (high TC). Connected networks highlight on hover. Buildings with `GameTags.OverlayInFrontOfConduits` / `OverlayBehindConduits` adjust Z-depth. |
| Gas Conduits | "GasConduit" | Buildings in `OverlayScreen.GasVentIDs` (gas pipes, vents, bridges, pumps) | Same behavior as Liquid Conduits but for gas infrastructure. |
| Solid Conveyor | "SolidConveyor" | Buildings in `OverlayScreen.SolidConveyorIDs` (rails, loaders, chutes) | Gray tint `(0.65, 0.65, 0.65)`. Connected network highlighting on hover. |
| Logic | "Logic" | Buildings in `Logic.HighlightItemIDs` (gates, sensors, wires, ribbon cables, bridges) | Wires colored by signal state: logicOn (active, bit 0 set) / logicOff (inactive). 4-bit ribbon cables color each bit independently. UI port icons placed at I/O locations (Input, Output, Reset, RibbonInput, RibbonOutput, ControlInput). Connected circuit highlights on hover. **Enables minor grid lines** (unique among non-Power overlays). |
| Suit | "Suit" | Buildings in `OverlayScreen.SuitIDs` (suit lockers, docks, checkpoints) | UI overlay widgets at each suit/locker showing availability status. White tint (visibility emphasis only). |
| Decor | "Decor" | All buildings with `DecorProvider` component, **excluding**: Tile, SnowTile, WoodTile, MeshTile, InsulationTile, GasPermeableMembrane, CarpetTile, gas/liquid conduits | **Hover-interactive**: objects only highlight when in `SelectToolHoverTextCard.highlightedObjects`. Positive decor: `decorHighlightPositive` color. Negative decor: `decorHighlightNegative` color. Legend: HIGH DECOR / LOW DECOR. |
| Disease | "Disease" | All duplicants (`Components.LiveMinionIdentities`) + optionally gas/liquid conduits (filter toggleable) | Uses `Infrared.Mode.Disease` for grid coloring. `CameraController.ToggleColouredOverlayView(true)` for shader overlay. UI widgets on duplicants showing immune level. Legend: one entry per disease type with custom overlay color. Filters: ALL (on), LIQUIDCONDUIT (off), GASCONDUIT (off). |
| Crop | "Crop" | All `HarvestDesignatable` plants (from `OverlayScreen.HarvestableIDs`) | Three-state coloring: Wilting (`cropHalted`), Growing (`cropGrowing`), Fully Grown (`cropGrown`). UI icon (`HarvestableOverlayWidget`) above each plant. Legend: FULLY GROWN / GROWING / GROWTH HALTED. |
| Harvest | "HarvestWhenReady" | All `HarvestDesignatable` plants (from `OverlayScreen.HarvestableIDs`) | Uniform gray highlight `(0.65, 0.65, 0.65, 0.65)` on all harvestable plants regardless of state. No legend entries. |
| Tile Mode | "TileMode" | All objects with `PrimaryElement` component (any material-bearing object) | Colors each object by `Element.substance.uiColour` (material's UI color). Filterable by material category: Metal, BuildableRaw, BuildableProcessed, Filter, ConsumableOre, Organics, Farmable, Liquifiable, Gas, Liquid, Misc. The most complex filter system of any overlay. |

---

## Camera System

### CameraController

The camera is managed by `CameraController`, a singleton accessible via `CameraController.Instance`. It controls a Unity orthographic camera with a fixed Z position of `-100f`.

**Key positioning methods:**

| Method | Behavior |
|---|---|
| `SnapTo(Vector3 pos)` | Immediate jump, clears follow target, no animation |
| `SnapTo(Vector3 pos, float orthoSize)` | Immediate jump with zoom level |
| `SetTargetPos(Vector3 pos, float orthoSize, bool playSound)` | Smooth animated pan. Handles cross-world transitions (star wipe). |
| `CameraGoTo(Vector3 pos, float speed, bool playSound)` | Helper: calls `SetTargetPos` with ortho=10 |
| `SetPosition(Vector3 pos)` | Direct transform update, no follow target clear |
| `SetFollowTarget(Transform target)` | Tracks an entity, auto-zooms to ortho=6 |
| `ClearFollowTarget()` | Stops following, resumes manual control |

`GameUtil.FocusCamera(Vector3 position)` is a convenience wrapper around `CameraGoTo`.

### Zoom

Zoom is controlled via `OrthographicSize` (the camera's orthographic size in world units). Smaller = more zoomed in.

- Default max: `20f` (`maxOrthographicSize`)
- Follow mode: `6f`
- Zoom input multiplies/divides `targetOrthographicSize` by `zoomFactor` each step
- Zoom eases smoothly toward `targetOrthographicSize` each frame

### Visible Area

`CameraController.VisibleArea` (`GridVisibleArea`) tracks which cells are on screen. Updated every frame.

- `GridVisibleArea.GetVisibleAreaExtended(padding)` returns a `GridArea` struct with min/max cell coordinates, computed from viewport corners converted to world coordinates, clamped to world bounds
- `CameraController.Instance.IsVisiblePos(Vector3 pos)` checks if a world position is in the current viewport
- The `GridArea` struct has `min`/`max` (`Vector2I`) and a `Contains(Vector3)` method

### Keyboard Camera Panning

Camera panning uses `Action.PanLeft/Right/Up/Down` input actions, which set boolean flags (`panLeft`, `panRight`, `panUp`, `panDown`). Each frame in `NormalCamUpdate()`, these accumulate into `keyPanDelta` at a speed based on `keyPanningSpeed` (from player prefs, 0.1-2.0) scaled by current zoom level. Panning clears follow target and smooth-pan target.

### Camera Bounds

`ConstrainToWorld()` runs every frame and clamps the camera so the viewport stays within `ClusterManager.Instance.activeWorld` bounds. Top Y bound is multiplied by `MAX_Y_SCALE = 1.1f`. Constraint is disabled in free camera mode.

---

## Active Tool and Mode Detection

### Active Tool

`PlayerController.Instance.ActiveTool` returns the current `InterfaceTool`. Check type to determine which tool:

```csharp
var tool = PlayerController.Instance.ActiveTool;
if (tool is SelectTool) { /* default selection */ }
if (tool is BuildTool) { /* placing a building */ }
if (tool is DigTool) { /* marking dig orders */ }
if (tool is MopTool) { /* marking mop orders */ }
if (tool is DeconstructTool) { /* deconstructing */ }
if (tool is PrioritizeTool) { /* setting priorities */ }
if (tool is CancelTool) { /* canceling orders */ }
if (tool is HarvestTool) { /* marking harvest */ }
```

**Build tool state**: When `BuildTool` is active, the selected building definition can be accessed via `BuildToolHoverTextCard.currentDef` on the tool's hover card component.

### Active Overlay

`OverlayScreen.Instance.GetMode()` returns the active overlay as a `HashedString`. Compare against overlay ID constants:

```csharp
HashedString mode = OverlayScreen.Instance.GetMode();
if (mode == OverlayModes.Oxygen.ID) { /* oxygen overlay active */ }
if (mode == OverlayModes.Power.ID) { /* power overlay active */ }
// etc.
```

**Overlay change callback**: `OverlayScreen.Instance.OnOverlayChanged` is an `Action<HashedString>` event fired when the overlay changes.

### Keyboard Navigation and Virtual Cursor

**The game has no native tile-by-tile keyboard navigation**, but it does have built-in virtual cursor infrastructure. All tools resolve cursor position through a single bottleneck: `KInputManager.GetMousePos()`.

**KInputManager.GetMousePos()** (`KInputManager.cs`):
```csharp
public static Vector3 GetMousePos()
{
    if (isMousePosLocked)
        return lockedMousePos;
    if (currentControllerIsGamepad)
        return virtualCursorPos;
    return Input.mousePosition;
}
```

Two override mechanisms exist:
1. **Mouse lock**: Set `KInputManager.isMousePosLocked = true` and `KInputManager.lockedMousePos` to a screen-space position. The entire pipeline (hover tooltips, tool actions, selection) will read this locked position instead of the real mouse.
2. **Gamepad virtual cursor**: Set `KInputManager.currentControllerIsGamepad = true` and write to `KInputManager.virtualCursorPos`. Same effect.

Because every system reads cursor position through this single method, overriding it redirects the full pipeline: hover tooltip cell lookup (`SelectToolHoverTextCard.UpdateHoverElements`), object-under-cursor detection (`InterfaceTool.GetObjectUnderCursor2D`), tool actions (`PlayerController.GetCursorPos`), and selection (`SelectTool.OnLeftClickDown`).

**Direct selection** is also possible without faking mouse position: `SelectTool.Instance.Select(KSelectable)` takes a `KSelectable` reference directly and triggers the inspect panel via `DetailsScreen`. To get the `KSelectable` at a cell, look up the object via `Grid.Objects[cell, layer]` and call `GetComponent<KSelectable>()`.

**SelectTool.selectedCell** is a private field only set on mouse click. It is read-only via `GetSelectedCell()` and not useful for injection.

A tile cursor needs to: (1) track a cell index, (2) convert it to screen coordinates via `Camera.main.WorldToScreenPoint(Grid.CellToPos(cell))`, (3) write that to `KInputManager.lockedMousePos` or `virtualCursorPos`. The entire hover, selection, and tool action pipeline then operates on that cell with no further patches needed.

---

## Getting Speakable Text from GameObjects

### Universal Name Method

`go.GetProperName()` is an extension method (`KSelectableExtensions.cs`) that works on any `GameObject` or `Component`. It delegates to `KSelectable.GetName()`, which returns the `entityName` field (falls back to `GameObject.name`).

`GameUtil.GetUnitFormattedName(GameObject go)` extends this: for countable items (those with `PrimaryElement`), it appends quantity, e.g. "Copper Ore (150 kg)". For non-countable objects, it returns just the name.

### By Entity Type

| Entity | Name Method | Additional Data |
|---|---|---|
| Building | `go.GetProperName()` | Material: `go.GetComponent<PrimaryElement>().ElementID` -> `ElementLoader.FindElementByHash(id).name` |
| Duplicant | `go.GetProperName()` | Name stored in `MinionIdentity.name` (serialized field) |
| Critter | `go.GetProperName()` | Species name only (no individual names by default) |
| Debris | `GameUtil.GetUnitFormattedName(go)` | Includes quantity. Element via `PrimaryElement`. |
| Plant | `go.GetProperName()` | Growth: `Growing` component — `IsGrown()`, `IsWilting()`, growing percentage |
| Tag/Material | `TagManager.GetProperName(tag)` | Converts tags to localized display names |

### Building Operational Status

Every `KSelectable` has a `StatusItemGroup` that tracks active status items (working, idle, disabled, entombed, flooded, overheated, etc.):

```csharp
KSelectable selectable = building.GetComponent<KSelectable>();
StatusItemGroup group = selectable.GetStatusItemGroup();
foreach (StatusItemGroup.Entry entry in group)
{
    string statusText = entry.GetName(); // e.g. "Working", "Entombed"
    StatusItem item = entry.item;        // Status definition
}
```

Status items are defined in `Database/BuildingStatusItems.cs` and dynamically added/removed as conditions change.

---

## Pending Orders Per Cell

### Order Storage

| Order Type | Grid Layer | Detection |
|---|---|---|
| Build (ghost building) | `Grid.Objects[cell, (int)ObjectLayer.Building]` | Has `Constructable` component |
| Dig | `Grid.Objects[cell, 7]` | Has `Diggable` component |
| Mop | `Grid.Objects[cell, 8]` | Has `Moppable` component |
| Sweep | Not per-cell (manager-based) | `Clearable.IsMarkedForClear()` on individual `Pickupable` items |

All cell-based orders (build, dig, mop) have a `Prioritizable` component for work priority:
```csharp
Prioritizable p = order.GetComponent<Prioritizable>();
PrioritySetting priority = p.GetMasterPriority();
int level = priority.priority_value; // 1-9
```

### Detecting Orders

```csharp
// Pending build
GameObject building = Grid.Objects[cell, (int)ObjectLayer.Building];
bool hasBuildOrder = building?.GetComponent<Constructable>() != null;

// Pending dig
GameObject digPlacer = Grid.Objects[cell, 7];
bool hasDigOrder = digPlacer?.GetComponent<Diggable>() != null;

// Pending mop
GameObject mopPlacer = Grid.Objects[cell, 8];
bool hasMopOrder = mopPlacer?.GetComponent<Moppable>() != null;

// Sweep: check individual debris items
GameObject debris = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
Clearable clearable = debris?.GetComponent<Clearable>();
bool hasSweepOrder = clearable != null && clearable.IsMarkedForClear();
```

---

## Fog of War

### Visibility Arrays

`Grid.Visible` (`byte[]`, one per cell) tracks current visibility:
- `0`: Not explored (in fog of war)
- `> 0`: Visible/explored

`Grid.Spawnable` (`byte[]`, one per cell) tracks exploration history:
- `0`: Never explored
- `> 0`: Has been revealed at some point

`Grid.VisMasks` (`VisFlags[]`) provides bitflag visibility:
```
VisFlags:
  Revealed              = 0x01  — has been explored
  PreventFogOfWarReveal = 0x02  — don't auto-reveal
  RenderedByWorld       = 0x04  — visual rendering active
  AllowPathfinding      = 0x08  — navigation permitted
```

### Querying Visibility

```csharp
bool isVisible = Grid.IsVisible(cell);  // true if Visible[cell] > 0 (or fog disabled)
bool explored = Grid.Spawnable[cell] > 0;  // true if ever revealed
bool revealed = Grid.Revealed[cell];  // VisFlags.Revealed bit
```

Cells are revealed via `Grid.Reveal(cell, visibility, forceReveal)`, which sets both `Visible` and `Spawnable` arrays and fires `Grid.OnReveal` callback.

---

## Implementation Reference

### Accessing Per-Cell Data

```csharp
int cell = Grid.PosToCell(worldPosition);

// Element
Element element = Grid.Element[cell];
string name = element.name;
bool isSolid = element.IsSolid;
bool isLiquid = element.IsLiquid;
bool isGas = element.IsGas;
bool isVacuum = element.IsVacuum;

// Thermodynamics
float massKg = Grid.Mass[cell];
float tempKelvin = Grid.Temperature[cell];
float pressureKPa = Grid.Pressure[cell]; // mass * 101.3

// Disease
byte diseaseIdx = Grid.DiseaseIdx[cell]; // 255 = none
int germs = Grid.DiseaseCount[cell];

// Radiation (DLC)
float rads = Grid.Radiation[cell];

// Passability
bool solid = Grid.Solid[cell];
bool dupeBlocked = Grid.DupeImpassable[cell];
bool hasDoor = Grid.HasDoor[cell];
```

### Accessing Buildings and Objects

```csharp
// Foreground building
GameObject building = Grid.Objects[cell, (int)ObjectLayer.Building];

// Foundation tile (constructed/insulated tiles)
GameObject tile = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];

// Background building (drywall, etc.)
GameObject backwall = Grid.Objects[cell, (int)ObjectLayer.Backwall];

// Duplicant (direct slot, not linked list)
GameObject dupe = Grid.Objects[cell, (int)ObjectLayer.Minion];

// ObjectLayer.Critter is dead code — critters are on Pickupables layer

// Plant
GameObject plant = Grid.Objects[cell, (int)ObjectLayer.Plants];

// Pickupables linked list (debris, critters, AND duplicants share this layer)
GameObject pickupableHead = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
// Traverse: pickupableHead.GetComponent<Pickupable>().objectLayerListItem.nextItem
// Filter by component: MinionIdentity = duplicant, CreatureBrain = critter, else = debris

// Utilities
GameObject gasPipe = Grid.Objects[cell, (int)ObjectLayer.GasConduit];
GameObject liquidPipe = Grid.Objects[cell, (int)ObjectLayer.LiquidConduit];
GameObject wire = Grid.Objects[cell, (int)ObjectLayer.Wire];
GameObject logicWire = Grid.Objects[cell, (int)ObjectLayer.LogicWire];
GameObject conveyor = Grid.Objects[cell, (int)ObjectLayer.SolidConduit];
```

### Accessing Conduit Contents

```csharp
// Get the ConduitFlow for the conduit type
ConduitFlow liquidFlow = Game.Instance.liquidConduitFlow;
ConduitFlow gasFlow = Game.Instance.gasConduitFlow;

// Query contents at a cell
ConduitFlow.ConduitContents contents = liquidFlow.GetContents(cell);
SimHashes element = contents.element;
float mass = contents.mass;
float temperature = contents.temperature;
```

### Accessing Computed Properties

```csharp
// Decor
float decor = Grid.Decor[cell];

// Light
int lux = Grid.LightIntensity[cell];

// Room
CavityInfo cavity = Game.Instance.roomProber.GetCavityForCell(cell);
Room room = cavity?.room;
RoomType type = room?.roomType;

// World membership
byte worldIdx = Grid.WorldIdx[cell];
WorldContainer world = ClusterManager.Instance.GetWorld(worldIdx);
```

### Key Source Files

| File | Purpose |
|---|---|
| `Grid.cs` | All per-cell data arrays, coordinate conversion, cell queries, fog of war |
| `Element.cs` | Element properties (molarMass, state, thermal properties) |
| `ElementLoader.cs` | Element registry, lookup by hash or index |
| `ObjectLayer.cs` | Enum of all 49 object layers |
| `Building.cs` | Building base class, multi-tile PlacementCells |
| `BuildingDef.cs` | Building definitions, placement rules, object layers |
| `ConduitFlow.cs` | Pipe/conduit simulation, GridNode, ConduitContents |
| `SelectToolHoverTextCard.cs` | Hover tooltip content generation |
| `CellSelectionObject.cs` | Raw tile selection data for inspect panel |
| `DetailsScreen.cs` | Click inspect panel rendering |
| `OverlayScreen.cs` | Overlay mode registration, switching, `OnOverlayChanged` event |
| `OverlayModes.cs` | Individual overlay mode implementations |
| `RoomProber.cs` | Per-cell room/cavity assignment |
| `DecorProvider.cs` | Decor source and per-cell accumulation |
| `LightGridManager.cs` | Light emission and per-cell lux accumulation |
| `OxygenBreather.cs` | Duplicant breathing and CO2 emission |
| `GasBreatherFromWorldProvider.cs` | Cell selection for oxygen consumption |
| `CO2Manager.cs` | CO2 particle physics and grid settling |
| `WorldContainer.cs` | Per-world bounds, offset, size |
| `ClusterManager.cs` | Multi-world management |
| `CameraController.cs` | Camera positioning, zoom, follow, visible area, bounds |
| `GridVisibleArea.cs` | Visible cell range calculation from viewport |
| `PlayerController.cs` | Active tool singleton, input dispatch, cursor position |
| `InterfaceTool.cs` | Base class for all tools, selection cycling |
| `SelectTool.cs` | Default selection tool, `selectedCell` |
| `BuildTool.cs` | Building placement tool, selected building def |
| `DigTool.cs` | Dig order placement (layer 7) |
| `MopTool.cs` | Mop order placement (layer 8) |
| `Constructable.cs` | Pending build order component |
| `Diggable.cs` | Pending dig order component |
| `Prioritizable.cs` | Work priority per order/building |
| `KSelectableExtensions.cs` | Universal `GetProperName()` extension methods |
| `GameUtil.cs` | `GetUnitFormattedName`, temperature conversion, `FocusCamera` |
| `KSelectable.cs` | Entity name (`entityName`), status item group |
| `StatusItemGroup.cs` | Active status items on a selectable (working, idle, etc.) |
| `StatusItem.cs` | Status definition (name, tooltip, category) |
| `MinionIdentity.cs` | Duplicant name, traits, attributes |
| `Growing.cs` | Plant growth state machine |
| `Clearable.cs` | Sweep/clear marking for debris |
| `ClearableManager.cs` | Global sweep order tracking |
