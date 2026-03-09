# Gas Elements

Complete reference for all 31 gas-state elements in Oxygen Not Included, plus the pseudo-elements Vacuum and Void. Values are sourced from the game's element YAML definitions and ElementLoader.cs.

All gases share certain properties enforced by ElementLoader during loading:
- **Max mass per cell**: 1.8 kg (overrides any YAML value)
- **Default mass**: 1 kg (overrides any YAML value)
- **solidSurfaceAreaMultiplier**: 25 (defined per element but only applies when transitioning)
- **liquidSurfaceAreaMultiplier**: 1 (same)
- **gasSurfaceAreaMultiplier**: 1 (all gases use 1)

Temperature transitions for gases only go downward (condensation). The lowTemp value is the condensation point; there is no highTemp transition for gases.

## Breathable Gases

Only two gases are tagged Breathable. All others are tagged Unbreathable.

| Property | Oxygen | Polluted Oxygen |
|----------|--------|-----------------|
| Element ID | Oxygen | ContaminatedOxygen |
| SHC (DTU/g/K) | 1.005 | 1.01 |
| Thermal Conductivity (DTU/(m*s)/K) | 0.024 | 0.024 |
| Flow | 0.12 | 0.12 |
| Molar Mass (g/mol) | 15.9994 | 15.9994 |
| Condensation Point | 90.19 K (-182.96 C) | 90.19 K (-182.96 C) |
| Condenses Into | Liquid Oxygen | Liquid Oxygen |
| Default Temperature | 300 K (26.85 C) | 300 K (26.85 C) |
| Default Pressure | 101.3 | 101.3 |
| Light Absorption | 0 | 0.1 |
| Radiation Absorption | 0.08 | 0.08 |
| Toxicity | 0 | 0.01 |
| DLC | Base game | Base game |

Polluted Oxygen condenses into Liquid Oxygen (not polluted water). Polluted Oxygen has a higher flow rate (0.12) than most gases (0.1), matching Oxygen.

## Common Industrial Gases

Gases regularly encountered through normal gameplay.

| Property | Carbon Dioxide | Chlorine | Hydrogen | Natural Gas | Steam |
|----------|---------------|----------|----------|-------------|-------|
| Element ID | CarbonDioxide | ChlorineGas | Hydrogen | Methane | Steam |
| SHC (DTU/g/K) | 0.846 | 0.48 | 2.4 | 2.191 | 4.179 |
| Thermal Cond. | 0.0146 | 0.0081 | 0.168 | 0.035 | 0.184 |
| Flow | 0.1 | 0.1 | 0.1 | 0.1 | 0.1 |
| Molar Mass (g/mol) | 44.01 | 34.453 | 1.00794 | 16.044 | 18.01528 |
| Condensation Point | 225 K (-48.15 C) | 238.55 K (-34.6 C) | 21 K (-252.15 C) | 111.65 K (-161.5 C) | 372.5 K (99.35 C) |
| Condenses Into | Liquid CO2 | Chlorine (liquid) | Liquid Hydrogen | Liquid Methane | Water |
| Default Temp | 300 K | 300 K | 300 K | 300 K | 400 K |
| Default Pressure | 139 | 228 | 7 | 7 | 57 |
| Light Absorption | 0.1 | 0.2 | 0.1 | 0.25 | 0.1 |
| Radiation Absorption | 0.08 | 0.07 | 0.09 | 0.07 | 0.08 |
| Toxicity | 0.0001 | 0 | 0 | 0 | 0 |
| Tags | - | - | - | CombustibleGas | - |
| DLC | Base game | Base game | Base game | Base game | Base game |

Steam has the highest SHC among common gases at 4.179 DTU/g/K, making it an effective heat carrier (only Super Coolant Gas is higher at 8.44). Hydrogen has the lowest molar mass, causing it to rise above all other gases.

## Sour Gas and Ethanol Gas

