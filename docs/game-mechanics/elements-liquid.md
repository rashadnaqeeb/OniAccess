# Liquid Elements

Complete reference for all liquid elements in Oxygen Not Included. Values sourced from the game's element YAML definitions (StreamingAssets/elements/liquid.yaml) and ElementLoader.cs.

All temperatures are in Kelvin with Celsius in parentheses. "Low temp transition" is the freezing point; "high temp transition" is the boiling point. The "speed" field maps to viscosity in the Element class (lower = more viscous). The "liquidCompression" field maps to maxCompression. All liquids have a liquidSurfaceAreaMultiplier of 25.

Elements marked "disabled" exist in the data but are not available in normal gameplay. Elements with a dlcId of "EXPANSION1_ID" require the Spaced Out! DLC.

## Water and Aqueous Solutions

| Property | Water | Polluted Water | Salt Water | Brine | Sugar Water | Milk |
|----------|-------|----------------|------------|-------|-------------|------|
| Element ID | Water | DirtyWater | SaltWater | Brine | SugarWater | Milk |
| SHC (kJ/kg/K) | 4.179 | 4.179 | 4.1 | 3.4 | 4.1 | 4.1 |
| Thermal conductivity | 0.609 | 0.58 | 0.609 | 0.609 | 0.609 | 0.609 |
| Max mass (kg) | 1000 | 1000 | 1100 | 1200 | 1100 | 1100 |
| Max compression | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 |
| Viscosity (speed) | 125 | 125 | 100 | 100 | 100 | 100 |
| Min horiz. flow | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 |
| Min vert. flow | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 |
| Molar mass | 18.015 | 20 | 21 | 22 | 21 | 23 |
| Radiation absorption | 0.8 | 0.8 | 0.8 | 0.8 | 0.9 | 0.8 |
| Light absorption | 0.25 | 0.7 | 0.25 | 0.25 | 0.5 | 0.8 |
| Default temp (K) | 300 | 300 | 300 | 282.15 | 233.15 | 310 |
| Default mass (kg) | 1000 | 1000 | 1100 | 1200 | 1100 | 1100 |
| Toxicity | 0 | 0.1 | 0 | 0 | 0 | 0 |
| Tags | AnyWater | Mixture, AnyWater | AnyWater | AnyWater | PlastifiableLiquid | (none) |

### Temperature Transitions

| Element | Freezing point | Freezes into | Freeze byproduct | Boiling point | Boils into | Boil byproduct |
|---------|---------------|--------------|------------------|---------------|------------|----------------|
| Water | 272.5 K (-0.65 C) | Ice | | 372.5 K (99.35 C) | Steam | |
| Polluted Water | 252.5 K (-20.65 C) | Dirty Ice | | 392.5 K (119.35 C) | Steam | Dirt (1%) |
| Salt Water | 265.65 K (-7.5 C) | Brine | Ice (77%) | 372.84 K (99.69 C) | Steam | Salt (7%) |
| Brine | 250.65 K (-22.5 C) | Brine Ice | | 375.9 K (102.75 C) | Steam | Salt (30%) |
| Sugar Water | 190.65 K (-82.5 C) | Ice | Sucrose (77%) | 433.15 K (160 C) | Steam | Sucrose (77%) |
| Milk | 256.65 K (-16.5 C) | Milk Ice | | 353.15 K (80 C) | Brine | Milk Fat (10%) |

### Off-gassing

Polluted Water off-gasses Polluted Oxygen at a rate of 0.1% per cell with a probability of 1% per sim tick, at 100% efficiency.

### Special Properties

Water has a convertId of DirtyWater, meaning germy water can convert to Polluted Water under certain disease conditions.

## Oils and Hydrocarbons

