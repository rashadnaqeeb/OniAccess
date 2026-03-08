# Duplicant Health and Damage

How duplicants take damage, become incapacitated, die, and recover. Derived from decompiled source code.

## Overview

Every duplicant has a `Health` component backed by the `HitPoints` Amount. When HP reaches zero, duplicants do not die immediately. Instead they become incapacitated, triggering a bleed-out timer managed by `IncapacitationMonitor`. Other duplicants can rescue the downed dupe to a medical bed. If the timer expires, the duplicant dies.

Critters and non-duplicant entities use the same `Health` component but skip incapacitation and die directly when HP hits zero.

## Hit Points

**Max HP**: 100 (`DUPLICANTSTATS.STANDARD.BaseStats.HIT_POINTS`)

Source: `BaseMinionConfig` applies this as an `AttributeModifier` on `Amounts.HitPoints.maxAttribute`.

### Health States

`Health.UpdateStatus` maps current HP percentage to a `HealthState` enum:

| State | HP Range | Effect |
|-------|----------|--------|
| Perfect | 100% | No wound effects |
| Alright | 85-99% | No wound effects |
| Scuffed | 66-84% | LightWounds effect applied |
| Injured | 33-65% | ModerateWounds effect, wounded walk anim |
| Critical | 1-32% | SevereWounds effect, wounded walk anim, auto-searches for medical bed |
| Incapacitated | 0% | Bleed-out timer starts |

Source: `Health.UpdateStatus`, `WoundMonitor.InitializeStates`.

### Natural Regeneration

There is no base HP regeneration. HP only recovers through specific effects:

- **Regeneration trait** (gene shuffler): +1/30 HP/s = +20 HP/cycle (`HitPointsDelta` modifier of `1f/30f`)
- **Recuperating effect**: Applied when resting in a medical bed (triggers disease recovery multiplier 1.1x, or 1.2x with doctor treatment)
- **Medical bed rest**: The `WoundMonitor` toggles a `Heal` urge when wounded, driving the duplicant to seek a clinic bed

Source: `TUNING.TRAITS` Regeneration definition, `TUNING.MEDICINE`.

## Damage Sources

### Scalding and Scolding (Temperature Contact Damage)

Managed by `ScaldingMonitor`. Applies damage when the averaged external temperature crosses thresholds.

**Scalding threshold** (too hot): 345 K / 71.85 C (`Def.defaultScaldingTreshold`)
**Scolding threshold** (too cold): 183 K / -90.15 C (`Def.defaultScoldingTreshold`)

These are base values applied as `AttributeModifier` on `ScaldingThreshold` and `ScoldingThreshold` attributes, meaning they can be modified by equipment or traits.

**Damage formula**: `dt * 10f` HP per tick, but only if at least 5 seconds have elapsed since the last damage tick (`MIN_SCALD_INTERVAL = 5f`). Ticked at `SIM_1000ms` (once per second), so effective damage is **10 HP every 5 seconds** (2 HP/s averaged).

**Temperature averaging**: External temperature is smoothed over a 6-second window (`TEMPERATURE_AVERAGING_RANGE = 6f`). The transition from idle to scalding/scolding requires the condition to persist for 1 second (`TRANSITION_TO_DELAY`).

Vacuum and Void tiles are excluded from temperature checks.

Source: `ScaldingMonitor.TemperatureDamage`, `ScaldingMonitor.IsScalding`, `ScaldingMonitor.InitializeStates`.

### Internal Temperature (Hypothermia / Hyperthermia)

Managed by `TemperatureMonitor`. Tracks the duplicant's internal body temperature (smoothed over 4 seconds). Does not directly deal HP damage but triggers behavioral responses and can lead to death via the scalding system or incapacitation.

| Threshold | Value (K) | Value (C) | Source |
|-----------|-----------|-----------|--------|
| Ideal body temp | 310.15 | 37.0 | `DUPLICANTSTATS.STANDARD.Temperature.Internal.IDEAL` (used by `TemperatureMonitor`) |
| Hypothermia onset | 307.15 | 34.0 | `TemperatureMonitor.Instance.HypothermiaThreshold` (hardcoded default) |
| Hyperthermia onset | 313.15 | 40.0 | `TemperatureMonitor.Instance.HyperthermiaThreshold` (hardcoded default) |

Note: `DUPLICANTSTATS.STANDARD.Temperature.Internal` also declares `THRESHOLD_HYPOTHERMIA` (308.15 K) and `THRESHOLD_HYPERTHERMIA` (312.15 K), but these are never referenced by any code. The actual thresholds used at runtime are the `TemperatureMonitor` defaults above. Similarly, `THRESHOLD_FATAL_COLD` (300.15 K) and `THRESHOLD_FATAL_HOT` (320.15 K) are declared but unused. `TemperatureMonitor` does not trigger death directly; duplicants die from temperature via the scalding/scolding system (`ScaldingMonitor`), not from internal temperature thresholds.

When hyperthermic, a `CoolDown` urge is toggled. When hypothermic, a `WarmUp` urge is toggled.

Source: `TemperatureMonitor`.

