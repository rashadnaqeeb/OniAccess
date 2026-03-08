# Priorities and Errands

How duplicants decide what to do. Covers the chore system, priority classes, personal priorities, the errand selection algorithm, and interrupt behavior. Derived from decompiled source code (`Chore.cs`, `ChoreConsumer.cs`, `ChoreDriver.cs`, `ChoreType.cs`, `ChoreTypes.cs`, `ChoreGroups.cs`, `ChorePreconditions.cs`, `Prioritizable.cs`, `PrioritySetting.cs`).

## Core Concepts

A **chore** is a unit of work a duplicant can perform. Every chore has a **ChoreType** (e.g., Dig, Cook, Build) which belongs to one or more **ChoreGroups** (the columns in the priority screen). The system has three independent priority axes that combine to determine which chore a duplicant picks:

1. **Priority class** -- coarse bucket (idle, basic, personal needs, top priority, compulsory)
2. **Personal priority** -- per-duplicant, per-chore-group setting (the rows/columns in the Jobs screen)
3. **Building priority** -- per-errand value set by the player with the priority tool (1--9)

## Priority Classes (PriorityScreen.PriorityClass)

An enum that acts as the highest-order sort key. Higher numeric value always wins over everything below it.

| Value | Name | Meaning |
|-------|------|---------|
| -1 | `idle` | Idle chore, lowest possible |
| 0 | `basic` | Normal player-assigned errands (priority 1--9) |
| 1 | `high` | Legacy class, no longer used in normal gameplay |
| 2 | `personalNeeds` | Biological needs: eating, sleeping, breathing, peeing |
| 3 | `topPriority` | Yellow alert (!!) -- player-set emergency priority |
| 4 | `compulsory` | Hardcoded survival: dying, entombed, incapacitated |

A chore's priority class is stored in `masterPriority.priority_class`. The default for player-created errands is `basic` with `priority_value = 5`.

## Building Priority (masterPriority.priority_value)

The 1--9 slider the player sets on buildings and errands via the priority overlay (P key). Stored in `Prioritizable.masterPrioritySetting`. Default is 5. This is the second-highest sort key after priority class -- a priority-9 basic errand beats a priority-1 basic errand, but any `topPriority` errand beats all `basic` errands regardless of this value.

The `!!` (yellow alert) button sets `priority_class = topPriority` with `priority_value = 1`.

## Personal Priority (per-duplicant, per-chore-group)

Set in the Jobs screen (L key). Each duplicant has a priority 0--5 for each chore group:

| Value | Label | Meaning |
|-------|-------|---------|
| 0 | Disabled | Duplicant will never do errands in this group |
| 1 | Very Low | |
| 2 | Low | |
| 3 | Normal | Default for most groups |
| 4 | High | |
| 5 | Very High | |

Stored in `ChoreConsumer.choreGroupPriorities`. When a chore type belongs to multiple chore groups (e.g., BuildFetch belongs to both Build and Hauling), the duplicant's effective personal priority for that chore type is the **maximum** of their priorities across all groups it belongs to (`UpdateChoreTypePriorities` takes `Mathf.Max`).

Setting a group to 0 (disabled) also calls `SetPermittedByUser(group, false)`, which adds it to `userDisabledChoreGroups`. A chore type is permitted only if the duplicant has at least one of its groups enabled by the user AND none of its groups are disabled by traits.

### Default Personal Priorities by Chore Group

From `ChoreGroups` constructor (the `default_personal_priority` parameter):

| Priority | Groups |
|----------|--------|
| 5 (Very High) | Combat, Life Support, Toggle |
| 4 (High) | Medical Aid, Rocketry, Basekeeping |
| 3 (Normal) | Cook, Art, Research, Operate, Farming, Ranching |
| 2 (Low) | Build, Dig |
| 1 (Very Low) | Hauling, Storage, Recreation |

## Chore Groups

The columns in the Jobs screen. Each group has an associated attribute that affects work speed:

| Group | Attribute | Notable Chore Types |
|-------|-----------|-------------------|
| Combat | Digging | Attack |
| Life Support | LifeSupport | FetchCritical |
| Toggle | Toggle | Toggle |
| Medical Aid | Caring | Doctor, Compound, DoctorFetch |
| Rocketry | SpaceNavigation | RocketControl |
| Basekeeping | Strength | Mop, Repair, RepairFetch, Disinfect, CleanToilet, EmptyStorage, EmptyDesalinator, Transport |
| Cook | Cooking | Cook, CookFetch |
| Art | Art | Art, AnalyzeArtifact, ExcavateFossil |
| Research | Learning | Research, ResearchFetch, AnalyzeSeed, AnalyzeArtifact, ExcavateFossil |
| Operate | Machinery | GeneratePower, Fabricate, PowerFabricate, Depressurize, PowerTinker, MachineTinker, Train, Astronaut, LiquidCooledFan, IceCooledFan, RemoteOperate |
| Farming | Botanist | Harvest, CropTend, Uproot, FarmFetch, FlipCompost, FarmingFabricate, AnalyzeSeed, BuildUproot |
| Ranching | Ranching | Ranch, Capture, ProcessCritter, EggSing, CreatureFetch, RanchingFetch, ArmTrap |
| Build | Construction | Build, BuildDig, BuildUproot, Deconstruct, Demolish, BuildFetch |
| Dig | Digging | Dig, ExcavateFossil, BuildDig |
| Hauling | Strength | Most fetch chores (FoodFetch, EquipmentFetch, EmptyStorage, RepairFetch, etc.), Transport |
| Storage | Strength | Fetch, StorageFetch |
| Recreation | Strength | Relax (not user-prioritizable) |

Many chore types belong to multiple groups. For example, `BuildFetch` belongs to both Build and Hauling, `ResearchFetch` belongs to both Research and Hauling. This means a duplicant needs at least one of those groups enabled to perform the chore.

## Chore Type Priority (implicit_priority and explicit_priority)

Each chore type has two priority values:

- **`priority`** (implicit) -- assigned automatically by declaration order. Starts at 10000, decreases by 50 for each chore type that has `skip_implicit_priority_change = false`. This creates a fixed ranking among chore types. Earlier-declared types have higher implicit priority.
- **`explicitPriority`** -- defaults to the same as implicit priority, but can be overridden. Most player-assignable chore types set this to 5000.

Which one is used depends on the game setting `advancedPersonalPriorities`:
- **Off** (default): uses `priority` (implicit) -- chore types have a hidden built-in ranking
- **On** ("Enable Proximity"): uses `explicitPriority` -- most work chores flatten to 5000, making proximity the tiebreaker

## The Errand Selection Algorithm

The complete sort order is defined in `Chore.Precondition.Context.CompareTo()`. When a duplicant needs a new chore, `ChoreConsumer.FindNextChore()` collects all available chores from all providers, evaluates preconditions for each, sorts the successful ones, and picks the best. The sort is a cascade of tiebreakers -- the first non-zero difference wins:

### Sort Order (highest to lowest precedence)

1. **Success vs. failure** -- chores that passed all preconditions always rank above those that failed
2. **`masterPriority.priority_class`** -- the PriorityClass enum value (compulsory > topPriority > personalNeeds > basic > idle)
3. **`personalPriority`** -- the duplicant's personal priority for this chore type (0--5)
4. **`masterPriority.priority_value`** -- the building/errand priority (1--9)
5. **`priority`** -- the chore type's implicit or explicit priority
6. **`priorityMod`** -- a per-chore modifier (rarely used, defaults to 0)
7. **`consumerPriority`** -- set by `IWorkerPrioritizable` (used for recreation buildings to prefer unvisited ones)
8. **`cost`** (INVERTED -- lower cost wins) -- navigation cost from the duplicant to the chore target
9. **`chore.id`** -- creation order tiebreaker (earlier-created chores win)

This means: priority class dominates everything. Within the same class, a duplicant's personal preference for the chore group matters more than what number the player set on the building. A duplicant with Dig set to "Very High" (5) will dig a priority-1 tile before supplying a priority-9 building if their Supply is set to "High" (4).

