# Meteor Showers

How meteors spawn, deal damage, deposit resources, and how the shower scheduling system selects and times events. Derived from decompiled source code (`Comet.cs`, `BaseCometConfig.cs`, `MeteorShowerEvent.cs`, `MeteorShowerSeason.cs`, `GameplaySeasons.cs`, `GameplayEvents.cs`, `ClusterMapMeteorShower.cs`, `ClusterMapMeteorShowerConfig.cs`, `BunkerDoorConfig.cs`, `BunkerTileConfig.cs`, `TUNING/METEORS.cs`, and individual `*CometConfig.cs` files).

## Comet Types

Each comet is a prefab with a `Comet` component. Key properties: mass range, temperature range, element dropped, entity damage (HP to buildings/creatures per cell traversed), total tile damage (fraction of tile HP consumed while boring through terrain), splash radius, number of ore items spawned on explosion, and number of terrain tiles deposited.

### Base Game Comets

| Comet | Element | Mass (kg) | Temp (K) | Entity Dmg | Tile Dmg | Ore Count | Add Tiles | Source |
|-------|---------|-----------|----------|------------|----------|-----------|-----------|--------|
| Iron | Iron | 3-20 | 323-423 | 15 | 0.5 | 2-4 | 0 | `IronCometConfig.cs` |
| Rock | Regolith | mass\*0.8\*6 - mass\*1.2\*6 | 323-423 | 20 | 0.0 | 0 | 6 | `RockCometConfig.cs` |
| Copper | Cuprite | 3-20 | 323-423 | 15 | 0.5 | 2-4 | 0 | `CopperCometConfig.cs` |
| Gold | Gold Amalgam | 3-20 | 323-423 | 15 | 0.5 | 2-4 | 0 | `GoldCometConfig.cs` |
| Dust | Regolith | 0.2-0.5 | 223-253 | 2 | 0.15 | 0 | 0 | `DustCometConfig.cs` |
| Satellite | Aluminum | 100-200 | 473-573 | 2 | 2.0 | 8 | 0 | `SatelliteCometConfig.cs` (deprecated) |

Rock comets use `ElementLoader.FindElementByHash(SimHashes.Regolith).defaultValues.mass` as the per-tile mass, multiplied by 0.8-1.2 and the tile count (6). They deposit 6 tiles of Regolith on impact (min height 2, max height 8) instead of scattering ore.

### Spaced Out (DLC1) Comets

| Comet | Element | Mass (kg) | Temp (K) | Entity Dmg | Tile Dmg | Ore Count | Add Tiles | Source |
|-------|---------|-----------|----------|------------|----------|-----------|-----------|--------|
| Snowball | Snow | 3-20 | 253-263 | 0 | 0.0 | 0 | 3 | `SnowballCometConfig.cs` |
| Hard Ice | Crushed Ice | mass\*0.8\*6 - mass\*1.2\*6 | 173-248 | 0 | 0.0 | 0 | 6 | `HardIceCometConfig.cs` |
| Light Dust | Regolith | 10-14 | 223-253 | 0 | 0.0 | 1-2 | 0 | `LightDustCometConfig.cs` |
| Oxylite | OxyRock | mass\*0.8\*6 - mass\*1.2\*6 | 310-323 | 0 | 0.0 | 0 | 6 | `OxyliteCometConfig.cs` |
| Bleach Stone | BleachStone | mass\*0.8 - mass\*1.2 | 310-323 | 0 | 0.0 | 2-4 | 1 | `BleachStoneCometConfig.cs` |
| Slime | Slime Mold | mass\*0.8\*2 - mass\*1.2\*2 | 310-323 | 0 | 0.0 | 1-2 | 2 | `SlimeCometConfig.cs` |
| Algae | Algae | 3-20 | 310-323 | 0 | 0.0 | 2-4 | 0 | `AlgaeCometConfig.cs` |
| Phosphoric | Phosphorite | 3-20 | 310-323 | 0 | 0.0 | 1-2 | 0 | `PhosphoricCometConfig.cs` |
| Uranium | Uranium Ore | mass\*0.8\*6 - mass\*1.2\*6 | 323-403 | 15 | 0.0 | 1-2 | 6 | `UraniumCometConfig.cs` |
| Fullerene | Fullerene | 3-20 | 323-423 | 15 | 0.5 | 2-4 | 0 | `FullereneCometConfig.cs` |
| Nuclear Waste | Corium | 1 | 473-573 | 2 | 0.45 | 0 | 1 | `NuclearWasteCometConfig.cs` |

