# Stations

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

## Research Station
- **ID**: ResearchCenter
- **Name**: Research Station
- **Description**: Research stations are necessary for unlocking all research tiers.
- **Effect**: Conducts Novice Research to unlock new technologies. Consumes Dirt.
- **Size**: 2x2
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 60 W
- **Heat**: 0.125 kDTU/s exhaust, 1 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Research type**: basic (Novice Research)
- **Input**: Dirt, 50 kg per research point, consumed at 1.111 kg/s (= 50 kg / 45 s per point)
- **Storage capacity**: 750 kg Dirt (refill threshold 150 kg)
- **Manually operated**: Yes
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Base game

## Super Computer
- **ID**: AdvancedResearchCenter
- **Name**: Super Computer
- **Description**: Super computers unlock higher technology tiers than research stations alone.
- **Effect**: Conducts Advanced Research. Consumes Water. Requires Advanced Research skill.
- **Size**: 3x3
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 120 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Research type**: advanced
- **Input**: Water, 50 kg per research point, consumed at 0.833 kg/s (= 50 kg / 60 s per point)
- **Storage capacity**: 750 kg Water (refill threshold 150 kg)
- **Manually operated**: Yes
- **Required skill**: Advanced Research (AllowAdvancedResearch)
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Base game

## Materials Study Terminal
- **ID**: NuclearResearchCenter
- **Name**: Materials Study Terminal
- **Description**: Comes with a few ions thrown in, free of charge.
- **Effect**: Conducts Materials Science Research. Consumes Radbolts. Requires Applied Sciences Research skill.
- **Size**: 5x3
- **HP**: 30
- **Construction**: 400 kg Refined Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 120 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Research type**: nuclear (Materials Science Research)
- **Input**: Radbolts (High Energy Particles), 10 particles per research point, 100 s per point
- **Radbolt storage**: 100 particles capacity, input port at offset (-2, 1)
- **Logic output**: HEP_STORAGE port at offset (2, 2) -- signals radbolt storage level
- **Manually operated**: Yes
- **Required skill**: Applied Sciences Research (AllowNuclearResearch)
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Spaced Out! (EXPANSION1). Deprecated if radiation is disabled.

## Orbital Data Collection Lab
- **ID**: OrbitalResearchCenter
- **Name**: Orbital Data Collection Lab
- **Description**: Orbital Data Collection Labs record data while orbiting a Planetoid and write it to a Data Bank.
- **Effect**: Creates Data Banks that can be consumed at a Virtual Planetarium. Consumes Plastic and Power.
- **Size**: 2x3
- **HP**: 30
- **Construction**: 200 kg Plastic, build time 120 s
- **Decor**: 0 (radius 1)
- **Power**: 60 W
- **Heat**: 0.125 kDTU/s exhaust, 0.5 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Recipe**: 5 kg Plastic -> 1 Orbital Research Databank, 33 s per craft
- **Output temperature**: 308.15 K (35 C)
- **Required skill**: Mission Control (CanMissionControl)
- **Special**: Must be in orbit (InOrbitRequired). Tagged as Rocket Interior building.
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Spaced Out! (EXPANSION1)

## Virtual Planetarium (Base Game)
- **ID**: CosmicResearchCenter
- **Name**: Virtual Planetarium
- **Description**: Planetariums allow the simulated exploration of locations discovered with a telescope.
- **Effect**: Conducts Interstellar Research. Consumes Data Banks from Research Modules. Requires Astronomy skill.
- **Size**: 4x4
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 120 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Research type**: space (Interstellar Research)
- **Input**: Data Banks (ResearchDatabank), 1 per research point, consumed at 0.02 kg/s (= 1 / 50 s per point)
- **Storage capacity**: 300 Data Banks (refill threshold 3)
- **Manually operated**: Yes
- **Required skill**: Astronomy (AllowInterstellarResearch)
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Base game only (forbidden in Spaced Out!)

