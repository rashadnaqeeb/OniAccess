# Solid Elements

Complete reference for every solid element in Oxygen Not Included. All values are sourced from the game's `solid.yaml` element definitions and entity config files in the decompiled source.

## How to Read This Document

**Temperature** is given in Kelvin with Celsius in parentheses. The game uses Kelvin internally.

**Specific Heat Capacity (SHC)** is in DTU/(g-K). Higher values mean the material takes more energy to change temperature.

**Thermal Conductivity (TC)** is in DTU/(m-s-K). Higher values mean the material transfers heat faster. A value of 0 makes the element temperature-insulated. A value of 0.00001 is effectively insulating.

**Hardness** ranges from 0 to 255. Determines which digging skill tier is needed:
- 0: Soft (no skill needed)
- 1-5: Soft
- 6-10: Firm
- 11-50: Normal (Hard Digging skill)
- 51-150: Very Hard (Super-Hard Digging skill)
- 151-200: Nearly Impenetrable (Super-Hard Digging)
- 201-254: Nearly Impenetrable (requires Super-Hard Digging)
- 255: Impenetrable (Neutronium, cannot be dug)

**Strength** determines break resistance. 0 means unbreakable.

**Melts into** shows the high-temperature transition target. The temperature listed is when the transition occurs.

**Ore ID** after a melt transition means a secondary product is also created, with the listed mass conversion fraction.

**Sublimation** entries show the gas emitted, the rate (kg/s per 600kg), efficiency (mass fraction converted to gas), and probability (chance per sim tick). Some elements have sublimation defined only in their entity config rather than the YAML; these are noted.

**Material category** determines where the element appears in build menus. Elements with `isDisabled: true` exist in the data but are not available in normal gameplay; these are marked as "Disabled".

**DLC** column: blank means base game, "SP" means Spaced Out! DLC (EXPANSION1_ID).

**Radiation absorption** is a factor from 0 to 1 indicating how much radiation the element blocks. **Radiation emission** (radiationPer1000Mass) indicates how much radiation the element emits per 1000 kg; 0 means none.

---

## Raw Minerals (BuildableRaw)

Natural rock types usable as construction material.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Sandstone | 0.8 | 2.9 | 10 | 0.5 | 1840 kg | 1200 K (926.85 C) | Magma | 0.7 | Plumbable, Crushable, BuildableAny | |
| Igneous Rock | 1.0 | 2.0 | 25 | 1.0 | 1840 kg | 1683 K (1409.85 C) | Magma | 0.75 | Plumbable, Crushable, BuildableAny | |
| Sedimentary Rock | 0.2 | 2.0 | 2 | 0.2 | 1840 kg | 1200 K (926.85 C) | Magma | 0.7 | Plumbable, Crushable, BuildableAny | |
| Granite | 0.79 | 3.39 | 80 | 1.5 | 1840 kg | 942 K (668.85 C) | Magma | 0.7 | Plumbable, Crushable, BuildableAny, PreciousRock | |
| Obsidian | 0.2 | 2.0 | 200 | 1.0 | 1840 kg | 3000 K (2726.85 C) | Magma | 0.75 | Plumbable, Crushable, BuildableAny, PreciousRock | |
| Shale | 0.25 | 1.8 | 2 | 0.25 | 1840 kg | 1100 K (826.85 C) | Magma | 0.75 | Plumbable, Crushable, BuildableAny | |
| Ceramic | 0.84 | 0.62 | 50 | 1.0 | 2000 kg | 2123 K (1849.85 C) | Magma | 0.65 | Plumbable, Crushable, BuildableAny, Insulator | |
| Fossil | 0.91 | 2.0 | 50 | 0.2 | 1000 kg | 1612 K (1338.85 C) | Magma | 0.7 | Fossils | |
| Mafic Rock | 0.2 | 1.0 | 2 | 1.0 | 1840 kg | 1683 K (1409.85 C) | Magma | 0.65 | BuildableAny | |
| Graphite | 0.71 | 8.0 | 0 | 0.1 | 600 kg | 550 K (276.85 C) | Refined Carbon | 0.85 | | |

## Metal Ore (Metal)

