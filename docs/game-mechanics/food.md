# Food System

Covers food quality, calorie values, spoilage mechanics, cooking recipes, duplicant consumption, and food storage. Derived from decompiled source (`TUNING.FOOD`, `Rottable`, `Edible`, `EdiblesManager.FoodInfo`, station configs).

## Duplicant Calorie Consumption

A duplicant burns **1,000 kcal per cycle** (600 seconds). That is ~1,666.7 cal/s (`DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_CYCLE = -1000000f`; internal units are calories, so 1,000,000 cal = 1,000 kcal).

Maximum stomach capacity: 4,000 kcal (`MAX_CALORIES = 4000000f`).

Eating speed: 2e-5 seconds per calorie (`EATING_SECONDS_PER_CALORIE`). A 1,000 kcal meal takes 20 seconds to eat.

Hunger threshold: satisfied above 95% stomach. Hungry at 50% of a cycle's burn below satisfied. Starving at one cycle's burn worth remaining.

## Food Quality Tiers and Morale

Food quality ranges from -1 to +6. The effective quality is `FoodInfo.Quality + FoodExpectation`, where `FoodExpectation` comes from traits:
- **Gourmet** trait: +1 (perceives food as one tier higher)
- **Shrivelled Tastebuds / Kitchen Menace**: -1 (perceives food as one tier lower)

The effective quality is clamped to [-1, 5] for effect lookup. Internal effect names are offset: quality -1 maps to `EdibleMinus3`, quality 0 to `EdibleMinus2`, etc.

| Quality | Label | Morale | Notes |
|---------|-----------|--------|-------|
| -1 | Grisly | -1 | Raw ingredients, emergency food |
| 0 | Terrible | 0 | Basic prepared foods, raw mushrooms |
| +1 | Poor | +1 | Simple cooked foods |
| +2 | Standard | +4 | Intermediate recipes |
| +3 | Good | +8 | Advanced cooking station recipes |
| +4 | Great | +12 | Gourmet recipes |
| +5 | Superb | +16 | Top-tier gourmet recipes |
| +6 | Ambrosial | +16 | Frost Burger only (clamped to +5 for effect) |

## Food Items by Quality

Internal calorie values are in calories (divide by 1000 for kcal). Spoil time is in seconds (one cycle = 600s, so 4800s = 8 cycles).

### Quality -1 (Grisly)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Meal Lice (BasicPlantFood) | 600 | 8 | Yes | Mealwood plant |
| Mush Bar | 800 | 8 | Yes | Microbe Musher (75 kg Dirt + 75 kg Water) |
| Nutrient Bar (FieldRation) | 800 | 32 | No | Starting supply |
| Muckroot (BasicForagePlant) | 800 | 8 | No | Foraging |
| Hexalent (ForestForagePlant) | 6,400 | 8 | No | Foraging (forest biome) |
| Raw Egg | 1,600 | 8 | Yes | Critter eggs |
| Meat | 1,600 | 8 | Yes | Critter butchery |
| Pickled Meal | 1,800 | 32 | Yes | Electric Grill (3x Meal Lice) |
| Swamp Chard Heart (Expansion1) | 2,400 | 8 | No | Foraging (swamp) |
| Hard Skin Berry (DLC2) | 800 | 16 | Yes | Pikeapple plant |
| Ice Caves Forage (DLC2) | 800 | 8 | No | Foraging (ice caves biome) |
| Garden Forage (DLC4) | 800 | 8 | No | Foraging (garden biome) |
| Garden Food Plant (DLC4) | 800 | 16 | Yes | Garden plant |
| Dinosaur Meat (DLC4) | 0 | 4 | Yes | Ingredient only |

### Quality 0 (Terrible)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Mushroom | 2,400 | 8 | Yes | Dusk Cap plant |
| Lettuce | 400 | 4 | Yes | Waterweed plant |
| Bristle Berry (PrickleFruit) | 1,600 | 8 | Yes | Bristle Blossom |
| Liceloaf (BasicPlantBar) | 1,700 | 8 | Yes | Microbe Musher (2x Meal Lice + 50 kg Water) |
| Mush Fry (FriedMushBar) | 1,050 | 8 | Yes | Electric Grill (1x Mush Bar) |
| Swamp Fruit (Expansion1) | 1,840 | 4 | Yes | Bog Bucket plant |
| Worm Basic Fruit (Expansion1) | 800 | 8 | Yes | Saturn Critter Trap |
| Carrot (DLC2) | 4,000 | 16 | Yes | Spindly Grubfruit plant |
| Vine Fruit (DLC4) | 325 | 8 | Yes | Vine plant |