## Virtual Planetarium (Spaced Out!)
- **ID**: DLC1CosmicResearchCenter
- **Name**: Virtual Planetarium
- **Description**: Planetariums allow the simulated exploration of locations recorded in Data Banks.
- **Effect**: Conducts Data Analysis Research. Consumes Orbital Research Data Banks generated by exploration.
- **Size**: 4x4
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 120 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Research type**: orbital (Data Analysis Research)
- **Input**: Orbital Research Data Banks, 1 per research point, consumed at 0.02 kg/s (= 1 / 50 s per point)
- **Storage capacity**: 300 Data Banks (refill threshold 3)
- **Manually operated**: Yes
- **Required skill**: Data Analysis (AllowOrbitalResearch)
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Spaced Out! (EXPANSION1)

## Telescope
- **ID**: Telescope
- **Name**: Telescope
- **Description**: Telescopes are necessary for learning starmaps and conducting rocket missions.
- **Effect**: Maps Starmap destinations, producing Data Banks. Requires Field Research skill. Must be exposed to space.
- **Size**: 4x6
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 120 W
- **Heat**: 0.125 kDTU/s exhaust, 0 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Production**: 2 research points per day, 2 mass per point
- **Input material**: Glass
- **Gas input**: Oxygen via gas conduit, 1 kg/s consumption rate, 10 kg capacity
- **Sky visibility**: Scan radius 4 cells from offset (0,3) and (1,3)
- **Manually operated**: Yes
- **Required skill**: Field Research (CanStudyWorldObjects)
- **Room bonus**: Counts as Science building (Laboratory room)
- **DLC**: Base game only (forbidden in Spaced Out!)

## Geotuner
- **ID**: GeoTuner
- **Name**: Geotuner
- **Description**: The targeted geyser receives stored amplification data when it is erupting.
- **Effect**: Increases the Temperature and output of an analyzed Geyser. Multiple Geotuners can target a single Geyser anywhere on an asteroid.
- **Size**: 4x3
- **HP**: 30
- **Construction**: 400 kg Refined Metal, build time 120 s
- **Decor**: 0 (radius 1)
- **Power**: 120 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: Does not overheat (Overheatable = false)
- **Flippable**: Yes (FlipH)
- **Max geotuned geysers**: 5 (per tuner targeting same geyser)
- **Buff duration**: 600 s (one cycle) per charge
- **Logic output**: Geyser Eruption Monitor -- Green when geyser is erupting, Red otherwise
- **Required skill**: Geyser Tuning (AllowGeyserTuning)
- **Room requirement**: Laboratory (required)
- **Geyser category buffs** (each charge lasts 600 s):
  - **Default**: 50 kg Dirt, +10% mass/cycle, +10 K temperature
  - **Water** (steam, hot steam, hot water, salt water, slush salt water, filthy water, slush water): 50 kg Bleach Stone, +20% mass/cycle, +20 K temperature
  - **Organic** (slimy PO2, hot PO2, chlorine gas, cool chlorine gas): 50 kg Salt, +20% mass/cycle, +15 K temperature
  - **Hydrocarbon** (methane, hot hydrogen, liquid sulfur, oil drip): 100 kg Abyssalite, +20% mass/cycle, +15 K temperature
  - **Volcano** (small/big volcano): 100 kg Abyssalite, +20% mass/cycle, +150 K temperature
  - **Metals** (molten copper, gold, iron, aluminum, cobalt, niobium, tungsten): 80 kg Phosphorus, +20% mass/cycle, +50 K temperature
  - **CO2** (hot CO2, liquid CO2): 50 kg Toxic Sand, +20% mass/cycle, +5 K temperature
- **DLC**: Base game (available in all DLCs)

