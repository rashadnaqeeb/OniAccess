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
- Modules can be reordered via `ReorderableBuilding` (DLC only)
- Only one command/habitat module per rocket (`LimitOneCommandModule`)
- Nosecone and small habitat must be on top (`TopOnly`)

### Module Types

**Command/Habitat modules** (one required per rocket):
- Base game: `CommandModule` (5x5, requires `CanUseRockets` skill perk, stores 1 astronaut via `MinionStorage`)
- DLC: `HabitatModuleSmall` (3x3, burden 3, tagged `NoseRocketModule` + `TopOnly`), `HabitatModuleMedium` (5x4, burden 6). Both create interior worlds via `PassengerRocketModule` + `ClustercraftExteriorDoor`
- `RoboPilotCommandModule` (5x5, base game starmap only, requires DLC3, forbidden in Spaced Out) for crewless rockets

**Nosecone** (DLC): `NoseconeBasic` (5x2, burden 2, `TopOnly`), `NoseconeHarvest` (for space POI harvesting)

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

All DLC engines use `RocketEngineCluster` and the burden/power/fuel-per-distance system. All generate power via `ModuleGenerator` while burning.

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

The Radbolt Engine exhausts Fallout at 873 K with radiation disease and emits radiation while burning.

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

Oxidizer efficiencies (base game):
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

Oxidizer power = mass * efficiency factor:
- Fertilizer: 1.0 (VERY_LOW)
- Oxylite: 2.0 (LOW)
- Liquid Oxygen: 4.0 (HIGH)

### Fuel Tanks

| Tank | Game | Capacity | Notes |
|---|---|---|---|
| Steam Engine (built-in) | Base | 900 kg | Integrated fuel tank |
| Steam Engine Cluster (built-in) | DLC | 150 kg | Integrated |
| CO2 Engine (built-in) | DLC | 100 kg | Integrated, gas input |
| Sugar Engine (built-in) | DLC | 450 kg | Integrated, manual delivery |
| Small Petroleum Engine (built-in) | DLC | 450 kg | Integrated, liquid input |
| Separate fuel tanks | Both | Varies | Not covered here; depend on building config |

### Oxidizer Tanks (DLC)

| Tank | Capacity | Type | Burden |
|---|---|---|---|
| Solid Oxidizer Tank | 900 kg | Oxylite + Fertilizer | 5 |
| Liquid Oxidizer Tank | 450 kg | Liquid Oxygen only | 5 |

DLC oxidizer tanks do not consume on landing (`consumeOnLand = false` when cluster space enabled).

## Cargo Bays

### DLC Cargo Bays

Capacity = base value * `CARGO_CAPACITY_SCALE` (10).

| Cargo Bay | ID | Capacity (kg) | Burden | Size |
|---|---|---|---|---|
| Solid (large) | `CargoBayCluster` | 27,000 | 6 | 5x5 |
| Solid (small) | `SolidCargoBaySmall` | 12,000 | 4 | 3x3 |
| Liquid (large) | `LiquidCargoBayCluster` | 27,000 | 5 | 5x5 |
| Liquid (small) | `LiquidCargoBaySmall` | 9,000 | 3 | 3x3 |
| Gas (large) | `GasCargoBayCluster` | 11,000 | 4 | 5x5 |
| Gas (small) | `GasCargoBaySmall` | 3,600 | 2 | 3x3 |

## Piloting and Speed

### Base Game

The astronaut is stored in the `CommandModule` via `MinionStorage`. Requires the `CanUseRockets` skill perk. The pilot's Space Navigation attribute affects mission duration:
```
missionDuration = distance * 1800 / pilotNavigationEfficiency
```
Where `pilotNavigationEfficiency = 1 + sum(spaceNavigation skill bonuses)`.

Mission Control Station buff: adds 20% mission progress rate while active (`controlStationBuffTimeRemaining > 0`).

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

Organized by size tier (the number is `iconSize`, which also serves as the tier identifier):

