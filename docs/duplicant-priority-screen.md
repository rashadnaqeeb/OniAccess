# Duplicant Priority Screen (JobsTableScreen)

Research document covering the game's duplicant priority management screen. This is the screen opened from the management menu (sort key 22) where players assign per-duplicant priorities for each job category.

## Sighted User Experience

### Opening the Screen

The screen is accessed via the management menu bar at the top of the screen. It shows the title "MANAGE DUPLICANT PRIORITIES". It extends `TableScreen` which extends `ShowOptimizedKScreen`, so it participates in the standard management menu open/close flow via `ManagementMenu`.

### Layout

The screen is a scrollable table/grid:

- **Header row**: Column labels across the top. Each job column header shows the job name, a sort toggle, and a "set all" priority button.
- **"New Duplicants" row**: A default row (always first) that controls what priority new duplicants will arrive with. This row uses `Immigration.Instance` as its priority manager.
- **Duplicant rows**: One row per live duplicant, sorted within world dividers. Each row has a portrait, name label (click to select, double-click to focus), and one cell per job column.
- **World dividers**: In Spaced Out DLC, rows are grouped by asteroid/rocket with labeled dividers showing the planetoid name and icon.
- **Stored minion rows**: Duplicants in storage (rockets, etc.) appear but their priorities cannot be modified. Tooltip explains why.
- **Row priority buttons**: The rightmost column has up/down arrow buttons to raise/lower ALL priorities for that duplicant by 1.
- **Settings button**: In the header row, opens an options panel with:
  - "Reset Priorities" button
  - "Enable Proximity" toggle (advanced mode)

### Priority Cells

Each cell in the grid represents one duplicant's priority for one job category. The cell displays:

- **A directional arrow sprite** indicating priority level (disabled X, double-down, down, flat, up, double-up)
- **A colored fill** based on the duplicant's skill level for that job's attribute. Color ranges from neutral (no skill) through a gradient from `PrioritiesLowColor` to `PrioritiesHighColor` (skill 1-10)
- **Grayed out / non-interactive** if a trait disables that job for the duplicant

### Mouse Interaction on Cells

- **Left click**: Increase priority by 1 (wraps: 5 -> 0). Sound: "HUD_Click"
- **Right click**: Decrease priority by 1 (wraps: 0 -> 5). Sound: "HUD_Click_Deselect"
- **If trait-disabled**: Plays "Negative" sound, no change

### Column Header Interaction

- **Click sort toggle**: Sorts rows by that job's priority (highest first). Disabled jobs sort to bottom. Secondary sort by name. Clicking again reverses. Clicking a third time clears sort.
- **Click priority button**: Opens a dropdown panel with 6 items (Very High through Disallowed). Selecting one sets ALL duplicants' priority for that job to the chosen level.

### Row Priority Buttons (Up/Down Arrows)

- **Up arrow**: +1 to all job priorities for that duplicant (clamped, no wrap)
- **Down arrow**: -1 to all job priorities for that duplicant (clamped, no wrap)

### Tooltips

**Cell tooltip (for a live duplicant)**:
```
{Job} Priority for {Name}:
{Priority Name} Priority ({PriorityValue})

{Name}'s {Attribute} Skill: {value}

---
Left Click: Increase Priority
Right Click: Decrease Priority
```

**Cell tooltip (trait-disabled)**:
```
{Name} possesses the {Trait} trait and cannot do {Job} Errands
```

**Cell tooltip (stored minion)**:
```
Priorities for {Name} cannot be adjusted currently because they're in {StorageReason}
```

**Cell tooltip (new duplicant row)**:
```
The {Job} Errand Type is automatically a {Priority} Priority for Arriving Duplicants
```

**Column header tooltip**:
```
{Job} Errand Type

{Description}

Affected errands: {comma-separated list of chore type names}

Duplicants will first choose what Errand Type to perform based on Duplicant Priorities,
then they will choose individual tasks within that type using Building Priorities set by the Priority Tool
```

**Column header "set all" button tooltip**:
```
Set the priority for the {Job} Errand Type colonywide
```

**Sort button tooltip**:
```
Sort by the {Job} Errand Type
```

**Row up button tooltip**:
```
Prioritize All Errands for {Name}
```

**Row down button tooltip**:
```
Deprioritize All Errands for {Name}
```

**Name label tooltip**: Lists all skill attributes and their values, color-coded (green positive, red negative, white neutral).

### Settings Panel