## Data Miner
- **ID**: DataMiner
- **Name**: Data Miner
- **Description**: Data banks can also be used to program robo-pilot rocket modules.
- **Effect**: Mass-produces Data Banks that can be processed into research points.
- **Size**: 3x2
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 1000 W
- **Heat**: 0.5 kDTU/s exhaust, 3 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Recipe**: 5 kg Plastic (Polypropylene) -> 1 Data Bank, 200 s per craft
- **Base units produced per cycle**: 3
- **Production rate scaling**: 0.6x to 5.333x based on temperature (10 K to 325 K range -- colder = faster)
- **Automated**: Not duplicant-operated, runs automatically
- **Logic input**: Automation port at (0,0)
- **Storage**: 1000 kg
- **DLC**: DLC3 (Bionic Booster Pack)

## Power Control Station
- **ID**: PowerControlStation
- **Name**: Power Control Station
- **Description**: Only one Duplicant may be assigned to a station at a time.
- **Effect**: Produces Microchip to increase the Power output of generators. Requires Tune Up skill.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 200 kg Refined Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: None (no power input)
- **Overheat**: Does not overheat (Overheatable = false)
- **Tinker station**: Consumes 5 kg Refined Metal per tinker, produces Power Station Tools
- **Tool production time**: 160 s (modified by Machinery Speed attribute)
- **Storage**: 50 kg capacity, filtered to Refined Metal
- **Logic input**: Automation port at (0,0)
- **Required skill**: Tune Up (CanPowerTinker)
- **Room requirement**: Power Plant (required)
- **Skill experience**: Technicals group
- **DLC**: Base game

## Farm Station
- **ID**: FarmStation
- **Name**: Farm Station
- **Description**: This station only has an effect on crops grown within the same room.
- **Effect**: Produces Micronutrient Fertilizer to increase Plant growth rates. Requires Crop Tending skill. Necessary component of Greenhouse room.
- **Size**: 2x3
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: None (no power input)
- **Overheat**: Does not overheat (Overheatable = false)
- **Tinker station**: Consumes 5 kg Fertilizer per tinker, produces Farm Station Tools
- **Tool production time**: 15 s (modified by Harvest Speed attribute)
- **Storage**: 50 kg capacity (refill threshold 5 kg)
- **Logic input**: Automation port at (0,0)
- **Required skill**: Crop Tending (CanFarmTinker)
- **Room requirement**: Farm (required)
- **Skill experience**: Farming group
- **DLC**: Base game

## Botanical Analyzer
- **ID**: GeneticAnalysisStation
- **Name**: Botanical Analyzer
- **Description**: Would a mutated rose still smell as sweet?
- **Effect**: Identifies new Seed subspecies.
- **Size**: 7x2
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 480 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Input**: Unidentified Seeds, capacity 5, refill at 1.1 (min mass 1)
- **Manually operated**: Yes
- **Required skill**: Seed Identification (CanIdentifyMutantSeeds)
- **Room bonus**: Counts as Science building (Laboratory room)
- **Special**: Deprecated if Plant Mutations feature is disabled
- **DLC**: Spaced Out! (EXPANSION1)

## Grooming Station
- **ID**: RanchStation
- **Name**: Grooming Station
- **Description**: A groomed critter is a happy, healthy, productive critter.
- **Effect**: Allows the assigned Rancher to care for Critters. Requires Critter Ranching skill.
- **Size**: 2x3
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: None (no power input)
- **Overheat**: Does not overheat (Overheatable = false)
- **Work time**: 12 s per ranching action
- **Logic input**: Automation port at (0,0)
- **Required skill**: Critter Ranching (CanUseRanchStation)
- **Room requirement**: Stable / Creature Pen (required)
- **Ranching effect**: Applies "Ranched" effect to critter. Duration scaled by 1 + (Ranching attribute * 0.1). Also fully heals critter HP.
- **Eligibility**: Critters without the "Ranched" effect
- **Target cell**: One cell to the right of the station
- **DLC**: Base game

