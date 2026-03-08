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

### Body Temperature Regulation (WarmBlooded)

Duplicants are warm-blooded creatures managed by the `WarmBlooded` component operating in `FullHomeostasis` mode. The system maintains body temperature at the ideal 310.15 K (37 C) by actively heating or cooling, at the cost of burning calories.

**Heat generation**: Duplicants constantly produce baseline body heat at `DUPLICANT_BASE_GENERATION_KILOWATTS`. This value derives from calorie burn rate and heat generation efficiency:

```
baseGenerationKW = HEAT_GENERATION_EFFICIENCY * (-CALORIES_BURNED_PER_SECOND) * 0.001 * KCAL2JOULES / 1000
                 = 0.012 * (1000000/600) * 0.001 * 4184 / 1000
                 = ~0.0836 kW
```

The baseline heat modifier is registered with `CreatureSimTemperatureTransfer.NonSimTemperatureModifiers` and continuously warms the duplicant's `PrimaryElement`.

**Homeostatic regulation**: When body temperature drifts from ideal (after a 3-second transition delay to prevent oscillation), the regulator activates:

- **Too hot** (body temp > ideal): The `WarmingRegulator` applies a cooling modifier using `DUPLICANT_COOLING_KILOWATTS` (same formula structure, with `COOLING_EFFICIENCY = 0.08`). The cooling strength is proportional to the temperature delta, clamped so the body doesn't overshoot past ideal. Burns calories proportional to the cooling effort.
- **Too cold** (body temp < ideal): The `CoolingRegulator` applies a warming modifier using `DUPLICANT_WARMING_KILOWATTS` (`WARMING_EFFICIENCY = 0.08`). Also proportional and clamped, also burns calories.

Both warming and cooling KW values are computed as:
```
efficiency * (-CALORIES_BURNED_PER_SECOND) * 0.001 * KCAL2JOULES / 1000
= 0.08 * (1000000/600) * 0.001 * 4184 / 1000
= ~0.557 kW
```

The calorie cost of regulation is: `-(regulationKW * scale) / KCAL2JOULES` for cooling, and `-(regulationKW * scale * 1000) / KCAL2JOULES` for warming.

Source: `WarmBlooded`, `DUPLICANTSTATS.STANDARD.BaseStats`.

### Heat Exchange with Environment

Duplicants exchange heat with the simulation as `CreatureSimTemperatureTransfer` objects (subclass of `SimTemperatureTransfer`). The sim calculates energy flow based on:

- **Surface area**: `DUPLICANTSTATS.STANDARD.Temperature.SURFACE_AREA` = 1 m^2
- **Skin thickness** (thermal conductivity barrier): base `SKIN_THICKNESS` = 0.002 m, modified by clothing and body type traits
- **Ground transfer scale**: 0.0625 (reduces heat exchange with the ground tile)

The `ThermalConductivityBarrier` attribute represents total insulation thickness. Higher values slow heat transfer to/from the environment. Base skin provides 0.002 m, and clothing/suits add on top of that.

**Body type modifiers** (`CONDUCTIVITY_BARRIER_MODIFICATION`):
- Skinny: -0.005 m (less insulated, loses/gains heat faster)
- Pudgy: +0.005 m (more insulated, temperature changes more slowly)

Every 200ms, `CreatureSimTemperatureTransfer.Sim200ms` converts the `NonSimTemperatureModifiers` (baseline heat + homeostatic regulation) into energy deltas and injects them into the sim via `SimMessages.ModifyElementChunkEnergy`. It also reads back the sim's energy exchange data into `average_kilowatts_exchanged` for use by the external temperature monitor.

Source: `CreatureSimTemperatureTransfer`, `SimTemperatureTransfer`, `DUPLICANTSTATS.STANDARD.Temperature`.

### External Temperature Comfort (ExternalTemperatureMonitor)

Separate from internal temperature, `ExternalTemperatureMonitor` tracks whether the duplicant's surroundings feel comfortable based on the rate of heat exchange with the environment (not absolute temperature).

**Thresholds** (energy flow rate, not temperature):
- Cold threshold: losing more than 0.039 kW to environment (`GetExternalColdThreshold` returns -0.039)
- Hot threshold: gaining more than 0.008 kW from environment (`GetExternalWarmThreshold` returns 0.008)

The asymmetry means duplicants feel hot more easily than they feel cold. The check uses `average_kilowatts_exchanged.GetUnweightedAverage` from `CreatureSimTemperatureTransfer`.

**State transitions**: Requires the uncomfortable condition to persist for 6 seconds before transitioning from comfortable, then 1 second in a transition state before applying the effect. Exiting an uncomfortable state also requires 6 seconds of comfort. An `EffectAdded` event can also immediately return the duplicant to comfortable (e.g., when receiving WarmTouch or RefreshingTouch).