Unrefined metal ores. Can be smelted into refined metals.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Refines to | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------------|------|-----|
| Cuprite | 0.386 | 4.5 | 25 | 0.7 | 1840 kg | 1357 K (1083.85 C) | Molten Copper | Copper | 0.56 | Ore, BuildableAny, StartingMetalOre | |
| Iron Ore | 0.449 | 4.0 | 25 | 0.9 | 1840 kg | 1808 K (1534.85 C) | Molten Iron | Iron | 0.61 | Ore, BuildableAny | |
| Gold Amalgam | 0.15 | 2.0 | 2 | 0.8 | 1840 kg | 1337 K (1063.85 C) | Molten Gold | Gold | 0.3 | Ore, BuildableAny | |
| Aluminum Ore | 0.91 | 20.5 | 25 | 0.7 | 500 kg | 1357 K (1083.85 C) | Molten Aluminum | Aluminum | 0.72 | Ore, BuildableAny, StartingMetalOre | |
| Wolframite | 0.134 | 15.0 | 150 | 0.8 | 1840 kg | 3200 K (2926.85 C) | Molten Tungsten | Tungsten | 0.65 | Plumbable, BuildableAny | |
| Fool's Gold | 0.386 | 4.5 | 25 | 0.7 | 1840 kg | 1357 K (1083.85 C) | Molten Iron | Iron | 0.7 | Ore, BuildableAny | |
| Cobaltite | 0.42 | 4.0 | 25 | 0.9 | 6300 kg | 1768 K (1494.85 C) | Molten Cobalt | Cobalt | 0.58 | Ore, BuildableAny, StartingMetalOre | |
| Cinnabar | 0.386 | 4.5 | 25 | 0.7 | 1840 kg | 856.65 K (583.5 C) | Mercury Gas | Solid Mercury | 0.47 | Ore, BuildableAny, StartingMetalOre | |
| Nickel Ore | 0.45 | 3.0 | 30 | 0.7 | 1840 kg | 1728 K (1454.85 C) | Molten Nickel | Nickel | 0.62 | Ore, BuildableAny, StartingMetalOre | |
| Uranium Ore | 1.0 | 20.0 | 150 | 1.0 | 200 kg | 406 K (132.85 C) | Molten Uranium | -- | 0.3 | BuildableAny, Metal, Noncrushable | SP |
| Electrum | 0.15 | 2.0 | 2 | 0.7 | 1840 kg | 1337 K (1063.85 C) | Molten Gold | -- | 0.35 | Ore, BuildableAny (Disabled) | |

## Refined Metals (RefinedMetal)

Smelted metals for high-quality construction.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Copper | 0.385 | 60.0 | 25 | 0.8 | 3870 kg | 1357 K (1083.85 C) | Molten Copper | 0.61 | BuildableAny, StartingRefinedMetal | |
| Iron | 0.449 | 55.0 | 50 | 1.0 | 7870 kg | 1808 K (1534.85 C) | Molten Iron | 0.66 | BuildableAny | |
| Gold | 0.129 | 60.0 | 2 | 0.7 | 9970 kg | 1337 K (1063.85 C) | Molten Gold | 0.35 | BuildableAny | |
| Aluminum | 0.91 | 205.0 | 25 | 1.0 | 1000 kg | 933.45 K (660.3 C) | Molten Aluminum | 0.77 | BuildableAny, StartingRefinedMetal | |
| Tungsten | 0.134 | 60.0 | 200 | 0.9 | 1000 kg | 3695 K (3421.85 C) | Molten Tungsten | 0.35 | Plumbable, BuildableAny | |
| Lead | 0.128 | 35.0 | 10 | 0.8 | 2000 kg | 600.65 K (327.5 C) | Molten Lead | 0.85 | BuildableAny | |
| Cobalt | 0.42 | 100.0 | 75 | 0.9 | 8900 kg | 1768 K (1494.85 C) | Molten Cobalt | 0.63 | BuildableAny, StartingRefinedMetal | |
| Solid Mercury | 0.14 | 8.3 | 2 | 0.5 | 1000 kg | 234.3 K (-38.85 C) | Mercury | 0.25 | BuildableAny | |
| Depleted Uranium | 1.0 | 20.0 | 250 | 1.0 | 200 kg | 406 K (132.85 C) | Molten Uranium | 0.85 | BuildableAny, RefinedMetal | SP |
| Nickel | 0.44 | 91.0 | 30 | 0.8 | 2000 kg | 1728 K (1454.85 C) | Molten Nickel | 0.7 | BuildableAny, StartingRefinedMetal | |
| Iridium | 0.131 | 170.0 | 200 | 2.0 | 10000 kg | 2719 K (2445.85 C) | Molten Iridium | 0.88 | Metal, RefinedMetal, BuildableAny | |

