# Creature AI

Creature AI, diet, reproduction, taming, and lifecycle -- how creatures behave regardless of species. For per-species stats (diets, temperatures, egg chances, production rates), see `creature-species.md`. Derived from decompiled source code.

## Brain Architecture

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
| 18 | CritterCondoStates | Using condo (adults only) |
| 19 | IdleStates | Default wandering |

All behaviors are `GameStateMachine`-based, with type-safe state definitions and tag-driven transitions.

## Diet and Metabolism

Creature diets are defined as arrays of `Diet.Info` entries, each specifying:
- **Consumed tags:** What the creature accepts (element tags, food tags, ore tags)
- **Produced element:** What the creature excretes
- **Calories per kg:** Energy gained from consumption
- **Conversion rate:** Mass ratio of output to input (e.g., 0.5 = 50% mass retained as excrement)
- **Disease:** Optional pathogen added to output with specified concentration

**Example (Hatch diets):**
- Basic Rock Diet: Sand, Sandstone, Clay, Dirt, etc. -> produces excrement at configured ratio
- Metal Diet: Ore tags -> produces refined metals (CopperOre -> Copper)
- Hard Rock Diet: Sedimentary Rock, Igneous Rock, Obsidian, Granite -> produces excrement

The `CreatureCalorieMonitor` tracks hunger as an Amount (0 to species-specific max):

**State machine:**
```
normal -> hungry.hungry -> hungry.outofcalories (wild | tame | starvedtodeath)
```

- **Normal:** Calories above hunger threshold (default: 90% of max). Base metabolism burns calories over time.
- **Hungry:** Below threshold. Creature seeks food. Metabolism rate may change.
- **Out of Calories:** Starvation. Splits into wild vs tame paths.
  - Wild creatures: survive indefinitely (no starvation death)
  - Tame creatures: die after `deathTimer` seconds (default 6000s / 10 cycles) at zero calories

**Metabolism modifiers:**
- Tame creatures burn calories faster than wild ones
- Temperature stress increases metabolic rate
- `GameTags.Creatures.PausedHunger` can suspend hunger entirely

## Reproduction

Reproduction is handled by `FertilityMonitor` (egg-laying) and `IncubationMonitor` (egg hatching).

**Fertility cycle:**
1. Fertility amount increases from 0 to 100 over time. Rate is species-specific: `100 / (baseFertileCycles * 600)` per second
2. Rate modified by happiness, temperature comfort, and feeding status
3. At 100%: creature lays an egg and fertility resets to 0

**Egg type selection:** Each creature defines possible egg types with base weights. Environment modifiers shift weights:
- Element exposure (atmosphere type) can boost specific egg variants
- Diet type can influence which subspecies egg is laid
- Modifiers are applied via `OnUpdateEggChances` events each cycle

**Incubation:**
- Eggs have an incubation amount (0 to 100)
- Base rate is species-specific (`baseIncubationRate` per second)
- Incubators apply a speed multiplier
- Lullabied eggs receive a bonus
- Temperature outside viable range pauses incubation

**Egg viability:** Eggs track a viability amount. If the egg's temperature is outside its viable range for too long, viability drops. At 0 viability, the egg dies without hatching.

## Wildness and Domestication

`WildnessMonitor` tracks the wild-to-tame transition:

**Wildness amount:** 0 (fully tame) to 100 (fully wild), starting at 100 for wild-spawned creatures.

**Taming:** Grooming applies a "Ranched" effect that reduces wildness at -11/120 per second. Eating from a fish feeder applies a separate modifier of -1/30 per second. Wild creatures have a natural wildness increase of +1/120 per second. When wildness reaches 0, the creature becomes domesticated.

**Wild vs tame effects:**

| Aspect | Wild | Tame |
|--------|------|------|
| Lifespan | Longer (species-specific) | Shorter |
| Metabolism | Slower calorie burn | Faster calorie burn |
| Reproduction | Slower fertility rate | Faster fertility rate |
| Starvation | Survives longer | Dies sooner |

The wild effect (applied while `GameTags.Creatures.Wild` is present) ticks wildness upward at +1/120 per second. Continuous grooming offsets this to drive wildness to 0. Once tame (wildness = 0), the wild tag is removed and the natural increase stops, but wildness can increase again if the creature transitions back to the wild state.

## Age and Lifespan

`AgeMonitor` tracks creature aging:

