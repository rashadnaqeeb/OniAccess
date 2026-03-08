# Navigation and Movement

How duplicants and critters navigate the world, including passability, walkability, pathfinding, and movement speed. Derived from decompiled source code.

## The BuildMask

Every cell in the world grid has a one-byte bitmask called `BuildMasks`. The navigation system reads this mask to decide what can go where. The flags are:

- **Solid** (0x01) -- The cell contains solid material. Set by natural terrain and by buildings that replace the cell's element (tiles, bunker tiles, etc.).
- **Foundation** (0x02) -- The cell is a constructed foundation tile or door. Foundation cells cannot be dug. Doors and built tiles set this.
- **Door** (0x04) -- The cell contains a door.
- **DupePassable** (0x08) -- Dupes can walk through this cell even though it is solid. Used by mesh tiles and gas permeable membranes.
- **DupeImpassable** (0x10) -- Dupes are explicitly blocked from entering this cell even though it is not solid. Used by locked internal doors.
- **CritterImpassable** (0x20) -- Critters cannot enter this cell. Used by doors that are not fully open.
- **FakeFloor** (0xC0) -- A 2-bit reference counter (values 0 through 3). When nonzero, the cell acts as walkable ground for the cell above it, without being solid itself. Gas and liquid pass through freely.

## How Passability Is Checked

`NavTableValidator.IsCellPassable(cell, is_dupe)` determines whether a given entity can occupy a cell. It first strips out `FakeFloor`, `Foundation`, and `Door` from the mask -- these flags describe structural properties, not passability -- then examines what remains:

- If nothing remains, the cell is passable.
- If `DupeImpassable` is set, the cell is impassable for dupes regardless of anything else.
- If `Solid` is set, the cell is passable for dupes only if `DupePassable` is also set (mesh tiles, gas permeable membranes). Otherwise it is blocked.
- For critters: any `Solid` or `CritterImpassable` flag means the cell is impassable.

## How Walkability Is Checked

Passability answers "can this entity enter the cell?" Walkability answers "can this entity stand here?" -- meaning is there valid ground underfoot?

`FloorValidator.IsWalkableCell(cell, anchor_cell, is_dupe)` checks both the cell the entity wants to stand in and the anchor cell (one cell below):

1. Is the cell itself passable? If not, the entity cannot stand there.
2. Is the anchor cell marked `FakeFloor`? If yes, walkable. This is how the pitcher pump creates a floor surface without being solid.
3. Is the anchor cell `Solid` and NOT `DupePassable`? If yes, walkable. This is natural ground and built tiles.
4. If the anchor cell is `Solid` AND `DupePassable` (mesh tile), it is NOT walkable ground -- dupes fall through it.
5. For dupes only: if neither solid nor fake floor, check for ladders and poles in the cell or anchor cell.

This means a mesh tile is a special case. A dupe can pass through it (it is passable), but it does not count as ground (it is not walkable from above). In practice mesh tiles do provide a floor because of how the game sets them up -- see the section on solid-but-passable buildings below.

## Building Categories by Passability

### Solid buildings (tiles that replace the cell element)

These buildings use `SimCellOccupier` with `doReplaceElement = true`. On spawn, they call `SimMessages.ReplaceAndDisplaceElement` to physically replace whatever element was in the cell with their own material. The cell becomes `Solid`, blocks movement, blocks gas and liquid, and acts as ground for the cell above.

Examples:
- **Tile** (sandstone, granite, etc.) -- the basic built floor
- **Insulation Tile** -- same as tile but with thermal properties
- **Bunker Tile** -- hardened tile, very high HP
- **Metal Tile** -- provides a speed bonus via `movementSpeedMultiplier`
- **Carpet Tile**, **Plastic Tile**, **Glass Tile** -- decorative variants with different sim properties (e.g. glass sets `setTransparent = true`)
- **Farm Tile**, **Hydroponic Farm** -- foundation tiles that also support plant growth

All of these also set `IsFoundation = true` (via `BuildingTemplates.CreateFoundationTileDef()`), which adds the `Foundation` flag to the cell, preventing digging.

### Solid-but-passable buildings (mesh and membrane)

These buildings use `SimCellOccupier` with `doReplaceElement = false`. They do not replace the cell's element. Instead, `SimCellOccupier.ForceSetGameCellData()` reads `Grid.DupePassable[cell]` and sets solidity accordingly. The cell is marked `Solid` + `DupePassable`: structurally present, but dupes can walk through.

