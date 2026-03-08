# Creature Species

Per-species stats for all major creature families -- diets, temperatures, egg chances, production rates. For how creature AI, reproduction, taming, and lifecycle work in general, see `creature-ai.md`. Derived from decompiled source code (`*Config.cs` and `*Tuning.cs`).

All temperatures in Kelvin from source. Conversion: K - 273.15 = C. For reference: 273.15K = 0C, 293.15K = 20C, 313.15K = 40C, 373.15K = 100C.

**Conversion efficiency constants (from `TUNING.CREATURES.CONVERSION_EFFICIENCY`):**

| Name | Ratio |
|------|-------|
| BAD_2 | 0.10 (10%) |
| BAD_1 | 0.25 (25%) |
| NORMAL | 0.50 (50%) |
| GOOD_1 | 0.75 (75%) |
| GOOD_2 | 0.95 (95%) |
| GOOD_3 | 1.00 (100%) |

**Space requirements (from `TUNING.CREATURES.SPACE_REQUIREMENTS`):**

| Tier | Tiles per creature |
|------|--------------------|
| TIER1 | 4 |
| TIER2 | 8 |
| TIER3 | 12 |
| TIER4 | 16 |

---

## Hatch Family

Floor walkers. Eat minerals and excrete refined materials. The primary early-game ranching creature.

**Shared traits:** 100 kg mass, 100-cycle lifespan, 12-tile space requirement, 2 kg egg mass. Drownable, not entombable. Can burrow. Calorie burn: 700,000 kcal/cycle, stomach: 7,000,000 kcal (10 cycles of starvation reserve). Temperature comfort: 10-40C (283.15-313.15K), lethal: -45 to 100C (228.15-373.15K).

### Hatch (base)

| Property | Value |
|----------|-------|
| Eats | Basic rocks (Sand, Sandstone, Clay, Crushed Rock, Dirt, Sedimentary Rock, Shale) at 140 kg/cycle; also any food item |
| Produces | Coal (Carbon) |
| Rock conversion | NORMAL (50%) |
| Food conversion | GOOD_1 (75%) |
| HP | 25 |
| Egg | HatchEgg (base chance 98%, with 2% Stone Hatch, 2% Sage Hatch) |

### Stone Hatch (HatchHard)

| Property | Value |
|----------|-------|
| Eats | Hard rocks (Sedimentary Rock, Igneous Rock, Obsidian, Granite) at 140 kg/cycle; also metal ores |
| Produces | Coal from rocks; Coal from metals (BAD_1, 25%) |
| Rock conversion | NORMAL (50%) |
| HP | 200 |
| Egg | HatchHardEgg (base 65%, 32% Hatch, 2% Smooth Hatch) |

### Smooth Hatch (HatchMetal)

| Property | Value |
|----------|-------|
| Eats | Metal ores only at 100 kg/cycle |
| Produces | Refined metals (ore-specific: Copper Ore -> Copper, Gold Amalgam -> Gold, Wolframite -> Tungsten, etc.) |
| Conversion | GOOD_1 (75%) |
| HP | 400 |
| Egg | HatchMetalEgg (base 67%, 22% Stone Hatch, 11% Hatch) |

### Sage Hatch (HatchVeggie)

| Property | Value |
|----------|-------|
| Eats | Organics (Dirt, Slime, Algae, Fertilizer, Polluted Dirt) at 140 kg/cycle; also any food item |
| Produces | Coal |
| Conversion | GOOD_3 (100%) for both organics and food |
| HP | 25 |
| Min poop | 50 kg |
| Egg | HatchVeggieEgg (base 67%, 33% Hatch) |

---

## Drecko Family

Wall/ceiling walkers. Eat plants directly (consuming growth days, not mass). Produce fiber or plastic from scales when in Hydrogen atmosphere.

**Shared traits:** 200 kg mass, 150-cycle lifespan, 12-tile space requirement, 2 kg egg mass. Drownable, not entombable. Calorie burn: 2,000,000 kcal/cycle, stomach: 10,000,000 kcal (5 cycles reserve).

### Drecko (base)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 10-60C (283.15-333.15K), lethal: -30 to 100C (243.15-373.15K) |
| Eats | Balm Lily, Pincha Pepperplant, Mealwood (0.75 growth-days/cycle) |
| Produces | Phosphorite (13.33 kg per growth-day consumed) |
| Scale product | Reed Fiber (BasicFabric) |
| Scale growth | 8 cycles to full, drops 2 kg fiber (0.25 kg/cycle) |
| Scale atmosphere | Hydrogen |
| Scale levels | 6 |
| HP | 25 |
| Egg | DreckoEgg (base 98%, 2% Glossy Drecko) |
| Incubation | 90 seconds (fertility), 30 seconds (egg) |

### Glossy Drecko (DreckoPlastic)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 20-50C (293.15-323.15K), lethal: -30 to 100C |
| Eats | Mealwood, Bristle Blossom (1.0 growth-day/cycle) |
| Produces | Phosphorite (9 kg per growth-day consumed) |
| Scale product | Plastic (Polypropylene) |
| Scale growth | 3 cycles to full, drops 150 kg plastic (50 kg/cycle) |
| Scale atmosphere | Hydrogen |
| Egg | DreckoPlasticEgg (base 65%, 35% Drecko) |

