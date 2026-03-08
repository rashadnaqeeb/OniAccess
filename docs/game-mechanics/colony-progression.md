# Colony Progression

Printing pod, immigration, duplicant generation, care packages, skill scrubbing, and colony achievements -- systems that advance the colony over time. For permissions, access control, reports, diagnostics, and notifications, see `colony-controls.md`. Derived from decompiled source code.

## Cycle Timing

A cycle is 600 real-time seconds (10 minutes) at 1x speed. Defined in `Constants.SECONDS_PER_CYCLE`.

Daytime occupies the first 87.5% of the cycle (525s). Nighttime is the remaining 12.5% (75s). The night boundary is checked in `GameClock.IsNighttime()` against `GetCurrentCycleAsPercentage() >= 0.875f`.

The game clock ticks at 33ms intervals (`ISim33ms`). Time accumulates continuously; when `timeSinceStartOfCycle >= 600f`, the cycle counter increments and the new-day event fires (`GameHashes.NewDay`, hash 631075836). Autosave triggers at the cycle boundary if enabled.

Schedules divide cycles into 24 blocks of 25s each (600 / 24).

## Printing Pod (Headquarters)

The Printing Pod is the colony's starting building (`HeadquartersConfig`, ID `"Headquarters"`). It is a 4x4 non-deconstructable structure that serves three functions:
- Immigration portal (spawning new duplicants/care packages)
- Skill assignment station (`RoleStation`)
- Social gathering point (up to 4 duplicants)

New duplicants delivered via the printing pod receive 1 skill point (`startingSkillPoints = 1f` in `HeadquartersConfig`).

### Immigration Timer

The `Immigration` class manages the spawn cycle. It implements `ISim200ms` and counts down `timeBeforeSpawn` in 200ms steps. The timer is halted when no operational Telepad exists on the map (`IsHalted()` checks `Components.Telepads`).

The `spawnInterval` and `spawnTable` arrays are serialized on the prefab (not in code constants). The `spawnIdx` advances each time immigration ends (accept or reject). The wait time is `spawnInterval[min(spawnIdx, spawnInterval.Length - 1)]`, so it clamps at the last entry for all subsequent deliveries.

When `timeBeforeSpawn` reaches 0, `bImmigrantAvailable` is set true. The Telepad's `Update()` detects this and opens the portal animation. If the player does not interact within 120 seconds (`MAX_IMMIGRATION_TIME = 120f`), a `DuplicantsLeftMessage` is queued and the immigration is ended automatically.

### Selection Screen

`ImmigrantScreen` extends `CharacterSelectionController`. When opened, it calls `InitializeContainers()` which creates the choice slots:

- **Without care packages** (starter or care packages disabled): 3 duplicant slots, 0 care package slots
- **With care packages enabled**: always 4 total slots. 70% chance of 1 care package + 3 duplicants; 30% chance of 2 care packages + 2 duplicants

Care package slots are randomly inserted among the duplicant slots (`SetSiblingIndex(Random.Range(0, childCount))`).

The player selects exactly 1 deliverable (dupe or package). The ImmigrantScreen disables reshuffling on all containers -- reshuffling is only available during initial colony setup (`MinionSelectScreen`).

Rejecting all candidates calls `Telepad.RejectAll()`, which runs `Immigration.EndImmigration()` and restarts the timer.

## Duplicant Generation

`MinionStartingStats` generates a new duplicant's traits, aptitudes, and starting attributes. The system differs between starter minions (colony setup via `MinionSelectScreen`) and pod-printed minions (immigration via `ImmigrantScreen`).

Source classes: `MinionStartingStats`, `DUPLICANTSTATS`, `CharacterSelectionController`, `CharacterContainer`.

### Aptitudes

Each duplicant receives 1-3 aptitude groups (skill groups). The count is `Random.Range(1, 4)`. If a guaranteed aptitude is specified (e.g., for archetypes), it is assigned first and counts toward the total. Aptitude bonus is `APTITUDE_BONUS = 1` per group.

Aptitude attribute bonuses depend on how many aptitudes the duplicant has (`APTITUDE_ATTRIBUTE_BONUSES = [7, 3, 1]`): a duplicant with 1 aptitude gets +7 to that group's attributes, with 2 aptitudes gets +3 each, with 3 aptitudes gets +1 each.

Bionic duplicants do not receive aptitudes.

### Trait Configuration

Starter minions always get exactly 1 positive trait and 1 negative trait.

Pod-printed minions draw from `POD_TRAIT_CONFIGURATIONS_DECK`, a 20-entry deck that is shuffled and consumed one entry at a time (refilled when empty):

