# Grid and Elements

How ONI's simulation grid works and what elements are made of. Derived from decompiled source code.

## Architecture Overview

ONI uses a hybrid simulation architecture split between two layers:

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

The two layers communicate through a message-passing interface. The C# side sends structured messages (e.g., `ModifyCell`, `AddElementConsumer`, `ModifyEnergy`) to the native DLL, and the DLL sends back callbacks (e.g., `MassConsumedCallback`, `MassEmittedCallback`, state change notifications). Neither side directly accesses the other's memory.

### Simulation Tick Rates

| Interface | Frequency | Typical Users |
|-----------|-----------|---------------|
| `ISimEveryTick` | Every frame | Critical real-time systems |
| `ISim33ms` | Every 33ms | Fast-response systems |
| `ISim200ms` | Every 200ms | Most building logic, pipe flow, power |
| `ISim1000ms` | Every 1s | Slower updates, radiation checks |
| `ISim4000ms` | Every 4s | Infrequent checks |
| `ISlicedSim1000ms` | Distributed 1s | Load-balanced across frames |

`SlicedUpdaterSim1000ms` distributes updates across multiple frames to prevent all buildings from updating simultaneously.

## The Grid

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
- Mass is rejected if it exceeds `maxMass` on elements whose `maxMass` is 0 (e.g., Vacuum); for other elements no ModifyCell-side mass clamping occurs
- Invalid values are logged as warnings and reset to element defaults

## Elements

Every material in the game is defined as an `Element` object with extensive physical properties. Element definitions are loaded from YAML files at startup, processed by `ElementLoader`, and referenced by their `SimHashes` enum value (a 32-bit hash).

There are approximately 191 elements total (per `SimHashes` enum), split roughly into:
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