## Shearing Station
- **ID**: ShearingStation
- **Name**: Shearing Station
- **Description**: Those critters aren't gonna shear themselves.
- **Effect**: Shearing stations allow eligible Critters to be safely sheared for raw materials. Visiting restores Critters' physical and emotional well-being.
- **Size**: 3x3
- **HP**: 100
- **Construction**: 400 kg Raw Metal, build time 10 s
- **Decor**: 0 (radius 1)
- **Power**: 60 W
- **Heat**: 0.125 kDTU/s exhaust, 0.5 kDTU/s self
- **Work time**: 12 s per shearing action
- **Manually operated**: Yes
- **Required skill**: Critter Ranching (CanUseRanchStation)
- **Room requirement**: Stable / Creature Pen (required)
- **Eligibility**: Critters implementing IShearable whose growth is fully complete
- **Special**: Drops sheared material at one cell to the left of the worker, inheriting critter temperature and disease
- **DLC**: Base game

## Milking Station
- **ID**: MilkingStation
- **Name**: Milking Station
- **Description**: The harvested liquid is basically the equivalent of soda for critters.
- **Effect**: Allows Duplicants with Critter Ranching II skill to milk Gassy Moos for Milk. Milk can be used to refill the Milk Feeder.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 400 kg Refined Metal + 200 kg Plastic, build time 60 s
- **Decor**: -15 (radius 3)
- **Power**: None (no power input)
- **Overheat**: Does not overheat (Overheatable = false)
- **Work time**: 20 s per milking action
- **Logic input**: Automation port at (0,0)
- **Liquid output**: Liquid conduit at offset (1,1), always dispenses
- **Storage**: 400 kg capacity (= max(200, 200) * 2, where MILK_AMOUNT_AT_MILKING = 50 * 4 = 200 kg, DIESEL_PER_CYCLE = 200 kg)
- **Required skill**: Critter Ranching II (CanUseMilkingStation)
- **Room requirement**: Stable / Creature Pen (required)
- **Eligibility**: Critters with MilkProductionMonitor that have the RequiresMilking tag
- **Milking behavior**: Extracts milk at critter's body temperature. Removes RequiresMilking tag. Milk amount equals current MilkProduction amount value scaled by capacity ratio.
- **DLC**: Base game

## Skills Board
- **ID**: RoleStation
- **Name**: Skills Board
- **Description**: A skills board can teach special skills to Duplicants they can't learn on their own.
- **Effect**: Allows Duplicants to spend Skill Points to learn new Skills.
- **Size**: 2x2
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: None (no power input)
- **Manually operated**: Yes
- **Special**: DEPRECATED -- no longer buildable in current versions
- **DLC**: Base game

## Skill Scrubber
- **ID**: ResetSkillsStation
- **Name**: Skill Scrubber
- **Description**: Erase skills from a Duplicant's mind, returning them to their default abilities.
- **Effect**: Refunds a Duplicant's Skill Points for reassignment. Duplicants will lose all assigned skills in the process.
- **Size**: 3x3
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 480 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Work time**: 180 s
- **Manually operated**: Yes
- **Assignable**: Yes (one Duplicant assigned via ResetSkillsStation slot)
- **DLC**: Base game

## Artifact Analysis Station
- **ID**: ArtifactAnalysisStation
- **Name**: Artifact Analysis Station
- **Description**: Discover the mysteries of the past.
- **Effect**: Analyses and extracts Neutronium from artifacts of interest.
- **Size**: 4x4
- **HP**: 30
- **Construction**: 800 kg Any Metal, build time 60 s
- **Decor**: -15 (radius 3)
- **Power**: 480 W
- **Heat**: 0 kDTU/s exhaust, 1 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Work time**: 150 s per analysis
- **Input**: Charmed Artifacts, capacity 25 kg (= 1 * ARTIFACT_MASS of 25 kg), refill at 25 kg
- **Manually operated**: Yes
- **Required skill**: Artifact Analysis (CanStudyArtifact)
- **DLC**: Spaced Out! (EXPANSION1)

