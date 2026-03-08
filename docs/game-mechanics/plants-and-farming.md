# Plants and Farming

Plant growth mechanics, farm buildings, and per-plant data. Derived from decompiled source code.

## Growth Cycle

Plant growth is controlled by the `Growing` state machine. Plants have a **maturity** amount that increases from 0 to a species-specific maximum. When maturity reaches 100%, the plant transitions to the `grown` state and can be harvested.

**State machine:**
```
growing.wild      -- wild growth rate applied
growing.planted   -- domestic growth rate applied
stalled           -- wilting or custom stall condition; no growth
grown.idle        -- mature, awaiting harvest; old age timer ticking
```

**Growth rates (from `TUNING.CROPS`):**
- Domestic: `0.0016666667` maturity per second (1/600 per second = 1 unit per cycle)
- Wild: `0.00041666668` maturity per second (exactly 0.25x domestic rate)
- Wild plants grow at **25% of domestic speed** -- 4x longer to harvest

**Maturity-to-time conversion:** The `cropDuration` in `CROP_TYPES` is in seconds. `ExtendEntityToBasicPlant` divides by 600 (one cycle = 600 seconds) to get the maturity max attribute. Growth time in cycles = `cropDuration / 600`.

**Old age:** After reaching maturity, plants enter `grown.idle` where an `oldAge` amount ticks up at rate 1.0/s. When `oldAge` reaches `maxAge` (default 2400s = 4 cycles), the plant auto-harvests itself. This ensures unharvested plants eventually drop their crop and restart growth. `shouldGrowOld` can be set to false (e.g., Arbor Tree) to disable auto-harvest.

**Self-harvest time:** `SELF_HARVEST_TIME = 2400f` seconds (4 cycles after maturity).

## Wilt Conditions

Plants stop growing (enter `stalled` state) when any wilt condition is active. Wilt causes include:

- **Temperature:** `TemperatureVulnerable` -- each plant has 4 temperature thresholds: lethal low, warning low (growth stops), warning high (growth stops), lethal high. Reaching lethal temperatures kills the plant.
- **Pressure:** `PressureVulnerable` -- minimum/maximum atmospheric pressure. Also enforces safe atmosphere elements.
- **Wrong atmosphere:** Plant must be in one of its listed safe gas elements.
- **Irrigation:** `IrrigationMonitor` -- insufficient liquid delivery to the farm plot.
- **Fertilization:** `FertilizationMonitor` -- insufficient solid fertilizer delivery.
- **Light:** `IlluminationVulnerable` -- some plants require light (Bristle Blossom needs 200 lux), some require darkness (Dusk Cap, Bog Jelly).
- **Entombed:** `EntombVulnerable` -- buried in solid tiles.
- **Drowning:** `DrowningMonitor` -- submerged (except aquatic plants like Waterweed).
- **Uprooted:** `UprootedMonitor` -- no solid tile below (or above for hanging plants).

When a wilt condition clears, the plant receives `WiltRecover` and resumes growth from where it stopped. Growth progress is never lost from wilting.

## Farm Buildings

Three buildings serve as `PlantablePlot` receptacles for seeds:

### Farm Tile (`FarmTile`)
- 1x1 tile, acts as a floor tile
- Built from farmable materials (Dirt, etc.)
- Accepts fertilizer (solid delivery by duplicants)
- No liquid piping -- irrigation is delivered manually by duplicants
- Can be flipped vertically for ceiling-mounted plants
- Accepts both `CropSeed` and `WaterSeed` tags

### Hydroponic Farm (`HydroponicFarm`)
- 1x1 tile, acts as a floor tile
- Built from metals
- Accepts fertilizer (solid delivery by duplicants)
- Has liquid pipe input -- irrigation delivered via plumbing
- `has_liquid_pipe_input = true` disables manual liquid delivery by duplicants
- Can be flipped vertically
- Accepts both `CropSeed` and `WaterSeed` tags