Slime comets carry Slime Lung disease. Their exhaust element is Contaminated Oxygen. Nuclear Waste comets exhaust Fallout and deposit Radiation Poisoning disease (1,000,000 germs).

Hard Ice comets exhaust Oxygen instead of the default CO2. Snowball comets and several biological comets exhaust nothing (`SimHashes.Void`).

### Creature Comets

| Comet | Spawns | Mass (kg) | Temp (K) | Exhaust | Source |
|-------|--------|-----------|----------|---------|--------|
| Gassy Moo | Moo (on impact) | 100-200 | 296-318 | Methane | `GassyMooCometConfig.cs` |
| Diesel Moo | DieselMoo (on impact) | 100-200 | 296-318 | CO2 | `DieselMooCometConfig.cs` |

Creature comets use `GassyMooComet` (subclass of `Comet`). They have `destroyOnExplode = false` and spawn the creature via `craterPrefabs`. Both deal 0 entity and tile damage. Not affected by difficulty. If destroyed by missile, they drop 3 Meat instead of the creature.

### DLC2 (Frosty Planet Pack) Comets

| Comet | Element | Mass (kg) | Temp (K) | Entity Dmg | Tile Dmg | Add Tiles | Source |
|-------|---------|-----------|----------|------------|----------|-----------|--------|
| Space Tree Seed | Snow | 50-100 | 253-263 | 0 | 0.0 | 3 | `SpaceTreeSeedCometConfig.cs` |

Uses `SpaceTreeSeededComet` subclass. Drops a Space Tree Seed if destroyed by missile.

### DLC4 (Demolior) Comets

| Comet | Element | Temp (K) | Source |
|-------|---------|----------|--------|
| Large Impactor | Regolith | 20,000 | `LargeImpactorCometConfig.cs` |
| Iridium | Iridium | 473-548 | `IridiumCometConfig.cs` |

The Large Impactor uses `LargeComet` (separate class from `Comet`). It stamps a template onto the world on impact rather than using the normal tile damage/deposit system. Temperature of 20,000 K.

Iridium comets are standard `Comet` instances: mass 10-100 kg, entity damage 15, tile damage 0.5, 2-4 ore.

## Meteor Shower Events

Each shower is a `MeteorShowerEvent` (subclass of `GameplayEvent`). Constructor parameters:

- **duration** - total event runtime in seconds
- **secondsPerMeteor** - base interval between individual meteor spawns
- **secondsBombardmentOn** - MinMax range for how long each bombardment burst lasts
- **secondsBombardmentOff** - MinMax range for snooze time between bursts
- **clusterMapMeteorShowerID** - if set, shower travels across cluster map before arriving (Spaced Out)
- **affectedByDifficulty** - whether difficulty settings scale timing and mass

Each event holds a weighted list of `BombardmentInfo` entries (comet prefab + weight). During bombardment, a random comet type is selected proportional to weight.

### Base Game Shower Events

| Event ID | Duration (s) | Sec/Meteor | Bombard On (s) | Bombard Off (s) | Comets (weight) |
|----------|-------------|------------|----------------|-----------------|-----------------|
| MeteorShowerGoldEvent | 3000 | 0.4 | 50-100 | 800-1200 | Gold (2), Rock (0.5), Dust (5) |
| MeteorShowerCopperEvent | 4200 | 5.5 | 100-400 | 300-1200 | Copper (1), Rock (1) |
| MeteorShowerIronEvent | 6000 | 1.25 | 100-400 | 300-1200 | Iron (1), Rock (2), Dust (5) |

### Spaced Out Cluster Shower Events

All cluster showers use `BOMBARDMENT_ON.UNLIMITED` (10,000s) and `BOMBARDMENT_OFF.NONE` (1s), meaning they bombard continuously for their entire duration with no snooze breaks.