| Configuration | Count in deck |
|---|---|
| 1 positive, 1 negative | 6 |
| 2 positive, 1 negative | 6 |
| 1 positive, 2 negative | 4 |
| 2 positive, 2 negative | 2 |
| 3 positive, 1 negative | 1 |
| 1 positive, 3 negative | 1 |

Maximum total traits per duplicant: `MAX_TRAITS = 4` (including congenital traits).

### Trait Rarity

Each trait has a rarity from 1 (Common) to 5 (Legendary). Negative traits draw rarity from `RARITY_DECK`, a 20-entry deck:
- Common (1): 7 entries
- Uncommon (2): 6 entries
- Rare (3): 4 entries
- Epic (4): 2 entries
- Legendary (5): 1 entry

Positive trait rarity is balanced against `rarityBalance`, which tracks net rarity (negative traits add, positive traits subtract). Starter minions draw from a tighter range around the balance point.

### Stat Point Bonuses

Negative traits grant stat point bonuses that add attribute points to the duplicant's aptitude groups. Some positive traits (GrantSkill variants) apply a negative stat bonus of `-LARGE_STATPOINT_BONUS` (-4), representing the tradeoff for starting with a free skill.

| Bonus Level | Points |
|---|---|
| NO_STATPOINT_BONUS | 0 |
| TINY_STATPOINT_BONUS | 1 |
| SMALL_STATPOINT_BONUS | 2 |
| MEDIUM_STATPOINT_BONUS | 3 |
| LARGE_STATPOINT_BONUS | 4 |
| HUGE_STATPOINT_BONUS | 5 |

### Trait Exclusions

Traits have multiple exclusion mechanisms:
- `mutuallyExclusiveTraits` -- cannot coexist with listed traits
- `mutuallyExclusiveAptitudes` -- cannot appear if the duplicant has the listed aptitude group
- `ARCHETYPE_TRAIT_EXCLUSIONS` -- per-archetype trait blacklists used for starter minions with guaranteed aptitudes

### Stress and Joy Traits

Every duplicant gets one stress response trait (from `STRESSTRAITS`: Aggressive, Stress Vomiter, Ugly Crier, Binge Eater, Banshee) and one joy response trait (from `JOYTRAITS`: Balloon Artist, Sparkle Streaker, Sticker Bomber, Super Productive, Happy Singer, plus Data Rainer and Robo Dancer for bionics). These are assigned via `Personality` data, not randomly generated per roll.

### Congenital Traits

Some personalities have a congenital trait (Joshua, Ellie, Stinky, Liam). These are applied before random trait generation and count toward trait limits. They can be positive or negative.

### Bionic Duplicants (DLC3)

Bionic duplicants use a separate trait system:
- One random bug trait from `BIONICBUGTRAITS` (7 variants, all negative)
- One upgrade trait from `BIONICUPGRADETRAITS`, selected to be compatible with the guaranteed aptitude via `ARCHETYPE_BIONIC_TRAIT_COMPATIBILITY`
- No standard positive/negative trait generation
- No aptitudes (skip `GenerateAptitudes`)

### Colony Setup (MinionSelectScreen)

The initial colony setup screen (`MinionSelectScreen`) differs from immigration:
- `IsStarterMinion = true`, which forces 1 positive + 1 negative trait configuration
- All 3 containers allow reshuffling
- The proceed button selects all 3 duplicants (not just 1)
- Ceres clusters pre-assign Freyja as the third starter; Prehistoric clusters pre-assign Maya and Higby
- Care packages are never offered during colony setup

## Care Packages

`CarePackageInfo` defines a deliverable resource bundle with three fields:
- `id` -- tag/prefab ID of the item
- `quantity` -- amount delivered (mass in kg for elements, count for items/critters)
- `requirement` -- nullable `Func<bool>` that must return true for the package to appear in the pool

### Availability Conditions

Packages use two condition helpers:
- `CycleCondition(int cycle)` -- true when `GameClock.Instance.GetCycle() >= cycle`
- `DiscoveredCondition(Tag tag)` -- true when `DiscoveredResources.Instance.IsDiscovered(tag)`

In Spaced Out (cluster space), most critter and plant packages additionally require `DiscoveredCondition` for the species, with a `CycleCondition(500)` fallback that unlocks everything after cycle 500 regardless. Base game packages are simpler, gated only by cycle thresholds.

### Selection

`Immigration.RandomCarePackage()` filters the full list to packages whose `requirement` is null or returns true, then picks uniformly at random.

### Cycle Thresholds

Constants defined in `Immigration`:
| Constant | Cycle |
|---|---|
| CYCLE_THRESHOLD_A | 6 |
| CYCLE_THRESHOLD_B | 12 |
| CYCLE_THRESHOLD_C | 24 |
| CYCLE_THRESHOLD_D | 48 |
| CYCLE_THRESHOLD_E | 100 |
| CYCLE_THRESHOLD_UNLOCK_EVERYTHING | 500 |

