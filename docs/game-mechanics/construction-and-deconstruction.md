# Construction and Deconstruction

How build orders are placed, materials are delivered, buildings are constructed and deconstructed, and repairs work. Derived from decompiled source code.

## Construction Process

Construction is a multi-phase process managed by the `Constructable` component (which extends `Workable`). The phases execute in order: material delivery, digging, then building.

### Phase 1: Material Delivery

When a build order is placed, `Constructable.OnSpawn()` creates a `FetchList2` from the building's recipe ingredients. Each ingredient is resolved from the `BuildingDef.CraftRecipe`, which maps material categories to specific element tags chosen by the player. The fetch list uses the `BuildFetch` chore type.

Materials are delivered to a `Storage` component on the construction site object. Once all materials arrive, `OnFetchListComplete()` fires, nulls the fetch list, and advances to the dig phase.

Source: `Constructable.OnSpawn()` (lines 316-328), `Constructable.OnFetchListComplete()`

### Phase 2: Digging

If solid tiles occupy the building's footprint, `PlaceDiggables()` creates `Diggable` objects at each blocked cell using the `BuildDig` chore type. Plants in the way are marked for uprooting with the `BuildUproot` chore type.

For certain building types (ladders, tiles, doors, liquid pumping stations), digging waits until all fetches complete before starting (`waitForFetchesBeforeDigging = true`). This prevents duplicants from digging out support structures before materials are ready.

Source: `Constructable.OnSpawn()` (lines 377-378), `Constructable.PlaceDiggables()`

### Phase 3: Building

The build chore is created only when three conditions are met: all digs are complete, the build location is valid, and the fetch list is null (all materials delivered). The chore uses `ChoreTypes.Build` and is a `WorkChore<Constructable>`.

Source: `Constructable.PlaceDiggables()` (lines 706-716)

### Completion

`OnCompleteWork()` calculates the building's initial temperature as the mass-weighted average of all delivered materials, clamped to a maximum of 318.15 K (45 C). If all materials are liquifiable, there is no lower clamp; otherwise the temperature is clamped to the range [0, 318.15] K.

The completed building is instantiated via `BuildingDef.Build()`, which places the `BuildingComplete` prefab at the cell, sets its orientation, marks grid area, and consumes all storage contents.

Source: `Constructable.OnCompleteWork()`, `Constructable.FinishConstruction()`, `BuildingDef.Build()`

## Build Time

### Base Time

Each building has a `ConstructionTime` field on its `BuildingDef`, set during building configuration. `BuildingLoader` calls `Constructable.SetWorkTime(def.ConstructionTime)` to apply it. The game defines standard tiers in `TUNING.BUILDINGS.CONSTRUCTION_TIME_SECONDS`:

| Tier | Seconds |
|------|---------|
| TIER0 | 3 |
| TIER1 | 10 |
| TIER2 | 30 |
| TIER3 | 60 |
| TIER4 | 120 |
| TIER5 | 240 |
| TIER6 | 480 |

Source: `BuildingLoader` line 166, `TUNING.BUILDINGS.CONSTRUCTION_TIME_SECONDS`

### Skill Modifier

Construction speed is modified by the duplicant's Construction attribute through the `ConstructionSpeed` attribute converter. Each point of Construction adds 25% speed (multiplier `0.25f`, base `0f`). The efficiency multiplier has a floor of 75% (`minimumAttributeMultiplier = 0.75f`).

The effective work rate per tick is `dt * GetEfficiencyMultiplier(worker)`, where the multiplier is `max(0.75, 1.0 + Construction * 0.25)`. At Construction 0, work proceeds at 100% speed. At Construction 4, work proceeds at 200% speed.

Source: `Constructable.OnPrefabInit()` (lines 278-280), `Database.AttributeConverters` (line 75), `Workable.GetEfficiencyMultiplier()`

### Tech Tier Restriction

Buildings with a tech tier above 2 set `requireMinionToWork = true`, meaning they cannot be built by automated workers (sweep bots, etc.) and require a duplicant on the asteroid.

Source: `Constructable.OnPrefabInit()` (lines 272-275)

## Material Requirements

### Category System

Each `BuildingDef` has a `MaterialCategory` string array and a parallel `Mass` float array. The category is a tag like `"Metal"`, `"RefinedMetal"`, `"BuildableRaw"`, etc. When the recipe is created in `BuildingDef.PostProcess()`, each category becomes a `Recipe.Ingredient` with the category tag and mass amount (cast to int).

Standard mass tiers are defined in `TUNING.BUILDINGS.CONSTRUCTION_MASS_KG`:

| Tier | Mass (kg) |
|------|-----------|
| TIER_TINY | 5 |
| TIER0 | 25 |
| TIER1 | 50 |
| TIER2 | 100 |
| TIER3 | 200 |
| TIER4 | 400 |
| TIER5 | 800 |
| TIER6 | 1200 |
| TIER7 | 2000 |

Source: `BuildingDef.PostProcess()` (lines 1766-1773), `TUNING.BUILDINGS.CONSTRUCTION_MASS_KG`

### Material Selection

