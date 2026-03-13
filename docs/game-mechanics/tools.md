# Tools

Player-facing tools available in the toolbar. These are drag-select tools the player uses to issue orders to duplicants across one or more cells. All tools are registered in `ToolMenu.CreateBasicTools()`. Sandbox and debug tools are excluded.

## Tool List

1. Dig
2. Cancel
3. Deconstruct
4. Prioritize
5. Disinfect
6. Sweep (Clear / Mark for Storage)
7. Mop
8. Harvest
9. Attack
10. Capture (Wrangle)
11. Empty Pipe
12. Disconnect

## Dig

**Class**: `DigTool` (extends `DragTool`)

Places dig markers that create `WorkChore<Diggable>` errands. Also uproots any plants in the target cells.

### Eligibility

- Cell must be solid: `Grid.Solid[cell]`
- Cell must not be a Foundation tile (player-built tile): `!Grid.Foundation[cell]`
- No existing dig marker at that cell (object layer 7)
- No pending construction at that cell (no `Constructable` component on any of the 45 object layers)
- Hardness must not be 255 (Unobtanium/Neutronium) -- the marker is placed but immediately cancelled in `Diggable.OnSpawn()`

### Skill Requirements

Set by `Diggable.OnSolidChanged()` based on element hardness:

- Hardness < 50: no skill required
- Hardness >= 50: CanDigVeryFirm (Mining 1)
- Hardness >= 150: CanDigNearlyImpenetrable (Mining 2)
- Hardness >= 200: CanDigSuperDuperHard (Mining 3)
- Hardness >= 251: CanDigRadioactiveMaterials (Mining 4, DLC only)
- Hardness == 255: CanDigUnobtanium -- no skill grants this; effectively undiggable

### Dig Time

Calculated by `Diggable.GetApproximateDigTime(cell)`:

```
hardness_ratio = element.hardness / ice.hardness
mass_factor = Min(mass, 400) / 400
base_time = 4 * mass_factor
dig_time = base_time + hardness_ratio * base_time
```

Ice hardness is 25. Mass is capped at 400 kg for the time calculation. The Digging attribute modifies actual work speed but not this base formula.

### What Happens

Each work tick calls `DoDigTick(cell, dt)` which applies `dt / approximateDigTime` as damage to the tile via `WorldDamage.ApplyDamage()`. When the tile's damage reaches 1.0, the element is destroyed and drops as a pickupable item. The dig marker auto-cancels if the tile becomes non-solid before the dig completes (e.g., melted or fell as unstable).

Source: DigTool.cs, Diggable.cs.

---

## Cancel

**Class**: `CancelTool` (extends `FilteredDragTool`)

Cancels pending orders (dig markers, construction, deconstruction, mop, sweep, harvest, etc.) and also unmarks Attack and Capture designations in the selected area.

### Eligibility

Targets all 45 object layers per cell. Each object's layer is checked against the filter menu. Default filters have most layers On, but CLEANANDCLEAR and DIGPLACER are Off by default (can be toggled on to cancel mop/sweep/dig markers).

### What Happens

- Fires event `2127324410` (the cancel event) on every matching object in each cell
- After the drag completes, calls `AttackTool.MarkForAttack(min, max, mark: false)` and `CaptureTool.MarkForCapture(min, max, mark: false)` across the entire selected region

Each component that listens for the cancel event handles its own cleanup: `Diggable` removes the dig marker, `Constructable` cancels construction and drops partial materials, `Clearable` removes the sweep designation, `Moppable` removes the mop marker, etc.

Source: CancelTool.cs.

---

## Deconstruct

**Class**: `DeconstructTool` (extends `FilteredDragTool`)

Marks buildings for deconstruction. Uses the same filter system as Cancel to target specific building layers.

### Eligibility

