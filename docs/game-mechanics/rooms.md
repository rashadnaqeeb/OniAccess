# Rooms

How the game detects enclosed rooms, classifies them by type, and applies bonuses. Derived from decompiled source (`RoomProber.cs`, `RoomTypes.cs`, `RoomConstraints.cs`, `RoomType.cs`, `Room.cs`, `CavityInfo.cs`).

## Room Detection

### Cavity Flood Fill

`RoomProber` detects rooms by flood-filling from non-solid cells. A cell is a cavity boundary if it is solid, a foundation, or contains a door. The flood fill runs every 1000ms (`Sim1000ms`) when any solid change or building change has occurred.

The fill produces a `CavityInfo` containing:
- The list of all cells in the cavity
- Bounding box (`minX`, `minY`, `maxX`, `maxY`)
- Lists of buildings, plants, creatures, eggs, fish, and other entities within the cavity
- `NumCells` = total cell count

A cavity becomes a `Room` only if `NumCells <= maxRoomSize`. The `maxRoomSize` value is loaded from `TuningData<RoomProber.Tuning>` at runtime (typically 256 tiles based on game data files). Cavities exceeding this limit are tracked but never assigned a room type.

### Flood Fill Direction

The flood fill expands in four cardinal directions (left, right, up, down) from each cell. Diagonal cells are not considered neighbors. This means a 1-tile diagonal gap between two solid tiles does not connect two cavities -- each side remains a separate cavity.

### Dirty Cell Propagation

When a solid change occurs at a cell, `RoomProber` marks that cell and its four cardinal neighbors as dirty. All cavities containing any dirty cell are condemned (destroyed), and their cells are re-flood-filled to build new cavities. This means a single tile change can split one cavity into multiple cavities or merge adjacent cavities into one.

### Entity Detection Within Cavities

After flood-filling, the prober scans each cell in new cavities that are within the `maxRoomSize` limit. For each cell, it checks object layer 1 (buildings layer) for a `KPrefabID`:
- If the entity has the `RoomProberBuilding` tag, it is added to the cavity's `buildings` list. All standard buildings get this tag automatically via `BuildingConfigManager`.
- If the entity has the `Plant` tag (and not `RoomProberBuilding`), it is added to the `plants` list.
- Multi-cell buildings are deduplicated by instance ID so they appear only once in the list regardless of how many cells they occupy.

Creatures, eggs, and fish are tracked separately through `OvercrowdingMonitor.AddToCavity()` calls when entities enter a cavity, not from the flood fill scan. Other entities are added via `CavityInfo.AddEntity()`.

### Boundary Rules

A cell is a cavity boundary (`IsCavityBoundary`) when:
- The cell has `Solid` or `Foundation` build flags, OR
- The cell contains a door (`Grid.HasDoor`)

Doors act as walls for room detection purposes. This means a single enclosed space divided by a door counts as two separate cavities.

### What Does Not Count as a Boundary

- Mesh tiles and airflow tiles: these have the `Foundation` build flag set, so they ARE boundaries despite allowing gas/liquid flow. A room enclosed by mesh tiles is still a valid room.
- Pneumatic doors, manual airlocks, and mechanized airlocks all set `Grid.HasDoor`, so they all act as boundaries.
- Open space (vacuum or gas-filled cells without solid or foundation flags) is NOT a boundary. An enclosure with a gap in its walls is not a room -- the flood fill leaks through the gap, merging the interior with the exterior into one large cavity that typically exceeds `maxRoomSize`.

### Room Overlap Rules

Each cell belongs to exactly one cavity. Two rooms cannot share interior space. If two enclosed areas share a wall, the wall cells are boundaries and belong to neither room's interior. The rooms are entirely separate.

If a wall between two rooms is removed, the two cavities merge into one. The merged cavity is re-evaluated as a single room. If the combined cell count exceeds the room type's maximum size constraint (or `maxRoomSize`), the room becomes Neutral or is not assigned a type at all.

### Room Type Assignment

When a cavity qualifies as a room, `RoomTypes.GetRoomType()` iterates all registered room types (excluding `Neutral`) and returns the first type where:
1. `isSatisfactory(room)` returns `all_satisfied`
2. No ambiguity exists with another room type

If no type matches, the room is classified as `Neutral` (Miscellaneous Room).

