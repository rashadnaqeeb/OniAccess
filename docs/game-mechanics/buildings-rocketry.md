# Rocketry

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code.

Many rocketry buildings exist in two variants: a base-game version (forbidden in Spaced Out) and a Spaced Out (DLC1) version. Where both exist, they are documented separately. Base-game rocket modules use the classic stack-and-launch system. Spaced Out modules use the cluster map system with burden/engine-power/fuel-cost mechanics.

## Telescope (ClusterTelescope)

- **DLC**: Spaced Out only
- **Name**: Telescope
- **Size**: 3x3
- **Materials**: 400 kg Raw Metals
- **HP**: 30, **Decor**: none
- **Power**: 120 W
- **Heat**: 0.125 kDTU/s exhaust, 0 kDTU/s self
- **Manually operated**: yes
- **Required skill**: CanUseClusterTelescope (Astronomy)
- **Sky visibility**: scan radius 4, vertical offset 1
- **Storage**: 1000 kg (Data Banks)
- **Effect**: Reveals visitable Planetoids in space, producing Data Banks. Must be exposed to space.

## Enclosed Telescope (ClusterTelescopeEnclosed)

- **DLC**: Spaced Out only
- **Name**: Enclosed Telescope
- **Size**: 4x6
- **Materials**: 400 kg Any Metal
- **HP**: 30, **Decor**: none
- **Power**: 120 W
- **Heat**: 0.125 kDTU/s exhaust, 0 kDTU/s self
- **Manually operated**: yes
- **Required skill**: CanUseClusterTelescopeEnclosed (Astronomy)
- **Gas input**: Oxygen, 1 kg/s consumption rate, 10 kg buffer
- **Sky visibility**: scan radius 4, vertical offset 3
- **Storage**: 1000 kg (Data Banks)
- **Provides oxygen to operator**: yes
- **Analyze cluster radius**: 4
- **Effect**: Reveals visitable Planetoids with Oxygen-supplied comfort, sunburn and partial radiation protection. Must be exposed to space.

## Mission Control Station (MissionControl) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Mission Control Station
- **Size**: 3x3
- **Materials**: 400 kg Refined Metals
- **HP**: 100, **Decor**: -5/1 tile
- **Power**: 960 W
- **Heat**: 0.5 kDTU/s exhaust, 2 kDTU/s self
- **Manually operated**: yes
- **Required skill**: CanMissionControl (Astronomy)
- **Sky visibility**: scan radius 1, vertical offset 2
- **Room requirement**: Laboratory
- **Speed multiplier**: 1.2x rocket speed
- **Effect duration**: 600 s
- **Effect**: Provides guidance data to rocket pilots to improve rocket speed. Requires sky visibility.

## Mission Control Station (MissionControlCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Mission Control Station
- **Size**: 3x3
- **Materials**: 400 kg Refined Metals
- **HP**: 100, **Decor**: -5/1 tile
- **Power**: 960 W
- **Heat**: 0.5 kDTU/s exhaust, 2 kDTU/s self
- **Manually operated**: yes
- **Required skill**: CanMissionControl (Astronomy)
- **Sky visibility**: scan radius 1, vertical offset 2
- **Work range radius**: 2 (cluster hexes)
- **Room requirement**: Laboratory
- **Speed multiplier**: 1.2x rocket speed
- **Effect duration**: 600 s
- **Effect**: Provides guidance data to rocket pilots within range to improve rocket speed. Requires sky visibility.

## Rocket Platform (LaunchPad)

- **DLC**: Spaced Out only
- **Name**: Rocket Platform
- **Size**: 7x2
- **Materials**: 800 kg Refined Metals
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Not floodable**, **No structure temperature**
- **Logic input**: TriggerLaunch (launch rocket)
- **Logic outputs**: LaunchReady (checklist complete), LandedRocket (rocket present)
- **Provides fake floor**: 7 tiles wide at y+1
- **Chains to**: ModularLaunchpadPort buildings
- **Nameable**: yes
- **Effect**: Precursor to construction of all other rocket modules. Allows launch and landing. Links to Rocket Port Loaders/Unloaders.