Targets built objects (BuildingComplete) across all 45 object layers, filtered by the layer menu. The building must have a `Deconstructable` component with `allowDeconstruction == true`. If the building has a pending replacement (something queued in the ReplacementLayer), deconstruction is blocked.

### Work Time

- Tile pieces: `constructionTime * 0.5` seconds
- All other buildings: 30 seconds (unless `customWorkTime` is set on the building)
- Modified by the Construction Speed attribute

### What Happens

Fires event `-790448070` (deconstruct event) on matching objects, which triggers `Deconstructable.QueueDeconstruction()`. This creates a `WorkChore<Deconstructable>`. When complete, the building is destroyed and **returns its construction materials** as pickupable items. Each material type in the recipe is spawned at the building's position with its original mass, temperature, and disease state.

Source: DeconstructTool.cs, Deconstructable.cs.

---

## Prioritize

**Class**: `PrioritizeTool` (extends `FilteredDragTool`)

Sets the errand priority on all prioritizable objects in the selected area to the currently selected priority level.

### Eligibility

Targets all 45 object layers per cell. Objects must have a `Prioritizable` component with `showIcon == true` and `IsPrioritizable() == true`. Custom filter categories:

- **Dig**: objects with `Diggable` component
- **Construction**: objects with `Constructable`, or `Deconstructable` that is already marked for deconstruction
- **Clean**: objects with `Clearable`, `Moppable`, or `StorageLocker`
- **Operate**: everything else with a visible priority icon

For pickupable items, the tool walks the entire Pickupable chain at the cell (all stacked items) and prioritizes each one individually (skipping duplicants).

### What Happens

Calls `Prioritizable.SetMasterPriority()` with the currently selected priority on every matching object. This propagates to all chores associated with that object.

Source: PrioritizeTool.cs.

---

## Disinfect

**Class**: `DisinfectTool` (extends `DragTool`)

Marks germ-contaminated objects for disinfection.

### Eligibility

Targets all 45 object layers per cell. Objects must have:
- A `Disinfectable` component
- `PrimaryElement.DiseaseCount > 0` (must actually have germs)

### Work Time

10 seconds. Disease is removed proportionally over the work duration: `diseasePerSecond = totalDiseaseCount / 10`. On completion, all remaining disease is removed. Modified by Tidying Speed attribute (Basekeeping skill group).

### What Happens

Creates a `WorkChore<Disinfectable>`. During work, germs are gradually removed each tick. On completion, all germs are cleared from the object's PrimaryElement. The Disease overlay activates automatically when the tool is selected.

Source: DisinfectTool.cs, Disinfectable.cs.

---

## Sweep (Clear / Mark for Storage)

**Class**: `ClearTool` (extends `DragTool`)

Marks loose items (debris) for pickup and storage. Known in the UI as "Sweep" or "Mark for Storage".

### Eligibility

Targets pickupable items at object layer 3. For each item in the Pickupable chain at the cell:
- Must have a `Clearable` component with `isClearable == true`
- Must not be a duplicant (`!HasTag(GameTags.BaseMinion)`)
- Must not be entombed (`!IsEntombed`)
- Must not already be stored (unless the storage has `allowClearable == true`)

### What Happens

Calls `Clearable.MarkForClear()` on each matching item, which:
1. Adds `GameTags.Garbage` to the item
2. Registers with `GlobalChoreProvider.RegisterClearable()`, which creates fetch chores for duplicants to move items to any available storage that accepts them
3. Shows a "Pending Clear" status icon on the item

If no storage destination exists that will accept the item, the status changes to "Pending Clear - No Storage" to warn the player. This check runs every 1000ms.

When an item absorbs another item (stacking), if the absorbed item was marked for sweep, the absorber also gets marked.

Source: ClearTool.cs, Clearable.cs.

---

## Mop

**Class**: `MopTool` (extends `DragTool`)

Marks small liquid spills for cleanup.

### Eligibility

