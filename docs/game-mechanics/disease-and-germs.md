# Disease and Germs

How germs grow, spread, and infect duplicants. Derived from decompiled source code.

## Overview

ONI tracks germs at both the cell level and the entity level. Each cell can hold one disease type with an integer germ count. Germs grow, die, spread, and infect duplicants through multiple pathways.

## Per-Cell Disease Data

- `Grid.DiseaseIdx[cell]` (byte) - Index of disease present (255 = none)
- `Grid.DiseaseCount[cell]` (int) - Number of germs in the cell

## Disease Types

There are 5 germ types in the game:

| Germ | ID | Primary Vector | Threshold |
|------|----|---------------|-----------|
| Food Poisoning | FoodPoisoning | Digestion | 100 germs |
| Slimelung | SlimeLung | Inhalation | 100 germs |
| Zombie Spores | ZombieSpores | Inhalation + Contact | 1 germ |
| Pollen | PollenGerms | Inhalation | 2 germs |
| Radiation Poisoning | RadiationSickness | None (sickness_id is null) | 1 germ |

## Growth and Die-Off: The Half-Life System

Germ populations change over time based on **half-life values** that depend on what element they're on. A negative half-life means growth (population doubling time), a positive half-life means die-off.

Each disease defines growth rules per element via `ElementGrowthRule`:
- A set of temperature ranges with associated half-lives
- Different rates for different elements
- 4-tier temperature interpolation: `minViable`, `minGrowth`, `maxGrowth`, `maxViable` (symmetric curve)

**Temperature affects growth through ranges:**

| Range | Effect |
|-------|--------|
| Below `minViable` | Lethal: rapid die-off (short positive half-life) |
| `minViable` to `minGrowth` | Slow growth or slow die-off |
| `minGrowth` to `maxGrowth` | Optimal: maximum growth (most negative half-life) |
| `maxGrowth` to `maxViable` | Slow growth or slow die-off |
| Above `maxViable` | Lethal: rapid die-off |

**Example growth rates (half-life in seconds):**
- Food Poisoning on Polluted Water: -12,000s (grows, doubles every ~3.3 hours)
- Food Poisoning on any liquid (generic liquid state rule): 12,000s (dies, halves every ~3.3 hours)
- Food Poisoning on Chlorine: 10s (dies extremely fast)
- Slimelung on Polluted Oxygen: -300s (grows quickly)
- Slimelung on Clean Oxygen: 1,200s (dies slowly)

## Germ Spreading Between Cells

Germs diffuse to adjacent cells when population exceeds thresholds. The diffusion is proportional to mass transfer: when mass moves between cells (gas diffusion, liquid flow), germs move with it proportionally.

## Radiation Kills Germs

Each disease has a `radiationKillRate`:
- Food Poisoning: 2.5 (killed in 2-3 seconds at 1 rad)
- Slimelung: 2.5
- Zombie Spores: 1.0 (slower killing)
- Pollen: 0.0 (immune to radiation)
- Radiation Sickness: 0.0 (is radiation)

High radiation sterilizes items and prevents most disease growth.

## Pressure Is Irrelevant

All 5 diseases use `RangeInfo.Idempotent()` for pressure, meaning pressure has zero effect on germ growth or death.

## Disease Mixing

When two disease sources combine (e.g., merging pipe contents), the system uses strength-weighted competition:
1. Same disease: counts simply add together
2. Different diseases: compare `disease.strength * count` for each
3. The winning disease (higher product) survives
4. The loser's count is reduced based on the strength ratio

## Infection Pathway

Germ exposure follows a state machine:

```
None -> Contact -> Exposed -> Contracted -> Sick
```

**Contact:** Duplicant touches germs above the exposure threshold. The source determines the infection vector:
- **Inhalation:** Breathing germy gas (OxygenBreather consumes gas + germs)
- **Digestion:** Eating contaminated food (germs proportional to food fraction eaten)
- **Contact:** Physical touch with contaminated surfaces

**Exposed:** Accumulation exceeds threshold. For inhalation, requires continuous exposure (10+ ticks for Tier 1, 15+ for Tier 2, 20+ for Tier 3). Tier is clamped to [1, 3].

**Contracted:** Happens via `GermExposureTracker`, which accumulates exposure weights and randomly selects exposed duplicants to contract. On **sleep**, Exposed is cleared (back to None), while Contracted advances to Sick and the duplicant becomes infected.

**Minimum exposure period:** 540 seconds (9 minutes) between same-germ exposures, preventing rapid stacking.

## Contraction Chance

The probability of contracting a disease from exposure uses a hyperbolic tangent curve:

```
contractionChance = 0.5 - 0.5 * tanh(0.25 * resistanceRating)
```

Where:
```
resistanceRating = base_resistance + duplicant_germ_resistance + exposure_tier_bonus
```

Exposure tier bonuses: Tier 1 = +3.0, Tier 2 = +1.5, Tier 3 = +0.0

| Rating | Chance |
|--------|--------|
| 0 | 50% |
| 2 | ~27% |
| 5 | ~7.6% |
| 10 | ~0.7% |
| 20 | <0.01% |

## Sickness Effects

Each disease causes different gameplay effects when contracted:

**Food Poisoning** (17 minutes):
- Increased bladder fill rate, reduced toilet efficiency, reduced stamina
- Periodic sick emotes

**Slimelung** (37 minutes):
- -3 Athletics, -125% breath rate (breath delta multiplied by -1.25, severely restricting breathing)
- **Active coughing spreads disease:** produces 0.1 kg Contaminated Oxygen + 1000 Slimelung germs per cough

**Zombie Spores** (3 hours):
- -10 to ALL 11 skill attributes
- Completely debilitates the duplicant
- Visible spore cloud effect

**Allergies** (60 seconds, requires Allergies trait):
- +15% stress per cycle, +10 Sneezyness
- Contracts immediately (no sleep required)
- Non-allergic duplicants get harmless "Smelled Flowers" instead

**Radiation Sickness** (3 hours):
- Same symptoms as Zombie Spores (-10 to all skills)
- Not transmitted by normal germ mechanics; caused by direct radiation exposure

## Recovery

When cured, a recovery effect prevents immediate re-infection (e.g., "FoodSicknessRecovery"). While the recovery effect is active, the same sickness cannot be contracted again.

## Immunity Traits

- **IronGut:** Prevents Food Poisoning exposure entirely
- **Allergies:** Required to be affected by Pollen germs (others are immune)

## Disease in Pipes

Conduit systems track disease separately via `ConduitDiseaseManager`. When fluid moves through pipes, germs travel proportionally with the mass. Disease mixing in pipes follows the same strength-weighted rules.

## Special Element Interactions

- **Contaminated Oxygen:** Growth medium for Slimelung (-300s). Food Poisoning survives longer than on other gases (12,000s die-off vs 1,200s for generic gas) but does not grow
- **Pickled food:** Kills Food Poisoning germs extremely fast (10s half-life)
- **Chlorine:** Universal sterilizer for most germs
- **Creatures:** Living creatures (element SimHashes.Creature) become walking disease vectors