### Ambiguity Resolution

When multiple room types are fully satisfied, `HasAmbiguousRoomType` resolves conflicts using:
1. **Priority** -- if the competing type has higher `priority`, the suspected type is ambiguous (room becomes Neutral)
2. **Upgrade paths (forward)** -- if the suspected type lists the competing type as an upgrade, the suspected type is ambiguous (forces player to commit to the upgrade)
3. **Upgrade paths (reverse)** -- if the competing type lists the suspected type as its upgrade, the suspected type is NOT ambiguous (the upgrade wins cleanly)
4. **Stomp-in-conflict** -- constraints can declare which other constraints they override via `stomp_in_conflict` lists (e.g., `DINING_TABLE` stomps `REC_BUILDING`, `MESS_STATION_SINGLE`, and `MULTI_MINION_DINING_TABLE`)

When only one type is fully satisfied but both have their primary constraint satisfied, ambiguity is still checked. The stomp-in-conflict and upgrade path rules apply the same way, but if neither resolves the conflict, the types are considered ambiguous (room becomes Neutral). This prevents partial matches from silently blocking a room classification.

A room that satisfies two conflicting types with no resolution becomes `Neutral` until the player removes the conflicting building.

### Room Invalidation and Re-evaluation

Rooms are not persistent objects. When any solid cell changes or any building is added/removed in a cavity, the entire cavity is condemned and rebuilt from scratch:

1. The old `CavityInfo` is freed and its `Room` (if any) is cleared via `ClearRoom()`, which unassigns all buildings from the room and removes the room from the assignment manager.
2. New cavities are created by re-flood-filling all affected cells.
3. New cavities that fall within `maxRoomSize` are assigned room types via `GetRoomType()`.
4. All buildings and plants in the new cavities receive a room-changed event (hash `144050788`), which triggers `RoomTracker` components to update their status.

This means a room can lose its type and bonuses instantly when:
- A required building is deconstructed (e.g., removing the last toilet from a Latrine)
- An incompatible building is placed (e.g., placing industrial machinery in a Barracks)
- The room grows too large (breaking a wall to merge with adjacent space)
- The room shrinks below minimum size
- The enclosure is breached (removing a wall tile, causing the cavity to leak into open space)

When a room loses its type, it reverts to Neutral. Buildings with `RoomTracker` components that require a specific room type display a status warning ("Not in required room" or "Not in recommended room") and may stop functioning. For example, a Farm Station outside a Greenhouse stops providing its crop growth bonus.

### UpdateRoom vs Full Refresh

`RoomProber.UpdateRoom()` can be called on a specific cavity to re-evaluate its room type without a full solid-change rebuild. This is used when an entity changes within the room (e.g., a creature enters or leaves) that doesn't affect the cavity boundaries but might affect room constraint satisfaction. It clears the old room, creates a new one, and re-triggers all building events.

## Room Type Identification

`RoomType.isSatisfactory()` returns one of three results:
- `all_satisfied` -- primary constraint and all additional constraints met
- `primary_satisfied` -- primary constraint met but one or more additional constraints failed
- `primary_unsatisfied` -- primary constraint not met (room cannot be this type at all)

The primary constraint is the building that defines the room's identity (e.g., a bed for Barracks, a toilet for Latrine). Additional constraints are secondary requirements (size, no industrial machinery, decor items, etc.).

## Size Constraints

Six size constraints are used across room types:

| Constraint | Cell Count |
|---|---|
| `MINIMUM_SIZE_12` | >= 12 |
| `MINIMUM_SIZE_24` | >= 24 |
| `MINIMUM_SIZE_32` | >= 32 |
| `MAXIMUM_SIZE_64` | <= 64 |
| `MAXIMUM_SIZE_96` | <= 96 |
| `MAXIMUM_SIZE_120` | <= 120 |

Size is measured in total cavity cells (`CavityInfo.NumCells`), not bounding box area.

### Ceiling Height

Two height constraints exist, measured as `1 + maxY - minY` (bounding box height):
- `CEILING_HEIGHT_4`: height >= 4 tiles
- `CEILING_HEIGHT_6`: height >= 6 tiles (defined but not used by any standard room type)

## Room Types Reference

### Registration Order

`GetRoomType()` returns the first fully-satisfied, non-ambiguous type it finds while iterating the registry in insertion order. The registration order in `RoomTypes` constructor is:

