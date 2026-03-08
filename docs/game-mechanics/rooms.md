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

### Boundary Rules

A cell is a cavity boundary (`IsCavityBoundary`) when:
- The cell has `Solid` or `Foundation` build flags, OR
- The cell contains a door (`Grid.HasDoor`)

Doors act as walls for room detection purposes. This means a single enclosed space divided by a door counts as two separate cavities.

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

## Room Assignment

Non-primary buildings in a room with a type other than Neutral are automatically assigned to the room via the `Assignable` system. Primary buildings (those matching `primary_constraint.building_criteria`) are excluded from room assignment -- they are assigned to individual Duplicants instead.

Rooms that set `single_assignee = true` (Hospital, Massage Clinic, Power Plant, Greenhouse, Stable, Laboratory, Recreation Room) restrict assignment to one Duplicant at a time. Rooms with `priority_building_use = true` give the assigned Duplicant priority access to the room's primary building.
