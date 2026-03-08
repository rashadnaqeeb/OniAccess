# Geysers and Vents

How geysers, vents, and volcanoes emit resources and cycle through eruption states. Derived from decompiled source code (`Geyser.cs`, `GeyserConfigurator.cs`, `GeyserGenericConfig.cs`, `Studyable.cs`, `ElementEmitter.cs`, `GeoTunerConfig.cs`).

## State Machine

Each geyser runs a `Geyser.States` state machine with these states:

```
dormant -> pre_erupt -> erupt (erupting | overpressure) -> post_erupt -> idle -> ...
```

**dormant** - Geyser is in its yearly off-period. Transitions to `pre_erupt` after `RemainingDormantTime()` expires.

**idle** - Geyser is in its yearly active period but between eruptions (iteration off-phase). On entry, checks `ShouldGoDormant()` and may jump straight to `dormant`. Otherwise transitions to `pre_erupt` after `RemainingIdleTime()` expires.

**pre_erupt** - Shaking animation, pressure building. Duration = 10% of the iteration off-duration. Transitions to `erupt`.

**erupt** - The emitter is active. Has two sub-states:
- **erupting** - Normal emission. Transitions to `overpressure` on `EmitterBlocked` event.
- **overpressure** - Emitter is blocked by ambient pressure exceeding `maxPressure`. Transitions back to `erupting` on `EmitterUnblocked`. The emitter remains registered but the native sim suppresses output.

The entire `erupt` state lasts `RemainingEruptTime()` (the iteration on-duration), then transitions to `post_erupt` regardless of overpressure status.

**post_erupt** - Brief cooldown. Duration = 5% of the iteration off-duration. Transitions to `idle`.

### Off-Duration Split

The iteration off-duration (time between eruptions within an active period) is divided:

| Sub-phase | Share of off-duration |
|-----------|----------------------|
| Post-erupt | 5% |
| Idle | 85% |
| Pre-erupt | 10% |

Constants from `Geyser.cs`: `PRE_PCT = 0.1f`, `POST_PCT = 0.05f`, idle gets the remainder.

## Two-Level Timing

Geysers operate on two nested cycles:

### Iteration Cycle (eruption cycle)

Controls individual eruptions within an active period.

- **Iteration length**: Total time for one eruption + one rest. Range: `[minIterationLength, maxIterationLength]` seconds.
- **Iteration percent**: Fraction of the iteration spent erupting. Range: `[minIterationPercent, maxIterationPercent]`.
- **On-duration** = iterationLength * iterationPercent
- **Off-duration** = iterationLength * (1 - iterationPercent)

Default ranges (most geysers): length 60-1140s, percent 10%-90%.

### Year Cycle (active/dormant cycle)

Controls the longer active and dormant periods.

- **Year length**: Total time for one active + one dormant period. Range: `[minYearLength, maxYearLength]` seconds.
- **Year percent**: Fraction of the year spent active. Range: `[minYearPercent, maxYearPercent]`.
- **Year on-duration** = yearLength * yearPercent
- **Year off-duration** = yearLength * (1 - yearPercent)

Default ranges (most geysers): length 15,000-135,000s (25-225 cycles), percent 40%-80%.

One game cycle = 600 seconds.

## Emission Rate Calculation

The emit rate (kg/s during eruption) is derived from mass-per-cycle:

```
iterationsPerCycle = 600 / iterationLength
emitRate = massPerCycle / iterationsPerCycle / onDuration
```

From `GeyserInstanceConfiguration.GetEmitRate()`:
```csharp
float num = 600f / GetIterationLength();
return GetMassPerCycle() / num / GetOnDuration();
```

### Average Emission

The average emission rate accounts for both cycles:

```
massPerEruption = emitRate * onDuration
averageEmission = (yearOnDuration / iterationLength) * massPerEruption / yearLength
```

This is the steady-state kg/s averaged over the entire year cycle, visible in the info panel after study.

## Overpressure

