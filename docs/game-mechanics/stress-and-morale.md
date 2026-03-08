# Stress and Morale

The stress and morale systems form a feedback loop: morale (Quality of Life) represents how well a duplicant's lifestyle meets their expectations, and the gap between morale and expectation drives stress gain or loss. Derived from decompiled source code; wiki values noted where they come from YAML data files not present in the C# source.

## Core Concepts

**Morale** is the `QualityOfLife` attribute. It is the sum of all morale modifiers: base value, food quality, decor average, room bonuses, recreation, skill aptitude bonuses, and miscellaneous effects.

**Morale Expectation** is the `QualityOfLifeExpectation` attribute. It rises as duplicants learn skills. The deficit or surplus between morale and expectation determines stress change.

**Stress** is an `Amount` ranging from 0 to 100%. The stress delta (rate of change per second) is the sum of all `StressDelta` attribute modifiers, with the morale-based modifier being the primary driver.

Source classes: `QualityOfLifeNeed`, `StressMonitor`, `StressBehaviourMonitor`, `DecorMonitor`, `RecreationTimeMonitor`, `Edible`, `MinionResume`, `Expectations`.

## Morale Expectation from Skills

Each skill has a tier (0-6). When a duplicant masters a skill, their morale expectation increases by `tier + 1`. Granted skills (from traits) do not count.

From `TUNING.SKILLS`:
```
SKILL_TIER_MORALE_COST = [1, 2, 3, 4, 5, 6, 7]
```

The `Skill.GetMoraleExpectation()` method indexes into this array by tier. `MinionResume.UpdateExpectation()` sums `tier + 1` for all non-granted mastered skills and applies the total as a modifier on `QualityOfLifeExpectation`.

Skills in a duplicant's aptitude group grant +1 morale (not expectation) via `MinionResume.UpdateMorale()`.

## Morale Sources

### Base Morale

Every duplicant starts with a base morale of **1** (`BaseMinionConfig`: `QualityOfLife.Id, 1f`).

### Food Quality

Food quality ranges from -1 (Grisly) to 5 (Superb). When a duplicant eats, `Edible.AddOnConsumeEffects()` applies an effect based on the food's quality adjusted by the duplicant's `FoodExpectation` attribute (which traits like Foodie modify by -1, SimpleTastes by +1). The effective quality is clamped to [-1, 5].

Effect mapping (`Edible.qualityEffects`):

| Effective Quality | Effect ID | Morale Bonus |
|-------------------|-----------|-------------|
| -1 (Grisly) | EdibleMinus3 | -1 |
| 0 (Terrible) | EdibleMinus2 | 0 |
| 1 (Poor) | EdibleMinus1 | +1 |
| 2 (Standard) | Edible0 | +4 |
| 3 (Good) | Edible1 | +8 |
| 4 (Great) | Edible2 | +12 |
| 5 (Superb) | Edible3 | +16 |

Morale values are from YAML effect definitions, not C# source. Validated against wiki.

### Decor

`DecorMonitor` tracks a running total of decor values sampled at the duplicant's cell each frame. At the start of each new day (`OnNewDay`), the daily average is computed as `cycleTotalDecor / 600` (600 seconds per cycle), offset by the duplicant's `DecorExpectation` attribute. The result selects a tier from `effectLookup`:

| Threshold (avg + expectation) | Effect ID | Morale Bonus |
|-------------------------------|-----------|-------------|
| < -30 (MAXIMUM_DECOR_VALUE * -0.25) | DecorMinus1 | -1 |
| < 0 | Decor0 | 0 |
| < 30 (MAXIMUM_DECOR_VALUE * 0.25) | Decor1 | +1 |
| < 60 (MAXIMUM_DECOR_VALUE * 0.5) | Decor2 | +3 |
| < 90 (MAXIMUM_DECOR_VALUE * 0.75) | Decor3 | +6 |
| < 120 (MAXIMUM_DECOR_VALUE) | Decor4 | +9 |
| >= 120 | Decor5 | +12 |

`MAXIMUM_DECOR_VALUE` is 120. `DecorExpectation` defaults to 0 but is modified by traits (e.g., InteriorDecorator: -5, ArtDown: +5, Uncultured: +20) and skill-based profession modifiers.

### Recreation / Downtime

`RecreationTimeMonitor` tracks completed recreation schedule blocks. The morale bonus equals `max(0, min(count - 1, 5))` where count is the number of recreation blocks completed within the bonus duration. The first recreation block grants no morale; each additional block adds +1, up to **+5** (requires 6 blocks). Each block's timestamp lasts 600 seconds (one cycle) for standard duplicants, 1800 seconds for bionics, after which it expires and stops counting.

From source:
```
MAX_BONUS = 5
BONUS_DURATION_STANDARD = 600f  // 1 cycle
BONUS_DURATION_BIONICS = 1800f  // 3 cycles
```

### Room Bonuses

Room effects are applied when a duplicant uses the primary building in a qualifying room. Effect values are from YAML data files, validated against wiki:

| Room Type | Morale Bonus |
|-----------|-------------|
| Latrine | +1 |
| Washroom | +2 |
| Mess Hall | +3 |
| Great Hall | +6 |
| Barracks | +1 |
| Luxury Barracks | +2 |
| Private Bedroom | +3 |
| Park | +3 |
| Nature Reserve | +6 |

### Other Morale Sources

- **Massage Table**: -30% stress/cycle (`TUNING.RELAXATION.MASSAGE_TABLE = -30f`), applied as a direct stress delta, not as morale
- **Shower**: +3 morale
- **Beach Chair** (sufficient light): +8 morale
- **Hot Tub**: +5 morale
- **Arcade Cabinet**: +3 morale
- **Espresso / Juice / Soda**: +4 morale each
- **Table Salt** (used at mess table): +1 morale
- **Water Cooler**: +1 morale
- **Rehydrated food**: -1 morale penalty (`FoodRehydratorConfig`: `QualityOfLife.Id, -1f`)
- **Bionic Fresh Oil**: variable morale bonus via `BionicOilMonitor`

## Morale-to-Stress Conversion

`QualityOfLifeNeed.Sim4000ms()` runs every 4 seconds and compares morale (QualityOfLife) to expectation (QualityOfLifeExpectation). The morale setting (game difficulty) controls the stress penalty rate.

### Morale Above Expectation (surplus)

Stress decreases. Rate: `surplus * (-1/60)`, capped at `-1/30` per second.

From `TUNING.DUPLICANTSTATS.QOL_STRESS`:
```
ABOVE_EXPECTATIONS = -1f/60f   // rate multiplier per point of surplus
MIN_STRESS = -1f/30f           // maximum stress decrease rate (cap)
```

### Morale Equal to Expectation

Stress decreases at a fixed rate of **-1/120 per second** (~5%/cycle).

```
AT_EXPECTATIONS = -1f/120f
```

### Morale Below Expectation (deficit)

Stress increases. Rate: `deficit * rate_per_point`, capped at a max rate. Both scale with difficulty:

| Difficulty | Rate per Deficit Point | Max Rate |
|------------|----------------------|----------|
| Easy | 0.00333/s | 1/60 /s |
| Default | 0.00417/s | 1/24 /s |
| Hard | 1/120 /s | 0.05/s |
| Very Hard | 1/60 /s | 1/12 /s |

The morale setting can also be **Disabled**, in which case `QualityOfLifeNeed` applies no stress modifier at all.

### Conversion to %/cycle

One cycle = 600 seconds. Examples at default difficulty with a deficit of 5:
```
stress_per_second = min(5 * 0.00417, 1/24) = min(0.02083, 0.04167) = 0.02083
stress_per_cycle = 0.02083 * 600 = 12.5%/cycle
```

## Stress Thresholds and State Machine

`StressMonitor` manages three states:

| State | Stress Range | Behavior |
|-------|-------------|----------|
| `satisfied` | < 60% | Normal operation. Neutral expression |
| `stressed.tier1` | 60-99% | Stressed status item shown. Concern emote reactable. Transitions to tier2 at 100% |
| `stressed.tier2` | 100% | Stress break. Triggers `GameHashes.StressedHadEnough`. Returns to tier1 when stress drops below 100% |

Stress breaks can be disabled via the `StressBreaks` custom game setting. When disabled, `HasHadEnough()` always returns false.

## Stress Responses

Each duplicant has exactly one stress response trait, assigned at creation from `TUNING.DUPLICANTSTATS.STRESSTRAITS`. The response activates via `StressBehaviourMonitor` when `StressMonitor` fires `GameHashes.Stressed` (at 60%).

### Two-Tier Behavior

`StressBehaviourMonitor` has two tiers:

**Tier 1 (60-99% stress):** Periodic stress emotes. The duplicant plays a stress-specific emote chore, then gets a 30-second reprieve before the next one. These are interruptive but brief.

**Tier 2 (100% stress / stress break):** Full stress response chore. The duplicant performs their stress response behavior repeatedly with short reprieves (default 3 seconds, some responses override this). After 150 seconds total in tier 2, stress resets to **60%** (`STRESS.ACTING_OUT_RESET = 60f`) and the duplicant returns to tier 1.

### Response Types

#### Ugly Crier (`UglyCrier`)
- **Tier 1 emote:** Crying animation
- **Tier 2 behavior:** `UglyCryChore` - cries for ~20 seconds, producing water at `STRESS.TEARS_RATE` (0.04 kg/s). Applies "UglyCrying" effect: -30 decor. Leaves "CryFace" effect after completion
- **Colony impact:** Flooding from tears, decor penalty affects nearby duplicants

#### Destructive (`Aggressive`)
- **Tier 1 emote:** Destructive animation
- **Tier 2 behavior:** `AggressiveChore` - finds a `Breakable` building (50% chance to target buildings, otherwise punches walls). Damages buildings via `Breakable.Work()` or walls at 6% per second for up to 26 seconds. Wall damage only affects tiles with strength < 100
- **Colony impact:** Destroyed buildings and infrastructure. Note: duplicants with Aggressive trait cannot repair buildings above 60% stress (documented in trait description)
- **Building damage:** `DUPLICANTSTATS.STANDARD.BaseStats.BUILDING_DAMAGE_ACTING_OUT = 100`