There are exactly two buildings in this category:

- **Mesh Tile** -- allows gas and liquid to pass through. Dupes walk on it as a floor.
- **Gas Permeable Membrane** -- allows gas through but blocks liquid (`setLiquidImpermeable = true`). Dupes walk on it as a floor.

Both are foundation tiles. They appear solid in the grid and provide walkable ground above them, but a dupe in the same cell can pass through.

### Fake floor buildings (walkable surface without solidity)

These buildings use `FakeFloorAdder` to mark cells with the `FakeFloor` flag. The cells are not solid -- gas and liquid flow through freely -- but the navigation system treats them as ground for the cell above.

The `FakeFloor` bits are a reference counter (0-3), not a boolean. Multiple buildings can add fake floors to the same cell, and the floor only disappears when all of them remove it. `FakeFloorAdder.SetFloor(active)` increments or decrements the counter and marks the nav cell dirty.

Always-active fake floors:
- **Pitcher Pump** (LiquidPumpingStation) -- offsets `(0,0)` and `(1,0)`, the bottom two cells of a 2x4 building. Dupes stand on top of the pump's base to operate it.
- **Launch Pad** -- 7 cells across at y+1, providing a walkable platform above the pad surface.
- **Modular Launchpad Port Bridge** -- offset `(0,1)`.
- **Water Trap** -- offset `(0,0)`.

Conditionally-active fake floors (`initiallyActive = false`):
- **Gantry** -- offsets `(0,1)` through `(3,1)`. The `Gantry` component toggles `SetFloor()` at runtime: extended = walkable, retracted = not walkable.
- **Pioneer Module**, **Orbital Cargo Module**, **Habitat Module Small** -- offsets at y-1 (3 cells). Activated when landed.
- **Habitat Module Medium** -- 5 cells at y-1, same pattern.

### Doors (dynamic passability)

Doors are the only buildings that manipulate `DupeImpassable` and `CritterImpassable` at runtime. The `Door` component updates the cell's build flags whenever the door's control state changes (open, locked, auto).

Internal doors (`DoorType.Internal`):
- `DupeImpassable = true` when locked. Dupes cannot enter.
- `CritterImpassable = true` when not fully open. Critters are blocked by any door that is not in the open state.

Pressure doors, manual pressure doors, bunker doors, and sealed doors (`DoorType.ManualPressure`, `Pressure`, `Sealed`):
- Use `Game.SetDupePassableSolid(cell, passable, solid)` to toggle between solid-and-impassable (closed) and passable (open).
- `CritterImpassable = true` when not fully open, same as internal doors.

All doors set `IsFoundation = true` on their `BuildingDef`, so they also mark cells with the `Foundation` flag. This means the cell cannot be dug even when the door is open.

## Non-Building Passability

Natural terrain follows the same rules but without any building flags. A natural solid cell (dirt, granite, etc.) is `Solid` in the sim layer, blocks movement, and provides walkable ground above. It can be dug because it is NOT `Foundation`. Vacuum cells, gas cells, and liquid cells are passable but not walkable ground (dupes fall through unless there is a ladder, pole, or fake floor).

Ladders and poles do not use BuildMask flags at all. They set `NavValidatorFlags.Ladder` or `NavValidatorFlags.Pole` in a separate `NavValidatorMasks` array. The floor validator checks these as a last resort when neither solid ground nor fake floor is available.

## Navigation Grid

The game constructs navigation meshes from the world grid. Each nav grid defines valid cell transitions for different movement types. Nav grids are static until tiles or buildings change, at which point affected regions are rebuilt.

**Navigation types (NavType enum):**

| NavType | Description |
|---------|-------------|
| Floor | Walking on solid ground |
| LeftWall | Walking on left wall surface |
| RightWall | Walking on right wall surface |
| Ceiling | Walking on ceiling (creatures only) |
| Ladder | Climbing ladders |
| Hover | Jet suit flight |
| Swim | Swimming in liquid |
| Pole | Climbing fire poles |
| Tube | Traveling through transit tubes |
| Solid | Inside a diggable solid cell (used by digging creatures) |
| Teleport | Transitional state used during falls and teleportation |

## Pathfinding

The pathfinding system uses a cost-based graph search over the navigation grid. Each transition between cells has an associated cost that incorporates distance, movement type, and environmental penalties.

**Transition costs (base values):**

