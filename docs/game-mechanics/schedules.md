# Schedules

How the schedule system controls duplicant behavior across the cycle. Derived from decompiled source code (`Schedule.cs`, `ScheduleManager.cs`, `ScheduleBlock.cs`, `ScheduleGroup.cs`, `ScheduleBlockType.cs`, `Database/ScheduleGroups.cs`, `Database/ScheduleBlockTypes.cs`, `RecreationTimeMonitor.cs`, `StaminaMonitor.cs`, `SleepChore.cs`, `SleepChoreMonitor.cs`, `Schedulable.cs`).

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
