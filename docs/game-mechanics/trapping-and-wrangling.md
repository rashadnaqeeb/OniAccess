# Trapping and Wrangling

How creatures are captured, relocated, and managed through traps, wrangling, lures, and delivery buildings. Derived from decompiled source (`Trap`, `ReusableTrap`, `TrapTrigger`, `Trappable`, `Capturable`, `Lure`, `LureableMonitor`, `FixedCapturePoint`, `CreatureDeliveryPoint`, and associated configs).

## Capture Methods Overview

There are three ways to capture a creature:

1. **Wrangling** -- a duplicant manually captures a marked creature (Capture tool or right-click menu)
2. **Traps** -- passive buildings that spring when a creature walks over them
3. **Fixed capture points** -- Critter Pick-Up stations that have a rancher call creatures over for bagging

All three require the **Creature Wrangling** skill perk (`CanWrangleCreatures`), granted by the Rancher skill (Ranching 1). Capture speed scales with the Ranching attribute: `+5% per skill point` (`CapturableSpeed` converter, `0.05f` multiplier on `Ranching` attribute).

---

## Wrangling (Manual Capture)

Controlled by the `Capturable` component on creatures. A creature can be marked for capture via:
- The **Capture tool** (`CaptureTool`), a drag-select tool in the toolbar
- The creature's right-click menu ("Wrangle" / "Cancel Wrangle")

### Preconditions

A creature is capturable (`IsCapturable()`) only if:
- `allowCapture` is true (set false on death)
- Not tagged `GameTags.Trapped` (already in a trap)
- Not tagged `GameTags.Stored` (already in storage)
- Not tagged `GameTags.Creatures.Bagged` (already wrangled)

### Work Parameters

| Parameter | Value | Source |
|-----------|-------|--------|
| Work time | 10 seconds | `Capturable.OnSpawn()` |
| Chore type | Capture | `ChoreTypes.Capture` (Ranching group) |
| Required perk | CanWrangleCreatures | `Capturable.OnPrefabInit()` |
| Attribute | Ranching | Speeds up work via `CapturableSpeed` converter |
| XP skill group | Ranching | `skillExperienceSkillGroup = SkillGroups.Ranching.Id` |
| Preemptable | Yes | `is_preemptable: true` in chore creation |

### What Happens on Completion

1. The creature's `Baggable.SetWrangled()` is called
2. The creature gets tagged `GameTags.Creatures.Bagged`
3. The creature plays its "trussed" animation in a loop
4. Its navigator switches to Floor nav type
5. The creature becomes a pickupable item (3-second pickup time)
6. Duplicants can then carry it to a Critter Drop-Off

---

## Trap Buildings

Traps are passive buildings that automatically capture creatures when they walk onto the trap's trigger cell. There are two trap systems: legacy single-use traps (`Trap`) and the current reusable traps (`ReusableTrap`).

### Legacy Traps (Deprecated)

`CreatureTrap` and `FishTrap` use the `Trap` component. Both are deprecated (`Deprecated = true`).

- **Creature Trap** (`CreatureTrap`) -- 2x1, catches Walker and Hoverer creatures, made of Plastics (200 kg). Hidden from build menu.
- **Fish Trap** (`FishTrap`) -- 1x2, catches Swimmer creatures, made of Plastics (200 kg). Has a built-in lure (`FishTrapLure`, radius 32). Deprecated.

Legacy traps are single-use: they go through `ready -> trapping -> occupied -> finishedUsing -> destroySelf`. Once the creature is removed, the trap is destroyed.

### Reusable Traps (Current)

The current trap buildings use `ReusableTrap`, a state machine with arm/disarm/capture/release cycles. All require a duplicant with the Creature Wrangling perk to arm them.

| Building | ID | Size | Catches | Material | Mass | Lure Tag | Lure Radius |
|----------|----|------|---------|----------|------|----------|-------------|
| Ground Trap | CreatureGroundTrap | 2x2 | Walker, Hoverer, Swimmer | Raw Metals | 200 kg | None | -- |
| Water Trap | WaterTrap | 1x2 | Swimmer | Raw Metals | 200 kg | FishTrapLure | 32 |
| Air Trap | CreatureAirTrap | 1x2 | Flyer | Raw Metals | 50 kg | FlyersLure | 32 |