### Quality 1 (Poor)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Gristle Berry (GrilledPrickleFruit) | 2,000 | 8 | Yes | Electric Grill (1x Bristle Berry) |
| Fried Mushroom | 2,800 | 8 | Yes | Electric Grill (1x Mushroom) |
| Gamma Mush | 1,050 | 4 | Yes | Microbe Musher (recipe removed in current game version) |
| Swamp Delights (Expansion1) | 2,240 | 8 | Yes | Electric Grill (1x Swamp Fruit) |
| Cooked Pikeapple (DLC2) | 1,200 | 8 | Yes | Electric Grill (1x Hard Skin Berry) |
| Plant Meat (Expansion1) | 1,200 | 4 | Yes | Saturn Critter Trap |
| Worm Super Fruit (Expansion1) | 250 | 4 | Yes | Saturn Critter Trap (fertilized) |
| Cooked Worm (WormBasicFood, Expansion1) | 1,200 | 8 | Yes | Electric Grill (1x Worm Basic Fruit) |
| Butterfly Food (DLC4) | 1,500 | 8 | Yes | Electric Grill (1x Butterfly Plant Seed) |

### Quality 2 (Standard)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Pacu Fillet (FishMeat) | 1,000 | 4 | Yes | Pacu |
| Shellfish (ShellfishMeat) | 1,000 | 4 | Yes | Pokeshell |
| Frost Bun (ColdWheatBread) | 1,200 | 8 | Yes | Electric Grill (3x Sleet Wheat Grain) |
| Omelette (CookedEgg) | 2,800 | 4 | Yes | Electric Grill (1x Raw Egg) |
| Tofu | 3,600 | 4 | Yes | Microbe Musher (6x Nosh Bean + 50 kg Water) |
| Pemmican (DLC2) | 2,600 | 32 | No | Microbe Musher (1x Meat + 1x Tallow) |
| Smoked Vegetables (DLC4) | 2,862.5 | 16 | Yes | Smoker (7x veg + 100 kg wood/peat) |

### Quality 3 (Good)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Cooked Seafood (CookedFish) | 1,600 | 4 | Yes | Electric Grill (1x Fish/Shellfish Meat) |
| Barbeque (CookedMeat) | 4,000 | 4 | Yes | Electric Grill (2x Meat) |
| Souffl Pancakes (Pancakes) | 3,600 | 8 | Yes | Electric Grill (1x Egg + 2x Grain) |
| Fruit Cake | 4,000 | 32 | No | Microbe Musher (5x Grain + 1x Berry) |
| Smoked Fish (DLC4) | 2,800 | 32 | Yes | Smoker (6x Fish + 100 kg wood/peat) |
| Smoked Dinosaur Meat (DLC4) | 5,000 | 8 | Yes | Smoker (6x Dino Meat + 100 kg wood/peat) |
| Deep Fried Meat (DLC2) | 4,000 | 4 | Yes | Deep Fryer (no recipe in current source) |
| Deep Fried Nosh (DLC2) | 5,000 | 8 | Yes | Deep Fryer (6x Nosh Bean + 1x Tallow) |
| Fries Carrot (DLC2) | 5,400 | 4 | Yes | Deep Fryer (1x Carrot + 1x Tallow) |
| Jawbo Fillet (DLC4) | 1,000 | 4 | Yes | Prehistoric Pacu |
| Candy (WormSuperFood, Expansion1) | 2,400 | 32 | Yes | Electric Grill (8x Grubfruit + 4x Sucrose) |

### Quality 4 (Great)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Stuffed Berry (Salsa) | 4,400 | 4 | Yes | Gas Range (2x Gristle Berry + 2x Pincha Pepper) |
| Surf'n'Turf | 6,000 | 4 | Yes | Gas Range (1x Barbeque + 1x Cooked Seafood) |
| Mushroom Wrap | 4,800 | 4 | Yes | Gas Range (1x Fried Mushroom + 4x Lettuce) |
| Curried Beans (Curry) | 5,000 | 16 | Yes | Gas Range (4x Ginger + 4x Nosh Bean) |
| Deep Fried Fish (DLC2) | 4,200 | 4 | Yes | Deep Fryer (1x Fish + 2.4x Tallow + 2x Grain) |
| Deep Fried Shellfish (DLC2) | 4,200 | 4 | Yes | Deep Fryer (1x Shellfish + 2.4x Tallow + 2x Grain) |