The player chooses a specific element for each category slot. `MaterialSelector.GetValidMaterials()` filters `ElementLoader.elements` to find all solid elements that match the category tag. The category string can use `&` to combine multiple tags (OR logic): any element matching any of the joined tags is valid.

When ingredients are resolved at construction time, `Recipe.GetAllIngredients()` replaces category tags with the player's selected element tags, preserving the mass amounts.

Source: `MaterialSelector.GetValidMaterials()`, `Recipe.GetAllIngredients()`

### Multi-Material Buildings

Some buildings have multiple material slots. The `MaterialCategory` and `Mass` arrays can have more than one entry. Rocketry components commonly use two slots (e.g., `NOSE_CONE_TIER1 = {200f, 100f}`). Each slot is an independent category/mass pair with its own element selection.

Source: `TUNING.BUILDINGS.ROCKETRY_MASS_KG`

## Deconstruction

### Work Time

Deconstruction uses the `Deconstructable` component (also extends `Workable`). Work time depends on building type:
- **Tiles** (`IsTilePiece`): `ConstructionTime * 0.5` (half the build time)
- **Non-tiles**: fixed 30 seconds
- **Custom override**: if `customWorkTime > 0`, uses that value

Source: `Deconstructable.OnPrefabInit()` (lines 103-116)

### Skill Modifier

Deconstruction uses the same `ConstructionSpeed` attribute converter as construction, with the same 25% per point and 75% minimum multiplier.

Source: `Deconstructable.OnPrefabInit()` (lines 94-98)

### Material Return

Deconstruction returns 100% of construction materials. `SpawnItemsFromConstruction()` iterates the `constructionElements` tag array and the `BuildingDef.Mass` array, spawning each material at its full mass. Materials inherit the building's temperature and disease.

For elements, materials are spawned via `element.substance.SpawnResource()`. For non-element items (prefab tags), they are instantiated via `GameUtil.KInstantiate()`. Large masses (over 400 kg) are split into 400 kg chunks distributed across the building's placement cells.

Source: `Deconstructable.SpawnItemsFromConstruction()` (lines 336-359), `Deconstructable.SpawnItem()`

### Tile Deconstruction

When deconstructing tiles (`SimCellOccupier` present), the tile layer and object layer grid entries are cleared, `Grid.Foundation` is set to false, and `SimCellOccupier.DestroySelf()` handles the sim-side cleanup. Material spawning occurs in the destroy callback.

Source: `Deconstructable.OnCompleteWork()` (lines 185-208)

## Demolition

`Demolishable` is a separate component from `Deconstructable`, used for POI buildings and other special structures. It requires the `CanDemolish` skill perk. Unlike deconstruction, demolition does not return any materials -- it simply destroys the object.

Work time follows the same rules as deconstruction: half construction time for tiles, 30 seconds for non-tiles.

Source: `Demolishable.OnPrefabInit()`, `Demolishable.OnCompleteWork()`

## Cancellation

### Cancelling a Build Order

When a construction is cancelled (via user menu or the Cancel tool), `Constructable.OnCancel()` fires:
1. Clears material needs tracking (`ClearMaterialNeeds`)
2. Cancels pending uproots (`ClearPendingUproots`)
3. The fetch list is cancelled in `OnCleanUp()` if still active
4. The `Storage` component's `OnQueueDestroyObject` handler calls `DropAll()`, returning all already-delivered materials to the world

Materials that were in transit (being carried by duplicants) are not consumed; the fetch chore is simply cancelled and the duplicant drops the item.

Source: `Constructable.OnCancel()`, `Constructable.OnCleanUp()`, `Storage.OnQueueDestroyObject()`

### Cancelling Deconstruction

`Deconstructable.CancelDeconstruction()` cancels the work chore, removes the pending status item, hides the progress bar, and clears the marked flag. No materials are affected since the building is still intact.

Source: `Deconstructable.CancelDeconstruction()`

## Repair

### Triggering Repair

Repair is managed by the `Repairable` component, which runs a state machine with three top-level states: `repaired`, `allowed`, and `forbidden`. Buildings start in the `repaired` state.

When `BuildingHP` receives damage (`BuildingReceivedDamage` event), the state machine transitions to `allowed` if auto-repair is enabled. The `allowed` state has two sub-states:

1. **needMass**: Creates a `FetchChore` (type `RepairFetch`) to deliver repair materials. The required mass is 10% of the building's total mass (`component.Mass * 0.1f`), using the same element as the building's `PrimaryElement`.
2. **repairable**: Once materials arrive, creates a recurring `WorkChore<Repairable>` (type `Repair`).

Source: `Repairable.States.InitializeStates()`, `Repairable.SMInstance.GetRequiredMass()`

### Repair Rate

Repair uses infinite work time (`workTime = float.PositiveInfinity`) and ticks manually via `OnWorkTick()`. The tick interval is `sqrt(building_mass) * 0.1` seconds (or `expectedRepairTime * 0.1` if set). Each tick:

1. Calculates the worker's Machinery attribute value
2. Heals `ceil((10 + max(0, Machinery * 10)) * 0.1)` HP

At Machinery 0, each tick heals 1 HP. At Machinery 5, each tick heals 6 HP.