### ReusableTrap State Machine

```
operational.unarmed --> (ArmTrap work complete) --> operational.armed
operational.armed --> (creature triggers) --> operational.capture.capturing
operational.capture.capturing --> (anim complete) --> operational.capture.idle (waiting for pickup)
operational.capture.idle --> (storage emptied) --> operational.capture.release --> operational.unarmed

noOperational.idle --> (if has critter) --> noOperational.releasing (releases, then idle)
noOperational.idle --> (if armed) --> noOperational.disarming (disarms, then idle)
```

### Arming

| Parameter | Value | Source |
|-----------|-------|--------|
| Arm work time | 5 seconds | `ArmTrapWorkable.SetWorkTime(5f)` |
| Chore type | ArmTrap | `ChoreTypes.ArmTrap` (Ranching group) |
| Required perk | CanWrangleCreatures | `ArmTrapWorkable.requiredSkillPerk` |
| Attribute | Ranching | Via `CapturableSpeed` converter |
| Reset on interrupt | Yes | `resetProgressOnStop = true` |

When armed, the `TrapTrigger` is enabled and lures are activated (if the trap has them). When not operational (power off, logic signal), the trap disarms and releases any captured creature.

### Logic Ports

All reusable traps have:
- **Logic input** (`LogicOperationalController`) -- enables/disables the trap
- **Logic output** (`TRAP_HAS_PREY_STATUS_PORT`) -- green (1) when the trap contains a creature and is operational; red (0) otherwise

### TrapTrigger Mechanism

`TrapTrigger` registers on the `trapsLayer` of the `GameScenePartitioner`. When a `Trappable` creature changes cells, it triggers `OnCreatureOnTrap()` on all traps in that cell.

The trap captures the creature if:
1. The trap is enabled and its storage is empty
2. The creature is not already tagged `Stored`, `Trapped`, or `Bagged`
3. The creature has at least one tag matching the trap's `trappableCreatures` array
4. Any custom conditions (`customConditionsToTrap`) pass

On capture, the creature is stored in the trap's `Storage` component and tagged `GameTags.Trapped`. Its navigator stops and its brain is prioritized for a re-evaluation.

### Creature Movement Tags vs. Trap Compatibility

| Movement Tag | Ground Trap | Water Trap | Air Trap |
|-------------|-------------|------------|----------|
| Walker | Yes | -- | -- |
| Hoverer | Yes | -- | -- |
| Swimmer | Yes | Yes | -- |
| Flyer | -- | -- | Yes |

### Water Trap Special Mechanics

The Water Trap uses `WaterTrapTrail` to extend its trigger cell downward. It scans up to 4 cells below itself for open liquid, and moves both the trap trigger and lure to the deepest available cell. The trail pipe symbols are shown/hidden to match the depth. This allows the trap to catch fish swimming below the surface.

---

## Lure System

The `Lure` state machine registers an area in the `GameScenePartitioner.lure` layer. Creatures with `LureableMonitor` periodically scan for active lures within their cell's partition.

### Lure.Def Parameters

| Field | Description | Default |
|-------|-------------|---------|
| `defaultLurePoints` | Cell offsets where lured creatures navigate to | `{(0,0)}` |
| `radius` | Partitioner extent around the lure cell | 50 |
| `initialLures` | Lure tags activated on spawn | null |

### LureableMonitor Behavior

Each creature with `LureableMonitor` has a cooldown (default **20 seconds**, `cooldown = 20f`). After cooldown, the creature searches for the nearest reachable active lure matching any of its lure tags. If found, it toggles the `MoveToLure` behavior, navigating to the lure's `LurePoints`.

The creature picks the lure with the lowest navigation cost. One-time-use lures (tagged `GameTags.OneTimeUseLure`) get tagged `LureUsed` after arrival, which triggers their destruction.