### Proximity (cost)

Navigation cost is computed by `ChoreConsumer.GetNavigationCost()`, which calls `Navigator.GetNavigationCost(IApproachable)`. This returns the actual pathfinding cost to reach the chore target, not straight-line distance. A cost of -1 means unreachable (chore fails the `CanMoveTo` precondition).

Cost is the **7th tiebreaker** -- it only matters when two chores are identical in all six priority dimensions above it. With `advancedPersonalPriorities` off, the implicit priority ranking means chore types rarely tie, so proximity seldom matters. With it on, most work chores share `explicitPriority = 5000`, making proximity the effective tiebreaker for same-group, same-building-priority errands.

## Interrupt Priority

Determines whether a duplicant will abandon their current chore to start a new one. Separate from the selection sort order.

`ChoreType.interruptPriority` is assigned in the `ChoreTypes` constructor by grouping chore types into tiers. The first tier gets 100000, each subsequent tier gets 100 less. A new chore can interrupt the current one only if its interrupt priority is **strictly greater** than the current chore's interrupt priority AND the two chores don't share an `interruptExclusion` tag.

### Interrupt Priority Tiers (highest to lowest)

The tiers from the `ChoreTypes` constructor, with interrupt priority values:

| Interrupt Priority | Chore Types |
|-------------------|-------------|
| 100000 | Die |
| 99900 | Entombed |
| 99800 | HealCritical |
| 99700 | BeIncapacitated, GeneShuffle, Migrate |
| 99600 | BeOffline |
| 99500 | DebugGoTo |
| 99400 | StressVomit |
| 99300 | MoveTo, RocketEnterExit |
| 99200 | RecoverBreath, FindOxygenSourceItem_Critical, BionicAbsorbOxygen_Critical |
| 99100 | ReturnSuitUrgent |
| 99000 | UglyCry |
| 98900 | BingeEat, BansheeWail, StressShock |
| 98800 | WaterDamageZap |
| 98700 | ExpellGunk |
| 98600 | EmoteHighPriority, StressActingOut, Vomit, Cough, Pee, StressIdle, RescueIncapacitated, SwitchHat, RadiationPain, OilChange, SolidOilChange |
| 98500 | MoveToQuarantine |
| 98400 | TopPriority |
| 98300 | RocketControl |
| 98200 | Attack |
| 98100 | Flee |
| 98000 | LearnSkill, UnlearnSkill, Eat, ReloadElectrobank, BreakPee |
| 97900 | FindOxygenSourceItem, BionicAbsorbOxygen |
| 97800 | TakeMedicine |
| 97700 | Heal, SleepDueToDisease, RestDueToDisease, BionicRestDueToDisease |
| 97600 | Sleep, BionicBedtimeMode, Narcolepsy |
| 97500 | Doctor, GetDoctored |
| 97400 | Emote, Hug, Fart |
| 97300 | Mourn |
| 97200 | StressHeal |
| 97100 | JoyReaction |
| 97000 | Party |
| 96900 | Relax |
| 96800 | Equip, Unequip, SeekAndInstallUpgrade |
| 96700 | All regular work chores (Build, Dig, Cook, Research, Fetch, etc.) -- 72 chore types |
| 96600 | RecoverWarmth, RecoverFromHeat |
| 96500 | ReturnSuitIdle, EmoteIdle |
| 96400 | Idle |

All regular player-assigned work chores share the same interrupt tier (96700), meaning they cannot interrupt each other. A duplicant doing a Dig errand will not stop to do a Cook errand even if the Cook errand has higher priority -- they finish the current errand first. Only chores in a higher tier (personal needs, survival, stress responses) can interrupt regular work.

### Interrupt Exclusion Tags

Some chore types carry exclusion tags that prevent specific interrupts. For example, `Heal` has interrupt exclusion tags `{"Vomit", "Cough", "EmoteHighPriority"}`, meaning a healing duplicant won't be interrupted by vomiting or coughing even though those have higher interrupt priority.

### TopPriority Override