## Remote Worker Dock
- **ID**: RemoteWorkerDock
- **Name**: Remote Worker Dock
- **Description**: It's a Duplicant's duplicate's dock.
- **Effect**: Remote Worker Docks deploy automatons that operate machinery based on instructions received from a connected Remote Controller. Must be placed within range of its target building.
- **Size**: 1x2
- **HP**: 100
- **Construction**: 400 kg Plastic, build time 60 s
- **Decor**: -10 (radius 2)
- **Power**: 120 W
- **Heat**: 2 kDTU/s self, 0 kDTU/s exhaust
- **Overheat**: Does not overheat (Overheatable = false)
- **Liquid input**: Lubricating Oil via liquid conduit at offset (0,1), 50 kg capacity
- **Liquid output**: Liquid Gunk via liquid conduit at offset (0,0)
- **Work range**: 12 cells horizontal
- **New worker deploy delay**: 2 s
- **Nameable**: Yes (UserNameable)
- **DLC**: DLC3 (Bionic Booster Pack)

## Remote Controller
- **ID**: RemoteWorkTerminal
- **Name**: Remote Controller
- **Description**: Remote controllers cut down on colony commute times.
- **Effect**: Enables Duplicants to operate machinery remotely via a connected Remote Worker Dock.
- **Size**: 3x3
- **HP**: 30
- **Construction**: 100 kg Raw Metal, build time 60 s
- **Decor**: +15 (radius 3)
- **Power**: 120 W
- **Heat**: 2 kDTU/s self, 0 kDTU/s exhaust
- **Work time**: Infinite (continuous operation)
- **Input**: Data Banks, capacity 10, consumed at 0.01333 kg/s (1/75), refill at 5
- **Storage**: 100 kg
- **DLC**: DLC3 (Bionic Booster Pack)

## Blastshot Maker
- **ID**: MissileFabricator
- **Name**: Blastshot Maker
- **Description**: Blastshot shells are an effective defense against incoming meteor showers.
- **Effect**: Produces Blastshot from Refined Metals combined with Petroleum.
- **Size**: 5x4
- **HP**: 250
- **Construction**: 800 kg Refined Metal, build time 60 s
- **Decor**: -15 (radius 3)
- **Power**: 960 W
- **Heat**: 8 kDTU/s self, 0 kDTU/s exhaust
- **CO2 emission**: 0.0125 kg/s at 313.15 K (40 C)
- **Liquid input**: Liquid conduit at offset (-1,1), 400 kg capacity, stores any liquid
- **Required skill**: Missile Fabrication (CanMakeMissiles)
- **POI unlockable**: Yes (must be unlocked)
- **Manually operated**: Yes
- **Recipes**:
  - **Blastshot (MissileBasic)**: 25 kg Basic Refined Metal + 50 kg Petroleum or Refined Lipid -> 5 kg Blastshot, 80 s
  - **Long Range Missile (MissileLongRange)**: 50 kg Basic Refined Metal + 100 kg Fertilizer or Peat + 200 kg Petroleum or Refined Lipid -> 1 Long Range Missile, 80 s (requires DLC4 + EXPANSION1, or EXPANSION1 only variant)
- **DLC**: Base game (Long Range Missile recipes require Spaced Out!)