Depleted Uranium emits radiation: 50 per 1000 kg.

## Manufactured Materials (ManufacturedMaterial)

Crafted or processed materials.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Steel | 0.49 | 54.0 | 50 | 2.0 | 10000 kg | 2700 K (2426.85 C) | Molten Steel | 0.74 | Metal, RefinedMetal, BuildableAny | |
| Glass | 0.84 | 1.11 | 10 | 1.0 | 1840 kg | 1700 K (1426.85 C) | Molten Glass | 0.65 | BuildableAny, Transparent | |
| Polypropylene (Plastic) | 1.92 | 0.15 | 1 | 0.4 | 913 kg | 433 K (159.85 C) | Naphtha | 0.85 | Antiseptic, Plastic, BuildableAny | |
| Hard Polypropylene | 1.5 | 0.25 | 1 | 0.4 | 913 kg | 2100 K (1826.85 C) | Sour Gas | 0.85 | Antiseptic, Plastic, BuildableAny | |
| Enriched Uranium | 1.0 | 20.0 | 250 | 1.0 | 200 kg | 1132 K (858.85 C) | Molten Uranium | 0.3 | | SP |
| Super Insulator (Insulation) | 5.57 | 0.00001 | 200 | 2.0 | 1800 kg | 3895 K (3621.85 C) | Molten Tungsten + Sour Gas (15%) | 0.6 | BuildableRaw, Plumbable, Crushable, BuildableAny, Insulator | |
| Thermium (Temp Conductor Solid) | 0.622 | 220.0 | 80 | 0.8 | 1800 kg | 2950 K (2676.85 C) | Molten Niobium + Tungsten (95%) | 0.6 | Metal, RefinedMetal, Plumbable, BuildableAny | |
| Solid Super Coolant | 8.44 | 9.46 | 2 | 0.1 | 1840 kg | 2 K (-271.15 C) | Super Coolant | 0.6 | | |
| Solid Visco-Gel | 1.55 | 0.45 | 2 | 0.1 | 150 kg | 242.5 K (-30.65 C) | Visco-Gel | 0.6 | Plastic | |

Enriched Uranium emits radiation: 250 per 1000 kg.

Super Insulator melts into Molten Tungsten (85% of mass) plus Sour Gas (15% of mass). Thermium melts into Molten Niobium (5% of mass) plus Tungsten (95% of mass).

## Organic (Organics)

Biological materials.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Sublimation | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|-------------|------|-----|
| Algae | 0.2 | 2.0 | 2 | 0.2 | 300 kg | 398.15 K (125 C) | Dirt | 0.65 | -- | | |
| Slime (Slime Mold) | 0.2 | 2.0 | 2 | 0.2 | 300 kg | 398.15 K (125 C) | Dirt | 0.65 | Polluted Oxygen (entity config) | FlyingCritterEdible | |
| Polluted Dirt (Toxic Sand) | 0.83 | 2.0 | 10 | 0.25 | 1840 kg | 1986 K (1712.85 C) | Molten Glass | 0.7 | Polluted Oxygen | Compostable | |
| Toxic Mud | 0.83 | 2.0 | 10 | 0.25 | 1840 kg | 372.84 K (99.69 C) | Steam + Polluted Dirt (40%) | 0.7 | Polluted Oxygen | Unstable | |
| Mud | 0.83 | 2.0 | 10 | 0.25 | 1840 kg | 372.84 K (99.69 C) | Steam + Dirt (40%) | 0.7 | -- | Unstable | |
| Wood Log | 2.3 | 0.22 | 18 | 0.7 | 1840 kg | 873.15 K (600 C) | Carbon Dioxide | 0.6 | -- | HideFromSpawnTool, BuildingWood, IndustrialIngredient | |
| Fabricated Wood | 2.3 | 0.35 | 18 | 0.7 | 1840 kg | 823.15 K (550 C) | Carbon Dioxide | 0.58 | -- | HideFromSpawnTool, BuildingWood, IndustrialIngredient | |
| Solid Resin (Isosap) | 1.3 | 0.17 | 10 | 0.2 | 1850 kg | 293.15 K (20 C) | Resin | 0.75 | -- | BuildableAny, Transparent | |
| Natural Solid Resin | 1.3 | 0.17 | 10 | 0.2 | 1850 kg | 293.15 K (20 C) | Natural Resin | 0.75 | -- | BuildableAny, Transparent | |
| Amber | 1.3 | 0.17 | 12 | 0.2 | 1850 kg | 368.15 K (95 C) | Natural Resin + Fossil (20%) | 0.75 | -- | BuildableAny, Transparent, Fossils | |
| Gunk | 1.2 | 1.5 | 10 | 0.25 | 2500 kg | 265 K (-8.15 C) | Liquid Gunk | 0.9 | -- | Slippery, Unstable | |

