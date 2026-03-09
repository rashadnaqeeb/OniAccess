# Refinement

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

---

## Compost

- **Name:** Compost
- **Description:** Composts safely deal with biological waste, producing fresh dirt.
- **Effect:** Reduces Polluted Dirt, rotting Foods, and discarded organics down into Dirt.
- **Size:** 2x2
- **Construction:** 800 kg Raw Minerals
- **HP:** 30
- **Decor:** -20 (radius 4)
- **Power:** None
- **Heat:** 0.125 kDTU/s exhaust, 1 kDTU/s self
- **Overheatable:** No
- **Duplicant-operated:** Yes (manual delivery, manual toggle)
- **Technology:** Farming (FarmingTech)
- **Storage:** 2000 kg capacity; manual delivery 300 kg (compostable items), refill at 60 kg
- **Conversion:** 0.1 kg/s Compostable -> 0.1 kg/s Dirt at 348.15 K (75 C)
- **Output behavior:** Drops Dirt in 10 kg chunks

---

## Water Sieve (WaterPurifier)

- **Name:** Water Sieve
- **Description:** Sieves cannot kill germs and pass any they receive into their waste and water output.
- **Effect:** Produces clean Water from Polluted Water using Sand. Produces Polluted Dirt.
- **Size:** 4x3
- **Construction:** 200 kg All Metals
- **HP:** 100
- **Decor:** -15 (radius 3)
- **Power:** 120 W
- **Heat:** 0 kDTU/s exhaust, 4 kDTU/s self
- **Duplicant-operated:** No (automated)
- **Flippable:** Yes (FlipH)
- **Technology:** Distillation
- **Liquid input:** Polluted Water via pipe (input offset -1,2)
- **Liquid output:** Water via pipe (output offset 2,2), filters out Dirty Water
- **Manual delivery:** 1200 kg Sand (Filter), refill at 300 kg
- **Conversion:**
  - 1 kg/s Sand (Filter) + 5 kg/s Polluted Water -> 5 kg/s Water + 0.2 kg/s Polluted Dirt
  - Output temperature: input temperature (no forced temp)
- **Output behavior:** Polluted Dirt drops in 10 kg chunks; Water dispatched via liquid output pipe

---

## Desalinator

- **Name:** Desalinator
- **Description:** Salt can be refined into table salt for a mealtime morale boost.
- **Effect:** Removes Salt from Brine or Salt Water, producing Water.
- **Size:** 4x3
- **Construction:** 200 kg Raw Metals
- **HP:** 30
- **Decor:** -5 (radius 1)
- **Power:** 480 W
- **Heat:** 0 kDTU/s exhaust, 8 kDTU/s self
- **Floodable:** No
- **Flippable:** Yes (FlipH)
- **Technology:** Liquid Filtering (LiquidFiltering)
- **Liquid input:** Salt Water or Brine via pipe (input offset -1,0)
- **Liquid output:** Water via pipe (output offset 0,0), filters out Salt Water and Brine
- **Conversion (two modes, selected automatically by input):**
  - Salt Water: 5 kg/s Salt Water -> 4.65 kg/s Water + 0.35 kg/s Salt
  - Brine: 5 kg/s Brine -> 3.5 kg/s Water + 1.5 kg/s Salt
  - Output temperature: input temperature (no forced temp)
- **Salt storage:** Max 945 kg before requiring emptying (duplicant errand, 90s work time)
- **Special:** Duplicant must empty salt buildup periodically

---

## Algae Distiller (AlgaeDistillery)

- **Name:** Algae Distiller
- **Description:** Algae distillers convert disease-causing slime into algae for oxygen production.
- **Effect:** Refines Slime into Algae.
- **Size:** 3x4
- **Construction:** 200 kg All Metals
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** 120 W
- **Heat:** 0.5 kDTU/s exhaust, 1 kDTU/s self
- **Overheatable:** No
- **Duplicant-operated:** No (automated)
- **Technology:** Distillation
- **Manual delivery:** 480 kg Slime, refill at 120 kg
- **Liquid output:** Dirty Water via pipe
- **Storage:** 1000 kg
- **Conversion:** 0.6 kg/s Slime -> 0.2 kg/s Algae + 0.4 kg/s Polluted Water
  - Output temperature: 303.15 K (30 C) for both
- **Output behavior:** Algae emitted in 30 kg chunks

---

## Ethanol Distiller (EthanolDistillery)

