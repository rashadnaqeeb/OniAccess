# Bladder and Bathrooms

How duplicants fill and empty their bladders, toilet building types and their resource costs, bathroom accidents, and wash stations. Derived from decompiled source (`BladderMonitor`, `Toilet`, `FlushToilet`, `PeeChore`, `GunkMonitor`, `HandSanitizer`, various config classes).

## Bladder Amount

The Bladder amount ranges from 0 to 100. It fills at a constant rate controlled by the `BladderDelta` attribute.

**Base fill rate**: 1/6 per second (`DUPLICANTSTATS.STANDARD.BaseStats.BLADDER_INCREASE_PER_SECOND`). At this rate, a full cycle (600s) fills the bladder from 0 to 100.

Source: `Database.Amounts` creates Bladder as `(0, 100)`. `MinionConfig` applies the delta modifier.

### Modifiers

| Source | BladderDelta Modifier | Effective Fill Time |
|--------|----------------------|---------------------|
| Base (no modifiers) | +1/6 /s | 600s (1 cycle) |
| SmallBladder trait | +1/3600 /s | ~599s (negligible) |
| Food Poisoning disease | +1/3 /s | 200s (1/3 cycle) |

Source: `TRAITS.cs`, `FoodSickness.cs`.

### Thresholds and State Machine

`BladderMonitor` has three top-level states:

- **satisfied**: bladder below thresholds
- **breakwant**: bladder >= 40 during a Hygiene schedule block. Dupe voluntarily seeks a toilet
- **urgentwant**: bladder >= 100 (full). Dupe urgently seeks a toilet, gains the `FullBladder` effect, and the `PeeChoreMonitor` activates

Dupes do not wake up to pee. `NeedsToPee()` returns false while the duplicant has the `Asleep` tag.

Source: `BladderMonitor.cs`.

### Pee Fuse (Accident Timer)

When `urgentwant` is active, `PeeChoreMonitor` starts a countdown from **120 seconds** (`PEE_FUSE_TIME`). The fuse counts down each frame. If it reaches 60s remaining, the state becomes `critical` and the dupe is added to `Components.CriticalBladders`. If it reaches 0, a `PeeChore` triggers (bathroom accident). The fuse pauses while sleeping.

Source: `PeeChoreMonitor.cs`.

## Toilet Types

### Outhouse

Basic toilet. Uses dirt as input, produces polluted dirt (Toxic Sand) as waste.

| Property | Value |
|----------|-------|
| ID | `Outhouse` |
| Size | 2x3 |
| Uses per refill | 15 |
| Dirt per refill | 200 kg (delivered manually) |
| Dirt consumed per use | 13 kg |
| Waste per use | 6.7 kg Toxic Sand (`PEE_PER_TOILET_PEE`) |
| Waste output total per use | 6.7 + 13 = 19.7 kg Toxic Sand (pee mass + consumed dirt) |
| Disease per flush (in waste) | 100,000 Food Poisoning germs |
| Disease added to dupe | 100,000 Food Poisoning germs |
| Clean time | 90s (Tidying attribute speeds this up) |
| Spawns Glom | Yes, if left full for 1200s (2 cycles) |

When all 15 uses are consumed, the outhouse enters a `full` state and a clean chore is created. A duplicant must come empty it, which drops the Toxic Sand and remaining Dirt on the floor. Bionics can also use outhouses (they produce gunk instead and clog the toilet).

Source: `OuthouseConfig.cs`, `Toilet.cs`.

### Lavatory (Flush Toilet)

Piped toilet. Consumes water via liquid input, outputs polluted water via liquid output.

| Property | Value |
|----------|-------|
| ID | `FlushToilet` |
| Size | 2x3 |
| Water consumed per use | 5 kg |
| Waste emitted per use | 11.7 kg Dirty Water (5 kg water + 6.7 kg pee) |
| Disease per flush (in waste) | 100,000 Food Poisoning germs |
| Disease added to dupe | 5,000 Food Poisoning germs (1/20th of waste germs) |
| Storage capacity | 25 kg |
| Clean time | 90s (only when clogged by bionic gunk) |
| Requires output pipe | Yes |

The lavatory does not need periodic emptying like the outhouse. It flushes automatically after each use. However, bionic duplicants clog it with gunk, requiring a clean chore.

Source: `FlushToiletConfig.cs`, `FlushToilet.cs`.

### Wall Toilet (DLC1: Spaced Out)

Compact piped toilet that mounts on a wall. Only 1 tile wide. Drops waste liquid instead of piping it out.

| Property | Value |
|----------|-------|
| ID | `WallToilet` |
| Size | 1x3 |
| Construction material | Plastic |
| Water consumed per use | 2.5 kg |
| Waste emitted per use | 9.2 kg Dirty Water (2.5 kg water + 6.7 kg pee) |
| Disease per flush (in waste) | 100,000 Food Poisoning germs |
| Disease added to dupe | 20,000 Food Poisoning germs (1/5th) |
| Storage capacity | 12.5 kg |
| Requires output pipe | No (uses `AutoStorageDropper` to drop waste liquid) |

Source: `WallToiletConfig.cs`.

### Gunk Emptier (DLC3: Bionic Booster Pack)

Bionic-only toilet. Bionics accumulate gunk (Liquid Gunk) instead of using a bladder. The gunk emptier drains gunk via liquid output pipe.

| Property | Value |
|----------|-------|
| ID | `GunkEmptier` |
| Size | 3x3 |
| Gunk capacity (bionic) | 80 kg (`GunkMonitor.GUNK_CAPACITY`) |
| Storage capacity | 120 kg (1.5x gunk capacity) |
| Output | Liquid Gunk via liquid conduit |
| Disease added to dupe | 5,000 Food Poisoning germs (1/20th of `DISEASE_PER_PEE`) |
| Work time | 8.5s |
| Assignable to | Bionics only |