---

## Puft Family

Flyers. Inhale gases, excrete solids. Faction: Prey (will not attack).

**Shared traits:** 50 kg mass, 75-cycle lifespan, 16-tile space requirement, 0.5 kg egg mass. Drownable and entombable. Calorie burn: 200,000 kcal/cycle, stomach: 1,200,000 kcal (6 cycles reserve).

### Puft (base)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 15-55C (288.15-328.15K), lethal: -50 to 100C |
| Eats | Polluted Oxygen at 50 kg/cycle |
| Produces | Slime (SlimeMold), carries Slimelung |
| Conversion | GOOD_2 (95%) |
| HP | 25 |
| Egg | PuftEgg (base 98%, 2% each: Prince, Oxylite, Bleachstone) |
| Incubation | 45s fertility, 15s egg |

### Puft Prince (PuftAlpha)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 20-40C (293.15-313.15K) |
| Eats | Polluted Oxygen -> Slime, Chlorine -> Bleach Stone, Oxygen -> Oxylite (30 kg/cycle total) |
| Conversion | BAD_2 (10%) for all |
| Scale | 1.1x visual scale |
| Egg | PuftAlphaEgg (base 2%, 98% Puft) |

### Dense Puft (PuftOxylite)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 0-60C (273.15-333.15K) |
| Eats | Oxygen at 50 kg/cycle |
| Produces | Oxylite (OxyRock) |
| Conversion | GOOD_2 (95%) |
| Lured by | Oxylite |
| Egg | PuftOxyliteEgg (base 67%, 31% Puft, 2% Prince) |

### Squeaky Puft (PuftBleachstone)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 0-60C (273.15-333.15K) |
| Eats | Chlorine at 30 kg/cycle |
| Produces | Bleach Stone |
| Conversion | GOOD_2 (95%) |
| Lured by | Bleach Stone |
| Egg | PuftBleachstoneEgg (base 67%, 31% Puft, 2% Prince) |

---

## Shine Bug Family (LightBug)

Flyers that emit light (1800 lux, 5-tile radius) and radiation (60 rads, 6-tile radius in DLC1). Evolve through a color chain.

**Shared traits:** 5 kg mass, 25-cycle lifespan, 6-tile space requirement, 1 kg egg mass. Drownable and entombable. Calorie burn: 40,000 kcal/cycle, stomach: 320,000 kcal (8 cycles reserve). Temperature comfort: 10-40C. Babies produce 5 kg Natural Resin when hatching. Lured by Phosphorite.

### Shine Bug (base)

| Property | Value |
|----------|-------|
| Eats | Bristle Berry, Gristle Berry, Phosphorite (0.166 kg/cycle) |
| HP | 5 |
| Egg | LightBugEgg (base 98%, 2% Sun) |
| Incubation | 15s fertility, 5s egg |

### Sun Bug (LightBugOrange)

| Property | Value |
|----------|-------|
| Eats | Mushroom, Fried Mushroom, Gristle Berry, Phosphorite (0.25 kg/cycle) |
| Decor | TIER6 |
| Egg | LightBugOrangeEgg (66% Sun, 33% Shine Bug, 2% Royal) |
| Light | Orange, 1800 lux |

### Royal Bug (LightBugPurple)

| Property | Value |
|----------|-------|
| Eats | Fried Mushroom, Gristle Berry, Pincha Peppernut, Pepper Bread, Phosphorite (1 kg/cycle) |
| Decor | TIER6 |
| Egg | LightBugPurpleEgg (66% Royal, 33% Sun, 2% Coral) |
| Light | Purple, 1800 lux |

### Coral Bug (LightBugPink)

| Property | Value |
|----------|-------|
| Eats | Fried Mushroom, Pepper Bread, Bristle Berry, Gristle Berry, Stuffed Berry, Phosphorite (1 kg/cycle) |
| Decor | TIER6 |
| Egg | LightBugPinkEgg (66% Coral, 33% Royal, 2% Azure) |
| Light | Pink, 1800 lux |

### Azure Bug (LightBugBlue)

| Property | Value |
|----------|-------|
| Eats | Pepper Bread, Stuffed Berry, Phosphorite, Phosphorus (1 kg/cycle) |
| Decor | TIER6 |
| Egg | LightBugBlueEgg (66% Azure, 33% Coral, 2% Abyss) |
| Light | Blue, 1800 lux |
| Lured by | Phosphorite, Phosphorus |

### Abyss Bug (LightBugBlack)

| Property | Value |
|----------|-------|
| Lifespan | 75 cycles |
| Eats | Stuffed Berry, Meat, Barbecue, Abyssalite, Phosphorus (1 kg/cycle) |
| Decor | TIER7 |
| Egg | LightBugBlackEgg (66% Abyss, 33% Azure, 2% Radiant) |
| Light | None (no light emission) |
| Lured by | Phosphorus |
| Incubation | 45s fertility, 15s egg |