- **Name:** Ethanol Distiller
- **Description:** Ethanol distillers convert Wood into burnable Ethanol fuel.
- **Effect:** Refines Wood into Ethanol.
- **Size:** 4x3
- **Construction:** 200 kg All Metals
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** 240 W
- **Heat:** 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheatable:** No
- **Duplicant-operated:** No (automated)
- **Technology:** Distillation
- **Manual delivery:** 600 kg Lumber (BuildingWood), refill at 150 kg
- **Liquid output:** Ethanol via pipe (output offset -1,0)
- **Storage:** 1000 kg
- **Conversion:** 1 kg/s Lumber -> 0.5 kg/s Ethanol (346.5 K) + 0.333 kg/s Polluted Dirt (366.5 K) + 0.167 kg/s Carbon Dioxide (366.5 K)
- **Output behavior:** Polluted Dirt emitted in 20 kg chunks; CO2 vented to environment

---

## Fertilizer Synthesizer (FertilizerMaker)

- **Name:** Fertilizer Synthesizer
- **Description:** Fertilizer synthesizers convert polluted dirt into fertilizer for domestic plants.
- **Effect:** Uses Polluted Water and Phosphorite to produce Fertilizer.
- **Size:** 4x3
- **Construction:** 200 kg All Metals
- **HP:** 30
- **Decor:** -15 (radius 3)
- **Power:** 120 W
- **Heat:** 1 kDTU/s exhaust, 2 kDTU/s self
- **Duplicant-operated:** No (automated)
- **Technology:** Agriculture
- **Liquid input:** Polluted Water via pipe (input offset 0,0); capacity 0.195 kg
- **Manual delivery:**
  - 136.5 kg Dirt, refill at 19.5 kg
  - 54.6 kg Phosphorite, refill at 7.8 kg
- **Conversion:** 0.065 kg/s Dirt + 0.039 kg/s Polluted Water + 0.026 kg/s Phosphorite -> 0.12 kg/s Fertilizer (323.15 K / 50 C) + 0.01 kg/s Natural Gas (349.15 K / 76 C)
- **Output behavior:** Fertilizer drops in 10 kg chunks; Natural Gas emitted to environment at offset (2,2)

---

## Rock Crusher (RockCrusher)

- **Name:** Rock Crusher
- **Description:** Rock Crushers loosen nuggets from raw ore and can process many different resources.
- **Effect:** Inefficiently produces refined materials from raw resources. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 4x4
- **Construction:** 800 kg All Metals
- **HP:** 30
- **Decor:** -15 (radius 3)
- **Power:** 240 W
- **Heat:** 0 kDTU/s exhaust, 16 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** Basic Refinement (BasicRefinement)
- **Recipes (all 40s fabrication time, ComplexFabricator):**

| Recipe | Input | Output | Notes |
|--------|-------|--------|-------|
| Raw Mineral to Sand | 100 kg any Crushable mineral | 100 kg Sand | Temperature preserved |
| Metal Ore to Refined Metal | 100 kg Metal Ore | 50 kg Refined Metal + 50 kg Sand | 50% efficiency; auto-generated for all non-Noncrushable metals whose melt-cycle yields a different element |
| Egg Shell to Lime | 5 kg Egg Shell | 5 kg Lime | Temperature preserved |
| Pokeshell Molt to Lime | 10 kg Pokeshell Molt (CrabShell) | 10 kg Lime | Temperature preserved |
| Oakshell Molt to Lumber | 500 kg Oakshell Molt (CrabWoodShell) | 500 kg Lumber (WoodLog) | Temperature preserved |
| Fossil to Lime + Sedimentary Rock | 100 kg Fossil | 5 kg Lime + 95 kg Sedimentary Rock | Temperature preserved |
| Salt to Table Salt | 100 kg Salt | 0.005 kg Table Salt + 99.995 kg Sand | Temperature preserved |
| Fullerene to Graphite | 100 kg Fullerene | 90 kg Graphite + 10 kg Sand | Spaced Out! DLC (EXPANSION1) |
| Depleted Electrobank recycling | 1 Depleted Electrobank (GarbageElectrobank) | 100 kg Abyssalite (Katairite) | Bionic Booster DLC (DLC3) |
| Brackbomb Shell to Phosphorite + Clay | 120 kg Brackbomb Shell (IceBellyPoop) | 32 kg Phosphorite + 88 kg Clay | Frosty Planet DLC (DLC2) |
| Gold Belly Crown to Gold Amalgam | 1 Gold Belly Crown (GoldBellyCrown) | 250 kg Gold Amalgam | Frosty Planet DLC (DLC2) |