**Enable Proximity toggle** (`advancedPersonalPriorities`):
When enabled, duplicants always choose the closest most-urgent errand. When disabled, they use a hidden priority hierarchy between same-priority errands. Tooltip text explains this is useful for large colonies.

**Reset Priorities**:
- If proximity enabled: Resets using `Immigration.Instance.ApplyDefaultPersonalPriorities()` (restores the database defaults)
- If proximity disabled: Resets all priorities to 3 (Standard)

## Priority Levels

There are 6 priority levels (0-5):

| Value | Name        | Sprite                   | Priority Value (for calculation) |
|-------|-------------|--------------------------|----------------------------------|
| 0     | Disallowed  | icon_priority_disabled   | 0                                |
| 1     | Very Low    | icon_priority_down_2     | 10                               |
| 2     | Low         | icon_priority_down       | 20                               |
| 3     | Standard    | icon_priority_flat       | 30 (default)                     |
| 4     | High        | icon_priority_up         | 40                               |
| 5     | Very High   | icon_priority_up_2       | 50                               |

There is also a 7th entry in the `priorityInfo` list with value 5 and sprite `icon_priority_automatic` (same "Very High" name) -- this appears to be an alternate display used for the proximity/advanced mode.

Setting priority to 0 ("Disallowed") does not delete the priority data; it just prevents the duplicant from doing that job type. The `IsPermittedByUser()` method returns false when priority is 0.

## Job Categories (ChoreGroups)

16 user-prioritizable job categories, defined in `Database/ChoreGroups.cs`. Listed with their default priority, associated attribute, and icon:

| ChoreGroup        | Default Priority | Attribute       | Icon                      |
|--------------------|-----------------|-----------------|---------------------------|
| Combat             | 5               | Digging         | icon_errand_combat        |
| Life Support       | 5               | LifeSupport     | icon_errand_life_support  |
| Toggle             | 5               | Toggle          | icon_errand_toggle        |
| Medical Aid        | 4               | Caring          | icon_errand_care          |
| Rocketry (DLC)     | 4               | SpaceNavigation | icon_errand_rocketry      |
| Basekeeping        | 4               | Strength        | icon_errand_tidy          |
| Cook               | 3               | Cooking         | icon_errand_cook          |
| Art                | 3               | Art             | icon_errand_art           |
| Research           | 3               | Learning        | icon_errand_research      |
| Machine Operating  | 3               | Machinery       | icon_errand_operate       |
| Farming            | 3               | Botanist        | icon_errand_farm          |
| Ranching           | 3               | Ranching        | icon_errand_ranch         |
| Build              | 2               | Construction    | icon_errand_toggle        |
| Dig                | 2               | Digging         | icon_errand_dig           |
| Hauling            | 1               | Strength        | icon_errand_supply        |
| Storage            | 1               | Strength        | icon_errand_storage       |

There is also a **Recreation** group (default priority 1) with `userPrioritizable = false`, so it does NOT appear in the priority screen.

Each `ChoreGroup` contains a list of `ChoreType` entries. The header tooltip lists all chore types in a group. Complete mapping from `Database/ChoreTypes.cs`:

| ChoreGroup | Chore Types |
|------------|-------------|
| Combat | Attack |
| Life Support | FetchCritical |
| Toggle | Toggle |
| Medical Aid | Doctor, Compound |
| Rocketry | RocketControl |
| Basekeeping | EmptyStorage, Mop, Disinfect, Repair, CleanToilet, EmptyDesalinator, Transport |
| Cook | Cook, CookFetch |
| Art | Art, AnalyzeArtifact, ExcavateFossil |
| Research | Research, AnalyzeArtifact, AnalyzeSeed, ExcavateFossil, ResearchFetch |
| Machine Operating | Astronaut, GeneratePower, PowerTinker, RemoteOperate, MachineTinker, LiquidCooledFan, IceCooledFan, Train, Fabricate, PowerFabricate, Depressurize, MachineFetch, PowerFetch |
| Farming | CropTend, Harvest, FarmingFabricate, FlipCompost, FarmFetch, AnalyzeSeed, BuildUproot, Uproot |
| Ranching | Capture, CreatureFetch, RanchingFetch, EggSing, ProcessCritter, Ranch, ArmTrap |
| Build | Deconstruct, Demolish, Build, BuildDig, BuildUproot |
| Dig | Dig, BuildDig, ExcavateFossil |
| Hauling | RanchingFetch, FetchCritical, MachineFetch, FarmFetch, BuildFetch, RepairFetch, DoctorFetch, CookFetch, PowerFetch, FabricateFetch, FoodFetch, EquipmentFetch, Transport |
| Storage | Fetch, StorageFetch |
| Recreation | Relax (not user-prioritizable) |