1. Neutral, 2. Washroom, 3. Latrine, 4. Private Bedroom, 5. Luxury Barracks, 6. Barracks, 7. Banquet Hall, 8. Great Hall, 9. Mess Hall, 10. Kitchen, 11. Massage Clinic, 12. Hospital, 13. Power Plant, 14. Greenhouse, 15. Stable, 16. Laboratory, 17. Recreation Room, 18. Nature Reserve, 19. Park

Upgraded types are registered before their base types (e.g., Washroom before Latrine, Private Bedroom before Barracks). This ensures that when a room satisfies both a base type and its upgrade, the upgrade is found first and returned without needing to rely solely on the ambiguity resolution rules.

Each room type also has a `sortKey` for UI display ordering, which is separate from registration order and priority.

### Bathroom Category

#### Latrine
- **Primary constraint:** Toilet (Outhouse or Lavatory, tag `ToiletType`)
- **Additional constraints:** Wash station (Wash Basin, Sink, Hand Sanitizer, or Shower), no industrial machinery, 12-64 tiles
- **Effect:** Morale bonus (+1)
- **Upgrade path:** Washroom
- **Priority:** 1

#### Washroom
- **Primary constraint:** Flush toilet (Lavatory, tag `FlushToiletType`)
- **Additional constraints:** Plumbed wash station (Sink, Hand Sanitizer, or Shower -- not Wash Basin), no Outhouses, no industrial machinery, 12-64 tiles
- **Effect:** Morale bonus (+2)
- **Upgrade path:** None (top of chain)
- **Priority:** 1

### Sleep Category

#### Barracks
- **Primary constraint:** Bed (Cot or Comfy Bed, tag `BedType`, excluding Clinic beds)
- **Additional constraints:** No industrial machinery, 12-64 tiles
- **Effect:** Morale bonus (+1)
- **Upgrade paths:** Luxury Barracks, Private Bedroom
- **Priority:** 1

#### Luxury Barracks (internal ID: Bedroom)
- **Primary constraint:** Comfy Bed (tag `LuxuryBedType`)
- **Additional constraints:** No Cots, no industrial machinery, 12-64 tiles, 1 decor item, ceiling height 4
- **Effect:** Morale bonus (+2)
- **Upgrade path:** Private Bedroom
- **Priority:** 1

#### Private Bedroom
- **Primary constraint:** Single Comfy Bed (exactly one `LuxuryBedType` in the room)
- **Additional constraints:** No Cots, 24-64 tiles, ceiling height 4, 2 decor items, no industrial machinery, fully backwalled
- **Effect:** Morale bonus (+3)
- **Upgrade path:** None (top of chain)
- **Priority:** 1
- **Backwall rule:** Every cell in the cavity that belongs to the room must have a backwall tile placed behind it. Tiles under construction do not count.

### Food Category

#### Mess Hall
- **Primary constraint:** Dining table (Mess Table or Communal Table, tag `DiningTableType`)
- **Additional constraints:** No industrial machinery, 12-64 tiles
- **Effect:** Morale bonus (+3)
- **Upgrade paths:** Great Hall, Banquet Hall
- **Priority:** 1

#### Great Hall
- **Primary constraint:** Dining table (tag `DiningTableType`)
- **Additional constraints:** No industrial machinery, 32-120 tiles, 1 decor item, 1 recreational building
- **Effect:** Morale bonus (+6)
- **Upgrade path:** Banquet Hall
- **Priority:** 1
- **Note:** The `DINING_TABLE` constraint has `stomp_in_conflict` entries for `REC_BUILDING`, `MESS_STATION_SINGLE`, and `MULTI_MINION_DINING_TABLE`, preventing ambiguity with Rec Room when both a dining table and rec building are present.

#### Banquet Hall
- **Primary constraint:** Communal Table (`MultiMinionDiningTable` or `MultiMinionDiningSeat`)
- **Additional constraints:** No industrial machinery, 32-120 tiles, 1 decor item, 1 recreational building, 1 displayed ornament, no basic Mess Tables
- **Effect:** Morale bonus (+8)
- **Upgrade path:** None (top of chain)
- **Priority:** 1
- **Note:** The `MULTI_MINION_DINING_TABLE` primary constraint has `stomp_in_conflict` entries for `REC_BUILDING` and `DINING_TABLE`, preventing ambiguity with Rec Room and Great Hall.