### Quality 5 (Superb)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Pepper Bread (SpiceBread) | 4,000 | 8 | Yes | Gas Range (10x Grain + 1x Pincha Pepper) |
| Spicy Tofu | 4,000 | 4 | Yes | Gas Range (1x Tofu + 1x Pincha Pepper) |
| Mushroom Quiche (Quiche) | 6,400 | 4 | Yes | Gas Range (1x Omelette + 1x Lettuce + 1x Fried Mushroom) |
| Berry Pie (Expansion1) | 4,200 | 4 | Yes | Gas Range (3x Grain + 4x Grubfruit + 1x Berry) |

### Quality 6 (Ambrosial)

| Food | kcal | Spoil (cycles) | Rots | Source |
|------|------|----------------|------|--------|
| Frost Burger | 6,000 | 4 | Yes | Gas Range (1x Frost Bun + 1x Lettuce + 1x Barbeque) |

Frost Burger also grants a `GoodEats` bonus effect.

## Spoilage and Rot Mechanics

Source: `Rottable` state machine.

### Rot Amount

Each food item has a rot amount that starts at `spoilTime` and counts down. When it reaches zero, the food turns into Rot Pile and is destroyed. Food transitions through three states:

- **Fresh**: rot amount > `staleTime` (which is always `spoilTime / 2`)
- **Stale**: rot amount between 0 and `staleTime`
- **Spoiled**: rot amount <= 0 (destroyed, becomes Rot Pile)

### Decay Rate Modifiers

The decay rate is the product of two independent multipliers: temperature and atmosphere. Each applies a modifier to the rot amount's delta attribute.

**Temperature modifiers:**

| Level | Rate Multiplier | Condition |
|-------|----------------|-----------|
| Unrefrigerated | -0.7 | temp >= `rotTemperature` (default 4 C / 277.15 K) and not in active fridge |
| Refrigerated | -0.2 | temp < `rotTemperature` OR in active powered Refrigerator |
| Frozen | 0.0 (paused) | temp < `preserveTemperature` (default -18 C / 255.15 K) |

Temperature check uses `min(grid_temperature, item_temperature)`, except in vacuum where only item temperature is used.

**Atmosphere modifiers:**

| Quality | Rate Multiplier | Elements |
|---------|----------------|----------|
| Contaminating | -1.0 | Contaminated Oxygen, Phosphorus Gas |
| Normal | -0.3 | Oxygen, Water (liquid cell) |
| Sterile | 0.0 (paused) | Carbon Dioxide, Steam, Hydrogen, Chlorine Gas, Helium, Propane, Copper Gas, Gold Gas, Rock Gas, Steel Gas, Vacuum |

The atmosphere check samples both the food's cell and the cell above it. If they differ, the worse quality wins (Contaminating > Normal > Sterilizing).

**Combined decay rate:**

The effective decay is `temperature_modifier + atmosphere_modifier` applied per second to the rot amount. For example, unrefrigerated (-0.7) in normal atmosphere (-0.3) = -1.0 per second, consuming the full spoilTime in `spoilTime` seconds.

With default spoil time of 4800s (8 cycles):
- Unrefrigerated + Normal atmo: ~8 cycles to spoil
- Refrigerated + Normal atmo: ~9.6 cycles (0.5 rate)
- Unrefrigerated + Sterile atmo: ~11.4 cycles (0.7 rate)
- Refrigerated + Sterile atmo: ~24 cycles (0.2 rate)
- Frozen OR in sterile atmo with frozen temp: indefinite (0.0 rate)

### Preservation

Items tagged `Preserved`, `Dehydrated`, or `Entombed` skip all rot modifiers entirely (enter `Preserved` state). The `PreservingSpice` from the Spice Grinder also adds a modifier reducing decay.

## Cooking Stations

### Microbe Musher
No skill required. 240W power, liquid input. Produces lower-quality prepared foods.

