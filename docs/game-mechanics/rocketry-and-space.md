# Rocketry and Space

How rockets are constructed, fueled, launched, and navigated. Covers both the base game starmap system and the Spaced Out (DLC1) cluster map system. Derived from decompiled source code.

## Two Rocketry Systems

The game has two completely separate rocketry implementations gated by `DlcManager.FeatureClusterSpaceEnabled()`:

- **Base game**: Rockets are vertical stacks of `RocketModule` components managed by `LaunchConditionManager` and `SpacecraftManager`. Space destinations are abstract points at varying distances. Rockets disappear on launch, complete a timed mission, and return.
- **Spaced Out (DLC1)**: Rockets are stacks of `RocketModuleCluster` managed by `CraftModuleInterface` and `Clustercraft`. They exist as entities on a hex-based cluster map and travel in real time between hex cells.

Engine configs use `GetForbiddenDlcIds = EXPANSION1` for base game and `GetRequiredDlcIds = EXPANSION1` for DLC versions.

## Rocket Construction

### Module Stacking

Rockets are vertical stacks of modules connected via `BuildingAttachPoint`/`AttachableBuilding`. The engine sits at the bottom. Each module defines an attachment point at its top edge (e.g., `CellOffset(0, 5)` for a 5-tile-tall module) with tag `GameTags.Rocket`.

**DLC constraints:**
- Each engine defines `maxModules` (max number of modules in stack) and `maxHeight` (max total height in cells)
- `RocketHeight` is the sum of `HeightInCells` for all modules in the stack
- `ConditionRocketHeight` enforces the engine's `maxHeight` limit
- `MAX_MODULE_STACK_HEIGHT = VERY_TALL - 5 = 30` cells (absolute cap)
- Modules can be reordered via `ReorderableBuilding` (DLC only)
- Only one command/habitat module per rocket (`LimitOneCommandModule`)
- Nosecone and small habitat must be on top (`TopOnly`)

### Module Types

**Command/Habitat modules** (one required per rocket):
- Base game: `CommandModule` (5x5, requires `CanUseRockets` skill perk, stores 1 astronaut via `MinionStorage`)
- DLC: `HabitatModuleSmall` (3x3, burden 3, tagged `NoseRocketModule` + `TopOnly`), `HabitatModuleMedium` (5x4, burden 6). Both create interior worlds via `PassengerRocketModule` + `ClustercraftExteriorDoor`
- `RoboPilotCommandModule` (5x5, base game starmap only, requires DLC3, forbidden in Spaced Out) for crewless rockets

**Nosecone** (DLC): `NoseconeBasic` (5x2, burden 2, `TopOnly`), `NoseconeHarvest` (5x4, burden 2, `TopOnly`, for space POI harvesting)

**Robo Pilot Module** (DLC, requires both Expansion1 + DLC3): `RoboPilotModule` (3x4, burden 4). Allows crewless flight in cluster space. Limited to one per rocket (`LimitOneRoboPilotModule`).

**Fuel/Oxidizer tanks**, **Cargo bays**, **Engines** - detailed in sections below.

## Engines

### Base Game Engines

All base game engines use `RocketEngine` and the thrust-vs-mass range system.

| Engine | ID | Fuel | Efficiency | Requires Oxidizer | Exhaust | Exhaust Temp |
|---|---|---|---|---|---|---|
| Steam Engine | `SteamEngine` | Steam | 20 (WEAK) | No | Steam | ~150 C |
| Petroleum Engine | `KeroseneEngine` | Petroleum | 40 (MEDIUM) | Yes | CO2 | default |
| Biodiesel Engine | `BiodieselEngine` | Refined Lipid | 50 (MEDIUM_PLUS) | Yes | CO2 | 1700 K |
| Hydrogen Engine | `HydrogenEngine` | Liquid Hydrogen | 60 (STRONG) | Yes | Steam | 2000 K |

**Solid Booster** (`SolidBooster`): Not a main engine. Uses Iron + Oxylite (400 kg each, 800 kg total storage). Efficiency 30 (BOOSTER). Adds supplemental thrust.

### DLC Engines (Cluster)

All DLC engines use `RocketEngineCluster` and the burden/power/fuel-per-distance system. Most generate power via `ModuleGenerator` while burning (exceptions: CO2 Engine and Radbolt Engine produce 0 W and have no `ModuleGenerator`).