### Slime Sublimation (Entity Config)

Slime emits Polluted Oxygen when stored as a debris item. From `SlimeMoldConfig`: rate 0.025 kg/s, minimum 0.125 kg, max destination mass 1.8 kg, mass power 0 (constant rate regardless of mass).

### Polluted Dirt Sublimation (YAML)

From YAML: sublimateId ContaminatedOxygen, rate 0.1, efficiency 0.2, probability 0.05.

From entity config (`ToxicSandConfig`): rate 0.00002 kg/s, minimum 0.05 kg, max destination mass 1.8 kg, mass power 0.5.

### Toxic Mud Sublimation (YAML)

sublimateId ContaminatedOxygen, rate 0.1, efficiency 0.2, probability 0.05. Also a composite element: 40% Polluted Dirt, 60% Dirty Water.

### Mud Composition

Composite element: 40% Dirt, 60% Water.

## Sublimating Elements (Sublimating category)

Elements whose primary function involves off-gassing.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Sublimation Product | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|---------------------|------|-----|
| Oxylite (OxyRock) | 1.0 | 4.0 | 10 | 1.0 | 500 kg | 1683 K (1409.85 C) | Magma | 0.82 | Oxygen | FlyingCritterEdible, Oxidizer | |
| Bleach Stone | 0.5 | 4.0 | 50 | 1.0 | 500 kg | 942 K (668.85 C) | Chlorine | 0.73 | Chlorine Gas | FlyingCritterEdible | |

### Oxylite Sublimation

YAML: sublimateId Oxygen, rate 0.4, efficiency 0.5, probability 1.

Entity config (`OxyRockConfig`): rate 0.01 kg/s, minimum 0.005 kg, max destination mass 1.8 kg, mass power 0.7.

### Bleach Stone Sublimation

YAML defines no sublimateId. Sublimation is set entirely in entity config (`BleachStoneConfig`): rate 0.0002 kg/s, minimum 0.0025 kg, max destination mass 1.8 kg, mass power 0.5, product is Chlorine Gas.

## Consumable Ore (ConsumableOre)

Resources consumed by buildings and processes.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Carbon (Coal) | 0.71 | 1.25 | 2 | 0.8 | 4000 kg | 550 K (276.85 C) | Refined Carbon | 0.84 | Coal, BuildableAny | |
| Peat | 0.71 | 0.6 | 2 | 0.5 | 4000 kg | 500 K (226.85 C) | Carbon | 0.75 | BuildableAny | |
| Refined Carbon | 1.74 | 3.1 | 2 | 0.8 | 4000 kg | 4600 K (4326.85 C) | Molten Carbon | 0.84 | Insulator | |
| Lime | 0.834 | 2.0 | 50 | 0.2 | 1840 kg | 1330 K (1056.85 C) | Magma | 0.75 | | |
| Rust | 0.449 | 4.0 | 25 | 0.9 | 1840 kg | 1808 K (1534.85 C) | Molten Iron | 0.7 | | |
| Salt | 0.7 | 0.444 | 5 | 0.1 | 2000 kg | 1073 K (799.85 C) | Molten Salt | 0.75 | | |
| Solid Methane | 2.191 | 0.03 | 2 | 1.0 | 750 kg | 90.55 K (-182.6 C) | Liquid Methane | 0.75 | | |
| Sucrose (Sugar) | 1.255 | 0.15 | 5 | 0.1 | 2000 kg | 459 K (185.85 C) | Molten Sucrose | 0.7 | | |
| Milk Fat | 1.92 | 0.15 | 1 | 0.4 | 913 kg | 433 K (159.85 C) | Naphtha | 0.85 | Slippery | |

## Agriculture (Agriculture)