| Type | Category | Base Resources | Entities | Artifact Rate |
|---|---|---|---|---|
| Satellite | Debris (16) | Steel, Copper, Glass | - | Bad |
| Metallic Asteroid | Asteroid (32) | Iron, Copper, Obsidian | Metal Hatch x3 | Mediocre |
| Rocky Asteroid | Asteroid (32) | Cuprite, Sedimentary Rock, Igneous Rock | Hard Hatch x3 | Good |
| Carbonaceous Asteroid | Asteroid (32) | Refined Carbon, Carbon, Diamond | - | Mediocre |
| Oily Asteroid | Asteroid (32) | Solid Methane, Solid CO2, Crude Oil, Petroleum | - | Mediocre |
| Gold Asteroid | Asteroid (32) | Gold, Fullerene, Fool's Gold | - | Bad |
| Icy Dwarf | Dwarf Planet (64) | Ice, Solid CO2, Solid Oxygen | Cold Breather x3, Cold Wheat x4 | Great |
| Organic Dwarf | Dwarf Planet (64) | Slime Mold, Algae, Polluted O2 | Moo x2, Gas Grass x12 | Great |
| Dusty Moon | Dwarf Planet (64) | Regolith, Mafic Rock, Sedimentary Rock | - | Amazing |
| Salt Dwarf | Dwarf Planet (64) | Salt Water, Solid CO2, Brine | Salt Vine x3 | Bad |
| Red Dwarf | Dwarf Planet (64) | Aluminum, Liquid Methane, Fossil | - | Amazing |
| Terra Planet | Planet (96) | Water, Algae, Oxygen, Dirt | Prickle Flower x4, Pacu x4 | Amazing |
| Volcano Planet | Planet (96) | Magma, Igneous Rock, Insulation | - | Amazing |
| Rust Planet | Planet (96) | Rust, Solid CO2 | - | Perfect |
| Forest Planet | Planet (96) | Aluminum Ore, Solid Oxygen | Squirrel x1, Arbor Tree x4 | Mediocre |
| Shiny Planet | Planet (96) | Tungsten, Wolframite | - | Good |
| Chlorine Planet | Planet (96) | Solid Chlorine, Bleach Stone | - | Bad |
| Salt Desert Planet | Planet (96) | Salt, Crushed Rock | Pokeshell x1 | Bad |
| Gas Giant | Giant (96) | Methane, Hydrogen | - | Perfect |
| Ice Giant | Giant (96) | Ice, Solid CO2, Solid O2, Solid Methane | - | Perfect |
| Hydrogen Giant | Giant (96) | Liquid Hydrogen, Water, Niobium | - | Mediocre |
| Wormhole | Special (96) | Vacuum | - | Perfect |
| Earth | Special (96) | (none) | - | None |

Rare elements discoverable through research: Insulation (Katairite), Niobium, Fullerene, Isoresin. Rare item: Gene Shuffler Recharge (33% chance).

### Mission Flow (Base Game)

1. Rocket launches: `Spacecraft.BeginMission(destination)`, state = Launching
2. Mission duration = `distance * 1800 / pilotEfficiency` seconds
3. Progress ticks in `ProgressMission`: elapsed += dt (+ 20% bonus from Mission Control)
4. On completion: state = WaitingToLand, triggers landing events on all modules
5. Cargo bays receive resources from destination based on cargo type and available mass
6. Destination mass depletes; replenishes at `replishmentPerSim1000ms`

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
- `TUNING/ROCKETRY.cs`: All tuning constants (efficiencies, burdens, fuel costs, heights)
- Engine configs: `SteamEngineConfig.cs`, `KeroseneEngineClusterConfig.cs`, `HEPEngineConfig.cs`, etc.
- Module configs: `HabitatModuleMediumConfig.cs`, `SolidCargoBayClusterConfig.cs`, `OxidizerTankClusterConfig.cs`, etc.