**Comfort states and effects**:
- `tooWarm`: Tags the duplicant with `GameTags.FeelingWarm`, applies stress/stamina/athletics penalties via the `WarmAir` effect
- `tooCool`: Tags the duplicant with `GameTags.FeelingCold`, applies stress/stamina/athletics penalties via the `ColdAir` effect

**Immunity effects**: Several effects suppress external temperature discomfort:
- `WarmTouch`: Grants immunity to `ColdAir` (from Sauna, Hot Tub)
- `WarmTouchFood`: Grants immunity to `ColdAir` (from hot food)
- `RefreshingTouch`: Grants immunity to `WarmAir`
- The `FrostProof` trait causes `ColdAir` to be ignored entirely

**WarmthProvider zones**: Buildings like the Space Heater register cells as "warm" via `WarmthProvider`. When a duplicant stands in a warm cell (`WarmthProvider.IsWarmCell`), they are exempt from the `IsTooCold` check regardless of actual heat exchange. The warm zone requires line-of-sight from the source building; solid tiles block it.

**Body temperature influence**: If the duplicant's internal temperature is more than 0.5 K above ideal (from `TemperatureMonitor.IdealTemperatureDelta`), the cold threshold is suppressed to 0, preventing the "too cold" feeling. A warm body overrides environmental chill.

Source: `ExternalTemperatureMonitor`, `WarmthProvider`, `ColdImmunityMonitor`.

### Clothing and Suit Temperature Effects

Clothing and suits modify the `ThermalConductivityBarrier` attribute, which changes how fast heat transfers between the duplicant and the environment. Higher barrier = slower exchange = more insulation.

**Clothing** (`ClothingWearer`):

| Clothing Type | Conductivity Barrier | Effect |
|---------------|---------------------|--------|
| Basic Clothing (default) | +0.0025 m | Standard insulation |
| Warm Vest | +0.008 m | Highest clothing insulation, best for cold environments |
| Cool Vest | +0.0005 m | Minimal insulation, lets heat escape faster |
| Fancy Clothing | +0.0025 m | Same as basic |
| Custom Clothing | +0.0025 m | Same as basic |

All clothing types except Cool Vest also have a `homeostasisEfficiencyMultiplier` of -1.25, though this field is declared but not referenced by `WarmBlooded` or any other runtime system in the decompiled source.

When clothing changes, `CreatureSimTemperatureTransfer.RefreshRegistration` is called to re-register the duplicant with the sim using the updated insulation thickness.

**Suits** modify both insulation and scalding thresholds:

| Suit | Conductivity Barrier | Scalding Threshold Mod | Scolding Threshold Mod |
|------|---------------------|----------------------|----------------------|
| Atmo Suit | +0.2 m | +1000 K | -1000 K |
| Jet Suit | +0.2 m | +1000 K | -1000 K |
| Lead Suit | +0.3 m | +1000 K | -1000 K |

The +/-1000 K scalding/scolding modifiers effectively make suited duplicants immune to contact temperature damage. The Lead Suit provides the highest insulation at 0.3 m, making it the most thermally protective.

When an airtight suit is unequipped, `ScaldingMonitor.OnSuitUnequipped` resets the external temperature average to the duplicant's internal temperature. This prevents a sudden scalding/scolding trigger from accumulated suit-internal temperature diverging from the actual environment.

Source: `ClothingWearer`, `TUNING.EQUIPMENT.SUITS`, `LeadSuitConfig`, `JetSuitConfig`, `ScaldingMonitor.OnSuitUnequipped`.

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

## Medical Buildings

### Medical Cot

The basic medical bed. Duplicants with wounds or disease are assigned here to rest and recuperate. Built from 400 kg raw minerals (TIER3), 3x2 tiles.

The `Clinic` component manages the healing state machine. When a patient occupies the bed, undoctored health/disease effects are applied (`MedicalCot` / `MedicalCot`). A doctor can visit to upgrade these to doctored effects (`MedicalCotDoctored` / `MedicalCotDoctored`), which provide the 1.2x disease recovery multiplier instead of 1.1x.

**Doctor visit interval**: 300 seconds (half a cycle). After the doctored effect expires, the bed re-enters the undoctored state and creates a new `DoctorChoreWorkable` chore for another visit.

**Doctor work time**: 45 seconds (`DoctorChoreWorkable.workTime = 45f`). Modified by the `DoctorSpeed` attribute converter.

**No skill requirement to build, assign, or doctor**. Any duplicant can rest in it. The Medical Cot's `DoctorChoreWorkable` does not set a `requiredSkillPerk`, so any duplicant can perform the doctor visit -- unlike the Disease Clinic and Sick Bay which require `CanDoctor` and `CanAdvancedMedicine` respectively.

**Health threshold slider**: The `Clinic` component exposes a slider (default 70%) controlling the minimum HP percentage at which duplicants will auto-assign to the bed.