#### Kitchen
- **Primary constraint:** Spice Grinder (tag `SpiceStation`)
- **Additional constraints:** Cooking station (Electric Grill or Gas Range), Refrigerator, no mess stations (buildings tagged `MessTable`), 12-96 tiles
- **Effect:** Enables Spice Grinder use (functional, no morale bonus)
- **Upgrade path:** None
- **Priority:** 1

### Medical Category

#### Hospital
- **Primary constraint:** Medical equipment (Sick Bay or Disease Clinic, tag `Clinic`)
- **Additional constraints:** Toilet, Mess Table (single), no industrial machinery, 12-96 tiles
- **Effect:** Quarantines sick Duplicants, reduces disease spread
- **Upgrade path:** None
- **Priority:** 2, single assignee, priority building use
- **Note:** The `CLINIC` primary constraint has `stomp_in_conflict` entries for `TOILET`, `FLUSH_TOILET`, and `MESS_STATION_SINGLE`. The `MESS_STATION_SINGLE` additional constraint also stomps `REC_BUILDING` and `DINING_TABLE`. Together these prevent ambiguity with Latrine, Washroom, Mess Hall, and Rec Room.

#### Massage Clinic
- **Primary constraint:** Massage Table (exact prefab match)
- **Additional constraints:** No industrial machinery, 1 decor item, 12-64 tiles
- **Effect:** Stress reduction bonus (functional, not morale)
- **Upgrade path:** None
- **Priority:** 2, single assignee, priority building use

### Industrial Category

#### Power Plant
- **Primary constraint:** Heavy-Duty Generator (tag `HeavyDutyGeneratorType`) plus room criteria requiring at least 2 power buildings total
- **Additional constraints:** 12-120 tiles
- **Effect:** Enables Power Control Station tune-ups on heavy-duty generators
- **Upgrade path:** None
- **Priority:** 2, single assignee, priority building use
- **Note:** No industrial machinery restriction. This is intentionally an industrial room.

#### Machine Shop (not registered)
- Defined in code but **not added** to the room type registry (`Add()` is not called). The `MachineShop` field is assigned directly without `Add()`, so this room type cannot be detected in-game through the standard room system.

### Agricultural Category

#### Greenhouse (internal ID: Farm)
- **Primary constraint:** Farm Station (tag `FarmStationType`)
- **Additional constraints:** 12-96 tiles
- **Effect:** Enables Farm Station fertilizer for crop growth speed bonus
- **Upgrade path:** None
- **Priority:** 2, single assignee, priority building use

#### Stable (internal ID: CreaturePen)
- **Primary constraint:** Ranching building (Grooming Station, Shearing Station, Critter Condo, Critter Fountain, or Milking Station, tag `RanchStationType`)
- **Additional constraints:** 12-96 tiles
- **Effect:** Enables critter tending for happiness, domestication, and production bonuses
- **Upgrade path:** None
- **Priority:** 2, single assignee, priority building use

### Science Category

#### Laboratory
- **Primary constraint:** 2 science buildings (tag `ScienceBuilding`)
- **Additional constraints:** No industrial machinery, 32-120 tiles
- **Effect:** Science buildings function more efficiently; enables Geotuner and Mission Control Station
- **Upgrade path:** None
- **Priority:** 2, single assignee, priority building use

### Recreation Category

#### Recreation Room
- **Primary constraint:** Recreational building (tag `RecBuilding`)
- **Additional constraints:** No industrial machinery, 1 decor item, 12-96 tiles
- **Effect:** No room effect (null). Recreational buildings provide their own morale bonuses when used during Downtime
- **Upgrade path:** None
- **Priority:** 0 (lowest), single assignee, priority building use

### Park Category

#### Park
- **Primary constraint:** Park Sign (tag `Park`)
- **Additional constraints:** 2 wild plants (non-branch, not replanted), no industrial machinery, 12-64 tiles
- **Effect:** Morale bonus (+3) when passing through
- **Upgrade path:** Nature Reserve
- **Priority:** 1