| Property | Crude Oil | Petroleum | Naphtha | Ethanol | Phyto Oil | Refined Lipid |
|----------|-----------|-----------|---------|---------|-----------|---------------|
| Element ID | CrudeOil | Petroleum | Naphtha | Ethanol | PhytoOil | RefinedLipid |
| SHC (kJ/kg/K) | 1.69 | 1.76 | 2.191 | 2.46 | 0.9 | 2.19 |
| Thermal conductivity | 2 | 2 | 0.2 | 0.171 | 2 | 2 |
| Max mass (kg) | 870 | 740 | 740 | 1000 | 870 | 870 |
| Max compression | 1.01 | 1.01 | 1.01 | 1.01 | 1 | 1 |
| Viscosity (speed) | 50 | 50 | 30 | 125 | 100 | 100 |
| Min horiz. flow | 0.1 | 0.1 | 10 | 0.01 | 0.1 | 0.1 |
| Min vert. flow | 0.1 | 0.1 | 10 | 0.01 | 0.1 | 0.1 |
| Molar mass | 500 | 82.2 | 102.2 | 46.07 | 450 | 450 |
| Radiation absorption | 0.8 | 0.8 | 0.6 | 0.7 | 0.9 | 0.7 |
| Light absorption | 1 | 0.8 | 0.8 | 0.25 | 0.3 | 0.3 |
| Default temp (K) | 350 | 300 | 350 | 300 | 293 | 293 |
| Default mass (kg) | 870 | 740 | 740 | 1000 | 800 | 800 |
| Toxicity | 0 | 0 | 0.25 | 0 | 0 | 0 |
| Tags | Slippery, LubricatingOil, UnrefinedOil | CombustibleLiquid, PlastifiableLiquid | Oil | CombustibleLiquid | Slippery, LubricatingOil, UnrefinedOil | Slippery, CombustibleLiquid |

### Temperature Transitions

| Element | Freezing point | Freezes into | Boiling point | Boils into | Boil byproduct |
|---------|---------------|--------------|---------------|------------|----------------|
| Crude Oil | 233 K (-40.15 C) | Solid Crude Oil | 673 K (399.85 C) | Petroleum | |
| Petroleum | 216 K (-57.15 C) | Solid Petroleum | 812 K (538.85 C) | Sour Gas | |
| Naphtha | 223 K (-50.15 C) | Solid Naphtha | 812 K (538.85 C) | Sour Gas | |
| Ethanol | 159.1 K (-114.05 C) | Solid Ethanol | 351.5 K (78.35 C) | Ethanol Gas | |
| Phyto Oil | 240 K (-33.15 C) | Frozen Phyto Oil | 348.15 K (75 C) | Carbon Dioxide | Algae (66.66%) |
| Refined Lipid | 263.15 K (-10 C) | Tallow | 453.15 K (180 C) | Carbon Dioxide | |

## Resins

| Property | Natural Resin | Resin |
|----------|---------------|-------|
| Element ID | NaturalResin | Resin |
| SHC (kJ/kg/K) | 1.11 | 1.11 |
| Thermal conductivity | 0.15 | 0.15 |
| Max mass (kg) | 920 | 920 |
| Max compression | 1.01 | 1.01 |
| Viscosity (speed) | 1.1 | 1.1 |
| Min horiz. flow | 1.1 | 1.1 |
| Min vert. flow | 0.01 | 0.01 |
| Molar mass | 52.5 | 52.5 |
| Radiation absorption | 0.75 | 0.75 |
| Light absorption | 0.8 | 0.8 |
| Default temp (K) | 300 | 300 |
| Default mass (kg) | 920 | 920 |
| Tags | PlastifiableLiquid | (none) |

### Temperature Transitions

| Element | Freezing point | Freezes into | Boiling point | Boils into | Boil byproduct |
|---------|---------------|--------------|---------------|------------|----------------|
| Natural Resin | 293.15 K (20 C) | Natural Solid Resin | 398.15 K (125 C) | Steam | Refined Carbon (25%) |
| Resin | 293.15 K (20 C) | Solid Resin | 398.15 K (125 C) | Steam | Isoresin (25%) |

Both resins are extremely viscous (speed 1.1) and barely flow horizontally.

## Cryogenic Liquids