| Movement | Cost | Notes |
|----------|------|-------|
| Floor horizontal | 10 | Standard walking |
| Floor diagonal | 14 | ~sqrt(2) * 10 |
| Floor double hop (2 cells) | 20 | Jumping gaps |
| Ladder up/down | 10 | Standard climbing |
| Ladder horizontal | 15 | Side-to-side on ladder |
| Pole down | 6 | Fast sliding |
| Pole up | 50 | Very slow climbing |
| Tube straight | 5 | Fast tube travel |
| Tube entry | 40 | Mounting tube |
| Tube exit | 5-17 | Varies by exit direction |
| Hover any direction | 8-9 | Jet suit flight |
| Fall initiation | 14 | Starting to fall |
| Fall landing | 1 | Touching down |

**Submerged penalty:** Duplicants without a protective suit (atmo suit, jet suit, lead suit) pay an additional **2x the base transition cost** as a penalty when traveling through liquid cells, making the effective cost **3x normal**. This makes the pathfinder strongly prefer dry routes.

**Creature underwater limit:** Creatures have a `MaxUnderwaterTravelCost` attribute that caps how far they can path through submerged cells.

## Cell Validation

A cell is walkable (Floor nav type) if:
1. The cell is valid and passable (not a solid building)
2. AND one of:
   - Solid ground below (`Grid.Solid[below]` and not `DupePassable`)
   - Fake floor below (`Grid.FakeFloor[below]`)
   - Ladder or pole present

Wall climbing requires a solid anchor cell to the left or right. Ceiling walking requires a solid block above.

Swimming activates when a duplicant is substantially submerged (`SubmergedMonitor` forces NavType to Swim).

## Movement Speed

**Tile speed multipliers:**

| Tile | Multiplier |
|------|-----------|
| Metal Tile | 1.5x |
| Plastic Tile | 1.5x |
| Tile (basic) | 1.25x |
| Wood Tile | 1.25x |
| Floor Switch | 1.25x |
| Carpet Tile | 0.75x |
| Storage Tile | 0.75x |
| Wire Bridge (Heavy-Watt) | 0.5x |
| Travel Tube Wall Bridge | 0.5x |

These multipliers are applied by `SimCellOccupier` on the building's occupied cells.

**Transit tube speed:** Base 18 units/second. Waxed tubes receive a 25% boost (22.5 units/second).

**Athletics attribute:** Trainable skill that directly modifies movement speed. Gained through physical activity and skill point allocation.

## Access Control

Door permissions are checked during pathfinding via `Grid.HasPermission()`:

1. Check if cell has an access door (`Grid.HasAccessDoor[cell]`)
2. Determine travel direction from source to destination cell
3. Check door orientation (vertical doors check left/right, horizontal doors check up/down)
4. Verify the duplicant's ID against the door's permission list

**Permission types:** Both (bidirectional), GoLeft, GoRight, Neither (fully locked).

Robots use a separate permission tag (`GameTags.Robot`) and can have distinct access rules from duplicants.

## Suit Requirements

Suit markers define zones requiring specific equipment:

| Suit Type | Flag | Capability |
|-----------|------|------------|
| Atmo Suit | HasAtmoSuit | Standard protection |
| Jet Suit | HasJetPack | Enables Hover nav type |
| Oxygen Mask | HasOxygenMask | Limited protection |
| Lead Suit | HasLeadSuit | Radiation protection |

When a suit marker is operational, duplicants cannot traverse in the restricted direction without the required suit. The `OnlyTraverseIfUnequipAvailable` flag also requires a locker on the far side to store the suit when leaving the zone.

## Jet Suit Mechanics

Jet suits consume fuel at 0.2 kg/s while hovering. When fuel reaches 0:
- Navigator is forced back to Floor nav type
- `GameTags.JetSuitOutOfFuel` is applied
- Duplicant falls and must walk to a jet suit dock

Hover transitions cost 8 (straight) or 9 (diagonal), making jet suit travel faster than walking (cost 10-14) but slower than tubes (cost 5).

## Falling Duplicants

When a duplicant loses ground support:
- `FallMonitor` detects the unsupported state
- `GravityComponent` applies falling physics with accumulating velocity
- On landing, recovery checks for nearby ladders or poles within 1-2 cells
- If extremely stuck, the system tracks up to 3 previous safe positions for recovery pathfinding

Fall damage is handled through the health/damage system on landing impact.