---

## Kiln

- **Name:** Kiln
- **Description:** It gets quite hot.
- **Effect:** Fires Clay to produce Ceramic, and Coal or Wood to produce Refined Carbon. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 2x2
- **Construction:** 200 kg All Metals
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** None
- **Heat:** 16 kDTU/s exhaust, 4 kDTU/s self
- **Overheatable:** No
- **Duplicant-operated:** No (automated, despite being a ComplexFabricator)
- **Technology:** Basic Refinement (BasicRefinement)
- **Recipes (all 40s fabrication time):**

| Recipe | Input | Output | Notes |
|--------|-------|--------|-------|
| Ceramic | 100 kg Clay + 25 kg fuel (Lumber, Plywood, Coal, or Peat) | 100 kg Ceramic | Output at heated temp (353.15 K / 80 C) |
| Refined Carbon | Variable fuel input (see below) | 100 kg Refined Carbon | Output at heated temp (353.15 K / 80 C) |

- **Refined Carbon input amounts vary by material:** 200 kg Lumber, 200 kg Plywood, 125 kg Coal, 300 kg Peat (each produces 100 kg Refined Carbon)

---

## Sludge Press (SludgePress)

- **Name:** Sludge Press
- **Description:** What Duplicant doesn't love playing with mud?
- **Effect:** Separates Mud and other sludges into their base elements. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 4x3
- **Construction:** 200 kg All Minerals
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** 120 W
- **Heat:** 0 kDTU/s exhaust, 4 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** Advanced Filtration (AdvancedFiltration)
- **DLC:** Spaced Out! (EXPANSION1)
- **Liquid output:** via pipe (output offset 1,0), dispenses all liquids
- **Recipes:** Dynamically generated for every element that has an `elementComposition` defined (composite/sludge elements). Each recipe takes 150 kg of the sludge element and outputs its component elements proportionally. Fabrication time: 20s per recipe. Output temperature preserved from input.
- **Special:** Recipes are data-driven from element definitions, not hardcoded. The exact sludge elements available depend on which DLCs are active.

---

## Metal Refinery (MetalRefinery)

- **Name:** Metal Refinery
- **Description:** Refined metals are necessary to build advanced electronics and technologies.
- **Effect:** Produces Refined Metals from raw Metal Ore. Significantly Heats and outputs the Liquid piped into it. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 3x4
- **Construction:** 800 kg All Minerals
- **HP:** 30
- **Decor:** -15 (radius 3)
- **Power:** 1200 W
- **Heat:** 0 kDTU/s exhaust, 16 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** Smelting
- **Liquid input:** Any liquid as coolant via pipe (input offset -1,1); capacity 800 kg
- **Liquid output:** Heated coolant via pipe (output offset 1,0)
- **Coolant:** Requires 400 kg minimum of any Liquid; 80% of process heat goes into coolant (thermalFudge = 0.8)
- **Output storage:** 2000 kg
- **Recipes (all 40s fabrication time):**

| Recipe | Input | Output | Notes |
|--------|-------|--------|-------|
| Metal Ore to Refined Metal | 100 kg Metal Ore | 100 kg Refined Metal | Auto-generated for all non-Noncrushable metals; 100% mass efficiency; temperature preserved |
| Steel | 70 kg Iron + 20 kg Refined Carbon + 10 kg Lime | 100 kg Steel | Temperature preserved |

- **Special:** The liquid coolant absorbs most of the heat. Choice of coolant is critical for heat management. Outputs coolant via liquid pipe.

---

## Glass Forge (GlassForge)

- **Name:** Glass Forge
- **Description:** Glass can be used to construct window tile.
- **Effect:** Produces Molten Glass from raw Sand. Outputs High Temperature Liquid. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 5x4
- **Construction:** 800 kg All Minerals
- **HP:** 30
- **Decor:** -15 (radius 3)
- **Power:** 1200 W
- **Heat:** 0 kDTU/s exhaust, 16 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** High Temp Forging (HighTempForging)
- **Liquid output:** via pipe (output offset 1,3), dispenses all output
- **Output storage:** 2000 kg
- **Recipe (40s fabrication time):**
  - 100 kg Sand -> 25 kg Molten Glass
  - Output temperature: melting point of glass (TemperatureOperation.Melted)
- **Special:** 75% mass loss (100 kg Sand yields only 25 kg Molten Glass). Molten Glass is output at extremely high temperature via liquid pipe. The Molten Glass solidifies into Glass tiles/items when cooled below its freezing point.

---