All four conditions must be met:
- Cell must not be solid: `!Grid.Solid[cell]`
- Cell must contain liquid: `Grid.Element[cell].IsLiquid`
- Cell must have a solid floor below: `Grid.Solid[Grid.CellBelow(cell)]`
- Liquid mass must be <= 150 kg: `Grid.Mass[cell] <= 150f`

If the cell has liquid but is above the 150 kg threshold, shows "Too Much Liquid" PopFX. If there is no solid floor, shows "Not on Floor" PopFX.

### What Happens

Places a MopPlacer at object layer 8 and creates a `WorkChore<Moppable>`. The Moppable work has infinite work time (runs until the liquid is gone). Each tick (200ms while a worker is active), `MopTick` consumes up to 1000 units of liquid from the mop cell and its two horizontal neighbors (offsets 0, +1, -1). The consumed liquid is converted to a SubstanceChunk (a pickupable liquid bottle) dropped at the mop location.

The mop marker auto-destroys 1 second after no moppable liquid remains in its neighborhood. It also monitors liquid changes in real time and will self-cancel if the liquid disappears (e.g., evaporates or drains). Modified by Tidying Speed attribute (Basekeeping skill group).

Source: MopTool.cs, Moppable.cs.

---

## Harvest

**Class**: `HarvestTool` (extends `DragTool`)

Marks plants for harvesting or disables auto-harvest. Has a toggle parameter menu with two modes.

### Eligibility

Targets all `HarvestDesignatable` components globally. A plant matches if:
- Its cell position equals the target cell, OR
- It has an `OccupyArea` component that overlaps the target cell (for multi-tile plants like trees)

### Modes

Controlled by the ToolParameterMenu toggle (one is always active):

- **Harvest When Ready** (default On): Sets `HarvestWhenReady(true)`. The plant will be automatically harvested by duplicants when its growth is complete.
- **Do Not Harvest** (default Off): Fires the cancel event on the plant's `Harvestable` and sets `HarvestWhenReady(false)`. The plant will not be harvested even when mature.

### What Happens

In Harvest When Ready mode, when the plant becomes harvestable (`canBeHarvested == true`), a harvest chore is created. Priority can be set on each plant via the tool's priority screen. The Harvest overlay activates automatically when the tool is selected.

Source: HarvestTool.cs, HarvestDesignatable.cs, Harvestable.cs.

---

## Attack

**Class**: `AttackTool` (extends `DragTool`)

Marks creatures as attack targets.

### Eligibility