### Radiant Bug (LightBugCrystal)

| Property | Value |
|----------|-------|
| Lifespan | 75 cycles |
| Eats | Barbecue, Diamond (1 kg/cycle) |
| Decor | TIER8 |
| Egg | LightBugCrystalEgg (98% Radiant, 2% Shine Bug) |
| Light | Crystal, 1800 lux |
| Lured by | Diamond |
| Incubation | 45s fertility, 15s egg |

**Evolution chain:** Shine Bug -> Sun -> Royal -> Coral -> Azure -> Abyss -> Radiant (Crystal). Each variant primarily produces its own eggs (66%), with a 33% chance of the previous variant and a 2% chance of the next. Radiant wraps back to base Shine Bug at 2%. Dietary modifiers increase next-tier egg chances: Gristle Berry -> Sun, Fried Mushroom -> Royal, Pepper Bread -> Coral, Stuffed Berry -> Azure, Phosphorus -> Abyss, Barbecue -> Radiant.

---

## Pokeshell Family (Crab)

Floor walkers, 2 tiles tall (adults). Amphibious (do not drown). Faction: Pest. Defend eggs and allies. Drop shells on death.

**Shared traits:** 100 kg mass, 100-cycle lifespan, 12-tile space requirement, 2 kg egg mass. Not drownable, not entombable. Calorie burn: 100,000 kcal/cycle, stomach: 1,000,000 kcal (10 cycles reserve). Temperature comfort: 0-40C (273.15-313.15K), lethal: -50 to 100C. Weapon damage: 2-3.

### Pokeshell (base)

| Property | Value |
|----------|-------|
| Eats | Polluted Dirt, Rot Piles at 70 kg/cycle |
| Produces | Sand |
| Conversion | NORMAL (50%) |
| Death drop | 10 Pokeshell Molts (CrabShell) |
| Egg | CrabEgg (base 97%, 2% Oakshell, 1% Sanishell) |

### Oakshell (CrabWood)

| Property | Value |
|----------|-------|
| Eats | Polluted Dirt, Rot Piles, Slime at 70 kg/cycle |
| Produces | Sand |
| Conversion | BAD_1 (25%) |
| Death drop | 500 kg Lumber Shell (CrabWoodShell) |
| Special | Drops 100 kg Lumber Shell periodically when happy and fed (once per cycle) |
| Egg | CrabWoodEgg (base 65%, 32% Pokeshell, 2% Sanishell) |

### Sanishell (CrabFreshWater)

| Property | Value |
|----------|-------|
| Eats | Polluted Dirt, Rot Piles, Slime at 70 kg/cycle |
| Produces | Sand |
| Conversion | NORMAL (50%) |
| Death drop | 4 Shellfish Meat |
| Special | Cleans germs from nearby liquids (2-tile radius, 30s cooldown). Emits negative disease counts to eliminate Food Poisoning, Slimelung, Zombie Spores, etc. |
| Egg | CrabFreshWaterEgg (base 65%, 32% Pokeshell, 2% Oakshell) |

---

## Pip Family (Squirrel)

Floor and tree climbers. Eat plants directly. Plant seeds in natural tiles.

**Shared traits:** 100 kg mass, 100-cycle lifespan, 12-tile space requirement, 2 kg egg mass. Drownable, not entombable. Calorie burn: 100,000 kcal/cycle, stomach: 1,000,000 kcal (10 cycles reserve). Temperature comfort: 10-40C, lethal: -45 to 100C.

### Pip (base)

| Property | Value |
|----------|-------|
| Eats | Arbor Trees, Thimble Reed, Space Trees (0.4 growth-days/cycle) |
| Produces | Dirt (50 kg per growth-day consumed) |
| HP | 25 |
| Special | Plants seeds in natural tiles, climbs trees |
| Egg | SquirrelEgg (base 98%, 2% Cuddle Pip) |

### Cuddle Pip (SquirrelHug)

| Property | Value |
|----------|-------|
| Space | 4 tiles (TIER1) |
| Eats | Same plants as Pip (0.5 growth-days/cycle) |
| Produces | Dirt (25 kg per growth-day consumed) |
| Special | Hugs eggs (tending) and duplicants. Provides decor bonus (TIER3). |
| Egg | SquirrelHugEgg (base 65%, 35% Pip) |

---

## Shove Vole Family (Mole)

Digger creatures. Burrow through solid tiles, eat minerals, and deposit them as solid tiles. Extremely wide temperature tolerance.

**Shared traits:** 25 kg mass, 100-cycle lifespan, no space requirement (0 tiles), 2 kg egg mass. Drownable, not entombable. Calorie burn: 4,800,000 kcal/cycle, stomach: 48,000,000 kcal (10 cycles reserve). Lethal: -200 to 500C. Poop Regolith.

### Shove Vole (base)

| Property | Value |
|----------|-------|
| Temperature | Comfort: -100 to 400C (173.15-673.15K) |
| Eats | Regolith, Dirt, Iron Ore (1,000 kcal/kg) |
| Produces | Eaten material as solid tiles (NORMAL conversion, 50%) |
| HP | 25 |
| Death drop | 10 Meat |
| Egg | MoleEgg (base 98%, 2% Delicacy) |

