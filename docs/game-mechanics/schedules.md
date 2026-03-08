# Schedules

How the schedule system controls duplicant behavior across the cycle. Derived from decompiled source code (`Schedule.cs`, `ScheduleManager.cs`, `ScheduleBlock.cs`, `ScheduleGroup.cs`, `ScheduleBlockType.cs`, `Database/ScheduleGroups.cs`, `Database/ScheduleBlockTypes.cs`, `RecreationTimeMonitor.cs`, `StaminaMonitor.cs`, `SleepChore.cs`, `SleepChoreMonitor.cs`, `Schedulable.cs`, `ChoreConsumer.cs`, `ChoreDriver.cs`, `ChoreType.cs`, `ChorePreconditions.cs`, `WorkChore.cs`, `CalorieMonitor.cs`, `BladderMonitor.cs`, `UrgeMonitor.cs`, `Database/ChoreTypes.cs`).

## Cycle Structure

A cycle is 600 seconds. The schedule divides each cycle into **24 blocks of 25 seconds each**. `ScheduleManager` implements `ISim33ms` and checks the current hour every 33ms tick. When the hour changes, it calls `Schedule.Tick()` on every schedule, which notifies all assigned `Schedulable` components.

The current block index is computed as:
```
blockIdx = min(floor(cyclePercentage * 24), 23) + (progressTimetableIdx * 24)
```

## Block Types (ScheduleBlockType)

Five atomic activity types exist in `Database.ScheduleBlockTypes`:

| Id | Display Name | Purpose |
|---|---|---|
| `Sleep` | Sleep | Resting in bed |
| `Eat` | Mealtime | Eating at mess tables |
| `Work` | Work | Performing colony errands |
| `Hygiene` | Hygiene | Bathroom, shower, handwashing |
| `Recreation` | Recreation | Leisure activities for morale |

These are the primitive types. Schedule groups compose them into the four user-facing block categories.

## Schedule Groups (User-Facing Block Categories)

`Database.ScheduleGroups` defines four groups, each allowing a set of block types:

### Bathtime (`Hygene`)
- **Allowed types:** Hygiene, Work
- **Default segments:** 1
- **Behavior:** Duplicants prioritize hygiene tasks (bathroom when bladder > 40%, shower, handwashing). When hygiene needs are met, they fall back to work errands.
- **Note:** The internal ID is `"Hygene"` (typo preserved from game source).

### Work (`Worktime`)
- **Allowed types:** Work
- **Default segments:** 18
- **Behavior:** Duplicants perform queued errands. This is the only group that triggers the shift-change alarm by default (`alarm: true`).

### Downtime (`Recreation`)
- **Allowed types:** Hygiene, Eat, Recreation, Work
- **Default segments:** 2
- **Behavior:** The most permissive group. Duplicants may eat, use the bathroom, engage in recreation activities, or work. All four activity types are allowed, so duplicants choose based on their current needs and urges.

### Bedtime (`Sleep`)
- **Allowed types:** Sleep
- **Default segments:** 3
- **Behavior:** Duplicants sleep. Only the Sleep activity is permitted. Emergencies override this (see Sleep Mechanics below).

### Default Layout

The default schedule totals exactly 24 blocks, asserted at initialization:
```
Bathtime(1) + Work(18) + Downtime(2) + Bedtime(3) = 24
```

Blocks are laid out in that order: Bathtime starts at dawn (block 0), then 18 Work blocks, 2 Downtime blocks, and 3 Bedtime blocks at the end of the cycle.

## Multiple Schedules

`ScheduleManager` maintains a list of `Schedule` objects. Players can:

- **Add schedules** via the schedule screen. New schedules use the default group layout.
- **Duplicate schedules** which copies all block assignments and alarm state.
- **Delete schedules** (minimum 1 must remain). Assigned duplicants are moved to the first schedule.
- **Rename schedules** via an editable title bar.

Each schedule has an independent alarm toggle (`alarmActivated`). When enabled, a notification and chime sound play on shift changes.

### Duplicant Assignment

Every living duplicant (`MinionIdentity`) has a `Schedulable` component. On spawn, `ScheduleManager.OnAddDupe` assigns unassigned duplicants to `schedules[0]`. A duplicant can be reassigned to any schedule via the schedule screen's minion widgets.

Each `Schedule` maintains a `List<Ref<Schedulable>>` of its assigned duplicants. Assignment is exclusive: a duplicant belongs to exactly one schedule.

### Bionic Schedules