Targets all `FactionAlignment` components globally. A creature matches if:
- Its position falls within the drag region
- Its faction disposition relative to Duplicants is NOT `Assist` (won't target friendly creatures like Shine Bugs set to ally)

### What Happens

Calls `SetPlayerTargeted(true)` on matching creatures, which adds the creature to duplicants' attack errand lists. Priority can be set via the tool's priority screen.

The Cancel tool's drag-complete also calls `AttackTool.MarkForAttack(min, max, mark: false)` to unmark creatures in the cancelled region.

Source: AttackTool.cs.

---

## Capture (Wrangle)

**Class**: `CaptureTool` (extends `DragTool`)

Marks creatures for capture/wrangling.

### Eligibility

Targets all `Capturable` components globally. A creature matches if its position falls within the drag region. Additional checks on the creature:
- `allowCapture == true` (some creatures cannot be captured)
- Not tagged as Trapped
- Not tagged as Stored
- Not tagged as Bagged (already captured)

If a creature is in the region but not capturable, shows "Not Capturable" PopFX.

### Skill Requirement

Requires the `CanWrangleCreatures` skill perk (Critter Ranching 1).

### Work Time

10 seconds. During work, the creature is tagged with `StunnedForCapture` (stops its AI movement). On completion, the creature is "bagged" -- converted to a pickupable item that can be moved or released. Modified by Capturable Speed attribute (Ranching skill group).

### What Happens

Creates a `WorkChore<Capturable>`. The Cancel tool's drag-complete also calls `CaptureTool.MarkForCapture(min, max, mark: false)` to unmark creatures in the cancelled region.

Source: CaptureTool.cs, Capturable.cs.

---

## Empty Pipe

**Class**: `EmptyPipeTool` (extends `FilteredDragTool`)

Marks conduit segments for emptying.

### Eligibility

Targets all 45 object layers per cell, filtered by the layer menu. Objects must implement the `IEmptyConduitWorkable` interface. Default filter layers:

- Liquid Conduit
- Gas Conduit
- Solid Conduit

### What Happens

Calls `MarkForEmptying()` on matching conduit segments, which creates a work chore for a duplicant to extract the contents. The conduit segment is not destroyed -- only its contents are removed and dropped as a pickupable item or vented. Priority can be set via the tool's priority screen.

Source: EmptyPipeTool.cs.

---

## Disconnect

**Class**: `DisconnectTool` (extends `FilteredDragTool`)

Severs utility connections between adjacent buildings without destroying either building.

### Eligibility

Targets `Building` components that implement `IHaveUtilityNetworkMgr` -- buildings that participate in utility networks (power wires, liquid/gas/solid conduits, automation/logic wires). Default filter layers:

- Wires (power)
- Liquid Conduit
- Gas Conduit
- Solid Conduit
- Buildings
- Logic

### Drag Modes

The tool operates in **single disconnect mode** by default:
- Uses Line mode with a maximum drag length of 2 cells
- Disconnects a single connection between two adjacent cells

The code also supports a multi-cell Box mode (toggled by `singleDisconnectMode`), but single mode is the default.

### What Happens

For each cell in the selected region, the tool checks which utility connections (Up/Down/Left/Right) lead to other cells also within the selected region. Those connections are removed via `KAnimGraphTileVisualizer.UpdateConnections()`, and the network is rebuilt with `ForceRebuildNetworks()`. This splits a utility network into two separate networks at the disconnection point without deconstructing any buildings.

During the drag, visualizers preview which connections will be severed.

Source: DisconnectTool.cs.

---

## Common Mechanics

### Priority Screen

All tools except Cancel and Disconnect show the Priority Screen when activated, allowing the player to set errand priority (1-9). The selected priority is applied to every object the tool touches. The Prioritize tool shows the screen at 1.35x scale with a priority diagram.

### Drag Select Modes

Tools use one of three drag modes (set by `DragTool.GetMode()`):

- **Box**: Click-drag to define a rectangular region. Applied on release. Used by: Dig, Cancel, Deconstruct, Prioritize, Attack, Capture, Empty Pipe.
- **Brush**: Click-drag to paint along the cursor path. Applied to each cell as the cursor moves. Used by: Mop, Sweep, Harvest, Disinfect.
- **Line**: Click-drag constrained to a line. Used by: Disconnect (max 2 cells).

### Filter Layers (FilteredDragTool)

Tools that extend `FilteredDragTool` (Cancel, Deconstruct, Prioritize, Empty Pipe, Disconnect) have a filter menu that lets the player choose which building layers to affect. Common filter categories:

| Filter | Covers |
|--------|--------|
| All | Master toggle |
| Wires | Power wires |
| Liquid Conduit | Liquid pipes |
| Gas Conduit | Gas pipes |
| Solid Conduit | Conveyor rails |
| Buildings | Standard buildings |
| Logic | Automation wires |
| Backwall | Drywall, tempshift plates |
| Construction | Pending construction |
| Dig | Dig markers |
| Clean and Clear | Mop/sweep markers |

Not all filters appear for every tool -- each tool defines its own default set via `GetDefaultFilters()`.

### Cancel Event

Event ID `2127324410` is the universal cancel signal. The Cancel tool fires it, and many components listen for it: Diggable, Constructable, Clearable, Moppable, Disinfectable, Harvestable, and others. Each handles its own cleanup when cancelled.