## Gantry

- **DLC**: both (no DLC restriction)
- **Name**: Gantry
- **Size**: 6x2
- **Materials**: 200 kg Steel
- **HP**: 30, **Decor**: none
- **Power**: 1200 W
- **Heat**: 1 kDTU/s exhaust, 1 kDTU/s self
- **Overheat**: 2273.15 K (2000 C)
- **Flippable**: horizontally
- **Logic input**: Extend/Retract (Green = extend, Red = retract)
- **Fake floor**: 4 tiles when extended
- **Solid cells**: 2 cells at offsets (-2,1) and (-1,1)
- **Effect**: Provides scaffolding across rocket modules for Duplicant access. Retracts for launch.

## Steam Engine (SteamEngine) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Steam Engine
- **Size**: 7x5
- **Materials**: 2000 kg Steel
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Gas input**: Steam conduit at offset (2,3)
- **Fuel**: Steam, capacity 900 kg (integrated tank, FUEL_TANK_WET_MASS)
- **Engine efficiency**: 20 (WEAK)
- **Requires oxidizer**: no
- **Exhaust**: Steam at 423.15 K (150 C)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Utilizes Steam to propel rockets for space exploration. Placed at bottom of stack.

## Steam Engine (SteamEngineCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Steam Engine
- **Size**: 7x5
- **Materials**: 200 kg Refined Metals (DENSE_TIER0)
- **HP**: 1000, **Decor**: none
- **Power generation**: 600 W (to rocket interior)
- **Overheat**: 2273.15 K (2000 C)
- **Gas input**: Steam conduit at offset (2,3)
- **Fuel**: Steam, capacity 150 kg (FUEL_TANK_WET_MASS_GAS_LARGE)
- **Engine efficiency**: 20 (WEAK)
- **Requires oxidizer**: no
- **Exhaust**: Steam at 423.15 K (150 C)
- **Max modules**: 6
- **Max height**: 25 tiles (TALL)
- **Burden**: 15 (MONUMENTAL)
- **Engine power**: 27 (MID_WEAK)
- **Fuel cost per hex**: 0.025 (GAS_VERY_LOW)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Utilizes Steam to propel rockets. Must be built via Rocket Platform.

## Petroleum Engine (KeroseneEngine) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Petroleum Engine
- **Size**: 7x5
- **Materials**: 200 kg Steel (ENGINE_MASS_SMALL)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: Petroleum (no integrated tank; uses separate LiquidFuelTank)
- **Engine efficiency**: 40 (MEDIUM)
- **Requires oxidizer**: yes (default)
- **Exhaust**: CO2 (default element)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Burns Petroleum to propel rockets for mid-range space exploration. Placed at bottom of stack.

## Petroleum Engine (KeroseneEngineCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Petroleum Engine
- **Size**: 7x5
- **Materials**: 200 kg Steel (ENGINE_MASS_SMALL)
- **HP**: 1000, **Decor**: none
- **Power generation**: 480 W (to rocket interior)
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: Petroleum (no integrated tank)
- **Engine efficiency**: 40 (MEDIUM)
- **Requires oxidizer**: yes
- **Exhaust**: CO2 at 1263.15 K (990 C)
- **Max modules**: 7
- **Max height**: 35 tiles (VERY_TALL)
- **Burden**: 6 (MAJOR)
- **Engine power**: 48 (MID_VERY_STRONG)
- **Fuel cost per hex**: 0.15 (VERY_HIGH)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Burns Petroleum for mid-range exploration. Generous height restrictions. Requires oxidizer tank.