#### Vomiter (`StressVomiter`)
- **Tier 1 emote:** Vomiter interrupt animation
- **Tier 2 behavior:** `VomitChore` - vomits polluted water. Amount per event: `STRESS.VOMIT_AMOUNT` (0.9 kg)
- **Colony impact:** Polluted water with food poisoning germs (`DISEASE_PER_VOMIT = 100000` germs)

#### Binge Eater (`BingeEater`)
- **Tier 1 emote:** Binge eat interrupt animation
- **Tier 2 behavior:** `BingeEatChore` - seeks and consumes 2 kg of food. Applies "Binge_Eating" effect: -30 decor, -6666.67 cal/s calorie drain. Reprieve duration: 8 seconds
- **Colony impact:** Rapid food consumption depletes colony reserves

#### Banshee Wail (`Banshee`)
- **Tier 1 emote:** Banshee interrupt animation
- **Tier 2 behavior:** `BansheeChore` - seeks audience within `STRESS.BANSHEE_WAIL_RADIUS` (8 tiles, line-of-sight required, checked via `Grid.CollectCellsInLine`). Wails for 5 seconds, applies "WailedAt" effect to nearby duplicants, causes them to flee. "BansheeWailing" effect: adds 75x base oxygen consumption rate as a modifier (total rate becomes 76x normal). Recovery phase: "BansheeWailingRecovery" effect adds 10x base rate (total 11x normal)
- **Colony impact:** Other duplicants get stressed and flee, oxygen gets consumed rapidly

#### Stress Shocker (`StressShocker`) - Bionic DLC only
- **Tier 1 emote:** Shocker animation
- **Tier 2 behavior:** `StressShockChore` - runs around shocking targets within `STRESS.SHOCKER.SHOCK_RADIUS` (4 tiles). Damages health at `STRESS.SHOCKER.DAMAGE_RATE` (2.5/s). Drains bionic battery at `STRESS.SHOCKER.POWER_CONSUMPTION_RATE` (2000W). Stops after consuming `STRESS.SHOCKER.MAX_POWER_USE` (120000J) total power. Reprieve duration: 12 seconds
- **Colony impact:** Damages creatures, duplicants, electobanks, and wires in range

## Joy Reactions (Overjoyed)

The inverse of stress responses. Each duplicant has one joy trait from `TUNING.DUPLICANTSTATS.JOYTRAITS`.

`JoyBehaviourMonitor` checks each schedule block tick whether morale exceeds expectation by at least `TRAITS.JOY_REACTIONS.MIN_MORALE_EXCESS` (8 points). The chance scales linearly from 1/12 (at +8 surplus) to 5/24 (at +20 surplus). Duration: 1800 seconds (3 cycles).

Joy traits: Balloon Artist, Sparkle Streaker, Sticker Bomber, Super Productive, Happy Singer, Data Rainer (DLC3), Robo Dancer (DLC3).

## Thriver Trait

The `Thriver` trait is unique: it applies the "Thriver" effect when the duplicant is stressed (above 60%), granting a positive buff while stressed. This is a good trait that benefits from otherwise negative conditions.

## Relevant TUNING Constants

```csharp
// StressMonitor thresholds
StressThreshold_One = 60f    // enter stressed state
StressThreshold_Two = 100f   // stress break

// StressBehaviourMonitor
TIER2_STRESS_RESPONSE_TIMEOUT = 150f  // seconds in tier 2 before reset
ACTING_OUT_RESET = 60f                // stress value after tier 2 timeout

// Tier 1 reprieve between emotes
reprieve_duration = 30f  // seconds

// Stress response specific
VOMIT_AMOUNT = 0.9f           // kg per vomit
TEARS_RATE = 0.04f            // kg/s while crying
BANSHEE_WAIL_RADIUS = 8       // tiles
SHOCKER.SHOCK_RADIUS = 4      // tiles
SHOCKER.DAMAGE_RATE = 2.5f    // HP/s
SHOCKER.POWER_CONSUMPTION_RATE = 2000f  // watts
SHOCKER.MAX_POWER_USE = 120000f         // joules total

// QOL stress rates (per second)
QOL_STRESS.ABOVE_EXPECTATIONS = -1f/60f   // multiplier per surplus point
QOL_STRESS.AT_EXPECTATIONS = -1f/120f     // flat rate when equal
QOL_STRESS.MIN_STRESS = -1f/30f           // max decrease rate (cap)

// Standard stress modifiers (per second)
STANDARD_STRESS_PENALTY = 1f/60f    // ~10%/cycle
STANDARD_STRESS_BONUS = -1f/30f     // ~-20%/cycle

// Joy reaction thresholds
JOY_REACTIONS.MIN_MORALE_EXCESS = 8f
JOY_REACTIONS.MAX_MORALE_EXCESS = 20f
JOY_REACTIONS.MIN_REACTION_CHANCE = 1f/12f
JOY_REACTIONS.MAX_REACTION_CHANCE = 5f/24f
JOY_REACTIONS.JOY_REACTION_DURATION = 1800f  // seconds (3 cycles)
```
