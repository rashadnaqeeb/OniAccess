# Passability and Walkability

How ONI decides whether a duplicant (or critter) can move through a cell, and whether they can stand on top of it. Derived from decompiled source: `Grid.cs`, `NavTableValidator.cs`, `GameNavGrids.cs`, `SimCellOccupier.cs`, `FakeFloorAdder.cs`, `Door.cs`, and various building configs.

## The BuildMask

Every cell in the world grid has a one-byte bitmask called `BuildMasks`. The navigation system reads this mask to decide what can go where. The flags are:

- **Solid** (0x01) -- The cell contains solid material. Set by natural terrain and by buildings that replace the cell's element (tiles, bunker tiles, etc.).
- **Foundation** (0x02) -- The cell is a constructed foundation tile or door. Foundation cells cannot be dug. Doors and built tiles set this.
- **Door** (0x04) -- The cell contains a door.
- **DupePassable** (0x08) -- Dupes can walk through this cell even though it is solid. Used by mesh tiles and gas permeable membranes.
- **DupeImpassable** (0x10) -- Dupes are explicitly blocked from entering this cell even though it is not solid. Used by locked internal doors.
- **CritterImpassable** (0x20) -- Critters cannot enter this cell. Used by doors that are not fully open.
- **FakeFloor** (0xC0) -- A 2-bit reference counter (values 0 through 3). When nonzero, the cell acts as walkable ground for the cell above it, without being solid itself. Gas and liquid pass through freely.

## How passability is checked

`NavTableValidator.IsCellPassable(cell, is_dupe)` determines whether a given entity can occupy a cell. It first strips out `FakeFloor`, `Foundation`, and `Door` from the mask -- these flags describe structural properties, not passability -- then examines what remains:

- If nothing remains, the cell is passable.
- If `DupeImpassable` is set, the cell is impassable for dupes regardless of anything else.
- If `Solid` is set, the cell is passable for dupes only if `DupePassable` is also set (mesh tiles, gas permeable membranes). Otherwise it is blocked.
- For critters: any `Solid` or `CritterImpassable` flag means the cell is impassable.

## How walkability is checked

Passability answers "can this entity enter the cell?" Walkability answers "can this entity stand here?" -- meaning is there valid ground underfoot?

`FloorValidator.IsWalkableCell(cell, anchor_cell, is_dupe)` checks both the cell the entity wants to stand in and the anchor cell (one cell below):

1. Is the cell itself passable? If not, the entity cannot stand there.
2. Is the anchor cell marked `FakeFloor`? If yes, walkable. This is how the pitcher pump creates a floor surface without being solid.
3. Is the anchor cell `Solid` and NOT `DupePassable`? If yes, walkable. This is natural ground and built tiles.
4. If the anchor cell is `Solid` AND `DupePassable` (mesh tile), it is NOT walkable ground -- dupes fall through it.
5. For dupes only: if neither solid nor fake floor, check for ladders and poles in the cell or anchor cell.

This means a mesh tile is a special case. A dupe can pass through it (it is passable), but it does not count as ground (it is not walkable from above). In practice mesh tiles do provide a floor because of how the game sets them up -- see the section on solid-but-passable buildings below.

## Building categories by passability

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
- **Gantry** -- offsets `(0,1)` through `(3,1)`. The `Gantry` component toggles `SetFloor()` at runtime: extended = walkable, retracted = not walkable. This lets dupes walk across the gantry arm only when it is extended.
- **Pioneer Module**, **Orbital Cargo Module**, **Habitat Module Small** -- offsets at y-1 (3 cells). Activated when landed, providing ground below the module.
- **Habitat Module Medium** -- 5 cells at y-1, same pattern.

### Doors (dynamic passability)

Doors are the only buildings that manipulate `DupeImpassable` and `CritterImpassable` at runtime. The `Door` component updates the cell's build flags whenever the door's control state changes (open, locked, auto).

Internal doors (`DoorType.Internal`):
- `DupeImpassable = true` when locked. Dupes cannot enter.
- `CritterImpassable = true` when not fully open. Critters are blocked by any door that is not in the open state.

Pressure doors, manual pressure doors, bunker doors, and sealed doors (`DoorType.ManualPressure`, `Pressure`, `Sealed`):
- Use `Game.SetDupePassableSolid(cell, passable, solid)` to toggle between solid-and-impassable (closed) and passable (open). This sets `Grid.DupePassable` and modifies sim solidity directly.
- `CritterImpassable = true` when not fully open, same as internal doors.

All doors set `IsFoundation = true` on their `BuildingDef`, so they also mark cells with the `Foundation` flag. This means the cell cannot be dug even when the door is open.

## Non-building passability

Natural terrain follows the same rules but without any building flags. A natural solid cell (dirt, granite, etc.) is `Solid` in the sim layer, blocks movement, and provides walkable ground above. It can be dug because it is NOT `Foundation`. Vacuum cells, gas cells, and liquid cells are passable but not walkable ground (dupes fall through unless there is a ladder, pole, or fake floor).

Ladders and poles do not use BuildMask flags at all. They set `NavValidatorFlags.Ladder` or `NavValidatorFlags.Pole` in a separate `NavValidatorMasks` array. The floor validator checks these as a last resort when neither solid ground nor fake floor is available.

## Summary of the pitcher pump question

The pitcher pump is 2 tiles wide and 4 tiles tall. Its bottom 2 cells use `FakeFloorAdder`, not `Foundation` or `Solid`. This means:

- Dupes can walk on top of the pump's base row (the cell above the fake floor is walkable).
- The pump's cells are not solid. Gas and liquid flow through them.
- The pump is NOT registered in the foundation layer (`ObjectLayer.FoundationTile`). It lives in `ObjectLayer.Building` like most non-tile buildings.
- A dupe can also walk through the pump's cells because they are passable.

Your sighted friend's description of "the bottom 2 tiles act as flooring" is accurate in the navigational sense -- dupes treat them as ground -- but they are fundamentally different from actual floor tiles. They provide a walking surface without providing a physical barrier.