### Shove Vole Delicacy (MoleDelicacy)

| Property | Value |
|----------|-------|
| Temperature | Comfort: -100 to 100C (173.15-373.15K) |
| Eats | Regolith, Dirt, Iron Ore (1,000 kcal/kg) |
| Produces | Eaten material as solid tiles (NORMAL conversion, 50%) |
| Stomach | 24,000,000 kcal (half of base Shove Vole) |
| HP | 25 |
| Death drop | 5 Meat |
| Scale product | Ginger (GingerConfig) |
| Scale growth | 8 cycles to full, drops 8 kg ginger (1 kg/cycle) |
| Scale temperature | 70-80C (343.15-353.15K) required for growth |
| Scale levels | 5 |
| Egg | MoleDelicacyEgg (base 65%, 32% Shove Vole) |

---

## Oil Floater Family (Oilfloater)

Hovering creatures. Inhale gases, excrete liquids. Live in hot environments.

**Shared traits:** 50 kg mass, 12-tile space requirement, 2 kg egg mass. Drownable, not entombable. Calorie burn: 120,000 kcal/cycle, stomach: 600,000 kcal (5 cycles reserve). Can swim.

### Slickster (base)

| Property | Value |
|----------|-------|
| Lifespan | 100 cycles |
| Temperature | Comfort: 50-140C (323.15-413.15K), lethal: 0-200C |
| Eats | Carbon Dioxide at 20 kg/cycle |
| Produces | Crude Oil |
| Conversion | NORMAL (50%) |
| HP | 25 |
| Egg | OilfloaterEgg (base 98%, 2% Molten, 2% Longhair) |

### Molten Slickster (OilfloaterHighTemp)

| Property | Value |
|----------|-------|
| Lifespan | 100 cycles |
| Temperature | Comfort: 100-200C (373.15-473.15K), lethal: 50-300C |
| Eats | Carbon Dioxide at 20 kg/cycle |
| Produces | Petroleum |
| Conversion | NORMAL (50%) |
| Egg | OilfloaterHighTempEgg (base 66%, 33% Slickster, 2% Longhair) |

### Longhair Slickster (OilfloaterDecor)

| Property | Value |
|----------|-------|
| Lifespan | 150 cycles |
| Temperature | Comfort: 0-50C (273.15-323.15K), lethal: -50 to 100C |
| Eats | Oxygen at 30 kg/cycle |
| Produces | Nothing (no output) |
| Special | High decor (TIER6) |
| Egg | OilfloaterDecorEgg (base 66%, 33% Slickster, 2% Molten) |
| Incubation | 90s fertility, 30s egg |

---

## Pacu Family

Swimmers. Eat algae, seeds, and kelp. Drop Polluted Dirt. Live in water.

**Shared traits:** 200 kg mass, 25-cycle lifespan, 8-tile space requirement, 4 kg egg mass. Not drownable, not entombable. Calorie burn: 100,000 kcal/cycle, stomach: 500,000 kcal (5 cycles reserve). Not ranchable (no grooming). Fish overcrowding monitor instead of standard overcrowding.

### Pacu (base)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 0-60C (273.15-333.15K), lethal: -20 to 100C |
| Eats | Algae (7.5 kg/cycle), any seed (1 seed = 1 cycle of calories), Kelp |
| Produces | Polluted Dirt |
| Conversion | NORMAL (50%) |
| Death drop | 1 Fish Meat |
| Egg | PacuEgg (base 98%, 2% Tropical, 2% Gulp Fish) |
| Incubation | 15s fertility, 5s egg |

### Tropical Pacu (PacuTropical)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 30-80C (303.15-353.15K), lethal: 10-100C |
| Diet | Same as base Pacu |
| Special | High decor (TIER4) |
| Egg | PacuTropicalEgg (base 65%, 32% Pacu, 2% Gulp Fish) |

### Gulp Fish (PacuCleaner)

| Property | Value |
|----------|-------|
| Temperature | Comfort: -30 to 5C (243.15-278.15K), lethal: -50 to 25C |
| Diet | Same as base Pacu |
| Special | Passively consumes Polluted Water (0.2 kg/s) and emits Clean Water bubbles |
| Egg | PacuCleanerEgg (base 65%, 32% Pacu, 2% Tropical) |

---

## Gassy Moo Family (Moo)

Hovering creatures found in space (2x2 grid, FlyerNavGrid2x2). Eat Gas Grass and Plant Fiber. Can beckon comets. Faction: Prey.

**Shared traits:** 50 kg mass, 75-cycle lifespan, 16-tile space requirement. Drownable and entombable. Calorie burn: 200,000 kcal/cycle, stomach: 1,200,000 kcal (6 cycles reserve). Temperature comfort: -50 to 50C (223.15-323.15K), lethal: -200 to 200C. HP: 50. Death drop: 10 Meat. Lured by Bleach Stone.

Both variants eat Gas Grass (2 growth-days/cycle) and Plant Fiber (200 kg/cycle). Milk production fills over 4 cycles. No egg-based reproduction. Beckons comets approximately 4 times per lifetime.