Source: `Repairable.OnWorkTick()` (lines 302-323), `Repairable.OnSpawn()` (lines 231-232)

### Repair Preconditions

Repair chores have two extra preconditions beyond standard work chores:
- **IsNotBeingAttacked**: Skips if a `Breakable` component has an active worker (duplicant is smashing the building)
- **IsNotAngry**: Skips if the duplicant has the Aggressive trait and stress is above the acting-out threshold

Source: `Repairable.States.CreateRepairChore()`

### BuildingHP Direct Repair

`BuildingHP` also extends `Workable` and has its own `OnCompleteWork()` that heals `10 + max(0, Machinery * 10)` HP (note: no 0.1 multiplier here, so 10x the per-tick amount). This path is used for the simpler repair mechanic that does not require material delivery.

Source: `BuildingHP.OnCompleteWork()` (lines 349-354)

### User Control

Players can toggle auto-repair via the user menu. Disabling it transitions the state machine to `forbidden`, which cancels any active repair chore and drops stored repair materials.

Source: `Repairable.CancelRepair()`, `Repairable.States.allowed.Exit()`

## Building Placement Rules

### BuildLocationRule Enum

Each `BuildingDef` has a `BuildLocationRule` that determines valid placement. The rules and their foundation checks:

| Rule | Requirement |
|------|-------------|
| OnFloor | Solid cells below entire building width |
| OnCeiling | Solid cells above entire building width |
| OnWall | Solid wall along one side (full height) |
| InCorner | Solid ceiling AND solid wall on one side |
| WallFloor | Solid floor AND solid wall on one side |
| OnBackWall | Every occupied cell must have a non-solid cell with a building in the Building layer behind it |
| Tile | Can overlap existing tiles (unless a NotInTiles building occupies the cell) |
| NotInTiles | Cannot be placed in cells occupied by tiles |
| Anywhere | No placement restriction |
| Conduit | No placement restriction (same as Anywhere) |
| BuildingAttachPoint | Must be placed at a matching `BuildingAttachPoint` (checked via `AttachmentSlotTag`) |
| OnFloorOverSpace | Floor foundation AND all occupied cells must be in Space zone |
| OnRocketEnvelope | Foundation cells must be solid AND tagged `RocketEnvelopeTile` |
| BelowRocketCeiling | Building top + 35 cells must be below world ceiling |
| OnFoundationRotatable | Same as OnFloor but supports rotated orientations |
| OnFloorOrBuildingAttachPoint | No restriction (treated as Anywhere in validation) |

Source: `BuildLocationRule` enum, `BuildingDef.IsValidBuildLocation()`, `BuildingDef.CheckFoundation()`

### Foundation Checks

`CheckBaseFoundation()` checks the row of cells directly below (for floor) or above (for ceiling) the building footprint. Each cell must satisfy `Grid.Solid[cell]`. If an optional foundation tag is specified (e.g., `RocketEnvelopeTile`), the cell must also contain a building with that tag.

`CheckWallFoundation()` checks the column of cells along one side of the building (left or right, determined by orientation). Each cell must be solid or contain a building under construction that `IsFoundation`.

`CheckBackWallFoundation()` checks every cell the building occupies: each must be non-solid AND contain a building in object layer 2 (the Backwall layer).

Source: `BuildingDef.CheckBaseFoundation()`, `BuildingDef.CheckWallFoundation()`, `BuildingDef.CheckBackWallFoundation()`

### Additional Validation

Beyond foundation checks, `IsValidBuildLocation()` also validates:
- All placement cells are valid building cells (`Grid.IsValidBuildingCell`)
- Power ports are in valid positions (`ArePowerPortsInValidPositions`)
- Conduit ports are in valid positions (`AreConduitPortsInValidPositions`)
- No conflicting buildings in the same object layer (except for replacement tiles)

Source: `BuildingDef.IsValidBuildLocation()` (line 1314), `BuildingDef.IsAreaValid()`

## Tile Construction Specifics

### Replacement Tiles

Tiles can be replaced in-place without deconstructing first. When `IsReplacementTile` is true on a `Constructable`:
- The construction site is registered on the `ReplacementLayer` instead of the `ObjectLayer`
- Any pending deconstruction on the existing tile is cancelled
- On completion, the old tile's `Deconstructable.SpawnItemsFromConstruction()` is called (returning its materials), then the old tile is deleted and the new one is built

Source: `Constructable.OnSpawn()` (lines 344-375), `Constructable.OnCompleteWork()` (lines 156-213)

### Tile Work Offsets

Tiles use `OffsetGroups.InvertedStandardTableWithCorners` for worker reachability (allowing approach from diagonal positions), while non-tile buildings use `OffsetGroups.InvertedStandardTable`.

Source: `Constructable.OnSpawn()` (lines 294-296)

### Tile Under Construction Flag

While a tile is being built, `Grid.IsTileUnderConstruction[cell]` is set to true. This flag is cleared in `UnmarkArea()` when construction completes or is cancelled.

Source: `Constructable.MarkArea()` (line 429), `Constructable.UnmarkArea()` (line 443)