### Creature Lure Tags

| Creature | Movement | Lure Tags |
|----------|----------|-----------|
| Hatch (all variants) | Walker | None (no LureableMonitor) |
| Drecko | Walker | None |
| Pacu (all variants) | Swimmer | FishTrapLure |
| Shine Bug (base) | Flyer | Phosphorite, FlyersLure |
| Shine Bug (Abyss) | Flyer | Phosphorus, FlyersLure |
| Shine Bug (Radiant) | Flyer | Phosphorite, Phosphorus, FlyersLure |
| Shine Bug (Crystal) | Flyer | Diamond, FlyersLure |
| Puft (all variants) | Flyer | SlimeMold, FlyersLure |
| Pip | Walker | None |
| Pokeshell | Walker | None |
| Moo | Flyer | BleachStone, FlyersLure |
| Sweetle/Grub Grub | Walker | None |
| Plug Slug | Walker | None |
| Oil Floater | Hoverer | None |
| Flutterby | Flyer | Algae, FlyersLure |
| Mosquito (adult) | Flyer | FlyersLure |
| Mosquito (larva) | Swimmer | FishTrapLure |

### Creature Lure Building (Deprecated)

`AirborneCreatureLure` uses `CreatureLure`, which requires a bait element delivered to its storage. The side screen (`LureSideScreen`) lets the player select one of:

| Bait Element | Attracts |
|-------------|----------|
| Slime Mold | Pufts |
| Phosphorite | Shine Bugs |

The bait is consumed at `CONSUMPTION_RATE = 1f` (1 kg/s). This building is deprecated and hidden from the build menu. The active traps with built-in lures (Water Trap, Air Trap) have replaced it.

### One-Time-Use Bait (Deprecated)

`FlyingCreatureBait` is a single-use lure building. It takes 50 kg Metal + 10 kg of a FlyingCritterEdible material. On construction, it uses the second construction element as the active lure tag (via `CreatureBait`). When a creature arrives, the bait is tagged `LureUsed` and plays a destruction animation. Deprecated and hidden from the build menu.

---

## Fixed Capture Points (Critter Pick-Up)

The **Critter Pick-Up** (`CritterPickUp`) station is a 1x3 building that uses `FixedCapturePoint` to have a rancher call creatures from the same room for capture. It works cooperatively: the rancher waits at the station, calls the creature, and captures it when it arrives.

### How It Works

1. `FixedCapturePoint` scans `Components.FixedCapturableMonitors` every 1000ms for eligible creatures
2. The station creates a `FixedCaptureChore` (Ranch chore type) requiring the `CanWrangleCreatures` perk
3. The rancher moves to the station and plays a "calling" animation
4. The creature gets the `WantsToGetCaptured` behavior, plays an "excited" animation, then moves toward the station at **1.25x its normal speed**
5. The rancher performs the capture work on the creature (uses `Capturable` workable)
6. On success, the creature is bagged

### Capture Eligibility

A creature can be captured at a pick-up point only if (`CanCapturableBeCapturedAtCapturePoint`):
- The creature's `FixedCapturableMonitor` is running
- The creature isn't already assigned to another capture point
- The creature is in the **same room** (same `CavityInfo`) as the station
- The creature is not tagged `Bagged`
- If the creature is a baby, the station must allow babies (`def.allowBabies`)
- The capture chore priority is high enough for the creature's current activity
- The creature can navigate to the station cell
- The room is **over capacity** (`isAmountStoredOverCapacity` returns true)

For the Critter Pick-Up, over-capacity means the number of matching (filtered) creatures in the room exceeds the user-set limit, and the creature's prefab tag is in the station's filter.

### Auto-Wrangle

The Critter Pick-Up has an `AutoWrangleCapture` checkbox. When automated mode is enabled (default for stations without the checkbox component), the station continuously creates capture chores. When manual, it does nothing until toggled on.

### Logic Input

The station accepts a logic input (`CritterPickUpInput`). When connected and receiving a green signal (>0), the station operates normally. When red, it is disabled. When not connected, it defaults to enabled.