Materials used in farming.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Fertilizer | 0.83 | 2.0 | 2 | 0.2 | 300 kg | 398.15 K (125 C) | Dirt | 0.7 | Oxidizer | |
| Phosphorite | 0.15 | 2.0 | 25 | 0.8 | 1840 kg | 517 K (243.85 C) | Liquid Phosphorus | 0.75 | FlyingCritterEdible | |
| Phosphate Nodules | 0.15 | 2.0 | 25 | 0.8 | 1840 kg | 700 K (426.85 C) | Liquid Phosphorus | 0.75 | (Disabled) | |

## Farmable (Farmable)

Soil-type materials for planting.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Dirt | 1.48 | 2.0 | 2 | 0.2 | 1840 kg | 600 K (326.85 C) | Sand | 0.75 | BuildableAny | |
| Clay | 0.92 | 2.0 | 5 | 0.2 | 1840 kg | 1200 K (926.85 C) | Ceramic | 0.65 | BuildableAny | |

## Filter Materials (Filter)

Used in filtration buildings.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Sand | 0.83 | 2.0 | 10 | 0.25 | 1840 kg | 1986 K (1712.85 C) | Molten Glass | 0.7 | Unstable | |
| Regolith | 0.2 | 1.0 | 2 | 0.5 | 1000 kg | 1683 K (1409.85 C) | Magma | 0.6 | Unstable | |

## Ice and Frozen Liquids (Liquifiable)

Solid forms of liquids that melt at relatively low temperatures.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Ice | 2.05 | 2.18 | 25 | 1.0 | 1100 kg | 272.5 K (-0.65 C) | Water | 0.8 | Slippery, IceOre, BuildableAny | |
| Brine Ice | 3.4 | 2.18 | 25 | 1.0 | 1100 kg | 256.65 K (-16.5 C) | Brine | 0.8 | Slippery, IceOre, BuildableAny | |
| Milk Ice | 3.4 | 2.18 | 25 | 1.0 | 1100 kg | 256.65 K (-16.5 C) | Milk | 0.8 | Slippery, IceOre, BuildableAny | |
| Dirty Ice | 3.05 | 1.0 | 10 | 1.0 | 800 kg | 252.5 K (-20.65 C) | Dirty Water | 0.75 | Slippery, IceOre, Mixture, BuildableAny | |
| Crushed Ice | 2.05 | 2.18 | 10 | 0.4 | 20 kg | 272.5 K (-0.65 C) | Water | 0.7 | IceOre, Unstable | |
| Snow | 2.05 | 0.545 | 10 | 0.3 | 20 kg | 272.5 K (-0.65 C) | Water | 0.7 | IceOre, Unstable | |
| Stable Snow | 2.05 | 0.545 | 10 | 0.3 | 20 kg | 272.5 K (-0.65 C) | Water | 0.7 | IceOre, HideFromSpawnTool, HideFromCodex | |
| Solid Carbon Dioxide | 0.846 | 1.46 | 2 | 0.6 | 2000 kg | 216.6 K (-56.55 C) | Liquid CO2 | 0.8 | | |
| Solid Chlorine | 0.48 | 0.75 | 25 | 0.5 | 1000 kg | 172.17 K (-100.98 C) | Chlorine (liquid) | 0.73 | | |
| Solid Crude Oil | 1.69 | 2.0 | 2 | 0.1 | 1840 kg | 233 K (-40.15 C) | Crude Oil | 0.8 | | |
| Solid Hydrogen | 2.4 | 1.0 | 2 | 0.25 | 1000 kg | 14 K (-259.15 C) | Liquid Hydrogen | 0.9 | | |
| Solid Naphtha | 2.191 | 0.2 | 2 | 0.1 | 1840 kg | 223 K (-50.15 C) | Naphtha | 0.6 | | |
| Solid Oxygen | 1.01 | 1.0 | 2 | 0.5 | 1000 kg | 54.36 K (-218.79 C) | Liquid Oxygen | 0.82 | | |
| Solid Petroleum | 1.76 | 2.0 | 2 | 0.1 | 1840 kg | 216 K (-57.15 C) | Petroleum | 0.8 | | |
| Solid Propane | 2.4 | 1.0 | 10 | 0.5 | 1000 kg | 85 K (-188.15 C) | Liquid Propane | 0.75 | BuildableAny, HideFromSpawnTool, HideFromCodex | |
| Solid Ethanol | 2.46 | 20.0 | 250 | 1.0 | 200 kg | 159.1 K (-114.05 C) | Ethanol | 0.7 | IceOre, BuildableAny | |
| Solid Syngas | 2.4 | 1.0 | 2 | 0.25 | 1000 kg | 14 K (-259.15 C) | Molten Syngas | 0.7 | (Disabled) | |
| Frozen Phyto Oil | 0.9 | 2.0 | 2 | 0.2 | 870 kg | 240 K (-33.15 C) | Phyto Oil | 0.9 | Slippery | |
| Tallow | 2.19 | 10.0 | 15 | 1.0 | 2500 kg | 353.15 K (80 C) | Refined Lipid | 0.7 | Slippery | |