## Oil Refinery (OilRefinery)

- **Name:** Oil Refinery
- **Description:** Petroleum can only be produced from the refinement of crude oil.
- **Effect:** Converts Crude Oil into Petroleum and Natural Gas.
- **Size:** 4x4
- **Construction:** 200 kg All Metals
- **HP:** 30
- **Decor:** -10 (radius 2)
- **Power:** 480 W
- **Heat:** 2 kDTU/s exhaust, 8 kDTU/s self
- **Duplicant-operated:** Yes (isManuallyOperated = true)
- **Flippable:** Yes (FlipH)
- **Technology:** Improved Combustion (ImprovedCombustion)
- **Liquid input:** Crude Oil via pipe (input offset 0,0); capacity 100 kg
- **Liquid output:** Petroleum via pipe (output offset 1,1), filters out Crude Oil
- **Conversion:** 10 kg/s Crude Oil -> 5 kg/s Petroleum (348.15 K / 75 C) + 0.09 kg/s Natural Gas (348.15 K / 75 C)
- **Overpressure:** Warning at 4.5 kg gas in cell, stops at 5 kg
- **Special:** Natural Gas is emitted to environment (not stored). 50% mass conversion to Petroleum; ~49.1% mass is lost (not conserved). Total output is 5.09 kg/s from 10 kg/s input.

---

## Polymer Press (Polymerizer)

- **Name:** Polymer Press
- **Description:** Plastic can be used to craft unique buildings and goods.
- **Effect:** Converts Plastic Monomers into raw Plastic.
- **Size:** 3x3
- **Construction:** 400 kg All Metals
- **HP:** 30
- **Decor:** 0 (radius 1)
- **Power:** 240 W
- **Heat:** 0.5 kDTU/s exhaust, 32 kDTU/s self
- **Duplicant-operated:** No (automated, state machine driven)
- **Flippable:** Yes (FlipH)
- **Technology:** Plastics
- **Liquid input:** Plastifiable Liquid (Petroleum or Naphtha) via pipe (input offset 0,0); capacity 1.667 kg
- **Gas output:** Carbon Dioxide via pipe (output offset 0,1)
- **Conversion:** 0.833 kg/s Plastifiable Liquid -> 0.5 kg/s Plastic (Polypropylene, 348.15 K / 75 C) + 0.00833 kg/s Steam (473.15 K / 200 C) + 0.00833 kg/s Carbon Dioxide (423.15 K / 150 C)
- **Output behavior:** Plastic emitted as solid blob at 30 kg; Steam vented to environment; CO2 sent via gas output pipe
- **Special:** Very high self-heating (32 kDTU/s). ~40% mass loss overall (0.833 in, ~0.517 out).

---

## Oxylite Refinery (OxyliteRefinery)

- **Name:** Oxylite Refinery
- **Description:** Oxylite is a solid and easily transportable source of consumable oxygen.
- **Effect:** Synthesizes Oxylite using Oxygen and a small amount of Gold.
- **Size:** 3x4
- **Construction:** 800 kg Refined Metal + 100 kg Plastic (two-material build)
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** 1200 W
- **Heat:** 8 kDTU/s exhaust, 4 kDTU/s self
- **Overheatable:** No
- **Duplicant-operated:** No (automated)
- **Technology:** Catalytics
- **Gas input:** Oxygen via pipe (input offset 1,0); capacity 6 kg, consumption rate 1.2 kg/s
- **Manual delivery:** 7.2 kg Gold, refill at 1.8 kg
- **Storage:** 23.2 kg total capacity
- **Conversion:** 0.6 kg/s Oxygen + 0.003 kg/s Gold -> 0.6 kg/s Oxylite (OxyRock, 303.15 K / 30 C)
- **Output behavior:** Oxylite emitted in 10 kg chunks
- **Special:** Gold is consumed very slowly (0.003 kg/s) as a catalyst. Construction time is 480s (much longer than most buildings).

---

## Chlorinator (Bleach Stone Hopper)

- **Name:** Bleach Stone Hopper
- **Description:** Bleach stone is useful for sanitation and geotuning.
- **Effect:** Uses Salt and Gold to produce Bleach Stone.
- **Size:** 3x3
- **Construction:** 800 kg Refined Metals
- **HP:** 100
- **Decor:** -15 (radius 3)
- **Power:** 480 W
- **Heat:** 1 kDTU/s exhaust, 2 kDTU/s self
- **Duplicant-operated:** No (automated)
- **Flippable:** Yes (FlipH)
- **Technology:** Catalytics
- **Recipe (40s fabrication time):**
  - 30 kg Salt + 0.5 kg Gold -> 10 kg Bleach Stone + 20 kg Sand