| Engine | ID | Fuel | Oxidizer | Max Modules | Max Height | Burden | Engine Power | Fuel/Distance | Power (W) |
|---|---|---|---|---|---|---|---|---|---|
| CO2 Engine | `CO2Engine` | CO2 | No | 3 | 10 (VERY_SHORT) | 3 | 23 | 0.0278 | 0 |
| Sugar Engine | `SugarEngine` | Sucrose | Yes | 5 | 16 (SHORT) | 1 | 16 | 0.125 | 60 |
| Steam Engine | `SteamEngineCluster` | Steam | No | 6 | 25 (TALL) | 15 | 27 | 0.025 | 600 |
| Small Petroleum | `KeroseneEngineClusterSmall` | Petroleum | Yes | 4 | 20 (MEDIUM) | 5 | 31 | 0.075 | 240 |
| Petroleum Engine | `KeroseneEngineCluster` | Petroleum | Yes | 7 | 35 (VERY_TALL) | 6 | 48 | 0.15 | 480 |
| Biodiesel Engine | `BiodieselEngineCluster` | Refined Lipid | Yes | 7 | 35 (VERY_TALL) | 7 | 31 | 0.1154 | 640 |
| Radbolt Engine | `HEPEngine` | Radbolts (HEP) | No | 4 | 20 (MEDIUM) | 5 | 34 | 0.333 | 0 |
| Hydrogen Engine | `HydrogenEngineCluster` | Liquid Hydrogen | Yes | 7 | 35 (VERY_TALL) | 7 | 55 | 0.09375 | 600 |

The Radbolt Engine exhausts Fallout at 873.15 K (600 C) with radiation disease and emits radiation while burning.

### Engine Exhaust During Launch

Both `RocketEngine` and `RocketEngineCluster` emit exhaust during the burn state:
- Emit mass at `exhaustEmitRate` kg/s of `exhaustElement` at `exhaustTemperature`
- Heat spreads downward through 10 cells below the engine, with temperature decreasing by `1/(distance+1)` on the sides and `1/distance` on center

## Fuel and Range

### Base Game Range Calculation

Range is determined by thrust vs. mass in `RocketStats`:

```
totalThrust = min(fuel, oxidizer) * efficiency * (oxidizerEfficiency/100)   [if requireOxidizer]
            = fuel * efficiency                                              [if no oxidizer]
totalThrust += boosterThrust

massPenalty = max(totalMass, (totalMass / 300)^3.2)
range = max(0, totalThrust - massPenalty)
```

Base game oxidizer efficiencies (in `RocketStats.oxidizerEfficiencies`):
- Oxylite: 1.0 (LOW)
- Liquid Oxygen: 1.33 (HIGH)

### DLC Range Calculation

DLC uses a burden/power system via `RocketModulePerformance`. Each module has:
- **Burden**: Weight contribution (sums across all modules)
- **Engine Power**: Thrust contribution (only engines have non-zero values)
- **Fuel kg per distance**: How much fuel is consumed per unit of travel distance

Range formula in `CraftModuleInterface`:
```
range = burnableMassRemaining / fuelKilogramPerDistance
rangeInHexes = floor(range / 600)
fuelPerHex = fuelKilogramPerDistance * 600
```

Where `burnableMassRemaining`:
- Without oxidizer: total fuel in all `IFuelTank` storages
- With oxidizer: `min(fuelRemaining, oxidizerPowerRemaining)`

If a `RoboPilotModule` is present, range is capped to `min(fuelRange, dataBankRange)`.

### DLC Oxidizer Efficiencies

Oxidizer power = mass * efficiency factor (in `Clustercraft.dlc1OxidizerEfficiencies`):
- Fertilizer: 1.0 (VERY_LOW)
- Oxylite: 2.0 (LOW)
- Liquid Oxygen: 4.0 (HIGH)

### Fuel Tanks (DLC)

| Tank | ID | Capacity | Size | Burden | Input |
|---|---|---|---|---|---|
| Liquid Fuel Tank | `LiquidFuelTankCluster` | 900 kg | 5x5 | 5 (MODERATE_PLUS) | Liquid conduit |
| CO2 Engine (built-in) | `CO2Engine` | 100 kg | - | - | Gas conduit |
| Steam Engine (built-in) | `SteamEngineCluster` | 150 kg | - | - | Gas conduit |
| Sugar Engine (built-in) | `SugarEngine` | 450 kg | - | - | Manual delivery |
| Small Petroleum (built-in) | `KeroseneEngineClusterSmall` | 450 kg | - | - | Liquid conduit |

### Oxidizer Tanks (DLC)

