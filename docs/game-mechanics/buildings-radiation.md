# Radiation

Per-building specs. For how buildings work in general (element conversion, cell occupation, damage), see `building-mechanics.md`. All rates are per second unless noted. Temperatures in Kelvin (subtract 273.15 for Celsius). Derived from decompiled source code. Spaced Out DLC only.

## Radiation Lamp

- **ID**: RadiationLight
- **Name**: Radiation Lamp
- **Description**: Duplicants can become sick if exposed to radiation without protection.
- **Effect**: Emits Radiation when Powered that can be collected by a Radbolt Generator.
- **Tech**: Agriculture
- **Size**: 1x1
- **Placement**: OnWall, FlipH rotation
- **Materials**: 50 kg Any Metal (TIER1)
- **HP**: 10
- **Decor**: none
- **Power**: 60 W
- **Self-heat**: 0.5 kDTU/s
- **Max temperature**: 800 K (526.85 C)
- **Fuel**: Uranium Ore
  - Consumption: 1/60 kg/s (10 kg/cycle)
  - Storage: 50 kg (5 cycles worth)
  - Refill threshold: 5 kg
- **Waste**: Depleted Uranium
  - Output: 1/120 kg/s (fuel-to-waste ratio 0.5)
  - Drops in 5 kg batches
- **Radiation emission**: 240 rads, radius 16x4, constant, 90-degree angle aimed right
  - Emission offset: 1 cell right

## Manual Radbolt Generator

- **ID**: ManualHighEnergyParticleSpawner
- **Name**: Manual Radbolt Generator
- **Description**: Radbolts are necessary for producing Materials Science research.
- **Effect**: Refines radioactive ores to generate Radbolts. Emits generated Radbolts in the direction of your choosing.
- **Tech**: NuclearResearch
- **Size**: 1x3
- **Placement**: OnFloor
- **Materials**: 800 kg Raw Mineral (TIER5)
- **HP**: 30
- **Decor**: -10 / radius 2 (PENALTY.TIER1)
- **Power**: none
- **Self-heat**: none
- **Max temperature**: 1600 K (1326.85 C)
- **Not overheatable, not floodable**
- **Duplicant-operated**: yes
- **HEP output port**: offset (0, 2)
- **Logic input**: automation on/off
- **Radiation emission**: 120 rads, radius 3x3, constant, offset (0, 2)
- **Recipes** (each takes 40 s):
  - Uranium Ore: 1 kg input -> 0.5 kg Depleted Uranium + 5 radbolts
  - Enriched Uranium: 1 kg input -> 0.8 kg Depleted Uranium + 25 radbolts

## Radbolt Generator

- **ID**: HighEnergyParticleSpawner
- **Name**: Radbolt Generator
- **Description**: Radbolts are necessary for producing Materials Science research.
- **Effect**: Attracts nearby Radiation to generate Radbolts. Emits generated Radbolts in the direction of your choosing when the set Radbolt threshold is reached. Radbolts collected will gradually decay while this building is disabled.
- **Tech**: AdvancedNuclearResearch
- **Size**: 1x2
- **Placement**: NotInTiles, R360 rotation (no foundation required)
- **Materials**: 400 kg Raw Mineral (TIER4)
- **HP**: 30
- **Decor**: -10 / radius 2 (PENALTY.TIER1)
- **Power**: 480 W
- **Exhaust heat**: 1 kDTU/s
- **Self-heat**: 4 kDTU/s
- **Max temperature**: 1600 K (1326.85 C)
- **Not overheatable, not floodable**
- **HEP output port**: offset (0, 1)
- **HEP storage capacity**: 500 radbolts
- **Conversion**: 0.1 radbolts per rad absorbed (HEP_PER_RAD)
- **Radiation sample rate**: 0.2 s
- **Min launch interval**: 2 s
- **Radbolt threshold slider**: 50 to 500
- **Disabled decay rate**: 0.05 radbolts/s

## Research Reactor (Nuclear Reactor)