- **Output behavior:** Bleach Stone ejected in 2 kg ore chunks (2-3 at a time); Sand ejected in 6 kg ore chunks (1 at a time). Both launched with velocity.

---

## Plywood Press (FabricatedWoodMaker)

- **Name:** Plywood Press
- **Description:** Flattened plant bits are a useful wood substitute.
- **Effect:** Combines a Binder liquid and Plant Fiber to create Plywood.
- **Size:** 4x3
- **Construction:** 200 kg All Metals
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** 480 W
- **Heat:** 0.25 kDTU/s exhaust, 1 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** Refined Objects (RefinedObjects)
- **Liquid input:** Natural Resin via pipe (input offset 0,0); capacity 1000 kg
- **Recipe (40s fabrication time):**
  - 90 kg Plant Fiber + 10 kg Natural Resin -> 100 kg Plywood (FabricatedWood)
  - Output temperature: 333.15 K (60 C) (heated)
- **Special:** Natural Resin piped in via liquid conduit but conduit is not required for operation (requireConduit: false). Excess liquids are kept.

---

## Emulsifier (ChemicalRefinery)

- **Name:** Emulsifier
- **Description:** It's like a blender, but better.
- **Effect:** Combines Liquids and other inputs into fluid compounds. Duplicants will not fabricate emulsions unless recipes are queued.
- **Size:** 4x3
- **Construction:** 200 kg Refined Metal + 100 kg Glass (two-material build)
- **HP:** 30
- **Decor:** -10 (radius 2)
- **Power:** 480 W
- **Heat:** 0 kDTU/s exhaust, 1 kDTU/s self
- **Duplicant-operated:** Yes
- **Required skill:** Chemistry (AllowChemistry)
- **Technology:** Advanced Distillation (AdvancedDistillation)
- **Liquid input:** via pipe (input offset 0,0)
- **Liquid output:** via pipe (output offset 2,1), dispenses all output
- **Input storage:** 1000 kg
- **Output temperature:** 313.15 K (40 C) for heated recipes
- **Recipes:**

| Recipe | Input | Output | Time | Tech gate |
|--------|-------|--------|------|-----------|
| Salt Water | 93 kg Water + 7 kg Salt | 100 kg Salt Water | 40s | None |
| Biodiesel (Refined Lipid) | 160 kg Phyto Oil + 40 kg Bleach Stone | 200 kg Refined Lipid | 40s | None |
| Super Coolant | 1 kg Fullerene + 49.5 kg Gold + 49.5 kg Petroleum | 100 kg Super Coolant | 80s | Catalytics (SUPER_LIQUIDS) |
| Visco-Gel | 35 kg Isoresin + 65 kg Petroleum | 100 kg Visco-Gel | 80s | Catalytics (SUPER_LIQUIDS) |

---

## Molecular Forge (SupermaterialRefinery)

- **Name:** Molecular Forge
- **Description:** Rare materials can be procured through rocket missions into space.
- **Effect:** Processes Rare Materials into advanced industrial goods. Rare materials can be retrieved from space missions. Duplicants will not fabricate items unless recipes are queued.
- **Size:** 4x5
- **Construction:** 800 kg All Metals
- **HP:** 30
- **Decor:** -15 (radius 3)
- **Power:** 1600 W
- **Heat:** 0 kDTU/s exhaust, 16 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** Catalytics
- **Output temperature:** 313.15 K (40 C) for all heated recipes
- **Recipes (all 80s fabrication time):**

| Recipe | Input | Output | DLC |
|--------|-------|--------|-----|
| Fullerene | 90 kg Graphite + 5 kg Sulfur + 5 kg Aluminum | 100 kg Fullerene | Spaced Out! (EXPANSION1) |
| Plastium (Hard Polypropylene) | 15 kg Thermium + 70 kg Plastic (Polypropylene) + 15 kg Brackwax (MilkFat) | 100 kg Plastium | Base game |
| Insulite (Super Insulator) | 15 kg Isoresin + 80 kg Abyssalite (Katairite) + 5 kg Reed Fiber or Thimble Reed Fiber | 100 kg Insulite | Base game |
| Thermium (TempConductorSolid) | 5 kg Niobium + 95 kg Tungsten | 100 kg Thermium | Base game |
| Atomic Power Bank (SelfChargingElectrobank) | 10 kg Enriched Uranium | 1 Atomic Power Bank | Spaced Out! + Bionic Booster (DLC3) |

