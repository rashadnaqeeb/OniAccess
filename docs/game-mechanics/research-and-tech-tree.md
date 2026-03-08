# Research and Tech Tree

## Research Point Types

Five research point types exist, defined in `ResearchTypes` with string IDs in `ResearchTypes.ID`:

| ID | Internal Name | Input Material | Fabrication Time | Station |
|---|---|---|---|---|
| `"basic"` | Alpha (RESEARCH.TYPES.ALPHA) | Dirt | 600s | ResearchCenter |
| `"advanced"` | Beta (RESEARCH.TYPES.BETA) | Water | 1200s | AdvancedResearchCenter |
| `"space"` | Gamma (RESEARCH.TYPES.GAMMA) | Research Databank | 2400s | CosmicResearchCenter |
| `"nuclear"` | Delta (RESEARCH.TYPES.DELTA) | High Energy Particle | 2400s | NuclearResearchCenter |
| `"orbital"` | Orbital (RESEARCH.TYPES.ORBITAL) | Orbital Research Databank | 2400s | OrbitalResearchCenter or DLC1CosmicResearchCenter |

Source: `ResearchTypes.cs` lines 7-39, `ResearchType.cs`

Each `ResearchType` creates a recipe entity via `ResearchType.CreatePrefab()` that binds to a specific fabricator building.

## Research Stations

### Research Station (ResearchCenter)

- **ID**: `"ResearchCenter"` -- produces `"basic"` research points
- **Size**: 2x2, power 60W
- **Input**: Dirt (tag `GameTags.Dirt`), 50 kg per point, storage capacity 750 kg
- **Consumption rate**: 1.111 kg/s base (= 50 kg / 45 seconds per point)
- **Seconds per point**: 45 (base, before skill multiplier)
- Source: `ResearchCenterConfig.cs`

### Super Computer (AdvancedResearchCenter)

- **ID**: `"AdvancedResearchCenter"` -- produces `"advanced"` research points
- **Size**: 3x3, power 120W
- **Input**: Water (tag `GameTags.Water`), 50 kg per point, storage capacity 750 kg
- **Consumption rate**: 0.833 kg/s base (= 50 kg / 60 seconds per point)
- **Seconds per point**: 60 (base)
- **Required skill perk**: `AllowAdvancedResearch`
- Source: `AdvancedResearchCenterConfig.cs`

### Virtual Planetarium (CosmicResearchCenter) -- base game only

- **ID**: `"CosmicResearchCenter"` -- produces `"space"` research points
- **Size**: 4x4, power 120W
- **Input**: Research Databank (`ResearchDatabankConfig.TAG`), 1 kg per point, storage capacity 300 kg
- **Consumption rate**: 0.02 kg/s base (= 1 kg / 50 seconds per point)
- **Seconds per point**: 50 (base)
- **Required skill perk**: `AllowInterstellarResearch`
- **DLC restriction**: Forbidden when Spaced Out! is active (`GetForbiddenDlcIds` returns `DlcManager.EXPANSION1`)
- Source: `CosmicResearchCenterConfig.cs`

### Nuclear Research Center (NuclearResearchCenter) -- Spaced Out! only

- **ID**: `"NuclearResearchCenter"` -- produces `"nuclear"` research points
- **Size**: 5x3, power 120W, built from refined metals
- **Input**: High Energy Particles (`GameTags.HighEnergyParticle`), 10 particles per point, HEP storage capacity 100
- **Time per point**: 100s (base)
- **Required skill perk**: `AllowNuclearResearch`
- **DLC restriction**: Requires Spaced Out! (`GetRequiredDlcIds` returns `DlcManager.EXPANSION1`)
- Uses `NuclearResearchCenterWorkable` instead of `ResearchCenter` component. Point generation in `OnWorkTick`: each tick consumes `(dt / timePerPoint) * materialPerPoint` particles and accumulates fractional points, adding integer points to research.
- Source: `NuclearResearchCenterConfig.cs`, `NuclearResearchCenter.cs`, `NuclearResearchCenterWorkable.cs`

### Orbital Data Collection Lab (DLC1CosmicResearchCenter) -- Spaced Out! only

- **ID**: `"DLC1CosmicResearchCenter"` -- produces `"orbital"` research points
- **Size**: 4x4, power 120W
- **Input**: Orbital Research Databank (`OrbitalResearchDatabankConfig.TAG`), 1 kg per point, storage capacity 300 kg
- **Consumption rate**: 0.02 kg/s base (= 1 kg / 50 seconds per point)
- **Seconds per point**: 50 (base)
- **Required skill perk**: `AllowOrbitalResearch`
- **DLC restriction**: Requires Spaced Out!
- Replaces the base-game CosmicResearchCenter for space-tier research in Spaced Out!
- Source: `DLC1CosmicResearchCenterConfig.cs`

### Orbital Research Center (OrbitalResearchCenter) -- Spaced Out! only

