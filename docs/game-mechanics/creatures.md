# Creatures

Creature AI, diet, reproduction, taming, and lifecycle. Derived from decompiled source code.

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
| 18 | PlayStates | Using recreation |
| 19 | CritterCondoStates | Using condo (adults only) |
| 20 | IdleStates | Default wandering |

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
- Hard Rock Diet: Granite, Obsidian, Igneous Rock -> produces excrement

The `CreatureCalorieMonitor` tracks hunger as an Amount (0 to species-specific max):

**State machine:**
```
hungry.outOfCalories -> hungry.hungry -> satisfied -> eating
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

## Reproduction

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

## Wildness and Domestication

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

## Age and Lifespan

`AgeMonitor` tracks creature aging:

- Age amount increases continuously (base rate: 0.1675 per update)
- Maximum age is species-specific, with separate wild and domestic values
- At max age: creature dies of old age
- Baby creatures track a separate `Maturity` amount; at 100%, they grow into adults

**Growth:** Baby-to-adult transition triggers `GrowUpStates`, which replaces the baby prefab with the adult prefab, preserving temperature, disease, and taming status.

## Happiness and Mood

`HappinessMonitor` tracks creature mood on a simple scale:

| State | Happiness Value | Effect |
|-------|----------------|--------|
| Happy | >= 4 | Increased reproduction rate |
| Neutral | -1 to 4 | Normal behavior |
| Glum | -10 to -1 | Reduced reproduction |
| Miserable | <= -10 | No reproduction, severe penalties |

**Happiness modifiers:**
- Correct temperature: +1 per comfortable state
- Overcrowded: -1 per excess creature in room
- Fed (tame): +1 when not hungry
- Groomed: +1 from recent grooming
- Temperature stress: -1 (uncomfortable), -2 (deadly range)

The `CritterEmoteMonitor` displays creature emotions as thought bubbles. Emotes have 30-second cooldowns and prioritize negative emotions over positive ones. Expression intervals range from 37.5 to 75 seconds.

## Temperature Comfort

`CritterTemperatureMonitor` tracks thermal comfort:

**States:**
```
comfortable <-> hot.uncomfortable <-> hot.deadly
            <-> cold.uncomfortable <-> cold.deadly -> dead
```

- Internal temperature: creature's `PrimaryElement.Temperature`
- External temperature: average of occupied cells (or internal temp if in vacuum)
- Deadly state inflicts configurable damage per second with 1-second cooldown
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
safe.passive -> threatened.creature (flee or fight based on species)
safe.seeking -> threatened.duplicant (flee or fight based on species)
```

Flee threshold is configurable per species. Some creatures always fight (e.g., adult Hatches when attacked), some always flee (e.g., Pufts).

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
1. Creature enters dying state, plays death animation
2. Tags `GameTags.Dead` and `GameTags.Corpse` are applied
3. Creature drops to ground state
4. No carrying mechanics for creature corpses (unlike duplicants)

Creature deaths are silent (no notification), unlike duplicant deaths which trigger alerts and sound effects.
