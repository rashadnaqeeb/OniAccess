# Vitals Screen - Technical Analysis

## Overview

The Vitals Screen (`VitalsTableScreen`) is a table-based management screen showing all duplicants' health stats in a grid. It extends `TableScreen` which extends `ShowOptimizedKScreen`, and is opened from the `ManagementMenu` with hotkey `Action.ManageVitals`. Title: `UI.VITALS`.

Source: `ONI-Decompiled/Assembly-CSharp/VitalsTableScreen.cs` (772 lines)

## Columns

| # | Column | Type | Width | Data Source | Sortable |
|---|--------|------|-------|-------------|----------|
| 1 | Portrait | `PortraitTableColumn` | -- | `CrewPortrait` | No |
| 2 | Name | `ButtonLabelColumn` | -- | `IAssignableIdentity` | Yes (alpha) |
| 3 | Stress | `LabelTableColumn` | 64px | `Db.Get().Amounts.Stress` | Yes (desc) |
| 4 | QoL Expectations | `LabelTableColumn` | 64px | `Db.Get().Attributes.QualityOfLife` | Yes |
| 5 | Power Banks | `LabelTableColumn` | 64px | `Db.Get().Amounts.BionicInternalBattery` | Yes (DLC3 only) |
| 6 | Fullness | `LabelTableColumn` | 96px | `Db.Get().Amounts.Calories` | Yes |
| 7 | Health | `LabelTableColumn` | 64px | `Db.Get().Amounts.HitPoints` | Yes |
| 8 | Immunity | `LabelTableColumn` | 192px | `MinionModifiers.sicknesses` | No |

Columns 1-2 are fixed (non-scrollable). Columns 3-8 are in a scrollable region.

## Row Types

- **Header** -- Column headers with sort toggles
- **Minion** -- One row per live duplicant from `Components.LiveMinionIdentities`
- **StoredMinion** -- Frozen/cryofrozen duplicants (60% opacity, all stats show "N/A")
- **WorldDivider** -- Asteroid separators in multi-world saves

`has_default_duplicant_row = false` -- no "New Duplicants" default row on this screen.

## Data Display Details

All columns use `AmountInstance.GetValueString()` or `AttributeInstance.GetFormattedValue()`. The exact display formats are:

### Stress
`Db.Get().Amounts.Stress.Lookup(minion).GetValueString()` -- percentage string, e.g. `"15%"`.

### QoL Expectations
`Db.Get().Attributes.QualityOfLife.Lookup(minion).GetFormattedValue()` -- formatted attribute with sign, e.g. `"+3"`. Note: this uses `AttributeInstance.GetFormattedValue()`, not `AmountInstance.GetValueString()`.

### Power Banks (DLC3 only)
`Db.Get().Amounts.BionicInternalBattery` formatted as joules. Shows "N/A" for non-bionic dupes. Conditionally added via `Game.IsDlcActiveForCurrentSave("DLC3_ID")`.

### Fullness / Calories
`Db.Get().Amounts.Calories.Lookup(minion).GetValueString()` -- calorie display, e.g. `"3500 kcal"`. Tooltip includes calories eaten today via `UI.VITALSSCREEN.EATEN_TODAY_TOOLTIP`.

### Health
`Db.Get().Amounts.HitPoints.Lookup(minion).GetValueString()` -- current/max format, e.g. `"100 / 100"`.

### Immunity
The display string is built from `MinionModifiers.sicknesses` plus optional radiation data:

- **Healthy**: `"No diseases"` (`UI.VITALSSCREEN.NO_SICKNESSES`)
- **Single sickness**: `"{SicknessName}\n({FormattedCycles})"` via `UI.VITALSSCREEN.SICKNESS_REMAINING` (format: `"{0}\n({1})"`)
  - Example: `"Food Poisoning\n(1.2 Cycles)"`
- **Multiple sicknesses**: `"Multiple diseases ({FormattedCycles})"` via `UI.VITALSSCREEN.MULTIPLE_SICKNESSES` (format: `"Multiple diseases ({0})"`)
  - Shows the minimum remaining time across all sicknesses. Note: the game code has a bug where `Mathf.Min()` is called with a single argument per iteration, so it actually shows the *last* sickness's time, not the true minimum.

