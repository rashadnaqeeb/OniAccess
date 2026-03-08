# Duplicant Attributes and Skills

How duplicant attributes are defined, leveled, and modified; how the skill tree grants perks and raises morale expectations; how interests accelerate learning; and how traits shape starting stats. Derived from decompiled source code (`Database/Attributes.cs`, `Database/Skills.cs`, `Database/SkillPerks.cs`, `MinionResume.cs`, `Klei.AI/AttributeLevel.cs`, `TUNING/SKILLS.cs`, `TUNING/DUPLICANTSTATS.cs`, `TUNING/ROLES.cs`).

## Attributes

### Trainable (Profession) Attributes

These are the core duplicant attributes. All start at base value 0 and gain levels through work experience. Each attribute maps to a skill group and chore group.

| Attribute ID | Display Name (STRINGS key) | Skill Group | What It Affects (via AttributeConverters) |
|---|---|---|---|
| `Construction` | Construction | Building | Construction speed (+25%/point) |
| `Digging` | Excavation | Mining | Digging speed (+25%/point), attack damage (+5%/point) |
| `Machinery` | Machinery | Technicals | Machine operating speed (+10%/point), tune-up effect duration (+2.5%/point) |
| `Athletics` | Athletics | Suits | Movement speed (+10%/point) |
| `Learning` | Science | Research | Research speed (+40%/point), training speed (+10%/point), geotuning speed (+5%/point) |
| `Cooking` | Cuisine | Cooking | Cooking speed (+5%/point) |
| `Caring` | Medicine | MedicalAid | Compounding speed (+10%/point), doctoring speed (+20%/point) |
| `Strength` | Strength | Hauling/Basekeeping | Tidying speed (+25%/point), carry amount (+40 kg/point) |
| `Art` | Creativity | Art | Art speed (+10%/point) |
| `Botanist` | Agriculture | Farming | Harvest speed (+5%/point), plant tend speed (+2.5%/point), seed harvest chance (+3.3%/point), farmer's touch duration (+10%/point) |
| `Ranching` | Husbandry | Ranching | Ranching effect duration (+10%/point), capture speed (+5%/point) |
| `SpaceNavigation` | Piloting | Rocketry (DLC1) | Piloting speed (+2.5%/point) |

Other trainable attributes that do not appear in the skills UI: `LifeSupport`, `Toggle`, `PowerTinker`, `FarmTinker`.

### Non-Trainable Attributes

These cannot be leveled through work. They are modified by traits, equipment, status effects, or skills.

| Attribute ID | Purpose |
|---|---|
| `Immunity` | Immune level recovery rate |
| `GermResistance` | Resistance to germ exposure |
| `ThermalConductivityBarrier` | Insulation from environment temperature |
| `Insulation` | Temperature insulation modifier |
| `Decor` | Decor contribution |
| `FoodQuality` | Food quality level |
| `ScaldingThreshold` | Temperature at which scalding occurs |
| `ScoldingThreshold` | Temperature at which scolding (cold) occurs |
| `CarryAmount` | Additional carry capacity (in kg) |
| `QualityOfLife` | Morale (total from all sources) |
| `QualityOfLifeExpectation` | Morale expectation (from skills) |
| `DecorExpectation` | Minimum acceptable decor |
| `FoodExpectation` | Minimum acceptable food quality |
| `AirConsumptionRate` | Oxygen consumption rate |
| `MachinerySpeed` | Base machinery speed multiplier (starts at 1.0) |
| `RadiationResistance` | Radiation resistance (DLC1 only) |
| `RadiationRecovery` | Radiation recovery rate per cycle (DLC1 only) |
| `BionicBoosterSlots` | Booster slots for bionic duplicants (DLC3, base 2) |
| `BionicBatteryCountCapacity` | Battery slots for bionics (DLC3, base 4) |

## Attribute Leveling (Experience System)

Trainable attributes gain levels through performing related work. This is separate from the skill point system.

### Tuning Constants (`DUPLICANTSTATS.ATTRIBUTE_LEVELING`)

| Constant | Value | Meaning |
|---|---|---|
| `MAX_GAINED_ATTRIBUTE_LEVEL` | 20 | Maximum level achievable through experience |
| `TARGET_MAX_LEVEL_CYCLE` | 400 | Target cycle to reach max level if working full time |
| `EXPERIENCE_LEVEL_POWER` | 1.7 | Exponent controlling experience curve steepness |

