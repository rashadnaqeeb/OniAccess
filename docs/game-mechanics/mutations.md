# Plant Mutations

How plants acquire mutations through radiation exposure, and what each mutation does. Requires the Spaced Out! DLC (`DlcManager.FeaturePlantMutationsEnabled()` returns true only when Expansion 1 is active).

## Which Plants Can Mutate

Every plant configured with a crop via `EntityTemplates.ExtendEntityToBasicPlant` gets a `MutantPlant` component automatically when the DLC is active. This includes all standard crop plants (Mealwood, Bristle Blossom, Sleet Wheat, etc.). The `SpaceTreeConfig` and `SpaceTreeBranchConfig` also add `MutantPlant` explicitly. Any plant with `MutantPlant` can mutate.

Seeds created by `EntityTemplates.CreateAndRegisterSeedForPlant` inherit their parent plant's `MutantPlant` component and `SpeciesID`.

## Mutation Triggers

Mutations can occur in two places during the harvest cycle:

### 1. Seed Production on Harvest (`SeedProducer.ProduceSeed`)

When a plant with `ProductionType.Harvest` or `HarvestOnly` drops a bonus seed (base 10% chance from `seedDropChances = 0.1f`, modified by the harvester's Seed Harvest Chance attribute), the seed rolls for mutation. The `canMutate` parameter is true for harvest-produced seeds but false for uprooting (`DropSeed` passes `canMutate: false`).

Mutation only rolls if the seed's `MutantPlant` component `IsOriginal` (has no existing mutations).

### 2. Crop Fruit Spawning (`Crop.SpawnSomeFruit`)

When a crop spawns its fruit item and the fruit has a `MutantPlant` component, mutation is rolled only if the parent plant `IsOriginal`. Already-mutated plants never roll again from fruit spawning.

### Mutation Roll Formula (`SeedProducer.RollForMutation`)

```
maxRad = MaxRadiationThreshold attribute value
cellRad = radiation level at the plant's cell (clamped to [0, maxRad])
chance = (cellRad / maxRad) * 0.8
mutates = Random.value < chance
```

The maximum mutation probability is **80%**, reached when the plant's cell radiation equals its `MaxRadiationThreshold`. At zero radiation, the chance is 0%. The formula scales linearly between these bounds.

## Mutation Selection (`PlantMutations.GetRandomMutation`)

When a mutation is rolled, one is chosen uniformly at random from the eligible pool:

```
eligible = all mutations where:
  - originalMutation == false (the "Original" marker is excluded)
  - targetPlantPrefabID is NOT in restrictedPrefabIDs
  - requiredPrefabIDs is empty OR contains targetPlantPrefabID
```

Currently no mutations use `requiredPrefabIDs` or `restrictedPrefabIDs`, so all 10 mutations are equally likely for all plants.

## Maximum Mutations Per Plant

`MutantPlant.MAX_MUTATIONS = 1`. A plant can hold at most one mutation. The `Mutate()` method removes any existing mutations before adding a new random one.

## Sterility

All non-original mutations force `SeedProducer.ProductionType.Sterile` on the plant if its current production type is `Harvest` (`PlantMutation.ApplyFunctionalTo` only converts `Harvest`, not `HarvestOnly` or other types). A mutated plant cannot produce seeds through normal harvest. The only way to get mutant seeds is from the original (non-mutated) plant's mutation roll.

## Radiation Thresholds

Every mutation adds `MinRadiationThreshold = 250` rads/cycle. This means a mutated plant requires at least 250 rads/cycle of ambient radiation to avoid wilting, enforced by `RadiationVulnerable`. The plant wilts if cell radiation drops below `MinRadiationThreshold` or exceeds `MaxRadiationThreshold`.

The `MaxRadiationThreshold` is set per-plant in `EntityTemplates.ExtendEntityToBasicPlant` via the `max_radiation` parameter.

## Identification System

Mutant seeds spawn as unidentified (tagged `GameTags.UnidentifiedSeed`). They display as "Unidentified" until analyzed.

### Genetic Analysis Station

- Building ID: `GeneticAnalysisStation`
- Power: 480 W
- Work time: 150 seconds (modified by Research Speed attribute)
- Required skill perk: `CanIdentifyMutantSeeds` (granted by Farming 3 / Improved Farming II skill)
- Accepts seeds tagged `UnidentifiedSeed` via auto-delivery (capacity 5 kg)

Once analysis completes, `PlantSubSpeciesCatalog.IdentifySubSpecies` is called. All existing seeds and plants of that subspecies across the colony update their names to show the identified mutation. A `GeneticAnalysisCompleteMessage` is queued.

### Auto-Identification Rules

- Seeds planted as growing plants (`GameTags.Plant`) are auto-analyzed on spawn
- Original (non-mutated) seeds are always considered analyzed

## Subspecies Catalog (`PlantSubSpeciesCatalog`)

Each unique combination of species + mutation list forms a subspecies, tracked globally. The subspecies ID is built by concatenating the species tag with mutation IDs (e.g., `"BasicPlantFood_moderatelyTight"`). Original plants get the suffix `_Original`.

The catalog persists across save/load. New subspecies trigger a `NEWMUTANTSEED` notification.

## Mutation Definitions

All mutations are defined in `Database.PlantMutations`. Every mutation adds `MinRadiationThreshold = 250`.

### Attribute Effects

| Mutation | ID | Yield | Growth Time | Fertilizer Use | Temp Range | Other |
|---|---|---|---|---|---|---|
| Moderately Loose | `moderatelyLoose` | -25% | -- | -50% | +50% | -- |
| Moderately Tight | `moderatelyTight` | +50% | -- | -- | -50% | -- |
| Extremely Tight | `extremelyTight` | +100% | -- | -- | -80% | -- |
| Bonus Lice | `bonusLice` | -- | -- | +25% | -- | +1 Meal Lice per harvest |
| Sunny Speed | `sunnySpeed` | -- | -50% (grows 2x faster) | +25% | -- | Requires 1000 lux |
| Slow Burn | `slowBurn` | -- | +350% (grows 4.5x slower) | -90% | -- | -- |
| Blooms | `blooms` | -- | -- | -- | -- | +20 Decor |
| Loaded With Fruit | `loadedWithFruit` | +100% | -- | +20% | -- | +400% harvest time, requires 200 lux |
| Rotten Heaps | `rottenHeaps` | -- | -75% (grows 4x faster) | +50% | -- | +4 kg Rot Pile per harvest, +10000 Food Poisoning germs on harvest, requires darkness |
| Heavy Fruit | `heavyFruit` | -- | -- | +25% | -- | Auto-harvests on grown, max age reduced by 99.9999% |

Notes on attribute modifiers:
- **Yield**: `PlantAttributes.YieldAmount`, multiplier. -0.25 means 75% of base yield; +1.0 means 200%
- **Growth Time**: `Amounts.Maturity.maxAttribute`, multiplier. -0.5 means half the growth time (2x speed); +3.5 means 4.5x the growth time
- **Fertilizer Use**: `PlantAttributes.FertilizerUsageMod`, multiplier. -0.9 means 10% of normal; +0.5 means 150%
- **Temp Range**: `PlantAttributes.WiltTempRangeMod`, multiplier. +0.5 means 50% wider survivable temperature band; -0.8 means 80% narrower
- **Harvest Time**: `PlantAttributes.HarvestTime`, multiplier. +4.0 means 5x longer for a duplicant to harvest

### Bonus Crop Details

| Mutation | Bonus Crop | Amount |
|---|---|---|
| Bonus Lice | `BasicPlantFood` (Meal Lice) | 1 unit |
| Rotten Heaps | `RotPile` (Rot Pile) | 4 kg |

Bonus crops are spawned via `Crop.SpawnSomeFruit` on the harvest event.

### Disease Effects

| Mutation | Disease | On Harvest Amount |
|---|---|---|
| Rotten Heaps | Food Poisoning (`FoodGerms`) | 10,000 germs added to harvested crop |

### Special Behaviors

- **ForcePrefersDarkness** (Rotten Heaps): Overrides `IlluminationVulnerable` to require darkness instead of any light requirement the base plant may have
- **ForceSelfHarvestOnGrown** (Heavy Fruit): Sets `OldAge.maxAttribute` multiplier to -0.999999, effectively reducing max age to near zero so the plant auto-drops its crop immediately on reaching maturity

## Source Files

- `Klei.AI.PlantMutation` -- mutation definition and application logic
- `Database.PlantMutations` -- all mutation instances and random selection
- `MutantPlant` -- component on plants/seeds tracking mutation state
- `PlantSubSpeciesCatalog` -- global subspecies discovery and identification tracking
- `SeedProducer` -- seed production, mutation roll, sterility enforcement
- `Crop` -- fruit spawning with mutation roll for original plants
- `RadiationVulnerable` -- radiation threshold wilt checking
- `GeneticAnalysisStationWorkable` -- seed identification work logic
- `GeneticAnalysisStationConfig` -- building definition (480 W, Farming 3 skill)
- `EntityTemplates` -- plant/seed setup including `MutantPlant` registration
