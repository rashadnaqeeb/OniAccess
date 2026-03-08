# Oxygen and Breathing

How duplicants consume oxygen, exhale CO2, and suffocate. Derived from decompiled source code.

## Overview

Every standard duplicant has an `OxygenBreather` component that runs on a 200ms tick. Each tick, it consumes oxygen from the environment (or a suit tank), converts a fraction to CO2, and emits the CO2 at the duplicant's mouth position. A parallel state machine (`SuffocationMonitor`) tracks a breath meter that drains when no oxygen is available and kills the duplicant when it empties.

## Oxygen Consumption

**Base rate**: 0.1 kg/s (`DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND`)

The actual rate comes from the `AirConsumptionRate` attribute, which starts at 0.1 kg/s and can be modified by traits. Each 200ms tick, `OxygenBreather.Sim200ms` consumes `rate * dt` kg from the current gas provider.

### Trait Modifiers

| Trait | Modifier | Effective Rate |
|-------|----------|---------------|
| Base (no trait) | 0 | 0.1 kg/s |
| Ellie (congenital) | -0.045 kg/s (-45%) | 0.055 kg/s |
| Diver's Lungs (good) | -0.025 kg/s (-25%) | 0.075 kg/s |
| Deeper Diver's Lungs (gene shuffler) | -0.05 kg/s (-50%) | 0.05 kg/s |
| Mouth Breather (bad) | +0.1 kg/s (+100%) | 0.2 kg/s |

Source: `TUNING.TRAITS` trait definitions.

### Breathable Gases

The game tags certain elements as `GameTags.Breathable`. Only gases with this tag can be consumed by `GasBreatherFromWorldProvider`. From the element descriptions:
- **Oxygen** (SimHashes.Oxygen) - breathable
- **Polluted Oxygen** (SimHashes.ContaminatedOxygen) - breathable, but triggers `GameHashes.PoorAirQuality` event on each breath, which can cause disease exposure

Non-breathable gases (CO2, hydrogen, chlorine, natural gas, etc.) provide no oxygen. Vacuum provides no oxygen.

### Minimum Breathable Mass

The `noOxygenThreshold` field on `OxygenBreather` determines the minimum gas mass in a cell for it to count as breathable. Set from `BASESTATS.NO_OXYGEN_THRESHOLD` = **0.05 kg**.

The `lowOxygenThreshold` for the "Low Oxygen" warning is `BASESTATS.LOW_OXYGEN_THRESHOLD` = **0.52 kg**.

### Breathable Cell Selection

`GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell` checks 6 cells around the duplicant for breathable gas and picks the one with the highest mass (above `noOxygenThreshold`):

| Offset | Position |
|--------|----------|
| (0, 0) | Feet |
| (0, 1) | Head |
| (1, 1) | Upper-right |
| (-1, 1) | Upper-left |
| (1, 0) | Right |
| (-1, 0) | Left |

If no cell has breathable gas above the threshold, the duplicant is considered to have no oxygen.

## CO2 Exhalation

**Conversion ratio**: `O2toCO2conversion` = 0.02 (set from `BASESTATS.OXYGEN_TO_CO2_CONVERSION`)

This means for every 1 kg of O2 consumed, 0.02 kg of CO2 is produced. At the base rate of 0.1 kg/s O2, that is **0.002 kg/s CO2**.

Note: Despite the field name `O2toCO2conversion`, this is not a 1:1 mass ratio. It is a multiplier applied to the consumed O2 mass: `co2_produced = o2_consumed * 0.02`.

**Minimum emission batch**: `minCO2ToEmit` = 0.02 kg (from `BASESTATS.MIN_CO2_TO_EMIT`). CO2 accumulates in `accumulatedCO2` until it reaches this threshold, then emits all at once.

**Emission temperature**: CO2 is emitted at the duplicant's current body temperature (`temperature.value` from the Temperature amount).

**Emission position**: At the duplicant's mouth offset (0.25, 0.97) relative to transform, flipped when facing left. The CO2 is spawned via `CO2Manager.instance.SpawnBreath`, which creates a visual CO2 puff that falls downward (affected by gravity acceleration) for up to 3 seconds before depositing into the grid cell as actual CO2 gas mass.

