# Colony Management

Consumable permissions, door access control, daily reports, colony diagnostics (alert system), and notification dispatch. Derived from decompiled source code.

For colony achievements, printing pod, duplicant generation, and care packages, see `colony-systems.md`. For duplicant health/vitals tracking, see `duplicant-health.md`. For stress and morale, see `stress-and-morale.md`.

## Consumable Permissions

Each duplicant has a `ConsumableConsumer` component that controls which food items and medicines they are allowed to consume. The system uses two tag sets:

- **`forbiddenTagSet`** (serialized, player-controlled) -- items the player has explicitly forbidden for this duplicant. Toggling a food item in the consumables screen adds/removes it from this set.
- **`dietaryRestrictionTagSet`** (model-based, not serialized) -- items that are structurally incompatible with the duplicant's model type. Set automatically on spawn.

Source: `ConsumableConsumer.cs`, `ConsumerManager.cs`.

### Permission Check

`ConsumableConsumer.IsPermitted(string consumable_id)` returns true only if the item's tag is in neither `forbiddenTagSet` nor `dietaryRestrictionTagSet`. Both must pass.

`SetPermitted(string consumable_id, bool is_allowed)` enforces dietary restrictions: even if `is_allowed` is true, the item stays forbidden if it is in `dietaryRestrictionTagSet`.

### Dietary Restrictions by Model

`ConsumerManager` defines the hard restrictions per duplicant model:

**Standard duplicants** cannot consume:
- Charged portable batteries (`GameTags.ChargedPortableBattery`)
- Oxygen tanks (`ClosestOxygenCanisterSensor.GenericBreathableGassesTankTag`)

**Bionic duplicants** cannot consume:
- All edible food items (`GameTags.Edible`)
- Incompatible batteries (`GameTags.BionicIncompatibleBatteries`)

Source: `ConsumerManager.StandardDuplicantDietaryRestrictions`, `ConsumerManager.BionicDuplicantDietaryRestrictions`.

### Default Forbidden List

`ConsumerManager.DefaultForbiddenTagsList` is a serialized per-colony list of tags that are forbidden by default for newly created duplicants. When a `ConsumableConsumer` initializes, it copies this list as its starting `forbiddenTagSet`.

### Discovery System

Not all consumables appear in the permissions UI from the start. `ConsumerManager.RefreshDiscovered()` maintains an `undiscoveredConsumableTags` list. A food item becomes discovered when:
- `DiscoveredResources.Instance.IsDiscovered(tag)` returns true (the item has been encountered in the world), OR
- A recipe that produces it has an unlocked fabricator (`TechItems.IsTechItemComplete`), OR
- A visible crop on the map produces it

Source: `ConsumerManager.ShouldBeDiscovered()`.

## Door Access Control

The `AccessControl` component manages per-entity traversal permissions on doors and teleporters. Permissions are enforced at the pathfinding level via `Grid.Restriction`.

Source: `AccessControl.cs`.

### Permission Levels

`AccessControl.Permission` enum:

| Value | Name | Effect |
|-------|------|--------|
| Both | Both | Can pass in either direction |
| GoLeft | GoLeft | Can only pass leftward (blocks rightward traversal) |
| GoRight | GoRight | Can only pass rightward (blocks leftward traversal) |
| Neither | Neither | Cannot pass in either direction |

The permission is inverted when applied to the grid: `GoLeft` sets a `Right` direction restriction (blocking rightward movement), `GoRight` sets a `Left` restriction, and `Neither` sets both.

For teleporters, any non-`Both` permission maps to `Directions.Teleport` (full block).

### Permission Hierarchy

Permissions are stored at two levels:

1. **Default permission by tag** (`defaultPermissionByTag`) -- per model type (e.g., Standard, Bionic). Default is `Both` (unrestricted) if no entry exists.
2. **Per-entity permission** (`savedPermissionsById`) -- overrides the default for a specific duplicant (stored by instance ID via `MinionAssignablesProxy`).

`GetSetPermission(primary_id, secondary_id)` checks per-entity overrides first, falling back to the default for the entity's model tag.

### Door Control States

`Door.ControlState` enum:

| Value | Name | Behavior |
|-------|------|----------|
| Auto | Auto | Door opens/closes automatically based on duplicant proximity |
| Opened | Opened | Door stays permanently open |
| Locked | Locked | Door stays permanently closed; access control status item is suppressed |

When the door is in `Locked` state, the access control status item is cleared (no "Access Control Active" status shown), since the door blocks everyone regardless of permissions.

Source: `Door.cs`, `AccessControl.SetStatusItem()`.

### Door Types

`Door.DoorType` enum:

| Value | Description |
|-------|-------------|
| Pressure | Blocks gas/liquid flow when closed |
| ManualPressure | Manual door that blocks gas/liquid |
| Internal | Does not block gas/liquid flow |
| Sealed | Starts sealed, requires a duplicant to unseal (one-time unlock) |

Source: `Door.cs`.

### Grid Registration

When an `AccessControl` component spawns, it registers restrictions on all cells occupied by the building via `Grid.RegisterRestriction()`. The orientation is determined by:
- Teleporters: `SingleCell` orientation
- Rotated buildings: `Horizontal` orientation
- Default: `Vertical` orientation

Restrictions are cleared when the building is deconstructed or the component is cleaned up.

### Copy Settings

Door access settings can be copied between buildings. `OnCopySettings` clears the target's saved permissions and copies both `savedPermissionsById` and `defaultPermissionByTag` from the source building.

## Daily Reports

`ReportManager` tracks colony statistics per cycle. A new `DailyReport` is created at the start of each cycle and finalized at nightfall (`OnNightTime`). Past reports are stored in a `dailyReports` list.

Source: `ReportManager.cs`.

### Report Types

`ReportManager.ReportType` enum, organized by display group:

**Group 1: Duplicant Details**

| Type | What It Tracks | Format |
|------|---------------|--------|
| CaloriesCreated | Food calories produced and consumed | kcal |
| StressDelta | Per-duplicant stress change | % |
| DiseaseAdded | New disease contraction events | count (hidden if zero) |
| DiseaseStatus | Current germ counts on duplicants | germ count |
| LevelUp | Skill level-ups this cycle | count (hidden if zero) |
| ToiletIncident | Bladder accidents | count (hidden if zero) |
| ChoreStatus | Chores completed vs. incomplete | count |
| DomesticatedCritters | Count of tamed critters | count |
| WildCritters | Count of wild critters | count |
| RocketsInFlight | Active rockets | count |

**Group 2: Time Spent**

Time values are stored in seconds and displayed as percentage of a 600-second cycle. The group format divides by the number of duplicants to show an average.

| Type | What It Tracks |
|------|---------------|
| WorkTime | Seconds spent on work errands |
| TravelTime | Seconds spent moving between tasks |
| PersonalTime | Seconds spent on personal needs (eating, sleeping, etc.) |
| IdleTime | Seconds spent idle (no available errands) |

**Group 3: Base Details**

| Type | What It Tracks | Format |
|------|---------------|--------|
| OxygenCreated | Net oxygen produced/consumed | kg |
| EnergyCreated | Energy generated vs. consumed | Joules |
| EnergyWasted | Energy wasted (generated but not consumed) | Joules |
| ContaminatedOxygenToilet | Polluted oxygen from toilets | kg (hidden if zero) |
| ContaminatedOxygenSublimation | Polluted oxygen from sublimation | kg (hidden if zero) |

### Report Entries

Each `ReportEntry` accumulates:
- `accPositive` -- sum of all positive contributions
- `accNegative` -- sum of all negative contributions
- `Net` = `accPositive + accNegative`

Entries can have `contextEntries` -- sub-entries keyed by a context string (typically a duplicant or building name), allowing drill-down into per-entity contributions.

Notes (descriptive strings attached to data points) are stored in a separate `NoteStorage` system using hash-based string deduplication.

### Report Lifecycle

1. `todaysReport` is created on save-game load with `day = GameUtil.GetCurrentCycle()`
2. Game systems call `ReportManager.Instance.ReportValue(type, value, note, context)` throughout the cycle
3. At nightfall (`OnNightTime` event, hash -722330267):
   - `todaysReport` is appended to `dailyReports`
   - A `ManagementMenuNotification` is created with the cycle number
   - A new `todaysReport` is created for the next cycle