### Base Game Care Package Pool

Available from cycle 0 (no condition):
- Sandstone 1000 kg, Dirt 500 kg, Algae 500 kg, Oxylite 100 kg, Water 2000 kg, Sand 3000 kg, Carbon 3000 kg, Fertilizer 3000 kg, Brine 2000 kg, Salt Water 2000 kg, Rust 1000 kg
- Seeds: Bristle Blossom x3, Mealwood x3, Jumping Joya x3, Mushroom x1, Bristle Berry x2, Oxyfern x1, Arbor Tree x1
- Food: Field Ration x5, Muckroot x6
- Critters: Shine Bug x1, Hatchling x1, Puft x1, Pip x1, Pokeshell x1
- Eggs: Shine Bug x3, Hatch x3, Puft x3, Pip x2
- Medicine: Vitamin Chews x3
- Clothing: Custom Clothing x1 (random facade), Funky Vest x1

Cycle 6+: Omelette x3

Cycle 12+: Ice 4000 kg, Cuprite 2000 kg (if discovered), Gold Amalgam 2000 kg (if discovered), Bristle Berry x3, Oilfloater Egg x3

Cycle 24+: Copper 400 kg (if discovered), Iron 400 kg (if discovered), Reed Fiber Plant seed x3, Balm Lily x1, Wheezewort x1, Pincha Pepper x1, Waterweed x1, Mealwood x1, Drecko x1, Pacu x8, Fried Mushroom x3, Mole Egg x3, Drecko Egg x3

Cycle 48+: Lime 150 kg (if discovered), Plastic 500 kg (if discovered), Glass 200 kg (if discovered), Steel 100 kg (if discovered), Ethanol 100 kg (if discovered), Aluminum Ore 100 kg (if discovered), Shove Vole x1, Oilfloater x1, BBQ x3, Spicy Tofu x3

### Spaced Out Care Package Pool

The Spaced Out pool (`ConfigureMultiWorldCarePackages`) has the same element packages as the base game, but differs in critter/plant availability. Most entries require `DiscoveredCondition` for the species OR `CycleCondition(500)` as a universal fallback. Notable additions over the base game:

Cycle 0 (no condition): Wine Cups seed x3, Cylindrica seed x3. Cycle 0 (discovered only, no cycle 500 fallback): Muckroot x6, Forest Forage Plant x2, Swamp Forage Plant x2

Cycle 24+: Grubfruit Plant seed x1 (if discovered or cycle 500)

Cycle 48+: Divergent Beetle x1 (if discovered or cycle 500), Staterpillar x1 (if discovered or cycle 500), Divergent Beetle Egg x2 (if discovered or cycle 500), Staterpillar Egg x2 (if discovered or cycle 500)

All critter/egg entries in Spaced Out require discovery of the egg type or cycle 500 fallback, unlike base game which has no discovery gate on early critters.

### DLC-Specific Care Packages

DLC packages are appended to whichever base pool (base game or Spaced Out) is active.

**DLC2 (Frostburger):**
- Cinnabar 2000 kg (cycle 12+, if discovered), Wood Log 200 kg (cycle 24+, if discovered)
- Critters: Wood Deer (cycle 24+), Seal (cycle 48+), Ice Belly Egg (cycle 100+)
- Food: Pemmican x3 (always), Carrot Fries x3 (cycle 24+)
- Seeds: Ice Flower x3, Blue Grass x1, Carrot Plant x1 (cycle 24+), Space Tree x1 (cycle 24+), Hard Skin Berry Plant x3

**DLC3 (Bionic):**
- Disposable Electrobank x3 (cycle 12+)
- Bionic upgrade components x1 each (requires at least one living bionic duplicant)

**DLC4 (Prehistoric):**
- Peat 3000 kg (always), Nickel Ore 2000 kg (cycle 12+, if discovered)
- Seeds: Garden Food Plant x1, Garden Decor Plant x1, Butterfly Plant x1, Dinofern x1 (cycle 48+), Dew Dripper Plant x1 (cycle 48+), Kelp Plant x1 (cycle 48+), Fly Trap Plant x1 (cycle 48+), Vine Mother x1 (cycle 48+)
- Food: Garden Forage Plant x3 (always), Vine Fruit x6 (always), Smoked Dinosaur Meat x1 (cycle 48+)
- Critters: Stego x1 (always), Chameleon Egg x1 (cycle 48+), Mosquito Egg x3 (cycle 48+), Prehistoric Pacu Egg x1 (cycle 100+), Raptor Egg x1 (cycle 100+)