**Transit tube exception**: When the duplicant is in a transit tube (`NavType.Tube`), `ShouldEmitCO2()` returns false. The gas is consumed but no CO2 is emitted.

## Breath Meter and Suffocation

Two state machines manage breath:

### SuffocationMonitor

Tracks whether the duplicant can breathe and manages the breath meter.

**States**:
- `satisfied.normal` - Breathing normally. Breath meter increases at `BREATH_RATE`.
- `satisfied.low` - Breathing but oxygen is low. Applies "LowOxygen" effect.
- `noOxygen.holdingbreath` - No breathable gas. Breath meter decreases at `BREATH_RATE`. Shows "Holding Breath" status.
- `noOxygen.suffocating` - Breath is critically low. Shows "Suffocating" status.
- `death` - Breath meter reached 0. Duplicant dies of suffocation.

**Breath meter values** (derived from `DUPLICANTSTATS.STANDARD.Breath`):

| Parameter | Value | Derivation |
|-----------|-------|-----------|
| Total bar amount | 100 | `BREATH_BAR_TOTAL_AMOUNT` |
| Total bar duration | 110 seconds | `BREATH_BAR_TOTAL_SECONDS` |
| Breath rate | ~0.909/s | 100 / 110 |
| Retreat threshold | ~72.7 | 80s / 110s * 100 |
| Suffocation warning threshold | ~45.5 | 50s / 110s * 100 |

In practice: a duplicant can hold their breath for **110 seconds** (1 minute 50 seconds) from full to death. The "suffocating" status appears at **50 seconds remaining**. With Red Alert active, the retreat threshold drops to the suffocation threshold (50s remaining instead of 80s remaining).

**HasAir delay**: The `OxygenBreather` uses a 2-second timer before toggling the `hasAir` state. This prevents rapid flickering between breathing and not-breathing when at the edge of an oxygen pocket.

### BreathMonitor

Manages the duplicant's response to low breath: triggers the `RecoverBreath` urge and finds a safe cell with breathable air.

When breath drops below `RETREAT_AMOUNT` (~72.7), the duplicant:
1. Gets the `RecoverBreath` urge and `Suffocating` thought bubble
2. Searches for the nearest breathable cell using `SafetyQuery`
3. Creates a `RecoverBreathChore` (compulsory priority) to move to that cell
4. Gets an additional breath delta of `RECOVER_BREATH_DELTA` = **3.0/s** on top of the normal breathing rate (0.909/s), for a total recovery rate of ~3.909/s
5. During recovery, tagged with `GameTags.RecoveringBreath`, which makes `HasOxygen` return true regardless of actual gas

During Red Alert, the retreat threshold is lowered to `SUFFOCATE_AMOUNT` (~45.5), so duplicants keep working longer before retreating.

### Death

When `breath.value` reaches 0, `SuffocationMonitor` transitions to the `death` state and calls `Kill()` with `Deaths.Suffocation`.

## Suit Oxygen Supply

When a duplicant equips a suit, the `SuitTank` component is added as a higher-priority gas provider on the `OxygenBreather.gasProviders` stack. The provider stack is checked from top (most recent) to bottom; the first non-blocked provider with oxygen is used.

When a suit tank is equipped, the `GasBreatherFromWorldProvider` becomes blocked (returns `IsBlocked() = true` when `GameTags.HasSuitTank` is present), so the duplicant breathes exclusively from the tank.

### Atmo Suit

- **Tank capacity**: `0.1 * 600 * 1.25` = **75 kg** O2
- Duration at base rate: 75 / 0.1 = **750 seconds** (1.25 cycles)
- Tagged `AirtightSuit` - CO2 is stored in the suit's `Storage` component instead of emitted into the world
- Refill threshold: below 25% (18.75 kg)
- Low oxygen warning: below 25%

### Jet Suit

- **O2 tank capacity**: same formula = **75 kg** O2 (uses `SuitTank` with identical capacity)
- Also has a separate fuel tank (`JetSuitTank`): 100 kg petroleum capacity
- Tagged `AirtightSuit` - same CO2 storage behavior as atmo suit

### Lead Suit

- **O2 tank capacity**: `0.1 * 400` = **40 kg** O2
- Duration at base rate: 40 / 0.1 = **400 seconds** (~0.67 cycles)
- Also has a battery with 200s duration
- Tagged `AirtightSuit`

### Oxygen Mask