| Tank | ID | Capacity | Accepts | Size | Burden |
|---|---|---|---|---|---|
| Solid Oxidizer Tank (large) | `OxidizerTankCluster` | 900 kg | Oxylite, Fertilizer | 5x5 | 5 (MODERATE_PLUS) |
| Solid Oxidizer Tank (small) | `SmallOxidizerTank` | 450 kg | Oxylite, Fertilizer | 3x2 | 2 (MINOR) |
| Liquid Oxidizer Tank | `OxidizerTankLiquidCluster` | 450 kg | Liquid Oxygen only | 5x2 | 5 (MODERATE_PLUS) |

DLC oxidizer tanks do not consume on landing (`consumeOnLand = false` when cluster space enabled).

## Cargo Bays

### DLC Cargo Bays

Capacity = base value * `CARGO_CAPACITY_SCALE` (10).

| Cargo Bay | ID | Base * Scale | Capacity (kg) | Burden | Size |
|---|---|---|---|---|---|
| Solid (large) | `CargoBayCluster` | 2700 * 10 | 27,000 | 6 (MAJOR) | 5x5 |
| Solid (small) | `SolidCargoBaySmall` | 1200 * 10 | 12,000 | 4 (MODERATE) | 3x3 |
| Liquid (large) | `LiquidCargoBayCluster` | 2700 * 10 | 27,000 | 5 (MODERATE_PLUS) | 5x5 |
| Liquid (small) | `LiquidCargoBaySmall` | 900 * 10 | 9,000 | 3 (MINOR_PLUS) | 3x3 |
| Gas (large) | `GasCargoBayCluster` | 1100 * 10 | 11,000 | 4 (MODERATE) | 5x5 |
| Gas (small) | `GasCargoBaySmall` | 360 * 10 | 3,600 | 2 (MINOR) | 3x3 |
| Critter/Entity | `SpecialCargoBayCluster` | - | - | 1 (INSIGNIFICANT) | 3x1 |

The critter cargo bay (`SpecialCargoBayCluster`) stores one bagable creature at a time via `SpecialCargoBayClusterReceptacle`. It also has a secondary storage for loot/side products.

## Piloting and Speed

### Base Game

The astronaut is stored in the `CommandModule` via `MinionStorage`. Requires the `CanUseRockets` skill perk. The pilot's Space Navigation attribute affects mission duration:
```
missionDuration = distance * 1800 / pilotNavigationEfficiency
```
Where `pilotNavigationEfficiency = 1 + sum(spaceNavigation skill bonuses)`.

Mission Control Station buff: adds 20% mission progress rate while active (`controlStationBuffTimeRemaining > 0`). Each work session sets the buff to 600s (1 cycle).

### DLC Speed

Speed in `Clustercraft` determines how fast the rocket accumulates `movePotential` (600 units = 1 hex):

```
baseSpeed = enginePower / totalBurden
pilotedSpeed = baseSpeed * pilotSkillMultiplier
```

Modifiers (checked in order):
- **Robo active + autopilot active** (robo has data banks > 1 AND dupe is piloting): speed * 1.5
- **Has habitat but autopilot not active** (passenger module exists but dupe is not actively piloting): speed * 0.5
- **No habitat and no robo** (no passenger module AND no robo with data banks > 1): speed = 0 (rocket stops)
- **Mission Control buff**: adds `baseSpeed * 0.2` while active (applied after above multipliers)

Autopilot (`AutoPilotMultiplier > 0.5`) is set when the dupe is actively piloting at a Rocket Control Station.

Robo pilot (`RoboPilotModule`) needs data banks stored (`GetDataBanksStored() > 1`).

### Travel Time

In `ClusterTraveler.Sim200ms`:
```
movePotential += dt * speed
```
When `movePotential >= 600`, the rocket advances one hex and `onTravelCB` fires (which calls `BurnFuelForTravel`). Travel to an adjacent asteroid (1 hex remaining) happens instantly if `quickTravelToAsteroidIfInOrbit = true`.

ETA calculation:
```
ETA = remainingTravelDistance / speed
remainingTravelDistance = (pathNodes - (hasAsteroidDestination ? 1 : 0)) * 600 - movePotential
```

### Mission Control

Two variants: `MissionControl` (base game) and `MissionControlCluster` (DLC).

**Base game**: Targets a `Spacecraft` with state `Underway` that doesn't already have a buff. Sets `controlStationBuffTimeRemaining = 600` (1 cycle). While buffed, mission elapsed time accumulates at 120% rate.