**Room bonus**: Recommends placement in a Hospital room. Displays a status warning if placed outside one.

**On start work**: Applies the `Sleep` effect and performs instant extreme radiation recovery -- if the patient has 900+ rads (scaled by difficulty), their radiation balance is immediately reduced to 600 rads.

**On complete work**: Removes the `SoreBack` effect.

Source: `MedicalCotConfig`, `Clinic`, `DoctorChoreWorkable`.

### Disease Clinic (Doctor Station)

A treatment station where a doctor administers medicine to cure specific diseases. The patient sits in the station while a doctor operates. Built from 400 kg raw minerals (TIER3), 3x2 tiles.

**Requires `CanDoctor` perk** (Medicine2 skill) to operate.

The station stores medicine items tagged for `DoctorStation` supply. When a sick duplicant has a matching treatment available in storage, they sit in the station and wait for a doctor. The doctor works at the `DoctorStationDoctorWorkable` component.

**Doctor work time**: 40 seconds (`SetWorkTime(40f)`). Modified by `DoctorSpeed` attribute.

**Treatment flow**:
1. Station checks storage for `MedicinalPill` items whose `curedSicknesses` match the patient's diseases
2. Patient sits and enters `waiting` state (`hasPatient = true`)
3. Doctor arrives and enters `being_treated` state (`hasDoctor = true`)
4. On completion, `CompleteDoctoring()` cures the first matching sickness and consumes 1 unit of the medicine from storage

**Preconditions**: The patient chore checks both `TreatmentAvailable` (medicine in storage matches a disease the dupe has) and `DoctorAvailable` (another dupe with the required skill perk exists).

**Room bonus**: Recommends Hospital room.

Source: `DoctorStationConfig`, `DoctorStation`, `DoctorStationDoctorWorkable`.

### Sick Bay (Advanced Doctor Station)

An advanced medical station for administering high-tier medicine. Built from 400 kg refined metals (TIER3), 2x3 tiles.

**Requires power**: 480 W when active.

**Requires `CanAdvancedMedicine` perk** (Senior Medic skill, Medicine3) to operate.

**Doctor work time**: 60 seconds (`SetWorkTime(60f)`). Modified by `DoctorSpeed` attribute.

Functionally identical to the Disease Clinic in treatment flow, but uses the `AdvancedDoctorStation` supply tag. Only medicines configured with `doctorStationId = "AdvancedDoctorStation"` are delivered here (Serum Vial for Zombie Spores, Intermediate Rad Pills).

**Room bonus**: Recommends Hospital room.

Source: `AdvancedDoctorStationConfig`, `DoctorStation`, `DoctorStationDoctorWorkable`.

### Massage Table

A stress relief building. Built from 400 kg raw minerals (TIER3), 2x2 tiles.

**Requires power**: 240 W when active.

**Stress reduction rate**: -30%/cycle base, or -60%/cycle if placed in a Massage Clinic room. The rate formula displayed in-game is `stressModificationValue / 600f * 60f` (per-minute percentage).

**Activation threshold**: Configurable via slider (default 50%). Duplicants only use the table when their stress exceeds this value. A separate deactivation threshold controls when they stop.

**Work duration**: Infinite (`SetWorkTime(float.PositiveInfinity)`). The duplicant stays until stress drops to the deactivation threshold (`stopStressingValue`).

**On complete work**: Removes the `SoreBack` effect.

**Room bonus**: Recommends Massage Clinic room. The `RoomTracker` checks for `Db.Get().RoomTypes.MassageClinic`.

**Assignable**: Uses the `MassageTable` assignable slot and can be set to public.

**Chore type**: `StressHeal` at `PriorityClass.high`, ignores schedule blocks. Bionics are excluded (`IsNotARobot` precondition).

Source: `MassageTableConfig`, `MassageTable`, `RelaxationPoint`.

### Medical Cot vs Disease Clinic

The Medical Cot and Disease Clinic serve different purposes:

- **Medical Cot** (`Clinic` component): A bed where wounded or sick duplicants rest. Provides passive recuperation effects. A doctor visits periodically (every 300s) to apply the doctored buff, but no medicine is consumed. Cannot cure diseases directly.
- **Disease Clinic** (`DoctorStation` component): A treatment chair where a doctor administers medicine from storage to cure a specific disease. The patient does not rest here indefinitely -- once treatment completes, the disease is cured and the duplicant leaves. Consumes one medicine item per treatment.

Both recommend Hospital room placement. The Disease Clinic requires a doctor with `CanDoctor`; the Medical Cot has no skill requirement for doctoring. The Medical Cot additionally serves as a rescue destination for incapacitated duplicants (tagged as `Clinic` and `BedType`).

### Apothecary