## Small Petroleum Engine (KeroseneEngineClusterSmall) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Small Petroleum Engine
- **Size**: 3x4
- **Materials**: 200 kg Refined Metals (ENGINE_MASS_SMALL)
- **HP**: 1000, **Decor**: none
- **Power generation**: 240 W (to rocket interior)
- **Overheat**: 2273.15 K (2000 C)
- **Liquid input**: Petroleum conduit at offset (0,2)
- **Fuel**: Petroleum, integrated tank 450 kg
- **Engine efficiency**: 40 (MEDIUM)
- **Requires oxidizer**: yes
- **Exhaust**: CO2 at 1263.15 K (990 C)
- **Max modules**: 4
- **Max height**: 20 tiles (MEDIUM)
- **Burden**: 5 (MODERATE_PLUS)
- **Engine power**: 31 (MID_STRONG)
- **Fuel cost per hex**: 0.075 (MEDIUM)
- **Module height**: 4 (attach point at y+4)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Same speed as Petroleum Engine but with smaller height restrictions.

## Biodiesel Engine (BiodieselEngine) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Biodiesel Engine
- **Size**: 7x5
- **Materials**: 200 kg Steel (ENGINE_MASS_SMALL)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: Refined Lipid (no integrated tank)
- **Engine efficiency**: 50 (MEDIUM_PLUS)
- **Requires oxidizer**: yes (default)
- **Exhaust**: CO2 at 1700 K (1426.85 C), 100 kg/s emit rate (default 50)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Burns Refined Lipid for mid-range exploration. Generous height restrictions.

## Biodiesel Engine (BiodieselEngineCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Biodiesel Engine
- **Size**: 7x5
- **Materials**: 200 kg Steel (ENGINE_MASS_SMALL)
- **HP**: 1000, **Decor**: none
- **Power generation**: 640 W (to rocket interior)
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: Refined Lipid (no integrated tank)
- **Engine efficiency**: 60 (STRONG)
- **Requires oxidizer**: yes
- **Exhaust**: CO2 at 1263.15 K (990 C)
- **Max modules**: 7
- **Max height**: 35 tiles (VERY_TALL)
- **Burden**: 7 (MAJOR_PLUS)
- **Engine power**: 31 (MID_STRONG)
- **Fuel cost per hex**: ~0.1154 (HIGHER = 3/26)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Burns Refined Lipid for mid-range exploration. Generous height restrictions. Requires oxidizer tank.

## Hydrogen Engine (HydrogenEngine) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Hydrogen Engine
- **Size**: 7x5
- **Materials**: 500 kg Steel (ENGINE_MASS_LARGE)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: Liquid Hydrogen (no integrated tank)
- **Engine efficiency**: 60 (STRONG)
- **Requires oxidizer**: yes (default)
- **Exhaust**: Steam at 2000 K (1726.85 C)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Burns Liquid Hydrogen for long-range space exploration. Same height restrictions as Petroleum Engine but slightly faster.

## Hydrogen Engine (HydrogenEngineCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Hydrogen Engine
- **Size**: 7x5
- **Materials**: 500 kg Steel (ENGINE_MASS_LARGE)
- **HP**: 1000, **Decor**: none
- **Power generation**: 600 W (to rocket interior)
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: Liquid Hydrogen (no integrated tank)
- **Engine efficiency**: 60 (STRONG)
- **Requires oxidizer**: yes (implicit, not explicitly set but default)
- **Exhaust**: Steam at 2000 K (1726.85 C)
- **Max modules**: 7
- **Max height**: 35 tiles (VERY_TALL)
- **Burden**: 7 (MAJOR_PLUS)
- **Engine power**: 55 (LATE_VERY_STRONG)
- **Fuel cost per hex**: ~0.09375 (HIGH = 3/32)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Burns Liquid Hydrogen for long-range exploration. Same generous height restrictions as Petroleum Engine but slightly faster.

## Solid Fuel Thruster (SolidBooster) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Solid Fuel Thruster
- **Size**: 7x5
- **Materials**: 200 kg Steel (ENGINE_MASS_SMALL)
- **HP**: 1000 (invincible), **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Fuel**: 400 kg Iron + 400 kg Oxylite (total storage 800 kg)
- **Engine efficiency**: 30 (BOOSTER)
- **Main engine**: no (supplementary booster only)
- **Module height**: 5 (attach point at y+5, placed via BuildingAttachPoint)
- **Effect**: Burns Iron and Oxylite to increase rocket exploration distance. Not a main engine.