**DLC**: Same buff mechanic but for `Clustercraft`. Additional constraint: the rocket must be within range 2 hexes of the asteroid the Mission Control Station is on (`IsInRange(rocketLocation, stationLocation, 2)`). Cannot boost your own rocket if the station is inside it.

## Robo Pilot Module

The `RoboPilotModule` enables crewless rocket flight. Two variants exist:

**DLC cluster version** (`RoboPilotModule` building, requires Expansion1 + DLC3):
- Storage: 100 kg data banks
- Burden: 4 (MODERATE)
- Consumes 2 data banks per hex traveled (`dataBankConsumption = 2`)
- Data bank range: `storedBanks / 2 * 600` = 300 units per data bank
- Does not consume on landing (`consumeDataBanksOnLand = false`)

**Base game version** (`RoboPilotCommandModule`, requires DLC3, forbidden in Spaced Out):
- Storage: 100 kg data banks
- Consumption: 1 data bank per distance unit
- `consumeDataBanksOnLand = true` - banks consumed when rocket returns
- Consumes `destination.OneBasedDistance * 2` data banks on landing
- Data bank range: `storedBanks / 1 * 5000` = 5000 units per data bank (`DATABANKRANGE = 10000 / 2`)

## Cluster Map (DLC)

### Grid Structure

`ClusterGrid` is a hexagonal grid using axial coordinates (`AxialI`). Generated with a configurable number of rings around center (0,0). The constant `NodeDistanceScale = 600f` defines units per hex.

Hex distance formula (cube coordinates):
```
distance = max(|a.x - b.x|, |a.y - b.y|, |a.z - b.z|)
```

### Entity Layers

Entities on the cluster map have layers:
- `EntityLayer.Asteroid`: Planetoid worlds
- `EntityLayer.Craft`: Rockets (`Clustercraft`)
- `EntityLayer.POI`: Space Points of Interest (harvestable)
- `EntityLayer.Meteor`: Meteor showers

### Fog of War

Managed by `ClusterFogOfWarManager`. Cells start hidden. Revealed by:
- Rockets traveling through cells (`revealsFogOfWarAsItTravels = true`)
- Telescope analysis (`POINTS_TO_REVEAL = 100`, `DEFAULT_CYCLES_PER_REVEAL = 0.5`)
- Peek radius around traveling rockets (`peekRadius = 2`)

### Pathfinding

`ClusterGrid.GetPath` uses breadth-first search on the hex grid:
- Cannot path through cells containing visible asteroids (except start/end)
- Cannot path through fog of war unless `canNavigateFogOfWar = true`
- If destination is an asteroid, requires a `LaunchPad` on it (if `requireLaunchPadOnAsteroidDestination = true`)
- Returns null with a reason string on failure

### Location Descriptions

`GetLocationDescription` resolves what's at a hex cell, in priority order:
1. Asteroid at cell: shows asteroid name and world type
2. POI at cell: shows POI name
3. Asteroid in adjacent cell: shows "Orbit of {Name}" and world type
4. Visible empty: "Empty Space"
5. Hidden: "Fog of War"

### Rocket States

`Clustercraft.CraftStatus`:
- `Grounded`: On a launch pad. Tags: `RocketOnGround`
- `Launching`: Taking off. Tags: `RocketNotOnGround`
- `InFlight`: Traveling or orbiting. Tags: `RocketNotOnGround`, `RocketInSpace`, `EntityInSpace`
- `Landing`: Descending to pad. Tags: `RocketNotOnGround`

Status items while in flight:
- **InFlight**: Actively traveling
- **RocketStranded**: No fuel and no landing pad nearby
- **DestinationOutOfRange**: Not enough fuel to reach destination
- **WaitingToLand**: At destination asteroid, waiting for pad
- **InOrbit**: Stopped at a hex adjacent to an asteroid
- **Piloted/AutoPiloted/SuperPilot/Unpiloted**: Piloting state

### Landing

Landing requires:
1. A `LaunchPad` at the destination asteroid
2. Sufficient ceiling clearance (`PadTopEdgeDistanceToCeilingEdge >= rocketHeight`)
3. Clear flight path (no obstructions)
4. Pad is operational
5. No solid tiles where the rocket body would occupy

The rocket remembers preferred landing pads per world (`preferredLaunchPad` dictionary).

## Space Destinations (Base Game)