## Crafting Station
- **ID**: CraftingTable
- **Name**: Crafting Station
- **Description**: Crafting stations allow Duplicants to make oxygen masks to wear in low breathability areas.
- **Effect**: Produces items and equipment for Duplicant use. Duplicants will not fabricate items unless recipes are queued.
- **Size**: 2x2
- **HP**: 100
- **Construction**: 200 kg Raw Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 60 W
- **Heat**: 0 kDTU/s exhaust, 0 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Output temperature**: 318.15 K (45 C) heated
- **POI unlockable**: Yes
- **Manually operated**: Yes
- **Recipes**:
  - **Disposable Electrobank (Raw Metal)**: 200 kg Basic Metal Ore (Copper Ore, Gold Amalgam, Iron Ore, etc.) -> 1 Electrobank, 60 s (DLC3)
  - **Disposable Electrobank (Uranium Ore)**: 10 kg Uranium Ore -> 1 Electrobank, 60 s (DLC3 + EXPANSION1)
  - **Oxygen Mask**: 50 kg Basic Metal Ore -> 1 Oxygen Mask, 20 s
  - **Repair Worn Oxygen Mask**: 1 Worn Oxygen Mask -> 1 Oxygen Mask, 20 s
- **DLC**: Base game (Electrobank recipes require DLC3)

## Soldering Station
- **ID**: AdvancedCraftingTable
- **Name**: Soldering Station
- **Description**: Soldering stations allow Duplicants to build helpful Flydo retriever bots.
- **Effect**: Produces advanced electronics and bionic Boosters.
- **Size**: 3x3
- **HP**: 100
- **Construction**: 200 kg Raw Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 480 W
- **Heat**: 0 kDTU/s exhaust, 0 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Output temperature**: 318.15 K (45 C) heated
- **Required skill**: Electronics Crafting (CanCraftElectronics)
- **Manually operated**: Yes
- **Recipes**:
  - **Electrobank (rechargeable)**: 200 kg Abyssalite (Katairite) -> 1 Electrobank, 60 s
  - **Flydo (from Plastic)**: 200 kg Plastic (Polypropylene) -> 1 Flydo, 120 s
  - **Flydo (from Hard Plastic)**: 200 kg Hard Plastic (HardPolypropylene) -> 1 Flydo, 120 s
- **DLC**: DLC3 (Bionic Booster Pack)

## Textile Loom
- **ID**: ClothingFabricator
- **Name**: Textile Loom
- **Description**: A textile loom can be used to spin Reed Fiber into wearable Duplicant clothing.
- **Effect**: Tailors Duplicant Clothing items.
- **Size**: 4x3
- **HP**: 100
- **Construction**: 200 kg Refined Metal, build time 240 s
- **Decor**: 0 (radius 1)
- **Power**: 240 W
- **Heat**: 0 kDTU/s exhaust, 0 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Manually operated**: Yes
- **Recipes**:
  - **Warm Sweater (Warm_Vest)**: 4 kg Fabric -> 1 Warm Sweater, 180 s
  - **Snazzy Suit (Funky_Vest)**: 4 kg Fabric -> 1 Snazzy Suit, 180 s
- **DLC**: Base game

## Clothing Refashionator
- **ID**: ClothingAlterationStation
- **Name**: Clothing Refashionator
- **Description**: Allows skilled Duplicants to add extra personal pizzazz to their wardrobe.
- **Effect**: Upgrades Snazzy Suits into Primo Garb.
- **Size**: 4x3
- **HP**: 100
- **Construction**: 200 kg Refined Metal, build time 240 s
- **Decor**: 0 (radius 1)
- **Power**: 240 W
- **Heat**: 0 kDTU/s exhaust, 0 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Required skill**: Clothing Alteration (CanClothingAlteration)
- **Manually operated**: Yes
- **Recipes**:
  - **Custom Clothing (Primo Garb)**: 1 Snazzy Suit + 3 kg Fabric -> 1 Custom Clothing, 180 s (one recipe per facade/appearance variant)
- **Skill experience**: Art group
- **DLC**: Base game

