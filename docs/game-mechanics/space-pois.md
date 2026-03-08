# Space POIs

Points of Interest on the cluster map (Spaced Out DLC) and starmap destinations (base game) that provide harvestable resources, artifacts, and research data. Derived from decompiled source code (`HarvestablePOIConfig.cs`, `HarvestablePOIConfigurator.cs`, `HarvestablePOIStates.cs`, `ArtifactPOIConfig.cs`, `ArtifactPOIConfigurator.cs`, `ArtifactPOIStates.cs`, `ResourceHarvestModule.cs`, `NoseconeHarvestConfig.cs`, `SpaceDestination.cs`, `SpaceDestinationTypes.cs`).

## Base Game vs Spaced Out

The two rocketry systems handle space resources completely differently:

- **Base game**: Telescopes discover `SpaceDestination` entries at varying distances on a linear starmap. Rockets fly timed missions and return with cargo. No drill module; resources are loaded into cargo bays automatically on mission completion.
- **Spaced Out (DLC1)**: `HarvestablePOIClusterGridEntity` objects sit on the hex cluster map. Rockets park at a POI's hex cell and drill in real time using a `NoseconeHarvest` module. Mined resources go to a `StarmapHexCellInventory` at that hex, then must be loaded into cargo bays for transport home.

## Harvestable POIs (Spaced Out)

Each harvestable POI has a type defined by `HarvestablePOIConfigurator.HarvestablePOIType` with these properties:

- **harvestableElements**: Dictionary of elements and their relative weights (determines output proportions)
- **poiCapacityMin/Max**: Total kg of resources the POI can hold (randomized per instance between min and max)
- **poiRechargeMin/Max**: Seconds to fully replenish from empty (randomized per instance)
- **canProvideArtifacts**: Whether the POI also spawns artifacts
- **initialDataBanks**: Databank items available on first visit (50 for all standard POIs)
- **initialLiberatedResources**: One-time free resources spawned at the hex on first visit

### Harvestable POI Types

All require DLC1 (Spaced Out) unless noted otherwise.

| POI | Resources (weight) | Capacity (kg) | Recharge (s) | Artifacts | Extra DLC |
|---|---|---|---|---|---|
| CarbonAsteroidField | Carbon (5.5), Refined Carbon (1.5) | 30,000-45,000 | 30,000-60,000 | Yes | - |
| MetallicAsteroidField | Obsidian (6.25), Cuprite (1.75), Molten Lead (1.75), Molten Iron (1.25) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| SatelliteField | Sand (3), Iron Ore (3), Molten Copper (2.67), Glass (1.33) | 30,000-45,000 | 30,000-60,000 | Yes | - |
| RockyAsteroidField | Igneous Rock (3), Sedimentary Rock (3), Granite (2), Insulation (2) | 81,000-94,000 | 30,000-60,000 | Yes | - |
| InterstellarIceField | Solid CO2 (7), Ice (2.5), Solid Oxygen (0.5) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| OrganicMassField | Slime Mold (3), Algae (3), Dirt (3), Polluted O2 (1) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| IceAsteroidField | Ice (6), Solid CO2 (2), Oxygen (1.5), Solid Methane (0.5) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| GasGiantCloud | Hydrogen (7), Methane (1), Liquid Methane (1), Solid Methane (1) | 15,000-20,000 | 30,000-60,000 | Yes | - |
| ChlorineCloud | Bleach Stone (7.5), Chlorine (2.5) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| GildedAsteroidField | Sedimentary Rock (4.5), Gold (2.5), Fullerene (1), Refined Carbon (1), Regolith (1) | 30,000-45,000 | 30,000-60,000 | Yes | - |
| GlimmeringAsteroidField | Wolframite (6), Molten Tungsten (2), Carbon (1), CO2 (1) | 30,000-45,000 | 30,000-60,000 | Yes | - |
| HeliumCloud | Water (8), Hydrogen (2) | 30,000-45,000 | 30,000-60,000 | Yes | - |
| OilyAsteroidField | Solid CO2 (7.75), Solid Methane (1.125), Crude Oil (1.125) | 15,000-25,000 | 30,000-60,000 | Yes | - |
| OxidizedAsteroidField | Rust (8), Solid CO2 (2) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| SaltyAsteroidField | Salt Water (5), Brine (4), Solid CO2 (1) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| FrozenOreField | Ice (2.33), Dirty Ice (2.33), Aluminum Ore (2), Snow (1.83) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| ForestyOreField | Igneous Rock (7), CO2 (2), Aluminum Ore (1) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| SwampyOreField | Toxic Sand (7), Mud (2), Cobaltite (1) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| SandyOreField | Sandstone (4), Sand (3), Algae (2), Cuprite (1) | 54,000-81,000 | 30,000-60,000 | Yes | - |
| RadioactiveGasCloud | CO2 (7), Uranium Ore (2), Chlorine (2) | 5,000-10,000 | 30,000-60,000 | Yes | - |
| RadioactiveAsteroidField | Rust (4), Sulfur (3), Bleach Stone (2), Uranium Ore (2) | 5,000-10,000 | 30,000-60,000 | Yes | - |
| OxygenRichAsteroidField | Water (4), Ice (4), Polluted O2 (2) | 15,000-25,000 | 30,000-60,000 | Yes | - |
| InterstellarOcean | Salt Water (2.5), Brine (2.5), Salt (2.5), Ice (2.5) | 15,000-25,000 | 30,000-60,000 | Yes | - |
| DLC2CeresField | Cinnabar (4.5), Mercury (2.5), Ice (2.5) | 15,000-25,000 | 30,000-60,000 | Yes | DLC2 |
| DLC2CeresOreField | Ice (3.5), Cinnabar (2.5), Mercury (2.5) | 15,000-25,000 | 30,000-60,000 | Yes | DLC2 |
| DLC4PrehistoricMixingField | Nickel Ore (4.5), Peat (2.5), Shale (1), Amber (1), Iridium (0.5) | 15,000-25,000 | 30,000-60,000 | Yes | DLC4 |
| DLC4PrehistoricOreField | Peat (2.5), Nickel Ore (1.5), Shale (1), Amber (1) | 15,000-25,000 | 30,000-60,000 | Yes | DLC4 |
| DLC4ImpactorDebrisField1 | Granite (3.9), Mafic Rock (2.3), Gold (2.1), Iridium (1.7) | 35,000-45,000 | 30,000-30,000 | No | DLC4 |
| DLC4ImpactorDebrisField2 | Liquid Sulfur (4.7), Petroleum (3.5), Isoresin (1.8) | 33,400-66,800 | 30,000-30,000 | No | DLC4 |
| DLC4ImpactorDebrisField3 | Magma (5.1), Molten Iridium (3.7), Liquid O2 (0.6), Liquid H2 (0.6) | 110,000-137,500 | 30,000-30,000 | No | DLC4 |