Base game space destinations are managed by `SpacecraftManager` and represented by `SpaceDestination`. Each has:
- A type from `SpaceDestinationTypes` (Satellite, MetallicAsteroid, TerraPlanet, etc.)
- A distance value (affects mission duration and thrust required)
- Recoverable elements with random roll values
- Research opportunities (5 base + rare element/item chances)
- A depletable mass that replenishes over time

### Destination Types

The constructor signature is: `SpaceDestinationType(id, parent, name, description, iconSize, spriteName, elementTable, recoverableEntities, artifactDropRate, maxMass, minMass, cyclesToRecover, visitable)`.

| Type | Category (iconSize) | Base Resources | Entities | Artifact Rate | Max Mass | Min Mass | Cycles to Recover |
|---|---|---|---|---|---|---|---|
| Satellite | Debris (16) | Steel, Copper, Glass | - | Bad | 64M | ~64M | 18 |
| Metallic Asteroid | Asteroid (32) | Iron, Copper, Obsidian | Metal Hatch x3 | Mediocre | 128M | ~128M | 12 |
| Rocky Asteroid | Asteroid (32) | Cuprite, Sedimentary Rock, Igneous Rock | Hard Hatch x3 | Good | 128M | ~128M | 18 |
| Carbonaceous Asteroid | Asteroid (32) | Refined Carbon, Carbon, Diamond | - | Mediocre | 128M | ~128M | default (6) |
| Oily Asteroid | Asteroid (32) | Solid Methane, Solid CO2, Crude Oil, Petroleum | - | Mediocre | 128M | ~128M | 12 |
| Gold Asteroid | Asteroid (32) | Gold, Fullerene, Fool's Gold | - | Bad | 128M | ~128M | 90 |
| Icy Dwarf | Dwarf Planet (64) | Ice, Solid CO2, Solid Oxygen | Cold Breather x3, Cold Wheat x4 | Great | 256M | ~256M | 24 |
| Organic Dwarf | Dwarf Planet (64) | Slime Mold, Algae, Polluted O2 | Moo x2, Gas Grass x12 | Great | 256M | ~256M | 30 |
| Dusty Moon | Dwarf Planet (64) | Regolith, Mafic Rock, Sedimentary Rock | - | Amazing | 256M | ~256M | 42 |
| Salt Dwarf | Dwarf Planet (64) | Salt Water, Solid CO2, Brine | Salt Vine x3 | Bad | 256M | ~256M | 30 |
| Red Dwarf | Dwarf Planet (64) | Aluminum, Liquid Methane, Fossil | - | Amazing | 256M | ~256M | 42 |
| Terra Planet | Planet (96) | Water, Algae, Oxygen, Dirt | Prickle Flower x4, Pacu x4 | Amazing | 384M | ~384M | 54 |
| Volcano Planet | Planet (96) | Magma, Igneous Rock, Insulation | - | Amazing | 384M | ~384M | 54 |
| Gas Giant | Giant (96) | Methane, Hydrogen | - | Perfect | 384M | ~384M | 60 |
| Ice Giant | Giant (96) | Ice, Solid CO2, Solid O2, Solid Methane | - | Perfect | 384M | ~384M | 60 |
| Rust Planet | Planet (96) | Rust, Solid CO2 | - | Perfect | 384M | ~384M | 60 |
| Forest Planet | Planet (96) | Aluminum Ore, Solid Oxygen | Squirrel x1, Arbor Tree x4 | Mediocre | 384M | ~384M | 24 |
| Shiny Planet | Planet (96) | Tungsten, Wolframite | - | Good | 384M | ~384M | 84 |
| Chlorine Planet | Planet (96) | Solid Chlorine, Bleach Stone | - | Bad | 256M | ~256M | 90 |
| Salt Desert Planet | Planet (96) | Salt, Crushed Rock | Pokeshell x1 | Bad | 384M | ~384M | 60 |
| Hydrogen Giant | Giant (96) | Liquid Hydrogen, Water, Niobium | - | Mediocre | 384M | ~384M | 78 |
| Wormhole | Special (96) | Vacuum | - | Perfect | 0 | 0 | 0 |
| Earth | Special (96) | (none) | - | None | 0 | 0 | 0 |

**DLC-specific destinations** (base game starmap, require their respective DLCs):
- DLC2 Ceres: Cinnabar, Mercury, Ice; Wood Deer x3, Hard Skin Berry x4; Good; 384M; 60 cycles
- DLC4 Prehistoric: Nickel Ore, Peat, Shale, Amber, Iridium; Stego x1, Raptor x1, Vine Mother x4; Good; 384M; 60 cycles
- DLC4 Demolior debris fields (3 variants): various rare materials; no artifacts; 384M; 60 cycles

