# Priorities and Errands

How duplicants decide what to do. Covers the chore system, priority classes, personal priorities, the errand selection algorithm, interrupt behavior, alert states, and re-evaluation timing. Derived from decompiled source code (`Chore.cs`, `ChoreConsumer.cs`, `ChoreDriver.cs`, `ChoreType.cs`, `ChoreTypes.cs`, `ChoreGroups.cs`, `ChorePreconditions.cs`, `Prioritizable.cs`, `PrioritySetting.cs`, `Brain.cs`, `BrainScheduler.cs`, `RedAlertMonitor.cs`, `AlertStateManager.cs`, `GlobalChoreProvider.cs`).

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

The 1--9 slider the player sets on buildings and errands via the priority overlay (P key). Stored in `Prioritizable.masterPrioritySetting`. Default is 5. This is the third sort key (after priority class and personal priority) -- a priority-9 basic errand beats a priority-1 basic errand within the same personal priority tier, but any `topPriority` errand beats all `basic` errands regardless of this value.

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

## Brain Tick Cycle and Re-evaluation

Duplicants do not continuously re-evaluate errands. The `BrainScheduler` calls `Brain.UpdateBrain()` in batches to spread CPU load across frames. Each brain tick runs `FindNextChore()`, which collects all available chores, evaluates preconditions, sorts them, and checks whether a better chore exists.

The brain tick happens even while a duplicant is mid-work. If a higher-interrupt-priority chore becomes available (e.g., a personal need triggers), the brain tick will detect it and switch via `ChoreDriver.SetChore()`. However, the `GameTags.PreventChoreInterruption` tag on a duplicant's prefab ID blocks all chore switching for the duration -- `Brain.UpdateChores()` returns immediately when this tag is present. Similarly, `GameTags.PerformingWorkRequest` defers the switch until the current work request completes.

The practical effect: errand re-evaluation is not instant. A newly created chore or priority change takes effect on the duplicant's next brain tick, not immediately. The scheduler processes minion brains in batches of up to 1000 per frame (tunable via `BrainScheduler.DupeBrainGroup.Tuning.idealProbeSize`).

## Priority Changes Mid-Work

When the player changes a building's priority via the priority tool or yellow alert button, `Prioritizable.SetMasterPriority()` fires the `onPriorityChanged` callback. Every chore linked to that `Prioritizable` subscribes to this callback via `StandardChoreBase.SetPrioritizable()`, which updates the chore's `masterPriority` field in real time (`OnMasterPriorityChanged` simply copies the new `PrioritySetting`).

However, updating the chore's `masterPriority` does not immediately interrupt the current worker or reassign the errand. The change only takes effect when:

1. **The current worker's brain ticks**: `FindNextChore()` re-evaluates all chores. If the priority change means a different chore now wins the sort, the brain will try to switch -- but only if the new chore's interrupt priority exceeds the current chore's interrupt priority. Since all regular work chores share interrupt tier 96700, raising one errand's building priority from 5 to 9 will not cause a duplicant doing a different work chore to switch. The duplicant finishes the current errand first, then picks the now-higher-priority one on the next selection cycle.

2. **Another duplicant's brain ticks**: that duplicant may now pick the re-prioritized errand if it ranks higher for them.

3. **Yellow alert toggle**: setting `!!` changes `priority_class` to `topPriority`, which does cross priority classes. A yellow-alert errand gets interrupt priority 98400 (via the `TopPriority` override), high enough to interrupt most personal-need chores. An idle or between-tasks duplicant will pick it up on their next brain tick.

When the player changes a personal priority on the Jobs screen, `ChoreConsumer.SetPersonalPriority()` updates the duplicant's `choreGroupPriorities` dictionary and recalculates `choreTypePriorities`. If the currently-active chore's group was just disabled (set to 0), `ChoreDriver.StatesInstance.OnChoreRulesChanged()` fires immediately and calls `EndChore("Permissions changed")`, forcing the duplicant to stop.

## Yellow Alert and Red Alert

### Yellow Alert