When a Bionic duplicant spawns, the game auto-creates a "Default Bionic Schedule" if one doesn't exist. This schedule has 3 timetable rows (72 blocks total) and is configured with all Work blocks except the last 12: 6 Recreation blocks followed by 6 Sleep blocks. Once deleted, the default bionic schedule won't be recreated (`hasDeletedDefaultBionicSchedule` flag).

## Multi-Day Timetables

Schedules support multiple 24-block rows (timetables). Each row represents one cycle's worth of blocks. The `progressTimetableIdx` tracks which row is active and advances at block 0 of each cycle, wrapping around when it exceeds the row count.

Players can:
- **Duplicate a timetable row** (inserts a copy below)
- **Delete a timetable row** (minimum 1 row)
- **Shift rows up/down** to reorder
- **Rotate blocks left/right** within a row (circular shift)

This enables multi-day repeating schedules (e.g., alternate heavy-work and rest days).

## How Schedules Control Chore Selection

The schedule system does not directly assign tasks. Instead, it acts as a **precondition filter** on the chore system. Every tick, the brain (`ChoreConsumer.FindNextChore()`) collects all available chores, evaluates their preconditions, sorts the successes by priority, and picks the best one. The schedule determines which chores pass precondition checks.

### The IsScheduledTime Precondition

The core mechanism is `ChorePreconditions.IsScheduledTime`. Each chore declares which `ScheduleBlockType` it requires. At evaluation time, the precondition checks whether the duplicant's current schedule block allows that type:

```csharp
// ChorePreconditions.IsScheduledTime
fn = delegate(ref Context context, object data) {
    if (context.chore.gameObject.GetMyWorld().IsRedAlert())
        return true;  // Red Alert bypasses schedule
    ScheduleBlockType type = (ScheduleBlockType)data;
    return context.consumerState.scheduleBlock?.IsAllowed(type) ?? true;
};
```

The `consumerState.scheduleBlock` is refreshed from `schedulable.GetSchedule().GetCurrentScheduleBlock()` each time `ChoreConsumer.FindNextChore()` runs. If the schedule block is null (edge case), the precondition passes by default.

### How WorkChore Applies Schedule Filtering

`WorkChore<T>` is the most common chore type for buildings. Its constructor determines schedule filtering via two parameters:

1. **`schedule_block`** (explicit): If set, the chore requires that specific block type. Recreation buildings pass `ScheduleBlockTypes.Recreation`, making them only available during blocks that include the Recreation type.
2. **`ignore_schedule_block`** (default false): If true, no schedule precondition is added at all.

If neither parameter is set (the default), the chore falls back to requiring `ScheduleBlockTypes.Work`:

```csharp
if (schedule_block != null)
    AddPrecondition(IsScheduledTime, schedule_block);
else if (!ignore_schedule_block)
    AddPrecondition(IsScheduledTime, Db.Get().ScheduleBlockTypes.Work);
```

This means most colony errands (building, digging, operating machines, hauling) are gated on the Work block type. Recreation chores (Arcade Machine, Beach Chair, Espresso Machine) are gated on the Recreation block type.

### Chores Without Schedule Gating

Many chore types have **no `IsScheduledTime` precondition** and run regardless of the current schedule block. These are "survival" or "autonomous" chores that use the urge system instead. They appear in `Database.ChoreTypes` with empty chore group arrays (`new string[0]`). Examples:

- **Die, Entombed, BeIncapacitated** - emergency states
- **Pee, ExpellGunk** - critical biological needs
- **Vomit, Cough, RecoverBreath** - involuntary reactions
- **Sleep, Narcolepsy** - controlled by `StaminaMonitor`/`UrgeMonitor` rather than schedule preconditions
- **Eat** - controlled by `CalorieMonitor` urge system
- **Flee, MoveToSafety** - danger response
- **StressVomit, UglyCry, BansheeWail** - stress reactions
- **MoveTo, DebugGoTo** - player-directed movement

These chores bypass the schedule entirely. The schedule only affects them indirectly through the urge threshold system (see below).

### Schedule Block Types and What They Gate