## Colony Diagnostics

The colony diagnostic system provides real-time health indicators for each asteroid (world). `ColonyDiagnosticUtility` manages diagnostics per world, evaluating them every 4 seconds (`ISim4000ms`).

Source: `ColonyDiagnostic.cs`, `ColonyDiagnosticUtility.cs`.

### Diagnostic Severity Levels

`ColonyDiagnostic.DiagnosticResult.Opinion` enum (worst to best):

| Value | Meaning | Color |
|-------|---------|-------|
| Unset | No evaluation yet | -- |
| DuplicantThreatening | Immediate danger to duplicant life | Red |
| Bad | Serious problem | Red |
| Warning | Significant concern | Red |
| Concern | Minor concern | Yellow |
| Suggestion | Advisory | White |
| Tutorial | Tutorial-phase hint | White |
| Normal | No issues | Neutral |
| Good | Positive status | Green |

The worst (lowest numeric) opinion across all diagnostics for a world determines the world's aggregate status icon on the starmap.

### Registered Diagnostics

`ColonyDiagnosticUtility.AddWorld()` creates diagnostics per world. All worlds get:

| Diagnostic | What It Monitors |
|-----------|-----------------|
| IdleDiagnostic | Duplicants with no available errands |
| BreathabilityDiagnostic | Oxygen availability |
| FoodDiagnostic | Food supply status |
| StressDiagnostic | Duplicant stress levels |
| RadiationDiagnostic | Radiation exposure (Spaced Out) |
| ReactorDiagnostic | Nuclear reactor status (Spaced Out) |
| SelfChargingElectrobankDiagnostic | Self-charging electrobank status |
| BionicBatteryDiagnostic | Bionic battery levels (DLC3) |

**Planet-surface worlds only** (not rocket interiors):

| Diagnostic | What It Monitors |
|-----------|-----------------|
| BedDiagnostic | Bed availability |
| ToiletDiagnostic | Toilet availability |
| PowerUseDiagnostic | Power grid consumption vs. capacity |
| BatteryDiagnostic | Battery charge levels |
| TrappedDuplicantDiagnostic | Duplicants unable to reach essential resources |
| FarmDiagnostic | Farm tile status |
| EntombedDiagnostic | Entombed buildings |
| RocketsInOrbitDiagnostic | Rockets in orbit around this world |
| MeteorDiagnostic | Incoming meteor showers |

**Rocket interiors only:**

| Diagnostic | What It Monitors |
|-----------|-----------------|
| FloatingRocketDiagnostic | Rocket floating without destination |
| RocketFuelDiagnostic | Fuel reserves |
| RocketOxidizerDiagnostic | Oxidizer reserves |

### Display Settings

Each diagnostic per world has a `DisplaySetting`:

| Setting | Behavior |
|---------|----------|
| Always | Always visible in the diagnostics panel |
| AlertOnly | Only shown when opinion is below Normal |
| Never | Hidden completely |

Default display settings for new worlds:
- **Always visible**: Breathability, Food, Stress
- **AlertOnly**: Idle (on planet surfaces), most others
- **Never**: Idle (in rocket interiors)
- Rocket-specific diagnostics (Floating, Fuel, Oxidizer) default to Always in rocket interiors

### Tutorial Grace Period

Some diagnostics are suppressed during early gameplay via `diagnosticTutorialStatus`. Each entry maps a diagnostic ID to a game-time threshold (in seconds) before which the diagnostic is hidden:

| Diagnostic | Grace Period |
|-----------|-------------|
| ToiletDiagnostic | 450s (0.75 cycles) |
| BedDiagnostic | 900s (1.5 cycles) |
| IdleDiagnostic | 600s (1 cycle) |
| BreathabilityDiagnostic | 1800s (3 cycles) |
| FoodDiagnostic | 3000s (5 cycles) |
| FarmDiagnostic | 6000s (10 cycles) |
| StressDiagnostic | 9000s (15 cycles) |
| PowerUseDiagnostic | 12000s (20 cycles) |
| BatteryDiagnostic | 12000s (20 cycles) |