### Depletion and Replenishment

Managed by `HarvestablePOIStates`:

- **Initial capacity**: Randomized between 0 and max capacity on first encounter
- **Depletion**: Drilling subtracts mass from `poiCapacity` via `DeltaPOICapacity()`
- **Replenishment**: Each in-game day (600s), the POI recharges by `(600 / rechargeTime) * maxCapacity` kg
- **State transitions**: When `poiCapacity < maxCapacity`, the POI enters `recharging` state. When full, transitions to `idle`. Drilling can only occur when `poiCapacity > 0`

Replenishment example: A POI with 60,000 kg max capacity and 45,000s recharge time regains `(600/45000) * 60000 = 800 kg/day`.

### Initial Liberated Resources

Each POI spawns one-time resources at its hex cell via `ClusterGridOneTimeResourceSpawner` on first visit. These do not deplete the POI's capacity and can be collected without drilling. The amounts are listed per POI in `HarvestablePOIConfig.GenerateConfigs()` (typically thousands of kg of the POI's signature resources). All standard POIs also spawn 50 databanks.

## Artifact POIs (Spaced Out)

Separate from harvestable POIs. These are Gravitas space stations and Russell's Teapot. They provide artifacts rather than bulk resources.

| POI | Destroy on Harvest | Recharge (s) | Databanks |
|---|---|---|---|
| GravitasSpaceStation1-8 | No | 30,000-60,000 | 50 |
| RussellsTeapot | Yes | 30,000-60,000 | 0 |

### Artifact Spawning

Managed by `ArtifactPOIStates`:

1. When fully charged (`poiCharge >= 1.0`), spawns an artifact at the hex cell via `SpawnArtifactOnHexCell()`
2. Charge resets to 0 after spawning
3. Recharges by `600 / rechargeTime` per in-game day (fractional, since charge is normalized 0-1)
4. The first harvest from a POI yields its designated artifact (if any, e.g., TeaPot for Russell's Teapot). Subsequent harvests yield random unique space artifacts via `ArtifactSelector`
5. Russell's Teapot self-destructs after its artifact is spawned at the hex cell (`destroyOnHarvest: true`), not after pickup

Harvestable POIs with `canProvideArtifacts: true` also get an artifact component using the `defaultArtifactPoiType` ("HarvestablePOIArtifacts"), which shares the same recharge and spawning mechanics.

## Drillcone Harvesting Module

The `NoseconeHarvest` module (ID: `NoseconeHarvest`) is the only way to extract resources from harvestable POIs in Spaced Out.

### Specifications

- **Size**: 5x4 tiles, sits on top of the rocket (`TopOnly`)
- **Diamond storage**: 1,000 kg capacity, requires manual delivery
- **Diamond consumption**: 0.05 kg per kg of resources harvested (5% ratio)
- **Harvest speed**: `solidCapacity / timeToFill` where `solidCapacity = 2700 * 10 = 27,000 kg` and `timeToFill = 3600s`, giving **7.5 kg/s**
- **Burden**: Minor

### Drilling Process

From `ResourceHarvestModule.StatesInstance.HarvestFromPOI()`:

1. Rocket must be at a hex cell containing a `HarvestablePOIClusterGridEntity`
2. The POI must have `poiCapacity > 0`
3. Diamond must be available in the module's storage
4. Each tick (every 4000ms sim time), the module:
   - Calculates max extractable kg from available diamond: `diamondAvailable / 0.05`
   - Takes `min(maxFromDiamond, harvestSpeed * dt)` total kg
   - Distributes resources proportionally by weight into the hex cell's `StarmapHexCellInventory`
   - Subtracts the total from `poiCapacity`
   - Consumes `totalHarvested * 0.05` kg of diamond
5. Newly discovered elements are registered with `DiscoveredResources`

### Resource Collection

Harvested resources land in the `StarmapHexCellInventory` at the POI's hex cell, not directly in the rocket's cargo bays. To bring resources home, the rocket needs cargo bays. Resources are transferred from the hex inventory into cargo storage separately (the drill only puts resources into the hex cell inventory).

## Space Destinations (Base Game)

In the base game, space destinations are abstract `SpaceDestination` objects discovered by telescope. See `rocketry-and-space.md` for the full destination type table and mission flow.

### Depletion and Replenishment

- **Available mass**: Starts at `maxMass - minMass` (typically 6,000 kg for most types)
- **Current mass**: `minimumMass + availableMass` (the minimum is a huge number like 64M kg, representing the body's total mass; only the excess above minimum is harvestable)
- **Depletion**: Each cargo bay reserves mass from the destination on mission completion, reducing `availableMass`
- **Replenishment**: `replishmentPerSim1000ms = 1000 / (cyclesToRecover * 600)` kg per sim tick

| Destination Category | Harvestable Mass (kg) | Cycles to Recover |
|---|---|---|
| Satellite | 6,000 | 18 |
| Asteroids (most) | 12,000 | 6-90 |
| Dwarf Planets | 18,000 | 24-90 |
| Planets | 20,000 | 24-84 |
| Giants | 20,000 | 60-78 |

### Research Opportunities

Each destination generates 5 research opportunities (upper atmosphere, lower atmosphere, magnetic field, surface, subsurface). Completing them yields data points and may discover rare resources:

- **Rare elements**: Insulation (Katairite), Niobium, Fullerene, Isoresin
- **Rare items**: Gene Shuffler Recharge (33% chance)

## Key Source Files

- `HarvestablePOIConfig.cs`: All harvestable POI type definitions with resource weights and capacities
- `HarvestablePOIConfigurator.cs`: POI type data structure and instance randomization
- `HarvestablePOIStates.cs`: Depletion/recharge state machine
- `HarvestablePOIClusterGridEntity.cs`: Cluster map entity (layer: POI, always visible)
- `ArtifactPOIConfig.cs`: Gravitas station and Russell's Teapot definitions
- `ArtifactPOIConfigurator.cs`: Artifact POI type and recharge configuration
- `ArtifactPOIStates.cs`: Artifact spawn/recharge state machine
- `ResourceHarvestModule.cs`: Drillcone harvesting state machine and extraction logic
- `NoseconeHarvestConfig.cs`: Drillcone building definition (speed, diamond cost)
- `StarmapHexCellInventory.cs`: Hex cell resource storage for harvested materials
- `SpaceDestination.cs`: Base game destination with mass, research, and resource recovery
- `SpaceDestinationTypes.cs`: All base game destination type definitions
- `SpacePOISimpleInfoPanel.cs`: UI panel showing POI mass, elements, and artifact status