### Suffocation

Managed by `SuffocationMonitor`. Uses a breath meter (`Amounts.Breath`) that fills when breathing and drains when no oxygen is available.

**Breath bar total**: 100 units over 110 seconds. Rate = 100/110 = ~0.909 units/s.

| Stage | Threshold | Time Without Air |
|-------|-----------|-----------------|
| Breathing normally | 100 (full) | N/A |
| Holding breath (no O2) | Draining at breath rate | 0-60s |
| Suffocating warning | 45.45 units (50s/110s of total) | ~60s |
| Death | 0 units | ~110s |

The breath meter drains at the same rate it fills. When it hits zero, `SuffocationMonitor` triggers `Deaths.Suffocation` directly (no incapacitation).

Source: `DUPLICANTSTATS.STANDARD.Breath`, `SuffocationMonitor`.

### Drowning

Managed by `DrowningMonitor`. Uses a countdown timer, not the breath meter.

**Time to drown**: 75 seconds (`MaxDrownTime = 75f`)
**Recovery rate**: 5x real time when out of liquid (`RegenRate = 5f`)
**Liquid threshold**: A cell is considered unsafe if `Grid.IsNavigatableLiquid` returns true, which checks if the cell contains "substantial liquid" (mass >= 35% of the element's default mass) or if the cell is liquid and the cell above is also liquid or solid. The `CellLiquidThreshold = 0.95f` constant declared in `DrowningMonitor` is unused.

The timer decrements each second while submerged. When it reaches zero, `Deaths.Drowned` is triggered directly (no incapacitation). When out of liquid, the timer recovers at 5 seconds per second of real time, clamped to the 75-second maximum.

Source: `DrowningMonitor.SlicedSim1000ms`, `DrowningMonitor.CheckDrowning`.

### Starvation

Managed by `CalorieMonitor`. Duplicants burn 1,000,000 kcal per cycle out of a maximum of 4,000,000 kcal.

| State | Calorie Threshold |
|-------|------------------|
| Satisfied | > 95% (3,800,000 kcal) |
| Hungry | < 82.5% (satisfied - half a cycle's burn = 0.95 - 500000/4000000) |
| Starving | < 25% (one cycle's burn / max) |
| Depleted | 0 kcal |

When calories reach zero, `Deaths.Starvation` is triggered directly (no incapacitation).

Source: `DUPLICANTSTATS.STANDARD.BaseStats`, `CalorieMonitor`.

### Combat Damage

Duplicant base weapon stats from `DUPLICANTSTATS.STANDARD.Combat.BasicWeapon`:

| Property | Value |
|----------|-------|
| Attacks per second | 2 |
| Min damage per hit | 1 |
| Max damage per hit | 1 |
| Max hits per attack | 1 |
| Target type | Single |
| AoE radius | 0 |

Duplicants deal 1 HP per hit at 2 hits per second = **2 HP/s** in combat. Critters have their own weapon stats defined in their configs.

The duplicant flee threshold is `Health.HealthState.Critical` (below 33% HP).

Source: `DUPLICANTSTATS.STANDARD.Combat`, `AttackProperties`.

### Radiation

Managed by `RadiationMonitor`. Radiation accumulates into a `RadiationBalance` amount. The absorption rate per cell is: `Grid.Radiation[cell] * (1 - RadiationResistance) / 600 * dt`.

Severity levels (scaled by difficulty modifier, default 1.0):

| Level | Exposure Threshold | Effect |
|-------|-------------------|--------|
| Minor | 100 rads | RadiationExposureMinor effect |
| Major | 300 rads | RadiationExposureMajor effect, vomiting every 120s |
| Extreme | 600 rads | RadiationExposureExtreme effect, vomiting every 60s |
| Deadly | 900 rads | Incapacitation via `Health.Incapacitate(RadiationSicknessIncapacitation)` |

Difficulty modifiers scale all thresholds:

| Setting | Multiplier | Deadly Threshold |
|---------|-----------|-----------------|
| Hardest | 0.33 | 297 rads |
| Harder | 0.66 | 594 rads |
| Default | 1.0 | 900 rads |
| Easier | 2.0 | 1800 rads |
| Easiest | 100.0 | 90,000 rads |

Source: `RadiationMonitor`, `DUPLICANTSTATS.RADIATION_DIFFICULTY_MODIFIERS`, `DUPLICANTSTATS.RADIATION_EXPOSURE_LEVELS`.

### Fall Damage

The `FallMonitor` handles duplicant falling, entombment, and recovery. The FallMonitor itself does not apply HP damage directly. Fall damage in ONI is handled through the physics/gravity system. The state machine transitions between standing, falling, landing, entombed, and recovery states. Duplicants can recover mid-fall to ladders or poles if available.

Source: `FallMonitor`.

## Incapacitation System

Managed by `IncapacitationMonitor`. When `Health.Incapacitate(cause)` is called, the duplicant's HP is set to zero and the `BecameIncapacitated` event fires.

### Bleed-Out Timer

**Starting stamina**: 120 seconds (`bleedOutStamina = 120f`)
**Bleed rate**: 1 unit/second (`baseBleedOutSpeed = 1f`)
**Recovery rate**: 1 unit/second when healthy (`baseStaminaRecoverSpeed = 1f`)
**Max stamina**: 120 (`maxBleedOutStamina = 120f`)

The timer counts down in real-time while incapacitated. If it reaches zero, the duplicant dies. The cause of death is determined by the incapacitation cause:

| Incapacitation Cause Tag | Death Type |
|--------------------------|-----------|
| `RadiationSicknessIncapacitation` | Deaths.Radiation |
| `HitPointsDepleted` | Deaths.Slain |
| Other | Deaths.Generic |

While incapacitated, the duplicant lies on the ground playing the incapacitated animation. Other duplicants can rescue them by carrying them to an assigned clinic bed.

Source: `IncapacitationMonitor`, `BeIncapacitatedChore`.

### Rescue Flow

1. Incapacitated dupe searches for available `Clinic` (medical bed) via `AutoAssignSlot`
2. If a reachable clinic is found, enters `waitingForPickup` state
3. Another duplicant performs `RescueIncapacitatedChore` to carry the dupe to the clinic
4. On arrival, enters `recovering` state, triggers `HealCritical` urge

Source: `BeIncapacitatedChore.FindAvailableMedicalBed`.

## Death Types

All death types defined in `Database.Deaths`:

| Death | ID | Trigger |
|-------|----|---------|
| Generic | Generic | Fallback when no specific cause |
| Frozen | Frozen | Fatal cold (declared but `Deaths.Frozen` is never triggered in decompiled source) |
| Suffocation | Suffocation | Breath meter reaches zero |
| Starvation | Starvation | Calories reach zero |
| Slain | Combat | HP depleted (combat or general damage) |
| Overheating | Overheating | Fatal heat (declared but `Deaths.Overheating` is never triggered in decompiled source) |
| Drowned | Drowned | Drown timer reaches zero |
| Explosion | Explosion | Rocket/explosive events |
| Fatal Disease | FatalDisease | Disease progression |
| Radiation | Radiation | Radiation sickness incapacitation bleed-out |
| Hit by Particle | HitByHighEnergyParticle | Radbolt impact |
| Dead Battery | DeadBattery | Bionic duplicant (uses HitByHighEnergyParticle strings, likely a game bug) |
| Dead Cyborg Charge Expired | DeadCyborgChargeExpired | Declared but never constructed in `Deaths` constructor |

Deaths that bypass incapacitation (kill directly): Suffocation, Starvation, Drowning.
Deaths that go through incapacitation: HP depletion (combat/scalding), Radiation.

Source: `Database.Deaths`, individual monitor classes.

## Recovery Mechanics

### Wound Effects

Applied by `Health.UpdateWoundEffects` based on health state:

| Health State | Effect Name |
|-------------|-------------|
| Scuffed | LightWounds |
| Injured | ModerateWounds |
| Critical | SevereWounds |

These effects apply attribute penalties (stress, athletics, etc.) while active. They are automatically removed when health improves past their threshold.

### Medical Treatment

**Medicine work time**: 10 seconds per treatment (`MEDICINE.WORK_TIME`)

| Medicine | Type | Station Required | Cures |
|----------|------|-----------------|-------|
| BasicBooster | Booster | None | N/A (preventive) |
| IntermediateBooster | Booster | None | N/A (preventive) |
| BasicCure | Cure | None | Food Poisoning |
| Antihistamine | Cure | None | Allergies, DupeMosquitoBite |
| IntermediateCure | Cure | Doctor Station | Slime Lung |
| AdvancedCure | Cure | Advanced Doctor Station | Zombie Spores |
| BasicRadPill | Booster | None | N/A (radiation protection) |
| IntermediateRadPill | Booster | Advanced Doctor Station | N/A (radiation protection) |

**Recuperation multipliers**:
- Resting in medical bed: 1.1x disease recovery rate
- Resting with doctor treatment: 1.2x disease recovery rate

Source: `TUNING.MEDICINE`.

### Clinic Assignment

The `WoundMonitor` drives clinic-seeking behavior:
- At **Critical** health state, it actively searches for an available medical bed every second via `FindAvailableMedicalBed`
- At **Injured** state, it applies wounded walk animation
- The `Heal` urge is toggled for all wound states, driving the duplicant to rest

Source: `WoundMonitor.InitializeStates`.

## Health-Related Traits

| Trait | Source | Health Effect |
|-------|--------|--------------|
| Regeneration | Gene Shuffler | +1/30 HP/s (~20 HP/cycle) passive regeneration |
| ScaredyCat | Bad trait | Cannot perform Combat tasks (avoids taking combat damage) |
| Joshua | Congenital | Cannot perform Combat tasks |
| WeakImmuneSystem | Bad trait | -1 Germ Resistance |
| StrongImmuneSystem | Good trait | +1 Germ Resistance |
| FrostProof | Good trait (DLC2) | Immune to ColdAir effect |

Source: `TUNING.TRAITS`, `TUNING.DUPLICANTSTATS`.
