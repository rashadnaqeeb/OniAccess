# Cluster Map

The Spaced Out DLC cluster map is a hexagonal grid of cells where asteroids, rockets, and space POIs exist. Derived from decompiled source (`ClusterGrid`, `ClusterManager`, `ClusterLayout`, `WorldContainer`, `Clustercraft`, `ClusterTraveler`, `ClusterFogOfWarManager`, `ClusterTelescope`, `TUNING.ROCKETRY`, `TUNING.FIXEDTRAITS`, `ProcGenGame.Cluster`).

## Hex Grid Geometry

The cluster map uses axial coordinates (`AxialI` with Q and R components) on a hexagonal grid. The grid is generated from a `numRings` parameter: all cells where `max(|x|, |y|, |z|) < numRings` in cube coordinates (where `x + y + z = 0`) are valid.

Default `numRings` is 12 for Spaced Out clusters (`ClusterLayout` constructor), stored as `m_numRings` in `ClusterManager` (default 9 for vanilla fallback). Total valid cells for a grid with N rings = `3N^2 - 3N + 1` (so 12 rings = 397 cells).

Distance between two cells uses cube coordinate max: `max(|x1-x2|, |y1-y2|, |z1-z2|)`. The constant `NodeDistanceScale = 600f` maps one hex step to 600 game distance units.

The starting asteroid is always placed at `AxialI.ZERO` (the center of the grid).

## Entity Layers

Each hex cell holds a list of `ClusterGridEntity` objects. Entities are categorized by layer:

| Layer | Description |
|-------|-------------|
| Asteroid | Asteroid worlds |
| Craft | Rockets (Clustercraft) |
| POI | Harvestable space POIs, artifact stations |
| Telescope | Telescope target markers |
| Payload | Railgun payloads, escape pods |
| FX | Visual effects |
| Meteor | Cluster map meteor showers |
| Debri | Rocket debris |

Asteroids cannot share hex cells. Rockets in the same hex are visually spaced out in a spiral pattern (`SpaceOutInSameHex`). Pathing routes around cells containing asteroids (unless the cell is the start or destination).

## Fog of War

Each hex cell has a reveal state tracked by `ClusterFogOfWarManager`:

| Level | Condition | Meaning |
|-------|-----------|---------|
| Hidden | 0 reveal points | Not visible at all |
| Peeked | > 0 but < 100 points | Silhouette visible, POIs show as outlines |
| Visible | >= 100 points | Fully revealed |

`POINTS_TO_REVEAL = 100f` (`TUNING.ROCKETRY.CLUSTER_FOW`).

### How cells are revealed

- **Starting asteroid**: The starting world's cell and all cells within radius 1 are revealed at game start. For non-Spaced Out mode, radius 2 is used instead.
- **Discovering an asteroid**: When an asteroid is at or adjacent to a revealed cell, the reveal radius is forced to at least 1 (so the asteroid and all its neighbors become visible).
- **Telescope**: A duplicant working a telescope earns reveal points over time. Rate = `POINTS_TO_REVEAL / DEFAULT_CYCLES_PER_REVEAL / 600f` per second of work. With `DEFAULT_CYCLES_PER_REVEAL = 0.5f`, that is `100 / 0.5 / 600 = 0.333` points per second, so one cycle of continuous work reveals ~200 points (two cells). Telescope range is configurable per building; `ClusterTelescopeConfig` uses the default `analyzeClusterRadius = 3` (3 hexes), while `ClusterTelescopeEnclosedConfig` sets `analyzeClusterRadius = 4` (4 hexes).
- **Rockets**: Rockets with `revealsFogOfWarAsItTravels = true` force-reveal each cell they enter as they fly.
- **Peeking**: When a cell is revealed, all cells within `AUTOMATIC_PEEK_RADIUS = 2` are peeked (set to 0.01 reveal points). Peeked cells show POI silhouettes (POIs have `IsVisibleInFOW = ClusterRevealLevel.Peeked`).

## World Placement During Generation

`Cluster.AssignClusterLocations()` places worlds onto the hex grid using these rules:

1. **Starting world** is placed at `AxialI.ZERO`.
2. Each subsequent world has `WorldPlacement.allowedRings` (min/max ring from center) and a `buffer` distance from other worlds (default 2).
3. Worlds are sorted by `LocationType` priority: `Startworld` (1) > `InnerCluster` (2) > `Cluster` (3).
4. For each world, candidate cells are filtered to exclude already-assigned cells, cells within the buffer distance of any placed world, and forbidden cells.
5. A random valid cell is chosen from candidates. If none exist, the buffer is reduced to 2 as a fallback.

### Space POI Placement

POIs are placed after worlds. Each `SpaceMapPOIPlacement` specifies:
- `allowedRings`: min/max ring distance from center
- `numToSpawn`: how many to place
- `avoidClumping`: if false, 50% chance to try placing adjacent to existing POIs (up to 3 consecutive clumped placements)
- POIs maintain a minimum 3-hex forbidden radius from each other
- POIs maintain a 2-hex avoidance radius from any world

## World Properties

Each asteroid world (`WorldContainer`) has fixed environmental traits:

### Sunlight Levels

Base value: `DEFAULT_SPACED_OUT_SUNLIGHT = 40000` lux.

| Trait | Lux | Multiplier |
|-------|-----|------------|
| None | 0 | 0x |
| Very Very Low | 10,000 | 0.25x |
| Very Low | 20,000 | 0.5x |
| Low | 30,000 | 0.75x |
| Med Low | 35,000 | 0.875x |
| Med | 40,000 | 1x |
| Med High | 50,000 | 1.25x |
| High | 60,000 | 1.5x |
| Very High | 80,000 | 2x (default) |
| Very Very High | 100,000 | 2.5x |
| Very Very Very High | 120,000 | 3x |

### Cosmic Radiation Levels

Base value: `BASELINE = 250` rads.

| Trait | Rads | Multiplier |
|-------|------|------------|
| None | 0 | 0x |
| Very Very Low | 62 | 0.25x |
| Very Low | 125 | 0.5x |
| Low | 187 | 0.75x |
| Med Low | 218 | 0.875x |
| Med | 250 | 1x (default) |
| Med High | 312 | 1.25x |
| High | 375 | 1.5x |
| Very High | 500 | 2x |
| Very Very High | 750 | 3x |

### Other Fixed Traits

- **Northern Lights**: binary (none or enabled)
- **Large Impactor Fragments**: binary (none or allowed)

### World Traits (Randomized)

Each world can have 2-4 random traits by default (`WorldTraitRules` default = `new TraitRule(2, 4)`). World traits can:
- Add or modify element band distributions (`ElementBandModifier` with `massMultiplier` and `bandMultiplier`)
- Add additional subworlds and biomes
- Add or remove template spawn rules
- Modify global features (geysers, etc.) via `globalFeatureMods`, scaled by the world's `worldTraitScale`
- Be mutually exclusive via `exclusiveWith` and `exclusiveWithTags`
- Worlds can disable traits entirely via `disableWorldTraits = true`

A trait is valid for a world only if its `globalFeatureMods` (scaled by `worldTraitScale`) produce at least one non-zero feature modification.

## Cluster Layout Categories

Each cluster layout has a `ClusterCategory`:

| Category | Description |
|----------|-------------|
| Vanilla | Base game style, single world |
| SpacedOutVanillaStyle | Spaced Out with classic feel |
| SpacedOutStyle | Full Spaced Out cluster |
| Special | Special configurations |

Cluster layouts also have a `difficulty` rating and a `startWorldIndex` indicating which world in the `worldPlacements` list is the starting asteroid.

## World Categories

Individual worlds are categorized as `WorldCategory.Asteroid` (default) or `WorldCategory.Moon`.

## Rocket Travel

### Speed Calculation

Rocket speed = `(EnginePower / TotalBurden) * PilotSkillMultiplier * [modifiers]`

Where:
- `EnginePower` and `TotalBurden` are summed from all modules' `performanceStats`
- `PilotSkillMultiplier` starts at 1.0 and is modified by the pilot's skill level

Speed modifiers:
- **Dupe pilot + robo pilot**: speed * 1.5x
- **No pilot, has passenger module**: speed * 0.5x (autopilot)
- **No pilot, no passenger module**: speed = 0 (cannot travel)
- **Mission Control boost**: adds 20% of base speed when `controlStationBuffTimeRemaining > 0`

### Hex Traversal

Rockets move by accumulating `movePotential` each tick (`dt * speed`). When `movePotential >= 600f`, the rocket advances one hex and `movePotential` resets to 0. One hex = 600 distance units.