- **Tank capacity**: **20 kg** O2 (hardcoded)
- Duration at base rate: 20 / 0.1 = **200 seconds** (~0.33 cycles)
- NOT tagged `AirtightSuit` - CO2 is emitted into the world normally (non-airtight)
- Auto-unequips when tank is empty
- Leaks O2 at 0.1 kg/s when not assigned to a duplicant
- Self-destructs when empty

### CO2 Handling by Suit Type

| Suit Type | Airtight | CO2 Behavior |
|-----------|----------|-------------|
| Atmo Suit | Yes | Stored in suit storage as CO2 gas chunks |
| Jet Suit | Yes | Stored in suit storage |
| Lead Suit | Yes | Stored in suit storage |
| Oxygen Mask | No | Emitted into world at mouth position |

For airtight suits, `ShouldStoreCO2()` returns true. The CO2 is added to the suit's `Storage` component at the duplicant's body temperature, in batches of `minCO2ToEmit` (0.02 kg). When the suit is removed, stored CO2 is dropped.

## Gas Provider Priority

`OxygenBreather.GetCurrentGasProvider()` iterates the provider list from the end (highest priority) to the start. It selects the first non-blocked provider, preferring one that `HasOxygen()`. The priority order on a suited duplicant:

1. **SuitTank** (added last when equipped, highest priority, never blocked)
2. **GasBreatherFromWorldProvider** (added on spawn, blocked when `HasSuitTank` tag present)

If the suit tank runs empty, `SuitTank.HasOxygen()` returns false, but `GasBreatherFromWorldProvider.IsBlocked()` still returns true (the tag is still present). The duplicant effectively has no oxygen source until the suit is removed or refilled.

## Polluted Oxygen

Polluted Oxygen (ContaminatedOxygen) is breathable but triggers `GameHashes.PoorAirQuality` each tick the duplicant breathes it. This event is used by the disease system for Slimelung exposure: breathing polluted oxygen containing Slimelung germs can infect the duplicant.

Polluted Oxygen is consumed and produces CO2 identically to clean Oxygen. There is no difference in consumption rate or CO2 output. The only mechanical difference is the disease exposure pathway.

## Bionic Duplicants

Bionic duplicants (`BionicMinionConfig`) have a separate oxygen system (`BionicOxygenTankMonitor`, `BionicSuffocationMonitor`) and do not use `BreathMonitor` or `RecoverBreathChore`. They absorb oxygen in bulk into an internal tank rather than breathing continuously from the environment. The `RecoverBreathChore` explicitly excludes bionics via `ChorePreconditions.instance.IsNotABionic`.

## Key Constants Summary

| Constant | Value | Source |
|----------|-------|--------|
| O2 consumption rate | 0.1 kg/s | `BASESTATS.OXYGEN_USED_PER_SECOND` |
| O2 to CO2 ratio | 0.02 | `BASESTATS.OXYGEN_TO_CO2_CONVERSION` |
| CO2 emission batch size | 0.02 kg | `BASESTATS.MIN_CO2_TO_EMIT` |
| Low O2 threshold | 0.52 kg | `BASESTATS.LOW_OXYGEN_THRESHOLD` |
| No O2 threshold | 0.05 kg | `BASESTATS.NO_OXYGEN_THRESHOLD` |
| Breath bar total | 100 | `BREATH.BREATH_BAR_TOTAL_AMOUNT` |
| Breath bar duration | 110 s | `BREATH.BREATH_BAR_TOTAL_SECONDS` |
| Retreat at | 80 s remaining | `BREATH.RETREAT_AT_SECONDS` |
| Suffocation warning at | 50 s remaining | `BREATH.SUFFOCATION_WARN_AT_SECONDS` |
| Breath recovery rate | 3.0/s | `BASESTATS.RECOVER_BREATH_DELTA` |
| HasAir toggle delay | 2 s | Hardcoded in `OxygenBreather.Sim200ms` |
| Atmo suit O2 capacity | 75 kg | `0.1 * 600 * 1.25` |
| Jet suit O2 capacity | 75 kg | Same formula |
| Lead suit O2 capacity | 40 kg | `0.1 * 400` |
| Oxygen mask capacity | 20 kg | Hardcoded |
| CO2 puff lifetime | 3 s | `CO2Manager.CO2Lifetime` |