Note: Some chore types appear in multiple groups (e.g., AnalyzeArtifact is in both Art and Research, BuildDig is in both Build and Dig). Many chore types have no group at all (personal needs like eating, sleeping, stress reactions, etc.) -- these are handled by priority classes rather than personal priorities.

### Traits That Disable Job Categories

Certain traits prevent a duplicant from performing specific job categories entirely. The priority cell is grayed out and non-interactive for these combinations. From `TUNING/TRAITS.cs`:

| Trait | Disabled ChoreGroup |
|-------|-------------------|
| Pacifist (Joshua) | Combat |
| Scaredy Cat (ScaredyCat) | Combat |
| Hemophobia | Medical Aid |
| Can't Research (CantResearch) | Research |
| Can't Build (CantBuild) | Build |
| Can't Cook (CantCook) | Cook |
| Can't Dig (CantDig) | Dig |
| Uncultured | Art |

The columns in the screen are ordered by the order in `Db.Get().ChoreGroups.resources`. The code attempts to sort by `DefaultPersonalPriority` descending then by `Name`, but the LINQ result is not assigned back to the list (`_ = from @group ...`), so the actual order is the database registration order.

## Data Model

### ChoreConsumer (per-duplicant priority storage)

**File**: `ChoreConsumer.cs`

Each live duplicant has a `ChoreConsumer` component that stores priorities.

```
choreGroupPriorities: Dictionary<HashedString, PriorityInfo>  [Serialized]
```

Key constants:
- `DEFAULT_PERSONAL_CHORE_PRIORITY = 3`
- `MIN_PERSONAL_PRIORITY = 0`
- `MAX_PERSONAL_PRIORITY = 5`
- `PRIORITY_DISABLED = 0`
- `PRIORITY_VERYLOW = 1`
- `PRIORITY_LOW = 2`
- `PRIORITY_FLAT = 3`
- `PRIORITY_HIGH = 4`
- `PRIORITY_VERYHIGH = 5`

Key methods:
- `GetPersonalPriority(ChoreGroup)`: Returns 0-5, defaults to `DEFAULT_PERSONAL_CHORE_PRIORITY` (3) if not set
- `SetPersonalPriority(ChoreGroup, int)`: Clamps to 0-5, stores in dictionary, triggers `choreRulesChanged`
- `IsChoreGroupDisabled(ChoreGroup)`: Checks trait-based disable via `Traits.IsChoreGroupDisabled()`
- `IsPermittedByUser(ChoreGroup)`: Returns true if priority > 0
- `SetPermittedByUser(ChoreGroup, bool)`: If disabling, sets priority to 0. If enabling and currently 0, sets to `DEFAULT_PERSONAL_CHORE_PRIORITY`
- `UpdateChoreTypePriorities()`: Called after priority changes to update the per-chore-type priority cache used by the task selection algorithm
- `GetAssociatedSkillLevel(ChoreGroup)`: Returns the duplicant's skill level for the group's attribute (used for cell coloring)

### IPersonalPriorityManager Interface

Implemented by three types:
1. **ChoreConsumer** - Live duplicants
2. **Immigration** - The "New Duplicants" default row
3. **StoredMinionIdentity** - Duplicants in storage (read-only; `SetPersonalPriority` is a no-op)

Methods:
- `GetPersonalPriority(ChoreGroup)`
- `SetPersonalPriority(ChoreGroup, int)`
- `IsChoreGroupDisabled(ChoreGroup)`
- `GetAssociatedSkillLevel(ChoreGroup)`
- `ResetPersonalPriorities()`

### Priority Wrapping/Clamping

**Cell clicks**: Use modulo 6 wrapping (`priority % 6`, with negative adjustment). So 5+1=0, 0-1=5.

**Row up/down buttons**: Use `Mathf.Clamp(0, 5)` with NO wrapping. So if a job is already at 5 and you press up, it stays at 5.

### How Priorities Affect Duplicant Behavior

Priorities flow into the chore selection system via `Chore.Context.CompareTo()` (`Chore.cs` lines 173-233). The comparison order (highest precedence first):