- **ID**: `"OrbitalResearchCenter"` -- produces Orbital Research Databanks (not research points directly)
- **Size**: 2x3, power 60W, built from plastics
- **Input**: Polypropylene, 5 kg per databank
- **Time per databank**: 33s
- **Required skill perk**: `CanMissionControl`
- Must be built inside a rocket and requires being in orbit (`InOrbitRequired` component)
- Uses `ComplexFabricator` -- it is a crafting station, not a `ResearchCenter`. It produces `OrbitalResearchDatabank` items that are then consumed by the DLC1CosmicResearchCenter.
- Source: `OrbitalResearchCenterConfig.cs`

## Databank Sources

### Research Databanks (base game space research)

- **ID**: `"ResearchDatabank"`, 1 kg each
- In the base game, produced by the Research Module rocket part. On landing, it produces `ROCKETRY.DESTINATION_RESEARCH.EVERGREEN` (10) databanks automatically, plus databanks from completed research opportunities at the destination (each worth `ROCKETRY.DESTINATION_RESEARCH.BASIC` = 50).
- Max stack size: 50 (`ROCKETRY.DESTINATION_RESEARCH.BASIC`)
- Source: `ResearchDatabankConfig.cs`, `ResearchModule.cs`, `TUNING/ROCKETRY.cs`

### Orbital Research Databanks (Spaced Out! space research)

- **ID**: `"OrbitalResearchDatabank"`, 1 kg each
- Produced by the Orbital Research Center (inside rockets in orbit) and by the Research Cluster Module
- Max stack size: 50
- Source: `OrbitalResearchDatabankConfig.cs`

### Research Cluster Module (Spaced Out!)

- **ID**: `"ResearchClusterModule"` -- rocket module that passively collects databanks in space
- Storage capacity: 50 kg
- Collection speed: 1/12 per second (`COLLECT_SPEED`)
- Drops all stored databanks on landing
- Source: `ResearchClusterModuleConfig.cs`, `ResearchClusterModule.cs`

## Research Point Generation Speed

For stations using the `ResearchCenter` component (basic, advanced, space, orbital via DLC1CosmicResearchCenter):

The `ElementConverter` consumes input material at a rate modified by `OnWorkTick`:

```
workSpeedMultiplier = 2.0 + efficiencyMultiplier
```

where `efficiencyMultiplier` comes from `GetEfficiencyMultiplier(worker)` which applies the `ResearchSpeed` attribute converter. The base work speed multiplier is 2.0 before any skill bonuses.

When `FastWorkersModeActive` (sandbox mode), the multiplier is doubled again.

`ConvertMassToResearchPoints` converts consumed mass into integer research points at the ratio of `mass_per_point` (e.g., 50 kg for basic).

Source: `ResearchCenter.cs` lines 139-149, 71-86

For the Nuclear Research Center (`NuclearResearchCenterWorkable`):

Each work tick produces `dt / timePerPoint` fractional points and consumes a proportional amount of particles. Integer points are added when accumulated fractions reach 1.0.

Source: `NuclearResearchCenterWorkable.cs` lines 36-61

Both station types use `attributeConverter = Db.Get().AttributeConverters.ResearchSpeed` and grant experience to the `Research` skill group.

## Tech Tree Structure

### Tier Calculation

Tech tier is computed recursively in `Techs.GetTier()`:

```
tier = max(tier of all prerequisite techs) + 1
```

Techs with no prerequisites are tier 1. Tier 0 is the empty tier (no costs).

Source: `Database/Techs.cs` lines 593-605

### Tier Costs -- Base Game

| Tier | Basic | Advanced | Space |
|---|---|---|---|
| 0 | -- | -- | -- |
| 1 | 15 | -- | -- |
| 2 | 20 | -- | -- |
| 3 | 30 | 20 | -- |
| 4 | 35 | 30 | -- |
| 5 | 40 | 50 | -- |
| 6 | 50 | 70 | -- |
| 7 | 70 | 100 | -- |
| 8 | 70 | 100 | 200 |
| 9 | 70 | 100 | 400 |
| 10 | 70 | 100 | 800 |
| 11 | 70 | 100 | 1600 |

Source: `Database/Techs.cs` lines 17-77

### Tier Costs -- Spaced Out! (Expansion 1)

| Tier | Basic | Advanced | Orbital | Nuclear |
|---|---|---|---|---|
| 0 | -- | -- | -- | -- |
| 1 | 15 | -- | -- | -- |
| 2 | 20 | -- | -- | -- |
| 3 | 30 | 20 | -- | -- |
| 4 | 35 | 30 | -- | -- |
| 5 | 40 | 50 | 0 | 20 |
| 6 | 50 | 70 | 30 | 40 |
| 7 | 70 | 100 | 250 | 370 |
| 8 | 100 | 130 | 400 | 435 |
| 9 | 100 | 130 | 600 | -- |
| 10 | 100 | 130 | 800 | -- |
| 11 | 100 | 130 | 1600 | -- |