| Recipe | Inputs | Output | Quality | kcal |
|--------|--------|--------|---------|------|
| Mush Bar | 75 kg Dirt + 75 kg Water | 1x Mush Bar | -1 | 800 |
| Liceloaf | 2x Meal Lice + 50 kg Water | 1x Liceloaf | 0 | 1,700 |
| Tofu | 6x Nosh Bean + 50 kg Water | 1x Tofu | 2 | 3,600 |
| Fruit Cake | 5x Grain + 1x Berry | 1x Fruit Cake | 3 | 4,000 |
| Pemmican (DLC2) | 1x Meat + 1x Tallow | 1x Pemmican | 2 | 2,600 |

### Electric Grill (CookingStation)
Requires Grilling skill. 60W power. Heats output to 368.15 K (95 C).

| Recipe | Inputs | Output | Quality | kcal |
|--------|--------|--------|---------|------|
| Pickled Meal | 3x Meal Lice | 1x Pickled Meal | -1 | 1,800 |
| Mush Fry | 1x Mush Bar | 1x Mush Fry | 0 | 1,050 |
| Fried Mushroom | 1x Mushroom | 1x Fried Mushroom | 1 | 2,800 |
| Gristle Berry | 1x Bristle Berry | 1x Gristle Berry | 1 | 2,000 |
| Frost Bun | 3x Sleet Wheat Grain | 1x Frost Bun | 2 | 1,200 |
| Omelette | 1x Raw Egg | 1x Omelette | 2 | 2,800 |
| Barbeque | 2x Meat | 1x Barbeque | 3 | 4,000 |
| Cooked Seafood | 1x Fish/Shellfish Meat | 1x Cooked Seafood | 3 | 1,600 |
| Souffl Pancakes | 1x Raw Egg + 2x Grain | 1x Pancakes | 3 | 3,600 |
| Swamp Delights (Exp1) | 1x Swamp Fruit | 1x Swamp Delights | 1 | 2,240 |
| Cooked Worm (Exp1) | 1x Worm Basic Fruit | 1x Cooked Worm | 1 | 1,200 |
| Candy (Exp1) | 8x Grubfruit + 4x Sucrose | 1x Candy | 3 | 2,400 |
| Cooked Pikeapple (DLC2) | 1x Hard Skin Berry | 1x Cooked Pikeapple | 1 | 1,200 |
| Butterfly Food (DLC4) | 1x Butterfly Seed | 1x Butterfly Food | 1 | 1,500 |

### Gas Range (GourmetCookingStation)
Requires Haute Cuisine skill. 240W power + Natural Gas pipe input (0.1 kg/s consumed, emits 0.025 kg/s CO2). Heats output to 368.15 K.

| Recipe | Inputs | Output | Quality | kcal |
|--------|--------|--------|---------|------|
| Stuffed Berry | 2x Gristle Berry + 2x Pincha Pepper | 1x Stuffed Berry | 4 | 4,400 |
| Mushroom Wrap | 1x Fried Mushroom + 4x Lettuce | 1x Mushroom Wrap | 4 | 4,800 |
| Surf'n'Turf | 1x Barbeque + 1x Cooked Seafood | 1x Surf'n'Turf | 4 | 6,000 |
| Pepper Bread | 10x Grain + 1x Pincha Pepper | 1x Pepper Bread | 5 | 4,000 |
| Spicy Tofu | 1x Tofu + 1x Pincha Pepper | 1x Spicy Tofu | 5 | 4,000 |
| Curried Beans | 4x Ginger + 4x Nosh Bean | 1x Curried Beans | 4 | 5,000 |
| Mushroom Quiche | 1x Omelette + 1x Lettuce + 1x Fried Mushroom | 1x Quiche | 5 | 6,400 |
| Frost Burger | 1x Frost Bun + 1x Lettuce + 1x Barbeque | 1x Frost Burger | 6 | 6,000 |
| Berry Pie (Exp1) | 3x Grain + 4x Grubfruit + 1x Berry | 1x Berry Pie | 5 | 4,200 |

### Deep Fryer (DLC2)
Requires Deep Frying skill. 480W power. Must be in a Kitchen room.