Radiation sickness (DLC) is checked separately via `RadiationMonitor.Instance` and appended to the sickness list before formatting.

All stored minions show `UI.TABLESCREENS.NA` (`"N/A"`) for every stat column.

## Interactions

- **Click/double-click name or portrait**: Select duplicant / select and focus camera
- **Click column header sort toggle**: Three-state cycle per column:
  1. First click: ascending (toggle state 1)
  2. Second click on same column: descending (toggle state 2)
  3. Third click on same column: clears sort entirely (`active_sort_column` and `active_sort_method` set to null)
  - Clicking a different column starts fresh at ascending
- **Sort state**: `active_sort_column` + `sort_is_reversed` tracked in `TableScreen`
- **Close**: Escape via `ManagementMenu.Instance.CloseAll()`
- No duplicant-specific hotkeys on this screen

## Code Architecture

### Activation Flow

1. `ManagementMenu` opens `VitalsTableScreen`
2. `OnActivate()` sets title, calls `base.OnActivate()`, adds all columns
3. `base.OnActivate()` subscribes to `Components.LiveMinionIdentities.OnAdd/OnRemove` which triggers `MarkRowsDirty()`

### Refresh Cycle

- `ScreenUpdate()` called each frame -- if `rows_dirty`, calls `RefreshRows()`
- `RefreshRows()` rebuilds all rows from `Components.LiveMinionIdentities` + `Components.MinionStorages` + world dividers, then `SortRows()`
- Columns marked dirty via `IRender1000ms` (every 1000ms game time). `ScreenUpdate()` then re-runs `on_load_action` for dirty columns

### Row Population (RefreshRows)

```csharp
// 1. All live minions
for (int i = 0; i < Components.LiveMinionIdentities.Count; i++) {
    AddRow(Components.LiveMinionIdentities[i]);
}

// 2. All stored minions
foreach (MinionStorage item2 in Components.MinionStorages.Items) {
    foreach (MinionStorage.Info item3 in item2.GetStoredMinionInfo()) {
        StoredMinionIdentity minion = item3.serializedMinion.Get<StoredMinionIdentity>();
        AddRow(minion);
    }
}

// 3. World dividers for multi-world saves
foreach (int worldId in ClusterManager.Instance.GetWorldIDsSorted()) {
    AddWorldDivider(worldId);
}

SortRows();
```

### Sorting

```csharp
SetSortComparison(comparer, column)
// Three-state cycle:
// 1. New column: sets active_sort_column, sort_is_reversed = false (ascending)
// 2. Same column, not yet reversed: sort_is_reversed = true (descending)
// 3. Same column, already reversed: clears both active_sort_column and active_sort_method (no sort)

SortRows()
// Re-orders all_sortable_rows using active_sort_method
// Updates column_sort_toggle visual states (0=unsorted, 1=ascending, 2=descending)
// Rows are sorted per-world: each asteroid's duplicants are sorted independently
```

### Comparison Functions

Each sortable column has a dedicated comparer. Pattern:

```csharp
private int compare_rows_stress(IAssignableIdentity a, IAssignableIdentity b) {
    MinionIdentity minionA = a as MinionIdentity;
    MinionIdentity minionB = b as MinionIdentity;
    if (minionA == null && minionB == null) return 0;
    if (minionA == null) return -1;   // Stored minions sort first
    if (minionB == null) return 1;
    float valueA = Db.Get().Amounts.Stress.Lookup(minionA).value;
    float valueB = Db.Get().Amounts.Stress.Lookup(minionB).value;
    return valueB.CompareTo(valueA);  // Descending (highest stress first)
}
```

## Tooltip System

Each column has dedicated tooltip delegates:

- **Stress**: Full breakdown from `Amounts.Stress.Lookup(minion).GetTooltip()`
- **QoL**: Attribute value tooltip from `Attributes.QualityOfLife.Lookup(minion).GetAttributeValueTooltip()`
- **Calories**: Full calorie tooltip plus "eaten today" via `UI.VITALSSCREEN.EATEN_TODAY_TOOLTIP`
- **Health**: `Amounts.HitPoints.Lookup(minion).GetTooltip()`
- **Immunity**: Multi-line list of each sickness name + status item tooltip + time remaining. If radiation DLC enabled, checks `RadiationMonitor.Instance` separately
- **Stored minion**: `UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP` with storage reason and duplicant name

## Key Source Files

| File | Lines | Purpose |
|------|-------|---------|
| `VitalsTableScreen.cs` | 772 | Main screen with all column definitions |
| `TableScreen.cs` | 600+ | Base table framework, row management, sorting |
| `TableRow.cs` | 200+ | Row representation and widget management |
| `TableColumn.cs` | 136 | Base column class |
| `LabelTableColumn.cs` | 66 | Label cell (used for all vital stat columns) |
| `ButtonLabelColumn.cs` | 67 | Clickable label (Name column) |
| `PortraitTableColumn.cs` | 45 | Portrait image column |
| `MinionVitalsPanel.cs` | 736 | Separate details panel (not the table screen) |
| `ManagementMenu.cs` | 500+ | Menu integration point |

## Localization Strings

### Column Headers
- `UI.VITALS` -- Screen title
- `UI.VITALSSCREEN.STRESS` -- Stress column
- `UI.VITALSSCREEN.QUALITYOFLIFE_EXPECTATIONS` -- QoL column
- `UI.VITALSSCREEN_HEALTH` -- Health column
- `UI.VITALSSCREEN_CALORIES` -- Fullness column
- `UI.VITALSSCREEN_POWERBANKS` -- Power Banks column

### Data Strings
- `UI.VITALSSCREEN.MULTIPLE_SICKNESSES` -- "Multiple sicknesses: {time}"
- `UI.VITALSSCREEN.SICKNESS_REMAINING` -- "{disease}: {time}"
- `UI.VITALSSCREEN.NO_SICKNESSES` -- Healthy status text
- `UI.TABLESCREENS.NA` -- "N/A" for stored minions
- `UI.VITALSSCREEN.EATEN_TODAY_TOOLTIP` -- Calories eaten today

### Sort Tooltips
- `UI.TABLESCREENS.COLUMN_SORT_BY_STRESS`
- `UI.TABLESCREENS.COLUMN_SORT_BY_EXPECTATIONS`
- `UI.TABLESCREENS.COLUMN_SORT_BY_HITPOINTS`
- `UI.TABLESCREENS.COLUMN_SORT_BY_SICKNESSES`
- `UI.TABLESCREENS.COLUMN_SORT_BY_FULLNESS`
- `UI.TABLESCREENS.COLUMN_SORT_BY_POWERBANKS`

### Other
- `UI.TABLESCREENS.GOTO_DUPLICANT_BUTTON` -- "Go to {name}" tooltip
- `UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP` -- Stored minion tooltip

## Dead Code: "Eaten Today" Column

`VitalsTableScreen` defines methods for an "Eaten Today" column (`on_load_eaten_today`, `get_value_eaten_today_label`, `compare_rows_eaten_today`, etc.) and the string `UI.VITALSSCREEN_EATENTODAY`. However, this column is NOT added in `OnActivate()`. It is dead code in the current game version. The data source would be `RationMonitor.Instance.GetRationsAteToday()` via a helper `RationsEatenToday(MinionIdentity)`. The "eaten today" data IS still surfaced in the Fullness column tooltip.

## Accessing Row Data from the Handler

Key public fields on `TableScreen`:
- `rows` (`List<TableRow>`) -- all rows including header
- `all_sortable_rows` (`List<TableRow>`) -- only minion/stored minion rows (no header, no world dividers)
- `columns` (`Dictionary<string, TableColumn>`) -- keyed by column name strings: `"Portrait"`, `"Names"`, `"Stress"`, `"QOLExpectations"`, `"PowerBanks"`, `"Fullness"`, `"Health"`, `"Immunity"`
- `active_sort_column` (`TableColumn`) -- currently sorted column, null if unsorted
- `sort_is_reversed` (`bool`) -- descending if true