| Property | Liquid Oxygen | Liquid Hydrogen | Liquid Methane | Liquid CO2 | Liquid Propane | Liquid Chlorine | Liquid Helium |
|----------|---------------|-----------------|----------------|------------|----------------|-----------------|---------------|
| Element ID | LiquidOxygen | LiquidHydrogen | LiquidMethane | LiquidCarbonDioxide | LiquidPropane | Chlorine | LiquidHelium |
| SHC (kJ/kg/K) | 1.01 | 2.4 | 2.191 | 0.846 | 2.4 | 0.48 | 0.2 |
| Thermal conductivity | 2 | 0.1 | 0.03 | 1.46 | 0.1 | 0.0081 | 0.236 |
| Max mass (kg) | 500 | 1000 | 1000 | 2000 | 1000 | 1000 | 1000 |
| Max compression | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 |
| Viscosity (speed) | 200 | 180 | 180 | 125 | 180 | 180 | 100 |
| Min horiz. flow | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 |
| Min vert. flow | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 | 0.01 |
| Molar mass | 15.999 | 1.008 | 16.044 | 44.01 | 44.1 | 34.453 | 4 |
| Radiation absorption | 0.82 | 0.9 | 0.75 | 0.8 | 0.75 | 0.73 | 0.89 |
| Light absorption | 1 | 1 | 0.6 | 1 | 1 | 1 | 0.8 |
| Default temp (K) | 74.36 | 18 | 100 | 220 | 200 | 200 | 1 |
| Default mass (kg) | 300 | 600 | 600 | 600 | 600 | 600 | 200 |
| Tags | Oxidizer | (none) | (none) | (none) | HideFromSpawnTool, HideFromCodex | (none) | (none) |
| Disabled | No | No | No | No | No | No | Yes |

### Temperature Transitions

| Element | Freezing point | Freezes into | Boiling point | Boils into |
|---------|---------------|--------------|---------------|------------|
| Liquid Oxygen | 54.36 K (-218.79 C) | Solid Oxygen | 90.19 K (-182.96 C) | Oxygen |
| Liquid Hydrogen | 14 K (-259.15 C) | Solid Hydrogen | 21 K (-252.15 C) | Hydrogen |
| Liquid Methane | 90.55 K (-182.6 C) | Solid Methane | 111.65 K (-161.5 C) | Methane |
| Liquid CO2 | 216.6 K (-56.55 C) | Solid Carbon Dioxide | 225 K (-48.15 C) | Carbon Dioxide |
| Liquid Propane | 85 K (-188.15 C) | Solid Propane | 231 K (-42.15 C) | Propane |
| Liquid Chlorine | 172.17 K (-100.98 C) | Solid Chlorine | 238.55 K (-34.6 C) | Chlorine Gas |
| Liquid Helium | (none) | (none) | 4.22 K (-268.93 C) | Helium |

Liquid Helium has lowTemp 0 (absolute zero) with no low-temp transition target, so it cannot freeze. It is disabled in normal gameplay.

Liquid Propane is hidden from the spawn tool and codex but is not disabled.

## Specialty and Industrial Liquids

| Property | Super Coolant | Visco-Gel | Nuclear Waste | Liquid Sulfur | Liquid Phosphorus | Mercury | Liquid Gunk |
|----------|--------------|-----------|---------------|---------------|-------------------|---------|-------------|
| Element ID | SuperCoolant | ViscoGel | NuclearWaste | LiquidSulfur | LiquidPhosphorus | Mercury | LiquidGunk |
| SHC (kJ/kg/K) | 8.44 | 1.55 | 7.44 | 0.7 | 0.7697 | 0.14 | 1.2 |
| Thermal conductivity | 9.46 | 0.45 | 6 | 0.2 | 0.236 | 8.3 | 1.5 |
| Max mass (kg) | 910 | 100 | 1000 | 740 | 1000 | 1000 | 870 |
| Max compression | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 | 1.01 |
| Viscosity (speed) | 150 | 1 | 100 | 50 | 100 | 140 | 50 |
| Min horiz. flow | 0.01 | 10 | 25 | 0.1 | 2 | 0.01 | 0.1 |
| Min vert. flow | 0.01 | 10 | 1 | 0.1 | 1 | 0.01 | 0.1 |
| Molar mass | 250 | 10 | 196.967 | 32 | 30.974 | 200.59 | 550 |
| Radiation absorption | 0.6 | 0.6 | 0.3 | 0.74 | 0.75 | 0.25 | 0.9 |
| Radiation per 1000 kg | 0 | 0 | 150 | 0 | 0 | 0 | 0 |
| Light absorption | 0.9 | 0.1 | 1 | 0.1 | 1 | 1 | 1 |
| Default temp (K) | 238 | 238 | 400 | 450 | 500 | 320 | 310 |
| Default mass (kg) | 800 | 100 | 500 | 190 | 200 | 600 | 870 |
| Tags | (none) | (none) | EmitsLight | (none) | EmitsLight | RefinedMetal | Slippery |
| DLC required | No | No | Spaced Out! | No | No | No | No |