### Experience Required Per Level

Experience required to advance from level N to level N+1:

```
expForLevel(N+1) - expForLevel(N)

where expForLevel(L) = ((L / MAX_LEVEL) ^ POWER) * TARGET_CYCLE * 600
```

Each level adds +1 to the attribute as an `AttributeModifier` with description "Skill Level". Higher levels require progressively more experience due to the 1.7 power curve.

### Experience Multipliers for Workables

Different tasks grant attribute experience at different rates. The `attributeExperienceMultiplier` on each workable controls how much credit each second of work provides toward the relevant attribute level:

| Multiplier Category | Value | Example Tasks |
|---|---|---|
| `FULL_EXPERIENCE` | 1.0 | Food smoker |
| `ALL_DAY_EXPERIENCE` | 1.25 (1/0.8) | Research stations, telescopes, manual generator (Athletics) |
| `MOST_DAY_EXPERIENCE` | 2.0 (1/0.5) | Digging, construction, cooking, art, ranching, deconstructing |
| `PART_DAY_EXPERIENCE` | 4.0 (1/0.25) | Harvesting, repairing, mopping, fabricators, tinkering |
| `BARELY_EVER_EXPERIENCE` | 10.0 (1/0.1) | Doctoring, egg incubation, fossil excavation, rocket control |

The `TrainingSpeed` converter from the `Learning` attribute further multiplies attribute experience gain: each point of Learning adds +10% training speed.

## Skill Points (MinionResume)

Skill points are a separate currency from attribute experience. They unlock skills in the skill tree.

### Tuning Constants (`TUNING.SKILLS`)

| Constant | Value | Meaning |
|---|---|---|
| `TARGET_SKILLS_EARNED` | 15 | Target total skill points in the time window |
| `TARGET_SKILLS_CYCLE` | 250 | Target cycles to earn that many points |
| `EXPERIENCE_LEVEL_POWER` | 1.44 | Power curve for skill point spacing |
| `PASSIVE_EXPERIENCE_PORTION` | 0.5 | Experience per second while alive (passive) |
| `ACTIVE_EXPERIENCE_PORTION` | 0.6 | Multiplier on active experience toward skill points |
| `APTITUDE_EXPERIENCE_MULTIPLIER` | 0.5 | Bonus multiplier for working in an interest area |

### How Skill Points Are Earned

Every 200ms tick, living duplicants gain passive experience:
```
passiveExp = dt * 0.5
```

When performing work, active experience is also added:
```
activeExp = workAmount * aptitudeMultiplier * 0.6

where aptitudeMultiplier = 1 + (aptitudeValue * 0.5 * buildingFrequencyMultiplier)
```

If the duplicant has an interest (aptitude) in the skill group associated with that work, `aptitudeValue` is typically 1, yielding a 1.5x multiplier (before the building frequency factor).

### Skill Points from Total Experience

```
totalSkillPoints = floor( (totalExp / (TARGET_SKILLS_CYCLE * 600)) ^ (1 / EXPERIENCE_LEVEL_POWER) * TARGET_SKILLS_EARNED )
```

The 600 factor converts cycles to seconds (1 cycle = 600s). Available (unspent) skill points:
```
available = totalSkillPoints - masteredSkillCount + grantedSkillCount
```

Granted skills (from traits like `GrantSkill_Mining1`) do not consume skill points.

## Skill Tree

### Skill Structure

Each skill (`Database.Skill`) has:
- **Tier** (0-4): Determines morale cost and position in the tree
- **Skill group**: Which category it belongs to (Mining, Building, etc.)
- **Prior skills**: Prerequisites that must be mastered first
- **Perks**: List of `SkillPerk` objects applied when mastered
- **Hat**: Cosmetic hat awarded
- **Required duplicant model**: "Minion" for standard duplicants, bionic tag for bionic-only skills

### Morale Cost Per Tier (`SKILL_TIER_MORALE_COST`)

| Tier | Morale Expectation Added |
|---|---|
| 0 | +1 |
| 1 | +2 |
| 2 | +3 |
| 3 | +4 |
| 4 | +5 |

Each mastered skill adds `tier + 1` to morale expectation. Granted skills (from traits) do not increase morale expectation.