| Recipe | Inputs | Output | Quality | kcal |
|--------|--------|--------|---------|------|
| Fries Carrot | 1x Carrot + 1x Tallow | 1x Fries Carrot | 3 | 5,400 |
| Deep Fried Nosh | 6x Nosh Bean + 1x Tallow | 1x Deep Fried Nosh | 3 | 5,000 |
| Deep Fried Fish | 1x Pacu Fillet + 2.4x Tallow + 2x Grain | 1x Deep Fried Fish | 4 | 4,200 |
| Deep Fried Shellfish | 1x Shellfish + 2.4x Tallow + 2x Grain | 1x Deep Fried Shellfish | 4 | 4,200 |

### Smoker (DLC4)
Requires Haute Cuisine skill. No power. Burns wood/peat as fuel (0.2 kg/s), emits CO2 (0.02 kg/s). Does not require a duplicant operator.

| Recipe | Inputs | Output | Quality | kcal | Cook Time |
|--------|--------|--------|---------|------|-----------|
| Smoked Dinosaur Meat | 6x Dino Meat + 100 kg wood/peat | 3.2x Smoked Dino Meat | 3 | 5,000 | 600s |
| Smoked Fish | 6x Fish/Jawbo Fillet + 100 kg wood/peat | 4x Smoked Fish | 3 | 2,800 | 600s |
| Smoked Vegetables | 7x veg + 100 kg wood/peat | 4x Smoked Vegetables | 2 | 2,862.5 | 600s |

## Food Storage

### Ration Box
No power. 150 kg capacity. Does not provide refrigeration or atmosphere sealing. Food rots at ambient rate. Available from game start.

### Refrigerator
120W power (20W energy saver mode). 100 kg capacity. Provides the **Refrigerated** temperature modifier to stored food when powered and operational, regardless of ambient temperature. This is checked via `Rottable.IsInActiveFridge()`. The fridge also provides 0.375 kW of active cooling to its contents.

### CO2 Pit Strategy
Carbon dioxide is denser than oxygen and pools at the bottom of rooms. Since CO2 is a sterile atmosphere element, food stored in CO2-filled areas gets the sterile atmosphere modifier (0.0 decay from atmosphere). Combined with refrigeration, this nearly halts spoilage. No special building is needed; just place storage in a low-lying area where CO2 naturally accumulates.

### Sterile Atmosphere Elements
The following elements provide the sterile (0.0 atmosphere decay) modifier: Carbon Dioxide, Steam, Hydrogen, Chlorine Gas, Helium, Propane, Vacuum, and several high-temperature gases (Copper Gas, Gold Gas, Rock Gas, Steel Gas).

### Contaminating Atmosphere Elements
Contaminated Oxygen and Phosphorus Gas double the decay rate compared to normal atmosphere.

## Special Food Effects

Some foods grant additional effects beyond morale:
- **SeafoodRadiationResistance**: +10 Radiation Resistance (Expansion1 only). Applies to: Fish Meat, Shellfish Meat, Lettuce, Cooked Seafood, Surf'n'Turf, Mushroom Wrap, Quiche, Frost Burger, Smoked Fish, Deep Fried Fish, Deep Fried Shellfish
- **Curried Beans / Spicy Tofu**: `WarmTouchFood` effect (body temperature modifier)
- **Curried Beans**: Also grants `HotStuff` effect
- **Frost Burger**: Grants `GoodEats` effect
- **Rehydrated food**: Grants a rehydration effect from `FoodRehydratorConfig`

## Key Constants (TUNING.FOOD)

| Constant | Value | Meaning |
|----------|-------|---------|
| DEFAULT_PRESERVE_TEMPERATURE | 255.15 K (-18 C) | Below this: frozen, rot paused |
| DEFAULT_ROT_TEMPERATURE | 277.15 K (4 C) | Below this: refrigerated |
| HIGH_PRESERVE_TEMPERATURE | 283.15 K (10 C) | Used by Sleet Wheat Grain |
| HIGH_ROT_TEMPERATURE | 308.15 K (35 C) | Used by Sleet Wheat Grain |
| SPOIL_TIME.DEFAULT | 4800s (8 cycles) | Standard spoil time |
| SPOIL_TIME.QUICK | 2400s (4 cycles) | Fast-spoiling foods |
| SPOIL_TIME.SLOW | 9600s (16 cycles) | Slow-spoiling foods |
| SPOIL_TIME.VERYSLOW | 19200s (32 cycles) | Long-lasting foods |
| SMALL_COOK_TIME | 30s | Quick recipe duration |
| STANDARD_COOK_TIME | 50s | Normal recipe duration |