When arriving at an asteroid's adjacent cell (1 hex away from destination asteroid) and `quickTravelToAsteroidIfInOrbit = true`, the rocket skips the final hex instantly.

### Fuel Consumption

Fuel is burned once per hex traversal via `BurnFuelForTravel()`. Each engine has a `FuelKilogramPerDistance` stat. Fuel consumed per hex = `FuelKilogramPerDistance * 600f`.

For engines requiring oxidizer, the effective burnable mass is `min(fuelRemaining, oxidizerPowerRemaining)`. Oxidizer efficiency varies:
- Fertilizer: 1.0x (very low)
- Oxylite: 2.0x (low)
- Liquid Oxygen: 4.0x (high)

### Range Calculation

`Range = BurnableMassRemaining / FuelKilogramPerDistance`
`RangeInTiles = floor((Range + 0.001) / 600)`

For robo-piloted rockets, range is further limited by available data banks.

### Pathfinding

`ClusterGrid.GetPath()` uses BFS (breadth-first search) to find the shortest path between two hexes. Cells containing visible asteroids are treated as impassable (except start and destination). If `canNavigateFogOfWar` is false, unrevealed cells are also impassable.

Requirements for landing on an asteroid:
- `requireLaunchPadOnAsteroidDestination`: a launch pad must exist at the destination
- `requireAsteroidDestination`: destination must be an asteroid cell

### Craft Status States

| Status | Meaning |
|--------|---------|
| Grounded | On a launch pad |
| Launching | Taking off |
| InFlight | Traveling through space |
| Landing | Descending to a pad |

## Teleporters

Teleportal Pads (`TeleportalPad` + `Teleporter`) allow instant transport between worlds. They are paired by a 4-bit ID set through logic inputs. A teleporter finds a valid target by matching ID across all active teleporters on any world. When active, they continuously teleport pickupable objects (including duplicants) within their building footprint to the paired teleporter's location. Teleporters only work when both pads are operational.

## Rocket Interiors

Rocket interior worlds are 32x32 tiles (`ROCKET_INTERIOR_SIZE`). They are dynamically created on the main simulation grid using `Grid.GetFreeGridSpace()` and assigned a world ID. Maximum 255 worlds can exist simultaneously (world IDs stored as bytes). The game warns at 255 worlds.

Interior worlds are fully enclosed (`fullyEnclosedBorder = true`), use `SubWorld.ZoneType.RocketInterior`, and are automatically discovered. When a rocket is destroyed in flight, duplicants are ejected in escape pods that travel to the nearest visible asteroid.

## Temporal Tear

The Temporal Tear is managed by `ClusterPOIManager`. It:
- Occupies a hex cell and can be revealed via `RevealTemporalTear()` (reveals location + radius 1)
- Can be opened, which starts the `TemporalTearMeteorShowers` season on the opener's world
- Can consume a rocket (victory condition check via `HasConsumedCraft()`)

## Meteor Showers on the Cluster Map

Meteor showers exist as `ClusterMapMeteorShower` entities on the `EntityLayer.Meteor` layer. Telescopes can identify unidentified meteor showers within their `analyzeClusterRadius` (3 hexes for standard, 4 for enclosed). Identification takes 20 seconds of continuous telescope work.

## Discovery and Visibility States

Worlds track multiple visibility states:

| State | Meaning |
|-------|---------|
| `IsDiscovered` | World is known to exist (hex revealed on cluster map) |
| `IsDupeVisited` | A duplicant has physically been to this world |
| `IsRoverVisited` | A rover has landed on this world |
| `IsSurfaceRevealed` | Surface layer cells are visible in-game |

The starting world begins as discovered, dupe-visited, and active. Other worlds start undiscovered. When a duplicant migrates to a world, it is automatically marked as both discovered and dupe-visited. Discovery timestamps are recorded for UI sorting (worlds sorted by discovery order in the planet sidebar).

## Location Description

`ClusterGrid.GetLocationDescription()` determines what to display for a hex cell, in priority order:
1. **Asteroid** at cell: show asteroid name and world type
2. **POI** at cell: show POI name
3. **Asteroid adjacent** to cell: show "Orbit of {Name}" and world type
4. **Revealed empty**: show "Empty Space"
5. **Unrevealed**: show "Fog of War"