- **ID**: NuclearReactor
- **Name**: Research Reactor
- **Description**: Radbolt generators and reflectors make radiation usable by other buildings.
- **Effect**: Uses Enriched Uranium to produce Radiation for Radbolt production. Generates a massive amount of Heat. Overheating will result in an explosive meltdown.
- **Tech**: NuclearRefinement
- **Size**: 5x6
- **Placement**: OnFloor
- **Materials**: 800 kg Refined Metal (TIER5)
- **HP**: 100
- **Decor**: -15 / radius 3 (PENALTY.TIER2)
- **Power**: none (does not generate or consume electrical power)
- **Self-heat**: 0 kDTU/s (heat comes from the reaction itself; see below)
- **Max temperature**: 9999 K (not overheatable, invincible, not floodable, not entombable, not breakable, corrosion-proof)
- **Thermal conductivity**: 0.1
- **Fuel**: Enriched Uranium
  - Fuel capacity: 180 kg (supply storage)
  - Consumption: 1/60 kg/s
  - Begin reaction threshold: 0.5 kg fuel + 30 kg coolant minimum
  - Stop reaction threshold: fuel drops to 0.25 kg
  - Reaction mass target: 60 kg (auto-refills to this)
- **Coolant**: Any Water (liquid pipe input)
  - Input offset: (-2, 2)
  - Conduit consumption rate: 10 kg/s
  - Active coolant amount: 30 kg
  - Coolant capacity: 90 kg
  - Coolant vents as steam at 673.15 K (400 C)
  - Max vent pressure: 150 kg/cell
  - Conduction scale: 5x (forced conduction between fuel and coolant)
- **Waste**: Nuclear Waste (liquid)
  - Waste mass multiplier: 100x spent fuel mass
  - Waste dump threshold: 100 kg (drops all waste when reached)
  - Waste contains Radiation Poisoning germs at 50 germs/kg
- **Radiation emission**: constant, radius 25x25
  - Operational: 2400 rads
  - Meltdown: 4800 rads
  - Post-meltdown dead state: linearly decays from 4800 to 0 over 3000 s (5 cycles)
  - Emission offset: (0, 2)
- **Reaction heat**: 100 kDTU/s per kg of active fuel added to the fuel element (via EnergyToTemperatureDelta with REACTION_STRENGTH=100)
- **Meltdown**: triggers when active fuel temperature reaches 3000 K (2726.85 C)
  - Ejects nuclear waste comets every 0.5 s, up to 5 kg/burst
  - Meltdown mass = 10 + all stored mass (supply + reaction + waste)
  - Nuclear waste spawned at 3000 K with radiation poisoning germs
  - After meltdown mass is exhausted, enters permanent dead state
- **Logic input**: CONTROL_FUEL_DELIVERY port at (0, 0)
  - Green signal: fuel delivery enabled (default)
  - Red signal: fuel delivery disabled
- **Storage**: 3 separate sealed/insulated/hidden storages (supply, reaction, waste)

## Uranium Centrifuge

- **ID**: UraniumCentrifuge
- **Name**: Uranium Centrifuge
- **Description**: Enriched uranium is a specialized substance that can be used to fuel powerful research reactors.
- **Effect**: Extracts Enriched Uranium from Uranium Ore. Outputs Depleted Uranium in molten form.
- **Tech**: NuclearRefinement
- **Size**: 3x4
- **Placement**: OnFloor
- **Materials**: 800 kg Refined Metal (TIER5) + 100 kg Plastic (TIER2)
- **HP**: 100
- **Decor**: -10 / radius 2 (PENALTY.TIER1)
- **Power**: 480 W
- **Exhaust heat**: 0.125 kDTU/s
- **Self-heat**: 0.5 kDTU/s
- **Max temperature**: 2400 K (2126.85 C)
- **Not overheatable**
- **Duplicant-operated**: no (automatic)
- **Logic input**: automation on/off
- **Liquid output conduit**: offset (1, 3)
- **Output storage capacity**: 2000 kg
- **Recipe** (40 s):
  - Input: 10 kg Uranium Ore
  - Output: 2 kg Enriched Uranium (average temperature) + 8 kg Molten Uranium (output at element melting point)

## Radbolt Reflector

- **ID**: HighEnergyParticleRedirector
- **Name**: Radbolt Reflector
- **Description**: We were all out of mirrors.
- **Effect**: Receives and redirects Radbolts from Radbolt Generators.
- **Tech**: AdvancedNuclearResearch
- **Size**: 1x2
- **Placement**: NotInTiles, R360 rotation (no foundation required)
- **Materials**: 400 kg Raw Mineral (TIER4)
- **HP**: 30
- **Decor**: -10 / radius 2 (PENALTY.TIER1)
- **Power**: none
- **Max temperature**: 1600 K (1326.85 C)
- **Not overheatable, not floodable**
- **HEP input port**: offset (0, 0)
- **HEP output port**: offset (0, 1)
- **HEP storage**: 501 capacity (auto-store, hidden from UI)
- **Travel delay**: 0.5 s
- **Redirect cost**: 10% of radbolt payload (REDIRECT_PARTICLE_COST = 0.1)
- **Logic input**: at offset (0, 1)
  - Green signal: allow incoming radbolts
  - Red signal: ignore incoming radbolts