### Temperature Transitions

| Element | Freezing point | Freezes into | Boiling point | Boils into | Boil byproduct |
|---------|---------------|--------------|---------------|------------|----------------|
| Super Coolant | 2 K (-271.15 C) | Solid Super Coolant | 710 K (436.85 C) | Super Coolant Gas | |
| Visco-Gel | 242.5 K (-30.65 C) | Solid Visco-Gel | 753 K (479.85 C) | Naphtha | |
| Nuclear Waste | 300 K (26.85 C) | Solid Nuclear Waste | 800 K (526.85 C) | Fallout | |
| Liquid Sulfur | 388.35 K (115.2 C) | Sulfur | 610.15 K (337 C) | Sulfur Gas | |
| Liquid Phosphorus | 317.3 K (44.15 C) | Phosphorus | 553.6 K (280.45 C) | Phosphorus Gas | |
| Mercury | 234.3 K (-38.85 C) | Solid Mercury | 629.9 K (356.75 C) | Mercury Gas | |
| Liquid Gunk | 265 K (-8.15 C) | Gunk | 721 K (447.85 C) | Petroleum | Sulfur (8%) |

### Notable Properties

- **Super Coolant** has the highest SHC (8.44) and very high thermal conductivity (9.46) of any liquid, making it ideal for heat transfer. Its enormous liquid range (2 K to 710 K) covers nearly all gameplay temperatures.
- **Nuclear Waste** has extremely high SHC (7.44) and thermal conductivity (6), and emits radiation (150 per 1000 kg). Requires Spaced Out! DLC.
- **Visco-Gel** has the lowest viscosity (speed 1) and highest minimum flow thresholds (10/10) of any liquid, making it almost immobile. Its max mass of 100 kg per cell is the lowest of any liquid.
- **Mercury** has very low SHC (0.14) but the highest thermal conductivity among non-molten liquids (8.3). Tagged as RefinedMetal.
- **Liquid Phosphorus** emits light.

## Molten Sucrose

| Property | Value |
|----------|-------|
| Element ID | MoltenSucrose |
| SHC (kJ/kg/K) | 1.255 |
| Thermal conductivity | 0.15 |
| Max mass (kg) | 740 |
| Max compression | 1.01 |
| Viscosity (speed) | 50 |
| Min horiz. flow | 0.1 |
| Min vert. flow | 0.1 |
| Molar mass | 32 |
| Radiation absorption | 0.7 |
| Light absorption | 0.1 |
| Default temp (K) | 500 |
| Default mass (kg) | 190 |

### Temperature Transitions

| Freezing point | Freezes into | Boiling point | Boils into |
|---------------|--------------|---------------|------------|
| 459 K (185.85 C) | Sucrose | 503.15 K (230 C) | Carbon Dioxide |

## Molten Metals