The same override applies to both sides of the interrupt comparison. When the current chore has `priority_class == topPriority`, the game uses `ChoreTypes.TopPriority.interruptPriority` (98400) as the effective interrupt threshold instead of the current chore type's own interrupt priority. Likewise, when the candidate chore has `priority_class == topPriority`, it uses 98400 as its interrupt priority instead of its chore type's own value. This means a yellow-alert work errand (normally interrupt tier 96700) effectively gets interrupt priority 98400, letting it interrupt most personal-need chores. Conversely, a yellow-alert errand being worked requires interrupt priority above 98400 to be interrupted -- only survival-level chores (breathing, incapacitation, entombing, dying) and stress responses can do so.

## Chore Lifecycle

### Selection Flow

1. `ChoreDriver` is in the `nochore` state and waits for a brain tick
2. The brain calls `ChoreConsumer.FindNextChore()` which:
   - Clears the precondition snapshot
   - Refreshes `ChoreConsumerState`
   - Collects chores from all registered `ChoreProvider`s (each provider iterates its chore list and runs preconditions)
   - Sorts succeeded contexts using `CompareTo`
   - Calls `ChooseChore()` to pick the best one
3. If a chore is found, `ChoreDriver.SetChore()` is called
4. If the duplicant already has a chore, `ChooseChore` only accepts the new chore if its interrupt priority exceeds the current chore's interrupt priority

### Preconditions

Every chore has a list of preconditions that must all pass for a duplicant to consider it. Key preconditions from `ChorePreconditions`:

- **IsValid** -- chore target exists and is in a valid world
- **IsPreemptable** -- chore is not already being done by another duplicant (or is marked preemptable)
- **IsPermitted** -- duplicant has the chore group enabled (not disabled by user or traits)
- **HasUrge** -- if the chore type has an urge (e.g., Sleep urge), the duplicant must currently have that urge
- **IsMoreSatisfyingEarly** -- quick check that this chore would beat the current one (avoids expensive pathfinding for chores that would lose anyway)
- **CanMoveTo** -- the duplicant can pathfind to the target (also computes navigation cost)
- **IsMoreSatisfyingLate** -- final priority comparison after all other checks
- **IsScheduledTime** -- the duplicant's schedule allows this type of activity (work, sleep, recreation)
- **IsNotRedAlert** -- during red alert, only topPriority chores are allowed

### Preemption

Some chores are marked `IsPreemptable = true`. When a duplicant picks a preemptable chore that already has a driver, the existing driver is told to `Fail("Preemption!")`. This is used for shared workstations where the higher-priority duplicant takes over.

## Chore Providers

Chores are registered with `ChoreProvider` instances. Each provider maintains a world-partitioned map (`choreWorldMap`) so duplicants only consider chores in their current world (asteroid). The `GlobalChoreProvider` holds world-scoped chores like fetch errands, while building-local `ChoreProvider`s hold chores tied to specific buildings.

A `ChoreConsumer` (the duplicant) has a list of providers it queries. By default this includes the duplicant's own provider (for self-directed chores like eating) and the global provider. Additional providers can be added (e.g., when a duplicant enters a rocket interior).

## Practical Implications

- **Priority class is king**: personal needs (eating, sleeping, breathing) always override work errands. Yellow alert overrides normal work but not personal needs. Compulsory chores (dying, entombed) override everything.
- **Personal priority outranks building priority**: a duplicant with Research at "Very High" and Hauling at "Normal" will research at a priority-1 station before hauling to a priority-9 building.
- **Same-tier work chores don't interrupt**: a duplicant building a ladder won't stop to dig even if a higher-priority dig errand appears. They finish the ladder first.
- **Proximity is a late tiebreaker**: it only matters when priority class, personal priority, building priority, and chore type priority are all equal. Enabling "advanced personal priorities" (Enable Proximity) flattens chore type priorities, making proximity matter more often.
- **Disabled groups are absolute**: if a duplicant has Hauling disabled (0), they will never haul, regardless of building priority or yellow alert on the errand.
- **Trait-disabled groups are also absolute**: some traits (e.g., "Unconstructive") disable chore groups at the trait level, and this cannot be overridden by the player.