1. **Failed preconditions** -- chores with failed preconditions always sort last
2. **Master priority class** (`PriorityClass`): idle < basic < high < personalNeeds < topPriority < compulsory
3. **Personal priority** (0-5 from this screen) -- the user-set duplicant priority
4. **Master priority value** (1-9, the building priority set with the priority tool)
5. **Chore type priority** -- this is where advanced mode changes behavior (see below)
6. **Priority modifier** (various bonuses from `priorityMod`)
7. **Consumer priority** (additional modifiers)
8. **Navigation cost** (distance to task)
9. **Chore ID** (tiebreaker)

### What "Enable Proximity" (Advanced Mode) Actually Does

The toggle controls `Game.Instance.advancedPersonalPriorities`. From `Chore.cs` `SetPriority()` (line 92-97):

```csharp
priority = Game.Instance.advancedPersonalPriorities
    ? chore.choreType.explicitPriority
    : chore.choreType.priority;
```

- **Off (default)**: Uses `choreType.priority` -- an implicit priority assigned by chore creation order. This creates a hidden hierarchy where certain chore types within the same group are always preferred over others.
- **On**: Uses `choreType.explicitPriority` -- explicitly assigned priorities that default to the same implicit value but can be overridden. In practice, this flattens the hidden hierarchy so duplicants choose more by proximity when priorities are otherwise equal.

The personal priority (step 3) is NOT affected by this toggle. It only changes step 5 -- how ties within the same personal priority level are broken. The tooltip explains: "Enabling Proximity helps cut down on travel time in areas with lots of high priority errands."

## CrewJobsScreen (Alternate/Simple View)

There is a SECOND priority-related screen: `CrewJobsScreen` extending `CrewListScreen<CrewJobsEntry>`. This is a **simpler toggle-based view** where each job is either enabled or disabled (on/off), not 0-5 graded. It uses `SetPermittedByUser(group, bool)` rather than `SetPersonalPriority(group, int)`.

**This screen is NOT the one referenced by ManagementMenu.jobsScreen** -- `ManagementMenu` references `JobsTableScreen`. The `CrewJobsScreen` may be an older version, an alternate overlay, or used in a different context.

`CrewJobsEntry` features:
- One button per ChoreGroup that toggles enabled/disabled
- An "All Tasks" button that toggles all jobs on/off
- Color-coded borders based on skill level (pink-purple gradient)
- Per-row toggle state tracking (on/mixed/off)
- Column-level "Everyone" toggles that set all duplicants' permission for a job
- Sorting by attribute effectiveness per column

## Screen Lifecycle

### Opening
1. `ManagementMenu` activates `JobsTableScreen`
2. `OnActivate()` sets title, creates columns (portrait, names, one per ChoreGroup, row priority)
3. `RefreshRows()` creates rows for header, default, each live minion, each stored minion, and world dividers
4. Effect listeners are registered to update rows when duplicants level up, get effects/diseases

### Refreshing
- `ScreenUpdate()` runs every frame when visible
- Checks for dirty rows, refreshes scrollers
- Individual minion rows are marked dirty on level-up, effect changes, disease changes
- `dirty_single_minion_rows` HashSet tracks which specific rows need refreshing

### Closing
- `OnCmpDisable()` clears EventSystem selection
- Closes all sub-panels (GroupSelectorWidget, GroupSelectorHeaderWidget, SelectablePanel)
- Hides the options panel

## Programmatic Cell Access

### Reading Priority from a Cell

The `OptionSelector` widget does **not** store the priority value. It is a stateless click handler. To read priority programmatically:

```csharp
// Get the duplicant's ChoreConsumer (or IPersonalPriorityManager)
IAssignableIdentity identity = row.GetIdentity();
MinionIdentity minion = identity as MinionIdentity;
ChoreConsumer consumer = minion.GetComponent<ChoreConsumer>();
int priority = consumer.GetPersonalPriority(choreGroup);  // returns 0-5
```

For the default row, priority is read from `Immigration.Instance` (implements `IPersonalPriorityManager`).
For stored minions, use `StoredMinionIdentity` which also implements `IPersonalPriorityManager`.

### Invoking Priority Changes Programmatically

The `OptionSelector` dispatches clicks through an `OnChangePriority(object id, int delta)` callback. To change priority:

```csharp
// Option 1: call the OptionSelector's callback directly
OptionSelector selector = widget.GetComponent<OptionSelector>();
selector.OnChangePriority(widget, 1);   // increment
selector.OnChangePriority(widget, -1);  // decrement
```