| Property | Sour Gas | Ethanol Gas |
|----------|----------|-------------|
| Element ID | SourGas | EthanolGas |
| SHC (DTU/g/K) | 1.898 | 2.148 |
| Thermal Conductivity | 0.018 | 0.167 |
| Flow | 0.1 | 0.1 |
| Molar Mass (g/mol) | 19.044 | 46.07 |
| Condensation Point | 111.65 K (-161.5 C) | 351.5 K (78.35 C) |
| Condenses Into | Liquid Methane + Sulfur | Ethanol |
| Low Temp Ore | Sulfur (33%) | - |
| Default Temperature | 300 K (26.85 C) | 390 K (116.85 C) |
| Default Pressure | 7 | 7 |
| Light Absorption | 0.25 | 0.5 |
| Radiation Absorption | 0.05 | 0.07 |
| DLC | Base game | Base game |

Sour Gas is notable for producing a secondary product when condensing: 33% of its mass becomes Sulfur, with the remaining 67% becoming Liquid Methane. Both products form at the same condensation point of 111.65 K.

## Sulfur Gas and Phosphorus Gas

| Property | Sulfur Gas | Phosphorus Gas |
|----------|------------|----------------|
| Element ID | SulfurGas | PhosphorusGas |
| SHC (DTU/g/K) | 0.7 | 0.7697 |
| Thermal Conductivity | 0.2 | 0.236 |
| Flow | 0.1 | 0.1 |
| Molar Mass (g/mol) | 32 | 30.973762 |
| Condensation Point | 610.15 K (337 C) | 553.6 K (280.45 C) |
| Condenses Into | Liquid Sulfur | Liquid Phosphorus |
| Default Temperature | 700 K (426.85 C) | 700 K (426.85 C) |
| Default Pressure | 7 | 98 |
| Light Absorption | 0.1 | 0.5 |
| Radiation Absorption | 0.07 | 0.07 |
| Tags | - | EmitsLight |
| DLC | Base game | Base game |

## Super Coolant Gas and Mercury Gas

| Property | Super Coolant Gas | Mercury Gas |
|----------|-------------------|-------------|
| Element ID | SuperCoolantGas | MercuryGas |
| SHC (DTU/g/K) | 8.44 | 0.14 |
| Thermal Conductivity | 1.2 | 8.3 |
| Flow | 0.1 | 0.1 |
| Molar Mass (g/mol) | 190 | 200.59 |
| Condensation Point | 710 K (436.85 C) | 629.9 K (356.75 C) |
| Condenses Into | Super Coolant | Mercury |
| Default Temperature | 850 K (576.85 C) | 850 K (576.85 C) |
| Default Pressure | 550 | 633 |
| Light Absorption | 0.5 | 0.5 |
| Radiation Absorption | 0.06 | 0.02 |
| Tags | - | RefinedMetal |
| DLC | Base game | Base game |

Super Coolant Gas has the highest SHC of any gas at 8.44 DTU/g/K (tied with its liquid form). Mercury Gas has the highest thermal conductivity of any gas at 8.3 DTU/(m*s)/K.

## Salt Gas and Rock Gas

| Property | Salt Gas | Rock Gas |
|----------|----------|----------|
| Element ID | SaltGas | RockGas |
| SHC (DTU/g/K) | 0.88 | 1 |
| Thermal Conductivity | 0.444 | 0.1 |
| Flow | 0.1 | 0.1 |
| Molar Mass (g/mol) | 50 | 50 |
| Condensation Point | 1738 K (1464.85 C) | 2630 K (2356.85 C) |
| Condenses Into | Molten Salt | Magma |
| Default Temperature | 3000 K (2726.85 C) | 3000 K (2726.85 C) |
| Default Pressure | 1076 | 1076 |
| Light Absorption | 0.1 | 0.5 |
| Radiation Absorption | 0.07 | 0.07 |
| Tags | - | EmitsLight |
| DLC | Base game | Base game |

## Molten Metal Gases

Gaseous forms of metals, produced at extreme temperatures. All are Unbreathable and have a flow rate of 0.1.