| Block Type | Chores Gated | Notes |
|---|---|---|
| `Work` | All `WorkChore<T>` instances without explicit schedule_block: building, digging, operating, hauling, cooking, researching, farming, ranching, etc. | Default for any WorkChore that doesn't specify otherwise |
| `Recreation` | `WorkChore<T>` instances for recreation buildings (Arcade Machine, Beach Chair, Espresso Machine, Water Cooler, etc.) | All pass `schedule_block: ScheduleBlockTypes.Recreation` |
| `Hygiene` | Not used as a chore precondition directly | Affects urge thresholds (bladder) rather than gating chores |
| `Eat` | Not used as a chore precondition directly | Affects urge thresholds (calories) rather than gating chores |
| `Sleep` | Not used as a chore precondition directly | Affects urge thresholds (stamina) rather than gating chores |

### Which Block Groups Allow Which Chores

Combining the above, here is what each schedule group enables:

| Schedule Group | Allowed Types | Work Chores? | Recreation Chores? | Effect on Urges |
|---|---|---|---|---|
| Bathtime | Hygiene, Work | Yes | No | Bladder urge triggers at 40% |
| Work | Work | Yes | No | Bladder urge only at 100% |
| Downtime | Hygiene, Eat, Recreation, Work | Yes | Yes | Bladder at 40%, eat urge active, sleep urge at stamina threshold |
| Bedtime | Sleep | No | No | Sleep urge fully active |

## The Urge System and Schedule Thresholds

Schedules influence duplicant behavior through **urge thresholds** that vary based on the current block type. The urge system determines when a duplicant "wants" to do something strongly enough to seek out the corresponding chore.

### UrgeMonitor Dual Thresholds

`UrgeMonitor` is a state machine that toggles an urge on/off based on an amount value. It has two thresholds:

- **`inScheduleThreshold`**: Used when the current schedule block allows the associated block type
- **`outOfScheduleThreshold`**: Used when it does not

```csharp
private float GetThreshold() {
    if (schedulable.IsAllowed(scheduleBlock))
        return inScheduleThreshold;
    return outOfScheduleThreshold;
}
```

For sleep, the `UrgeMonitor` is configured with `inScheduleThreshold=100` and `outOfScheduleThreshold=0`, `isThresholdMinimum=false`. Since the check is `value <= threshold`:
- **During Sleep blocks**: urge triggers when stamina <= 100 (always true), so the duplicant always wants to sleep
- **Outside Sleep blocks**: urge triggers when stamina <= 0 (only when exhausted)

### Eating and Schedule

`CalorieMonitor` uses a different pattern. It has two sub-states within `hungry`:

- **`hungry.normal`**: Active when `IsEatTime()` returns true (current block allows `ScheduleBlockTypes.Eat`). Toggles the Eat urge, causing the duplicant to seek food.
- **`hungry.working`**: Active when `IsEatTime()` returns false. The Eat urge is NOT toggled. The duplicant stays hungry but works instead.

Transitions between these states occur on `GameHashes.ScheduleBlocksChanged`. The practical effect: duplicants only eat during Downtime blocks (the only group that includes the Eat type). They will not eat during Work or Bathtime even if hungry. They will eat during Downtime even if not starving, as long as calories are below the hungry threshold (`DUPLICANTSTATS.STANDARD.BaseStats.HUNGRY_THRESHOLD`).

Exception: starving duplicants (`hungry.starving`) always have the Eat urge active regardless of schedule block.

### Bladder and Schedule

`BladderMonitor` has two pee states:

- **`urgentwant`**: Bladder at 100% (full). Always triggers the Pee urge regardless of schedule. The duplicant will interrupt any activity.
- **`breakwant`**: Bladder >= 40% AND `IsPeeTime()` returns true (block allows `ScheduleBlockTypes.Hygiene`). The duplicant seeks a toilet during Bathtime or Downtime blocks, even if not yet urgent.

When the block changes away from a Hygiene-allowing block, `breakwant` transitions back to `satisfied` (via `ScheduleBlocksChanged` event) unless bladder has reached 100%.

## Block Transitions

### What Happens When the Hour Changes

`ScheduleManager.Sim33ms()` compares `GetCurrentHour()` to `lastHour` every 33ms. When they differ, it calls `Schedule.Tick()` on every schedule. `Schedule.Tick()` does the following:

1. **Timetable row advancement**: If the current block index is at position 0 (start of cycle), `progressTimetableIdx` increments and wraps around the number of timetable rows.

2. **Block change detection**: Compares the allowed types of the current block against the previous block using `AreScheduleTypesIdentical()`. This means consecutive blocks of the same group type (e.g., two adjacent Work blocks) do NOT trigger a change notification.

3. **Alarm**: If the alarm is enabled and the `alarm` flag differs between the old and new group, `PlayScheduleAlarm()` fires a notification and chime. Only the Work group has `alarm: true` by default, so the alarm plays on transitions into or out of Work blocks.

