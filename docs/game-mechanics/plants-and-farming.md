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

### Frost Blossom / Hard Skin Berry Plant (HardSkinBerryPlant) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Crop | Frost Bun (`HardSkinBerry`) |
| Growth time | 1800s (3 cycles domestic, 12 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 118.15 / 218.15 / 259.15 / 269.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | Phosphorite, 1/120 kg/s (5 kg/cycle) |
| Irrigation | None |
| Light | None |
| Tinker | Yes |

### Ice Root / Carrot Plant (CarrotPlant) -- Frosty Planet DLC

| Property | Value |
|----------|-------|
| Crop | Carrot (`Carrot`) |
| Growth time | 5400s (9 cycles domestic, 36 cycles wild) |
| Yield | 1 unit per harvest |
| Temperature | 118.15 / 218.15 / 259.15 / 269.15 K |
| Atmosphere | Oxygen, Polluted Oxygen, Carbon Dioxide |
| Pressure | Sensitive, min 0.15 kg |
| Fertilizer | None |
| Irrigation | Ethanol, 0.025 kg/s (15 kg/cycle) |
| Light | None |
| Tinker | Yes |

## Growth Time Summary

Sorted by domestic growth time (cycles = cropDuration / 600):

| Plant | Crop | Domestic cycles | Wild cycles |
|-------|------|---------------:|------------:|
| Thimble Reed | Reed Fiber | 2.0 | 8.0 |
| Mealwood | Meal Lice | 3.0 | 12.0 |
| Frost Blossom | Frost Bun | 3.0 | 12.0 |
| Worm Plant | Grubfruit | 4.0 | 16.0 |
| Bristle Blossom | Bristle Berry | 6.0 | 24.0 |
| Dasha Salt Vine | Salt | 6.0 | 24.0 |
| Bog Bucket | Bog Jelly | 6.6 | 26.4 |
| Dusk Cap | Mushroom | 7.5 | 30.0 |
| Pincha Pepper | Peppernut | 8.0 | 32.0 |
| Carrot Plant | Carrot | 9.0 | 36.0 |
| Waterweed | Lettuce | 12.0 | 48.0 |
| Balm Lily | Balm Lily Flower | 12.0 | 48.0 |
| Sleet Wheat | Wheat Grain (x18) | 18.0 | 72.0 |
| Nosh Bean | Nosh Bean (x12) | 21.0 | 84.0 |

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