### Gassy Moo (base)

| Property | Value |
|----------|-------|
| Diet output | Methane (Natural Gas) |
| Milk element | Natural Gas |
| Milk rate | 50 kg/cycle, 200 kg capacity |
| Comet type | Gassy Moo comet (98% base chance) |

### Diesel Moo (DieselMoo)

| Property | Value |
|----------|-------|
| Diet output | Carbon Dioxide |
| Milk element | Tallow (Refined Lipid) |
| Milk capacity | 800 kg |
| Comet type | Diesel Moo comet (60% base chance, 30% Gassy Moo) |

---

## Morb (Glom)

Pest creatures spawned from Outhouse/Lavatory accidents. Cannot be captured for ranching.

| Property | Value |
|----------|-------|
| Mass | 25 kg |
| Temperature | Comfort: 30-100C (303.15-373.15K), lethal: 0-200C |
| Diet | None (does not eat) |
| Special | Periodically emits Polluted Oxygen (25% chance per check, 0.2 kg per emission). Carries Slimelung (1000 germs/kg emitted). Releases 3 kg Polluted Oxygen on death. |
| HP | 25 |
| Reproduction | None. Spawns from unsanitary buildings. |
| Faction | Pest |

---

## Beeta (Bee) -- Spaced Out DLC

Radioactive flyers. Hostile faction. Consume CO2, forage Uranium Ore, build hives.

| Property | Value |
|----------|-------|
| Mass | 5 kg |
| Lifespan | 5 cycles |
| Temperature | Comfort: -50 to 0C (223.15-273.15K), lethal: -100 to 10C |
| Diet | None (no calorie-based diet). Consumes CO2 passively (0.1 kg/s, 3-tile radius). |
| Radiation | Emits 240 rads (adults, 3-tile radius), 120 rads (larvae, 2-tile radius). |
| Weapon | 2-3 damage |
| Special | Forages Uranium Ore and delivers to hive. Builds Beeta Hives. |
| Reproduction | Spawned by Beeta Hives (not egg-laying). |
| Faction | Hostile |

---

## Sweetle/Grubgrub Family (Divergent) -- Spaced Out DLC

Plant-tending creatures. Eat Sulfur, tend crops to boost growth.

**Shared traits:** Floor walkers, drownable, not entombable. Temperature comfort: 10-40C, lethal: -30 to 100C. Calorie burn: 700,000 kcal/cycle, stomach: 7,000,000 kcal (10 cycles reserve), 2 kg egg mass.

### Sweetle (DivergentBeetle)

| Property | Value |
|----------|-------|
| Mass | 50 kg |
| Lifespan | 75 cycles |
| Space | 12 tiles (TIER3) |
| Eats | Sulfur at 20 kg/cycle |
| Produces | Sucrose |
| Conversion | NORMAL (50%) |
| HP | 25 |
| Special | Tends crops (8 plants/cycle). Tending applies +5% growth speed for 1 cycle. |
| Egg | DivergentBeetleEgg (base 98%, 2% Grubgrub) |

### Grubgrub (DivergentWorm)

| Property | Value |
|----------|-------|
| Mass | 200 kg |
| Lifespan | 150 cycles |
| Space | 16 tiles (TIER4) |
| Eats | Sulfur at 50 kg/cycle (BAD_2 conversion), also Sucrose at 30 kg/cycle |
| Produces | Mud |
| HP | 25 |
| Weapon | 2-3 damage |
| Special | Tends crops (8 plants/cycle). Tending applies +50% growth speed for 1 cycle (much stronger than Sweetle). Segmented body (5 segments). |
| Egg | DivergentWormEgg (base 67%, 33% Sweetle) |

---

## Plug Slug Family (Staterpillar) -- Spaced Out DLC

Wall/ceiling walkers. Eat metals, produce Hydrogen. Generate power or pump fluids when sleeping on conduits/wires.

**Shared traits:** 200 kg mass, 100-cycle lifespan, 12-tile space requirement, 2 kg egg mass. Not drownable, not entombable. Calorie burn: 2,000,000 kcal/cycle, stomach: 10,000,000 kcal (5 cycles reserve). Amphibious.

### Plug Slug (base)

| Property | Value |
|----------|-------|
| Temperature | Comfort: 10-40C (283.15-313.15K), lethal: -100 to 100C |
| Eats | Raw metal ores and refined metals at 60 kg/cycle |
| Produces | Hydrogen (5% conversion rate) |
| Special | Sleeps on electrical wires, generating power via `StaterpillarGenerator`. |
| Egg | StaterpillarEgg (base 98%, 2% Smother Slug, 2% Smog Slug) |

### Smother Slug (StaterpillarGas)

| Property | Value |
|----------|-------|
| Temperature | Comfort: -10 to 40C (263.15-313.15K), lethal: -100 to 100C |
| Eats | Raw/refined metals at 30 kg/cycle |
| Produces | Hydrogen (5% conversion) |
| Special | Inhales unbreathable gases (stores up to 100 kg). Sleeps on gas conduits, pumping stored gas. |
| Egg | StaterpillarGasEgg (base 66%, 32% Plug Slug, 2% Smog Slug) |