### Destination Mass and Replenishment

Each destination has a mass pool that cargo bays draw from:
- `availableMass` starts at `maxMass - minMass` (typically 6,000 to 20,000 kg depending on type)
- `replenishmentPerCycle = 1000 / cyclesToRecover` kg per cycle
- `replenishmentPerSim1000ms = 1000 / (cyclesToRecover * 600)` kg per second
- `CurrentMass = minimumMass + availableMass`
- Cargo fills up to `min(CurrentMass + reservedMass - minimumMass, totalCargoSpace)`

Resource distribution within cargo: each element's share is proportional to its value from `elementTable` (a `MinMax` of 100-200, lerped by a random roll per element).

### Research Opportunities

Each destination generates 5 research opportunities worth `BASIC = 50` data points each:
1. Upper Atmosphere
2. Lower Atmosphere
3. Magnetic Field
4. Surface
5. Subsurface

Rare elements discovered through research: Insulation (Katairite, 50-100 kg), Niobium (10-20 kg), Fullerene (0.5-1 kg), Isoresin (30-60 kg).

Rare element chances (weighted roll):
- 0 rare elements: weight 1.0 (most common)
- 1 rare element: weight 0.33
- 2 rare elements: weight 0.03

Rare item: Gene Shuffler Recharge (33% chance, 1-2 units).

### Artifact Drop Rates

Artifacts have tiers with decor bonuses:

| Tier | Decor Bonus Tier | Decor Amount | Decor Radius |
|---|---|---|---|
| TIER_NONE | NONE | 0 | 0 |
| TIER0 | BONUS.TIER0 | 10 | 1 |
| TIER1 | BONUS.TIER2 | 20 | 3 |
| TIER2 | BONUS.TIER4 | 30 | 5 |
| TIER3 | BONUS.TIER5 | 35 | 6 |
| TIER4 | BONUS.TIER6 | 50 | 7 |
| TIER5 | BONUS.TIER7 | 80 | 7 |

Drop rate tables (weights, not percentages; divide by total for probability):

| Rate | None | T0 | T1 | T2 | T3 | T4 | T5 | Total |
|---|---|---|---|---|---|---|---|---|
| None | 1 | - | - | - | - | - | - | 1 |
| Bad | 10 | 5 | 3 | 2 | - | - | - | 20 |
| Mediocre | 10 | - | 5 | 3 | 2 | - | - | 20 |
| Good | 10 | - | - | 5 | 3 | 2 | - | 20 |
| Great | 10 | - | - | - | 5 | 3 | 2 | 20 |
| Amazing | 10 | - | - | - | 3 | 5 | 2 | 20 |
| Perfect | 10 | - | - | - | - | 6 | 4 | 20 |

### Mission Flow (Base Game)

1. Rocket launches: `Spacecraft.BeginMission(destination)`, state = Launching
2. Mission duration = `distance * 1800 / pilotEfficiency` seconds
3. Progress ticks in `ProgressMission`: elapsed += dt (+ 20% bonus from Mission Control)
4. On completion: state = WaitingToLand, triggers landing events on all modules
5. Cargo bays receive resources from destination based on cargo type and available mass
6. Destination mass depletes; replenishes at `replishmentPerSim1000ms`

**Wormhole special case**: Sending a rocket to the Wormhole destination triggers `TemporallyTear`, which destroys all modules, consumes all stored items, and deletes any stored duplicants. Sets `hasVisitedWormHole = true`.

## Space POIs (DLC)

### Harvestable POIs

DLC space has `HarvestablePOI` entities on the cluster map. Each has:
- A type defining harvestable elements with weights (relative proportions, not rates)
- Capacity: randomized between `poiCapacityMin` and `poiCapacityMax` (default 54,000-81,000 kg)
- Recharge time: randomized between `poiRechargeMin` and `poiRechargeMax` (default 30,000-60,000 s, i.e., 50-100 cycles)
- Initial data banks (default 50 for most POIs)
- Initial liberated resources (spawned on first visit as floating items in the hex cell)
- Optional artifact spawning (`canProvideArtifacts`)

Recharge happens once per day: `delta = maxCapacity * (600 / rechargeTime)` added to current capacity.

### Harvestable POI Types