## Exosuit Forge
- **ID**: SuitFabricator
- **Name**: Exosuit Forge
- **Description**: Exosuits can be filled with oxygen to allow Duplicants to safely enter hazardous areas.
- **Effect**: Forges protective Exosuits for Duplicants to wear.
- **Size**: 4x3
- **HP**: 100
- **Construction**: 400 kg Refined Metal, build time 240 s
- **Decor**: 0 (radius 1)
- **Power**: 480 W
- **Heat**: 0 kDTU/s exhaust, 0 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Output temperature**: 318.15 K (45 C) heated
- **Manually operated**: Yes
- **Recipes**:
  - **Atmo Suit**: 300 kg Basic Refined Metal + 2 kg Fabric -> 1 Atmo Suit, 40 s
  - **Repair Worn Atmo Suit**: 1 Worn Atmo Suit + 1 kg Fabric -> 1 Atmo Suit, 40 s
  - **Jet Suit**: 200 kg Steel + 2 kg Fabric -> 1 Jet Suit, 40 s
  - **Repair Worn Jet Suit**: 1 Worn Jet Suit + 1 kg Fabric -> 1 Jet Suit, 40 s
  - **Lead Suit**: 200 kg Lead + 10 kg Glass -> 1 Lead Suit, 40 s (requires radiation enabled)
  - **Repair Worn Lead Suit**: 1 Worn Lead Suit + 5 kg Glass -> 1 Lead Suit, 40 s (requires radiation enabled)
- **DLC**: Base game (Lead Suit recipes require Spaced Out! with radiation enabled)

## Oxygen Mask Checkpoint
- **ID**: OxygenMaskMarker
- **Name**: Oxygen Mask Checkpoint
- **Description**: A checkpoint must have a correlating dock built on the opposite side its arrow faces.
- **Effect**: Marks a threshold where Duplicants must put on or take off an Oxygen Mask. Must be built next to an Oxygen Mask Dock. Can be rotated.
- **Size**: 1x2
- **HP**: 30
- **Construction**: 100 kg Raw Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: None
- **Flippable**: Yes (FlipH)
- **Logic input**: Automation port at (0,0)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with OxygenMaskLocker. Blocks Jet Suit path flag.
- **Path flag**: HasOxygenMask
- **DLC**: Base game

## Oxygen Mask Dock
- **ID**: OxygenMaskLocker
- **Name**: Oxygen Mask Dock
- **Description**: An oxygen mask dock will store and refill masks while they're not in use.
- **Effect**: Stores Oxygen Masks and refuels them with Oxygen. Build next to an Oxygen Mask Checkpoint.
- **Size**: 1x2
- **HP**: 30
- **Construction**: 100 kg Raw Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: None
- **Gas input**: Oxygen via gas conduit, 1 kg/s consumption rate, 30 kg O2 capacity
- **Stores**: Oxygen Masks (OxygenMask tag)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with OxygenMaskMarker.
- **DLC**: Base game

## Atmo Suit Checkpoint
- **ID**: SuitMarker
- **Name**: Atmo Suit Checkpoint
- **Description**: A checkpoint must have a correlating dock built on the opposite side its arrow faces.
- **Effect**: Marks a threshold where Duplicants must change into or out of Atmo Suits. Must be built next to an Atmo Suit Dock. Can be rotated.
- **Size**: 1x3
- **HP**: 30
- **Construction**: 100 kg Refined Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: None
- **Flippable**: Yes (FlipH)
- **Logic input**: Automation port at (0,0)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with SuitLocker. Blocks Jet Suit path flag.
- **Path flag**: HasAtmoSuit
- **DLC**: Base game

## Atmo Suit Dock
- **ID**: SuitLocker
- **Name**: Atmo Suit Dock
- **Description**: An atmo suit dock will empty atmo suits of waste, but only one suit can charge at a time.
- **Effect**: Stores Atmo Suits and refuels them with Oxygen. Empties suits of Polluted Water. Build next to an Atmo Suit Checkpoint.
- **Size**: 1x3
- **HP**: 30
- **Construction**: 100 kg Refined Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: 120 W
- **Gas input**: Oxygen via gas conduit, 1 kg/s consumption rate, 200 kg O2 capacity
- **Stores**: Atmo Suits (AtmoSuit tag)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with SuitMarker.
- **DLC**: Base game