#### Nature Reserve
- **Primary constraint:** Park Sign (tag `Park`)
- **Additional constraints:** 4 wild plants, no industrial machinery, 32-120 tiles
- **Effect:** Morale bonus (+6) when passing through
- **Upgrade path:** None (top of chain)
- **Priority:** 1

## Upgrade Paths Summary

```
Latrine --> Washroom
Barracks --> Luxury Barracks --> Private Bedroom
Barracks --> Private Bedroom (direct)
Mess Hall --> Great Hall --> Banquet Hall
Mess Hall --> Banquet Hall (direct)
Park --> Nature Reserve
```

When a room satisfies both a base type and its upgrade, the upgrade path relationship prevents ambiguity -- the base type recognizes the upgrade as valid and yields to it rather than creating a conflict.

## Effect Application

Room effects are applied through the `RoomType.TriggerRoomEffects()` method. When a Duplicant interacts with the primary building of a room (the building matching `primary_constraint.building_criteria`), the effect IDs stored in `RoomType.effects` are added to the Duplicant's `Effects` component.

Effect IDs per room type (loaded from game data at runtime):
- Latrine: `"RoomLatrine"`
- Washroom: `"RoomBathroom"`
- Barracks: `"RoomBarracks"`
- Luxury Barracks: `"RoomBedroom"`
- Private Bedroom: `"RoomPrivateBedroom"`
- Mess Hall: `"RoomMessHall"`
- Great Hall: `"RoomGreatHall"`
- Banquet Hall: `"RoomBanquetHall"`
- Park: `"RoomPark"`
- Nature Reserve: `"RoomNatureReserve"`

Rooms without effect IDs (Kitchen, Hospital, Massage Clinic, Power Plant, Greenhouse, Stable, Laboratory, Recreation Room) provide functional bonuses through their buildings' operational state being tied to room membership, not through the Effect system.

### Bonus Stacking

Room morale effects are applied per interaction, not per room. When a Duplicant uses a primary building (e.g., sleeps in a bed, eats at a table), `TriggerRoomEffects()` adds the effect to the Duplicant. The effect has a duration defined in game data and persists as a timed buff.

Because each room type has its own distinct effect ID, bonuses from different room types stack. A Duplicant who sleeps in a Luxury Barracks and eats in a Great Hall receives both `RoomBedroom` and `RoomGreatHall` effects simultaneously.

Within the same upgrade chain, only one effect applies at a time in practice because a room can only be one type. A room cannot be both a Barracks and a Luxury Barracks simultaneously, so a Duplicant sleeping in a Luxury Barracks gets `RoomBedroom` but not `RoomBarracks`. If the room later downgrades (e.g., a decor item is removed), the old effect expires naturally at the end of its duration and new interactions grant the lower-tier effect.

A single room does not grant its bonus multiple times per use. The effect is added once per interaction via `Effects.Add()`, which resets the timer if the effect already exists rather than stacking.

### Building Room Tracking

Buildings that require a specific room type to function use the `RoomTracker` component. This component subscribes to room-changed events and checks whether the building's current room matches its `requiredRoomType`. The requirement level determines the consequence:

- `Required` / `CustomRequired`: building displays a "Not in required room" status and may be non-functional
- `Recommended` / `CustomRecommended`: building displays a "Not in recommended room" status but still operates
- `TrackingOnly`: no status shown, room is tracked for informational purposes only

## Common Constraints

### No Industrial Machinery
Most non-industrial rooms require this. Any building tagged `IndustrialMachinery` disqualifies the room. This includes generators, refineries, and other heavy equipment.

### Wild Plant Detection
Plants count as wild if:
- They have a `ReceptacleMonitor` component with `Replanted == false`, OR
- They are a `BasicForagePlantPlanted` (naturally spawned forage plants)
- Plant branches (tag `PlantBranch`) are excluded from wild plant counts

### Decor Items
Buildings tagged `GameTags.Decoration` count as decor items. These include paintings, sculptures, flower pots, and other purely decorative buildings.

### Displayed Ornament
Requires a building tagged `OrnamentDisplayer` with an `OrnamentReceptacle` component that is operational and holding an item tagged as an `Ornament`. Checked against both buildings and other entities in the room.

### Backwall Requirement

Used only by Private Bedroom. The `IS_BACKWALLED` constraint checks every cell in the room's bounding box that belongs to the room's cavity. For each such cell, object layer 2 (backwall layer) must contain a placed object that is not tagged `UnderConstruction`. The scan iterates columns from both edges inward simultaneously, checking each row top to bottom. A single missing or under-construction backwall tile fails the entire constraint.