### Planter Box (`PlanterBox`)
- 1x1 freestanding building (placed on floor, not a tile)
- Built from farmable materials
- `IsOffGround = true` -- plants in planter boxes are tagged `PlantedOnFloorVessel`
- Accepts fertilizer (solid delivery by duplicants)
- No liquid piping
- Only accepts `CropSeed` tag (not `WaterSeed`)
- No `-0.5` growth penalty despite wiki claims -- the `PLANTERPLOT_GROWTH_PENTALY` (sic) constant exists in `TUNING.CROPS` at `-0.5` but is not applied anywhere in the decompiled source

All three buildings enable domestic growth rates when a plant is planted in them.

## Wild vs Domestic

A plant's growth rate depends on `ReceptacleMonitor.Replanted`:
- **Wild** (`IsWildPlanted() == true`): Not in a farm building. Grows at `WILD_GROWTH_RATE` (25% speed). No fertilizer or irrigation required. Wilting still applies for temperature/atmosphere.
- **Domestic** (`IsWildPlanted() == false`): Planted in a farm building. Grows at `GROWTH_RATE` (full speed). Requires any specified fertilizer and irrigation to avoid wilting.

When a wild plant is uprooted by a duplicant, it drops its seed. Replanting that seed in a farm building makes it domestic.

## Seed Mechanics

Seeds are created by `EntityTemplates.CreateAndRegisterSeedForPlant` with a `SeedProducer.ProductionType`:

| ProductionType | Seed behavior |
|----------------|---------------|
| `Harvest` | Bonus seed chance on harvest (default 10%) |
| `Crop` | Seed is the crop itself (Sleet Wheat, Nosh Bean) |
| `Hidden` | Seed only drops on uproot, not harvest (Arbor Tree, Wheezewort, Oxyfern) |
| `HarvestOnly` | Seed only from harvest, not from uproot |
| `Sterile` | No seed production |

**Bonus seed chance:** `SeedProducer.seedDropChances` defaults to `0.1` (10%). On harvest, the chance is: `base_chance + farmer_attribute_bonus`. The duplicant's Agriculture skill provides `SeedHarvestChance` attribute converter bonus. If the roll succeeds, one extra seed is produced alongside the crop.

**Crop-type seeds:** For `ProductionType.Crop` (Sleet Wheat Grain, Nosh Bean), the seed tag `ignoreDefaultSeedTag` is true -- they are tagged as food items, not generic seeds. The "crop" is the seed itself, and planting one uses it up.

## Fertilizer and Irrigation Delivery

When a plant is placed in a farm building:
- `FertilizationMonitor` / `IrrigationMonitor` track resource levels
- `ManualDeliveryKG` components are added for each consumed element
- Delivery capacity = `massConsumptionRate * 600 * 3` (3 cycles of supply)
- Refill threshold = `massConsumptionRate * 600 * 0.5` (half a cycle remaining)
- For Hydroponic Farms with `has_liquid_pipe_input`, manual liquid delivery is disabled; liquid comes through the pipe

When resources are depleted, the plant enters a starved wilt state. It resumes growth when enough resources are delivered (need at least 30 seconds worth to enter recovery).

## Farm Tinker

Plants with `can_tinker = true` can be tended by a duplicant with the Farmer's Touch skill. `Tinkerable.MakeFarmTinkerable` adds this capability. Tending applies a temporary growth speed bonus.

## Key Plants Reference

All temperatures in Kelvin. Consumption rates in kg/s. Growth times from `CROP_TYPES` `cropDuration` field. Temperature rows show four thresholds from `TemperatureVulnerable`: lethal low (plant dies), warning low (growth stops), warning high (growth stops), lethal high (plant dies). The plant grows normally between warning low and warning high.

### Mealwood (BasicSingleHarvestPlant)

