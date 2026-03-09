# Food

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

---

## MicrobeMusher

- **Name:** Microbe Musher
- **Description:** Musher recipes will keep Duplicants fed, but may impact health and morale over time.
- **Effect:** Produces low quality Food using common ingredients. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 2 x 3
- **Materials:** 400 kg Metal (any)
- **HP:** 30
- **Overheat temp:** 800 K (526.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Power:** 240 W
- **Self-heat:** 2 kDTU/s
- **Exhaust heat:** 0.5 kDTU/s
- **Manually operated:** Yes
- **Liquid input:** Conduit consumer (for Water recipes)
- **Tech:** Basic Farming (FarmingTech)
- **Skill required:** None

### Recipes

| # | Inputs | Output | Time (s) |
|---|--------|--------|----------|
| 1 | 75 kg Dirt + 75 kg Water | 1 Mush Bar | 40 |
| 2 | 2 Meal Lice (BasicPlantFood) + 50 kg Water | 1 Liceloaf (BasicPlantBar) | 50 |
| 3 | 6 Nosh Beans (BeanPlantSeed) + 50 kg Water | 1 Tofu | 50 |
| 4 | 5 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food + 1 Bristle Berry (PrickleFruit) OR 2 Pikeapple (HardSkinBerry) | 1 Fruit Cake | 50 |
| 5 | 1 Meat + 1 Tallow | 1 Pemmican | 50 | *DLC2 (Frosty Planet Pack) only*

Temperature operation: AverageTemperature (output temp = average of input temps).

---

## CookingStation

- **Name:** Electric Grill
- **Description:** Proper cooking eliminates foodborne disease and produces tasty, stress-relieving meals.
- **Effect:** Cooks a wide variety of improved Foods. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 3 x 2
- **Materials:** 400 kg Metal (any)
- **HP:** 30
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** 0 (radius 1) [NONE]
- **Power:** 60 W
- **Self-heat:** 4 kDTU/s
- **Exhaust heat:** 0.5 kDTU/s
- **Manually operated:** Yes
- **Heated output temp:** 368.15 K (95 C)
- **Tech:** Meal Preparation (FineDining)
- **Skill required:** CanElectricGrill
- **Room tag:** CookTop (counts toward Kitchen)

### Recipes

All output temperature operation: Heated (output heated to 368.15 K / 95 C).

| # | Inputs | Output | Time (s) | Notes |
|---|--------|--------|----------|-------|
| 1 | 3 Meal Lice (BasicPlantFood) | 1 Pickled Meal | 30 | SMALL_COOK_TIME |
| 2 | 1 Mush Bar | 1 Fried Mush Bar | 50 | |
| 3 | 1 Mushroom | 1 Fried Mushroom | 50 | |
| 4 | 1 Raw Egg + 2 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food | 1 Pancakes | 50 | |
| 5 | 2 Meat | 1 BBQ (CookedMeat) | 50 | |
| 6 | 1 Fish Meat (FishMeat) OR Shellfish Meat (ShellfishMeat) | 1 Cooked Seafood (CookedFish) | 50 | |
| 7 | 1 Bristle Berry (PrickleFruit) | 1 Grilled Bristle Berry (GrilledPrickleFruit) | 50 | |
| 8 | 1 Swamp Chard Heart (SwampFruit) | 1 Swamp Delights | 50 | *Spaced Out! only* |
| 9 | 3 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food | 1 Frost Bun (ColdWheatBread) | 50 | |
| 10 | 1 Raw Egg | 1 Omelette (CookedEgg) | 50 | |
| 11 | 1 Grubtruffle (WormBasicFruit) | 1 Berry Sludge (WormBasicFood) | 50 | *Spaced Out! only* |
| 12 | 8 Grubfruit (WormSuperFruit) + 4 kg Sucrose | 1 Grubfruit Preserve (WormSuperFood) | 50 | *Spaced Out! only* |
| 13 | 1 Pikeapple (HardSkinBerry) | 1 Baked Pikeapple (CookedPikeapple) | 50 | *DLC2 (Frosty Planet Pack) only* |
| 14 | 1 Butterfly Wing Grain (ButterflyPlantSeed) | 1 Roast Wing Nut (ButterflyFood) | 50 | *DLC4 (Prehistoric Planet Pack) only* |

---

## Deepfryer

- **Name:** Deep Fryer
- **Description:** Everything tastes better when it's deep-fried.
- **Effect:** Uses Tallow to cook a wide variety of improved Foods. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 2 x 2
- **Materials:** 400 kg Metal (any)
- **HP:** 30
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** 0 (radius 1) [NONE]
- **Power:** 480 W
- **Self-heat:** 8 kDTU/s
- **Exhaust heat:** 2 kDTU/s
- **Manually operated:** Yes
- **Heated output temp:** 368.15 K (95 C)
- **Tech:** Gourmet Meal Preparation (FinerDining)
- **Skill required:** CanDeepFry
- **Room requirement:** Kitchen (required)
- **Room tag:** CookTop
- **DLC required:** DLC2 (Frosty Planet Pack)

### Recipes

All output temperature operation: Heated (368.15 K / 95 C).

| # | Inputs | Output | Time (s) |
|---|--------|--------|----------|
| 1 | 1 Carrot + 1 Tallow | 1 Carrot Fries (FriesCarrot) | 50 |
| 2 | 6 Nosh Beans (BeanPlantSeed) + 1 Tallow | 1 Deep Fried Nosh (DeepFriedNosh) | 50 |
| 3 | 1 Fish Meat + 2.4 Tallow + 2 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food | 1 Deep Fried Fish (DeepFriedFish) | 50 |
| 4 | 1 Shellfish Meat + 2.4 Tallow + 2 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food | 1 Deep Fried Shellfish (DeepFriedShellfish) | 50 |

---

## GourmetCookingStation

- **Name:** Gas Range
- **Description:** Luxury meals increase Duplicants' morale and prevent them from becoming stressed.
- **Effect:** Cooks a wide variety of quality Foods. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 3 x 3
- **Materials:** 400 kg Metal (any)
- **HP:** 30
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** 0 (radius 1) [NONE]
- **Power:** 240 W (input offset: 1, 0)
- **Self-heat:** 8 kDTU/s
- **Exhaust heat:** 1 kDTU/s
- **Manually operated:** Yes
- **Heated output temp:** 368.15 K (95 C)
- **Gas input:** Conduit (offset -1, 0) for Methane fuel
- **Fuel:** Methane, stored up to 10 kg. Consumes 0.1 kg/s while active
- **CO2 output:** 0.025 kg/s at 348.15 K (75 C)
- **Tech:** Gourmet Meal Preparation (FinerDining)
- **Skill required:** CanGasRange
- **Room tag:** CookTop
- **Storage:** Output storage 10 kg. All storages preserve/insulate/seal items

### Recipes

All output temperature operation: Heated (368.15 K / 95 C). All cook time: 50 s.

| # | Inputs | Output |
|---|--------|--------|
| 1 | 2 Grilled Bristle Berry (GrilledPrickleFruit) + 2 Pincha Peppernut (SpiceNut) | 1 Stuffed Berry (Salsa) |
| 2 | 1 Fried Mushroom + 4 Lettuce | 1 Mushroom Wrap |
| 3 | 1 BBQ (CookedMeat) + 1 Cooked Seafood (CookedFish) | 1 Surf'n'Turf |
| 4 | 10 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food + 1 Pincha Peppernut (SpiceNut) | 1 Pepper Bread (SpiceBread) |
| 5 | 1 Tofu + 1 Pincha Peppernut (SpiceNut) | 1 Spicy Tofu |
| 6 | 4 Ginger (GingerConfig) + 4 Nosh Beans (BeanPlantSeed) | 1 Curried Beans (Curry) |
| 7 | 1 Omelette (CookedEgg) + 1 Lettuce + 1 Fried Mushroom | 1 Quiche |
| 8 | 1 Frost Bun (ColdWheatBread) + 1 Lettuce + 1 BBQ (CookedMeat) | 1 Burger |
| 9 | 3 Sleet Wheat Grain (ColdWheatSeed) OR Fern Food + 4 Grubfruit (WormSuperFruit) + 1 Grilled Bristle Berry OR 1.6667 Baked Pikeapple OR 6.153 Vine Fruit | 1 Berry Pie | *Spaced Out! only* |

---

## SpiceGrinder

- **Name:** Spice Grinder
- **Description:** Crushed seeds and other edibles make excellent meal-enhancing additives.
- **Effect:** Produces ingredients that add benefits to foods prepared at skilled cooking stations.
- **Size:** 2 x 3
- **Materials:** 400 kg Metal (any)
- **HP:** 30
- **Overheat:** Disabled
- **Decor:** 0 (radius 1) [NONE]
- **Power:** None
- **Manually operated:** Yes (via SpiceGrinderWorkable)
- **Tech:** Food Repurposing (FoodRepurposing)
- **Skill required:** CanSpiceGrinder
- **Room requirement:** Kitchen (required)
- **Logic input:** 1 port at (0, 0)
- **Work time:** 5 s per 1000 kcal
- **Spice capacity:** 10 per ingredient
- **Output temp:** 313.15 K (40 C)
- **Storage 1:** 1 kg capacity for edible food
- **Storage 2:** Holds seeds (crop seeds)

### Spice Recipes (per 1000 kcal of food)

| Spice | Ingredient 1 | Ingredient 2 | Effect | DLC |
|-------|-------------|-------------|--------|-----|
| Preserving Spice | 0.1 kg Mealwood Seed (BasicSingleHarvestPlantSeed) | 3 kg Salt | Rot rate +0.5 (halves spoilage) | Base |
| Piloting Spice | 0.1 kg Mushroom Seed (MushroomSeed) | 3 kg Sucrose | +3 Space Navigation | Spaced Out! |
| Strength Spice | 0.1 kg Waterweed Seed (SeaLettuceSeed) | 3 kg Iron | +3 Strength | Base |
| Machinery Spice | 0.1 kg Bristle Blossom Seed (PrickleFlowerSeed) | 3 kg Slime | +3 Machinery | Base |

---

## FoodDehydrator

- **Name:** Dehydrator
- **Description:** Some of the eliminated liquid inevitably ends up on the floor.
- **Effect:** Uses low, even heat to eliminate moisture from eligible Foods and render them shelf-stable. Dehydrated meals must be processed at the Rehydrator before they can be eaten.
- **Size:** 3 x 3
- **Materials:** 200 kg Refined Metal + 100 kg Plastic
- **HP:** 30
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** 0 (radius 1) [NONE]
- **Power:** None (not electrical)
- **Self-heat:** 4 kDTU/s
- **Manually operated:** Yes (loading/emptying by duplicant, fabrication is automatic)
- **Gas input:** Conduit (offset -1, 0) for Methane fuel
- **Fuel:** Methane, stored up to ~5 kg. Consumes 0.02 kg/s while active
- **CO2 output:** 0.005 kg/s at 348.15 K (75 C), not stored (emitted directly)
- **Logic input:** 1 port at (0, 0)
- **Heated output temp:** 368.15 K (95 C)
- **Tech:** Gourmet Meal Preparation (FinerDining)
- **Skill required:** None
- **Room tag:** IndustrialMachinery
- **Emptying work time:** 50 s

### Recipes

All recipes: 250 s fabrication time. Each consumes 6000 kcal of the input food (amount varies by kcal/unit) + 12 kg Polypropylene (plastic). Outputs 6 dehydrated food packets + 6 kg Water (heated).

| Input Food | Output (dehydrated) |
|-----------|---------------------|
| Stuffed Berry (Salsa) | Dried Salsa |
| Mushroom Wrap | Dried Mushroom Wrap |
| Surf'n'Turf | Dried Surf'n'Turf |
| Pepper Bread (SpiceBread) | Dried Pepper Bread |
| Quiche | Dried Quiche |
| Curried Beans (Curry) | Dried Curry |
| Spicy Tofu | Dried Spicy Tofu |
| Burger | Dried Burger (DehydratedFoodPackage) |
| Berry Pie | Dried Berry Pie | *Spaced Out! only* |

---

## FoodRehydrator

- **Name:** Rehydrator
- **Description:** Rehydrated food is nutritious and only slightly less delicious.
- **Effect:** Restores moisture to convert shelf-stable packaged meals into edible Food.
- **Size:** 1 x 2
- **Materials:** 100 kg Refined Metal + 50 kg Plastic
- **HP:** 10
- **Overheat temp:** 800 K (526.85 C)
- **Decor:** +5 (radius 1) [BONUS.TIER0]
- **Power:** 60 W
- **Self-heat:** 0.5 kDTU/s
- **Floodable:** No
- **Liquid input:** Conduit (offset 0, 0) for Water
- **Water storage:** 20 kg capacity
- **Water cost:** 1 kg per rehydration
- **Dehydrated food storage:** 5 kg capacity
- **Rehydration work time:** 5 s
- **Tech:** Gourmet Meal Preparation (FinerDining)
- **Debuff:** "RehydratedFoodConsumed" - applies -1 Morale for 600 s (1 cycle) after eating

---

## Smoker

- **Name:** Smoker
- **Description:** With a little patience, even tough meat can become deliciously edible.
- **Effect:** Cooks improved foods over low, slow heat. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 4 x 3
- **Materials:** 400 kg Metal (any)
- **HP:** 30
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** 0 (radius 1) [NONE]
- **Power:** None (not electrical)
- **Self-heat:** 8 kDTU/s
- **Exhaust heat:** 1 kDTU/s
- **Manually operated:** Yes (loading/emptying; fabrication is automatic)
- **Heated output temp:** 368.15 K (95 C)
- **Gas output:** Conduit (offset 1, 1) for CO2 output
- **CO2 output:** 0.02 kg/s at 348.15 K (75 C), stored then dispensed via gas pipe
- **Peat fuel:** ManualDeliveryKG, 240 kg capacity, refill at 120 kg, delivered as Peat
- **Tech:** Food Repurposing (FoodRepurposing)
- **Skill required:** CanGasRange
- **Room tag:** CookTop
- **Emptying work time:** 50 s
- **DLC required:** DLC4 (Prehistoric Planet Pack)

### Recipes

All cook time: 600 s. All output temperature operation: Heated.

| # | Inputs | Output |
|---|--------|--------|
| 1 | 6 Dinosaur Meat + 100 kg Wood/Fabricated Wood/Peat | 3.2 Smoked Dinosaur Meat |
| 2 | 6 Fish Meat OR Prehistoric Pacu Fillet + 100 kg Wood/Fabricated Wood/Peat | 4 Smoked Fish |
| 3 | 7 Garden Berry (GardenFoodPlantFood) OR Pikeapple (HardSkinBerry) OR Grubtruffle (WormBasicFruit) + 100 kg Wood/Fabricated Wood/Peat | 4 Smoked Vegetables |

---

## PlanterBox

- **Name:** Planter Box
- **Description:** Domestically grown seeds mature more quickly than wild plants.
- **Effect:** Grows one Plant from a Seed.
- **Size:** 1 x 1
- **Materials:** 100 kg Farmable
- **HP:** 10
- **Overheat:** Disabled
- **Floodable:** No
- **Decor:** -10 (radius 2) [PENALTY.TIER1]
- **Build time:** 3 s
- **Power:** None
- **Placement:** On floor (off-ground planter)
- **Accepts:** Crop seeds
- **Fertilization:** Manual fertilizer delivery (no liquid piping)
- **Tech:** Basic Farming (FarmingTech)

---

## FarmTile

- **Name:** Farm Tile
- **Description:** Duplicants can deliver fertilizer and liquids to farm tiles, accelerating plant growth.
- **Effect:** Grows one Plant from a Seed. Can be used as floor tile and rotated before construction.
- **Size:** 1 x 1
- **Materials:** 100 kg Farmable
- **HP:** 100
- **Overheat:** Disabled
- **Floodable:** No
- **Entombable:** No
- **Decor:** 0 (radius 1) [NONE]
- **Build time:** 30 s
- **Power:** None
- **Placement:** Tile (replaces cell element). Can be rotated (FlipV) for ceiling mounting
- **Accepts:** Crop seeds, Water seeds
- **Fertilization:** Manual fertilizer delivery (no liquid piping)
- **Drag build:** Yes
- **No repair:** BaseTimeUntilRepair = -1
- **Construction offset:** OneDown
- **Tech:** Meal Preparation (FineDining)

---

## HydroponicFarm

- **Name:** Hydroponic Farm
- **Description:** Hydroponic farms reduce Duplicant traffic by automating irrigating crops.
- **Effect:** Grows one Plant from a Seed. Can be used as floor tile and rotated before construction. Must be irrigated through Liquid Piping.
- **Size:** 1 x 1
- **Materials:** 100 kg Metal (any)
- **HP:** 100
- **Overheat:** Disabled
- **Floodable:** No
- **Entombable:** No
- **Decor:** -5 (radius 1) [PENALTY.TIER0]
- **Build time:** 30 s
- **Power:** None
- **Placement:** Tile. Can be rotated (FlipV) for ceiling mounting
- **Liquid input:** Conduit (offset 0, 0), consumes any liquid, 1 kg/s rate, 5 kg buffer. Wrong element dumped
- **Accepts:** Crop seeds, Water seeds
- **Fertilization:** Manual fertilizer + liquid piping
- **Drag build:** No
- **No repair:** BaseTimeUntilRepair = -1
- **Construction offset:** OneDown
- **Does not require conduit to have mass** (requireConduitHasMass = false)
- **Tech:** Agriculture

---

## RationBox

- **Name:** Ration Box
- **Description:** Ration boxes keep food safe from hungry critters, but don't slow food spoilage.
- **Effect:** Stores a small amount of Food. Food must be delivered to boxes by Duplicants.
- **Size:** 2 x 2
- **Materials:** 400 kg Raw Minerals (BuildableRaw)
- **HP:** 10
- **Overheat:** Disabled
- **Floodable:** No
- **Decor:** +5 (radius 1) [BONUS.TIER0]
- **Build time:** 10 s
- **Power:** None
- **Storage capacity:** 150 kg (food only)
- **Tech:** Basic Farming (FarmingTech)
- **Does NOT refrigerate** (food spoils at normal rate)

---

## Refrigerator

- **Name:** Refrigerator
- **Description:** Food spoilage can be slowed by ambient conditions as well as by refrigerators.
- **Effect:** Stores Food at an ideal Temperature to prevent spoilage.
- **Size:** 1 x 2
- **Materials:** 400 kg Raw Minerals (BuildableRaw)
- **HP:** 30
- **Overheat temp:** 800 K (526.85 C)
- **Floodable:** No
- **Decor:** +10 (radius 2) [BONUS.TIER1]
- **Build time:** 10 s
- **Power:** 120 W active, 20 W energy saver (when not actively cooling)
- **Self-heat:** 0.125 kDTU/s
- **Exhaust heat:** 0 kDTU/s
- **Cooling heat:** 0.375 kDTU/s (heat removed from contents while cooling)
- **Steady-state heat:** 0 kDTU/s
- **Storage capacity:** 100 kg (food only)
- **Logic output:** 1 port at (0, 1) - Green when full, Red otherwise
- **Tech:** Agriculture
- **Refrigerates food** (slows spoilage)

---

## CreatureDeliveryPoint

- **Name:** Critter Drop-Off (deprecated, replaced by CritterDropOff)
- **Description:** Duplicants automatically bring captured critters to these relocation points for release.
- **Effect:** Releases trapped Critters back into the world. Can be used multiple times.
- **Size:** 1 x 3
- **Materials:** 50 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 10 s
- **Power:** None
- **Max creatures:** 20
- **Auto-wrangle:** Yes (FixedCapturePoint)
- **Deprecated:** Yes
- **Tech:** Ranching

---

## CritterPickUp

- **Name:** Critter Pick-Up
- **Description:** Duplicants will automatically wrangle excess critters.
- **Effect:** Ensures the prompt relocation of Critters that exceed the maximum amount set. Monitoring and pick-up are limited to the specified species.
- **Size:** 1 x 3
- **Materials:** 50 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 10 s
- **Power:** None
- **Allows babies:** Yes
- **Filtered count:** Yes (counts only filtered species)
- **Logic input:** 1 port ("CritterPickUpInput" at 0, 0) - Green: wrangle excess, Red: ignore
- **Tech:** Ranching

---

## CritterDropOff

- **Name:** Critter Drop-Off
- **Description:** Duplicants automatically bring captured critters to these relocation points for release.
- **Effect:** Releases trapped Critters back into the world. Monitoring and drop-off are limited to the specified species.
- **Size:** 1 x 3
- **Materials:** 50 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 10 s
- **Power:** None
- **Filtered count:** Yes
- **Logic input:** 1 port ("CritterDropOffInput" at 0, 0) - Green: enable drop-off, Red: disable
- **Tech:** Ranching

---

## FishDeliveryPoint

- **Name:** Fish Release
- **Description:** A fish release must be built in liquid to prevent released fish from suffocating.
- **Effect:** Releases trapped Pacu back into the world. Can be used multiple times.
- **Size:** 1 x 3
- **Materials:** 50 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 10 s
- **Power:** None
- **Placement:** Anywhere (not restricted to floor)
- **Max creatures:** 20
- **Cavity offset:** CellOffset.down (counts room below)
- **Requires liquid offset:** Yes (must be submerged)
- **Makes base solid** at cell (0, 0)
- **Tech:** Ranching

---

## CreatureFeeder

- **Name:** Critter Feeder
- **Description:** Critters tend to stay close to their food source and wander less when given a feeder.
- **Effect:** Automatically dispenses food for hungry Critters.
- **Size:** 1 x 2
- **Materials:** 200 kg Metal (raw)
- **HP:** 100
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 120 s
- **Power:** None
- **Storage capacity:** 2000 kg
- **Accepts:** Diet items for: Light Bugs, Hatches, Moles, Pokeshells, Staterpillars, Divergent (Sweetles/Grubgrubs), Bammoths, Belly, Seals, Stegos, Raptors, Chameleons, Gassy Moos. Excludes prey critter items
- **Chore type:** RanchingFetch
- **Tech:** Ranching

---

## FishFeeder

- **Name:** Fish Feeder
- **Description:** Build this feeder above a body of water to feed the fish within.
- **Effect:** Automatically dispenses stored Critter food into the area below. Dispenses continuously as food is consumed.
- **Size:** 1 x 3
- **Materials:** 200 kg Metal (raw)
- **HP:** 100
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 120 s
- **Power:** None
- **Placement:** Anywhere
- **Storage:** Two 200 kg storages ("FishFeederTop" drops at +1y, "FishFeederBot" drops at +3.5y)
- **Makes base solid** at cell (0, 0)
- **Accepts:** Diet items for Pacu and Prehistoric Pacu
- **Feeding effect:** "AteFromFeeder" - 1200 s duration, -1/30 wildness delta/s, +5 Happiness
- **Feeder offset:** (0, -2)
- **Chore type:** RanchingFetch
- **Tech:** Ranching

---

## MilkFeeder

- **Name:** Critter Fountain
- **Description:** It's easier to tolerate overcrowding when you're all hopped up on brackene.
- **Effect:** Dispenses Milk (Brackene) to a wide variety of Critters. Accessing the fountain significantly improves Critters' moods.
- **Size:** 3 x 3
- **Materials:** 400 kg Refined Metal
- **HP:** 100
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Build time:** 120 s
- **Power:** None
- **Liquid input:** Conduit (offset 0, 0) for Milk only, 10 kg/s consumption rate. Wrong element dumped
- **Logic input:** 1 port at (0, 0)
- **Storage capacity:** 80 kg Milk
- **Milk consumed per feeding:** 5 kg
- **Effect:** "HadMilk" - 600 s duration
- **Room requirement:** Creature Pen (required)
- **Room tag:** RanchStationType
- **Can be flipped:** Yes (FlipH)
- **Drink offset:** (1, 0)
- **Tech:** Brackene Flow (DairyOperation)

---

## EggIncubator

- **Name:** Incubator
- **Description:** Incubators can maintain the ideal internal conditions for several species of critter egg.
- **Effect:** Incubates Critter eggs until ready to hatch. Assigned Duplicants must possess the Critter Ranching skill.
- **Size:** 2 x 3
- **Materials:** 200 kg Refined Metal
- **HP:** 30
- **Overheat temp:** 363.15 K (90 C)
- **Decor:** +5 (radius 1) [BONUS.TIER0]
- **Build time:** 120 s
- **Power:** 240 W
- **Self-heat:** 4 kDTU/s
- **Exhaust heat:** 0.5 kDTU/s
- **Accepts:** Eggs (GameTags.Egg)
- **Lullaby work time:** 5 s
- **Skill required:** CanWrangleCreatures
- **Tech:** Animal Control (AnimalControl)

### Incubation mechanics

When the incubator is operational (powered), it keeps the egg's incubation active (not suppressed as it would be in storage). A duplicant can perform the "Lullaby" chore (EggSing), which applies the "EggSong" effect to the egg: +400% incubation rate for 600 s. This is the primary way incubators speed up hatching.

---

## EggCracker

- **Name:** Egg Cracker
- **Description:** Raw eggs are an ingredient in certain high quality food recipes.
- **Effect:** Converts viable Critter eggs into cooking ingredients. Cracked Eggs cannot hatch. Duplicants will not crack eggs unless tasks are queued.
- **Size:** 2 x 2
- **Materials:** 50 kg Metal (raw)
- **HP:** 30
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** +5 (radius 1) [BONUS.TIER0]
- **Build time:** 10 s
- **Power:** None
- **Logic input:** 1 port at (0, 0)
- **Manually operated:** Yes
- **Tech:** Meal Preparation (FineDining)

### Recipes

One recipe per critter species (auto-generated from registered eggs). Each takes 5 s.

- **Input:** 1 egg (any morph of that species)
- **Output:** Raw Egg (50% of egg mass) + Egg Shell (50% of egg mass)
- Temperature operation: AverageTemperature
- Some species may have additional custom outputs

---

## CreatureGroundTrap

- **Name:** Critter Trap
- **Description:** It's designed for land critters, but flopping fish sometimes find their way in too.
- **Effect:** Captures a living Critter for transport. Only Duplicants with the Critter Ranching I skill can arm this trap. It's reusable!
- **Size:** 2 x 2
- **Materials:** 200 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Floodable:** No
- **Build time:** 10 s
- **Power:** None
- **Trappable creatures:** Walkers, Hoverers, Swimmers
- **Logic input:** 1 port at (0, 0) - Green: set trap, Red: disarm and empty
- **Logic output:** 1 port at (1, 0) - Green: critter trapped, Red: empty
- **Tech:** Animal Control (AnimalControl)

---

## WaterTrap

- **Name:** Fish Trap
- **Description:** Trapped fish will automatically be bagged for transport.
- **Effect:** Attracts and traps swimming Pacu. Only Duplicants with the Critter Ranching I skill can arm this trap. It's reusable!
- **Size:** 1 x 2
- **Materials:** 200 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Floodable:** No
- **Placement:** Anywhere
- **Build time:** 10 s
- **Power:** None
- **Trappable creatures:** Swimmers only
- **Lure:** FishTrapLure tag, radius 32 cells
- **Trail length:** 4
- **Makes fake floor** at (0, 0)
- **Logic input:** 1 port at (0, 0) - Green: set trap, Red: disarm and empty
- **Logic output:** 1 port at (0, 1) - Green: critter trapped, Red: empty
- **Tech:** Animal Control (AnimalControl)

---

## CreatureAirTrap

- **Name:** Airborne Critter Trap
- **Description:** It needs to be armed prior to use.
- **Effect:** Attracts and captures airborne Critters. Only Duplicants with the Critter Ranching I skill can arm this trap. It's reusable!
- **Size:** 1 x 2
- **Materials:** 50 kg Metal (raw)
- **HP:** 10
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** -15 (radius 3) [PENALTY.TIER2]
- **Placement:** On floor
- **Build time:** 10 s
- **Power:** None
- **Trappable creatures:** Flyers only
- **Lure:** FlyersLure tag, radius 32 cells
- **Logic input:** 1 port at (0, 0) - Green: set trap, Red: disarm and empty
- **Logic output:** 1 port at (0, 1) - Green: critter trapped, Red: empty
- **Tech:** Animal Control (AnimalControl)

---

## CritterCondo

- **Name:** Critter Condo
- **Description:** It's nice to have nice things.
- **Effect:** Provides a comfortable lounge area that boosts Critter happiness.
- **Size:** 3 x 3
- **Materials:** 200 kg Raw Minerals (BuildableRaw) + 10 kg Building Fiber
- **HP:** 100
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** +20 (radius 4) [BONUS.TIER3]
- **Build time:** 120 s
- **Power:** None
- **Can be flipped:** Yes (FlipH)
- **Room requirement:** Creature Pen (required)
- **Room tag:** RanchStationType
- **Effect on critter:** "InteractedWithCritterCondo" - +1 Happiness for 600 s
- **Operational when:** In correct room AND not flooded
- **Tech:** Creature Comforts (AnimalComfort)

---

## UnderwaterCritterCondo

- **Name:** Water Fort
- **Description:** Even wild critters are happier after they've had a little R&R.
- **Effect:** A fancy respite area for adult Pokeshells and Pacu.
- **Size:** 3 x 3
- **Materials:** 200 kg Plastic
- **HP:** 100
- **Overheat temp:** 1600 K (1326.85 C)
- **Floodable:** No
- **Decor:** +20 (radius 4) [BONUS.TIER3]
- **Build time:** 120 s
- **Power:** None
- **Can be flipped:** Yes (FlipH)
- **Effect on critter:** "InteractedWithUnderwaterCondo" - +1 Happiness for 600 s
- **Operational when:** All placement cells are submerged in liquid
- **No room requirement** (no RoomTracker)
- **Tech:** Creature Comforts (AnimalComfort)

---

## AirBorneCritterCondo

- **Name:** Airborne Critter Condo
- **Description:** Triggers natural nesting instincts and improves critters' moods.
- **Effect:** A hanging respite area for adult Pufts, Gassy Moos and Shine Bugs.
- **Size:** 3 x 3
- **Materials:** 200 kg Plastic
- **HP:** 100
- **Overheat temp:** 1600 K (1326.85 C)
- **Decor:** +20 (radius 4) [BONUS.TIER3]
- **Build time:** 120 s
- **Power:** None
- **Placement:** On ceiling
- **Can be flipped:** Yes (FlipH)
- **Room requirement:** Creature Pen (required)
- **Room tag:** RanchStationType
- **Effect on critter:** "InteractedWithAirborneCondo" - +1 Happiness for 600 s
- **Operational when:** In correct room
- **Tech:** Creature Comforts (AnimalComfort)