4. **Block change notification**: For each assigned `Schedulable`, calls `OnScheduleBlocksChanged()` which triggers `GameHashes.ScheduleBlocksChanged` (-894023145). This event is what `CalorieMonitor`, `BladderMonitor`, and other monitors listen for to re-evaluate urge thresholds.

5. **Tick notification**: Regardless of whether the block type changed, calls `OnScheduleBlocksTick()` on all assigned schedulables, triggering `GameHashes.ScheduleBlocksTick` (1714332666).

### Transition Does Not Interrupt Running Chores Directly

The block transition itself does not abort the current chore. It fires events that monitors listen to, which may toggle urges. The `ChoreConsumer` brain then re-evaluates on its next tick and picks a higher-priority chore if one is now available. The old chore gets interrupted only if the new one has higher interrupt priority.

## Chore Interruptions

### The Interrupt Priority System

Every `ChoreType` has an `interruptPriority` value assigned in `Database.ChoreTypes`. These are assigned in descending order, starting at 100000 and decreasing by 100 per tier. A chore can only interrupt the current chore if its interrupt priority is strictly greater.

The interrupt check happens in `ChoreConsumer.ChooseChore()`:

```csharp
// Simplified from ChooseChore()
int currentInterrupt = currentChore.choreType.interruptPriority;
// For TopPriority class, use TopPriority's interrupt value instead
for (each succeeded context, highest priority first) {
    if (context.interruptPriority > currentInterrupt
        && !currentChore.choreType.interruptExclusion.Overlaps(context.chore.choreType.tags))
        return context;  // this chore wins
}
```

### Interrupt Priority Tiers (abridged)

The tiers from highest to lowest interrupt priority, showing the chores most relevant to schedule interactions:

| Tier | Interrupt Priority | Chores |
|---|---|---|
| 1 | 100000 | Die |
| 2 | 99900 | Entombed |
| 3 | 99800 | HealCritical |
| 4 | 99700 | BeIncapacitated, GeneShuffle, Migrate |
| ... | ... | ... |
| 14 | 98700 | ExpellGunk |
| 15 | 98600 | EmoteHighPriority, Pee, StressIdle, etc. |
| 17 | 98400 | TopPriority (player !! priority) |
| 21 | 98000 | LearnSkill, Eat, BreakPee, ReloadElectrobank |
| 25 | 97600 | Sleep, BionicBedtimeMode, Narcolepsy |
| 32 | 96900 | Relax (recreation) |
| 34 | 96700 | All work chores (Build, Dig, Cook, Research, Haul, etc.) |
| 37 | 96400 | Idle |

Key takeaway: Eating (tier 21, priority 98000) interrupts Sleep (tier 25, priority 97600), and Sleep interrupts Relax (tier 32, priority 96900). Work chores (tier 34) cannot interrupt Sleep, Eat, or Recreation chores. Pee (tier 15, priority 98600) interrupts almost everything except emergencies.

### Interrupt Exclusions

`ChoreType` has an `interruptExclusion` tag set. A chore cannot interrupt the current chore if their exclusion tags overlap. For example, `Relax` has exclusion tag `"Sleep"`, meaning recreation chores won't interrupt sleep even though they technically have a lower interrupt priority (this is redundant since Sleep already has higher priority, but serves as a safety net).

### When Do Duplicants Break From Their Schedule Block?

Duplicants do not strictly follow their schedule block. They follow it to the extent that the precondition system and urge thresholds allow. Situations where a duplicant breaks from the expected behavior:

1. **Survival chores override everything**: Pee (bladder full), RecoverBreath (suffocating), Flee (danger), Vomit, and other survival chores have no schedule precondition and very high interrupt priority. A duplicant in Bedtime will get up to pee if bladder hits 100%.

2. **Sleep extends past Bedtime**: As documented in the Sleep Mechanics section, duplicants keep sleeping until stamina reaches 100%, even into Work blocks. Sleep's interrupt priority (97600) is higher than all work chores (96700), so the brain cannot find a work chore that can interrupt sleep.

3. **Eating is Downtime-only by default**: The Eat urge only activates during blocks that allow the Eat type (only Downtime). A hungry duplicant in a Work block will keep working. A starving duplicant has the Eat urge regardless of schedule.

4. **Work during Downtime**: Since the Downtime group includes the Work type, work chore preconditions pass during Downtime. If a duplicant has no active urges (not hungry, not tired, bladder low, no recreation desire), they will work during Downtime.