Key fields on `TableRow`:
- `GetIdentity()` returns `IAssignableIdentity` (cast to `MinionIdentity` for live dupes, `StoredMinionIdentity` for frozen)
- `rowType` -- `Header`, `Default`, `Minion`, `StoredMinon`, `WorldDivider`
- `widgets` (`Dictionary<TableColumn, GameObject>`) -- private, maps each column to its cell widget

Key fields on `TableColumn`:
- `widgets_by_row` (`Dictionary<TableRow, GameObject>`) -- public, maps each row to its cell widget
- `on_load_action` -- populate callback
- `sort_comparer` -- sort function
- `column_sort_toggle` -- `MultiToggle` for sort state (states: 0=unsorted, 1=ascending, 2=descending)

To read a specific cell's text: get the column from `columns["Stress"]`, get the widget from `widgets_by_row[row]`, then `widget.GetComponentInChildren<LocText>().text`.

## World Dividers Detail

World dividers are more complex than simple separators:
- A special divider at ID 255 is always added (catch-all for duplicants with no world)
- Dividers are hidden unless `DlcManager.FeatureClusterSpaceEnabled()` and the world is discovered
- Each divider has a "NobodyRow" child that shows when no duplicants are on that asteroid
- In base game (non-Spaceout DLC), dividers are invisible since cluster space is disabled

## Handler Registration

`VitalsTableScreen` is NOT currently registered in `ContextDetector.RegisterMenuHandlers()`. It uses normal `KScreen.Activate/Deactivate` lifecycle (not Show-patched). Registration would follow this pattern:

```csharp
Register<VitalsTableScreen>(screen => new VitalsScreenHandler(screen));
```

No `_showPatchedTypes.Add()` needed -- `TableScreen` uses standard activate/deactivate.

## TableScreen Internals

### columns Dictionary

`TableScreen.columns` is **protected** `Dictionary<string, TableColumn>`. No public accessor exists. External handlers must use reflection. However, `TableColumn.widgets_by_row` is **public**, so once you have a column reference you can freely read cell widgets.

### Row Ordering

`all_sortable_rows` **reflects the current visual sort order** after `SortRows()` runs. The method clears the list, re-sorts per world group using `active_sort_method` (reversing if `sort_is_reversed`), then rebuilds `all_sortable_rows` in sorted order and calls `SetSiblingIndex()` on each row to match. When no sort is active, rows are grouped by world in insertion order.

### Column Ordering

`columns` is a plain `Dictionary<string, TableColumn>`. On .NET Framework 4.7.2 / Mono, `Dictionary` preserves insertion order when no removals occur, and `columns` is only ever added to (via `RegisterColumn`). Columns are registered in `OnActivate()` in left-to-right visual order:

```csharp
// VitalsTableScreen.OnActivate() registration order:
AddPortraitColumn("Portrait", ...);
AddButtonLabelColumn("Names", ...);
AddLabelColumn("Stress", ...);
AddLabelColumn("QOLExpectations", ...);
// conditionally: AddLabelColumn("PowerBanks", ...);
AddLabelColumn("Fullness", ...);
AddLabelColumn("Health", ...);
AddLabelColumn("Immunity", ...);
```

Iterating `columns.Values` matches visual order in practice, though this is an implementation detail of `Dictionary`, not a contractual guarantee.

## Handler Design Considerations

1. **Row-by-row navigation** -- users want all stats for one duplicant at a time, not column-first
2. **Live data only** -- always re-query `Db.Get().Amounts/Attributes`, never cache values
3. **DLC3 Power Banks column** is conditional -- must detect at activation time via `columns.ContainsKey("PowerBanks")`
4. **World dividers** in multi-world saves need to be announced or skipped. Use `all_sortable_rows` to get only duplicant rows
5. **Stored minions** need clear differentiation (all stats are "N/A" with a storage reason)
6. **Sort state** should be announced when changed (three-state: ascending, descending, cleared)
7. **MinionVitalsPanel** is a different class for the details screen, not the management table
