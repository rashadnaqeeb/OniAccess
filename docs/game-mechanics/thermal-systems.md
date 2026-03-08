# Thermal Systems

How heat transfers between cells, buildings, and creatures. Derived from decompiled source code.

## Core Heat Transfer Formula

Heat flows between adjacent cells based on this equation:

```
energyFlow = (T_source - T_dest) * min(k_source, k_dest) * (surfaceArea / thickness)
```

Where:
- `T_source`, `T_dest` = temperatures in Kelvin
- `k_source`, `k_dest` = thermal conductivity of each material
- `surfaceArea` = contact area (modified by state-specific multipliers)
- `thickness` = distance between cell centers

The simulation uses the **minimum** thermal conductivity of the two materials. This is the bottleneck principle: heat transfer is limited by the worst conductor in the pair.

## Temperature Change from Energy

```
deltaT = kilojoules / (specificHeatCapacity * mass)
```

Higher specific heat capacity means more energy is needed to change temperature. Higher mass also means more energy needed. This is why large pools of water are effective heat sinks.

## Temperature Mixing

The standard mixing function `SimUtil.CalculateFinalTemperature` is mass-weighted without SHC:

```
finalTemp = (mass1 * temp1 + mass2 * temp2) / (mass1 + mass2)
```

The result is clamped to `[min(temp1, temp2), max(temp1, temp2)]` to prevent numerical overshoot. This function is used throughout the game for pipe contents merging, element chunk absorption, storage consolidation, and turbine intake.

**Exception: ElementConverter** uses SHC-weighted mixing when computing output temperatures from consumed inputs (see buildings.md):
```
finalTemp = sum(mass * temp * SHC) / sum(mass * SHC)
```
This accounts for different materials having different heat capacities, which matters when a building consumes multiple element types simultaneously.

## Building Heat Exchange

Buildings participate in the thermal simulation as thermally conductive objects. When registered with the sim via `AddBuildingHeatExchange`, each building specifies:

- Its construction material (element index)
- Thermal mass (mass in kg)
- Current temperature
- Thermal conductivity
- Overheat temperature threshold
- Operating kilowatts (heat generated while running)
- Physical footprint (minX/minY to maxX/maxY)

Buildings exchange heat with **all cells in their footprint**. The operating kilowatts value represents heat generated during operation, which continuously raises the building's temperature.

### Building Exhaust Heat Distribution

When buildings exhaust heat, the formula distributes it across occupied cells:

```
exhaustKJ = (kW * dt / cellCount) * (min(mass, 1.5) / 1.5)
```

Cells with mass >= 1.5 kg get the full multiplier (1.0). Cells with less mass get a proportionally reduced share. This prevents dumping all exhaust heat into near-vacuum cells.

For area-effect heaters, cells are weighted by proximity:
```
cellWeight = 1 + (maxDistance - |dx| - |dy|)
```

### Building-to-Building Heat Transfer

Separate from cell-based transfer. Registered via `RegisterBuildingToBuildingHeatExchange`, this allows adjacent buildings to conduct heat directly between each other, creating thermal bridges.

## Insulation

**Element-level:** If `thermalConductivity == 0`, the element is flagged as `TemperatureInsulated` and does not register with the sim for heat exchange. Vacuum cells are thermally insulated.

**Building-level:** Pipes and tiles with `ThermalConductivity < 1.0` are flagged as insulated to the C++ sim. Insulated tiles and pipes have reduced conductivity values that dramatically slow heat transfer.

**Cell-level:** The `Grid.Insulation[cell]` value provides per-cell insulation data used by the native sim.

## Overheat Damage

Buildings with the `Overheatable` component take damage when their temperature exceeds `OverheatTemperature`:
- Damage occurs every **7.5 seconds** while overheated
- Deals 1 damage per trigger
- When repaired, temperature resets to 293.15 K (20 C)
- Both `OverheatTemperature` and `FatalTemperature` are modifiable building attributes

## Minimum Operating Temperature

The `MinimumOperatingTemperature` component prevents equipment from working when too cold:
- Default threshold: 275.15 K (2 C)
- Checks both building temperature AND all placement cell temperatures
- Has a **5-second turn-on delay** to prevent flapping at the boundary
- Sets `warmEnoughFlag` operational requirement

## Creature Temperature

**Warm-blooded creatures** (`WarmBlooded` component) have three regulation modes:
1. **SimpleHeatProduction** - Just generates baseline heat
2. **HomeostasisWithoutCaloriesImpact** - Regulates temperature without calorie cost
3. **FullHomeostasis** - Burns calories to regulate temperature

Cold regulation heats the creature via `WarmingKW`. Hot regulation cools via evaporative `CoolingKW`. A **3-second transition delay** between heating and cooling states prevents oscillation.

**Internal temperature monitoring** (`TemperatureMonitor`) uses 4-second averaging:
- Hypothermia threshold: 307.15 K (34 C internal)
- Hyperthermia threshold: 313.15 K (40 C internal)

**External temperature monitoring** (`ExternalTemperatureMonitor`) tracks energy flow:
- Cold threshold: losing more than 0.039 kW to environment
- Hot threshold: gaining more than 0.008 kW from environment
- The asymmetry means creatures feel hot more easily than cold

## Key Temperature Constants

| Constant | Value | Context |
|----------|-------|---------|
| MIN_MASS_FOR_TEMPERATURE_TRANSFER | 0.01 kg | Below this, heat capacity = 0 |
| STATE_TRANSITION_TEMPERATURE_BUFFER | 3 K | Prevents state transition flickering |
| OVERHEAT_DAMAGE_INTERVAL | 7.5 s | Time between overheat damage ticks |
| Temperature reset on repair | 293.15 K | Buildings reset to 20 C when fixed |