- Age amount increases at 0.0016667 per second (1 cycle of age per 600-second game cycle)
- Maximum age is species-specific, with separate wild and domestic values
- At max age: creature dies of old age
- Baby creatures track a separate `Maturity` amount; at 100%, they grow into adults

**Growth:** Baby-to-adult transition triggers `GrowUpStates`, which replaces the baby prefab with the adult prefab, preserving temperature, disease, and taming status.

## Happiness and Mood

`HappinessMonitor` tracks creature mood on a simple scale:

| State | Happiness Value | Effect |
|-------|----------------|--------|
| Happy | >= 4 | +900% fertility rate (tame only) |
| Neutral | > -1 and < 4 | Normal behavior |
| Glum | > -10 and <= -1 | Reduced metabolism (-15 wild, -80 tame) |
| Miserable | <= -10 | Metabolism penalty, fertility and scale growth halted |

**Happiness modifiers:**
- Groomed (Ranched effect): +5
- Baby bonus: +5
- Fish feeder bonus: +5
- Tame penalty: -1 (always applied while tame)
- Overcrowded: -1 per excess creature beyond room capacity
- Confined (no room): -10
- Out of calories (tame): -10
- Temperature stress: -1 (uncomfortable), -2 (deadly range)
- Condo interaction: +1

The `CritterEmoteMonitor` displays creature emotions as thought bubbles. Emotes have 30-second cooldowns and prioritize negative emotions over positive ones. Expression intervals range from 37.5 to 75 seconds.

## Temperature Comfort

`CritterTemperatureMonitor` tracks thermal comfort:

**States:**
```
comfortable <-> hot.uncomfortable <-> hot.deadly
            <-> cold.uncomfortable <-> cold.deadly
                                       dead (via GameTags.Dead tag transition)
```

- Internal temperature: creature's `PrimaryElement.Temperature`
- External temperature: average of occupied cells (uses internal temp for vacuum cells)
- Deadly state inflicts `damagePerSecond` (default 0.25) with 1-second cooldown after initial `secondsUntilDamageStarts` delay (default 1s)
- Deadly thresholds compare against external temperature (skipped in vacuum); uncomfortable thresholds compare against internal temperature
- Each temperature state applies a happiness modifier

## Threat Response

`ThreatMonitor` handles fight-or-flight decisions:

**Detection:**
- Scans up to 20 cells (configurable `maxSearchDistance`)
- Checks up to 50 entities (configurable `maxSearchEntities`)
- Identifies threats based on creature tags (excluding `friendlyCreatureTags`)

**Grudge system:** Creatures hold 10-second grudges against attackers. The grudge target becomes the priority threat regardless of distance. Grudges expire with a "forgiveness" PopFX.

**Response states:**
```
safe.passive (faction cannot attack -- does nothing)
safe.seeking -> threatened.creature (for critters: flee or fight based on WillFight)
safe.seeking -> threatened.duplicant.ShoudFlee / .ShouldFight (for duplicants)
```

`WillFight()` returns false if the threat is a Predator-faction creature, or if health has deteriorated to or past `fleethresholdState` (enum order: Perfect, Alright, Scuffed, Injured, Critical, Incapacitated, Dead). Default threshold is `Injured`, so creatures flee once injured. Hatches set it to `Dead`, meaning they fight at any health level.

## Scale Growth and Molting

`ScaleGrowthMonitor` handles renewable resource harvesting:

- Growth tracked as 0-100% amount
- Rate: `defaultGrowthRate` percentage per cycle
- **Atmosphere requirement:** Some creatures require specific gas for scale growth; wrong atmosphere triggers stunted state (no growth)
- At 100%: creature can be sheared, dropping `itemDroppedOnShear` with configured mass
- Shearing resets growth to 0

Visual scale levels update in 5 stages as growth progresses.

## Death

`DeathMonitor` handles the death process:

**Triggers:** Starvation (calories reach 0), temperature damage (HP reaches 0), old age (age reaches max), drowning, explicit kill.

**Process:**
1. Creature enters `dying_creature` state via `ToggleBehaviour(GameTags.Creatures.Die)`
2. On completion, enters `dead_creature` state: `GameTags.Dead` is applied and death animation loops
3. Duplicants follow a separate path (`dying_duplicant` -> `die` -> `dead`) which adds `GameTags.Corpse` and supports carrying

Creature deaths are silent (no notification), unlike duplicant deaths which trigger alerts and sound effects.