Each geyser type defines a `maxPressure` threshold in kg of the emitted element per tile. The `ElementEmitter` registers with the native sim, which checks ambient mass in the emit cell (offset 0,1 from the geyser's origin). When mass in the 2-tile emit range exceeds `maxPressure`, the sim fires `EmitterBlocked` and stops emitting. When mass drops below threshold, `EmitterUnblocked` fires.

The geyser state machine remains in the `erupt` state during overpressure. The eruption timer continues to count down. Overpressure does not extend the eruption, it just wastes it.

### Pressure Thresholds by Shape

| Category | maxPressure (kg) |
|----------|-----------------|
| Gas | 5 |
| Gas (high) | 15 |
| Liquid (small) | 50 |
| Liquid | 500 |
| Molten | 150 |

## Seed-Based Randomization

Each geyser's parameters are determined at world generation and are deterministic per seed + position.

From `GeyserConfigurator.CreateRandomInstance()`:
```csharp
KRandom randomSource = new KRandom(
    SaveLoader.Instance.clusterDetailSave.globalWorldSeed
    + (int)transform.GetPosition().x
    + (int)transform.GetPosition().y
);
```

Five rolls are made in sequence:
1. **rateRoll** - Rolled in `[presetMin, presetMax]` (usually `[0, 1]`)
2. **iterationLengthRoll** - Rolled in `[0, 1]`
3. **iterationPercentRoll** - Rolled in `[presetMin, presetMax]`
4. **yearLengthRoll** - Rolled in `[0, 1]`
5. **yearPercentRoll** - Rolled in `[presetMin, presetMax]`

### Resample Function

Raw rolls are not mapped linearly. `Resample()` applies a logistic (sigmoid) curve to concentrate values toward the center of the range:

```csharp
private float Resample(float t, float min, float max)
{
    float num = 6f;           // steepness
    float num2 = 0.002472623f; // margin clamp
    float num3 = t * (1f - num2 * 2f) + num2;
    return (-Mathf.Log(1f / num3 - 1f) + num) / (num * 2f) * (max - min) + min;
}
```

This is an inverse-logistic transform with steepness 6. A uniform roll `t` in `[0,1]` maps to a value biased toward the midpoint of `[min, max]`. Extreme values (near min or max) are possible but unlikely. The `num2` margin prevents division by zero at the boundaries.

## Study Mechanics

Geysers spawn with a `Studyable` component. Until studied, the year cycle (active/dormant timing) is hidden from the player.

### Study Process

1. Player marks the geyser for study via the side screen button.
2. A `WorkChore<Studyable>` of type `Research` is created.
3. The assigned duplicant must have the `CanStudyWorldObjects` skill perk (`requiredSkillPerk`).
4. Work time: **3,600 seconds** (6 cycles at normal speed). Progress is not reset on interruption (`resetProgressOnStop = false`).
5. Research speed attribute affects work speed via `AttributeConverters.ResearchSpeed`.
6. On completion, `studied = true` is set and persisted.

### Information Revealed by Study

Before study, the geyser info panel shows:
- Element, temperature, and emit rate (per-eruption stats)
- Iteration cycle timing (eruption period)
- "Year unstudied" placeholder for active/dormant cycle
- "Average output unstudied" placeholder

After study:
- Year cycle timing (active duration out of total year length)
- Next dormant/active transition time
- Average emission rate (kg/s averaged over the full year)

### Databank Drops (Spaced Out DLC)

When `DlcManager.IsExpansion1Active()`, completing a study drops 8-13 Orbital Research Databanks at the geyser's position. The count is `Random.Range(7, 13)` (yielding 7-12), but the loop uses `i <= num` so it creates one extra item.

## Geo Tuner

The Geo Tuner building (Spaced Out DLC) modifies geyser parameters. Up to **5** Geo Tuners can affect a single geyser simultaneously (`MAX_GEOTUNED = 5`).

Requirements: 120W power, must be in a Laboratory room, operator needs `AllowGeyserTuning` skill perk. Consumes a material payload per tuning cycle (varies by geyser category). Each tuning lasts 600s (1 cycle).

### Modification Method

All modifications use percentage-based scaling except temperature which uses absolute values:

| Parameter | Method |
|-----------|--------|
| Mass per cycle | Percentages (multiplied by base scaledRate) |
| Temperature | Values (added directly in Kelvin) |
| Iteration duration | Percentages |
| Iteration percentage | Percentages |
| Year duration | Percentages |
| Year percentage | Percentages |
| Max pressure | Percentages |

### Tuning Categories

Each geyser type maps to a tuning category that determines the material cost and modification values:

| Category | Material | Quantity (kg) | Mass Modifier | Temp Modifier (K) |
|----------|----------|---------------|---------------|-------------------|
| Water | Bleach Stone | 50 | +20% | +20 |
| Organic | Salt | 50 | +20% | +15 |
| Hydrocarbon | Abyssalite | 100 | +20% | +15 |
| Volcano | Abyssalite | 100 | +20% | +150 |
| Metals | Phosphorus | 80 | +20% | +50 |
| CO2 | Polluted Dirt | 50 | +20% | +5 |
| Default | Dirt | 50 | +10% | +10 |

Category assignments:
- **Water**: steam, hot_steam, hot_water, salt_water, slush_salt_water, filthy_water, slush_water
- **Organic**: slimy_po2, hot_po2, chlorine_gas, chlorine_gas_cool
- **Hydrocarbon**: methane, hot_hydrogen, liquid_sulfur, oil_drip
- **Volcano**: small_volcano, big_volcano
- **Metals**: molten_copper, molten_gold, molten_iron, molten_aluminum, molten_cobalt, molten_niobium, molten_tungsten
- **CO2**: hot_co2, liquid_co2

### Logic Output

The Geo Tuner has a logic output port (`GEYSER_ERUPTION_STATUS_PORT`) that signals the assigned geyser's eruption status.

## Geyser Types

All values from `GeyserGenericConfig.GenerateConfigs()`. Temperature in Kelvin, rate in kg/cycle, pressure in kg. Unless noted, iteration and year ranges use the defaults (iteration: 60-1140s, 10%-90%; year: 15,000-135,000s, 40%-80%).

### Gas Geysers (2x4 tiles)

| ID | Element | Temp (K) | Rate Min | Rate Max | Max Pressure | Notes |
|----|---------|----------|----------|----------|--------------|-------|
| steam | Steam | 383.15 | 1,000 | 2,000 | 5 | |
| hot_steam | Steam | 773.15 | 500 | 1,000 | 5 | |
| hot_co2 | Carbon Dioxide | 773.15 | 70 | 140 | 5 | |
| hot_hydrogen | Hydrogen | 773.15 | 70 | 140 | 5 | |
| hot_po2 | Polluted Oxygen | 773.15 | 70 | 140 | 5 | |
| slimy_po2 | Polluted Oxygen | 333.15 | 70 | 140 | 5 | +5,000 Slimelung germs |
| chlorine_gas | Chlorine | 333.15 | 70 | 140 | 5 | |
| chlorine_gas_cool | Chlorine | 278.15 | 70 | 140 | 5 | Not in random pool |
| methane | Natural Gas | 423.15 | 70 | 140 | 5 | |

### Liquid Geysers (4x2 tiles)

| ID | Element | Temp (K) | Rate Min | Rate Max | Max Pressure | Notes |
|----|---------|----------|----------|----------|--------------|-------|
| hot_water | Water | 368.15 | 2,000 | 4,000 | 500 | |
| slush_water | Polluted Water | 263.15 | 1,000 | 2,000 | 500 | Geyser body temp: 263K |
| filthy_water | Polluted Water | 303.15 | 2,000 | 4,000 | 500 | +20,000 Food Poisoning germs |
| slush_salt_water | Brine | 263.15 | 1,000 | 2,000 | 500 | Geyser body temp: 263K |
| salt_water | Salt Water | 368.15 | 2,000 | 4,000 | 500 | |
| liquid_co2 | Liquid CO2 | 218 | 100 | 200 | 50 | Geyser body temp: 218K |
| oil_drip | Crude Oil | 600 | 1 | 250 | 50 | Special iteration: 600s fixed, 100% uptime; year: 100-500s |
| liquid_sulfur | Liquid Sulfur | 438.35 | 1,000 | 2,000 | 500 | Spaced Out DLC |

### Volcanoes and Molten Geysers (3x3 tiles)

| ID | Element | Temp (K) | Rate Min | Rate Max | Max Pressure | Iteration | Notes |
|----|---------|----------|----------|----------|--------------|-----------|-------|
| small_volcano | Magma | 2,000 | 400 | 800 | 150 | Infrequent: 6,000-12,000s, 0.5%-1% | |
| big_volcano | Magma | 2,000 | 800 | 1,600 | 150 | Infrequent: 6,000-12,000s, 0.5%-1% | |
| molten_copper | Molten Copper | 2,500 | 200 | 400 | 150 | Frequent: 480-1,080s, 1.7%-10% | |
| molten_iron | Molten Iron | 2,800 | 200 | 400 | 150 | Frequent: 480-1,080s, 1.7%-10% | |
| molten_gold | Molten Gold | 2,900 | 200 | 400 | 150 | Frequent: 480-1,080s, 1.7%-10% | |
| molten_aluminum | Molten Aluminum | 2,000 | 200 | 400 | 150 | Frequent: 480-1,080s, 1.7%-10% | Spaced Out DLC |
| molten_tungsten | Molten Tungsten | 4,000 | 200 | 400 | 150 | Frequent: 480-1,080s, 1.7%-10% | Spaced Out DLC, not in random pool |
| molten_niobium | Molten Niobium | 3,500 | 800 | 1,600 | 150 | Infrequent: 6,000-12,000s, 0.5%-1% | Spaced Out DLC, not in random pool |
| molten_cobalt | Molten Cobalt | 2,500 | 200 | 400 | 150 | Frequent: 480-1,080s, 1.7%-10% | Spaced Out DLC |

### Iteration Profiles for Molten Types

Volcanoes and molten geysers override the default iteration parameters:

**Infrequent molten** (volcanoes, niobium): Very short eruptions relative to cycle length.
- Iteration length: 6,000-12,000s (10-20 cycles)
- Iteration percent: 0.5%-1%
- Eruptions last seconds to minutes out of multi-cycle periods

**Frequent molten** (metal volcanoes): Short but more frequent eruptions.
- Iteration length: 480-1,080s
- Iteration percent: 1.67%-10% (1/60 to 1/10)

## Oil Reservoir Special Case

The oil drip (`oil_drip`) has unique iteration parameters:
- Iteration length: fixed 600s (exactly one cycle)
- Iteration percent: 100% (always erupting during active period)
- Year length: 100-500s (shorter than one cycle)
- Year percent uses defaults (40%-80%)
- Very low output: 1-250 kg/cycle

This means it effectively drips continuously during its active period at a very low rate.

## Geyser Body Temperature

Most geysers have a body temperature of 372.15K (99C), the default `geyserTemperature` parameter. The geyser entity is made of Abyssalite (`SimHashes.Katairite`), so it does not conduct heat to surroundings.

Exceptions where `geyserTemperature` differs from default:
- slush_water: 263K
- slush_salt_water: 263K
- liquid_co2: 218K

The emitted element temperature is independent of the geyser body temperature.

## Time Shifting

The `Geyser.AlterTime()` and `ShiftTimeTo()` methods allow programmatic adjustment of where a geyser sits in its cycle. This is used by the game for debug/sandbox features and for synchronizing geyser state after save/load. The `timeShift` offset is added to `GameClock.Instance.GetTime()` to compute the geyser's effective lifetime position.

`ShiftTimeTo()` supports four modes:
- `ActiveState` - Jump to start of active period
- `DormantState` - Jump to start of dormant period
- `NextIteration` - Advance to next eruption
- `PreviousIteration` - Rewind to previous eruption