| Property | Molten Aluminum | Molten Copper | Molten Gold | Molten Iron | Molten Cobalt | Molten Lead | Molten Nickel | Molten Niobium | Molten Steel | Molten Tungsten | Molten Iridium | Molten Uranium |
|----------|-----------------|---------------|-------------|-------------|---------------|-------------|---------------|----------------|--------------|-----------------|----------------|----------------|
| Element ID | MoltenAluminum | MoltenCopper | MoltenGold | MoltenIron | MoltenCobalt | MoltenLead | MoltenNickel | MoltenNiobium | MoltenSteel | MoltenTungsten | MoltenIridium | MoltenUranium |
| SHC (kJ/kg/K) | 0.91 | 0.386 | 0.1291 | 0.449 | 0.42 | 0.128 | 0.44 | 0.265 | 0.386 | 0.134 | 0.131 | 1.69 |
| Thermal cond. | 20.5 | 12 | 6 | 4 | 4 | 11 | 30 | 54 | 80 | 4 | 170 | 2 |
| Max mass (kg) | 7870 | 3870 | 9970 | 7870 | 7870 | 9970 | 7870 | 3870 | 3870 | 3870 | 3870 | 9970 |
| Viscosity (speed) | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 | 100 |
| Min horiz. flow | 30 | 20 | 25 | 30 | 30 | 25 | 15 | 20 | 20 | 20 | 20 | 25 |
| Min vert. flow | 3 | 2 | 1 | 3 | 3 | 1 | 2 | 10 | 10 | 10 | 10 | 1 |
| Molar mass | 55.845 | 63.546 | 196.967 | 55.845 | 58.9 | 196.967 | 58.69 | 92.9 | 63.546 | 183.84 | 183.84 | 196.967 |
| Radiation abs. | 0.77 | 0.61 | 0.35 | 0.66 | 0.63 | 0.85 | 0.7 | 0.49 | 0.74 | 0.35 | 0.88 | 0.3 |
| Radiation/1000 kg | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 150 |
| Light absorption | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 0.7 | 1 | 1 |
| Default temp (K) | 2000 | 1800 | 2000 | 2500 | 2500 | 2000 | 1800 | 3000 | 2000 | 4000 | 3500 | 460 |
| Default mass (kg) | 1000 | 900 | 870 | 1000 | 1000 | 3000 | 900 | 900 | 900 | 200 | 900 | 3000 |
| DLC required | No | No | No | No | No | No | No | No | No | No | No | Spaced Out! |

All molten metals have maxCompression 1.01 and share the tags Metal, RefinedMetal, and EmitsLight (except Molten Tungsten and Molten Iridium, which lack EmitsLight).

### Temperature Transitions

| Element | Freezing point | Freezes into | Boiling point | Boils into |
|---------|---------------|--------------|---------------|------------|
| Molten Aluminum | 933.45 K (660.3 C) | Aluminum | 2743.15 K (2470 C) | Aluminum Gas |
| Molten Copper | 1357 K (1083.85 C) | Copper | 2834 K (2560.85 C) | Copper Gas |
| Molten Gold | 1337 K (1063.85 C) | Gold | 3129 K (2855.85 C) | Gold Gas |
| Molten Iron | 1808 K (1534.85 C) | Iron | 3023 K (2749.85 C) | Iron Gas |
| Molten Cobalt | 1768 K (1494.85 C) | Cobalt | 3200 K (2926.85 C) | Cobalt Gas |
| Molten Lead | 600.65 K (327.5 C) | Lead | 2022.15 K (1749 C) | Lead Gas |
| Molten Nickel | 1728 K (1454.85 C) | Nickel | 3003 K (2729.85 C) | Nickel Gas |
| Molten Niobium | 2750 K (2476.85 C) | Niobium | 5017 K (4743.85 C) | Niobium Gas |
| Molten Steel | 1357 K (1083.85 C) | Steel | 4100 K (3826.85 C) | Steel Gas |
| Molten Tungsten | 3695 K (3421.85 C) | Tungsten | 6203 K (5929.85 C) | Tungsten Gas |
| Molten Iridium | 2719 K (2445.85 C) | Iridium | 4403 K (4129.85 C) | Iridium Gas |
| Molten Uranium | 406 K (132.85 C) | Depleted Uranium | 4405 K (4131.85 C) | Rock Gas |

### Notable Properties

- **Molten Iridium** has the highest thermal conductivity of any element in the game (170).
- **Molten Steel** has the second-highest thermal conductivity among liquids (80).
- **Molten Niobium** is notable for its extremely high thermal conductivity (54) and very high boiling point (5017 K).
- **Molten Tungsten** has the highest boiling point of any molten metal (6203 K) and the highest freezing point (3695 K).
- **Molten Uranium** emits radiation (150 per 1000 kg), freezes into Depleted Uranium (not Uranium Ore), and boils into Rock Gas rather than a metal-specific gas. Requires Spaced Out! DLC.
- **Molten Lead** has the lowest freezing point among molten metals (600.65 K), making it the easiest to encounter in liquid form.