| Property | Aluminum Gas | Copper Gas | Gold Gas | Iron Gas |
|----------|-------------|------------|----------|----------|
| Element ID | AluminumGas | CopperGas | GoldGas | IronGas |
| SHC (DTU/g/K) | 0.91 | 0.386 | 0.1291 | 0.449 |
| Thermal Cond. | 2.5 | 1 | 1 | 1 |
| Molar Mass (g/mol) | 63.546 | 63.546 | 196.966569 | 55.845 |
| Condensation Point | 2743.15 K (2470 C) | 2834 K (2560.85 C) | 3129 K (2855.85 C) | 3023 K (2749.85 C) |
| Condenses Into | Molten Aluminum | Molten Copper | Molten Gold | Molten Iron |
| Default Temp | 3200 K | 3200 K | 3500 K | 4000 K |
| Default Pressure | 202 | 202 | 624 | 177 |
| Light Absorption | 0.5 | 0.5 | 0.5 | 0.5 |
| Radiation Abs. | 0.07 | 0.06 | 0.03 | 0.06 |
| Tags | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal, EmitsLight |
| DLC | Base game | Base game | Base game | Base game |

| Property | Lead Gas | Cobalt Gas | Nickel Gas | Steel Gas |
|----------|----------|------------|------------|-----------|
| Element ID | LeadGas | CobaltGas | NickelGas | SteelGas |
| SHC (DTU/g/K) | 0.128 | 0.42 | 0.44 | 0.49 |
| Thermal Cond. | 3.5 | 1 | 1 | 1 |
| Molar Mass (g/mol) | 196.966569 | 58.9 | 58.69 | 54.97 |
| Condensation Point | 2022.15 K (1749 C) | 3200 K (2926.85 C) | 3003 K (2729.85 C) | 4100 K (3826.85 C) |
| Condenses Into | Molten Lead | Molten Cobalt | Molten Nickel | Molten Steel |
| Default Temp | 3500 K | 4000 K | 3200 K | 4500 K |
| Default Pressure | 624 | 177 | 202 | 1076 |
| Light Absorption | 0.5 | 0.5 | 0.5 | 0.5 |
| Radiation Abs. | 0.08 | 0.06 | 0.05 | 0.07 |
| Tags | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal, Alloy, EmitsLight |
| DLC | Base game | Base game | Base game | Base game |

| Property | Niobium Gas | Tungsten Gas | Iridium Gas |
|----------|-------------|--------------|-------------|
| Element ID | NiobiumGas | TungstenGas | IridiumGas |
| SHC (DTU/g/K) | 0.265 | 0.134 | 0.131 |
| Thermal Cond. | 1 | 1 | 1 |
| Molar Mass (g/mol) | 92.9 | 183.84 | 183.84 |
| Condensation Point | 5017 K (4743.85 C) | 6203 K (5929.85 C) | 4403 K (4129.85 C) |
| Condenses Into | Molten Niobium | Molten Tungsten | Molten Iridium |
| Default Temp | 5500 K | 6500 K | 4500 K |
| Default Pressure | 500 | 581.9685 | 1076 |
| Light Absorption | 0.5 | 0.5 | 0.5 |
| Radiation Abs. | 0.05 | 0.03 | 0.88 |
| Tags | Metal, RefinedMetal, EmitsLight | Metal, RefinedMetal | Metal, RefinedMetal |
| DLC | Base game | Base game | Base game |

Tungsten Gas has the highest condensation point of any gas at 6203 K. Iridium Gas has by far the highest radiation absorption factor among gases at 0.88. Lead Gas has both the lowest condensation point among metal gases at 2022.15 K and the highest thermal conductivity at 3.5, followed by Aluminum Gas at 2.5.

## Fallout (DLC: Spaced Out!)