## Radbolt Chamber (HEP Battery)

- **ID**: HEPBattery
- **Name**: Radbolt Chamber
- **Description**: Particles packed up and ready to go.
- **Effect**: Stores Radbolts in a high-energy state, ready for transport. Requires a Green Signal to release radbolts from storage when the Radbolt threshold is reached. Radbolts in storage will rapidly decay while this building is disabled.
- **Tech**: NuclearStorage
- **Size**: 3x3
- **Placement**: OnFloor
- **Materials**: 400 kg Refined Metal (TIER4)
- **HP**: 30
- **Decor**: -15 / radius 3 (PENALTY.TIER2)
- **Power**: 120 W
- **Exhaust heat**: 0.25 kDTU/s
- **Self-heat**: 1 kDTU/s
- **Max temperature**: 800 K (526.85 C)
- **Not overheatable, not floodable**
- **HEP input port**: offset (0, 1)
- **HEP output port**: offset (0, 2)
- **HEP storage capacity**: 1000 radbolts (auto-store, shows capacity status)
- **Min launch interval**: 1 s
- **Radbolt threshold slider**: 0 to 100
- **Disabled decay rate**: 0.05 radbolts/s
- **Logic input** (fire port): at offset (0, 2)
  - Green signal: emit radbolts
  - Red signal: do not emit radbolts
- **Logic output** (storage port): at offset (1, 1)
  - Green signal: storage full
  - Red signal: storage not full

## Radbolt Joint Plate

- **ID**: HEPBridgeTile
- **Name**: Radbolt Joint Plate
- **Description**: Allows Radbolts to pass through walls.
- **Effect**: Receives Radbolts from Radbolt Generators and directs them through walls. All other materials and elements will be blocked from passage.
- **Tech**: AdvancedNuclearResearch
- **Size**: 2x1
- **Placement**: Tile, R360 rotation
- **Materials**: 200 kg Plastic (TIER3)
- **HP**: 100 (destroyOnDamaged = true)
- **Decor**: -25 / radius 6 (PENALTY.TIER5)
- **Power**: none
- **Max temperature**: 1600 K (1326.85 C)
- **Not overheatable, not floodable, not entombable**
- **Does not use structure temperature**
- **No repair** (BaseTimeUntilRepair = -1)
- **Initial orientation**: R180
- **HEP input port**: offset (1, 0)
- **HEP output port**: offset (0, 0)
- **HEP storage**: 501 capacity (auto-store, hidden from UI)
- **Travel delay**: 0.5 s
- **Direction**: fixed (not player-controllable), determined by rotation:
  - Neutral: Left
  - R90: Up
  - R180: Right (default, matching InitialOrientation)
  - R270: Down
- **Tile behavior**: creates a solid cell at offset (0, 0) via MakeBaseSolid

## Dev Radbolt Generator

- **ID**: DevHEPSpawner
- **Name**: Dev Radbolt Generator
- **Description**: Radbolts are necessary for producing Materials Science research.
- **Effect**: Generates Radbolts.
- **Debug only**: yes (not available in normal gameplay)
- **Size**: 1x1
- **Placement**: NotInTiles, R360 rotation (no foundation required)
- **Materials**: 400 kg Raw Mineral (TIER4)
- **HP**: 30
- **Decor**: -10 / radius 2 (PENALTY.TIER1)
- **Power**: none
- **Max temperature**: 1600 K (1326.85 C)
- **Invincible, not overheatable, not floodable, not entombable**
- **HEP output port**: offset (0, 0)
- **Radbolt amount per launch**: 50
- **Logic input**: automation on/off

## Dev Radiation Emitter

- **ID**: DevRadiationGenerator
- **Name**: Dev Radiation Emitter
- **Description**: That's some *strong* coffee.
- **Effect**: Generates on-demand radiation to keep things clear. *Nu-*clear.
- **Debug only**: yes (not available in normal gameplay)
- **DLC gating**: none (available in base game, unlike all other radiation buildings)
- **Size**: 1x1
- **Placement**: Anywhere
- **Materials**: 25 kg Any Metal (TIER0)
- **HP**: 100
- **Decor**: -15 / radius 3 (PENALTY.TIER2)
- **Power**: none
- **Max temperature**: 9999 K
- **Not overheatable, not floodable**
- **Radiation emission**: constant, radius 12x12, not proportional to rads
  - Intensity: 2400 / (12/6) = 1200 rads