Note: Solid Mercury's materialCategory is RefinedMetal, so it appears in the Refined Metals section rather than here, despite being a frozen liquid. Solid Methane's materialCategory is ConsumableOre, so it appears in that section.

## Rare Materials (RareMaterials)

Rare elements found in limited quantities, often in space.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Niobium | 0.265 | 54.0 | 50 | 0.8 | 50 kg | 2750 K (2476.85 C) | Molten Niobium | 0.49 | Metal, RefinedMetal, BuildableAny | |
| Isoresin | 1.3 | 0.17 | 10 | 0.4 | 50 kg | 473.15 K (200 C) | Naphtha | 0.75 | BuildableRaw | |
| Fullerene | 0.95 | 50.0 | 250 | 1.0 | 50 kg | 4200 K (3926.85 C) | Molten Carbon | 0.6 | | |

## Other (Other category)

Elements that don't fit standard material categories.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Diamond | 0.516 | 80.0 | 250 | 2.5 | 1840 kg | 4200 K (3926.85 C) | Molten Carbon | 0.8 | BuildableAny, Transparent, FlyingCritterEdible | |
| Katairite (Abyssalite) | 4.0 | 0.00001 | 150 | 2.0 | 3200 kg | 3695 K (3421.85 C) | Molten Tungsten | 0.9 | | |
| Sulfur | 0.7 | 0.2 | 5 | 0.1 | 2000 kg | 388.35 K (115.2 C) | Liquid Sulfur | 0.74 | | |
| Phosphorus | 0.7697 | 0.236 | 0 | 0.85 | 1000 kg | 317.3 K (44.15 C) | Liquid Phosphorus | 0.75 | FlyingCritterEdible | |
| Solid Nuclear Waste | 7.44 | 6.0 | 0 | 0 | 9970 kg | 300 K (26.85 C) | Nuclear Waste | 0.3 | EmitsLight | SP |
| Corium | 7.44 | 6.0 | 251 | 0.45 | 200 kg | 900 K (626.85 C) | Fallout | 0.3 | | SP |
| Radium | 1.0 | 20.0 | 250 | 1.0 | 200 kg | 1233 K (959.85 C) | Magma | 0.25 | | SP |

Solid Nuclear Waste emits radiation: 150 per 1000 kg. Strength 0 means unbreakable.

Corium sublimation: sublimateId Nuclear Waste (liquid), rate 0.4, efficiency 0.5, probability 1.

## Processed Building Materials (BuildableProcessed)

Materials created through industrial processes. Most are disabled except Crushed Rock.

| Element | SHC | TC | Hardness | Strength | Max Mass | Melts at | Melts into | Rad Absorb | Tags | DLC |
|---------|-----|-----|----------|----------|----------|----------|------------|------------|------|-----|
| Brick | 0.84 | 0.62 | 50 | 1.0 | 2000 kg | 1683 K (1409.85 C) | Magma | 0.8 | BuildableAny (Disabled) | |
| Cement | 1.55 | 8.0 | 200 | 1.0 | 2000 kg | 1683 K (1409.85 C) | Magma | 0.8 | BuildableAny (Disabled) | |
| Sand Cement | 1.5 | 8.0 | 50 | 1.0 | 2000 kg | 1683 K (1409.85 C) | Magma | 0.7 | BuildableAny (Disabled) | |
| Slabs | 0.52 | 8.0 | 50 | 1.0 | 2000 kg | 1683 K (1409.85 C) | Magma | 0.7 | BuildableAny (Disabled) | |
| Crushed Rock | 0.2 | 2.0 | 10 | 0.7 | 1840 kg | 1683 K (1409.85 C) | Magma | 0.7 | Unstable | |
| Cement Mix | 0.52 | 8.0 | 10 | 1.0 | 2000 kg | 1683 K (1409.85 C) | Magma | 0.8 | BuildableAny (Disabled) | |