### Smog Slug (StaterpillarLiquid)

| Property | Value |
|----------|-------|
| Temperature | Comfort: -10 to 40C (263.15-313.15K), lethal: -100 to 100C |
| Eats | Raw/refined metals at 30 kg/cycle |
| Produces | Hydrogen (5% conversion) |
| Special | Drinks liquids (stores up to 1000 kg). Sleeps on liquid conduits, pumping stored liquid. |
| Egg | StaterpillarLiquidEgg (base 66%, 32% Plug Slug, 2% Smother Slug) |

---

## Wood Deer Family (Deer) -- DLC2 (Frosty Planet Pack)

Floor walkers, 2 tiles tall (adults). Eat plants directly and produce Dirt. Faction: Pest. Grow antlers that can be sheared.

**Shared traits:** 100 kg mass, 100-cycle lifespan, 12-tile space requirement (TIER3), 2 kg egg mass. Drownable, not entombable. Calorie burn: 100,000 kcal/cycle, stomach: 1,000,000 kcal (10 cycles reserve). Temperature comfort: -30 to 10C (243.15-283.15K), lethal: -60 to 100C (213.15-373.15K). HP: 25. Death drop: 1 Meat.

### Wood Deer (base)

| Property | Value |
|----------|-------|
| Eats | Hard Skin Berry Plant, Bristle Blossom (0.2 growth-days/cycle); also Hard Skin Berry, Bristle Berry as food items |
| Produces | Dirt |
| Antler product | Wood Logs |
| Antler growth | 6 cycles to full, drops 360 kg wood (60 kg/cycle), starts at 50% growth |
| Antler levels | 6 |
| Egg | WoodDeerEgg (base 98%, 2% Glass Deer) |

### Glass Deer (GlassDeer)

| Property | Value |
|----------|-------|
| Eats | Hard Skin Berry Plant, Bristle Blossom (0.2 growth-days/cycle); Hard Skin Berry, Bristle Berry as food items; also Abyssalite (20 kg/cycle) |
| Produces | Dirt |
| Antler product | Glass (requires eating Abyssalite to grow) |
| Antler growth | 6 cycles to full, drops 60 kg glass (10 kg/cycle), starts at 50% growth |
| Antler levels | 6 |
| Egg | GlassDeerEgg (base 65%, 35% Wood Deer) |

---

## Stegosaurus Family (Stego) -- DLC4 (Biome Bundle)

Large floor walkers (2x2 grid). Eat fruit, produce Peat. Stomp ability (60s cooldown, 10-tile radius). Faction: Pest.

**Shared traits:** 400 kg mass, 200-cycle lifespan, 16-tile space requirement (TIER4), 8 kg egg mass. Drownable, not entombable. Temperature comfort: 20-70C (293.15-343.15K), lethal: -100 to 100C (173.15-373.15K). HP: 50. Death drop: 12 Dinosaur Meat. Tagged as LargeCreature.

### Stegosaurus (base)

| Property | Value |
|----------|-------|
| Eats | Vine Fruit, Bristle Berry (and Swamp Fruit if Spaced Out DLC active) |
| Vine consumption | 4 fruit/cycle |
| Produces | Peat (200 kg/cycle from Vine Fruit) |
| Egg | StegoEgg (base 98%, 2% Algae Stego) |

### Algae Stegosaurus (AlgaeStego)

| Property | Value |
|----------|-------|
| Eats | Vine Fruit, Bristle Berry (and Swamp Fruit if Spaced Out DLC active) |
| Vine consumption | 4 fruit/cycle |
| Produces | Algae (132 kg/cycle from Vine Fruit) |
| Egg | AlgaeStegoEgg (base 65%, 35% Stegosaurus) |

---

## Raptor -- DLC4 (Biome Bundle)

Large predator (2x2 grid). Eats meat and hunts smaller creatures. Faction: Predator. Grows feathers when well-fed.

| Property | Value |
|----------|-------|
| Mass | 400 kg |
| Lifespan | 200 cycles |
| Space | 16 tiles (TIER4) |
| Temperature | Comfort: -50 to 15C (223.15-288.15K), lethal: -100 to 100C (173.15-373.15K) |
| Eats | Dinosaur Meat, Meat; also hunts and butchers smaller creatures (Hatches, Pips, Shove Voles, Dreckos, Slicksters, baby Stegos, Chameleons, and more) |
| Produces | Brine Ice |
| Meat conversion | 128 kg output per unit eaten; 256 kg from butchered prey |
| Scale product | Feather Fabric |
| Scale growth | 4 cycles to full, drops 4 kg feather fabric (1 kg/cycle), starts at 90% growth |
| Scale levels | 2 |
| HP | 50 |
| Death drop | 5 Dinosaur Meat |
| Egg | RaptorEgg (100% Raptor, no variants) |
| Egg mass | 8 kg |
| DLC | DLC4 (Biome Bundle) |

---

## Seal -- DLC2 (Frosty Planet Pack)