A crafting station for medicine. Uses the `Apothecary` component (extends `ComplexFabricator`). Built from 800 kg All Metals (TIER4), 2x3 tiles.

**No power required** (despite producing heat: 0.125 kW exhaust, 0.5 kW self-heat when active).

**Requires `CanCompound` perk** (Medicine1 / Field Medic skill) to operate. Manually operated with a logic input port for automation.

**Crafting speed**: Modified by the `CompoundingSpeed` attribute converter. Earns experience in the `MedicalAid` skill group at `PART_DAY_EXPERIENCE` rate.

All base-game and Spaced Out medicine recipes (except IntermediateRadPill) are fabricated here.

Source: `ApothecaryConfig`, `Apothecary`.

### Advanced Apothecary (Deprecated)

A radbolt-powered crafting station. Requires the Spaced Out DLC. Marked `Deprecated = true` in the building def (no longer buildable in current game versions). Built from 1000 kg Refined Metals (TIER5), 3x3 tiles.

**Requires `CanCompound` perk** to operate (same as the regular Apothecary). Uses high-energy particle input (capacity 400, consumes 1 particle/s while active). Produces 0.5 kW exhaust and 2 kW self-heat.

Only the IntermediateRadPill recipe uses this fabricator. Both the building and the recipe are deprecated content.

Source: `AdvancedApothecaryConfig`, `AdvancedApothecary`.

### Doctor Skill Tiers

| Skill | Perk | Unlocks |
|-------|------|---------|
| Medicine1 (Field Aid) | `CanCompound` | Apothecary operation (medicine crafting) |
| Medicine2 (Bedside Manner) | `CanDoctor` | Disease Clinic operation |
| Medicine3 (Senior Medic) | `CanAdvancedMedicine` | Sick Bay operation |

Source: `Database.Skills`, `Database.SkillPerks`.

## Medicine Items

All medicines are crafted at the Apothecary (except Intermediate Rad Pills at the Advanced Apothecary). The `MEDICINE.WORK_TIME` constant (10 seconds) governs self-administration time, not crafting time.

### Crafting Recipes

| Medicine | Ingredients | Craft Time | Fabricator | Required Tech |
|----------|------------|------------|------------|---------------|
| BasicBooster (Vitamin Chews) | 1 kg Carbon | 50s | Apothecary | None |
| IntermediateBooster (Immuno Booster) | 1 Pincha Peppernut | 100s | Apothecary | None |
| BasicCure (Curative Tablet) | 1 kg Carbon + 1 kg Water | 50s | Apothecary | None |
| Antihistamine | 1 Balm Lily Flower or 10 Waterweed + 1 kg Dirt | 100s | Apothecary | None |
| IntermediateCure (Serum Vial - Slime Lung) | 1 Balm Lily Flower + 1 kg Phosphorite | 100s | Apothecary | MedicineII |
| AdvancedCure (Serum Vial - Zombie Spores) | 1 kg Steel + 1 Shine Bug Egg (Orange) | 200s | Apothecary | MedicineIV |
| BasicRadPill (Rad Pills) | 1 kg Carbon | 50s | Apothecary | None |
| IntermediateRadPill | 1 kg Carbon | 50s | AdvancedApothecary | None (deprecated) |

The Antihistamine recipe produces **10 pills** per craft. All other recipes produce 1 pill per craft.

### Medicine Effects

| Medicine | Type | Self-Administered | Station Required | Effect |
|----------|------|-------------------|-----------------|--------|
| BasicBooster | Booster | Yes | None | `Medicine_BasicBooster` effect (germ resistance buff) |
| IntermediateBooster | Booster | Yes | None | `Medicine_IntermediateBooster` effect (germ resistance buff) |
| BasicCure | CureSpecific | Yes | None | Cures Food Poisoning (`FoodSickness`) |
| Antihistamine | CureSpecific | Yes | None | Cures Allergies, removes DupeMosquitoBite effect, applies `HistamineSuppression` |
| IntermediateCure | CureSpecific | No | Disease Clinic | Cures Slime Lung (`SlimeSickness`) |
| AdvancedCure | CureSpecific | No | Sick Bay | Cures Zombie Spores (`ZombieSickness`) |
| BasicRadPill | Booster | Yes | None | `Medicine_BasicRadPill` effect (radiation resistance) |
| IntermediateRadPill | Booster | No | Sick Bay | `Medicine_IntermediateRadPill` effect (deprecated content) |

Medicines with `doctorStationId = null` can be self-administered by the duplicant. Medicines with a station ID must be delivered to that station and administered by a doctor.

Source: `TUNING.MEDICINE`, `BasicBoosterConfig`, `IntermediateBoosterConfig`, `BasicCureConfig`, `IntermediateCureConfig`, `AdvancedCureConfig`, `AntihistamineConfig`, `BasicRadPillConfig`, `IntermediateRadPillConfig`, `MedicineInfo`.