## Magma and Molten Rock

| Property | Magma | Molten Glass | Molten Carbon | Molten Salt |
|----------|-------|--------------|---------------|-------------|
| Element ID | Magma | MoltenGlass | MoltenCarbon | MoltenSalt |
| SHC (kJ/kg/K) | 1 | 0.2 | 0.71 | 0.7 |
| Thermal conductivity | 1 | 1 | 2 | 0.444 |
| Max mass (kg) | 1840 | 1840 | 4000 | 740 |
| Max compression | 1.01 | 1.01 | 1.01 | 1.01 |
| Viscosity (speed) | 60 | 60 | 150 | 50 |
| Min horiz. flow | 50 | 50 | 0.01 | 0.1 |
| Min vert. flow | 20 | 20 | 0.01 | 0.1 |
| Molar mass | 50 | 50 | 12.011 | 32 |
| Radiation absorption | 0.8 | 0.65 | 0.84 | 0.75 |
| Light absorption | 1 | 0.7 | 1 | 0.1 |
| Default temp (K) | 2000 | 2215 | 4600 | 450 |
| Default mass (kg) | 1840 | 200 | 600 | 190 |
| Tags | EmitsLight | EmitsLight | (none) | (none) |

### Temperature Transitions

| Element | Freezing point | Freezes into | Boiling point | Boils into |
|---------|---------------|--------------|---------------|------------|
| Magma | 1683 K (1409.85 C) | Igneous Rock | 2630 K (2356.85 C) | Rock Gas |
| Molten Glass | 1400 K (1126.85 C) | Glass | 2630 K (2356.85 C) | Rock Gas |
| Molten Carbon | 3825 K (3551.85 C) | Refined Carbon | 5100 K (4826.85 C) | Carbon Gas |
| Molten Salt | 1073 K (799.85 C) | Salt | 1738 K (1464.85 C) | Salt Gas |

Magma and Molten Glass have very high minimum flow thresholds (50/20), making them sluggish. Molten Carbon, despite its extreme temperatures, flows freely (min flow 0.01/0.01).

## Disabled Elements

| Property | Liquid Helium | Molten Syngas |
|----------|---------------|---------------|
| Element ID | LiquidHelium | MoltenSyngas |
| SHC (kJ/kg/K) | 0.2 | 2.4 |
| Thermal conductivity | 0.236 | 0.1 |
| Max mass (kg) | 1000 | 1000 |
| Viscosity (speed) | 100 | 180 |
| Freezing point | (none) | 14 K (-259.15 C) |
| Freezes into | (none) | Solid Syngas |
| Boiling point | 4.22 K (-268.93 C) | 21 K (-252.15 C) |
| Boils into | Helium | Syngas |

Both elements are flagged as disabled and do not appear in normal gameplay. Molten Syngas has identical thermal properties to Liquid Hydrogen.

## Summary Statistics

Total liquid elements: 46 (44 enabled, 2 disabled; 2 require Spaced Out! DLC)

### Thermal Conductivity Extremes

| Rank | Element | Thermal Conductivity |
|------|---------|---------------------|
| Highest | Molten Iridium | 170 |
| 2nd | Molten Steel | 80 |
| 3rd | Molten Niobium | 54 |
| Lowest enabled | Liquid Chlorine | 0.0081 |
| 2nd lowest | Liquid Methane | 0.03 |

### Specific Heat Capacity Extremes

| Rank | Element | SHC (kJ/kg/K) |
|------|---------|---------------|
| Highest | Super Coolant | 8.44 |
| 2nd | Nuclear Waste | 7.44 |
| 3rd | Water / Polluted Water | 4.179 |
| Lowest | Molten Lead | 0.128 |
| 2nd lowest | Molten Gold | 0.1291 |

### Temperature Range Extremes

| Record | Element | Value |
|--------|---------|-------|
| Lowest freezing point (enabled) | Super Coolant | 2 K (-271.15 C) |
| Highest boiling point | Molten Tungsten | 6203 K (5929.85 C) |
| Widest liquid range | Super Coolant | 708 K (2 to 710) |
| Narrowest liquid range | Liquid CO2 | 8.4 K (216.6 to 225) |