Source: `ColonyDiagnosticUtility.diagnosticTutorialStatus`.

## Notification System

Notifications are the game's primary alert mechanism. A `Notification` object carries a title, type, tooltip, and optional click target. Notifications flow through `Notifier` (per-entity) to `NotificationManager` (global singleton) to `NotificationScreen` (UI display).

Source: `Notification.cs`, `Notifier.cs`, `NotificationManager.cs`, `NotificationScreen.cs`.

### Notification Types

`NotificationType` enum:

| Value | Name | Behavior |
|-------|------|----------|
| 1 | Bad | Red alert, plays ding sound on duplicates |
| 2 | Good | Green notification |
| 3 | BadMinor | Minor negative alert |
| 4 | Neutral | Informational |
| 5 | Tutorial | Tutorial hint |
| 6 | Messages | Story/lore message |
| 7 | DuplicantThreatening | Immediate danger, plays ding sound on duplicates |
| 8 | Event | Event notification |
| 9 | MessageImportant | Important message variant |
| 10 | Custom | Custom notification with custom prefab |

### Notification Valence

`NotificationValence` enum (used for management menu notifications):
- Good, Neutral, BadMinor, Bad, DuplicantThreatening

### Dispatch Flow

1. A game system creates a `Notification` with title, type, tooltip function, and optional parameters (delay, click callback, expiry)
2. The system calls `Notifier.Add(notification)` on the relevant entity's `Notifier` component
3. `Notifier.Add()` sets the notification's `NotifierName` (from the entity's `KSelectable` name, prefixed with "-- "), sets `clickFocus` to the entity's transform (if `AutoClickFocus` is true), and forwards to `NotificationManager.Instance.AddNotification()`
4. `NotificationManager` places the notification in `pendingNotifications`
5. On each `Update()`, pending notifications with elapsed delay (`IsReady()` checks `Time.time >= GameTime + Delay`) are promoted to the active `notifications` list and fire the `notificationAdded` event
6. `NotificationScreen` groups notifications by title text. If multiple notifications share the same title, they are merged into one entry showing a count suffix (e.g., "Scalding! (3)"). `Bad` and `DuplicantThreatening` duplicates play an escalating ding sound

### Notification Properties

| Property | Default | Description |
|----------|---------|-------------|
| expires | true | Notification auto-removes after the screen's `lifetime` duration |
| playSound | true | Play notification sound on appearance |
| Delay | 0 | Seconds to wait before showing (real time) |
| clearOnClick | false | Remove notification when clicked |
| showDismissButton | false | Show an X button to manually dismiss |
| volume_attenuation | true | Attenuate sound based on camera distance to source |

Notifications without a `Notifier` (added directly to `NotificationManager`) can be removed via `Notification.Clear()`, which calls `NotificationManager.Instance.RemoveNotification()`.

### Tag Replacement

Notification titles support `{NotifierName}` tag replacement. When `NotifierName` is set, any `{NotifierName}` in the title text is replaced with the notifier entity's name.

## Vitals Panel

The `MinionVitalsPanel` displays tracked Amounts and Attributes for the selected entity. For duplicants, it shows the following values (only amounts that exist on the entity are displayed):

| Amount/Attribute | What It Shows |
|-----------------|--------------|
| HitPoints | Current HP |
| BionicInternalBattery | Bionic battery charge (DLC3) |
| BionicOil | Bionic oil level (DLC3) |
| BionicGunk | Bionic gunk buildup (DLC3) |
| Stress | Current stress percentage |
| QualityOfLife (attribute) | Current morale value |
| Bladder | Bladder fullness |
| Breath | Oxygen remaining |
| BionicOxygenTank | Bionic O2 tank level (DLC3) |
| Stamina | Sleep meter |
| Calories | Current calorie level |
| Temperature | Body temperature |
| Decor | Current decor exposure |
| RadiationBalance | Accumulated radiation (Spaced Out) |

For critters, additional amounts appear: Wildness, Incubation, Viability, Fertility, Age, ScaleGrowth, MilkProduction, ElementGrowth, and various battery types.

Source: `MinionVitalsPanel.Init()`.