| Event ID | Duration (s) | Sec/Meteor | Comets (weight) |
|----------|-------------|------------|-----------------|
| ClusterSnowShower | 600 | 3.0 | Snowball (2), Light Dust (1) |
| ClusterIceShower | 300 | 1.4 | Snowball (14), Hard Ice (1) |
| ClusterOxyliteShower | 300 | 4.0 | Oxylite (4), Light Dust (4) |
| ClusterBleachStoneShower | 300 | 3.0 | Bleach Stone (13), Light Dust (3) |
| ClusterBiologicalShower | 300 | 3.0 | Slime (2), Algae (1), Phosphoric (1) |
| ClusterLightRegolithShower | 300 | 4.0 | Dust (1), Light Dust (1) |
| ClusterRegolithShower | 300 | 3.5 | Dust (3), Rock (2), Light Dust (1) |
| ClusterGoldShower | 75 | 1.0 | Gold (4), Rock (1), Light Dust (2) |
| ClusterCopperShower | 150 | 2.5 | Copper (2), Rock (1) |
| ClusterIronShower | 300 | 4.5 | Iron (4), Dust (1), Light Dust (2) |
| ClusterUraniumShower | 150 | 4.5 | Uranium (2.5), Dust (1), Light Dust (2) |
| MeteorShowerDustEvent | 9000 | 1.25 | Rock (1), Dust (6) |

### Special Shower Events

| Event ID | Duration (s) | Sec/Meteor | Comets | Notes |
|----------|-------------|------------|--------|-------|
| GassyMooteorEvent | 15 | 3.125 | Gassy Moo (1) | Not affected by difficulty |
| MeteorShowerFullereneEvent | 30 | 0.5 | Fullerene (6), Dust (1) | Not affected by difficulty |
| ClusterIceAndTreesShower | 300 | 1.4 | Space Tree Seed (1), Hard Ice (2), Snowball (22) | DLC2 |
| IridiumShower | 30 | 0.5 | Iridium (1) | DLC4 |

## Shower Scheduling: Seasons

Showers are grouped into `MeteorShowerSeason` instances (subclass of `GameplaySeason`). Each season has:

- **period** - cycle count between event starts (in game cycles, where 1 cycle = 600s)
- **startActive** - whether the season begins running immediately
- **minCycle / maxCycle** - cycle range during which the season is active
- **numEventsToStartEachPeriod** - how many events fire per period (always 1 for meteor seasons)
- **clusterTravelDuration** - seconds for the shower to travel across the cluster map before arriving (Spaced Out only; -1 means no travel)

Each period, one event is randomly selected from the season's event list and started.

### Season Definitions

**Base Game (Vanilla)**

| Season | Period (cycles) | Events | Travel |
|--------|----------------|--------|--------|
| MeteorShowers | 14 | Iron, Gold, Copper | none |

**Spaced Out Seasons** (all have period 20 cycles, travel duration 6000s unless noted)

| Season | Events | Notes |
|--------|--------|-------|
| RegolithMoonMeteorShowers | Dust, ClusterIron, ClusterIce | Regolith asteroid |
| SpacedOutStyleStartMeteorShowers | (empty) | Starting asteroid - no showers |
| SpacedOutStyleRocketMeteorShowers | ClusterOxylite | Rocket destination asteroids |
| SpacedOutStyleWarpMeteorShowers | ClusterCopper, ClusterIce, ClusterBiological | Warp destination |
| ClassicStyleStartMeteorShowers | ClusterCopper, ClusterIce, ClusterBiological | Classic start |
| ClassicStyleWarpMeteorShowers | ClusterGold, ClusterIron | Classic warp |
| TundraMoonletMeteorShowers | (empty) | |
| MarshyMoonletMeteorShowers | (empty) | |
| NiobiumMoonletMeteorShowers | (empty) | |
| WaterMoonletMeteorShowers | (empty) | |
| MiniMetallicSwampyMeteorShowers | ClusterBiological, ClusterGold | |
| MiniForestFrozenMeteorShowers | ClusterOxylite | |
| MiniBadlandsMeteorShowers | ClusterIce | |
| MiniFlippedMeteorShowers | (empty) | |
| MiniRadioactiveOceanMeteorShowers | ClusterUranium | |
| GassyMooteorShowers | GassyMooteor | Not affected by difficulty |
| TemporalTearMeteorShowers | Fullerene | Period 1 cycle, not active at start, no travel, not affected by difficulty |

**DLC2 (Frosty Planet Pack)**

| Season | Period | Events | Notes |
|--------|--------|--------|-------|
| CeresMeteorShowers | 20 | ClusterIceAndTrees | minCycle=10, travel 6000s |
| MiniCeresStartShowers | 20 | ClusterOxylite, ClusterSnow | travel 6000s |