Small floor walkers. Eat Sugar Trees (from plant storage) and Sucrose. Produce Ethanol. Not drownable. Faction: Pest.

| Property | Value |
|----------|-------|
| Mass | 100 kg |
| Lifespan | 100 cycles |
| Space | 12 tiles (TIER3) |
| Temperature | Comfort: -75 to 10C (198.15-283.15K), lethal: -100 to 100C (173.15-373.15K) |
| Eats | Sugar Trees (from plant storage, GOOD_3 100% conversion); also Sucrose (40 kg/cycle) |
| Produces | Ethanol |
| HP | 25 |
| Death drop | 50 Tallow |
| Special | 20% chance to drop a Space Tree Seed when pooping |
| Egg | SealEgg (100% Seal, no variants) |
| Egg mass | 2 kg |
| DLC | DLC2 (Frosty Planet Pack) |

---

## Prehistoric Pacu -- DLC4 (Biome Bundle)

Large swimmer (2x2 grid). Hunts regular Pacu. Not drownable, not entombable. Faction: Prey. Not ranchable (no grooming). Fish overcrowding monitor.

| Property | Value |
|----------|-------|
| Mass | 200 kg |
| Lifespan | 100 cycles |
| Space | 12 tiles (TIER3) |
| Temperature | Comfort: 0-60C (273.15-333.15K), lethal: -20 to 100C (253.15-373.15K) |
| Eats | Pacu (all variants, 1 per cycle); also Fish Meat (1 kg/cycle) |
| Produces | Rust (60 kg per Pacu consumed) |
| HP | 50 (base config says 25, overridden to 50 in PrehistoricPacuConfig) |
| Death drop | 12 Prehistoric Pacu Fillet |
| Egg | PrehistoricPacuEgg (100% Prehistoric Pacu, no variants) |
| Egg mass | 4 kg |
| Tagged | LargeCreature |
| DLC | DLC4 (Biome Bundle) |

---

## Ice Belly Family (Belly) -- DLC2 (Frosty Planet Pack)

Large floor walkers (2x2 grid). Warm-blooded (1.3 kW heat generation). Eat Carrot Plants, Carrots, Fries Carrot, Nosh Bean Plants, and Nosh Beans. Produce Ice Belly Poop (with Pollen Germs). Can burrow. Faction: Pest.

**Shared traits:** 400 kg mass, 200-cycle lifespan, 16-tile space requirement (TIER4, uses MooTuning), 8 kg egg mass. Drownable, not entombable. Temperature comfort: 30-70C (303.15-343.15K), lethal: -100 to 100C (173.15-373.15K). Death drop: 14 Meat.

### Ice Belly (base)

| Property | Value |
|----------|-------|
| Eats | Carrot Plants (directly), Carrots, Fries Carrot, Nosh Bean Plants (directly), Nosh Beans |
| Produces | Ice Belly Poop (emits 1000 Pollen Germs per kg) |
| Scale product | Reed Fiber (BasicFabric) |
| Scale growth | 10 cycles to full, drops 5 kg reed fiber (0.5 kg/cycle), starts at 25% growth |
| Scale levels | 6 |
| HP | 25 |
| Egg | IceBellyEgg (base 100% Ice Belly, 0% Gold Belly; diet-modified) |

### Gold Belly (GoldBelly)

| Property | Value |
|----------|-------|
| Eats | Same diet as Ice Belly (Carrot Plants, Carrots, Fries Carrot, Nosh Bean Plants, Nosh Beans) |
| Produces | Ice Belly Poop (emits 1000 Pollen Germs per kg) |
| Scale product | Gold Belly Crown (requires eating Fries Carrot to grow) |
| Scale growth | 10 cycles to full, drops 250 kg gold belly crown (25 kg/cycle), starts at 25% growth |
| Scale levels | 6 |
| HP | 50 |
| Egg | GoldBellyEgg (base 98%, 2% Ice Belly) |

---

## Butterfly -- DLC4 (Biome Bundle)

Small flyer. Pollinates plants (10-tile radius). Does not eat. Very short lifespan. Faction: Pest. Lured by Algae.

| Property | Value |
|----------|-------|
| Mass | 5 kg |
| Lifespan | 5 cycles |
| Space | 8 tiles (TIER2) |
| Temperature | Comfort: 10-45C (283.15-318.15K), lethal: -40 to 80C (233.15-353.15K) |
| Diet | None (does not eat) |
| HP | 5 |
| Death drop | 1 Butterfly Plant Seed |
| Special | Pollinator. No egg-based reproduction. |
| Reproduction | Does not lay eggs |
| DLC | DLC4 (Biome Bundle) |

---

## Mosquito -- DLC4 (Biome Bundle)

Small flyer (adults) / swimmer (larvae). Attacks duplicants and other creatures to feed (poke behavior). Short lifespan. Faction: Prey. Larvae live in water, adults fly.