| Property | Value |
|----------|-------|
| Crop | Meal Lice (`BasicPlantFood`) |
| Growth time | 1800s (3 cycles domestic, 12 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 218.15 / 283.15 / 303.15 / 398.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Dirt, 1/60 kg/s (10 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | No |

### Bristle Blossom (PrickleFlower)

| Property | Value |
|----------|-------|
| Crop | Bristle Berry (`PrickleFruit`) |
| Growth time | 3600s (6 cycles domestic, 24 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 218.15 / 278.15 / 303.15 / 398.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | None |
| Irrigation | Water, 1/30 kg/s (20 kg/cycle) |
| Light | Requires 200 lux minimum |
| Tinker | Yes |

### Dusk Cap (MushroomPlant)

| Property | Value |
|----------|-------|
| Crop | Mushroom (`Mushroom`) |
| Growth time | 4500s (7.5 cycles domestic, 30 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 228.15 / 278.15 / 308.15 / 398.15 K |
| Atmosphere | Carbon Dioxide only |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Slime, 1/150 kg/s (4 kg/cycle) |
| Irrigation | None |
| Light | Requires darkness |
| Tinker | Yes |

### Sleet Wheat (ColdWheat)

| Property | Value |
|----------|-------|
| Crop | Sleet Wheat Grain (`ColdWheatSeed`) -- seed is the crop |
| Growth time | 10800s (18 cycles domestic, 72 cycles wild) |
| Yield | 18 grains per harvest |
| Temperature | 118.15 / 218.15 / 278.15 / 358.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Dirt, 1/120 kg/s (5 kg/cycle) |
| Irrigation | Water, 1/30 kg/s (20 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Seed type | Crop (seed = food item) |

### Pincha Peppernut / Spice Vine (SpiceVine)

| Property | Value |
|----------|-------|
| Crop | Pincha Peppernut (`SpiceNut`) |
| Growth time | 4800s (8 cycles domestic, 32 cycles wild) |
| Yield | 4 units per harvest |
| Temperature | 258.15 / 308.15 / 358.15 / 448.15 K |
| Atmosphere | Any (pressure sensitive, min 0.15 kg) |
| Fertilizer | Phosphorite, 0.0016667 kg/s (1 kg/cycle) |
| Irrigation | Polluted Water, 7/120 kg/s (35 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Orientation | Hanging (grows downward, needs ceiling tile) |

### Waterweed / Sea Lettuce (SeaLettuce)

| Property | Value |
|----------|-------|
| Crop | Lettuce |
| Growth time | 7200s (12 cycles domestic, 48 cycles wild) |
| Yield | 12 units per harvest |
| Temperature | 248.15 / 295.15 / 338.15 / 398.15 K |
| Atmosphere | Water, Salt Water, Brine (submerged aquatic plant) |
| Pressure | Not pressure sensitive |
| Fertilizer | Bleach Stone, 0.00083333 kg/s (0.5 kg/cycle) |
| Irrigation | Salt Water, 1/120 kg/s (5 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Special | Lives underwater, cannot drown |

### Thimble Reed (BasicFabricPlant)

| Property | Value |
|----------|-------|
| Crop | Reed Fiber (`BasicFabric`) |
| Growth time | 1200s (2 cycles domestic, 8 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 248.15 / 295.15 / 310.15 / 398.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide, Polluted Water, Water |
| Pressure | Not pressure sensitive |
| Fertilizer | None |
| Irrigation | Polluted Water, 4/15 kg/s (160 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Special | Cannot drown (`can_drown: false`); safe in water/polluted water atmospheres |

### Balm Lily / Buddy Bud (SwampLily)

| Property | Value |
|----------|-------|
| Crop | Balm Lily Flower (`SwampLilyFlower`) |
| Growth time | 7200s (12 cycles domestic, 48 cycles wild) |
| Yield | 2 units per harvest |
| Temperature | 258.15 / 308.15 / 358.15 / 448.15 K |
| Atmosphere | Chlorine only |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | None |
| Irrigation | None |
| Light | None |
| Tinker | Yes |

### Arbor Tree (ForestTree)

| Property | Value |
|----------|-------|
| Crop | Wood (via branches, not direct harvest), 300 kg per branch |
| Branch growth time | 2700s per branch (4.5 cycles domestic) |
| Branch count | Up to 5 active (7 possible positions) |
| Temperature | 258.15 / 288.15 / 313.15 / 448.15 K |
| Atmosphere | Any (pressure sensitive, min 0.15 kg) |
| Fertilizer | Dirt, 1/60 kg/s (10 kg/cycle) |
| Irrigation | Polluted Water, 7/60 kg/s (70 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Special | `shouldGrowOld = false`, trunk never auto-harvests. Uses `PlantBranchGrower` -- branches grow at 7 offset positions. Seeds produced via `ForestTreeSeedMonitor` on branch harvest, not standard harvest. Seed production type is Hidden. |

### Oxyfern (Oxyfern)

| Property | Value |
|----------|-------|
| Crop | None (utility plant, converts CO2 to O2) |
| Growth time | N/A (no crop_id, but has maturity for lifecycle) |
| Temperature | 253.15 / 273.15 / 313.15 / 373.15 K |
| Atmosphere | Carbon Dioxide only |
| Pressure | Sensitive, min 0.025 kg |
| Fertilizer | Dirt, 1/150 kg/s (4 kg/cycle) |
| Irrigation | Water, 0.031667 kg/s (19 kg/cycle) |
| Light | None |
| Tinker | No |
| Special | Consumes CO2 at 0.000625 kg/s, produces O2 at 0.03125 kg/s (50x conversion ratio). Seed production type is Hidden. |

### Wheezewort (ColdBreather)

| Property | Value |
|----------|-------|
| Crop | None (utility plant, cools gas) |
| Temperature | 183.15 / 213.15 / 368.15 / 463.15 K |
| Fertilizer | Phosphorite, 1/150 kg/s (4 kg/cycle) |
| Light | None |
| Special | Not a standard crop plant. Cools gas by -5 C delta. Consumes surrounding gas at 0.25 kg/s and re-emits it cooled. Emits 480 rads (Spaced Out). Seed production type is Hidden. |

### Nosh Bean / Nosh Sprout (BeanPlant)

| Property | Value |
|----------|-------|
| Crop | Nosh Bean (`BeanPlantSeed`) -- seed is the crop |
| Growth time | 12600s (21 cycles domestic, 84 cycles wild) |
| Yield | 12 beans per harvest |
| Temperature | 198.15 / 248.15 / 273.15 / 323.15 K |
| Atmosphere | Carbon Dioxide only |
| Pressure | Sensitive, min 0.025 kg |
| Fertilizer | Dirt, 1/120 kg/s (5 kg/cycle) |
| Irrigation | Ethanol, 1/30 kg/s (20 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Seed type | Crop (seed = food item) |

### Dasha Salt Vine (SaltPlant)

| Property | Value |
|----------|-------|
| Crop | Salt |
| Growth time | 3600s (6 cycles domestic, 24 cycles wild) |
| Yield | 65 kg per harvest |
| Temperature | 198.15 / 248.15 / 323.15 / 393.15 K |
| Atmosphere | Chlorine only |
| Pressure | Sensitive, min 0.025 kg |
| Fertilizer | Sand, 0.011667 kg/s (7 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Orientation | Hanging |
| Special | Consumes Chlorine gas at 0.006 kg/s |

### Bog Jelly / Bog Bucket (SwampHarvestPlant) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Crop | Bog Jelly (`SwampFruit`) |
| Growth time | 3960s (6.6 cycles domestic, 26.4 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 218.15 / 283.15 / 303.15 / 398.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | None |
| Irrigation | Polluted Water, 1/15 kg/s (40 kg/cycle) |
| Light | Requires darkness |
| Tinker | Yes |

### Grubfruit Plant / Worm Plant (WormPlant) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Crop | Grubfruit (`WormBasicFruit`) |
| Growth time | 2400s (4 cycles domestic, 16 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 273.15 / 288.15 / 323.15 / 373.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Sulfur, 1/60 kg/s (10 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Special | Transforms into Super Worm Plant (yields Spindly Grubfruit) when tended by a Sweetle critter. Uses `TransformingPlant` on `CropTended` event from `DivergentSpecies`. |

### Pikeapple Bush (HardSkinBerryPlant) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Crop | Pikeapple (`HardSkinBerry`) |
| Growth time | 1800s (3 cycles domestic, 12 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 118.15 / 218.15 / 259.15 / 269.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Phosphorite, 1/120 kg/s (5 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |

### Plume Squash Plant (CarrotPlant) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Crop | Plume Squash (`Carrot`) |
| Growth time | 5400s (9 cycles domestic, 36 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 118.15 / 218.15 / 259.15 / 269.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | None |
| Irrigation | Ethanol, 0.025 kg/s (15 kg/cycle) |
| Light | None |
| Tinker | Yes |

### Gas Grass (GasGrass)

| Property | Value |
|----------|-------|
| Crop | Plant Fiber (`PlantFiber`) |
| Growth time | 2400s (4 cycles domestic, 16 cycles wild) |
| Yield | 400 kg per harvest |
| Temperature | 218.15 / 0 / 348.15 / 373.15 K |
| Atmosphere | Any (not pressure sensitive) |
| Fertilizer | Dirt, 1/24 kg/s (~25 kg/cycle) |
| Irrigation | Chlorine, 0.00083333 kg/s (0.5 kg/cycle) |
| Light | Requires 10000 lux minimum |
| Tinker | Yes |
| Special | Warning low temperature of 0 K means it effectively never wilts from cold above lethal low. `SetPrefersDarkness()` is called with default parameter `false`, so it requires light (not darkness). The 10000 lux `MinLightLux` attribute sets the light threshold. |

### Critter Trap Plant / Bluff Briar (CritterTrapPlant) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Crop | Plant Meat (`PlantMeat`) |
| Growth time | 18000s (30 cycles domestic, 120 cycles wild) |
| Yield | 10 units per harvest |
| Temperature | 173 / 183 / 273 / 283 K |
| Atmosphere | Any (not pressure sensitive) |
| Fertilizer | None |
| Irrigation | Polluted Water, 1/60 kg/s (10 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Seed type | Hidden (seed only from uproot) |
| Special | Traps walker and hoverer critters. Outputs Hydrogen gas at 1/24 kg/s after reaching 33.25 kg threshold. `shouldGrowOld = false`. |

### Filter Plant (FilterPlant) -- Spaced Out DLC (Deprecated)

| Property | Value |
|----------|-------|
| Crop | Water |
| Growth time | 6000s (10 cycles domestic, 40 cycles wild) |
| Yield | 350 kg per harvest |
| Temperature | 253.15 / 293.15 / 383.15 / 443.15 K |
| Atmosphere | Oxygen only |
| Pressure | Sensitive, min 0.025 kg |
| Fertilizer | Sand, 1/120 kg/s (5 kg/cycle) |
| Irrigation | Polluted Water, 13/120 kg/s (~65 kg/cycle) |
| Light | None |
| Tinker | Yes |
| Special | Consumes Oxygen gas at 1/120 kg/s. Tagged as `DeprecatedContent` -- no longer obtainable in normal gameplay. Uses `SaltPlant` component (consumes gas element). |

### Bonbon Tree / Space Tree (SpaceTree) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Crop | Sugar Water (via branches and storage) |
| Trunk growth time | 2700s (4.5 cycles) |
| Sugar Water production | 4 kg/cycle, 20 kg storage capacity, 150s optimal production duration |
| Branch count | Up to 5 branches |
| Temperature | 173.15 / 198.15 / 258.15 / 293.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide, Snow, Vacuum (not pressure sensitive) |
| Fertilizer | Snow, 1/6 kg/s (100 kg/cycle) |
| Irrigation | None |
| Light | Requires 300 lux minimum to grow branches |
| Tinker | No |
| Seed type | Harvest |
| Special | `shouldGrowOld = false`. Uses `PlantBranchGrower` with `SpaceTreeBranch` branches. Sugar Water stored internally, dispensable via liquid conduit. Can be manually harvested for syrup. Has `UnstableEntombDefense` (shakes free when entombed, 5s cooldown). |

### Blue Grass (BlueGrass) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Crop | Oxylite (`OxyRock`) |
| Growth time | 1200s (2 cycles domestic, 8 cycles wild) |
| Yield | ~36 kg per harvest (2 * 17.76 rounded) |
| Temperature | 193.15 / 193.15 / 273.15 / 273.15 K |
| Atmosphere | Carbon Dioxide only |
| Pressure | Sensitive (min 0 kg) |
| Fertilizer | Ice, 1/30 kg/s (~20 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Special | Consumes CO2 gas at 0.0005 kg/s. Warning and lethal temperatures are identical (193.15 / 273.15), meaning there is no warning buffer -- the plant dies immediately at boundary temperatures. |

### Garden Food Plant (GardenFoodPlant) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Garden Food (`GardenFoodPlantFood`) |
| Growth time | 1800s (3 cycles domestic, 12 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 263.15 / 268.15 / 313.15 / 323.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Peat, 1/60 kg/s (10 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Special | Has `PollinationMonitor` -- can be pollinated. Uses `DirectlyEdiblePlant_Growth` (critters can eat it while growing). |

### Vine Mother (VineMother) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Vine Fruit (via branches), 1 fruit per branch per 3-cycle harvest |
| Branch count | Up to 24 branches |
| Temperature | 273.15 / 298.15 / 318.15 / 378.15 K |
| Atmosphere | Oxygen, Carbon Dioxide, Polluted Oxygen (not pressure sensitive) |
| Fertilizer | None |
| Irrigation | Water, 0.15 kg/s (90 kg/cycle) |
| Light | None |
| Tinker | No (trunk); Yes (branches) |
| Seed type | Hidden (trunk); HarvestOnly with 1/6 chance (branches) |
| Special | `shouldGrowOld = false`. Uses `VineMother.Def` branching system (not `PlantBranchGrower`). Branches grow as separate `VineBranch` entities that produce Vine Fruit (`VineFruit`, 1800s per harvest per branch) and Plant Fiber (6 kg/cycle per branch). Branches share parent's temperature thresholds. |

### Kelp Plant (KelpPlant) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Kelp |
| Growth time | 3000s (5 cycles domestic, 20 cycles wild) |
| Yield | 50 kg per harvest |
| Temperature | 253.15 / 263.15 / 358.15 / 373.15 K |
| Atmosphere | Water, Dirty Water, Salt Water, Brine, Phyto Oil, Natural Resin (submerged aquatic plant) |
| Pressure | Not pressure sensitive (but `allCellsMustBeSafe = true`) |
| Fertilizer | Toxic Sand, 1/60 kg/s (10 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Orientation | Hanging (grows downward, needs ceiling tile) |
| Special | Cannot drown. Uses `DirectlyEdiblePlant_Growth`. All cells must be submerged in a safe element. |

### Dew Dripper Plant (DewDripperPlant) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Dew Drip |
| Growth time | 1200s (2 cycles domestic, 8 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 218.15 / 238.15 / 278.15 / 308.15 K |
| Atmosphere | Any |
| Pressure | Sensitive, min 0.25 kg; max warning 2 kg, max lethal 10 kg |
| Fertilizer | Brine Ice, 1/60 kg/s (10 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | No |
| Orientation | Hanging (grows downward, needs ceiling tile) |
| Special | Also produces Plant Fiber at 2 kg/cycle via `PlantFiberProducer`. Has both minimum and maximum pressure thresholds. |

### Fly Trap Plant (FlyTrapPlant) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Amber |
| Growth time | 7200s (12 cycles domestic, 48 cycles wild) |
| Yield | 264 kg per harvest |
| Temperature | 273.15 / 283.15 / 328.15 / 348.15 K |
| Atmosphere | Any |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | None |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Orientation | Hanging (grows downward, needs ceiling tile) |
| Special | Has `FlytrapConsumptionMonitor` -- digests trapped critters over 12 cycles (7200s). Spawns with 0% maturity (`MaxMaturityValuePercentageToSpawnWith = 0`). |

### Butterfly Plant (ButterflyPlant) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Butterfly (food item) |
| Growth time | 3000s (5 cycles domestic, 20 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 233.15 / 283.15 / 318.15 / 353.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide, Chlorine |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Dirt, 1/60 kg/s (10 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |
| Seed type | Crop (seed = food item, `ignoreDefaultSeedTag = true`) |
| Special | `maxAge = 0` (no old age auto-harvest). `HarvestDesignatable` is destroyed -- cannot be manually designated for harvest. Seed is a food item (`BUTTERFLY_SEED`). |

### Dinofern (Dinofern) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Crop | Fern Food (`FernFood`) |
| Growth time | 5400s (9 cycles domestic, 36 cycles wild) |
| Yield | 36 units per harvest |
| Temperature | 218.15 / 228.15 / 288.15 / 308.15 K |
| Atmosphere | Chlorine only |
| Pressure | Sensitive, min 0.5 kg |
| Fertilizer | None |
| Irrigation | None |
| Light | None |
| Tinker | No |
| Seed type | Hidden |
| Size | 3x3 tiles |
| Special | Consumes Chlorine gas at 0.09 kg/s (54 kg/cycle). Large 3x3 plant. Uses `Dinofern` component for consumption rate management. |

## Decorative Plants

Decorative plants provide decor bonuses when healthy and decor penalties when wilting. They produce no harvestable crop. All use `DecorPlantMonitor` and `PrickleGrass` components for decor state tracking. Seeds are tagged `DecorSeed` (not `CropSeed`) and can only be planted in Flower Pots. Seed production type is Hidden for all decorative plants (seeds only from uproot).

### Bristle Briar / Bliss Burst (PrickleGrass)

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 218.15 / 283.15 / 303.15 / 398.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 900 |
| DLC | Base game |

### Buddy Bud (BulbPlant)

| Property | Value |
|----------|-------|
| Decor | +15 radius 2 (healthy) / -20 radius 4 (wilting) |
| Temperature | 288 / 293.15 / 313.15 / 333.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 2200 |
| DLC | Base game |
| Special | Emits Pollen Germs at 5000/s average. |

### Mirth Leaf (LeafyPlant)

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 288 / 293.15 / 323.15 / 373 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide, Chlorine, Hydrogen |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 2200 |
| DLC | Base game |

### Jumping Joya (CactusPlant)

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 200 / 273.15 / 373.15 / 400 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Not pressure sensitive |
| Max radiation | 2200 |
| DLC | Base game |

### Sporechid (EvilFlower)

| Property | Value |
|----------|-------|
| Decor | +80 radius 7 (healthy) / -25 radius 6 (wilting) |
| Temperature | 168.15 / 258.15 / 513.15 / 563.15 K |
| Atmosphere | Carbon Dioxide only |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 12200 |
| DLC | Base game |
| Special | Emits Zombie Spores at 1000/s average (100000 per burst, every 1s). Highest decor bonus of any decorative plant but extremely dangerous. Uses `EvilFlower` component instead of `PrickleGrass`. |

### Tranquil Toes (ToePlant) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 173 / 183 / 273 / 283 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 2200 |

### Cylindrica (Cylindrica) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 288.15 / 293.15 / 323.15 / 373.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 2200 |

### Wine Cups (WineCups) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 218.15 / 283.15 / 303.15 / 398.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 900 |

### Ice Flower (IceFlower) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 173.15 / 203.15 / 278.15 / 318.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide, Chlorine, Hydrogen |
| Pressure | Sensitive, min 0.15 kg |
| Max radiation | 2200 |

### Garden Decor Plant (GardenDecorPlant) -- Biome Bundle DLC

| Property | Value |
|----------|-------|
| Decor | +25 radius 4 (healthy) / -20 radius 4 (wilting) |
| Temperature | 263.15 / 268.15 / 313.15 / 323.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Not pressure sensitive |
| Max radiation | 2200 |

## Special Plants

### Sap Tree (SapTree) -- Spaced Out DLC

| Property | Value |
|----------|-------|
| Decor | +35 radius 6 |
| Size | 5x5 tiles |
| Temperature | 0 / 173.15 / 373.15 / 1023.15 K |
| Special | Hostile environmental entity, not a farm plant. Cannot be planted or harvested. Attacks nearby duplicants with area-of-effect damage (5 damage, radius 2, 5s cooldown). Consumes food items at 0.05 kg/s and converts to resin at 0.005 kcal-to-kg ratio (5 kg stomach capacity, oozes at 2 kg/s). Aligned to Hostile faction. Has `WiltCondition`, `TemperatureVulnerable`, and `EntombVulnerable` but no standard plant growth. |

## Growth Time Summary

Sorted by domestic growth time (cycles = cropDuration / 600):

| Plant | Crop | Domestic cycles | Wild cycles |
|-------|------|---------------:|------------:|
| Blue Grass | Oxylite (~36 kg) | 2.0 | 8.0 |
| Dew Dripper Plant | Dew Drip | 2.0 | 8.0 |
| Thimble Reed | Reed Fiber | 2.0 | 8.0 |
| Garden Food Plant | Garden Food | 3.0 | 12.0 |
| Mealwood | Meal Lice | 3.0 | 12.0 |
| Pikeapple Bush | Pikeapple | 3.0 | 12.0 |
| Gas Grass | Plant Fiber (400 kg) | 4.0 | 16.0 |
| Worm Plant | Grubfruit | 4.0 | 16.0 |
| Butterfly Plant | Butterfly | 5.0 | 20.0 |
| Kelp Plant | Kelp (50 kg) | 5.0 | 20.0 |
| Bristle Blossom | Bristle Berry | 6.0 | 24.0 |
| Dasha Salt Vine | Salt | 6.0 | 24.0 |
| Bog Bucket | Bog Jelly | 6.6 | 26.4 |
| Dusk Cap | Mushroom | 7.5 | 30.0 |
| Pincha Pepper | Peppernut | 8.0 | 32.0 |
| Dinofern | Fern Food (x36) | 9.0 | 36.0 |
| Plume Squash Plant | Plume Squash | 9.0 | 36.0 |
| Filter Plant | Water (350 kg) | 10.0 | 40.0 |
| Fly Trap Plant | Amber (264 kg) | 12.0 | 48.0 |
| Waterweed | Lettuce | 12.0 | 48.0 |
| Balm Lily | Balm Lily Flower | 12.0 | 48.0 |
| Sleet Wheat | Wheat Grain (x18) | 18.0 | 72.0 |
| Nosh Bean | Nosh Bean (x12) | 21.0 | 84.0 |
| Critter Trap Plant | Plant Meat (x10) | 30.0 | 120.0 |

## Source References

- `Growing.cs` -- growth state machine, maturity tracking, old age
- `Crop.cs` -- crop value definition, harvest spawning, yield calculation
- `PlantablePlot.cs` -- farm building receptacle for seeds
- `EntityTemplates.cs` -- `ExtendEntityToBasicPlant`, `ExtendPlantToFertilizable`, `ExtendPlantToIrrigated`
- `TemperatureVulnerable.cs` -- temperature state machine with 4 thresholds
- `PressureVulnerable.cs` -- pressure/atmosphere state machine
- `IrrigationMonitor.cs` -- liquid resource tracking for domestic plants
- `FertilizationMonitor.cs` -- solid fertilizer tracking for domestic plants
- `SeedProducer.cs` -- seed drop logic, production types, mutation rolls
- `WildnessMonitor.cs` -- wild/tame state (for critters, not plants; plants use `ReceptacleMonitor.Replanted`)
- `TUNING/CROPS.cs` -- growth rates, crop durations, yield amounts
- Individual `*PlantConfig.cs` files -- per-species parameters