| Property | Value |
|----------|-------|
| Element ID | Fallout |
| SHC (DTU/g/K) | 0.265 |
| Thermal Conductivity | 1 |
| Flow | 0.1 |
| Molar Mass (g/mol) | 92.9 |
| Condensation Point | 340 K (66.85 C) |
| Condenses Into | Nuclear Waste |
| Default Temperature | 500 K (226.85 C) |
| Default Pressure | 500 |
| Light Absorption | 0.5 |
| Radiation Absorption | 0.03 |
| Radiation Per 1000 kg | 15 |
| Tags | EmitsLight |
| DLC | Spaced Out! (EXPANSION1_ID) |

Fallout is the only gas that emits radiation (radiationPer1000Mass = 15). It condenses into Nuclear Waste at a relatively low 340 K.

## Disabled Gases

These gases exist in the data files but are flagged `isDisabled: true`. They cannot appear in normal gameplay.

| Property | Helium | Propane | Syngas |
|----------|--------|---------|--------|
| Element ID | Helium | Propane | Syngas |
| SHC (DTU/g/K) | 0.14 | 2.4 | 2.4 |
| Thermal Conductivity | 0.236 | 0.015 | 0.168 |
| Flow | 0.1 | 0.1 | 0.1 |
| Molar Mass (g/mol) | 4 | 44.1 | 26.96 |
| Condensation Point | 4.22 K (-268.93 C) | 231 K (-42.15 C) | 21 K (-252.15 C) |
| Condenses Into | Liquid Helium | Liquid Propane | Molten Syngas |
| Default Temperature | 300 K | 350 K | 320 K |
| Default Pressure | 13 | 140 | 7 |
| Light Absorption | 0.1 | 0.25 | 0.1 |
| Radiation Absorption | 0.09 | 0.07 | 0.07 |
| Tags | - | HideFromSpawnTool, HideFromCodex | CombustibleGas |
| DLC | Base game | Base game | Base game |

Helium would have the lowest condensation point of any gas at 4.22 K if it were enabled. Propane is additionally hidden from the spawn tool and codex.

## Summary Tables

### All Gases Ranked by SHC

| Gas | SHC (DTU/g/K) |
|-----|---------------|
| Super Coolant Gas | 8.44 |
| Steam | 4.179 |
| Hydrogen | 2.4 |
| Propane (disabled) | 2.4 |
| Syngas (disabled) | 2.4 |
| Natural Gas | 2.191 |
| Ethanol Gas | 2.148 |
| Sour Gas | 1.898 |
| Polluted Oxygen | 1.01 |
| Oxygen | 1.005 |
| Rock Gas | 1 |
| Aluminum Gas | 0.91 |
| Salt Gas | 0.88 |
| Carbon Dioxide | 0.846 |
| Phosphorus Gas | 0.7697 |
| Carbon Gas | 0.71 |
| Sulfur Gas | 0.7 |
| Steel Gas | 0.49 |
| Chlorine | 0.48 |
| Iron Gas | 0.449 |
| Nickel Gas | 0.44 |
| Cobalt Gas | 0.42 |
| Copper Gas | 0.386 |
| Fallout | 0.265 |
| Niobium Gas | 0.265 |
| Helium (disabled) | 0.14 |
| Mercury Gas | 0.14 |
| Tungsten Gas | 0.134 |
| Iridium Gas | 0.131 |
| Gold Gas | 0.1291 |
| Lead Gas | 0.128 |

### All Gases Ranked by Thermal Conductivity

| Gas | TC (DTU/(m*s)/K) |
|-----|-----------------|
| Mercury Gas | 8.3 |
| Lead Gas | 3.5 |
| Aluminum Gas | 2.5 |
| Carbon Gas | 1.7 |
| Super Coolant Gas | 1.2 |
| Copper Gas | 1 |
| Gold Gas | 1 |
| Iron Gas | 1 |
| Cobalt Gas | 1 |
| Nickel Gas | 1 |
| Niobium Gas | 1 |
| Steel Gas | 1 |
| Tungsten Gas | 1 |
| Iridium Gas | 1 |
| Fallout | 1 |
| Salt Gas | 0.444 |
| Helium (disabled) | 0.236 |
| Phosphorus Gas | 0.236 |
| Sulfur Gas | 0.2 |
| Steam | 0.184 |
| Hydrogen | 0.168 |
| Syngas (disabled) | 0.168 |
| Ethanol Gas | 0.167 |
| Rock Gas | 0.1 |
| Natural Gas | 0.035 |
| Oxygen | 0.024 |
| Polluted Oxygen | 0.024 |
| Sour Gas | 0.018 |
| Carbon Dioxide | 0.0146 |
| Propane (disabled) | 0.015 |
| Chlorine | 0.0081 |