| Property | Value |
|----------|-------|
| Mass | 5 kg |
| Lifespan | 10 cycles (5 as baby, 5 as adult) |
| Space | 4 tiles (TIER1, adults); 0 (babies/larvae) |
| Temperature | Comfort: 5-65C (278.15-338.15K), lethal: 0-75C (273.15-348.15K) |
| Diet | Feeds by poking duplicants and creatures (not calorie-based) |
| HP | 5 |
| Targets | Duplicants and creatures (excludes Mosquitoes, swimming creatures, dead, and suited duplicants) |
| Egg | MosquitoEgg (100% Mosquito, no variants) |
| Egg mass | 1 kg |
| Special | Babies are aquatic swimmers; adults are flyers that drown in liquid. Spawns at 50% age minimum. Eggs cannot be cracked. |
| DLC | DLC4 (Biome Bundle) |

---

## Chameleon -- DLC4 (Biome Bundle)

Wall/ceiling walker (Drecko navigation). Eats Dew Drips, produces Bleach Stone and emits Chlorine Gas. Shakes Dew Dripper Plants to harvest fruit. Faction: Pest.

| Property | Value |
|----------|-------|
| Mass | 50 kg |
| Lifespan | 50 cycles |
| Space | 4 tiles (TIER1) |
| Temperature | Comfort: -40 to 20C (233.15-293.15K), lethal: -100 to 100C (173.15-373.15K) |
| Eats | Dew Drips (1 per cycle) |
| Produces | Bleach Stone (10 kg per Dew Drip consumed) |
| Secondary output | Chlorine Gas (0.005 kg/s continuous emission) |
| HP | 25 |
| Death drop | 0.5 Meat |
| Special | Shakes Dew Dripper Plants to harvest fruit (10-tile radius, 150s cooldown) |
| Egg | ChameleonEgg (base 98%) |
| Egg mass | 2 kg |
| DLC | DLC4 (Biome Bundle) |

---

## Shockworm

Hostile flyer (1x2 grid). No diet, no reproduction. Area-of-effect electrical attack.

| Property | Value |
|----------|-------|
| Mass | 50 kg |
| Temperature | Comfort: -10 to 40C (263.15-313.15K), lethal: -20 to 50C (253.15-323.15K) |
| Diet | None |
| HP | 32 |
| Death drop | 3 Meat |
| Weapon | 3-6 damage, AoE, 10 targets, 4-tile range, with status effect |
| Faction | Hostile |
| Reproduction | None |

---

## Key Production Rates Summary

Useful throughput numbers for ranch planning.

| Creature | Input | Input rate/cycle | Output | Output rate/cycle |
|----------|-------|------------------|--------|-------------------|
| Hatch | Sandstone | 140 kg | Coal | 70 kg |
| Smooth Hatch | Copper Ore | 100 kg | Copper | 75 kg |
| Sage Hatch | Dirt | 140 kg | Coal | 140 kg |
| Drecko | Balm Lily growth | 0.75 days | Phosphorite | 10 kg |
| Drecko (scales) | Hydrogen atmo | -- | Reed Fiber | 0.25 kg |
| Glossy Drecko (scales) | Hydrogen atmo | -- | Plastic | 50 kg |
| Puft | Polluted Oxygen | 50 kg | Slime | 47.5 kg |
| Dense Puft | Oxygen | 50 kg | Oxylite | 47.5 kg |
| Squeaky Puft | Chlorine | 30 kg | Bleach Stone | 28.5 kg |
| Slickster | CO2 | 20 kg | Crude Oil | 10 kg |
| Molten Slickster | CO2 | 20 kg | Petroleum | 10 kg |
| Pacu | Algae | 7.5 kg | Polluted Dirt | 3.75 kg |
| Gassy Moo | Gas Grass growth | 2 days | Nat. Gas | 10 kg |
| Diesel Moo | Gas Grass growth | 2 days | CO2 | 10 kg |
| Sweetle | Sulfur | 20 kg | Sucrose | 10 kg |
| Pokeshell | Polluted Dirt | 70 kg | Sand | 35 kg |
| Shove Vole | Regolith | 4800 kg* | Regolith tiles | 2400 kg* |
| Plug Slug | Metal Ore | 60 kg | Hydrogen | 3 kg |
| Shove Vole Delicacy (scales) | Heat (70-80C) | -- | Ginger | 1 kg |
| Wood Deer (antlers) | Well-fed | -- | Wood Logs | 60 kg |
| Glass Deer (antlers) | Well-fed + Abyssalite | -- | Glass | 10 kg |
| Stegosaurus | Vine Fruit | 4 fruits | Peat | 200 kg |
| Algae Stegosaurus | Vine Fruit | 4 fruits | Algae | 132 kg |
| Raptor | Meat | varies | Brine Ice | varies |
| Seal | Sugar Tree / Sucrose | 40 kg | Ethanol | 40 kg |
| Prehistoric Pacu | Pacu | 1 fish | Rust | 60 kg |
| Ice Belly (scales) | Well-fed | -- | Reed Fiber | 0.5 kg |
| Gold Belly (scales) | Well-fed + Fries Carrot | -- | Gold Belly Crown | 25 kg |
| Chameleon | Dew Drips | 1 drip | Bleach Stone | 10 kg |

*Shove Vole calorie rates imply very high consumption but actual throughput depends on access to diggable tiles.