### Skill Attribute Bonuses

All skill tiers grant the same attribute bonus per perk:

| Constant | Value |
|---|---|
| `ROLES.ATTRIBUTE_BONUS_FIRST` | +2 |
| `ROLES.ATTRIBUTE_BONUS_SECOND` | +2 |
| `ROLES.ATTRIBUTE_BONUS_THIRD` | +2 |

So each skill tier that includes an attribute perk adds +2 to the relevant attribute. Stacking multiple tiers gives cumulative bonuses (e.g., Mining 1 + Mining 2 + Mining 3 = +6 Digging from skills alone).

### Complete Skill Tree (Standard Duplicants)

#### Mining (Skill Group: Mining, Attribute: Digging)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Mining1` | Hard Digging | 0 | None | Digging +2 | Dig Very Firm materials |
| `Mining2` | Superhard Digging | 1 | Mining1 | Digging +2 | Dig Nearly Impenetrable materials |
| `Mining3` | Super-Duperhard Digging | 2 | Mining2 | Digging +2 | Dig Diamond/Obsidian |
| `Mining4` | Hazmat Digging | 3 | Mining3 | None | Dig Radioactive materials (DLC1 only) |

#### Building (Skill Group: Building, Attribute: Construction)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Building1` | Improved Construction I | 0 | None | Construction +2 | -- |
| `Building2` | Improved Construction II | 1 | Building1 | Construction +2 | -- |
| `Building3` | Demolition | 2 | Building2 | Construction +2 | Demolish buildings |

#### Farming (Skill Group: Farming, Attribute: Botanist)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Farming1` | Improved Farming I | 0 | None | Botanist +2 | -- |
| `Farming2` | Crop Tending | 1 | Farming1 | Botanist +2 | Farm Tinker, Farm Station |
| `Farming3` | Improved Farming II | 2 | Farming2 | Botanist +2 | Salvage Plant Fiber, Identify Mutant Seeds (if DLC1) |

#### Ranching (Skill Group: Ranching, Attribute: Ranching)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Ranching1` | Critter Ranching I | 1 | Farming1 | Ranching +2 | Wrangle Creatures, Ranch Station |
| `Ranching2` | Critter Ranching II | 2 | Ranching1 | Ranching +2 | Milking Station |

#### Research (Skill Group: Research, Attribute: Learning)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Researching1` | Advanced Research | 0 | None | Learning +2 | Advanced Research, Chemistry |
| `Researching2` | Field Research | 1 | Researching1 | Learning +2 | Study World Objects, Geyser Tuning |
| `Astronomy` | Astronomy | 1 | Researching1 | None | Cluster Telescope, Mission Control (DLC1) |
| `AtomicResearch` | Applied Sciences Research | 2 | Researching2 | Learning +2 | Nuclear Research (DLC1) |
| `SpaceResearch` | Data Analysis Researcher | 2 | Astronomy | Learning +2 | Orbital Research (DLC1) |
| `Researching3` | Astronomy | 2 | Researching2 | Learning +2 | Interstellar Research, Mission Control (base game) |

#### Cooking (Skill Group: Cooking, Attribute: Cooking)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Cooking1` | Grilling | 0 | None | Cooking +2 | Electric Grill, Gas Range, Deep Fryer |
| `Cooking2` | Grilling II | 1 | Cooking1 | Cooking +2 | Spice Grinder |

#### Art (Skill Group: Art, Attribute: Art)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Arting1` | Art Fundamentals | 0 | None | Art +2 | Ugly Art, Basic Art |
| `Arting2` | Aesthetic Design | 1 | Arting1 | Art +2 | Okay Art, Clothing Alteration, Study Artifacts (DLC1) |
| `Arting3` | Masterworks | 2 | Arting2 | Art +2 | Great Art |

#### Hauling (Skill Group: Hauling, Attributes: Strength + CarryAmount)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Hauling1` | Improved Carrying I | 0 | None | Strength +2, CarryAmount +400 kg | -- |
| `Hauling2` | Improved Carrying II | 1 | Hauling1 | Strength +2, CarryAmount +800 kg | -- |

#### Suits (Skill Group: Suits, Attribute: Athletics)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `ThermalSuits` | Suit Sustainability Training | 1 | Hauling1 (+ RocketPiloting1 in DLC1) | Athletics +2 | Exosuit Durability |
| `Suits1` | Exosuit Training | 2 | ThermalSuits | Athletics +2 | Exosuit Expertise (no speed penalty in suits) |