The callback is wired in `PrioritizationGroupTableColumn` to delegate to `JobsTableScreen`'s handler, which calls `ChoreConsumer.SetPersonalPriority()` with wrapping (mod 6).

### Row Up/Down Buttons

The rightmost column (`PrioritizeRowTableColumn`) has two child buttons accessed via `HierarchyReferences`:

- `"UpButton"` — `KButton`, calls `onChangePriority(widget, +1)` (clamped, no wrap)
- `"DownButton"` — `KButton`, calls `onChangePriority(widget, -1)` (clamped, no wrap)

### OptionSelector Widget Hierarchy

Each priority cell is an `OptionSelector` prefab (`Assets.UIPrefabs.TableScreenWidgets.PriorityGroupSelector`). Child elements accessed via `HierarchyReferences` on the `selectedItem` KImage:

| Reference | Type | Purpose |
|-----------|------|---------|
| `"BG"` | KImage | Background sprite (arrow direction icon) |
| `"FG"` | KImage | Foreground sprite overlay |
| `"Fill"` | KImage | Color fill based on duplicant's skill level for the job |
| `"Outline"` | KImage | Outline, disabled when trait prevents the job |

The `selectedItem` field is a `KImage` with an attached `KButton` that dispatches left-click (+1) and right-click (-1) via `OnClick(KKeyCode)`.

## Column Header "Set All" Dropdown

Each column header has a priority button that opens a dropdown to set all duplicants' priority for that job.

### Structure

The dropdown is **not** the `GroupSelectorHeaderWidget` component. `JobsTableScreen.InitializeHeader()` builds it manually:

1. A `KButton` labeled "PrioritizeButton" in the header widget
2. Clicking opens an `itemsPanel` (`RectTransform`, toggled active/inactive) with a `GridLayoutGroup`
3. The panel contains 6 dynamically created buttons, one per priority level (0–5)
4. Each button shows the priority's arrow sprite icon

### Selection Behavior

Clicking a priority button in the dropdown calls `ChangeColumnPriority(widget_go, new_priority)`, which sets every duplicant's priority for that chore group to the selected level.

### Closing

The panel uses a `SelectablePanel` component that auto-closes via `OnDeselect()` when focus leaves. Also closed explicitly in `OnCmpDisable()`.

## Settings Panel

### Fields

```csharp
[SerializeField] private KImage optionsPanel;
[SerializeField] private KButton resetSettingsButton;
[SerializeField] private KButton toggleAdvancedModeButton;
private KButton settingsButton;  // set dynamically in ConfigureOptionsPanel()
```

### Reaching the Settings Button from a Handler

`settingsButton` is **private** and assigned dynamically in `ConfigureOptionsPanel()`:

```csharp
private void ConfigureOptionsPanel()
{
    HierarchyReferences component = header_row.GetComponent<HierarchyReferences>();
    settingsButton = component.GetReference<KButton>("OptionsButton");
    settingsButton.ClearOnClick();
    settingsButton.onClick += OnSettingsButtonClicked;
}
```

`header_row` is a **protected** field on `TableScreen`, so an external handler can reach the button via the Unity hierarchy without reflection:

```csharp
screen.header_row.GetComponent<HierarchyReferences>().GetReference<KButton>("OptionsButton")
```

This is more robust than reflecting `settingsButton` since the field is re-fetched from the hierarchy on every `RefreshRows()` anyway.

### Opening/Closing

- **Toggle**: `settingsButton.onClick` calls `OnSettingsButtonClicked()`, which activates `optionsPanel.gameObject` and selects it
- **Closing**: `optionsPanel.gameObject.SetActive(false)` in `OnCmpDisable()`

### Reset Priorities (`OnResetSettingsClicked`)

Behavior depends on advanced mode:
- **If `advancedPersonalPriorities` is true**: calls `Immigration.Instance.ResetPersonalPriorities()` and applies database defaults to all duplicants
- **If false**: sets all duplicant priorities to 3 (Standard) for every prioritizable chore group

### Advanced Mode Toggle (`OnAdvancedModeToggleClicked`)

Toggles `Game.Instance.advancedPersonalPriorities` and updates a visual indicator on the button. This controls whether chore selection uses implicit priority hierarchy or explicit (proximity-based) ordering.

## Key Source Files