Note: Crushed Rock is not disabled and is available in normal gameplay. It uses the BuildableProcessed category but has no BuildableAny tag, so it cannot be used as a construction material.

## Special Elements

### Neutronium (Unobtanium)

The indestructible material forming the bottom boundary of the map and surrounding certain geysers.

| Property | Value |
|----------|-------|
| SHC | 0 |
| TC | 0 (temperature insulated) |
| Hardness | 255 (cannot be dug) |
| Strength | 0 (unbreakable) |
| Max Mass | 20000 kg |
| Default Mass | 20000 kg |
| Default Temperature | 1 K (-272.15 C) |
| Melts at | 10000 K (transitions to itself) |
| Surface Area Multipliers | All 0 (no heat exchange) |
| Radiation Absorption | 0.9 |
| Material Category | Special |

### Creature

An internal element representing living creatures. Not available to players.

| Property | Value |
|----------|-------|
| SHC | 3.47 |
| TC | 0.6 |
| Hardness | 10 |
| Strength | 1.0 |
| Max Mass | 200 kg |
| Default Temperature | 310 K (36.85 C) |
| Melts at | 10000 K (transitions to itself) |
| Tags | HideFromSpawnTool |

### Bitumen

A byproduct element that cannot be used as construction material. Hidden from spawn tool and codex.

| Property | Value |
|----------|-------|
| SHC | 1.76 |
| TC | 0.17 |
| Hardness | 2 |
| Strength | 0.1 |
| Max Mass | 1840 kg |
| Default Mass | 740 kg |
| Melts at | 812 K (538.85 C) |
| Melts into | Carbon Dioxide |
| Radiation Absorption | 0.85 |
| Tags | HideFromSpawnTool, HideFromCodex |

## Disabled Elements

The following elements exist in the data files but have `isDisabled: true` and are not available in normal gameplay:

| Element | Category | Notes |
|---------|----------|-------|
| Aerogel | (none) | SHC 1.0, TC 0.003, melts at 10000 K into Katairite |
| Carbon Fibre | (none) | SHC 0.52, TC 0 (insulated), melts at 5000 K into Molten Carbon, hardness 250 |
| Brick | BuildableProcessed | SHC 0.84, TC 0.62 |
| Cement | BuildableProcessed | SHC 1.55, TC 8.0, hardness 200 |
| Cement Mix | (none) | SHC 0.52, TC 8.0 |
| Sand Cement | BuildableProcessed | SHC 1.5, TC 8.0 |
| Slabs | BuildableProcessed | SHC 0.52, TC 8.0 |
| Electrum | Metal | SHC 0.15, TC 2.0 |
| Phosphate Nodules | Agriculture | SHC 0.15, TC 2.0 |
| Yellowcake | (none) | SHC 1.0, TC 20.0, hardness 250, Spaced Out! DLC |
| Solid Syngas | Liquifiable | SHC 2.4, TC 1.0 |

## Element Flags Reference

These flags are set automatically based on element properties during `ElementLoader.FinaliseElementsTable()`:

- **TemperatureInsulated**: Set when `thermalConductivity == 0`. Elements with this flag do not exchange heat. Applies to: Neutronium, Carbon Fibre.
- **Unbreakable**: Set when `strength == 0`. Elements with this flag cannot be broken by pressure or damage. Applies to: Neutronium, Solid Nuclear Waste.
- **Unstable**: Tagged elements fall like sand when unsupported. Applies to: Sand, Regolith, Snow, Crushed Ice, Crushed Rock, Toxic Mud, Mud, Gunk.

## Thermal Conductivity Extremes

For quick reference, the solid elements with the highest and lowest thermal conductivity:

**Highest TC (fastest heat transfer):**
1. Thermium: 220.0
2. Aluminum: 205.0
3. Iridium: 170.0
4. Cobalt: 100.0
5. Nickel: 91.0

**Lowest TC (best insulation):**
1. Katairite (Abyssalite): 0.00001
2. Super Insulator (Insulation): 0.00001
3. Aerogel: 0.003 (disabled)
4. Solid Methane: 0.03
5. Polypropylene (Plastic): 0.15