---

## Critter Drop-Off

The **Critter Drop-Off** (`CritterDropOff`) is a 1x3 building that accepts bagged creatures for delivery to a room. It uses `CreatureDeliveryPoint` with `BaggableCritterCapacityTracker`.

### Delivery Mechanics

1. The drop-off uses `TreeFilterable` to let the player choose which creature species to accept
2. `BaggableCritterCapacityTracker` counts creatures in the room (same `CavityInfo`), excluding those tagged `Bagged` or `Trapped`
3. If the room count is below the user-set limit, `CreatureDeliveryPoint` creates `FetchOrder2` chores (type `CreatureFetch`, Ranching group) for bagged creatures tagged `Deliverable`
4. Duplicants pick up the bagged creature and deliver it to the drop-off's storage
5. On storage change, the creature is dropped at the configured spawn offset
6. Large creatures (tagged `LargeCreature`) are dropped at a separate offset

### Capacity

| Parameter | Default | Maximum |
|-----------|---------|---------|
| Creature limit | 20 | 40 |
| Minimum | 0 | -- |
| Whole values only | Yes | -- |

The count refreshes every 1000ms via `Sim1000ms`. The drop-off only counts non-bagged, non-trapped creatures that match its filter tags.

### Logic Input

The drop-off accepts a logic input (`CritterDropOffInput`). When connected and receiving green, delivery fetches are active. When red, all pending fetches are cancelled and the station stops requesting deliveries.

---

## Carnivorous Plants (Saturn Critter Trap)

The **Saturn Critter Trap** (`CritterTrapPlant`, Spaced Out DLC) is a plant that passively traps and consumes creatures using the same `TrapTrigger` mechanism as trap buildings.

### Capture Rules

- Catches creatures tagged `Walker` or `Hoverer`
- Additionally requires `customConditionsToTrap`: the creature must have a `Trappable` component, match the plant's `CONSUMABLE_TAGs`, and occupy fewer than 3 cells (excludes large creatures)
- On capture, the creature is consumed immediately (storage cleared)
- The plant then enters a digesting state, enabling `Growing` until the next harvest
- During digestion, outputs Hydrogen gas at `1/24 kg/s`, venting at `33.25 kg` threshold
- Produces Plant Meat when fully grown

### Irrigation

- Consumes Polluted Water at `1/60 kg/s` (1 kg/cycle)

---

## Bagged Creature Lifecycle

When a creature is wrangled or removed from a trap, it enters the `Baggable` state:

1. Tagged `GameTags.Creatures.Bagged`
2. Plays "trussed" animation in a loop (or building-specific bagged animation)
3. Navigator set to Floor nav type
4. Becomes a pickupable item with 3-second pickup time (`Baggable.OnSpawn` sets `pickupable.SetWorkTime(3f)`)
5. When stored in a duplicant's inventory, the creature is hidden (animation and selectable disabled)
6. When dropped from storage, `Free()` removes the Bagged tag and restores visibility -- unless `keepWrangledNextTimeRemovedFromStorage` is set

### Trapped Creature State

Creatures in traps play the "trapped" animation via `TrappedStates`. The trap building can provide a custom animation name via `ITrapStateAnimationInstructions`. Swimming creatures trapped on land play "trapped_onLand". Trapped creatures are tagged `Deliverable`, making them eligible for fetch orders to Critter Drop-Offs.

---

## Chore Priority Order

All creature-management chores belong to the Ranching skill group. Their relative position in the global chore list (from `ChoreTypes` constructor order):

| Chore | Group | Priority Position |
|-------|-------|----|
| Capture (manual wrangle) | Ranching | High (near Toggle) |
| CreatureFetch (deliver to drop-off) | Ranching | Below Capture |
| RanchingFetch (deliver bait) | Ranching, Hauling | Below CreatureFetch |
| ArmTrap | Ranching | Low (near MoveToSafety) |
| Ranch (fixed capture station) | Ranching | Mid-range (near PowerFetch) |