### Delivery Effects

When a duplicant is delivered via the printing pod (`Telepad.OnAcceptDelivery`):
1. The duplicant spawns at the pod's cell with a portal birth animation
2. All living duplicants on the same world receive the `NewCrewArrival` effect
3. The delivered duplicant receives skill points equal to `startingSkillPoints` (1 for Headquarters, configurable per telepad type)
4. Default personal priorities from `Immigration` are applied
5. Bionic duplicants trigger a bonus electrobank delivery 5 seconds later (5 disposable electrobanks + 1 lubrication stick)

### Morale Expectations from Skills

Morale expectation increases with learned skills. See `stress-and-morale.md` for the full morale system. The key formula from `MinionResume.UpdateExpectations()`:

Each non-granted mastered skill adds `tier + 1` to `QualityOfLifeExpectation`. Skill tiers range from 0 to 6, so a single tier-0 skill adds +1 expectation and a tier-6 skill adds +7.

Skills in a duplicant's aptitude group grant +1 morale (not expectation) per skill, via `MinionResume.UpdateMorale()`. This partially offsets the expectation increase for in-aptitude skills.

## Skill Scrubber (Reset Skills Station)

Building ID: `"ResetSkillsStation"`. A 3x3 building requiring 480W power.

### Operation

The station is assignable -- a duplicant must be assigned to it. Once assigned, a `WorkChore<ResetSkillsStation>` is created at `PriorityClass.high` priority. The work takes 180 seconds (3 minutes real-time at 1x speed).

On completion (`OnCompleteWork`):
1. `MinionResume.ResetSkillLevels()` is called, which iterates all mastered skills and calls `UnmasterSkill()` on each. Skill points are returned (the `returnSkillPoints` parameter defaults to true).
2. The duplicant's hat is cleared
3. The assignment is removed
4. A "Reset Skill" notification is shown

There is no material cost beyond electricity. The duplicant retains all experience and attribute bonuses from traits/aptitudes -- only learned skills are removed and their skill points refunded.

## Colony Achievements

`ColonyAchievementTracker` manages achievement progress. It is a singleton component that checks achievements incrementally -- one achievement per frame via `RenderEveryTick()`, plus a full sweep on game events (hash 395452326). Achievements that require sandbox/debug mode disabled will not unlock if those modes were used.

### Victory Conditions

Victory achievements trigger a special sequence: pause the game, display a story message, then run a scripted victory animation.

| Achievement | Requirements |
|---|---|
| **Thriving** (Home Sweet Home) | Survive 200 cycles, minimum morale met, 12+ living dupes, Great Monument built |
| **The Great Escape** (base game) | Reach the Temporal Tear space destination |
| **The Great Escape** (Spaced Out) | Establish colonies, open Temporal Tear, send craft through it |
| **Cosmic Archaeology** (Spaced Out) | Collect all artifacts + space artifacts |
| **Geothermal Imperative** (DLC2) | Discover facility, repair controller, use plant, clear blocked vent |
| **Demolior Imperative** (DLC4) | Defeat the prehistoric asteroid |

### Non-Victory Achievements

Selected examples with their code-level requirements:

- **Turn of the Century**: Survive 100 cycles (`CycleNumber()`)
- **One Small Step**: Reach space (launch a rocket)
- **Super Sustainable**: Generate 240,000 kJ without coal/gas/petroleum/wood/peat generators
- **Locavore**: Eat 400,000 kcal with no farm tiles
- **Carnivore**: Eat 400,000 kcal of meat before cycle 100
- **No Place Like Clone**: Have 20+ living duplicants
- **Job Suitability**: All dupes complete chores in exosuits for 10 cycles
- **Animal Friends**: Tame one of each base critter (Drecko, Hatch, Shine Bug, Shove Vole, Oilfloater, Pacu, Puft, Gassy Moo, Pokeshell, Pip)
- **Not 0K**: Cool a building to 6 Kelvin
- **Down the Hatch**: Smooth Hatches produce 10,000 kg of refined metal
- **Some Reservations**: Build 4 Nature Reserves
- **Get a Room**: Build one of each room type (Nature Reserve, Hospital, Rec Room, Great Hall, Bedroom, Plumbed Bathroom, Farm, Stable)
- **One Year**: Survive 365.25 cycles
- **Life Found a Way** (DLC4): Survive 100 cycles after asteroid impact with no duplicant deaths

### Achievement Display

The `TelepadSideScreen` (printing pod side screen) displays victory condition progress with checkboxes. When any achievement completes, `completedAchievementsToDisplay` is populated and a notification badge appears on the colony summary button. Victory achievements trigger `BeginVictorySequence()` which pauses the game, hides most UI elements, and plays the victory cutscene.
