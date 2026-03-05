# Oxygen Not Included - Simulation Systems Guide

A comprehensive technical reference for how ONI's simulation engine works under the hood, derived from decompiled source code analysis.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [The Grid](#2-the-grid)
3. [Elements](#3-elements)
4. [State Transitions](#4-state-transitions)
5. [Sublimation and Off-Gassing](#5-sublimation-and-off-gassing)
6. [Temperature and Heat Transfer](#6-temperature-and-heat-transfer)
7. [Gas and Liquid Physics](#7-gas-and-liquid-physics)
8. [Pressure](#8-pressure)
9. [Gravity and Falling](#9-gravity-and-falling)
10. [Chemical Reactions](#10-chemical-reactions)
11. [Element Conversion and Consumption](#11-element-conversion-and-consumption)
12. [Building Cell Occupation](#12-building-cell-occupation)
13. [Disease and Germs](#13-disease-and-germs)
14. [Radiation](#14-radiation)
15. [Light, Decor, and Noise](#15-light-decor-and-noise)
16. [Pipe and Conduit Flow](#16-pipe-and-conduit-flow)
17. [Electrical Power](#17-electrical-power)
18. [World Generation](#18-world-generation)
19. [Vacuum, Entombment, and Mass Conservation](#19-vacuum-entombment-and-mass-conservation)
20. [Falling Physics](#20-falling-physics)
21. [Duplicant Navigation and Movement](#21-duplicant-navigation-and-movement)
22. [Creature AI and Metabolism](#22-creature-ai-and-metabolism)
23. [Performance and Active Regions](#23-performance-and-active-regions)
24. [Formula Reference](#24-formula-reference)

---

## 1. Architecture Overview

ONI uses a **hybrid simulation architecture** split between two layers:

**Native C++ engine (SimDLL.dll)** handles the core physics:
- Pressure-driven gas and liquid diffusion
- Thermal conduction between cells
- State transitions (melting, boiling, freezing, condensation)
- Gravity and falling particles
- Chemical element interactions
- Disease growth and diffusion

**C# managed layer (Assembly-CSharp.dll)** handles everything else:
- Message serialization to/from the native sim
- Building behavior and component logic
- Conduit/pipe flow (entirely C# side)
- Electrical circuits
- UI, overlays, and event routing
- Creature and duplicant AI

The two layers communicate through a **message-passing interface**. The C# side sends structured messages (e.g., `ModifyCell`, `AddElementConsumer`, `ModifyEnergy`) to the native DLL, and the DLL sends back callbacks (e.g., `MassConsumedCallback`, `MassEmittedCallback`, state change notifications). Neither side directly accesses the other's memory.

**Simulation tick rates** vary by system:
- The native sim runs per-frame
- `ISim33ms` callbacks fire every 33ms game time
- `ISim200ms` callbacks fire every 200ms game time (most building logic runs here)
- `ISim1000ms` callbacks fire every 1 second game time
- `ISim4000ms` callbacks fire every 4 seconds game time

---

## 2. The Grid

The game world is a 2D grid of cells. Each cell stores its state as parallel arrays in the `Grid` class:

| Array | Type | Description |
|-------|------|-------------|
| `Grid.Element[cell]` | Element ref | The element occupying this cell |
| `Grid.ElementIdx[cell]` | ushort | Index into ElementLoader.elements |
| `Grid.Mass[cell]` | float | Mass in kilograms |
| `Grid.Temperature[cell]` | float | Temperature in Kelvin |
| `Grid.DiseaseIdx[cell]` | byte | Disease type (255 = none) |
| `Grid.DiseaseCount[cell]` | int | Germ population count |
| `Grid.Properties[cell]` | byte | Flags: GasImpermeable, LiquidImpermeable, etc. |
| `Grid.Insulation[cell]` | byte | Thermal insulation value |
| `Grid.StrengthInfo[cell]` | byte | Structural strength |
| `Grid.Radiation[cell]` | float | Radiation level (DLC feature) |

**Critical constraint: each cell holds exactly one element.** There is no mixing of elements within a single cell. This is fundamental to how the engine works and is why you see sharp gas/liquid boundaries rather than gradual blending.

Cell coordinates are linearized: `cellIndex = x + (width * y)`.

### Cell Validation

When cells are modified through `SimMessages.ModifyCell()`, the game validates:
- Temperature must be between 0K and 10,000K
- If mass > 0, temperature must be > 0 (no absolute-zero matter)
- Mass is clamped to the element's `maxMass`
- Invalid values are logged as warnings and reset to element defaults

---

## 3. Elements

Every material in the game is defined as an `Element` object with extensive physical properties. Element definitions are loaded from YAML files at startup, processed by `ElementLoader`, and referenced by their `SimHashes` enum value (a 32-bit hash).

There are approximately **193 elements** total, split roughly into:
- ~30 gases
- ~35 liquids
- ~120 solids
- Plus special values: Vacuum, Void, Creature

### Element States

The `Element.State` enum is a bitfield:

| Value | Name |
|-------|------|
| 0x00 | Vacuum (empty cell) |
| 0x01 | Gas |
| 0x02 | Liquid |
| 0x03 | Solid |
| 0x04 | Unbreakable (flag, combined with state) |
| 0x08 | Unstable (flag, falling solid) |
| 0x10 | TemperatureInsulated (flag, no heat exchange) |

The base state is extracted with `state & 0x03`.

### Thermal Properties

| Property | Description |
|----------|-------------|
| `specificHeatCapacity` | Energy needed to change 1 kg by 1 K (kJ/kg/K). Higher = harder to heat/cool. |
| `thermalConductivity` | Rate of heat transfer to neighbors. 0 = thermally insulated. |
| `solidSurfaceAreaMultiplier` | Heat exchange surface area multiplier when solid |
| `liquidSurfaceAreaMultiplier` | Heat exchange surface area multiplier when liquid |
| `gasSurfaceAreaMultiplier` | Heat exchange surface area multiplier when gas |

### Mechanical Properties

| Property | Description |
|----------|-------------|
| `flow` | Base flow rate for fluids |
| `viscosity` | Resistance to flow (higher = slower) |
| `minHorizontalFlow` | Flow stops below this threshold horizontally |
| `minVerticalFlow` | Flow stops below this threshold vertically |
| `maxCompression` | Maximum compression ratio for liquids |
| `strength` | Breaking resistance for solids (0 = unbreakable) |
| `hardness` | Hardness rating 0-255 |

### Mass Limits

| State | Max mass per cell |
|-------|-------------------|
| Solid | 10,000 kg |
| Liquid | 10,000 kg (element-specific override possible) |
| Gas | 1.8 kg default (set during loading via `ElementLoader`; see note below) |

### Radiation Properties

| Property | Description |
|----------|-------------|
| `radiationAbsorptionFactor` | How much radiation the element absorbs (0-1) |
| `radiationPer1000Mass` | Radiation emitted per 1000 kg of this element |
| `lightAbsorptionFactor` | How much visible light is absorbed |

### Other Properties

- `toxicity` - Affects duplicant health
- `materialCategory` - Category tag (Metal, Rock, etc.)
- `oreTags` - Additional filtering tags
- `attributeModifiers` - Bonuses/penalties to duplicant attributes
- `elementComposition` - Percentage breakdown for composite elements

---

## 4. State Transitions

Each element defines temperature thresholds for changing state:

| Property | Description |
|----------|-------------|
| `lowTemp` | Temperature (K) below which the element transitions down |
| `highTemp` | Temperature (K) above which the element transitions up |
| `lowTempTransition` | Target element when cooling below lowTemp |
| `highTempTransition` | Target element when heating above highTemp |

### Transition Directions by State

**Solids** can only transition UP (melting):
- When temperature > `highTemp`: becomes `highTempTransition` (usually a liquid)
- Example: Iron Ore at its melting point becomes Molten Iron

**Liquids** can transition both directions:
- When temperature > `highTemp`: becomes `highTempTransition` (usually a gas)
  - Example: Water at 373K becomes Steam
- When temperature < `lowTemp`: becomes `lowTempTransition` (usually a solid)
  - Example: Water at 273K becomes Ice

**Gases** can only transition DOWN (condensation):
- When temperature < `lowTemp`: becomes `lowTempTransition` (usually a liquid)
  - Example: Steam below 373K becomes Water

### Ore Transitions

Some state transitions produce a secondary material alongside the main product:

| Property | Description |
|----------|-------------|
| `highTempTransitionOreID` | Secondary element produced when heating |
| `highTempTransitionOreMassConversion` | Mass ratio for the secondary product |
| `lowTempTransitionOreID` | Secondary element produced when cooling |
| `lowTempTransitionOreMassConversion` | Mass ratio for the secondary product |

Example: 1 kg of Iron Ore melting might produce 0.7 kg Molten Iron (the primary transition) plus some slag (the ore transition).

### Transition Buffer

The simulation uses a `STATE_TRANSITION_TEMPERATURE_BUFFER` of 3K to prevent flickering at boundaries. The `GetRelativeHeatLevel()` method maps temperature to a 0-1 scale between `lowTemp - 3` and `highTemp + 3`.

### What Triggers Transitions

State transitions are computed by the **native C++ simulation**, not the C# side. The Element class only stores the thresholds and target references. The native sim checks temperatures against thresholds every frame and performs transitions automatically, conserving energy across the change.

---

## 5. Sublimation and Off-Gassing

Two distinct mechanisms allow materials to release gas without going through the normal state transition chain.

### Sublimation (Solids to Gas)

Sublimation skips the liquid phase entirely. The `Sublimates` component (a KMonoBehaviour) handles this.

**Properties on the Element:**
- `sublimateId` - Which gas element is produced
- `sublimateRate` - Base emission rate (mass per frame)
- `sublimateProbability` - Per-frame chance of emission occurring
- `sublimateFX` - Visual effect

Note: The Element class also defines `sublimateEfficiency`, but this property is not used in the C# `Sublimates` component's rate calculation. It is checked during element loading validation (`sublimateRate * sublimateEfficiency > 0.001f`) and may be used by the native sim.

**Rate calculation:**
```
rate = max(sublimateRate, sublimateRate * mass^massPower) * dt
```
Emission only occurs if accumulated sublimated mass exceeds `minSublimationAmount`.

**Overpressure blocking:** The Sublimates component checks if the destination cell (and its left/right/above neighbors) exceed `maxDestinationMass`. If overpressured, the status shows "Blocked (High Pressure)" and emission stops until pressure drops.

**State machine:** The Sublimates component has three states:
1. **Emitting** - Normal emission occurring
2. **BlockedOnPressure** - Destination cell is full
3. **BlockedOnTemperature** - Source material is too cold

### Off-Gassing (Liquids to Gas)

Liquids use the `offGasPercentage` property instead of the sublimation system. This represents the percentage of liquid that converts to gas per frame. Unlike sublimation, off-gassing uses a different rate control mechanism and is also subject to overpressure blocking.

---

## 6. Temperature and Heat Transfer

### Core Heat Transfer Formula

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

### Temperature Change from Energy

```
deltaT = kilojoules / (specificHeatCapacity * mass)
```

Higher specific heat capacity means more energy is needed to change temperature. Higher mass also means more energy needed. This is why large pools of water are effective heat sinks.

### Temperature Mixing

The standard mixing function `SimUtil.CalculateFinalTemperature` is mass-weighted without SHC:

```
finalTemp = (mass1 * temp1 + mass2 * temp2) / (mass1 + mass2)
```

The result is clamped to `[min(temp1, temp2), max(temp1, temp2)]` to prevent numerical overshoot. This function is used throughout the game for pipe contents merging, element chunk absorption, storage consolidation, and turbine intake.

**Exception: ElementConverter** uses SHC-weighted mixing when computing output temperatures from consumed inputs (see Section 11):
```
finalTemp = sum(mass * temp * SHC) / sum(mass * SHC)
```
This accounts for different materials having different heat capacities, which matters when a building consumes multiple element types simultaneously.

### Building Heat Exchange

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

### Insulation

**Element-level:** If `thermalConductivity == 0`, the element is flagged as `TemperatureInsulated` and does not register with the sim for heat exchange. Vacuum cells are thermally insulated.

**Building-level:** Pipes and tiles with `ThermalConductivity < 1.0` are flagged as insulated to the C++ sim. Insulated tiles and pipes have reduced conductivity values that dramatically slow heat transfer.

**Cell-level:** The `Grid.Insulation[cell]` value provides per-cell insulation data used by the native sim.

### Overheat Damage

Buildings with the `Overheatable` component take damage when their temperature exceeds `OverheatTemperature`:
- Damage occurs every **7.5 seconds** while overheated
- Deals 1 damage per trigger
- When repaired, temperature resets to 293.15 K (20 C)
- Both `OverheatTemperature` and `FatalTemperature` are modifiable building attributes

### Minimum Operating Temperature

The `MinimumOperatingTemperature` component prevents equipment from working when too cold:
- Default threshold: 275.15 K (2 C)
- Checks both building temperature AND all placement cell temperatures
- Has a **5-second turn-on delay** to prevent flapping at the boundary
- Sets `warmEnoughFlag` operational requirement

### Creature Temperature

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

### Key Temperature Constants

| Constant | Value | Context |
|----------|-------|---------|
| MIN_MASS_FOR_TEMPERATURE_TRANSFER | 0.01 kg | Below this, heat capacity = 0 |
| STATE_TRANSITION_TEMPERATURE_BUFFER | 3 K | Prevents state transition flickering |
| OVERHEAT_DAMAGE_INTERVAL | 7.5 s | Time between overheat damage ticks |
| Temperature reset on repair | 293.15 K | Buildings reset to 20 C when fixed |

---

## 7. Gas and Liquid Physics

### Pressure-Driven Diffusion

The native C++ simulation uses **pressure-driven diffusion** to move gases and liquids. There is no momentum or velocity tracking per cell. Flow is entirely determined by pressure differences between adjacent cells.

For each cell, the sim:
1. Calculates pressure difference to each neighbor
2. Applies the element's viscosity and flow modifiers
3. Checks if flow exceeds `minHorizontalFlow` or `minVerticalFlow` thresholds
4. Transfers mass proportional to the time step and pressure gradient
5. Records accumulated flow for visualization

### Flow Properties

- **Low viscosity** (like water) = faster equalization
- **High viscosity** (like crude oil) = slower spreading
- **minFlow thresholds** prevent visually annoying micro-flows where tiny amounts shuttle back and forth

### Gas Behavior

Gases naturally stratify by density:
- **Light gases** (hydrogen) rise to the top of enclosed spaces
- **Heavy gases** (chlorine, CO2) sink to the bottom
- Gases expand to fill available space through pressure equalization
- Two cells of gas equalize in roughly 10-20 simulation frames

### Liquid Behavior

- Liquids are primarily gravity-driven (they fall and pool)
- Horizontal spreading is limited by viscosity
- Liquids settle into the lowest available cells
- When a liquid becomes unstable (unsupported), it enters the falling particle system

### One Element Per Cell

This is the most important constraint. Because each cell holds exactly one element:
- Gas boundaries are sharp, not gradual
- Different gases cannot occupy the same cell
- When two different elements try to occupy the same cell, one displaces the other
- This is why gas sorting happens (light gases push up past heavy ones)

### Element Replacement

When modifying cells, three replacement modes exist:
1. **None** - Simply add mass to the cell
2. **Replace** - Replace the entire cell's element, discarding the old one
3. **ReplaceAndDisplace** - Replace the element and physically displace old material:
   - Displaced solids drop as falling chunks
   - Displaced liquids become falling water particles
   - Displaced gases disperse to adjacent cells

---

## 8. Pressure

### Pressure Is Mass

ONI does not have a separate pressure simulation. **Pressure is directly proportional to mass in a cell.** The conversion is:

```
pressure = mass * element.defaultValues.pressure
```

Or inversely:
```
mass = pressure / element.defaultValues.pressure
```

This means "high pressure" simply means "high mass per cell." Pressure relief means removing mass (venting gas to space, pumping liquid away).

### Overpressure Effects

Overpressure manifests in two ways:

**For emitters/sublimates:** When the destination cell's mass exceeds `maxDestinationMass`, emission is blocked. The building shows "Blocked (High Pressure)" status and waits for pressure to drop naturally.

**For duplicants:** The `PressureMonitor` tracks whether a duplicant is in high-pressure gas (any gas cell with mass > 4 kg). If exposed for more than 3 seconds continuously, the duplicant receives the "Popped Ear Drums" effect. This is a pressure-based condition, not germ-based.

### Gas maxMass Property

During element loading, `ElementLoader` overrides every gas element's `maxMass` to **1.8 kg**, regardless of the YAML definition. This value is passed to the native C++ sim via `PhysicsData`. However, it is NOT a hard ceiling. Gas cells routinely exceed 1.8 kg in gameplay (overpressured vents, sealed rooms with emitters, etc.).

The `maxMass` value for gases likely acts as a **flow/diffusion parameter** within the native sim, influencing when a cell resists accepting more mass from pressure-driven flow. Direct emission from buildings (`ElementEmitter`, `AddRemoveSubstance`) can push cells well above this threshold. The C# side has a mass-clamping check in `ModifyCell()`, but it contains a dead-code bug (`if (element.maxMass == 0f && mass > element.maxMass)` — the condition is always false when maxMass is 1.8), so no C#-side clamping actually occurs.

The related constant `Sim.MAX_SUBLIMATE_MASS = 1.8f` caps the maximum gas mass emitted per sublimation event, which is a separate per-emission limit, not a per-cell cap.

---

## 9. Gravity and Falling

### Falling Solids

Solid materials are not stored in the grid like gases and liquids when they become unsupported. Instead:
- Solids with the `Unstable` flag enter a **falling chunk** physics system
- They are simulated as discrete particles with gravity
- When they reach a stable surface (floor or other solid), they stick and re-enter the grid as a cell element
- The `do_vertical_solid_displacement` parameter controls whether solids prefer falling downward vs. spreading sideways

### Falling Liquids

Liquids also use a separate particle system for falling:
- The `FallingWater` system manages liquid drops
- `SpawnFallingLiquidInfo` notifies the game when liquid starts falling
- Falling liquids settle into the lowest available cells once they stop

### The Settling Pass

During world generation, a 500-frame physics settling pass runs to let materials find their natural positions:
- Heavy elements sink, light elements float
- Gases expand and stratify
- Liquids pool at the bottom of cavities
- Each frame simulates 0.2 seconds, for a total of 100 seconds of simulated settling time

---

## 10. Chemical Reactions

The simulation supports element interactions via the `ElementInteraction` system:

```
struct ElementInteraction {
    interactionType      // Reaction type
    elemIdx1, elemIdx2   // Reactant elements
    elemResultIdx        // Product element
    minMass              // Minimum mass threshold for both reactants
    interactionProbability  // 0.0-1.0 chance per frame
    elem1MassDestructionPercent  // Fraction of element 1 consumed
    elem2MassRequiredMultiplier  // How much element 2 needed relative to element 1
    elemResultMassCreationMultiplier  // Output mass multiplier
}
```

When two elements meet in adjacent cells with sufficient mass, they react probabilistically. Reactions are registered at game startup via `SimMessages.CreateElementInteractions()`.

---

## 11. Element Conversion and Consumption

Buildings transform elements using three main systems: `ElementConverter` for complex multi-input/output conversion, `ElementConsumer`/`ElementEmitter` for simple intake/output, and `EntityElementExchanger` for creature metabolism.

### ElementConverter

The primary system for buildings that transform one set of elements into another (refineries, electrolyzers, etc.).

**Input definition:** An array of `ConsumedElement` entries, each specifying:
- Element tag and mass consumption rate (kg/s)
- All inputs must be available simultaneously; if any input is short, all outputs scale down proportionally

**Output definition:** An array of `OutputElement` entries, each specifying:
- Element hash and mass generation rate (kg/s)
- Output mass is **independent of input mass** (ratios are configured, not derived)
- Position offset from building center for emission location
- Whether to store output in building storage or emit to world
- Disease weight for distributing input diseases across outputs
- Optional injected disease (addedDiseaseIdx/Count)

**Completion fraction:** When inputs are insufficient, a `completionFraction` (0-1) scales all outputs:
```
outputMass = massGenerationRate * OutputMultiplier * speedMultiplier * dt * completionFraction
```

### Output Temperature Rules

Three modes determine what temperature outputs are produced at:

1. **Entity temperature** (`useEntityTemperature = true`): Uses the building's own PrimaryElement temperature
2. **Weighted input temperature** (`useEntityTemperature = false`, inputs exist): Calculates SHC-weighted average from consumed elements, then takes `max(minOutputTemperature, weightedAverage)`
3. **Fixed minimum** (no inputs): Uses `minOutputTemperature` directly

The weighted average formula:
```
outputTemp = sum(mass * temp * SHC) / sum(mass * SHC)
```

### Disease Through Converters

1. All input diseases are accumulated during consumption
2. Distributed across outputs based on `diseaseWeight` ratios
3. If `totalDiseaseWeight = 0`, no input disease propagates (used to "clean" outputs)
4. Optional injected disease is mixed in after distribution

### ElementConsumer

Registers with the native sim to consume elements from the grid:

- **Consumption radius:** Configurable (default 1 cell), sim searches within radius for matching element
- **Sample cell offset:** Adjustable offset from building center
- **Minimum mass:** Only consumes if cell mass exceeds threshold
- **Configuration modes:** Specific element, AllLiquid, or AllGas
- Consumption is callback-driven: sim delivers consumed mass, temperature, and disease
- Consumption rate set to 0 when building is non-operational or storage is full

**PassiveElementConsumer** is a 3-line subclass that overrides `IsActive()` to always return true, ignoring building operational state. Used for natural vents and plants that consume regardless of power.

### ElementEmitter

Emits elements into the world at a configured cell position (building center + offset vector):

- **Gas:** Added to cell via `SimMessages.AddRemoveSubstance()`
- **Liquid:** Spawned as falling particle via `FallingWater`
- **Solid:** Spawned as physical resource object

Tracks blocked state (`isEmitterBlocked`) when the target cell is full. The native sim invokes callbacks to toggle this state, and the building displays "Blocked" status.

`BuildingElementEmitter` is a simpler variant for creatures and basic buildings that always uses a fixed temperature (not computed from inputs).

### EntityElementExchanger

A lightweight system for creature metabolism (e.g., plants absorbing CO2, emitting O2):
- Single input element + single output element
- Simple mass ratio: `outputMass = consumedMass * exchangeRatio`
- Passes disease straight through (no weighting)
- State machine with `exchanging` and `paused` states (pauses when creature wilts)

---

## 12. Building Cell Occupation

### How Buildings Occupy Cells

Buildings occupy grid cells through the `SimCellOccupier` component. Each building's placement cells are calculated from its dimensions. The building's total mass is divided evenly across all occupied cells.

**Two occupation strategies:**

**With element replacement** (`doReplaceElement = true`): Standard buildings. Calls `SimMessages.ReplaceAndDisplaceElement()` per cell, completely replacing the existing element with the building's construction material. The displaced element drops as a falling chunk, liquid particle, or disperses as gas.

**Without element replacement** (`doReplaceElement = false`): Tiles and permeable buildings. Preserves the existing element while marking the cell as occupied. Updates navigation grid and triggers solid change events.

### Cell Property Flags

Buildings set bitflags on occupied cells via `SimMessages.SetCellProperties()`:

| Flag | Value | Effect |
|------|-------|--------|
| GasImpermeable | 1 | Blocks gas flow through this cell |
| LiquidImpermeable | 2 | Blocks liquid flow through this cell |
| SolidImpermeable | 4 | Always set by SimCellOccupier |
| Transparent | 16 | Cell is transparent to light |
| Opaque | 32 | Cell blocks light |
| NotifyOnMelt | 64 | Triggers event when building melts |
| ConstructedTile | 128 | Auto-set when building is a foundation |

**Typical combinations:**
- **Solid walls:** SolidImpermeable + GasImpermeable + LiquidImpermeable (blocks everything)
- **Mesh tiles:** SolidImpermeable only (permits gas and liquid flow through)
- **Airflow tiles:** SolidImpermeable + LiquidImpermeable (permits gas, blocks liquid)

### Tile Buildings vs Regular Buildings

| Property | Regular Building | Tile |
|----------|-----------------|------|
| doReplaceElement | Usually true | false (mesh) or true (standard) |
| IsFoundation | false | true |
| Floodable | true | false (immune) |
| Entombable | true | false (immune) |
| destroyOnDamaged | false | true (instant destruction) |

### Entombment

The `EntombVulnerable` component detects when a building or creature is completely surrounded by solids:
- Checks all occupied cells for `Grid.Solid[cell]`
- If any cell is solid-blocked, the entity is entombed
- Sets `notEntombedFlag` operational flag (disables building function)
- Tiles are immune (`Entombable = false` in BuildingDef)

### Flooding

The `Floodable` component detects liquid in occupied cells:
- Checks each cell for "substantial liquid": `mass >= element.defaultValues.mass * 0.35`. For Water (default mass 1 kg), this equals 0.35 kg, but varies by liquid type
- If any cell exceeds threshold, building enters flooded state
- Sets `notFloodedFlag` operational flag (building stops working)
- Tiles are immune (`Floodable = false`)

### Building Damage

The `BuildingHP` component tracks structural integrity:

1. **WorldDamage** accumulates damage in `Grid.Damage[cell]` (float, 0.0 to 1.0)
2. When cell damage exceeds 0.15, retrieves building from `Grid.Objects[cell]`
3. Calls `BuildingHP.DoDamage()` to decrement hitpoints
4. When HP reaches 0: building enters "damaged" state, shows notification
5. When `Grid.Damage[cell] >= 1.0`: cell is destroyed entirely

**Repair synchronization:** When duplicants repair a building, `SimCellOccupier` listens to repair events and calls `WorldDamage.RestoreDamageToValue()` to keep cell structural damage proportional to building HP: `cellDamage = 1.0 - (currentHP / maxHP)`.

### Building Destruction Cleanup

When `SimCellOccupier.DestroySelf()` is called:
1. Clears cell property flags
2. If the cell still holds the building's element: replaces with Vacuum, transfers disease
3. If the element was already changed: marks cell as non-solid
4. Triggers solid change events for world update

---

## 13. Disease and Germs

### Overview

ONI tracks germs at both the cell level and the entity level. Each cell can hold one disease type with an integer germ count. Germs grow, die, spread, and infect duplicants through multiple pathways.

### Per-Cell Disease Data

- `Grid.DiseaseIdx[cell]` (byte) - Index of disease present (255 = none)
- `Grid.DiseaseCount[cell]` (int) - Number of germs in the cell

### Disease Types

There are 5 germ types in the game:

| Germ | ID | Primary Vector | Threshold |
|------|----|---------------|-----------|
| Food Poisoning | FoodPoisoning | Digestion | 100 germs |
| Slimelung | SlimeLung | Inhalation | 100 germs |
| Zombie Spores | ZombieSpores | Inhalation + Contact | 1 germ |
| Pollen | PollenGerms | Inhalation | 2 germs |
| Radiation Poisoning | RadiationSickness | Contact | 1 germ |

### Growth and Die-Off: The Half-Life System

Germ populations change over time based on **half-life values** that depend on what element they're on. A negative half-life means growth (population doubling time), a positive half-life means die-off.

Each disease defines growth rules per element via `ElementGrowthRule`:
- A set of temperature ranges with associated half-lives
- Different rates for different elements
- 4-tier temperature interpolation: `minGrowth`, `lowGrowth`, `maxGrowth`, `lowGrowth` (symmetric curve)

**Temperature affects growth through ranges:**

| Range | Effect |
|-------|--------|
| Below `minTemp` | Lethal: rapid die-off (short positive half-life) |
| `minTemp` to `lowTemp` | Slow growth or slow die-off |
| `lowTemp` to `highTemp` | Optimal: maximum growth (most negative half-life) |
| `highTemp` to `maxTemp` | Slow growth or slow die-off |
| Above `maxTemp` | Lethal: rapid die-off |

**Example growth rates (half-life in seconds):**
- Food Poisoning on Contaminated Water: -12,000s (grows, doubles every ~3.3 hours)
- Food Poisoning on any liquid (generic liquid state rule): 12,000s (dies, halves every ~3.3 hours)
- Food Poisoning on Chlorine: 10s (dies extremely fast)
- Slimelung on Polluted Oxygen: -300s (grows quickly)
- Slimelung on Clean Oxygen: 1,200s (dies slowly)

### Germ Spreading Between Cells

Germs diffuse to adjacent cells when population exceeds thresholds. The diffusion is proportional to mass transfer: when mass moves between cells (gas diffusion, liquid flow), germs move with it proportionally.

### Radiation Kills Germs

Each disease has a `radiationKillRate`:
- Food Poisoning: 2.5 (killed in 2-3 seconds at 1 rad)
- Slimelung: 2.5
- Zombie Spores: 1.0 (slower killing)
- Pollen: 0.0 (immune to radiation)
- Radiation Sickness: 0.0 (is radiation)

High radiation sterilizes items and prevents most disease growth.

### Pressure Is Irrelevant

All 5 diseases use `RangeInfo.Idempotent()` for pressure, meaning pressure has zero effect on germ growth or death.

### Disease Mixing

When two disease sources combine (e.g., merging pipe contents), the system uses strength-weighted competition:
1. Same disease: counts simply add together
2. Different diseases: compare `disease.strength * count` for each
3. The winning disease (higher product) survives
4. The loser's count is reduced based on the strength ratio

### Infection Pathway

Germ exposure follows a state machine:

```
None -> Contact -> Exposed -> Contracted -> Sick
```

**Contact:** Duplicant touches germs above the exposure threshold. The source determines the infection vector:
- **Inhalation:** Breathing germy gas (OxygenBreather consumes gas + germs)
- **Digestion:** Eating contaminated food (germs proportional to food fraction eaten)
- **Contact:** Physical touch with contaminated surfaces

**Exposed:** Accumulation exceeds threshold. For inhalation, requires continuous exposure (5+ ticks for Tier 1, up to 20+ for Tier 4).

**Contracted:** Happens during **sleep**. If a duplicant sleeps while Exposed, they either clear the exposure (back to None) or become Contracted. Contracted duplicants become Sick on the next sleep.

**Minimum exposure period:** 540 seconds (9 minutes) between same-germ exposures, preventing rapid stacking.

### Contraction Chance

The probability of contracting a disease from exposure uses a hyperbolic tangent curve:

```
contractionChance = 0.5 - 0.5 * tanh(0.25 * resistanceRating)
```

Where:
```
resistanceRating = base_resistance + duplicant_germ_resistance + exposure_tier_bonus
```

Exposure tier bonuses: Tier 1 = +3.0, Tier 2 = +1.5, Tier 3 = +0.0

| Rating | Chance |
|--------|--------|
| 0 | 50% |
| 5 | ~42% |
| 10 | ~20% |
| 20 | <1% |

### Sickness Effects

Each disease causes different gameplay effects when contracted:

**Food Poisoning** (17 minutes):
- Increased bladder fill rate, reduced toilet efficiency, reduced stamina
- Periodic sick emotes

**Slimelung** (37 minutes):
- -3 Athletics, -25% breath rate
- **Active coughing spreads disease:** produces 0.1 kg Contaminated Oxygen + 1000 Slimelung germs per cough

**Zombie Spores** (3 hours):
- -10 to ALL 11 skill attributes
- Completely debilitates the duplicant
- Visible spore cloud effect

**Allergies** (60 seconds, requires Allergies trait):
- +25% stress per cycle, +10 Sneezyness
- Contracts immediately (no sleep required)
- Non-allergic duplicants get harmless "Smelled Flowers" instead

**Radiation Sickness** (3 hours):
- Same symptoms as Zombie Spores (-10 to all skills)
- Not transmitted by normal germ mechanics; caused by direct radiation exposure

### Recovery

When cured, a recovery effect prevents immediate re-infection (e.g., "FoodSicknessRecovery"). While the recovery effect is active, the same sickness cannot be contracted again.

### Immunity Traits

- **IronGut:** Prevents Food Poisoning exposure entirely
- **Allergies:** Required to be affected by Pollen germs (others are immune)

### Disease in Pipes

Conduit systems track disease separately via `ConduitDiseaseManager`. When fluid moves through pipes, germs travel proportionally with the mass. Disease mixing in pipes follows the same strength-weighted rules.

### Special Element Interactions

- **Contaminated Oxygen:** Growth medium for both Food Poisoning (-12,000s) and Slimelung (-300s)
- **Pickled food:** Kills Food Poisoning germs extremely fast (10s half-life)
- **Chlorine:** Universal sterilizer for most germs
- **Creatures:** Living creatures (element SimHashes.Creature) become walking disease vectors

---

## 14. Radiation

Radiation is a DLC expansion feature. It can be entirely disabled in world settings.

### Propagation Model

Radiation uses a **ray-based propagation model**, not grid diffusion. This is fundamentally different from gas/liquid simulation:

1. Each `RadiationEmitter` casts rays in configured directions
2. For each ray, radiation intensity decreases per cell based on material absorption
3. The formula for absorption per cell:
   ```
   absorption = pow(cellMass, 1.25) * elementMolarMass / 1000000
   ```
4. Radiation decays along each ray: `rads *= (1 - absorption) * random(0.96-0.98)`
5. Propagation stops when rads drop below 0.01
6. Maximum 128 cells per ray, up to 20 rays per emitter per frame

Because radiation is ray-based, it is **localized** around sources. Distant areas receive minimal radiation unless emitters are powerful or multiple sources converge.

### Radiation Grid

The `RadiationGridManager` maintains a per-cell radiation value in `Grid.Radiation[cell]`. This value lingers with a decay rate of 1/4 per frame, meaning radiation persists for several frames after the source stops.

Configurable simulation parameters:
- `LingerRate` - How long radiation persists in a cell
- `BaseWeight` - Base weighting for cell radiation
- `DensityWeight` - Weight scaling by material density
- `ConstructedFactor` - Enhancement for constructed tiles
- `MaxMass` - Maximum mass threshold for absorption calculations

### Emission Shapes

Emitters can cast radiation in several patterns:
- Directional (single ray)
- Cone (fan of rays)
- Circle (360-degree rays)
- Custom angle/direction parameters

### Material Shielding

Materials with `radiationAbsorptionFactor >= 0.8` are rated "Excellent Radiation Shield" in the UI.

**Shielding calculation for tiles:**
```
For constructed tiles:   absorption = factor * 0.8
For natural materials:   absorption = factor * 0.3 + (mass / 2000) * factor * 0.7
```

Constructed tiles (like lead blocks) provide consistent 0.8x absorption efficiency. Natural materials improve with mass, capped at 2000 kg.

### Duplicant Exposure

Duplicants track radiation exposure through the `RadiationMonitor`:

**Per-frame exposure:**
```
resistanceModifier = clamp01(1 - radiationResistanceAttribute)
exposureDelta = Grid.Radiation[cell] * resistanceModifier / 600 * deltaTime
```

**Sickness thresholds (multiplied by difficulty setting):**

| Level | Threshold | Effect |
|-------|-----------|--------|
| Minor | 100 rads | Warning, minor symptoms |
| Major | 300 rads | Significant debilitation |
| Extreme | 600 rads | Severe symptoms |
| Deadly | 900 rads | Incapacitation risk |

**Difficulty scaling:**
- Easiest: 0.5x thresholds
- Normal: 1.0x
- Hardest: 2.0x

### High-Energy Particles (HEP)

HEP is the game's radiation projectile system:

**HEP Spawners** absorb ambient radiation and convert it to particles:
```
particleStorage += (cellRadiation / 600) * radiationSampleRate * 0.1
```
Power consumption scales: `clamp(radiationPerSecond * 286W, 60W-480W)`

**HEP Detonation** on impact:
```
emitRads = payload * 0.5 * 600 / 9    (radiation burst)
fallout = payload * 0.001 kg           (Fallout element creation)
radiationPoisoning = payload * 0.5 / 0.01   (disease units)
```
Creates radioactive contamination (Fallout element) in a 6x6 cell explosion radius.

### Plant Mutations

Many plant mutations modify `MinRadiationThreshold` by +250 rads, allowing mutant plants to thrive in higher radiation environments. Plants use a comfort system with "too dark" (under-irradiated) and "too bright" (over-irradiated) states.

---

## 15. Light, Decor, and Noise

Three environmental quality systems affect duplicant morale and stress. All three use line-of-sight checks and have no effect through solid walls.

### Light

Light originates from `Light2D` components and propagates via discrete shadowcasting (`DiscreteShadowCaster`). Only cells with line-of-sight to the source receive illumination.

**Lux calculation:**
```
lux_at_cell = intensity / (falloffRate * max(distance_in_cells, 1))
```

| Property | Description |
|----------|-------------|
| `intensity` | Source lux value (e.g., 1000 for floor lamp) |
| `falloffRate` | Decay rate (typically 0.5) |
| `distance` | Cells from source to target |

**Light shapes:**
- **Circle:** 360-degree illumination (floor lamps, light bugs)
- **Cone:** 180-degree downward (ceiling lights, sunlamps)
- **Quad:** Rectangular directional beam (mercury ceiling lights)

**Example source intensities:**

| Source | Lux | Range | Shape |
|--------|-----|-------|-------|
| Floor Lamp | 1,000 | 4 cells | Circle |
| Ceiling Light | 1,800 | 8 cells | Cone |
| Sun Lamp | 40,000 | 16 cells | Cone |
| Mercury Ceiling Light | 60,000 | 8 cells | Quad |
| Light Bug | 1,800 | 5 cells | Circle |
| Fossil Sculpture | 3,000 | 8 cells | Quad |

Grid storage: `Grid.LightCount[cell]` accumulates raw lux. `Grid.LightIntensity[cell]` combines this with sunlight exposure (up to 80,000 lux).

### Decor

`DecorProvider` components add decor values to cells within a radius. Only cells with line-of-sight receive the decor bonus. Stored items have zero decor.

**Decor sampling:** Duplicants sample decor at their current cell each frame via `DecorMonitor`. The value is smoothly accumulated over the cycle, and the daily average is applied nightly as a morale modifier.

**Bonus tiers (TUNING.DECOR.BONUS):**

| Tier | Decor | Radius |
|------|-------|--------|
| TIER0 | +10 | 1 |
| TIER1 | +15 | 2 |
| TIER2 | +20 | 3 |
| TIER3 | +25 | 4 |
| TIER4 | +30 | 5 |
| TIER5 | +35 | 6 |
| TIER6 | +50 | 7 |
| TIER7 | +80 | 7 |
| TIER8 | +200 | 8 |

**Penalty tiers:** Range from -5 (radius 1) to -25 (radius 6).

**Light-decor interaction:** Any lit cell (LightIntensity > 0) receives an automatic **+15 decor bonus** on top of provider-based decor. This makes lighting functionally useful beyond visibility.

**Room interactions:** Rooms do not directly multiply decor. Room quality is determined by constraint satisfaction (furniture requirements), and rooms apply separate status effects via their `RoomType.effects[]` array. Decor is purely cell-based and independent of room boundaries.

### Noise Pollution

`NoisePolluter` components emit noise in decibels to surrounding cells via `NoiseSplat` objects.

**Falloff formula:**
```
dB_at_cell = sourceDB - (sourceDB * distance * 0.05)
```

Example: 75 dB source at 5 cells = `75 - (75 * 5 * 0.05) = 56 dB`

**Noise tiers (TUNING.NOISE_POLLUTION):**

| Tier | dB | Radius | Typical Source |
|------|----|----|----------------|
| TIER0 | 45 | 10 | Quiet equipment |
| TIER1 | 55 | 10 | Normal equipment |
| TIER2 | 65 | 10 | Moderate equipment |
| TIER3 | 75 | 15 | Loud equipment |
| TIER4 | 90 | 15 | Very loud |
| TIER5 | 105 | 20 | Extremely loud |
| TIER6 | 125 | 20 | Deafening |

Creature noise tiers are quieter (30-105 dB, 5-10 cell radius).

**Cone of Silence:** A special dampener at -120 dB, 5 cell radius, that suppresses nearby noise.

**Grid storage:** `Grid.Loudness[cell]` accumulates loudness converted from dB via `AudioEventManager.DBToLoudness()` (non-linear). Noise is temporal: splats are removed immediately when sources deactivate.

**Effects:** High loudness causes duplicant stress penalties, sampled per-frame at the duplicant's current cell. The stress threshold requires 3 seconds of continuous exposure.

---

## 16. Pipe and Conduit Flow

ONI has three conduit systems: **liquid pipes**, **gas pipes**, and **solid conveyor rails**. All three are simulated entirely in C# (not the native sim), running on a separate tick from the grid simulation.

### Fluid Pipe System (Liquid and Gas)

#### Capacity

| Type | Max mass per pipe cell |
|------|----------------------|
| Liquid pipes | 10 kg |
| Gas pipes | 1 kg |

#### Network Construction

When pipes are built or modified, the system constructs a flow graph:

1. **BFS from sources** (dispensers) to identify all reachable conduits
2. **BFS from sinks** (consumers) backward to find return paths
3. **Merge graphs** and **break cycles** via DFS to create a directed acyclic graph
4. **Topological sort** determines processing order (sources before sinks)

This graph is **static** until a pipe is built, destroyed, or reconfigured. Blocked pipes do NOT trigger recalculation.

#### Flow Direction

Each pipe segment stores:
- **Permitted flow directions** - A bitmask (`Down|Left|Right|Up`) of allowed directions, set during graph analysis
- **Target flow direction** - ONE active direction, updated dynamically as mass moves
- **Source flow direction** - Which direction fluid came from upstream

The flow solver cycles through directions when the target is blocked: Down, Left, Right, Up.

#### Flow Algorithm

The pipe system runs **every 1.0 seconds** of game time, with up to **4 passes per tick**:

1. For each conduit in network order:
   - Check available movable mass (`initial_mass - removed_mass`)
   - Try each permitted direction for a valid target
   - Target is valid if: same element (or vacuum) AND has remaining capacity
   - Transfer mass: `min(available_mass, target_capacity)`
   - Mix temperature (weighted average by mass)
   - Transfer disease proportionally
2. Consolidate mass changes
3. Repeat if any conduit made progress (up to 4 passes)

#### No Element Mixing

**Strict rule:** If pipe cell A contains Water and pipe cell B contains Polluted Water, flow is BLOCKED. No mixing, no displacement. The pipe backs up until the player redirects flow. Only same-element or vacuum cells can receive flow.

#### Bridges

Bridges bypass the normal flow solver entirely:
- Read from input cell, write to output cell directly
- Do not participate in permitted flow directions
- Transfer via `AddElement()` / `RemoveElement()` callbacks

#### Preferential Flow Valve

Checks primary input FIRST. Only uses secondary input if primary is empty. Always transfers maximum available mass.

#### Overflow Valve

Checks if primary output is occupied (any mass). If full, diverts to secondary output. Splits flow based on space availability.

#### Pipe Contents State

Each pipe cell tracks per-tick changes:
```
initial_mass    - Mass at tick start
added_mass      - Mass added during this tick
removed_mass    - Mass removed during this tick
mass            = initial_mass + added_mass - removed_mass
movable_mass    = initial_mass - removed_mass
```

`ConsolidateMass()` commits changes between passes.

#### Blockage Behavior

When pipes are full:
1. No backpressure signal propagates through the graph
2. Fluid simply accumulates upstream of the blockage
3. Dispensers detect full output pipes and set `blocked = true`
4. Eventually the entire pipe network backs up

### Solid Conveyor System

Conveyors are fundamentally different from fluid pipes:

| Aspect | Fluid Pipes | Conveyors |
|--------|-------------|-----------|
| Storage | Float mass (fractional) | One discrete Pickupable per cell |
| Flow rate | Up to 10 kg / 1 kg per tick | 1 item per tick |
| Mixing | Same element only | N/A (single items) |
| Direction | Multi-direction graph | Single-direction paths |
| Animation | Mass-based lerp | Smooth object movement between cells |

**Conveyor direction** is determined by DFS from sources to sinks, creating explicit paths. Items move one cell per tick along these paths.

**Dead-end handling:** When an item reaches a dead end (no valid downstream), it stops. The conveyor system recalculates the target direction for that cell.

---

## 17. Electrical Power

### Circuit Architecture

The power system uses a **circuit-based model** where all devices on connected wires share a single circuit. Each circuit tracks:
- Active generators (producing power)
- Consumers (drawing power)
- Batteries (storing power)
- Transformers (connecting circuits at different capacities)

Circuit membership is determined by physical wire connectivity through `UtilityNetworkManager`. Circuit IDs are **not cached** and are queried fresh each tick.

### Wire Tiers

| Tier | Max Wattage |
|------|-------------|
| Wire | 1,000 W |
| Conductive Wire | 2,000 W |
| Heavy-Watt Wire | 20,000 W |
| Heavi-Watt Conductive Wire | 50,000 W |

### Power Distribution Algorithm

The `CircuitManager.Sim200msLast()` method runs the main distribution every 200ms:

**Phase 1: Consumer Satisfaction**
1. Consumers sorted by `WattsNeededWhenActive` ascending (lowest-power devices first)
2. For each consumer:
   - Try active generators first (sorted by joules available, ascending)
   - Then output transformers
   - Then batteries
   - Mark as Powered if remaining need < 0.01 joules
3. This ordering ensures low-power devices (sensors, doors) stay powered during brownouts

**Phase 2: Battery Charging (Three passes)**
1. Charge input transformers from regular generators
2. Charge input transformers from output transformers
3. Charge input transformers from regular batteries

Batteries are sorted by (Capacity - JoulesAvailable) ascending, so emptier batteries charge first. Available joules are divided equally among batteries needing charge.

### Overload Detection

Per `ElectricalUtilityNetwork.UpdateOverloadTime`:

1. For each wire tier independently, check if `watts_used > max_wattage + 0.5W`
2. The 0.5W fudge factor prevents false positives from floating-point rounding
3. If overloaded: accumulate `timeOverloaded += dt`
4. After **6 seconds** continuous overload: deal 1 damage to a random wire in that tier
5. If not overloaded: decay timer at `0.95 * dt` per frame (slow decay, not instant reset)

A sustained overload of just 0.6W over threshold takes 6 seconds to cause first damage. Bouncing in/out rapidly will not accumulate meaningful damage.

### Consumer Power Loss

When a powered consumer loses power:
1. Status changes to `Unpowered`
2. Plays overdraw sound effect
3. Sets **6-second lockout timer** (`circuitOverloadTime = 6`)
4. Consumer cannot reconnect while lockout is active
5. After lockout expires, if power is available, transitions back to Powered

This prevents rapid on/off flickering when power is marginally available.

### Transformers

Transformers are composed of two parts:
- **Input side:** A Battery on the input circuit
- **Output side:** A Generator on the output circuit

The battery charges from the input circuit's generators. The generator provides power to the output circuit's consumers. Transfer is **100% efficient** (no energy loss), though the battery has standby leakage of 3.33 J/s when not operational.

**Loop detection:** If input circuit ID equals output circuit ID, "Power Loop Detected" is flagged.

### Smart Batteries

Smart batteries have hysteresis-based automation:
- Separate activate and deactivate thresholds
- Sends logic HIGH signal when charge rises above activate threshold
- Sends logic LOW signal when charge drops below deactivate threshold
- Used to control generators via automation wire

### Battery Leakage

All batteries lose energy over time via `joulesLostPerSecond`. This is reported to the energy statistics screen as wasted energy.

### Energy Tracking

All power transfers are logged to `ReportManager` for the colony statistics screen, tracking energy created, consumed, and wasted per cycle.

---

## 18. World Generation

### Generation Pipeline

1. **Initialize seeded RNG** (worldSeed, layoutSeed, terrainSeed, noiseSeed)
2. **Load settings** (YAML: worlds, subworlds, biomes, features)
3. **Generate spatial layout** using Voronoi or PowerTree diagrams
4. **Assign biomes** to overworld cells (typically 30-40 major regions)
5. **Subdivide** into terrain cells (3000-4000 leaf nodes)
6. **Generate noise maps** (base element noise, density, heat offset)
7. **Place features** (rooms, caves, geysers) into terrain cells
8. **Apply element bands** (vertical stratification within each biome)
9. **Draw world border** (neutronium walls)
10. **Render to cells** (write element/mass/temperature to cell array)
11. **Physics settling** (500 frames of gravity, pressure, heat)
12. **Place templates** (POIs, geysers, starting base)
13. **Serialize** to save file

### Voronoi Layout

The world is divided into overworld cells using Voronoi diagrams:
- Point sites are generated with configurable density and spacing
- Each site becomes a Voronoi cell representing a biome region
- `OverworldDensityMin/Max` controls site density
- `OverworldAvoidRadius` sets minimum distance between sites

Each overworld cell is further subdivided into leaf nodes (terrain cells) via a recursive Voronoi tree.

### Element Band System

Within each biome, elements are placed using a **band system** - a gradient of element types mapped to noise values:

```
noiseValue (0-1) -> lookup in element band table -> element type
```

Example band configuration for a temperate biome:
```
0.0 - 0.3: Sand (30%)
0.3 - 0.6: Sandstone (30%)
0.6 - 0.9: Igneous Rock (30%)
0.9 - 1.0: Granite (10%)
```

A noise sample of 0.45 selects Sandstone. Band sizes are relative and normalized to 0-1 range.

### Three Noise Maps

Each biome can reference three noise sources:

1. **biomeNoise** - Drives element band selection (0-1 range)
2. **densityNoise** - Varies mass per cell: `mass += mass * 0.2 * (density - 0.5)`, giving -10% to +10% variation
3. **overrideNoise** - Special element placement (100+ = katairite, 200+ = unobtanium, 300+ = void)

All noise values are normalized to local min/max before use.

### Erosion

Biomes can be tagged with erosion modifiers that create smooth transitions at boundaries:

| Tag | Effect |
|-----|--------|
| `ErodePointToCentroid` | Elements fade toward biome center |
| `ErodePointToEdge` | Elements fade toward biome edges |
| `ErodePointToBorder` | Elements fade toward world border |
| `ErodePointToWorldTop` | Top-to-bottom fade (38-58 cell transition zone) |
| `*Inv` variants | Inverse of the above |
| `*Weak` variants | Smaller fade range (7 cells vs 20) |

Erosion multiplies the base noise value, creating gradual transitions where band selections thin out near boundaries.

### Neutronium Border

The world boundary is drawn with Unobtanium (Neutronium):
- Applied to all four edges (left, right, top, bottom)
- Base thickness configured via `WorldBorderThickness`
- Thickness varies per row/column via **random walk** (+-2 cells per step, bounded by `WorldBorderRange`)
- Only overwrites non-void cells unless force mode is enabled
- Border cells are marked to prevent template/POI placement from overwriting them

### Geyser Placement

Geysers are placed via the template rules system:

1. Template rules specify which terrain cells can host geysers (filtered by tags, zone types, temperature ranges)
2. Geyser type is selected pseudo-randomly: `seed = globalWorldSeed + x + y`
3. Each geyser type has fixed parameters:
   - Element, temperature, mass per cycle range, max pressure
   - Eruption timing (pre-erupt, erupt, post-erupt durations)
   - Optional disease payload

**Example geyser types:**
| Type | Element | Temperature | Output |
|------|---------|-------------|--------|
| Steam Vent | Steam | 383 K | 500-1000 kg/cycle |
| Hot Water | Water | 368 K | 2000-4000 kg/cycle |
| Magma Volcano | Magma | 2000 K | 400-800 kg/cycle |
| Liquid CO2 | Liquid CO2 | 218 K | 100-200 kg/cycle |

### Physics Settling Pass

After all elements are placed, 500 frames of native simulation run to let the world settle:

```
for frame 0..499:
    if frame == 498: place template cells (POIs, geysers)
    SimMessages.NewGameFrame(0.2 seconds)
    read back updated cell states
```

Total simulated time: 100 seconds. During this pass:
- Heavy elements sink, light elements float
- Gases expand and stratify by density
- Liquids pool in low points
- Temperature begins to equilibrate
- State transitions can occur (water freezing in cold biomes, etc.)

Templates are placed at frame 498 (near the end) so they have 2 frames to settle but don't get disrupted by the full settling process.

---

## 19. Vacuum, Entombment, and Mass Conservation

### Vacuum Cells

Vacuum is a special element (`SimHashes.Vacuum`) representing empty space with zero mass and zero pressure. It is distinct from `SimHashes.Void`, which is a UI/placeholder element used in building logic and never placed in the simulation grid.

**Pressure at vacuum boundaries:**
```
pressure_kPa = mass_kg * 101.3
```
Vacuum cells have exactly 0 pressure. Adjacent gas or liquid cells diffuse into vacuum naturally through the standard pressure-driven flow. There is no special "rush" mechanic; the same pressure gradient logic that moves gas between unequal cells also fills vacuum. Because the pressure difference is maximal (some value vs. 0), flow into vacuum is fast.

### Entombment of Items

Only solid `ElementChunk` pickupables can be entombed. Gases and liquids are always stored as cell data and cannot be entombed.

The `EntombedItemManager` handles burial and release:

**Burial:** When a solid chunk overlaps a solid cell of the same element, and the combined mass is below `maxMass`, the chunk's mass is absorbed into the cell via `SimMessages.AddRemoveSubstance()`. Otherwise, the item is stored separately in an entombed items list with its cell, mass, temperature, and disease data.

**Release:** When a cell transitions from solid to non-solid (mining, melting, etc.), all entombed items at that cell are released. The full mass is added to the single cell via `AddRemoveSubstance()`. It does NOT spread to adjacent cells on release. The standard pressure-driven diffusion then distributes the mass over subsequent ticks.

This means digging into a high-pressure gas pocket causes a single-cell pressure spike, which then rapidly equalizes outward.

### Mass Conservation

Mass is strictly conserved in the simulation. There are no silent sinks or sources.

**Mass clamping:** When `SimMessages.ModifyCell()` receives mass exceeding `element.maxMass`, the value is silently clamped and a debug warning is logged. The excess mass is rejected (stays at the source), not destroyed.

**Three cell modification modes:**
1. **None** (AddRemoveSubstance) - Adds or removes mass from a cell. Clamped to maxMass.
2. **Replace** - Replaces the cell's element entirely. Old element is discarded (mass destroyed).
3. **ReplaceAndDisplace** - Replaces the element and physically displaces the old material: solids drop as chunks, liquids become falling particles, gases disperse to neighbors.

Mode 2 (Replace) is the only mechanism that destroys mass. It is used for deliberate replacement operations like building construction.

**ElementConsumers** and negative `AddRemoveSubstance` calls are the standard mechanisms for removing mass from cells. Mass removed from cells always goes somewhere: into building storage, pipe contents, falling particles, or creature inventory.

### Building Pressure Vulnerability

Buildings track pressure using a momentum-based reading updated every 1000ms:

```
displayPressure = previousPressure * 0.7 + currentPressure * 0.3
```

This 70/30 blend means pressure changes take multiple seconds to fully register. Buildings enter five pressure states:

| State | Condition |
|-------|-----------|
| LethalLow | Below lethal low threshold |
| WarningLow | Between lethal and warning low |
| Normal | Within operating range |
| WarningHigh | Between warning and lethal high |
| LethalHigh | Above lethal high threshold |

Buildings malfunction in LethalLow or LethalHigh states. The safe atmosphere check requires the building's element in at least 6% of occupied cells.

### Sublimation Pressure Check Asymmetry

The `GameUtil.IsEmissionBlocked()` function checks all four neighbors (below, left, right, above) for overpressure. If all four are non-gaseous or all four are over the maximum destination mass, emission is blocked.

---

## 20. Falling Physics

### Falling Liquids (FallingWater)

Liquid particles are managed by the `FallingWater` system, which tracks each particle as a physics object with position, velocity, and element properties.

**Particle creation:** When a liquid cell loses support or is displaced, `SpawnFallingLiquidInfo` creates a particle with the liquid's element, mass, temperature, and disease data.

**Physics update (per frame):**
1. Apply gravity to velocity
2. Update position by velocity * dt
3. Check collision with grid cells below
4. If collision: resolve to grid (add mass to landing cell)

**Liquid-on-liquid collision:** When a falling liquid particle hits an existing liquid cell:
- If the target cell's mass is below the element's default mass: continue falling (shallow liquid, particle passes through)
- If the target has significant mass: compare molar masses
  - Denser particle (higher molar mass) sinks through lighter liquid
  - Lighter particle stops and resolves to grid, spawning a splash effect

This molar mass comparison is why molten metal sinks through water rather than pooling on top.

**Particle mass scaling:** Particle visual size scales linearly from 25% (near-zero mass) to 100% (at `particleMassToSplit` threshold, ~75 kg).

**Render limit:** Maximum 16,249 particles can render simultaneously (limited by Unity's 65,536-vertex mesh cap at 4 vertices per particle). Beyond this, excess particles still simulate but don't render.

**World boundary enforcement:** Particles check `Grid.IsValidCellInWorld()` before updating position. Objects without the `mayLeaveWorld` flag (only duplicants have it) cannot cross world boundaries.

### Falling Solids (UnstableGroundManager)

Solid elements with the `Unstable` flag (0x08 in the state bitfield) enter a separate falling chunk system when they lose support.

**Trigger conditions:**
- Cell below becomes non-solid (mined, melted, etc.)
- Solid placed in an unsupported position
- World generation settling pass

**Physics:** Uses `GravityComponent` for per-frame position updates:
1. Accumulate velocity from gravity acceleration
2. Update position
3. Check if new position overlaps a solid cell (ground detection)
4. On landing: convert back to grid cell via `SimMessages.AddRemoveSubstance()` with `do_vertical_solid_displacement: true`

**Ground detection:** Uses collider bounds to project a check point below the object. Landing is detected when position minus ground offset minus 0.07 units (epsilon buffer) intersects a solid cell.

**Vertical displacement:** The `do_vertical_solid_displacement` flag tells the native sim to push overlapping solids upward when mass is added at the landing cell. This prevents debris from merging into existing terrain.

**Landing collision notification:** On impact, the system searches a 3x3 region above the landing point for colliders and broadcasts a collision event. This notifies nearby creatures and objects of the impact.

**Liquid drag:** Objects falling through liquid experience drag. Each object gets a deterministic variance factor based on its instance ID:
```
variance = (instanceID % 1000) / 1000 * 0.25
maxVelocity = tuning.maxVelocityInLiquid * (1.0 + variance)
```
Objects lose 8-16% of excess velocity per frame in liquid, with the exact rate varying per object to prevent lockstep movement of grouped items.

### Digging and Cell-to-Item Conversion

When a cell is mined via `Diggable`:
1. The cell's element, mass, temperature, and disease are recorded
2. `WorldDamage` accumulates damage on the cell (0.0 to 1.0)
3. When damage reaches 1.0, the cell is destroyed
4. The solid material spawns as a droppable `Pickupable` item at the cell position
5. The grid cell becomes Vacuum (mass 0)

The spawned item inherits the exact mass, temperature, and disease of the original cell. No mass is created or destroyed during mining.

### Faller Component State Machine

Physical objects (debris, items) use the `FallerComponents` system:

```
SPAWN:
  If on solid ground with zero velocity → register listener, wait
  If floating or has velocity → add gravity, start falling

FALLING:
  Gravity updates position each frame
  CellChangeMonitor fires on cell transitions
  OnSolidChanged checks ground state

LANDED:
  Remove gravity component
  Register solid-change listener (wait for ground to disappear)
```

The listener monitors a 1-cell-wide, 2-cell-tall region at the object's position for `solidChangedLayer` events. When the ground beneath disappears, the object re-enters falling state.

### Creature Falling

Creatures use `FallMonitor` instead of the generic faller system. A creature is falling if:
1. Not currently navigating (not mid-path)
2. Not in a travel tube
3. Not standing on a valid nav cell
4. Not entombed (surrounded by impassable solids)

Entombment is detected when the creature's cell or the cell above is solid and not `DupePassable`. Entombed creatures enter a trapped state rather than falling.

---

## 21. Duplicant Navigation and Movement

### Navigation Grid

The game constructs navigation meshes from the world grid. Each nav grid defines valid cell transitions for different movement types. Nav grids are static until tiles or buildings change, at which point affected regions are rebuilt.

**Navigation types (NavType enum):**

| NavType | Description |
|---------|-------------|
| Floor | Walking on solid ground |
| Ladder | Climbing ladders |
| Pole | Climbing fire poles |
| Tube | Traveling through transit tubes |
| Hover | Jet suit flight |
| Swim | Swimming in liquid |
| LeftWall | Walking on left wall surface |
| RightWall | Walking on right wall surface |
| Ceiling | Walking on ceiling (creatures only) |

### Pathfinding

The pathfinding system uses a **cost-based graph search** over the navigation grid. Each transition between cells has an associated cost that incorporates distance, movement type, and environmental penalties.

**Transition costs (base values):**

| Movement | Cost | Notes |
|----------|------|-------|
| Floor horizontal | 10 | Standard walking |
| Floor diagonal | 14 | ~sqrt(2) * 10 |
| Floor double hop (2 cells) | 20 | Jumping gaps |
| Ladder up/down | 10 | Standard climbing |
| Ladder horizontal | 15 | Side-to-side on ladder |
| Pole down | 6 | Fast sliding |
| Pole up | 50 | Very slow climbing |
| Tube straight | 5 | Fast tube travel |
| Tube entry | 40 | Mounting tube |
| Tube exit | 5-17 | Varies by exit direction |
| Hover any direction | 8-9 | Jet suit flight |
| Fall initiation | 14 | Starting to fall |
| Fall landing | 1 | Touching down |

**Submerged penalty:** Duplicants without a protective suit (atmo suit, jet suit, lead suit) pay **2x the base transition cost** when traveling through liquid cells. This makes the pathfinder strongly prefer dry routes.

**Creature underwater limit:** Creatures have a `MaxUnderwaterTravelCost` attribute that caps how far they can path through submerged cells.

### Cell Validation

A cell is walkable (Floor nav type) if:
1. The cell is valid and passable (not a solid building)
2. AND one of:
   - Solid ground below (`Grid.Solid[below]` and not `DupePassable`)
   - Fake floor below (`Grid.FakeFloor[below]`)
   - Ladder or pole present

Wall climbing requires a solid anchor cell to the left or right. Ceiling walking requires a solid block above.

Swimming activates when a duplicant is substantially submerged (`SubmergedMonitor` forces NavType to Swim).

### Movement Speed

**Tile speed multipliers:**

| Tile | Multiplier |
|------|-----------|
| Metal Tile | 1.5x |
| Plastic Tile | 1.5x |
| Wood Tile | 1.25x |
| Floor Switch | 1.25x |
| Carpet Tile | 0.75x |
| Storage Tile | 0.75x |
| Wire Bridge (Heavy-Watt) | 0.5x |
| Travel Tube Wall Bridge | 0.5x |

These multipliers are applied by `SimCellOccupier` on the building's occupied cells.

**Transit tube speed:** Base 18 units/second. Waxed tubes receive a 25% boost (22.5 units/second).

**Athletics attribute:** Trainable skill that directly modifies movement speed. Gained through physical activity and skill point allocation.

### Access Control

Door permissions are checked during pathfinding via `Grid.HasPermission()`:

1. Check if cell has an access door (`Grid.HasAccessDoor[cell]`)
2. Determine travel direction from source to destination cell
3. Check door orientation (vertical doors check left/right, horizontal doors check up/down)
4. Verify the duplicant's ID against the door's permission list

**Permission types:** Both (bidirectional), GoLeft, GoRight, Neither (fully locked).

Robots use a separate permission tag (`GameTags.Robot`) and can have distinct access rules from duplicants.

### Suit Requirements

Suit markers define zones requiring specific equipment:

| Suit Type | Flag | Capability |
|-----------|------|------------|
| Atmo Suit | HasAtmoSuit | Standard protection |
| Jet Suit | HasJetPack | Enables Hover nav type |
| Oxygen Mask | HasOxygenMask | Limited protection |
| Lead Suit | HasLeadSuit | Radiation protection |

When a suit marker is operational, duplicants cannot traverse in the restricted direction without the required suit. The `OnlyTraverseIfUnequipAvailable` flag also requires a locker on the far side to store the suit when leaving the zone.

### Jet Suit Mechanics

Jet suits consume fuel at 0.2 kg/s while hovering. When fuel reaches 0:
- Navigator is forced back to Floor nav type
- `GameTags.JetSuitOutOfFuel` is applied
- Duplicant falls and must walk to a jet suit dock

Hover transitions cost 8 (straight) or 9 (diagonal), making jet suit travel faster than walking (cost 10-14) but slower than tubes (cost 5).

### Falling Duplicants

When a duplicant loses ground support:
- `FallMonitor` detects the unsupported state
- `GravityComponent` applies falling physics with accumulating velocity
- On landing, recovery checks for nearby ladders or poles within 1-2 cells
- If extremely stuck, the system tracks up to 3 previous safe positions for recovery pathfinding

Fall damage is handled through the health/damage system on landing impact.

---

## 22. Creature AI and Metabolism

### Brain Architecture

Creature AI is driven by `CreatureBrain`, which extends `Brain`. Each creature has a `ChoreTable` that defines an ordered list of behaviors. The brain evaluates behaviors top-to-bottom, executing the highest-priority applicable behavior.

**Behavior priority order (typical creature):**

| Priority | Behavior | Description |
|----------|----------|-------------|
| 1 | DeathStates | Dying and death |
| 2 | AnimInterruptStates | Animation interrupts |
| 3 | ExitBurrowStates | Emerging from ground |
| 4 | GrowUpStates | Baby maturing (babies only) |
| 5 | TrappedStates | Caught in trap |
| 6 | IncubatingStates | In incubator (babies only) |
| 7 | BaggedStates | Picked up by duplicant |
| 8 | FallStates | Falling |
| 9 | StunnedStates | Stunned |
| 10 | DrowningStates | Drowning |
| 11 | FleeStates | Fleeing threats |
| 12 | AttackStates | Fighting (adults only) |
| 13 | CreatureSleepStates | Sleeping |
| 14 | FixedCaptureStates | Being wrangled |
| 15 | RanchedStates | Being groomed (adults only) |
| 16 | LayEggStates | Laying eggs (adults only) |
| 17 | EatStates | Eating |
| 18 | PlayStates | Using recreation |
| 19 | CritterCondoStates | Using condo (adults only) |
| 20 | IdleStates | Default wandering |

All behaviors are `GameStateMachine`-based, with type-safe state definitions and tag-driven transitions.

### Diet and Metabolism

Creature diets are defined as arrays of `Diet.Info` entries, each specifying:
- **Consumed tags:** What the creature accepts (element tags, food tags, ore tags)
- **Produced element:** What the creature excretes
- **Calories per kg:** Energy gained from consumption
- **Conversion rate:** Mass ratio of output to input (e.g., 0.5 = 50% mass retained as excrement)
- **Disease:** Optional pathogen added to output with specified concentration

**Example (Hatch diets):**
- Basic Rock Diet: Sand, Sandstone, Clay, Dirt, etc. → produces excrement at configured ratio
- Metal Diet: Ore tags → produces refined metals (CopperOre → Copper)
- Hard Rock Diet: Granite, Obsidian, Igneous Rock → produces excrement

The `CreatureCalorieMonitor` tracks hunger as an Amount (0 to species-specific max):

**State machine:**
```
hungry.outOfCalories → hungry.hungry → satisfied → eating
```

- **Satisfied:** Calories above hunger threshold. Base metabolism burns calories over time.
- **Hungry:** Below threshold. Creature seeks food. Metabolism rate may change.
- **Out of Calories:** Starvation. Creature takes damage over time until fed or dead.
  - Wild creatures: lifespan penalty, reduced reproduction
  - Tame creatures: die from starvation

**Metabolism modifiers:**
- Tame creatures burn calories faster than wild ones
- Temperature stress increases metabolic rate
- `GameTags.Creatures.PausedHunger` can suspend hunger entirely

### Reproduction

Reproduction is handled by `FertilityMonitor` (egg-laying) and `IncubationMonitor` (egg hatching).

**Fertility cycle:**
1. Fertility amount increases from 0 to 100 over time (base rate: 0.008375 per update)
2. Rate modified by happiness, temperature comfort, and feeding status
3. At 100%: creature lays an egg and fertility resets to 0

**Egg type selection:** Each creature defines possible egg types with base weights. Environment modifiers shift weights:
- Element exposure (atmosphere type) can boost specific egg variants
- Diet type can influence which subspecies egg is laid
- Modifiers are applied via `OnUpdateEggChances` events each cycle

**Incubation:**
- Eggs have an incubation amount (0 to 100)
- Base rate: 0.01675 per update
- Incubators apply a speed multiplier
- Lullabied eggs receive a bonus
- Temperature outside viable range pauses incubation

**Egg viability:** Eggs track a viability amount. If the egg's temperature is outside its viable range for too long, viability drops. At 0 viability, the egg dies without hatching.

### Wildness and Domestication

`WildnessMonitor` tracks the wild-to-tame transition:

**Wildness amount:** 0 (fully tame) to 100 (fully wild), starting at 100 for wild-spawned creatures.

**Taming:** Each grooming interaction by a rancher reduces wildness. Rate depends on rancher skill. When wildness reaches 0, the creature becomes domesticated.

**Wild vs tame effects:**

| Aspect | Wild | Tame |
|--------|------|------|
| Lifespan | Longer (species-specific) | Shorter |
| Metabolism | Slower calorie burn | Faster calorie burn |
| Reproduction | Slower fertility rate | Faster fertility rate |
| Starvation | Survives longer | Dies sooner |

Wild creatures revert wildness upward if not groomed. The wildness amount ticks up at a base rate, so continuous grooming is required to maintain domestication.

### Age and Lifespan

`AgeMonitor` tracks creature aging:

- Age amount increases continuously (base rate: 0.1675 per update)
- Maximum age is species-specific, with separate wild and domestic values
- At max age: creature dies of old age
- Baby creatures track a separate `Maturity` amount; at 100%, they grow into adults

**Growth:** Baby-to-adult transition triggers `GrowUpStates`, which replaces the baby prefab with the adult prefab, preserving temperature, disease, and taming status.

### Happiness and Mood

`HappinessMonitor` tracks creature mood on a simple scale:

| State | Happiness Value | Effect |
|-------|----------------|--------|
| Happy | > 4 | Increased reproduction rate |
| Neutral | 0 to 4 | Normal behavior |
| Unhappy | < 0 | Reduced/no reproduction |

**Happiness modifiers:**
- Correct temperature: +1 per comfortable state
- Overcrowded: -1 per excess creature in room
- Fed (tame): +1 when not hungry
- Groomed: +1 from recent grooming
- Temperature stress: -1 (uncomfortable), -2 (deadly range)

The `CritterEmoteMonitor` displays creature emotions as thought bubbles. Emotes have 30-second cooldowns and prioritize negative emotions over positive ones. Expression intervals range from 37.5 to 75 seconds.

### Temperature Comfort

`CritterTemperatureMonitor` tracks thermal comfort:

**States:**
```
comfortable ↔ hot.uncomfortable ↔ hot.deadly
           ↔ cold.uncomfortable ↔ cold.deadly → dead
```

- Internal temperature: creature's `PrimaryElement.Temperature`
- External temperature: average of occupied cells (or internal temp if in vacuum)
- Deadly state inflicts configurable damage per second with 1-second cooldown
- Each temperature state applies a happiness modifier

### Threat Response

`ThreatMonitor` handles fight-or-flight decisions:

**Detection:**
- Scans up to 20 cells (configurable `maxSearchDistance`)
- Checks up to 50 entities (configurable `maxSearchEntities`)
- Identifies threats based on creature tags (excluding `friendlyCreatureTags`)

**Grudge system:** Creatures hold 10-second grudges against attackers. The grudge target becomes the priority threat regardless of distance. Grudges expire with a "forgiveness" PopFX.

**Response states:**
```
safe.passive → threatened.creature (flee or fight based on species)
safe.seeking → threatened.duplicant (flee or fight based on species)
```

Flee threshold is configurable per species. Some creatures always fight (e.g., adult Hatches when attacked), some always flee (e.g., Pufts).

### Scale Growth and Molting

`ScaleGrowthMonitor` handles renewable resource harvesting:

- Growth tracked as 0-100% amount
- Rate: `defaultGrowthRate` percentage per cycle
- **Atmosphere requirement:** Some creatures require specific gas for scale growth; wrong atmosphere triggers stunted state (no growth)
- At 100%: creature can be sheared, dropping `itemDroppedOnShear` with configured mass
- Shearing resets growth to 0

Visual scale levels update in 5 stages as growth progresses.

### Death

`DeathMonitor` handles the death process:

**Triggers:** Starvation (calories reach 0), temperature damage (HP reaches 0), old age (age reaches max), drowning, explicit kill.

**Process:**
1. Creature enters dying state, plays death animation
2. Tags `GameTags.Dead` and `GameTags.Corpse` are applied
3. Creature drops to ground state
4. No carrying mechanics for creature corpses (unlike duplicants)

Creature deaths are silent (no notification), unlike duplicant deaths which trigger alerts and sound effects.

---

## 23. Performance and Active Regions

### Active Region System

Only cells within the **active region** are simulated each frame. The `NewGameFrame` message specifies which rectangular regions to simulate. Cells outside these regions are paused.

This is the primary performance optimization. Large bases with many exposed cells cause performance drops because more cells enter the active region.

### Simulation Tick Scheduling

The game uses tiered update rates to balance performance:

| Interface | Frequency | Typical Users |
|-----------|-----------|---------------|
| `ISimEveryTick` | Every frame | Critical real-time systems |
| `ISim33ms` | Every 33ms | Fast-response systems |
| `ISim200ms` | Every 200ms | Most building logic, pipe flow, power |
| `ISim1000ms` | Every 1s | Slower updates, radiation checks |
| `ISim4000ms` | Every 4s | Infrequent checks |
| `ISlicedSim1000ms` | Distributed 1s | Load-balanced across frames |

`SlicedUpdaterSim1000ms` distributes updates across multiple frames to prevent all buildings from updating simultaneously.

---

## 24. Formula Reference

### Heat Transfer

| Formula | Purpose |
|---------|---------|
| `EnergyFlow = (T1 - T2) * min(k1, k2) * (A / d)` | Core heat transfer between cells |
| `deltaT = kJ / (SHC * mass)` | Temperature change from energy input |
| `FinalTemp = (m1*T1 + m2*T2) / (m1 + m2)` | Standard mixing (SimUtil, clamped to [min,max]) |
| `FinalTemp = sum(m*T*SHC) / sum(m*SHC)` | SHC-weighted mixing (ElementConverter only) |
| `HeatCapacity = mass * SHC` | Thermal mass of a material |
| `ExhaustKJ = (kW * dt / cells) * (min(mass, 1.5) / 1.5)` | Building exhaust heat distribution |
| `CellWeight = 1 + (maxDist - abs(dx) - abs(dy))` | Area heater weighting |
| `DeltaT_clamped = clamp(T + deltaT, T, Ttarget)` | Clamped heating/cooling (no overshoot) |

### Disease

| Formula | Purpose |
|---------|---------|
| `ContractionChance = 0.5 - 0.5 * tanh(0.25 * rating)` | Disease contraction probability |
| `Resistance = base + attribute + tier_bonus` | Total resistance rating |
| `GermTransfer = totalGerms * (massFraction)` | Germs transferred with mass |
| `DiseaseWinner = argmax(strength * count)` | Disease mixing competition |

### Radiation

| Formula | Purpose |
|---------|---------|
| `Absorption = pow(mass, 1.25) * molarMass / 1000000` | Per-cell radiation absorption |
| `RadDecay = rads * (1 - absorption) * random(0.96, 0.98)` | Radiation decay per cell along ray |
| `Exposure = cellRad * (1 - resistance) / 600 * dt` | Duplicant radiation exposure per frame |
| `HEPCharge = (cellRad / 600) * sampleRate * 0.1` | HEP spawner absorption rate |

### Power

| Formula | Purpose |
|---------|---------|
| `Joules = Watts * dt` | Energy per tick |
| `OverloadThreshold = maxWattage + 0.5` | Wire overload detection |
| `OverloadDamageTime = 6 seconds` | Time to first wire damage |
| `ConsumerLockout = 6 seconds` | Brownout recovery delay |

### Pressure

| Formula | Purpose |
|---------|---------|
| `Pressure = mass * element.defaultPressure` | Mass to pressure conversion |
| `Mass = pressure / element.defaultPressure` | Pressure to mass conversion |
| `EarDamageThreshold = 4 kg gas mass` | Popped Ear Drums condition |

### Pipe Flow

| Formula | Purpose |
|---------|---------|
| `FlowRate = min(availableMass, targetCapacity)` | Per-tick mass transfer |
| `MaxLiquid = 10 kg per cell` | Liquid pipe capacity |
| `MaxGas = 1 kg per cell` | Gas pipe capacity |
| `MixTemp = (m1*T1 + m2*T2) / (m1 + m2)` | Pipe content temperature mixing |

### Light, Decor, and Noise

| Formula | Purpose |
|---------|---------|
| `Lux = intensity / (falloffRate * max(dist, 1))` | Light intensity at distance |
| `LitDecorBonus = +15 if LightIntensity > 0` | Automatic decor bonus from lighting |
| `NoiseFalloff = sourceDB - (sourceDB * dist * 0.05)` | Noise decay with distance |

### Element Conversion

| Formula | Purpose |
|---------|---------|
| `OutputMass = rate * multiplier * speed * dt * fraction` | Converter output calculation |
| `OutputTemp = sum(m*T*SHC) / sum(m*SHC)` | Weighted input temperature for outputs |
| `DiseaseShare = weight / totalWeight * inputCount` | Disease distribution across outputs |

### Falling Physics

| Formula | Purpose |
|---------|---------|
| `ParticleScale = lerp(0.25, 1.0, clamp01(mass / 75))` | Liquid particle visual size |
| `LiquidDragVariance = (instanceID % 1000) / 1000 * 0.25` | Per-object drag randomization |
| `MaxVelInLiquid = base * (1.0 + variance)` | Terminal velocity in liquid |
| `LandingEpsilon = 0.07 units` | Ground detection buffer |
| `MaxRenderParticles = 16249` | Unity vertex limit / 4 |

### Navigation

| Formula | Purpose |
|---------|---------|
| `SubmergedCost = baseCost * 2` | Penalty for unprotected underwater travel |
| `TubeSpeed = 18 * (1 + waxBonus)` | Transit tube velocity (waxBonus = 0.25) |
| `BuildingPressure = prev * 0.7 + current * 0.3` | Pressure reading momentum blend |
| `JetFuelRate = 0.2 kg/s` | Jet suit fuel consumption |

### Creature Metabolism

| Formula | Purpose |
|---------|---------|
| `Amount = prev + (baseRate * (1 + sumMultipliers) + sumAdditives) * dt` | Generic amount update |
| `OutputMass = consumedMass * conversionRate` | Diet excretion mass |
| `WildnessDecay = groomingReduction per interaction` | Taming progress |

---

*This guide was generated from decompiled ONI source code (Assembly-CSharp.dll). The native C++ simulation (SimDLL.dll) is not decompiled, so some physics details are inferred from the C# interface layer and message structures. Actual behavior may differ in edge cases where the native sim applies additional logic not visible in the managed code.*