## Jet Suit Checkpoint
- **ID**: JetSuitMarker
- **Name**: Jet Suit Checkpoint
- **Description**: A checkpoint must have a correlating dock built on the opposite side its arrow faces.
- **Effect**: Marks a threshold where Duplicants must change into or out of Jet Suits. Must be built next to a Jet Suit Dock. Can be rotated.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 200 kg Refined Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: None
- **Flippable**: Yes (FlipH)
- **Logic input**: Automation port at (0,0)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with JetSuitLocker. Blocks Jet Suit path flag.
- **Path flag**: HasJetPack
- **DLC**: Base game

## Jet Suit Dock
- **ID**: JetSuitLocker
- **Name**: Jet Suit Dock
- **Description**: Jet suit docks can refill jet suits with air and fuel, or empty them of waste.
- **Effect**: Stores Jet Suits and refuels them with Oxygen and Combustible Liquid. Empties suits of Polluted Water. Build next to a Jet Suit Checkpoint.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 200 kg Refined Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: 120 W
- **Gas input**: Oxygen via gas conduit, 1 kg/s consumption rate, 200 kg O2 capacity
- **Liquid input**: Secondary liquid conduit input at offset (0,1) for fuel
- **Storage**: 500 kg
- **Stores**: Jet Suits (JetSuit tag)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with JetSuitMarker.
- **DLC**: Base game

## Lead Suit Checkpoint
- **ID**: LeadSuitMarker
- **Name**: Lead Suit Checkpoint
- **Description**: A checkpoint must have a correlating dock built on the opposite side its arrow faces.
- **Effect**: Marks a threshold where Duplicants must change into or out of Lead Suits. Must be built next to a Lead Suit Dock. Can be rotated.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 100 kg Refined Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: None
- **Flippable**: Yes (FlipH)
- **Logic input**: Automation port at (0,0)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with LeadSuitLocker. Blocks Jet Suit path flag.
- **Path flag**: HasLeadSuit
- **Special**: Deprecated if radiation is disabled
- **DLC**: Spaced Out! (EXPANSION1)

## Lead Suit Dock
- **ID**: LeadSuitLocker
- **Name**: Lead Suit Dock
- **Description**: Lead suit docks can refill lead suits with air and empty them of waste.
- **Effect**: Stores Lead Suits and refuels them with Oxygen. Empties suits of Polluted Water. Build next to a Lead Suit Checkpoint.
- **Size**: 2x4
- **HP**: 30
- **Construction**: 100 kg Refined Metal, build time 30 s
- **Decor**: +10 (radius 2)
- **Power**: 120 W
- **Gas input**: Oxygen via gas conduit at offset (0,2), 1 kg/s consumption rate, 80 kg O2 capacity
- **Stores**: Lead Suits (LeadSuit tag)
- **Checkpoint behavior**: Prevents idle traversal. Pairs with LeadSuitMarker.
- **Special**: Deprecated if radiation is disabled
- **DLC**: Spaced Out! (EXPANSION1)

## Space Cadet Centrifuge
- **ID**: AstronautTrainingCenter
- **Name**: Space Cadet Centrifuge
- **Description**: Duplicants must complete astronaut training in order to pilot space rockets.
- **Effect**: Trains Duplicants to become Astronaut. Duplicants must possess the Astronaut trait.
- **Size**: 5x5
- **HP**: 30
- **Construction**: 400 kg Any Metal, build time 30 s
- **Decor**: 0 (radius 1)
- **Power**: 480 W
- **Heat**: 0.5 kDTU/s exhaust, 4 kDTU/s self
- **Overheat**: 348.15 K (75 C)
- **Work time**: Infinite (continuous training)
- **Training duration**: 10 days to master role
- **Manually operated**: Yes
- **Required skill**: Astronaut (CanTrainToBeAstronaut)
- **Special**: DEPRECATED -- no longer buildable in current versions
- **DLC**: Base game