5. **Idle during restrictive blocks**: During Bedtime, if a duplicant has full stamina and doesn't need sleep, most chores will fail the `IsScheduledTime` precondition. The Idle chore has no schedule precondition, so they idle. Same during Work blocks if no work is available.

## Stamina and Sleep Mechanics

### Stamina Amount

Stamina is an `Amount` ranging from 0 to 100. It is displayed as a percentage.

**Drain rate:** Duplicants lose stamina at `DUPLICANTSTATS.BaseStats.STAMINA_USED_PER_SECOND` which is `-7/60` per second, or about **-70% per cycle** (7/60 * 600 = 70).

**Recovery:** Sleep effects (applied by `Sleepable.OnStartWork`) add positive stamina delta modifiers. The base "PassedOutSleep" effect adds `+2/3` per second. Bed quality (cot vs comfy bed) and room bonuses (Barracks, Bedroom) affect the recovery rate through different named effects.

### When Duplicants Sleep

`StaminaMonitor` has two states:
- **satisfied:** Stamina > 0 and no active sleep urge
- **sleepy:** Stamina has hit 0, or the duplicant's current chore satisfies the Sleep urge

The `UrgeMonitor` is configured with threshold 100 (max) and minimum=false, meaning the sleep urge activates as stamina drops and the schedule permits it.

### When Duplicants Wake Up

`StaminaMonitor.Instance.ShouldExitSleep()` returns true only when ALL of these are true:
1. The current schedule block does NOT allow `ScheduleBlockTypes.Sleep`
2. The duplicant is NOT narcolepsing
3. Stamina has reached its maximum (100%)

This means duplicants keep sleeping past their Bedtime blocks until stamina reaches 100%, even if the current block is Work, Bathtime, or Downtime (none of which include Sleep in their allowed types). `Sleepable.OnWorkTick` calls `ShouldExitSleep()` each tick, so the duplicant wakes as soon as stamina is full. If stamina is not yet full when a non-Sleep block begins, the duplicant continues sleeping regardless of the block type.

### Sleep Quality and Interruptions

`SleepChore` tracks five types of sleep disturbance:
- **Noise** (from Snorers) - applies "TerribleSleep" effect, removes "BadSleep"
- **Light** (brightness at cell >= `DUPLICANTSTATS.STANDARD.Light.LOW_LIGHT`) - applies "BadSleep"
- **Fear of dark** (NightLight trait, wakes if dark) - applies "BadSleepAfraidOfDark"
- **Movement** (interrupted by nearby activity) - applies "BadSleepMovement"
- **Cold** (external temperature too cold) - applies "BadSleepCold"

After each interruption, if the schedule still allows Sleep, the duplicant returns to normal sleep. Otherwise the chore completes.

### Sleep Locations

`SleepChoreMonitor` determines where a duplicant sleeps:
1. **Assigned bed** (checked via `AssignableSlots.Bed` or `AssignableSlots.MedicalBed`) - normal sleep
2. **Floor** (no bed or bed unreachable) - applies "FloorSleep" effect and "SoreBack" on wake
3. **Passed out** (stamina at 0, exhausted) - applies "PassedOutSleep" effect and "SoreBack" on wake

### Narcolepsy

Duplicants with the Narcolepsy trait (`Narcolepsy.cs`) periodically fall asleep involuntarily:
- **Interval between episodes:** 300-600 seconds (`TRAITS.NARCOLEPSY_INTERVAL_MIN/MAX`)
- **Episode duration:** 15-30 seconds (`TRAITS.NARCOLEPSY_SLEEPDURATION_MIN/MAX`)
- Narcolepsy sleep overrides schedule: `ShouldExitSleep()` returns false while narcolepsing
- If already sleeping normally when narcolepsy triggers, the episode is skipped
- Uses `ChoreTypes.Narcolepsy` with floor locator and "NarcolepticSleep" effect

## Downtime and Morale

### Downtime Morale Bonus

`RecreationTimeMonitor` tracks how many Downtime blocks have elapsed recently and grants a morale bonus:

- Each completed Downtime block (where `GroupId == Recreation.Id`) adds a timestamp to `moraleAddedTimes`
- Timestamps expire after **600 seconds** (1 cycle) for standard duplicants, or **1800 seconds** (3 cycles) for Bionics
- The morale bonus is `clamp(count - 1, 0, 5)`, meaning:
  - 1 Downtime block: +0 morale (the first block grants no bonus)
  - 2 blocks: +1 morale
  - 3 blocks: +2 morale
  - 4 blocks: +3 morale
  - 5 blocks: +4 morale
  - 6+ blocks: +5 morale (capped, labeled "Max Downtime Bonus")