**DLC4 (Demolior)**

| Season | Period | Events | Notes |
|--------|--------|--------|-------|
| LargeImpactor | 1 | LargeImpactor | Finishes after 1 event |
| PrehistoricMeteorShowers | 50 | ClusterCopper, ClusterIron, ClusterGold | travel 6000s |

## Difficulty Scaling

The `MeteorShowers` custom game setting controls scaling. Five levels exist: ClearSkies (disables all difficulty-affected showers), Infrequent, Default (no modifier), Intense, Doomed.

### Season Period Multiplier (`MeteorShowerSeason.GetSeasonPeriod`)

| Difficulty | Multiplier | Effect on 14-cycle vanilla season |
|------------|------------|-----------------------------------|
| Infrequent | 2.0x | 28 cycles between showers |
| Default | 1.0x | 14 cycles |
| Intense | 1.0x | 14 cycles |
| Doomed | 1.0x | 14 cycles |

### Seconds Per Meteor Multiplier (`GetNextMeteorTime`)

Base value is scaled by `256 / worldWidth` to normalize spawn rate across different world sizes.

| Difficulty | Multiplier |
|------------|------------|
| Infrequent | 1.5x (slower) |
| Intense | 0.8x (faster) |
| Doomed | 0.5x (fastest) |

### Bombardment Off Time Multiplier (`GetBombardOffTime`)

| Difficulty | Multiplier |
|------------|------------|
| Infrequent | 1.0x |
| Intense | 1.0x |
| Doomed | 0.5x (shorter breaks) |

### Comet Mass Multiplier (`Comet.GetMassMultiplier`)

| Difficulty | Multiplier |
|------------|------------|
| Infrequent | 1.0x |
| Intense | 0.8x |
| Doomed | 0.5x |

Lower mass means less regolith deposited per comet and less ore dropped, but meteors arrive much more frequently on higher difficulties, so total throughput still increases.

## MeteorShowerEvent State Machine

The `MeteorShowerEvent.States` state machine drives each active shower instance:

```
planning -> [starMap.travelling -> starMap.arrive ->] running (bombarding <-> snoozing) -> finished
```

**planning** - Sets `runTimeRemaining` to the event duration. If the event has a cluster map travel component (`canStarTravel` and `clusterTravelDuration > 0`), transitions to `starMap`. Otherwise goes directly to `running`.

**starMap.travelling** - Creates a `ClusterMapMeteorShower` entity that visually travels across the cluster map. Waits for `OnClusterMapDestinationReached` signal.

**starMap.arrive** - Transitions to `running.bombarding`.

**running.bombarding** - Spawns individual comets at the rate determined by `secondsPerMeteor` (adjusted for difficulty and world width). Each spawn picks a random comet type from the weighted bombardment list. Triggers `MeteorShowerBombardStateBegins` / `MeteorShowerBombardStateEnds` global events (used by comet detectors). Activates background particle effects. Transitions to `snoozing` when `bombardTimeRemaining` hits zero.

**running.snoozing** - Pause between bombardment bursts. Duration from `GetBombardOffTime()`. Transitions back to `bombarding` when timer expires.

**running** - Parent state counts down `runTimeRemaining` each frame. When it reaches zero, transitions to `finished` regardless of current sub-state.

**finished** - Returns success, ending the event.

### Sleep Timer

If `GameplayEventManager.GetSleepTimer()` returns a value greater than 0 for the event, individual meteor spawns are skipped during `Bombarding()`. The timer still counts down normally. This is used by the comet detector sleep functionality.

## ClusterMapMeteorShower (Spaced Out)

In Spaced Out, showers that have a `clusterMapMeteorShowerID` spawn a `ClusterMapMeteorShower` entity. This entity:

1. Spawns at a random cell at the edge of the cluster map universe
2. Travels toward the destination world at a speed calculated from `(pathLength / timeUntilArrival) * 600`
3. Can be **identified** by a telescope or comet detector (progress from 0 to 1). Unidentified showers show as "Unidentified Object" on the cluster map
4. Can be **destroyed by long-range missiles** - transitions to `destroyed` state, which cancels the associated `MeteorShowerEvent`

State machine: `traveling (unidentified | identified) -> arrived | destroyed`

