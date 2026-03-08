# Building Mechanics

How buildings convert elements, occupy cells, take damage, and interact with the simulation -- systems shared by all buildings. For per-building specs (power, inputs, outputs, rates), see `building-catalog.md`. Derived from decompiled source code.

## Element Conversion and Consumption

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
2. **Weighted input temperature** (`useEntityTemperature = false`, and inputs consumed OR `minOutputTemperature > 0`): Calculates SHC-weighted average from consumed elements, then takes `max(minOutputTemperature, weightedAverage)`. If no inputs were consumed, weightedAverage is 0, so minOutputTemperature is used directly.
3. **Fallback to entity temperature**: If `useEntityTemperature = false` but no inputs exist AND `minOutputTemperature = 0`, falls back to entity temperature.

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

Registers with the native sim to emit elements at a configured cell position (building center + offset vector). Normal emission is handled entirely by the native sim via `SimMessages.ModifyElementEmitter`, which takes emission frequency, mass rate, temperature, and max pressure. The sim handles element placement and pressure checks internally.

Tracks blocked state (`isEmitterBlocked`) when the target cell is full. The native sim invokes callbacks to toggle this state, and the building displays "Blocked" status.

A separate `ForceEmit` method bypasses the native emitter: uses `SimMessages.AddRemoveSubstance()` for gas and liquid, `SpawnResource` for solids.

`BuildingElementEmitter` is a simpler variant for creatures and basic buildings that always uses a fixed temperature (not computed from inputs).

### EntityElementExchanger

A lightweight system for creature metabolism (e.g., plants absorbing CO2, emitting O2):
- Single input element + single output element
- Simple mass ratio: `outputMass = consumedMass * exchangeRatio`
- Discards all disease (emits with no disease index and zero count)
- State machine with `exchanging` and `paused` states (pauses when creature wilts)

## Building Cell Occupation

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

## Entombment

The `EntombVulnerable` component detects when a building or creature is completely surrounded by solids:
- Checks all occupied cells for `Grid.Solid[cell]`
- If any cell is solid-blocked, the entity is entombed
- Sets `notEntombedFlag` operational flag (disables building function)
- Tiles are immune (`Entombable = false` in BuildingDef)

## Flooding

The `Floodable` component detects liquid in occupied cells:
- Checks each cell for "substantial liquid": `mass >= element.defaultValues.mass * 0.35`. For Water (default mass 1 kg), this equals 0.35 kg, but varies by liquid type
- If any cell exceeds threshold, building enters flooded state
- Sets `notFloodedFlag` operational flag (building stops working)
- Tiles are immune (`Floodable = false`)

## Building Damage

The `BuildingHP` component tracks structural integrity:

1. **WorldDamage** accumulates damage in `Grid.Damage[cell]` (float, 0.0 to 1.0)
2. When cell damage exceeds 0.15, retrieves building from `Grid.Objects[cell]`
3. Calls `BuildingHP.DoDamage()` to decrement hitpoints
4. When HP reaches 0: building enters "damaged" state, shows notification
5. When `Grid.Damage[cell] >= 1.0`: cell is destroyed entirely

**Repair synchronization:** When duplicants repair a building, `SimCellOccupier` listens to repair events and calls `WorldDamage.RestoreDamageToValue()` to keep cell structural damage proportional to building HP: `cellDamage = 1.0 - (currentHP / maxHP)`.

## Building Destruction Cleanup

When `SimCellOccupier.DestroySelf()` is called:
1. Clears cell property flags
2. If the cell still holds the building's element: replaces with Vacuum, transfers disease
3. If the element was already changed: marks cell as non-solid
4. Triggers solid change events for world update