Yellow alert is a per-errand state, not a colony-wide mode. When the player clicks the `!!` button on a building, `Prioritizable.SetMasterPriority()` sets `priority_class = topPriority` with `priority_value = 1`. The `AlertStateManager` on the world tracks whether any `Prioritizable` in that world has `topPriority` class. If at least one exists, the world enters yellow alert state (visual vignette, alert sound).

Effect on errand selection:
- The errand's `priority_class` jumps from `basic` (0) to `topPriority` (3), dominating all normal errands in the sort
- Personal priority and building priority still apply within `topPriority` class (so a dupe with the relevant group at "Very High" will prefer this errand over other yellow-alert errands from groups at "Normal")
- The interrupt priority override means the errand gets effective interrupt priority 98400, which can interrupt eating (98000), sleeping (97600), relaxing (96900), and all regular work (96700). It cannot interrupt breathing (99200), entombing (99900), dying (100000), or stress responses above 98400

### Red Alert

Red alert is a colony-wide toggle per world, set via `AlertStateManager.Instance.ToggleRedAlert(true)`. Unlike yellow alert, it does not change any chore's priority class. Instead it works through preconditions:

- **`IsNotRedAlert` precondition**: added to most work chores. During red alert, this precondition fails for any chore whose `priority_class` is not `topPriority`. This means only yellow-alert errands and survival/personal-need chores (which don't carry this precondition) can be performed.
- **`IsScheduledTime` precondition**: red alert overrides schedule restrictions. During red alert, `IsScheduledTime` returns true regardless of the current schedule block. Duplicants will work through sleep and recreation time.
- **`RedAlertMonitor`**: when entering red alert, every duplicant's `RedAlertMonitor` checks if their current chore has the `IsNotRedAlert` precondition. If so, the chore is immediately stopped via `ChoreDriver.StopChore()`. The duplicant then re-evaluates on the next brain tick and can only pick chores that pass `IsNotRedAlert` (yellow-alert errands) or don't have that precondition (personal needs, survival).
- **`RedAlert` effect**: entering red alert also toggles the `RedAlert` status effect on all duplicants in that world (via `RedAlertMonitor.ToggleEffect("RedAlert")`), which applies attribute modifiers (movement speed bonus).

In practice: red alert + yellow alert is the way to get duplicants to drop everything and work on specific errands. Red alert alone just makes them idle (or attend to personal needs) since no normal errands pass `IsNotRedAlert`. The player must mark specific errands as `!!` first, then toggle red alert to force all duplicants onto those errands.

## Unreachable Errand Handling

When a duplicant cannot pathfind to a chore target, the chore is not silently ignored -- it actively fails a precondition:

- **`CanMoveTo` precondition**: calls `ChoreConsumer.GetNavigationCost(IApproachable)`, which calls `Navigator.GetNavigationCost()`. If the navigator returns -1 (no path), `GetNavigationCost` returns false, and the precondition fails. The chore goes into `failedContexts` instead of `succeededContexts`. The errand still exists and will be retried on every brain tick -- if a path becomes available (e.g., the player builds a ladder), the chore will pass on the next evaluation.

- **`CanMoveTo` also computes cost**: when the path exists, the navigation cost is added to `context.cost` (cumulative -- fetch chores may add cost for both the pickup location and the delivery destination). This cost is used as the 8th tiebreaker in sorting.

- **GlobalChoreProvider fetch deduplication**: for fetch chores specifically, `GlobalChoreProvider.UpdateFetches()` pre-filters fetches by reachability before collecting chores. It calls `Navigator.GetNavigationCost(destination)` for each fetch chore's storage destination. If the cost is -1 (unreachable), the fetch is excluded from the `fetches` list entirely and never presented to the consumer. This is a performance optimization -- without it, every duplicant would run `CanMoveTo` on every unreachable fetch every brain tick.

- **Fetch deduplication also prunes duplicates**: `GlobalChoreProvider.UpdateFetches()` sorts fetches by priority class, priority value, and cost, then removes duplicates. Two fetch chores with the same `tagsHash`, `choreType`, and `fetchCategory` are considered duplicates; only the higher-priority (or closer) one survives. This prevents duplicants from being overwhelmed by hundreds of similar fetch errands.

Unreachable errands produce a visible "unreachable" status on the building in the game's UI, but they do not generate speech events through the mod's system -- they are simply absent from the succeeded contexts list.

## Complete Chore Type List

All chore types from the `ChoreTypes` constructor, grouped by whether they have chore groups (and thus appear in the Jobs screen) or are groupless (autonomous/survival chores the player cannot prioritize per-duplicant).

### Groupless Chore Types (not in the Jobs screen)

These chores have no `ChoreGroup` assignment. The player cannot set personal priorities for them. They are triggered by urges, monitors, or game state. Listed in declaration order (which determines implicit priority -- earlier = higher):

Die, Entombed, SuitMarker, Slip, Checkpoint, TravelTubeEntrance, WashHands, HealCritical, BeIncapacitated, WaterDamageZap, BeOffline, GeneShuffle, Migrate, DebugGoTo, MoveTo, RocketEnterExit, DropUnusedInventory, FindOxygenSourceItem_Critical, BionicAbsorbOxygen_Critical, ExpellGunk, Pee, RecoverBreath, RecoverWarmth, RecoverFromHeat, Flee, MoveToQuarantine, EmoteIdle, Emote, EmoteHighPriority, StressEmote, Hug, StressVomit, UglyCry, BansheeWail, StressShock, BingeEat, StressActingOut, Vomit, Cough, RadiationPain, SwitchHat, StressIdle, RescueIncapacitated, BreakPee, Eat, ReloadElectrobank, SeekAndInstallUpgrade, OilChange, SolidOilChange, BionicAbsorbOxygen, FindOxygenSourceItem, Narcolepsy, ReturnSuitUrgent, SleepDueToDisease, BionicRestDueToDisease, Sleep, TakeMedicine, GetDoctored, RestDueToDisease, BionicBedtimeMode, ScrubOre, DeliverFood, Sigh, Heal, Shower, LearnSkill, UnlearnSkill, Equip, JoyReaction, Fart, StressHeal, Party, Recharge, Unequip, Mourn, TopPriority, MoveToSafety, ReturnSuitIdle, Idle, Relocate

### Grouped Chore Types (appear in the Jobs screen)

These chores belong to one or more ChoreGroups. The player can set personal priorities for the group(s) they belong to. Most have `explicit_priority = 5000`; the exceptions are `RocketControl` and `ArmTrap`, whose explicit priority equals their implicit priority (they pass no `explicit_priority` parameter to `Add`):

| Chore Type | Groups |
|------------|--------|
| Attack | Combat |
| Doctor | MedicalAid |
| Toggle | Toggle |
| Capture | Ranching |
| CreatureFetch | Ranching |
| RanchingFetch | Ranching, Hauling |
| EggSing | Ranching |
| Astronaut | Operate |
| FetchCritical | Hauling, LifeSupport |
| Art | Art |
| EmptyStorage | Basekeeping, Hauling |
| Mop | Basekeeping |
| Disinfect | Basekeeping |
| Repair | Basekeeping |
| RepairFetch | Basekeeping, Hauling |
| Deconstruct | Build |
| Demolish | Build |
| Research | Research |
| AnalyzeArtifact | Research, Art |
| AnalyzeSeed | Research, Farming |
| ExcavateFossil | Research, Art, Dig |
| ResearchFetch | Research, Hauling |
| GeneratePower | Operate |
| CropTend | Farming |
| PowerTinker | Operate |
| RemoteOperate | Operate |
| MachineTinker | Operate |
| MachineFetch | Operate, Hauling |
| Harvest | Farming |
| FarmFetch | Farming, Hauling |
| Uproot | Farming |
| CleanToilet | Basekeeping |
| EmptyDesalinator | Basekeeping |
| LiquidCooledFan | Operate |
| IceCooledFan | Operate |
| Train | Operate |
| ProcessCritter | Ranching |
| Cook | Cook |
| CookFetch | Cook, Hauling |
| DoctorFetch | MedicalAid, Hauling |
| Ranch | Ranching |
| PowerFetch | Operate, Hauling |
| FlipCompost | Farming |
| Depressurize | Operate |
| FarmingFabricate | Farming |
| PowerFabricate | Operate |
| Compound | MedicalAid |
| Fabricate | Operate |
| FabricateFetch | Operate, Hauling |
| FoodFetch | Hauling |
| Transport | Hauling, Basekeeping |
| Build | Build |
| BuildDig | Build, Dig |
| BuildUproot | Build, Farming |
| BuildFetch | Build, Hauling |
| Dig | Dig |
| Fetch | Storage |
| StorageFetch | Storage |
| EquipmentFetch | Hauling |
| ArmTrap | Ranching |
| RocketControl | Rocketry |
| Relax | Recreation |

### The IsMoreSatisfyingEarly Optimization

The `IsMoreSatisfyingEarly` precondition is an optimization that prevents expensive pathfinding for chores that would lose the sort anyway. It runs before `CanMoveTo` (which triggers pathfinding) and compares the candidate chore against the duplicant's current chore using the first four sort keys: priority class, personal priority, building priority, and chore type priority. If the candidate would lose on any of these, it fails immediately -- no pathfinding cost is incurred.

This precondition is skipped in two cases:
- When the building errand panel is active (`RootMenu.Instance.IsBuildingChorePanelActive()`), because the UI needs to show all errands regardless of whether they would win. In this case `IsMoreSatisfyingLate` (sortOrder 10000, runs last) handles the comparison instead.
- When the duplicant is selected, for the same reason -- the errands panel shows all available chores.

## Practical Implications

- **Priority class is king**: personal needs (eating, sleeping, breathing) always override work errands. Yellow alert overrides normal work but not personal needs. Compulsory chores (dying, entombed) override everything.
- **Personal priority outranks building priority**: a duplicant with Research at "Very High" and Hauling at "Normal" will research at a priority-1 station before hauling to a priority-9 building.
- **Same-tier work chores don't interrupt**: a duplicant building a ladder won't stop to dig even if a higher-priority dig errand appears. They finish the ladder first.
- **Proximity is a late tiebreaker**: it only matters when priority class, personal priority, building priority, and chore type priority are all equal. Enabling "advanced personal priorities" (Enable Proximity) flattens chore type priorities, making proximity matter more often.
- **Disabled groups are absolute**: if a duplicant has Hauling disabled (0), they will never haul, regardless of building priority or yellow alert on the errand.
- **Trait-disabled groups are also absolute**: some traits (e.g., "Unconstructive") disable chore groups at the trait level, and this cannot be overridden by the player.
- **Priority changes don't interrupt same-tier work**: changing a building from priority 5 to 9 updates the chore's `masterPriority` immediately, but the current worker won't switch because all work chores share interrupt tier 96700. The change takes effect after the current errand finishes.
- **Red alert without yellow alert = idle**: red alert alone blocks all normal errands via the `IsNotRedAlert` precondition. Only errands marked `!!` (yellow alert) pass the check. Without any `!!` errands, duplicants will only do personal-need chores or idle.
- **Red alert overrides schedules**: during red alert, `IsScheduledTime` always returns true. Duplicants ignore sleep and recreation blocks.
- **Unreachable errands are retried every brain tick**: an unreachable chore fails the `CanMoveTo` precondition and is skipped, but it remains registered and is re-evaluated next tick. Building a path to the target will allow the chore to be picked up automatically.
- **Fetch deduplication reduces pathfinding load**: `GlobalChoreProvider` prunes duplicate fetch chores (same tags, same type, same storage category) before presenting them to duplicants. Only the highest-priority, closest-destination instance of each unique fetch survives. This is why hundreds of storage errands don't cause lag.
- **Brain ticks are batched, not instant**: errand re-evaluation happens in batches spread across frames. A priority change or new errand may take several frames before any duplicant responds to it.