## Liquid Fuel Tank (LiquidFuelTank) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Liquid Fuel Tank
- **Size**: 5x5
- **Materials**: 100 kg Steel (FUEL_TANK_DRY_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Liquid input**: conduit at offset (2,3)
- **Fuel capacity**: 900 kg (FUEL_TANK_WET_MASS)
- **Accepts**: any liquid fuel (determined by engine type)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores liquid fuel piped into it to supply rocket engines.

## Large Liquid Fuel Tank (LiquidFuelTankCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Large Liquid Fuel Tank
- **Size**: 5x5
- **Materials**: 100 kg Steel (FUEL_TANK_DRY_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Liquid input**: conduit at offset (2,3)
- **Fuel capacity**: 900 kg (FUEL_TANK_WET_MASS)
- **Accepts**: any liquid fuel (determined by engine type)
- **Burden**: 5 (MODERATE_PLUS)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **On spawn**: discovers Liquid Oxygen
- **Effect**: Stores liquid fuel piped into it to supply rocket engines. Must be built via Rocket Platform.

## Solid Oxidizer Tank (OxidizerTank) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Solid Oxidizer Tank
- **Size**: 5x5
- **Materials**: 100 kg Steel (FUEL_TANK_DRY_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Oxidizer capacity**: 2700 kg
- **Accepted oxidizers**: Oxylite
- **Supports multiple oxidizers**: yes
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores Oxylite and other oxidizers for burning rocket fuels.

## Large Solid Oxidizer Tank (OxidizerTankCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Large Solid Oxidizer Tank
- **Size**: 5x5
- **Materials**: 100 kg Steel (FUEL_TANK_DRY_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Oxidizer capacity**: 900 kg
- **Accepted oxidizers**: Oxylite, Fertilizer
- **Supports multiple oxidizers**: yes
- **Burden**: 5 (MODERATE_PLUS)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Stores Oxylite and other oxidizers for burning rocket fuels. Must be built via Rocket Platform.

## Liquid Oxidizer Tank (OxidizerTankLiquid) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Liquid Oxidizer Tank
- **Size**: 5x5
- **Materials**: 100 kg Steel (FUEL_TANK_DRY_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Liquid input**: conduit at offset (2,3), Liquid Oxygen only
- **Oxidizer capacity**: 2700 kg
- **Supports multiple oxidizers**: no (Liquid Oxygen only)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores Liquid Oxygen for burning rocket fuels.

## Liquid Oxidizer Tank (OxidizerTankLiquidCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Liquid Oxidizer Tank
- **Size**: 5x2
- **Materials**: 100 kg Steel (FUEL_TANK_DRY_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Liquid input**: conduit at offset (1,1), Liquid Oxygen only
- **Oxidizer capacity**: 450 kg
- **Supports multiple oxidizers**: no (Liquid Oxygen only)
- **Burden**: 5 (MODERATE_PLUS)
- **Module height**: 2 (attach point at y+2)
- **Not shown in build menu** (built via Rocket Platform)
- **On spawn**: discovers Liquid Oxygen
- **Effect**: Stores Liquid Oxygen for burning rocket fuels. Must be built via Rocket Platform.

## Cargo Bay (CargoBay) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Cargo Bay
- **Size**: 5x5
- **Materials**: 1000 kg Buildable Raw + 1000 kg Steel
- **HP**: 1000 (invincible), **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 1000 kg solids
- **Solid conduit output**: at offset (0,3)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores Solid Materials found during space missions.

## Large Cargo Bay (CargoBayCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Large Cargo Bay
- **Size**: 5x5
- **Materials**: 1000 kg Steel (DENSE_TIER2)
- **HP**: 1000 (invincible), **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 27000 kg solids (2700 * 10 CARGO_CAPACITY_SCALE)
- **Burden**: 6 (MAJOR)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Stores Solid Materials found during space missions. Must be built via Rocket Platform.

## Gas Cargo Canister (GasCargoBay) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Gas Cargo Canister
- **Size**: 5x5
- **Materials**: 1000 kg Steel (CARGO_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 1000 kg gases
- **Gas conduit output**: at offset (0,3)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores Gas resources found during space missions.

## Large Gas Cargo Canister (GasCargoBayCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Large Gas Cargo Canister
- **Size**: 5x5
- **Materials**: 1000 kg Steel (DENSE_TIER2)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 11000 kg gases (1100 * 10 CARGO_CAPACITY_SCALE)
- **Burden**: 4 (MODERATE)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Stores Gas resources found during space missions. Must be built via Rocket Platform.

## Liquid Cargo Tank (LiquidCargoBay) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Liquid Cargo Tank
- **Size**: 5x5
- **Materials**: 1000 kg Steel (CARGO_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 1000 kg liquids
- **Liquid conduit output**: at offset (0,3)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores Liquid resources found during space missions.

## Large Liquid Cargo Tank (LiquidCargoBayCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Large Liquid Cargo Tank
- **Size**: 5x5
- **Materials**: 1000 kg Steel (DENSE_TIER2)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 27000 kg liquids (2700 * 10 CARGO_CAPACITY_SCALE)
- **Burden**: 5 (MODERATE_PLUS)
- **Module height**: 5 (attach point at y+5)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Stores Liquid resources found during space missions. Must be built via Rocket Platform.

## Command Capsule (CommandModule) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Command Capsule
- **Size**: 5x5
- **Materials**: 200 kg Steel (COMMAND_MODULE_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Required skill**: CanUseRockets (Astronauting)
- **Logic input**: TriggerLaunch (launch rocket)
- **Logic output**: LaunchReady (checklist complete)
- **Holds**: 1 Duplicant (MinionStorage), assignable
- **Finds artifacts**: yes
- **Module height**: 5 (attach point at y+5)
- **Effect**: Contains passenger seating for Duplicant Astronauts. Must be last module at top of rocket.

## Robo-Pilot Capsule (RoboPilotCommandModule) -- base game + DLC3

- **DLC**: requires DLC3 (Biome Upgrade), forbidden in Spaced Out
- **Name**: Robo-Pilot Capsule
- **Size**: 5x5
- **Materials**: 200 kg Steel (COMMAND_MODULE_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Logic input**: TriggerLaunch (launch rocket)
- **Logic output**: LaunchReady (checklist complete)
- **Data Bank consumption**: 1 per landing
- **Data Bank storage**: 100 kg capacity, refill at 20 kg
- **Data Bank range**: 5000 (10000 / 2)
- **No Duplicant pilot required**
- **Module height**: 5 (attach point at y+5)
- **Effect**: Enables rockets to travel without a Command Capsule. Must be last module at top of rocket.

## Robo-Pilot Module (RoboPilotModule) -- Spaced Out + DLC3

- **DLC**: requires both Spaced Out AND DLC3
- **Name**: Robo-Pilot Module
- **Size**: 3x4
- **Materials**: 200 kg Steel (HOLLOW_TIER1)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Data Bank storage**: 100 kg capacity, refill at 20 kg
- **Burden**: 4 (MODERATE)
- **Module height**: 4 (attach point at y+4)
- **No Duplicant pilot required**
- **One per rocket**: enforced by LimitOneRoboPilotModule
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Enables rockets to travel without a Rocket Control Station. Must be built via Rocket Platform.

## Sight-Seeing Module (TouristModule) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Sight-Seeing Module
- **Size**: 5x5
- **Materials**: 200 kg Steel (COMMAND_MODULE_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Holds**: 1 non-Astronaut Duplicant (MinionStorage), assignable
- **Finds artifacts**: yes
- **Module height**: 5 (attach point at y+5)
- **Effect**: Allows one non-Astronaut Duplicant to visit space. Decreases Stress.

## Research Module (ResearchModule) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Research Module
- **Size**: 5x5
- **Materials**: 200 kg Steel (COMMAND_MODULE_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Module height**: 5 (attach point at y+5)
- **Effect**: Completes one Research Task per space mission. Produces a small Data Bank regardless of destination.

## Research Module (ResearchClusterModule) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Research Module
- **Size**: 3x2
- **Materials**: 1000 kg Steel (DENSE_TIER2)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Storage**: 50 kg Data Banks
- **Collect speed**: 1/12 per second
- **Burden**: 3 (MINOR_PLUS)
- **Module height**: 2 (attach point at y+2)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Allows the rocket to gather Data Banks floating in space while travelling through the galaxy.

## Biological Cargo Bay (SpecialCargoBay) -- base game

- **DLC**: base game only (forbidden in Spaced Out)
- **Name**: Biological Cargo Bay
- **Size**: 5x5
- **Materials**: 1000 kg Steel (CARGO_MASS)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Cargo capacity**: 100 kg entities
- **Module height**: 5 (attach point at y+5)
- **Effect**: Stores unusual or organic resources found during space missions.

## Critter Cargo Bay (SpecialCargoBayCluster) -- Spaced Out

- **DLC**: Spaced Out only
- **Name**: Critter Cargo Bay
- **Size**: 3x1
- **Materials**: 200 kg Refined Metals (HOLLOW_TIER1)
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Accepts**: BagableCreature (deliverable critters)
- **Two storages**: one for critters (insulated), one for loot (sealed)
- **Burden**: 1 (INSIGNIFICANT)
- **Module height**: 1 (attach point at y+1)
- **Not shown in build menu** (built via Rocket Platform)
- **Effect**: Allows Duplicants to transport Critters through space. Critters do not require feeding during transit.

## Rocket Control Station (RocketControlStation)

- **DLC**: Spaced Out only
- **Name**: Rocket Control Station
- **Size**: 2x2
- **Materials**: 100 kg Raw Metals
- **HP**: 30, **Decor**: +15/3 tiles
- **Power**: none (not overheatable, not repairable, not floodable)
- **Manually operated**: yes
- **Required skill**: CanUseRocketControlStation (Rocket Piloting)
- **One per world**: yes (UniquePerWorld)
- **Logic input**: Restrict Building Usage (Green = restrict, Red = unrestrict)
- **Console work time**: 30 s
- **Console idle time**: 120 s
- **Warning cooldown**: 30 s
- **Speed modifiers**: 0.5x (unpiloted slow), 1.0x (default), 1.5x (piloted super speed)
- **Interior building**: yes (placed inside rocket)
- **Effect**: Allows Duplicants to pilot rockets and control access to interior buildings. Assigned Duplicant needs Rocket Piloting skill.

## Power Outlet Fitting (RocketInteriorPowerPlug)

- **DLC**: Spaced Out only
- **Name**: Power Outlet Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Rotatable**: 360 degrees
- **Power output**: yes (to connected buildings)
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Provides Power to connected buildings. Pulls power from Battery Modules and Rocket Engines.

## Liquid Intake Fitting (RocketInteriorLiquidInput)

- **DLC**: Spaced Out only
- **Name**: Liquid Intake Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Rotatable**: 360 degrees
- **Liquid conduit input**: yes
- **Buffer storage**: 10 kg
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Pumps Liquids into rocket storage via Pipes. Sends to first module with space.

## Liquid Output Fitting (RocketInteriorLiquidOutput)

- **DLC**: Spaced Out only
- **Name**: Liquid Output Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Power**: 60 W
- **Heat**: 0 kDTU/s exhaust, 0.5 kDTU/s self
- **Rotatable**: 360 degrees
- **Liquid conduit output**: yes
- **Buffer storage**: 10 kg
- **Filterable**: by liquid element
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Draws Liquids from rocket storage via Pipes. Draws from first module with requested material.

## Gas Intake Fitting (RocketInteriorGasInput)

- **DLC**: Spaced Out only
- **Name**: Gas Intake Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Rotatable**: 360 degrees
- **Gas conduit input**: yes
- **Buffer storage**: 1 kg
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Pumps Gases into rocket storage via Pipes. Sends to first module with space.

## Gas Output Fitting (RocketInteriorGasOutput)

- **DLC**: Spaced Out only
- **Name**: Gas Output Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Power**: 60 W
- **Heat**: 0 kDTU/s exhaust, 0.5 kDTU/s self
- **Rotatable**: 360 degrees
- **Gas conduit output**: yes
- **Buffer storage**: 1 kg
- **Filterable**: by gas element
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Draws Gases from rocket storage via Pipes. Draws from first module with requested material.

## Conveyor Receptacle Fitting (RocketInteriorSolidInput)

- **DLC**: Spaced Out only
- **Name**: Conveyor Receptacle Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Rotatable**: 360 degrees
- **Solid conduit input**: yes
- **Buffer storage**: 20 kg
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Moves Solid Materials into rocket storage via Conveyor Rails. Sends to first module with space.

## Conveyor Loader Fitting (RocketInteriorSolidOutput)

- **DLC**: Spaced Out only
- **Name**: Conveyor Loader Fitting
- **Size**: 1x1
- **Materials**: 25 kg Any Metal
- **HP**: 30, **Decor**: -15/3 tiles
- **Power**: 60 W
- **Heat**: 0 kDTU/s exhaust, 0.5 kDTU/s self
- **Rotatable**: 360 degrees
- **Solid conduit output**: yes
- **Buffer storage**: 20 kg
- **Filterable**: by solid element
- **Interior building**: yes (placed on rocket envelope)
- **Effect**: Moves Solid Materials out of rocket storage via Conveyor Rails. Draws from first module with requested material.

## Starmap Location Sensor (LogicClusterLocationSensor)

- **DLC**: Spaced Out only
- **Name**: Starmap Location Sensor
- **Size**: 1x1
- **Materials**: 25 kg Refined Metals
- **HP**: 30, **Decor**: -5/1 tile
- **Power**: none (always operational)
- **Logic output**: Green at chosen starmap locations, Red everywhere else
- **Interior building**: yes (placed inside rocket)
- **Effect**: Sends Green Signals at chosen Starmap locations and Red Signals everywhere else.

## Interplanetary Launcher (RailGun)

- **DLC**: Spaced Out only
- **Name**: Interplanetary Launcher
- **Size**: 5x6
- **Materials**: 400 kg Any Metal
- **HP**: 250, **Decor**: none
- **Power**: 240 W
- **Heat**: 0.5 kDTU/s exhaust, 2 kDTU/s self
- **Radbolt input**: at offset (-2,1), capacity 200 radbolts
- **Radbolt cost**: 10 per hex distance (0 base cost)
- **Range**: 20 hex cells
- **Takeoff velocity**: 35
- **Payload storage**: 1200 kg (solids, gases, liquids, food)
- **Maintenance**: required after 6 payloads, 30 s cooldown
- **Logic input**: Launch Toggle (Green = enable, Red = disable)
- **Logic output**: HEP_STORAGE (Green when radbolt storage full)
- **Conduit inputs**: solid at (-1,0), liquid at (0,0), gas at (1,0)
- **Requires sky visibility**: yes (all cells above must be clear)
- **Destination selector**: assignable, requires asteroid destination
- **Effect**: Launches Interplanetary Payloads between Planetoids. Can contain Solid, Liquid, or Gas materials. Cannot transport Duplicants.

## Payload Opener (RailGunPayloadOpener)

- **DLC**: Spaced Out only
- **Name**: Payload Opener
- **Size**: 3x3
- **Materials**: 100 kg Any Metal
- **HP**: 250, **Decor**: none
- **Power**: 120 W
- **Heat**: 0.5 kDTU/s self
- **Payload storage**: 2000 kg (accepts RailGunPayloadEmptyable)
- **Resource storage**: 20000 kg (solids, gases, liquids after unpacking)
- **Conduit outputs**: liquid at (0,0), gas at (1,0), solid at (-1,0)
- **Manual delivery**: refill at 200 kg, capacity 2000 kg
- **Drop work time**: 90 s
- **Effect**: Unpacks Interplanetary Payloads. Automatically separates Solid, Liquid, and Gas materials and distributes to appropriate systems.

## Targeting Beacon (LandingBeacon)

- **DLC**: Spaced Out only
- **Name**: Targeting Beacon
- **Size**: 1x3
- **Materials**: 100 kg Refined Metals
- **HP**: 1000, **Decor**: -10/2 tiles
- **Power**: 60 W
- **Overheat**: 398.15 K (125 C)
- **Landing accuracy**: 3 tiles
- **Requires sky visibility**: yes (all cells above must be clear)
- **Effect**: Guides Interplanetary Payloads and Orbital Cargo Modules to land nearby.

## Meteor Blaster (MissileLauncher)

- **DLC**: both (no DLC restriction, but DLC1-specific cluster targeting added if Spaced Out active)
- **Name**: Meteor Blaster
- **Size**: 3x5
- **Materials**: 400 kg Any Metal
- **HP**: 250, **Decor**: none
- **Power**: 240 W
- **Heat**: 0.5 kDTU/s exhaust, 2 kDTU/s self
- **Solid conduit input**: at offset (0,0)
- **Ammo storage (Blastshot)**: 300 kg, manual delivery refill at 50 kg
- **Ammo storage (Long Range Missile)**: 1000 kg, manual delivery refill at 1000 kg, min 200 kg
- **Conduit storage**: 200 kg (hidden, for conveyor belt input)
- **Launch range**: 16 tiles horizontal, 32 tiles vertical (from offset (0,4))
- **Launch speed**: 30
- **Rotation speed**: 100
- **Scanning angle**: 50 degrees
- **POI unlockable**: yes (blueprint found in space POIs)
- **Cluster targeting**: when Spaced Out active, can target meteors on starmap
- **Effect**: Fires explosive projectiles at incoming space objects. Projectiles must be crafted at a Blastshot Maker. Range: 16 tiles horizontal, 32 tiles vertical.

## Rocket Port Extension (ModularLaunchpadPortBridge)

- **DLC**: Spaced Out only
- **Name**: Rocket Port Extension
- **Size**: 1x2
- **Materials**: 200 kg Refined Metals
- **HP**: 1000, **Decor**: none
- **Power**: none
- **Overheat**: 2273.15 K (2000 C)
- **Not floodable, not entombable**
- **No structure temperature**
- **Chains to**: LaunchPad (head) and ModularLaunchpadPort buildings (links)
- **Provides fake floor**: 1 tile at y+1
- **Effect**: Links to Rocket Platform or other Rocket Port buildings. Allows rocket platforms to be built farther apart.

## Oxidizer efficiency reference (base game)

- Oxylite (OxyRock): VERY_LOW = 0.334
- Fertilizer: LOW = 1.0
- Liquid Oxygen: HIGH = 1.33

## Oxidizer efficiency reference (Spaced Out / DLC1)

- Oxylite: VERY_LOW = 1.0
- Fertilizer: LOW = 2.0
- Liquid Oxygen: HIGH = 4.0

## Engine efficiency summary (base game classic rockets)

| Engine | Fuel | Efficiency | Exhaust |
|---|---|---|---|
| Steam | Steam | 20 (WEAK) | Steam 423.15 K |
| Petroleum | Petroleum | 40 (MEDIUM) | CO2 |
| Biodiesel | Refined Lipid | 50 (MEDIUM_PLUS) | CO2 1700 K |
| Hydrogen | Liquid Hydrogen | 60 (STRONG) | Steam 2000 K |
| Solid Booster | Iron + Oxylite | 30 (BOOSTER) | N/A |

## Cluster engine summary (Spaced Out)

| Engine | Fuel | Efficiency | Power (W) | Burden | Engine Power | Fuel Cost/Hex | Max Modules | Max Height |
|---|---|---|---|---|---|---|---|---|
| Steam | Steam | 20 | 600 | 15 | 27 | 0.025 | 6 | 25 |
| Small Petroleum | Petroleum | 40 | 240 | 5 | 31 | 0.075 | 4 | 20 |
| Petroleum | Petroleum | 40 | 480 | 6 | 48 | 0.15 | 7 | 35 |
| Biodiesel | Refined Lipid | 60 | 640 | 7 | 31 | ~0.115 | 7 | 35 |
| Hydrogen | Liquid Hydrogen | 60 | 600 | 7 | 55 | ~0.094 | 7 | 35 |