Source: `GunkEmptierConfig.cs`, `GunkEmptier.cs`, `GunkEmptierWorkable.cs`.

## Toilet Use Work Time

All standard toilets (Outhouse, Lavatory, Wall Toilet) and the Gunk Emptier share the same base work time of **8.5 seconds** (`ToiletWorkableUse.SetWorkTime(8.5f)` / `GunkEmptierWorkable.SetWorkTime(8.5f)`). This is modified by the Toilet Speed attribute converter.

On completion, `ToiletWorkableUse.OnCompleteWork` resets the duplicant's bladder to 0 and records 6.7 kg as the waste mass contributed. For bionics, it instead drains all gunk via `GunkMonitor`.

Source: `ToiletWorkableUse.cs`, `GunkEmptierWorkable.cs`.

## Bionic Gunk System

Bionics do not have a Bladder amount. Instead they have a `BionicGunk` amount (0 to 80 kg). Gunk accumulates as a byproduct of oil consumption: when `BionicOilMonitor` consumes oil, the consumed delta is added to gunk mass.

`GunkMonitor` thresholds:

| Threshold | Percentage | Behavior |
|-----------|-----------|----------|
| Mild urge | >= 60% (48 kg) | Seeks toilet during Hygiene or Eat schedule blocks |
| Critical urge | >= 90% (72 kg) | Seeks toilet regardless of schedule, gains `GunkSick` effect, slouching animations |
| Can't hold | 100% (80 kg) | Forces a `BionicGunkSpillChore` (gunk accident), then applies `GunkHungover` effect |

When a bionic uses a standard flush toilet (Lavatory or Wall Toilet), it clogs the toilet with gunk. The toilet enters a `clogged` state requiring a clean chore (90s work time) before it can be used again.

Source: `GunkMonitor.cs`, `FlushToilet.cs`, `ToiletWorkableUse.cs`.

## Bathroom Accidents (PeeChore)

When the pee fuse expires in `PeeChoreMonitor`, a `PeeChore` is created at `compulsory` priority. The duplicant stands in place and pees on the floor until the bladder drains to 0.

| Property | Value |
|----------|-------|
| Output element | Dirty Water |
| Total mass | 2 kg (`PEE_PER_FLOOR_PEE`) |
| Disease | Food Poisoning germs (100,000 scaled over duration) |
| Effect applied | `StressfulyEmptyingBladder` |
| Report entry | Toilet Incident |
| Radiation | Expels stored rads (up to 100 * difficulty modifier) |

The dirty water (or gunk, for bionics via `BionicGunkSpillChore`) is deposited at the duplicant's current cell. If wearing an airtight suit, the waste goes into the suit's storage instead.

Source: `PeeChore.cs`, `PeeChoreMonitor.cs`.

## Wash Stations

Wash stations are pass-through buildings. Duplicants use them reactively when walking past, if they are carrying germs. Both types remove germs from the duplicant (and from suit storage for the Sink).

### Wash Basin

Manual wash station. Requires water delivery by hand. Dumps dirty water on the floor when full.

| Property | Value |
|----------|-------|
| ID | `WashBasin` |
| Size | 2x3 |
| Water per use | 5 kg |
| Uses before dump | 40 |
| Total water capacity | 200 kg |
| Work time | 5s |
| Disease removal per use | 120,000 germs (`DISEASE_PER_PEE + 20000`) |
| Output | Dirty Water (dumped on floor) |

Source: `WashBasinConfig.cs`.

### Sink (Wash Sink)

Piped wash station. Connects to liquid input and output conduits.

| Property | Value |
|----------|-------|
| ID | `WashSink` |
| Size | 2x3 |
| Water per use | 5 kg |
| Uses per pipe cycle | 2 |
| Input capacity | 10 kg |
| Work time | 5s |
| Disease removal per use | 120,000 germs (`DISEASE_PER_PEE + 20000`) |
| Output | Dirty Water (via liquid conduit) |
| Room tag | `AdvancedWashStation` (counts for Washroom upgrade) |

Source: `WashSinkConfig.cs`.

### Wash Trigger Behavior

Wash stations use a `WashHandsReactable`. A duplicant triggers the reactable when walking past the station in its configured direction, but only if the duplicant currently carries germs (any `DiseaseIdx != 255`). If `alwaysUse` is set (not the case for Wash Basin or Sink), the station triggers regardless of germ status.

Disease removal happens incrementally during the 5s work tick, proportional to time spent. The consumed water inherits the removed germs and is stored as Dirty Water.

Source: `HandSanitizer.cs`.

## Key Classes

| Class | Role |
|-------|------|
| `BladderMonitor` | State machine tracking bladder fullness, triggers toilet-seeking behavior |
| `PeeChoreMonitor` | Countdown timer from full bladder to forced accident |
| `PeeChore` | Bathroom accident chore: drains bladder, spawns Dirty Water on floor |
| `ToiletMonitor` | Tracks whether usable toilets exist and are reachable |
| `Toilet` | Outhouse component: manages uses, dirt consumption, cleaning |
| `FlushToilet` | Lavatory/Wall Toilet component: manages water/waste flow, clogging |
| `ToiletWorkableUse` | Shared workable for all toilet types (8.5s work time) |
| `ToiletWorkableClean` | Clean chore workable (90s work time) |
| `GunkMonitor` | Bionic gunk accumulation state machine |
| `GunkEmptier` | Bionic toilet building state machine |
| `HandSanitizer` | Wash Basin/Sink component: reactable pass-through germ removal |