The bonus is applied as a `QualityOfLife` attribute modifier via a dynamic effect.

### Recreation Activities

During Downtime, duplicants with recreation urges will use recreation buildings (Water Cooler, Espresso Machine, Arcade Cabinet, Beach Chair, etc.) that provide separate morale effects. These effects are independent of the Downtime Morale Bonus above. Duplicants prioritize recreation buildings by desirability tier, choosing the nearest building from the highest tier, with a 300-second cooldown on recently-used buildings.

## Red Alert Override

`Schedulable.IsAllowed()` bypasses schedule restrictions during Red Alert:
```csharp
if (!myWorld.AlertManager.IsRedAlert())
    return ScheduleManager.Instance.IsAllowed(this, schedule_block_type);
return true;  // all block types allowed during red alert
```

During Red Alert, all activity types are permitted regardless of the current schedule block. This means duplicants will not sleep during Red Alert even if it's bedtime.

## Schedule-Sensitive Traits

### Early Bird
- Active during blocks 0-4 (the first 5 blocks of the cycle, `TRAITS.EARLYBIRD_SCHEDULEBLOCK = 5`)
- Adds +2 to all 11 skill attributes (`TRAITS.EARLYBIRD_MODIFIER = 2f`)

### Night Owl
- Active when `GameClock.IsNighttime()` returns true, which is when cycle percentage >= 0.875 (blocks 21-23, the last 3 blocks)
- Adds +3 to all 11 skill attributes (`TRAITS.NIGHTOWL_MODIFIER = 3f`)

## Schedule Screen UI

`ScheduleScreen` (extends `KScreen`) displays all schedules. Each `ScheduleScreenEntry` contains:
- Editable schedule name
- Alarm toggle button
- Timetable rows with 24 `ScheduleBlockButton` widgets each
- Paint tools for Bathtime, Work, Downtime, and Sleep (click a paint tool, then click blocks to assign)
- Rotate left/right buttons (circular shift blocks within a row)
- Shift up/down buttons (reorder timetable rows)
- Duplicate/delete timetable row buttons
- Assigned minion portrait widgets with dropdown to reassign

The screen opens via `ManagementMenu` and has sort key 50.

## Key Source Constants

| Constant | Value | Location |
|---|---|---|
| Blocks per cycle | 24 | `Schedule.SetBlocksToGroupDefaults` assert |
| Seconds per block | 25 | 600s cycle / 24 blocks |
| Stamina drain rate | -7/60 per second | `DUPLICANTSTATS.BaseStats.STAMINA_USED_PER_SECOND` |
| Passed-out stamina recovery | +2/3 per second | `ModifierSet.cs` PassedOutSleep effect |
| Max downtime morale bonus | +5 | `RecreationTimeMonitor.MAX_BONUS = 5` |
| Downtime bonus duration | 600s (standard), 1800s (Bionic) | `RecreationTimeMonitor` constants |
| Early Bird active blocks | 0-4 | `TRAITS.EARLYBIRD_SCHEDULEBLOCK = 5` |
| Early Bird attribute bonus | +2 | `TRAITS.EARLYBIRD_MODIFIER` |
| Night Owl active threshold | cycle % >= 0.875 (blocks 21-23) | `GameClock.IsNighttime()` |
| Night Owl attribute bonus | +3 | `TRAITS.NIGHTOWL_MODIFIER` |
| Narcolepsy interval | 300-600s | `TRAITS.NARCOLEPSY_INTERVAL_MIN/MAX` |
| Narcolepsy duration | 15-30s | `TRAITS.NARCOLEPSY_SLEEPDURATION_MIN/MAX` |
| Interrupt priority start | 100000 | `Database.ChoreTypes` constructor |
| Interrupt priority step | -100 per tier | `Database.ChoreTypes` constructor |
| Bladder break threshold | 40% | `BladderMonitor.Instance.WantsToPee()` |
| Bladder urgent threshold | 100% | `BladderMonitor.Instance.NeedsToPee()` |
| Sleep urge in-schedule threshold | 100 (always active) | `StaminaMonitor` UrgeMonitor config |
| Sleep urge out-of-schedule threshold | 0 (only when exhausted) | `StaminaMonitor` UrgeMonitor config |