### No Cots / No Outhouses / No Basic Mess Tables

These are exclusion constraints that prevent mixing tiers within a room:
- `NO_COTS`: fails if any building has `BedType` tag but NOT `LuxuryBedType` (i.e., a basic Cot is present)
- `NO_OUTHOUSES`: fails if any building has `ToiletType` tag but NOT `FlushToiletType` (i.e., an Outhouse is present)
- `NO_BASIC_MESS_STATIONS`: fails if any building has the exact prefab ID `"DiningTable"` (the basic Mess Table)
- `NO_MESS_STATION`: fails if any building has the `MessTable` tag (used by Kitchen to prevent dining tables)

### Constraint Satisfaction Mechanics

A constraint with `building_criteria` counts how many buildings (and plants) in the room satisfy the criteria. If the count reaches `times_required`, the constraint is satisfied. Most constraints require 1; `SCIENCE_BUILDINGS` requires 2; `BIONICUPKEEP` requires 2.

A constraint with only `room_criteria` (no `building_criteria`) evaluates the room-level lambda directly. Size constraints, height constraints, no-industrial-machinery, and backwall all work this way.

## Room Assignment

Non-primary buildings in a room with a type other than Neutral are automatically assigned to the room via the `Assignable` system. Primary buildings (those matching `primary_constraint.building_criteria`) are excluded from room assignment -- they are assigned to individual Duplicants instead.

Rooms that set `single_assignee = true` (Hospital, Massage Clinic, Power Plant, Greenhouse, Stable, Laboratory, Recreation Room) restrict assignment to one Duplicant at a time. Rooms with `priority_building_use = true` give the assigned Duplicant priority access to the room's primary building.

## Edge Cases

### Minimum Enclosure

There is no minimum cell count enforced by the cavity system itself. A 1-cell enclosed space is a valid cavity. However, all room types require at least 12 cells (`MINIMUM_SIZE_12`), so cavities smaller than 12 cells always classify as Neutral.

### Maximum Cavity Size vs Maximum Room Size

Two separate limits apply:
- `maxRoomSize` (from `TuningData`, typically 256): cavities exceeding this are never assigned a Room object at all. They exist as `CavityInfo` with `room == null`.
- Room type maximum size constraints (64, 96, or 120): these are additional constraints checked during type classification. A cavity of 130 cells gets a Room object (under the 256 limit) but fails all maximum size constraints and becomes Neutral.

### Doors and Room Splitting

Placing a door in the middle of a room splits it into two separate cavities. Each half is independently evaluated for room type. If the original room required specific buildings and one half no longer contains them, that half loses the room type. This is a common way to accidentally break rooms.

### Multi-cell Buildings Spanning Boundaries

A building that occupies multiple cells might straddle a room boundary (e.g., a 3-wide building placed at the edge of a room). The building is added to whichever cavity contains the cell in object layer 1. Each cell is checked independently during entity detection, but the building is deduplicated by instance ID, so it appears in only one cavity's building list (the first cavity whose cell scan encounters it). This can cause unexpected behavior where a building appears to be "in" a room visually but is tracked by a different cavity.

### Open-to-Space Cavities

If a room's wall is breached to open space (vacuum), the flood fill leaks outward. Depending on the surrounding terrain, this can create an enormous cavity that exceeds `maxRoomSize`, destroying the room entirely. The entire connected open space becomes one cavity with no Room object.

### Room Owner Tracking

A room's owners are derived from the owners of its primary buildings. `Room.GetOwners()` iterates all buildings matching `primary_constraint.building_criteria`, checks each for an `Ownable` component with an assignee, and collects the unique `Ownables` from those assignees. A room with multiple primary buildings (e.g., a Barracks with 4 beds) can have multiple owners, one per assigned bed.

### Neutral Room Diagnostic Display

When a room is Neutral, the game's room overlay calls `GetPossibleRoomTypes()` to show which room types the cavity partially satisfies. For each type with at least `primary_satisfied`, it lists the unmet additional constraints in red. If a type is `all_satisfied` but ambiguous with another type, it shows a "type conflict" warning instead of listing missing constraints.