| File | Purpose |
|------|---------|
| `Assembly-CSharp/JobsTableScreen.cs` (986 lines) | Main priority screen, all UI logic |
| `Assembly-CSharp/TableScreen.cs` (998 lines) | Base class: row management, sorting, scrolling |
| `Assembly-CSharp/ChoreConsumer.cs` (654 lines) | Per-duplicant priority storage and chore selection |
| `Assembly-CSharp/IPersonalPriorityManager.cs` (12 lines) | Interface for priority management |
| `Assembly-CSharp/ChoreGroup.cs` (31 lines) | Job category definition |
| `Assembly-CSharp/Database/ChoreGroups.cs` (88 lines) | All 16+ job categories registered |
| `Assembly-CSharp/PrioritizationGroupTableColumn.cs` (92 lines) | Column type for priority cells |
| `Assembly-CSharp/PrioritizeRowTableColumn.cs` (57 lines) | Column type for row up/down buttons |
| `Assembly-CSharp/OptionSelector.cs` (87 lines) | Priority cell widget (handles clicks, displays sprite) |
| `Assembly-CSharp/PrioritySetting.cs` | Priority struct for building priorities |
| `Assembly-CSharp/Immigration.cs` | Default priority management for new duplicants |
| `Assembly-CSharp/StoredMinionIdentity.cs` | Read-only priority access for stored duplicants |
| `Assembly-CSharp/CrewJobsScreen.cs` (368 lines) | Alternate simple toggle-based priority view |
| `Assembly-CSharp/CrewJobsEntry.cs` (281 lines) | Per-duplicant row in the simple view |
| `Assembly-CSharp/STRINGS/UI.cs` (JOBSSCREEN section) | All localized text for the screen |

## Game Strings Reference

Priority names:
- `UI.JOBSSCREEN.PRIORITY.VERYHIGH` = "Very High"
- `UI.JOBSSCREEN.PRIORITY.HIGH` = "High"
- `UI.JOBSSCREEN.PRIORITY.STANDARD` = "Standard"
- `UI.JOBSSCREEN.PRIORITY.LOW` = "Low"
- `UI.JOBSSCREEN.PRIORITY.VERYLOW` = "Very Low"
- `UI.JOBSSCREEN.PRIORITY.DISABLED` = "Disallowed"

Priority classes (for the broader chore system):
- `UI.JOBSSCREEN.PRIORITY_CLASS.IDLE` = "Idle"
- `UI.JOBSSCREEN.PRIORITY_CLASS.BASIC` = "Normal"
- `UI.JOBSSCREEN.PRIORITY_CLASS.HIGH` = "Urgent"
- `UI.JOBSSCREEN.PRIORITY_CLASS.PERSONAL_NEEDS` = "Personal Needs"
- `UI.JOBSSCREEN.PRIORITY_CLASS.EMERGENCY` = "Emergency"
- `UI.JOBSSCREEN.PRIORITY_CLASS.COMPULSORY` = "Involuntary"

Row labels:
- `UI.JOBSCREEN_EVERYONE` = "Everyone" (header row label)
- `UI.JOBSCREEN_DEFAULT` = "New Duplicants" (default row label)

Screen title:
- `UI.JOBSSCREEN.TITLE` = "MANAGE DUPLICANT PRIORITIES"

Settings:
- `UI.JOBSSCREEN.TOGGLE_ADVANCED_MODE` = "Enable Proximity"
- `UI.JOBSSCREEN.RESET_SETTINGS` = "Reset Priorities"

## TableScreen Internals

### columns Dictionary

`TableScreen.columns` is **protected** `Dictionary<string, TableColumn>`. No public accessor exists. External handlers must use reflection or access via a Harmony patch / subclass. However, `TableColumn.widgets_by_row` is **public**, so once you have a column reference you can freely read cell widgets.

### Row Ordering

`all_sortable_rows` **reflects the current visual sort order** after `SortRows()` runs. The method clears the list, re-sorts per world group using `active_sort_method` (reversing if `sort_is_reversed`), then rebuilds `all_sortable_rows` in sorted order and calls `SetSiblingIndex()` on each row to match. When no sort is active, rows are grouped by world in insertion order.

### Column Ordering

`columns` is a plain `Dictionary<string, TableColumn>`. On .NET Framework 4.7.2 / Mono, `Dictionary` preserves insertion order when no removals occur, and `columns` is only ever added to (via `RegisterColumn`). Columns are registered in `OnActivate()` in left-to-right visual order. So iterating `columns.Values` matches visual order in practice, though this is an implementation detail, not a contractual guarantee.