---

## Diamond Press (DiamondPress)

- **Name:** Diamond Press
- **Description:** Crushes refined carbon into diamond.
- **Effect:** Uses Power and Radbolts to crush Refined Carbon into Diamond. Duplicants will not fabricate items unless recipes are queued and Refined Carbon has been discovered.
- **Size:** 3x5
- **Construction:** 800 kg All Metals
- **HP:** 30
- **Decor:** -15 (radius 3)
- **Power:** 240 W
- **Heat:** 0 kDTU/s exhaust, 16 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** High Pressure Forging (HighPressureForging)
- **DLC:** Spaced Out! (EXPANSION1)
- **Radbolt input:** HEP port at offset (0,2); capacity 2000 radbolts, auto-stores
- **Radbolt cost:** 1000 radbolts per recipe (10 per kg of output)
- **Logic output:** HEP storage status at offset (0,2)
- **Recipe (80s fabrication time):**
  - 100 kg Refined Carbon -> 100 kg Diamond
  - Temperature preserved from input

---

## Brackwax Gleaner (MilkFatSeparator)

- **Name:** Brackwax Gleaner
- **Description:** Duplicants can slather up with brackwax to increase their travel speed in transit tubes.
- **Effect:** Refines Brackene (Milk) into Brine and Brackwax (MilkFat), and emits Carbon Dioxide.
- **Size:** 4x4
- **Construction:** 800 kg Refined Metals
- **HP:** 100
- **Decor:** -15 (radius 3)
- **Power:** 480 W
- **Heat:** 0 kDTU/s exhaust, 8 kDTU/s self
- **Duplicant-operated:** No (automated, requires manual emptying)
- **Flippable:** Yes (FlipH)
- **Technology:** Dairy Operation (DairyOperation)
- **Liquid input:** Brackene (Milk) via pipe (input offset 0,0); capacity 4 kg
- **Liquid output:** Brine via pipe (output offset 2,2), filters out Milk
- **Conversion:** 1 kg/s Brackene -> 0.09 kg/s Brackwax (MilkFat) + 0.81 kg/s Brine + 0.1 kg/s Carbon Dioxide (348.15 K / 75 C)
  - Brackwax and Brine output at input temperature (no forced temp)
  - CO2 emitted to environment at 348.15 K
- **Brackwax storage:** 15 kg capacity, requires manual emptying
- **Special:** 90% efficiency (10% mass becomes CO2)

---

## Plant Pulverizer (MilkPress)

- **Name:** Plant Pulverizer
- **Description:** For Duplicants who are too squeamish to milk critters.
- **Effect:** Crushes organic materials to extract liquids such as Brackene (Milk) or Phyto Oil. Brackene can be used to refill the Critter Fountain.
- **Size:** 2x3
- **Construction:** 400 kg All Minerals
- **HP:** 100
- **Decor:** -10 (radius 2)
- **Power:** None
- **Heat:** 0 kDTU/s exhaust, 2 kDTU/s self
- **Duplicant-operated:** Yes
- **Technology:** Food Repurposing (FoodRepurposing)
- **Liquid output:** via pipe (output offset 1,0), dispenses all output
- **Recipes (all 40s fabrication time):**

| Recipe | Input | Output | DLC |
|--------|-------|--------|-----|
| Sleet Wheat Grain to Brackene | 10 kg Sleet Wheat Grain (ColdWheatSeed) + 15 kg Water | 20 kg Brackene (Milk) | Base game |
| Pincha Peppernut to Brackene | 3 kg Pincha Peppernut (SpiceNut) + 17 kg Water | 20 kg Brackene (Milk) | Base game |
| Nosh Bean to Brackene | 2 kg Nosh Bean (BeanPlantSeed) + 18 kg Water | 20 kg Brackene (Milk) | Base game |
| Dew Drip to Brackene | 2 Dew Drip (DewDrip) | 20 kg Brackene (Milk) | DLC4 |
| Slime to Phyto Oil | 100 kg Slime (SlimeMold) | 70 kg Phyto Oil + 30 kg Dirt | Base game |
| Kelp to Phyto Oil | 25 kg Kelp + 75 kg Water | 100 kg Phyto Oil | DLC4 |
| Amber to Natural Resin | 100 kg Amber | 50 kg Natural Resin + 25 kg Fossil + 25 kg Sand | DLC4 |

- **Special:** All recipes preserve input temperature (AverageTemperature). No power required.