### All Gases Ranked by Condensation Point

| Gas | Condensation Point |
|-----|--------------------|
| Tungsten Gas | 6203 K (5929.85 C) |
| Carbon Gas | 5100 K (4826.85 C) |
| Niobium Gas | 5017 K (4743.85 C) |
| Iridium Gas | 4403 K (4129.85 C) |
| Steel Gas | 4100 K (3826.85 C) |
| Cobalt Gas | 3200 K (2926.85 C) |
| Gold Gas | 3129 K (2855.85 C) |
| Iron Gas | 3023 K (2749.85 C) |
| Nickel Gas | 3003 K (2729.85 C) |
| Copper Gas | 2834 K (2560.85 C) |
| Aluminum Gas | 2743.15 K (2470 C) |
| Rock Gas | 2630 K (2356.85 C) |
| Lead Gas | 2022.15 K (1749 C) |
| Salt Gas | 1738 K (1464.85 C) |
| Super Coolant Gas | 710 K (436.85 C) |
| Mercury Gas | 629.9 K (356.75 C) |
| Sulfur Gas | 610.15 K (337 C) |
| Phosphorus Gas | 553.6 K (280.45 C) |
| Steam | 372.5 K (99.35 C) |
| Ethanol Gas | 351.5 K (78.35 C) |
| Fallout | 340 K (66.85 C) |
| Chlorine | 238.55 K (-34.6 C) |
| Propane (disabled) | 231 K (-42.15 C) |
| Carbon Dioxide | 225 K (-48.15 C) |
| Natural Gas | 111.65 K (-161.5 C) |
| Sour Gas | 111.65 K (-161.5 C) |
| Oxygen | 90.19 K (-182.96 C) |
| Polluted Oxygen | 90.19 K (-182.96 C) |
| Hydrogen | 21 K (-252.15 C) |
| Syngas (disabled) | 21 K (-252.15 C) |
| Helium (disabled) | 4.22 K (-268.93 C) |

## Pseudo-Elements: Vacuum and Void

These are not true gases. They use Element.State = Vacuum (0x00) and have no physical properties.

| Property | Vacuum | Void |
|----------|--------|------|
| SimHash | 758759285 | -1456075980 |
| State | Vacuum (0x00) | Vacuum (0x00) |
| SHC | 0 | 0 |
| Thermal Conductivity | 0 | 0 |
| Max Mass | 0 | 0 |
| Default Mass | 0 | 0 |
| Default Temperature | 0 | 0 |
| Hardness | 0 | 0 |
| Strength | 0 | 0 |
| Radiation Absorption | 0 | 0 |
| Material Category | Special | Special |
| highTempTransitionTarget | Vacuum | Void |
| DLC | Base game | Base game |

**Vacuum** represents an empty cell with no matter. It has zero mass, zero temperature, and no thermal conductivity (thermally insulated). Cells become Vacuum when all mass is removed.

**Void** is used internally by the simulation engine as a boundary element. Void cells destroy any matter that enters them and absorb infinite amounts of fluid. They appear at the edges of the map and in certain debug/testing scenarios. Void transitions to itself, ensuring it never changes state.

Both have `state: Vacuum` in their YAML definitions, meaning `IsVacuum` returns true for both. The `thermalConductivity: 0` causes ElementLoader to add the TemperatureInsulated flag, and `strength: 0` causes it to add the Unbreakable flag.