Note: Tier 5 has orbital cost of 0, meaning the orbital column exists but costs nothing at that tier. Nuclear costs stop after tier 8.

Source: `Database/Techs.cs` lines 81-148

### Cost Assignment

During `Techs.Load()`, each tech's tier is computed and then the corresponding tier cost list is applied. Costs are stored in `Tech.costsByResearchTypeID`. A tech can also have `overrideDefaultCosts` passed to its constructor (only applied when Expansion 1 is active).

Source: `Database/Techs.cs` lines 573-583, `Tech.cs` lines 37-49

### Prerequisites

Prerequisites are set during `Techs.Load()` by reading a `TextAsset` tree file (a visual graph). Each node's `references` become prerequisite links:

- `tech2.requiredTech.Add(tech)` -- the referenced tech requires the referencing tech
- `tech.unlockedTech.Add(tech2)` -- completing a tech unlocks its children

Source: `Database/Techs.cs` lines 522-571

### Tech Categories

Techs are grouped into visual categories via `TechTreeTitle` nodes in the same tree file. Category IDs are prefixed with `_` (e.g., `_Food`, `_Power`). The display names come from `STRINGS.RESEARCH.TREES.TITLE` strings.

Source: `Database/TechTreeTitles.cs`, `Database/Techs.cs` lines 542-550

## Research Queue System

The queue is managed by `Research` (singleton at `Research.Instance`):

- `queuedTech`: ordered list of `TechInstance` objects waiting to be researched
- `activeResearch`: the `TechInstance` currently receiving points (always `queuedTech[0]`)

### Selecting Research

`SetActiveResearch(tech, clearQueue)`:
1. Optionally clears the queue
2. Calls `AddTechToQueue(tech)` which recursively adds the tech and all its incomplete prerequisites
3. Sorts the queue by tier (ascending) so lowest-tier prerequisites are researched first
4. Sets `activeResearch = queuedTech[0]`
5. Notifies all research centers via `GameHashes.ActiveResearchChanged`

Source: `Research.cs` lines 200-227

### Adding to Queue

`AddTechToQueue(tech)`:
- Skips already-complete techs and techs already in the queue
- Recursively adds all `requiredTech` (prerequisites)

Source: `Research.cs` lines 155-166

### Completing Research

`CheckBuyResearch()`:
1. Checks if active research can afford all costs via `Tech.CanAfford(inventory)`
2. If affordable, deducts points from the inventory for each research type
3. Marks the tech as purchased (`TechInstance.Purchased()`)
4. Fires `GameHashes.ResearchComplete` (-107300940)
5. Calls `GetNextTech()` which removes the completed tech from the queue and promotes the next one

Source: `Research.cs` lines 281-299

### Point Inventory

Each `TechInstance` has its own `progressInventory` (a `ResearchPointInventory`) that tracks accumulated points per research type. When `UseGlobalPointInventory` is true, points go to `Research.globalPointInventory` instead (used for the global point display mode).

Source: `Research.cs` lines 269-279, `ResearchPointInventory.cs`, `TechInstance.cs`

### Canceling Research

`CancelResearch(tech)`:
- Recursively cancels all queued techs that depend on the canceled tech
- Clears active research
- Notifies research centers

Source: `Research.cs` lines 168-189

### Completion Percentage

`TechInstance.GetTotalPercentageComplete()` averages the completion percentage across all required research types. Each type's percentage is `points / cost`, clamped to [0, 1].

Source: `TechInstance.cs` lines 59-81

## Skill Requirements by Research Type

Research stations check for required skill perks. If active research requires a type and no duplicant has the corresponding perk, a notification is shown:

| Research Type | Required Skill Perk |
|---|---|
| `"advanced"` | `AllowAdvancedResearch` |
| `"space"` | `AllowInterstellarResearch` |
| `"nuclear"` | `AllowNuclearResearch` |
| `"orbital"` | `AllowOrbitalResearch` |

Basic research has no skill requirement.

Source: `Research.cs` lines 229-260

## Key Techs for Research Progression

Research buildings are unlocked by specific techs:

| Tech ID | Unlocks |
|---|---|
| `"AdvancedResearch"` | AdvancedResearchCenter, ClusterTelescope, ResetSkillsStation |
| `"DupeTrafficControl"` | CosmicResearchCenter (base game space research) |
| `"CrashPlan"` | OrbitalResearchCenter, DLC1CosmicResearchCenter (Spaced Out!) |
| `"NuclearResearch"` | NuclearResearchCenter, ManualHighEnergyParticleSpawner |
| `"SkyDetectors"` | Telescope, ResearchClusterModule, ClusterTelescopeEnclosed |

Source: `Database/Techs.cs` Init() method