| POI | Elements (weight) | Capacity (kg) | Recharge (s) | Artifacts |
|---|---|---|---|---|
| Carbon Asteroid Field | Refined Carbon (1.5), Carbon (5.5) | 30k-45k | 30k-60k | Yes |
| Metallic Asteroid Field | Molten Iron (1.25), Cuprite (1.75), Obsidian (6.25), Molten Lead (1.75) | 54k-81k | 30k-60k | Yes |
| Satellite Field | Sand (3), Iron Ore (3), Molten Copper (2.67), Glass (1.33) | 30k-45k | 30k-60k | Yes |
| Rocky Asteroid Field | Katairite (2), Granite (2), Sedimentary Rock (3), Igneous Rock (3) | 81k-94k | 30k-60k | Yes |
| Interstellar Ice Field | Ice (2.5), Solid CO2 (7), Solid O2 (0.5) | 54k-81k | 30k-60k | Yes |
| Organic Mass Field | Slime Mold (3), Algae (3), Polluted O2 (1), Dirt (3) | 54k-81k | 30k-60k | Yes |
| Ice Asteroid Field | Ice (6), Solid CO2 (2), Oxygen (1.5), Solid Methane (0.5) | 54k-81k | 30k-60k | Yes |
| Gas Giant Cloud | Methane (1), Liquid Methane (1), Solid Methane (1), Hydrogen (7) | 15k-20k | 30k-60k | Yes |
| Chlorine Cloud | Chlorine (2.5), Bleach Stone (7.5) | 54k-81k | 30k-60k | Yes |
| Gilded Asteroid Field | Gold (2.5), Fullerene (1), Refined Carbon (1), Sedimentary Rock (4.5), Regolith (1) | 30k-45k | 30k-60k | Yes |
| Glimmering Asteroid Field | Molten Tungsten (2), Wolframite (6), Carbon (1), CO2 (1) | 30k-45k | 30k-60k | Yes |
| Helium Cloud | Hydrogen (2), Water (8) | 30k-45k | 30k-60k | Yes |
| Oily Asteroid Field | Solid CO2 (7.75), Solid Methane (1.125), Crude Oil (1.125) | 15k-25k | 30k-60k | Yes |
| Oxidized Asteroid Field | Rust (8), Solid CO2 (2) | 54k-81k | 30k-60k | Yes |
| Salty Asteroid Field | Salt Water (5), Brine (4), Solid CO2 (1) | 54k-81k | 30k-60k | Yes |
| Frozen Ore Field | Ice (2.33), Dirty Ice (2.33), Snow (1.83), Aluminum Ore (2) | 54k-81k | 30k-60k | Yes |
| Foresty Ore Field | Igneous Rock (7), Aluminum Ore (1), CO2 (2) | 54k-81k | 30k-60k | Yes |
| Swampy Ore Field | Mud (2), Toxic Sand (7), Cobaltite (1) | 54k-81k | 30k-60k | Yes |
| Sandy Ore Field | Sandstone (4), Algae (2), Cuprite (1), Sand (3) | 54k-81k | 30k-60k | Yes |
| Radioactive Gas Cloud | Uranium Ore (2), Chlorine (2), CO2 (7) | 5k-10k | 30k-60k | Yes |
| Radioactive Asteroid Field | Uranium Ore (2), Sulfur (3), Bleach Stone (2), Rust (4) | 5k-10k | 30k-60k | Yes |
| Oxygen Rich Asteroid Field | Water (4), Polluted O2 (2), Ice (4) | 15k-25k | 30k-60k | Yes |
| Interstellar Ocean | Salt Water (2.5), Brine (2.5), Salt (2.5), Ice (2.5) | 15k-25k | 30k-60k | Yes |

DLC2/DLC4 add additional POI types (Ceres Fields, Prehistoric Fields, Impactor Debris Fields).

### POI Harvesting Mechanics

Harvesting requires a `NoseconeHarvest` module in orbit of a harvestable POI.

**Resource harvesting** (`ResourceHarvestModule`):
- Harvest speed: `solidCapacity / timeToFill` where solidCapacity = 2700 * 10 = 27,000 kg and timeToFill = 3600s, so speed = 7.5 kg/s
- Consumes Diamond at 5% of harvested mass (0.05 kg Diamond per kg harvested)
- Diamond storage: 1000 kg, delivered manually
- Max extractable from diamond: `storedDiamond / 0.05` kg (1000 kg Diamond = 20,000 kg harvest capacity)
- Harvested resources go to a `StarmapHexCellInventory` at the rocket's location
- Updates every 4000ms (`SIM_4000ms`)
- Depletes POI capacity; POI recharges over time