#### Technicals (Skill Group: Technicals, Attribute: Machinery)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Technicals1` | Improved Tinkering | 0 | None | Machinery +2 | -- |
| `Technicals2` | Electrical Engineering | 1 | Technicals1 | Machinery +2 | Power Tinker, Craft Electronics (DLC3) |
| `Engineering1` | Mechatronics Engineering | 2 | Hauling2 + Technicals2 | Machinery +2, Construction +2 | Conveyor Build |

#### Basekeeping (Skill Group: Basekeeping, Attribute: Strength)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Basekeeping1` | Improved Strength | 0 | None | Strength +2 | -- |
| `Basekeeping2` | Plumbing | 1 | Basekeeping1 | Strength +2 | Plumbing |
| `Pyrotechnics` | Pyrotechnics | 2 | Basekeeping2 | None | Make Missiles |

#### Medicine (Skill Group: MedicalAid, Attribute: Caring)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Medicine1` | Medicine Compounding | 0 | None | Caring +2 | Compound medicine |
| `Medicine2` | Bedside Manner | 1 | Medicine1 | Caring +2 | Doctor patients |
| `Medicine3` | Advanced Medical Care | 2 | Medicine2 | Caring +2 | Advanced medicine |

#### Rocketry (DLC1 only)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `RocketPiloting1` | Rocket Piloting | 0 | None | None | Rocket Control Station |
| `RocketPiloting2` | Rocket Piloting II | 2 | RocketPiloting1 + Astronomy | SpaceNavigation +2 | -- |

#### Astronautics (Base game only)

| Skill ID | Name | Tier | Prerequisites | Attribute Bonus | Ability Perks |
|---|---|---|---|---|---|
| `Astronauting1` | Rocket Piloting | 3 | Researching3 + Suits1 | None | Use Rockets |
| `Astronauting2` | Rocket Navigation | 4 | Astronauting1 | SpaceNavigation +0.1 | Faster Space Flight |

### Mastery Conditions

A duplicant can master a skill only if all of these pass:
1. **Has at least 1 available skill point** (earned, not yet spent)
2. **All prerequisite skills are mastered**
3. **Is able to learn** the skill (not blocked by a trait that disables the associated chore group)

A skill can still be learned even if it would push morale expectation above current morale; the game shows a stress warning but does not block it.

## Interest (Aptitude) System

### How Interests Are Assigned

At generation, each duplicant receives 1-3 interests (aptitudes) tied to skill groups. Interests are stored in `MinionResume.AptitudeBySkillGroup` as a `Dictionary<HashedString, float>` where the value is typically 1.0.

### Attribute Bonuses from Interests

Each interest grants a starting attribute bonus during duplicant generation. The bonus per interest depends on how many total interests the duplicant has, from `DUPLICANTSTATS.APTITUDE_ATTRIBUTE_BONUSES`:

| Total Interest Count | Bonus Per Interest |
|---|---|
| 1 interest | +7 |
| 2 interests | +3 each |
| 3 interests | +1 each |

All interests receive the same bonus. A duplicant with 2 interests gets +3 to both relevant attributes, not +7 and +3.

### Morale Bonus from Interests

When a duplicant masters a skill in their interest area, they receive +1 morale per interested skill mastered. This is applied as an `AttributeModifier` on `QualityOfLife` with description from `DUPLICANTS.NEEDS.QUALITYOFLIFE.APTITUDE_SKILLS_MOD_NAME`. This partially offsets the morale expectation increase from mastering skills.

### Experience Bonus from Interests

Active experience gain is multiplied when working in an interest area:
```
aptitudeMultiplier = 1 + (aptitudeValue * APTITUDE_EXPERIENCE_MULTIPLIER * buildingFrequencyMultiplier)
                   = 1 + (1.0 * 0.5 * buildingFrequencyMultiplier)
```

With a standard building frequency of 1.0, this gives 1.5x active experience toward skill points when working in an interested area.

## Trait System

### Trait Generation at the Printing Pod

Duplicants are generated with a configuration of positive and negative traits drawn from a deck:

**Trait configuration deck** (20 entries, drawn without replacement until reshuffled):
- 6x: 1 positive, 1 negative
- 6x: 2 positive, 1 negative
- 4x: 1 positive, 2 negative
- 2x: 2 positive, 2 negative
- 1x: 3 positive, 1 negative
- 1x: 1 positive, 3 negative
- Maximum traits total: 4 (`MAX_TRAITS`)

Each trait has a rarity drawn from a separate **rarity deck** (20 entries):
- 7x Common (1)
- 6x Uncommon (2)
- 4x Rare (3)
- 2x Epic (4)
- 1x Legendary (5)

### Negative Traits

Negative traits grant stat point bonuses (distributed across attributes to compensate) and may restrict skill learning.

| Trait ID | Effect | Stat Bonus | Rarity | Skill Restriction |
|---|---|---|---|---|
| `CantResearch` | Cannot research | 0 | Common | Blocks Research aptitude |
| `CantDig` | Cannot dig | +4 | Epic | Blocks Mining aptitude |
| `CantCook` | Cannot cook | 0 | Uncommon | Blocks Cooking aptitude |
| `CantBuild` | Cannot build | +4 | Epic | Blocks Building aptitude |
| `Hemophobia` | Cannot do medical tasks | 0 | Uncommon | Blocks MedicalAid aptitude |
| `ScaredyCat` | Panics easily | 0 | Uncommon | Blocks Mining aptitude |
| `ConstructionDown` | Reduced Construction | +3 | Uncommon | -- |
| `RanchingDown` | Reduced Ranching | +2 | Common | -- |
| `CaringDown` | Reduced Caring | +2 | Common | -- |
| `BotanistDown` | Reduced Agriculture | +2 | Common | -- |
| `ArtDown` | Reduced Creativity | +2 | Common | -- |
| `CookingDown` | Reduced Cooking | +2 | Common | -- |
| `MachineryDown` | Reduced Machinery | +2 | Common | -- |
| `DiggingDown` | Reduced Digging | +3 | Rare | -- |
| `SlowLearner` | Reduced Learning speed | +3 | Rare | -- |
| `NoodleArms` | Reduced Strength | +3 | Rare | -- |
| `DecorDown` | Increased decor expectation | +1 | Common | -- |
| `Anemic` | Greatly reduced Athletics | +5 | Legendary | -- |
| `Flatulence` | Emits natural gas | +3 | Rare | -- |
| `IrritableBowel` | Increased bladder frequency | +1 | Uncommon | -- |
| `Snorer` | Snores (disturbs sleep) | +1 | Rare | -- |
| `MouthBreather` | Increased oxygen consumption | +5 | Legendary | -- |
| `SmallBladder` | Faster bladder fill | +1 | Uncommon | -- |
| `CalorieBurner` | Increased calorie consumption | +4 | Epic | -- |
| `WeakImmuneSystem` | Reduced immunity | +2 | Uncommon | -- |
| `Allergies` | Allergic reactions | +2 | Rare | -- |
| `NightLight` | Needs light to sleep | +2 | Rare | -- |
| `Narcolepsy` | Falls asleep randomly | +5 | Rare | -- |

### Positive Traits

| Trait ID | Effect | Rarity |
|---|---|---|
| `Twinkletoes` | Increased Athletics | Epic |
| `StrongArm` | Increased Strength | Rare |
| `Greasemonkey` | Increased Machinery | Uncommon |
| `DiversLung` | Reduced oxygen consumption | Epic |
| `IronGut` | Immune to food poisoning | Common |
| `StrongImmuneSystem` | Increased immunity | Common |
| `EarlyBird` | Bonus to all attributes in morning | Rare |
| `NightOwl` | Bonus to all attributes at night | Rare |
| `Meteorphile` | No stress from meteors | Rare |
| `MoleHands` | Increased Digging | Rare |
| `FastLearner` | Increased Learning speed | Rare |
| `InteriorDecorator` | Increased decor production | Common |
| `Uncultured` | Reduced decor expectation | Common |
| `SimpleTastes` | Reduced food expectation | Uncommon |
| `Foodie` | Increased Cooking | Common |
| `BedsideManner` | Increased Caring | Common |
| `DecorUp` | Reduced decor expectation | Uncommon |
| `Thriver` | General morale bonus | Epic |
| `GreenThumb` | Increased Agriculture | Common |
| `ConstructionUp` | Increased Construction | Uncommon |
| `RanchingUp` | Increased Ranching | Uncommon |
| `Loner` | Stress reduction when alone (DLC1) | Epic |
| `StarryEyed` | Morale from space exposure (DLC1) | Rare |
| `GlowStick` | Emits light (DLC1) | Epic |
| `RadiationEater` | Heals from radiation (DLC1) | Epic |
| `FrostProof` | Cold immunity (DLC2) | Common |