Source: `ClusterMapMeteorShower.cs`, `ClusterMapMeteorShowerConfig.cs`

## Comet Impact Mechanics

### Tile Damage

When a comet enters a solid cell (`Sim33ms`), it calls `DamageTiles()`. Damage is calculated as:

```
effectiveDamage = inputDamage * damageMultiplier / element.strength
```

Where `damageMultiplier` depends on the tile type:
- **Window tiles** (tag `GameTags.Window`): `windowDamageMultiplier` (default 5.0x, amplifies damage)
- **Bunker tiles/doors** (tag `GameTags.Bunker`): `bunkerDamageMultiplier` (default 0.0, blocks all damage)
- **Normal tiles**: 1.0x

The comet starts with `remainingTileDamage = totalTileDamage` and spends it boring through successive cells. When remaining damage reaches zero, the comet explodes at the current cell.

### Entity/Building Damage

As a comet flies through non-solid cells, `DamageThings()` deals `entityDamage` HP to any building in the cell (layer 1) and to any pickupable entities (creatures, items). Bunker-tagged buildings receive damage scaled by `bunkerDamageMultiplier` (0 for standard comets).

### Explosion

On impact (`Explode()`):
1. Spawns ore items: `explosionOreCount` items of the comet's element, each with mass = `explosionMass / count` at a random temperature in `explosionTemperatureRange`
2. Deposits terrain tiles if `addTiles > 0`: uses flood-fill from the impact cell to place up to `addTiles` cells of the comet's element. The actual count is reduced as existing element depth approaches `addTilesMaxHeight`
3. Applies splash damage in a `(splashRadius+1)` area, with falloff based on distance
4. Spawns crater prefabs if configured (creature comets spawn the creature this way)
5. Flings nearby pickupable items outward

### Regolith Accumulation

Rock comets are the primary source of regolith accumulation. Each deposits 6 tiles of Regolith. The `addTilesMinHeight`/`addTilesMaxHeight` (2/8 for Rock) controls a soft cap: as the Regolith column below the impact grows taller, fewer tiles are placed per impact. At `addTilesMaxHeight` depth, the multiplier reaches 0 and only 1 tile is placed (clamped minimum).

Tile mass per cell = `addTileMass / addTiles`, where `addTileMass` is 95-98% of total comet mass.

## Bunker Protection

### Bunker Tile

- 1000 HP
- Built from 200 kg Steel (`TIER2` construction mass)
- `strengthMultiplier = 10` on `SimCellOccupier` (10x harder to damage as terrain)
- Tagged `GameTags.Bunker`

### Bunker Door

- 1000 HP
- Built from 500 kg Steel
- 4 tiles wide, 1 tile tall
- Requires 120W power (for opening/closing)
- Can be rotated 90 degrees (vertical placement)
- Tagged `GameTags.Bunker`
- Has logic input for automation control
- Blocking a comet with a bunker door sets `Game.Instance.savedInfo.blockedCometWithBunkerDoor = true` (used for the colony achievement)

### How Bunker Protection Works

All standard comets have `bunkerDamageMultiplier = 0` (the default from `Comet.cs`). When a comet hits a tile or building tagged `GameTags.Bunker`:
- **Tile damage** is multiplied by 0, so the comet cannot bore through bunker tiles
- **Entity damage** to bunker-tagged buildings is multiplied by 0, so bunker doors take no HP damage from meteors
- The comet's `remainingTileDamage` is not consumed (it returns 0 damage dealt), so it immediately explodes on the bunker surface

This means a single layer of bunker tiles or a closed bunker door completely blocks all meteor damage and regolith penetration. The meteor explodes harmlessly on top.

## Exhaust Trail

While flying, comets emit their `EXHAUST_ELEMENT` at `EXHAUST_RATE` kg/s (default 50 kg/s CO2). This creates a trail of gas. Some comets override this: Hard Ice exhales Oxygen, Gassy Moo exhales Methane, Bleach Stone exhales Chlorine Gas. Several benign comets set `EXHAUST_ELEMENT = SimHashes.Void` to emit nothing.

## Comet Detector Integration

The `CometDetector` / `ClusterCometDetector` buildings subscribe to `MeteorShowerBombardStateBegins` and `MeteorShowerBombardStateEnds` events to detect active showers and output automation signals. In Spaced Out, the telescope can progressively identify `ClusterMapMeteorShower` entities on the cluster map.