**Artifact harvesting** (`ArtifactHarvestModule`):
- Separate from resource harvesting
- Artifact POIs start fully charged and spawn one artifact on the hex cell
- After pickup, recharge timer starts (default 30,000-60,000 s)
- Recharges once per day: `chargeProgress += 600 / rechargeTime`
- When fully charged, spawns a new artifact and resets to 0
- First harvest gives a specific pre-assigned artifact; subsequent ones are random unique space artifacts
- POIs with `canProvideArtifacts = true` in harvestable POIs also support artifact harvesting

## Rocket Interior (DLC)

DLC rockets with a `PassengerRocketModule` (habitat) create an interior world:
- Interior world size: 32x32 tiles (`ROCKETRY.ROCKET_INTERIOR_SIZE`)
- Template loaded from `interiorTemplateName` (e.g., `expansion1::interiors/habitat_medium`)
- Connected to exterior via `ClustercraftExteriorDoor` / `ClustercraftInteriorDoor` pair
- Interior has gas/liquid conduit ports for connecting to external infrastructure
- Duplicants inside can operate the Rocket Control Station to set `AutoPilotMultiplier`

### Conduit Connections

The habitat module has 4 conduit ports:
- Gas input/output (for atmosphere management)
- Liquid input/output (for plumbing)

These use `RocketConduitSender`/`RocketConduitReceiver` to bridge between the interior world and the exterior module.

## Self-Destruct

In-space rockets can self-destruct via `CraftModuleInterface.CompleteSelfDestruct`:
1. All storage contents are dropped
2. Creatures are butchered for drops
3. Modules are deconstructed for materials
4. All items lose 50% of their mass (`SELF_DESTRUCT_REFUND_FACTOR = 0.5`)
5. Debris is packed into `DebrisPayload` containers and launched as `RailGunPayload` toward the closest visible asteroid
6. The `Clustercraft` is set to exploding state

## Launch Conditions

The game checks four condition categories before launch:

1. **RocketPrep**: Engine has fuel tank, oxidizer tank (if needed), height within limits
2. **RocketStorage**: Sufficient fuel, sufficient oxidizer
3. **RocketBoard**: Crew requirements met
4. **RocketFlight**: Flight path clear, destination valid

Each module contributes conditions to these categories. `CheckPreppedForLaunch` requires Prep + Storage + Flight to pass. `CheckReadyToLaunch` requires all four.

Automated launch (via logic port) uses `CheckReadyForAutomatedLaunchCommand` which requires Prep and Storage to be `Ready` (not just not-Failed).

## Key Source Files

- `RocketEngine.cs` / `RocketEngineCluster.cs`: Engine state machines and exhaust
- `RocketModule.cs` / `RocketModuleCluster.cs`: Base module components
- `CraftModuleInterface.cs`: DLC rocket module management, range/fuel calculations
- `Clustercraft.cs`: DLC rocket entity on cluster map, speed/fuel burning
- `ClusterTraveler.cs`: Hex-by-hex travel logic
- `ClusterGrid.cs`: Hex grid structure, pathfinding, entity management
- `RocketStats.cs`: Base game thrust/range calculations
- `Spacecraft.cs`: Base game mission timing
- `SpaceDestination.cs` / `SpaceDestinationTypes.cs`: Base game destination data
- `HarvestablePOIConfigurator.cs` / `HarvestablePOIConfig.cs`: DLC space POI types and harvesting config
- `HarvestablePOIStates.cs`: POI recharge state machine
- `ArtifactPOIStates.cs` / `ArtifactPOIConfigurator.cs`: Artifact spawning and harvesting
- `ResourceHarvestModule.cs`: Nosecone harvest drilling logic
- `ArtifactHarvestModule.cs`: Artifact collection from hex cells
- `RoboPilotModule.cs` / `RoboPilotCommandModuleConfig.cs` / `RoboPilotModuleConfig.cs`: Crewless flight
- `MissionControl.cs` / `MissionControlCluster.cs`: Mission Control Station speed buff
- `TUNING/ROCKETRY.cs`: All tuning constants (efficiencies, burdens, fuel costs, heights)
- Engine configs: `SteamEngineConfig.cs`, `KeroseneEngineClusterConfig.cs`, `HEPEngineConfig.cs`, etc.
- Module configs: `HabitatModuleMediumConfig.cs`, `SolidCargoBayClusterConfig.cs`, `OxidizerTankClusterConfig.cs`, etc.
