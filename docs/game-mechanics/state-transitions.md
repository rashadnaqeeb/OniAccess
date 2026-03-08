# State Transitions

How elements change phase (melting, boiling, freezing, condensation) and special emission systems. Derived from decompiled source code.

## Phase Transitions

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

State transitions are computed by the native C++ simulation, not the C# side. The Element class only stores the thresholds and target references. The native sim checks temperatures against thresholds every frame and performs transitions automatically, conserving energy across the change.

## Sublimation (Solids to Gas)

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

**Temperature blocking:** Sublimation is blocked if the source material's temperature is at or below `element.lowTemp`. The material must be warm enough for the target gas to exist.

**Sealed containers:** Items tagged `GameTags.Sealed` do not sublimate (unless the storage has decay enabled and is corrosion-proof).

**Disease transfer:** When mass sublimates, disease germs from the source transfer proportionally with the emitted mass.

**State machine:** The Sublimates component has three states:
1. **Emitting** - Normal emission occurring
2. **BlockedOnPressure** - Destination cell is full
3. **BlockedOnTemperature** - Source material is too cold

## Off-Gassing (Liquids to Gas)

Liquids use the `offGasPercentage` property instead of the sublimation system. This represents the percentage of liquid that converts to gas per frame. Unlike sublimation, off-gassing uses a different rate control mechanism and is also subject to overpressure blocking.