### Skill-Granting Traits

Some positive traits grant a skill at no skill point cost and no morale expectation increase. These are Legendary or Epic rarity and impose a stat point penalty of -4 to compensate.

Examples: `GrantSkill_Mining1`, `GrantSkill_Mining2`, `GrantSkill_Mining3`, `GrantSkill_Farming2`, `GrantSkill_Ranching1`, `GrantSkill_Cooking1`, `GrantSkill_Arting1`-`3`, `GrantSkill_Suits1`, `GrantSkill_Technicals2`, `GrantSkill_Engineering1`, `GrantSkill_Basekeeping2`, `GrantSkill_Medicine2`.

### Stress Response Traits

Each duplicant has exactly one stress response trait that determines destructive behavior at high stress:
- `Aggressive` - attacks buildings
- `StressVomiter` - vomits
- `UglyCrier` - cries (decor penalty)
- `BingeEater` - eats food uncontrollably
- `Banshee` - screams

### Joy Response Traits

Each duplicant has exactly one joy response trait that activates at very high morale:
- `BalloonArtist` - creates balloons
- `SparkleStreaker` - leaves sparkle trails
- `StickerBomber` - places stickers
- `SuperProductive` - increased work speed
- `HappySinger` - sings (morale buff to nearby duplicants)
- `DataRainer` - (DLC3 bionic)
- `RoboDancer` - (DLC3 bionic)

### Gene Shuffler Traits

Acquired from the Neural Vacillator, not at generation:
- `Regeneration` - health regeneration
- `DeeperDiversLungs` - greatly reduced oxygen consumption
- `SunnyDisposition` - stress reduction
- `RockCrusher` - bonus to digging

### Archetype-Trait Exclusions

The generation system prevents certain negative traits from appearing when a duplicant has a given interest. For example, a duplicant with Mining interest will never generate with `Anemic`, `DiggingDown`, or `Narcolepsy`. Full exclusion map in `DUPLICANTSTATS.ARCHETYPE_TRAIT_EXCLUSIONS`.

## Attribute Sources Summary

A duplicant's effective attribute value at any moment is the sum of:

1. **Base value**: 0 for all trainable attributes
2. **Interest bonus**: +7/+3/+1 per interest depending on whether the duplicant has 1/2/3 total interests
3. **Attribute level**: +1 per level gained through work experience (max +20)
4. **Skill perks**: +2 per skill tier with an attribute perk for that attribute
5. **Trait modifiers**: Positive/negative from innate traits
6. **Equipment modifiers**: From worn clothing, suits, etc.
7. **Status effect modifiers**: From buffs/debuffs (room bonuses, food quality, etc.)

## Key Source Files

| File | Contents |
|---|---|
| `Database/Attributes.cs` | All attribute definitions and their properties |
| `Database/AttributeConverters.cs` | How attributes translate to gameplay effects (speed multipliers, etc.) |
| `Database/Skills.cs` | Complete skill tree with prerequisites and perks |
| `Database/SkillPerks.cs` | All perk definitions with attribute bonuses and ability unlocks |
| `Database/SkillGroups.cs` | Skill group definitions mapping to chore groups |
| `Database/SkillAttributePerk.cs` | How skill perks apply attribute modifiers |
| `MinionResume.cs` | Skill point tracking, mastery, experience, morale updates |
| `Klei.AI/Attribute.cs` | Base attribute class with trainable/display flags |
| `Klei.AI/AttributeLevel.cs` | Per-attribute experience tracking and leveling |
| `Klei.AI/AttributeLevels.cs` | Component managing all attribute levels on a duplicant |
| `TUNING/SKILLS.cs` | Skill point tuning constants |
| `TUNING/ROLES.cs` | Attribute bonus values per skill tier |
| `TUNING/DUPLICANTSTATS.cs` | Attribute leveling constants, trait definitions, stat distributions |
