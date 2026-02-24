# Consumables Screen — Full Reference

The consumables screen lets the player control which food, medicine, batteries, and oxygen tanks each duplicant is permitted to consume. It is a management-menu table: rows are duplicants, columns are consumable items, and each cell is a permission checkbox.

## Opening the Screen

The screen is accessed from the `ManagementMenu` toolbar at the top of the game UI. It is stored as `ManagementMenu.Instance.consumablesScreen`. Clicking the consumables button opens the screen; clicking it again or pressing Escape closes it.

**Class**: `ConsumablesTableScreen` (extends `TableScreen`, which extends `ShowOptimizedKScreen`)

## Visual Layout

The screen is a scrollable table grid:

```
+-------------+--------+-----+---+--------+--------+---+--------+--------+---+
| Portrait    | Name   | QoL | | Food A | Food B |...| Med X  | Med Y  |...|
+-------------+--------+-----+---+--------+--------+---+--------+--------+---+
| [toggle-all]|        |     | | [sup]  | [sup]  |...| [sup]  | [sup]  |...|
+-------------+--------+-----+---+--------+--------+---+--------+--------+---+
| (default)   |New Mins|     | | [ ]    | [x]    |...| [x]    | [ ]    |...|
+-------------+--------+-----+---+--------+--------+---+--------+--------+---+
| [portrait]  | Meep   | 3   | | [x]    | [x]    |...| [ ]    | [x]    |...|
| [portrait]  | Bubbles| 5   | | [x]    | [ ]    |...| [x]    | [x]    |...|
| ...         | ...    | ... | | ...    | ...    |...| ...    | ...    |...|
+-------------+--------+-----+---+--------+--------+---+--------+--------+---+
```

### Fixed Columns (Left Side)

1. **Portrait** — duplicant avatar image (`PortraitTableColumn`)
2. **Name** — clickable button label; clicking selects the duplicant (`ButtonLabelColumn`)
3. **QoL Expectations** — quality-of-life expectation value (`LabelTableColumn`); shows "N/A" for stored minions

### Scrollable Columns (Right Side)

Wrapped in a horizontal scroller with ID `"consumableScroller"`. The scrollbar appears when 12 or more consumables have been discovered.

4. **Consumable columns** — one `ConsumableInfoTableColumn` per discovered consumable. Each column has:
   - A header icon/label showing the consumable name
   - A super-checkbox in the header area (toggle all duplicants for this item)
   - One checkbox per duplicant row
   - One checkbox for the "New Minions" default row
5. **Divider columns** — visual spacers between food quality groups (`DividerColumn`). Inserted whenever `MajorOrder` changes between adjacent consumables.

## Row Types

Defined by `TableRow.RowType`:

| RowType | Description |
|---------|-------------|
| `Header` | Column headers with consumable icons and names, plus super-checkboxes |
| `Default` | "New Minions" row — sets default permissions for future duplicants |
| `Minion` | One row per active duplicant |
| `StoredMinon` | Cryofrozen or otherwise stored duplicants (rendered at 60% opacity, permissions viewable but editing shows a storage-reason tooltip) |
| `WorldDivider` | Separator between asteroid worlds in multi-colony games |

## Consumable Types

All consumables implement `IConsumableUIItem`:

```csharp
public interface IConsumableUIItem
{
    string ConsumableId { get; }      // Prefab tag name
    string ConsumableName { get; }    // Localized display name
    int MajorOrder { get; }           // Primary sort key
    int MinorOrder { get; }           // Secondary sort key
    bool Display { get; }             // Whether to show in UI
}
```

### Food (`EdiblesManager.FoodInfo`)

- `MajorOrder` = food quality level (determines column group)
- `MinorOrder` = calories per unit (determines order within group)
- `Display` = false for ingredients with 0 calories (hidden)
- Quality ranges from -1 (worst) to 5 (best)

### Medicine (`MedicinalPillWorkable`)

- `MajorOrder` = `medicineType + 1000` (always sorts after food)
- `MinorOrder` = 0
- Always displayed, auto-discovered

### Batteries / Electrobanks (`Electrobank`, DLC3)

- `MajorOrder` = 500 (sorts between food and medicine)
- `MinorOrder` = 0
- Always displayed, auto-discovered

### Oxygen Tanks (`SymbolicConsumableItem`, DLC3)

- `MajorOrder` = 1, `MinorOrder` = 1
- Revealed when any bionic duplicant exists in the colony

## Column Ordering

Consumables are sorted by `MajorOrder` first, then `MinorOrder`. This produces:

1. Quality -1 foods (Gristle Berry, etc.)
2. Quality 0 foods
3. Quality 1 foods
4. ... up to quality 5
5. Batteries (MajorOrder 500)
6. Medicines (MajorOrder 1000+)

Divider columns are inserted between each quality group.

## Discovery System

Not all consumables are visible from game start. `ConsumerManager` tracks which have been discovered:

- **Food**: discovered when found in world inventory, recipe unlocked, or crop visible on the map
- **Medicine**: always discovered (auto-visible)
- **Batteries**: always discovered (DLC3)
- **Oxygen tanks**: discovered when bionic duplicants exist (DLC3)

When a consumable is discovered, `ConsumerManager.OnDiscover` fires and the table re-renders via `MarkRowsDirty()`. Debug mode (`DebugHandler.InstantBuildMode`) reveals all consumables.

## Permission Data Model

### Per-Duplicant: `ConsumableConsumer`

Attached to each duplicant's GameObject. Two sets control what they can eat:

```csharp
public HashSet<Tag> forbiddenTagSet;           // Player-set forbidden items (serialized/saved)
public HashSet<Tag> dietaryRestrictionTagSet;   // Model-based restrictions (not player-editable)
```

**Key methods**:

- `IsPermitted(string id)` — returns true if NOT in `forbiddenTagSet` AND NOT in `dietaryRestrictionTagSet`
- `SetPermitted(string id, bool allowed)` — adds/removes from `forbiddenTagSet`; enforces dietary restrictions (cannot permit a diet-restricted item)
- `IsDietRestricted(string id)` — checks `dietaryRestrictionTagSet`

Permission changes fire `consumableRulesChanged` event.

### Colony-Wide Defaults: `ConsumerManager`

Singleton at `ConsumerManager.instance`. Stores:

```csharp
private List<Tag> defaultForbiddenTagsList;   // Default restrictions for newly printed duplicants
```

When a new duplicant arrives, their `ConsumableConsumer.forbiddenTagSet` is initialized from this list.

### Dietary Restrictions

Restrictions are model-based and cannot be overridden by the player:

| Duplicant Model | Cannot Consume |
|----------------|---------------|
| Standard | Charged portable batteries, oxygen tanks |
| Bionic | All edible food, incompatible battery types |

Diet-restricted items appear with a disabled checkbox (state 3) and a cleared background color.

## Checkbox States

Each cell uses a `MultiToggle` with these states:

| State | Visual | Meaning |
|-------|--------|---------|
| 0 | Unchecked | Forbidden — duplicant will not consume this item |
| 1 | Checked | Permitted — duplicant may consume this item |
| 3 | Disabled/grayed | Not applicable — diet restriction prevents this combination |

The super-checkbox (toggle-all) uses `ResultValues`:

| Value | Visual | Meaning |
|-------|--------|---------|
| `False` | Unchecked | All duplicants forbidden |
| `Partial` | Indeterminate | Mixed — some allowed, some forbidden |
| `True` | Checked | All duplicants permitted |
| `NotApplicable` | Disabled | Diet restriction (entire column disabled for this model type) |

## User Interactions

### Clicking a Minion Checkbox

Toggles between permitted and forbidden:
- If currently True → set to False (forbid)
- If currently False → set to True (permit)
- If NotApplicable (diet restricted) → no change possible

Calls `ConsumableConsumer.SetPermitted()` on the duplicant.

### Clicking the Default Row Checkbox

Toggles the colony-wide default:
- True → removes consumable from `ConsumerManager.DefaultForbiddenTagsList`
- False → adds consumable to the list

Affects only future duplicants, not existing ones.

### Clicking the Super-Checkbox (Toggle All)

Controls all duplicants for one consumable:
- If currently True → set all to False
- If currently False or Partial → set all to True

Implementation cascades through a coroutine (`CascadeSetColumnCheckBoxes`) that updates the default row first, then each minion row.

### Clicking a Duplicant Name

Opens the duplicant's details screen (selects that minion).

### Horizontal Scrolling

When 12+ consumables are discovered, a horizontal scrollbar appears. All rows scroll in sync — the `TableScreen` parent synchronizes scroll positions across `consumableScroller` instances in each row.

## Tooltip Information

Hovering over a cell shows a tooltip with context-dependent information:

### Header Cell Tooltip
- Consumable name
- Available quantity (formatted as calories for food)
- Morale impact (e.g., "+3 morale")
- Quality rating with descriptor (e.g., "Quality: Great (4)")
- Full item description

### Default Row Tooltip
- Whether new minions will or will not be allowed to consume this item

### Minion Row Tooltip
- Whether this specific duplicant is or is not permitted to consume this item
- For food: quality adjusted by the duplicant's food expectation attribute
- Morale bonus/penalty after expectation adjustment

### Stored Minion Tooltip
- Storage reason (e.g., "Duplicant is cryofrozen")

## Food Quality and Morale

Food quality determines the morale effect when eaten. The actual effect depends on the gap between food quality and the duplicant's food expectation:

```
adjustedQuality = baseQuality + round(foodExpectationAttribute)
```

Quality-to-effect mapping:

| Adjusted Quality | Effect ID | Morale Impact |
|-----------------|-----------|---------------|
| -1 | EdibleMinus3 | Large penalty |
| 0 | EdibleMinus2 | Moderate penalty |
| 1 | EdibleMinus1 | Small penalty |
| 2 | Edible0 | Neutral |
| 3 | Edible1 | Small bonus |
| 4 | Edible2 | Moderate bonus |
| 5 | Edible3 | Large bonus |

Quality descriptors from `STRINGS.DUPLICANTS.NEEDS.FOOD_QUALITY.ADJECTIVES`: "Disgusting", "Poor", "Mediocre", "Okay", "Great", "Excellent", "Exceptional".

### Visual Quality Indicator

Each minion-row checkbox has a background tint based on quality vs expectation:

```csharp
float gap = max(foodQuality - foodExpectation + 1, 0);
bgColor = rgba(0.72, 0.44, 0.58, gap * 0.25);  // Purple tint, more opaque = better match
```

This gives sighted players an at-a-glance color gradient showing which foods are above or below each duplicant's expectations.

## Sorting

Minion rows can be sorted by:
- **Alphabetical** — by duplicant name
- **QoL Expectations** — by quality-of-life expectation value

Consumable columns have no user-controlled sorting; they are fixed in MajorOrder/MinorOrder order.

## Localization Strings

Key strings from `STRINGS.UI.CONSUMABLESSCREEN`:

| String Key | Purpose |
|-----------|---------|
| `TITLE` | Screen title |
| `FOOD_AVAILABLE` | Tooltip: "{0} available" |
| `FOOD_MORALE` | Tooltip: morale impact |
| `FOOD_QUALITY` | Tooltip: quality rating |
| `FOOD_PERMISSION_ON` | "[Minion] may eat [food]" |
| `FOOD_PERMISSION_OFF` | "[Minion] cannot eat [food]" |
| `NEW_MINIONS_FOOD_PERMISSION_ON` | Default row: food available to new minions |
| `NEW_MINIONS_FOOD_PERMISSION_OFF` | Default row: food forbidden to new minions |
| `CANNOT_ADJUST_PERMISSIONS` | Stored minion: reason permissions can't be changed |
| `TOOLTIP_TOGGLE_ALL` | Super-checkbox: toggle all on/off |
| `TOOLTIP_TOGGLE_ROW` | Row-level toggle tooltip |

## Programmatic Table Traversal

### Getting Rows

- `TableScreen.rows` (public `List<TableRow>`) — all rows including header, default, minions, stored minions, world dividers
- `TableScreen.all_sortable_rows` (public `List<TableRow>`) — minion and stored minion rows only (excludes header, default, dividers). **Reflects current visual sort order** after `SortRows()` runs (re-sorted per world group, respects `sort_is_reversed`).

### Getting Columns

- `TableScreen.columns` (protected `Dictionary<string, TableColumn>`) — keyed by registration ID. No public accessor; access via reflection or from within a subclass.
- On .NET Framework 4.7.2 / Mono, `Dictionary` preserves insertion order when no removals occur. `columns` is only ever added to (via `RegisterColumn`), and columns are registered in `OnActivate()` in left-to-right visual order. So iterating `columns.Values` matches visual order in practice, though this is an implementation detail.
- `TableColumn.widgets_by_row` is **public** `Dictionary<TableRow, GameObject>`, so once you have a column reference you can freely read cell widgets.

### Row-Column Cell Access

```csharp
GameObject widget = row.GetWidget(column);  // public, returns null if missing
```

### Reading Duplicant Identity

```csharp
IAssignableIdentity identity = row.GetIdentity();  // public
```

Returns by row type:
- Header → `null`
- Default → `null`
- Minion → `MinionIdentity`
- StoredMinon → `StoredMinionIdentity`
- WorldDivider → `null`

### Checking Row Type

```csharp
row.rowType  // public TableRow.RowType enum field
```

Values: `Header`, `Default`, `Minion`, `StoredMinon`, `WorldDivider`

### Reading Checkbox State

```csharp
MultiToggle toggle = widget.GetComponent<MultiToggle>();
int state = toggle.CurrentState;
// 0 = unchecked (forbidden), 1 = checked (permitted), 2 = partial, 3 = disabled/diet-restricted
```

### Invoking a Click Programmatically

Two options:
```csharp
// Option 1: fire the MultiToggle's click delegate
widget.GetComponent<MultiToggle>().onClick?.Invoke();

// Option 2: call the column's press handler directly
((CheckboxTableColumn)column).on_press_action(widget);
```

## GameObject Hierarchy

### Row Parenting

- **Header row**: instantiated from `prefab_row_header` under `header_content_transform`
- **All other rows**: instantiated from `prefab_row_empty` under `scroll_content_transform`

### Column Widget Parenting

- **Non-scrolled columns** (portrait, name, QoL): widgets parent directly to the row GameObject
- **Scrolled columns** (consumables): each row creates a `ScrollRect` child. Column widgets parent to `ScrollRect.content.gameObject`

### Per-Row Scroller Structure

For each row that has scrollable columns:

```
Row GameObject
├── [Portrait widget]
├── [Name widget]
├── [QoL widget]
└── ScrollRect GameObject (instantiated from scrollerPrefab)
    ├── Content panel (ScrollRect.content) ← consumable column widgets parent here
    ├── Scrollbar
    └── Border
```

Access: `row.GetScroller("consumableScroller")` returns the content panel. The `ScrollRect` component is on `scroller.transform.parent`, not the content panel itself.

## Scroll Position

### Tracking

- `TableScreen.targetScrollerPosition` (private float, 0.0–1.0 normalized)
- `TableScreen.scrollersDirty` (private bool)

### Sync Mechanism

1. User scrolls any row's `ScrollRect`
2. `ScrollRect.onValueChanged` fires, calls `screen.SetScrollersDirty(normalizedPosition)`
3. `PositionScrollers()` applies the same `horizontalNormalizedPosition` to every row's `ScrollRect`

### Programmatic Scrolling

```csharp
screen.SetScrollersDirty(targetPosition);  // public, 0.0 = left, 1.0 = right
```

Or set directly on a row and let sync propagate:
```csharp
row.scroll_rect.horizontalNormalizedPosition = position;
```

### Scroll Visibility

The scrollbar only appears when 12+ consumables are discovered. Controlled by `ConsumablesTableScreen.refresh_scrollers()`, which enables/disables each row's `ScrollRect.horizontal` and scrollbar visibility.

## Cascade Coroutine Behavior

The super-checkbox "toggle all" uses coroutines to update cells one per frame with a looping sound effect.

### CascadeSetColumnCheckBoxes (toggle all duplicants for one consumable)

- Iterates `all_sortable_rows`
- Calls `on_set_action(widget, state)` for each row that needs changing
- **Yields one frame (`yield return null`) per widget updated**
- Plays a looping cascade sound for the duration
- Refreshes the header widget only after all rows are done

### CascadeSetRowCheckBoxes (toggle all consumables for one duplicant)

- Iterates all `CheckboxTableColumn[]` for the row
- Same one-frame-per-widget yield pattern
- Same looping sound

### Mid-Cascade State

During a cascade, `on_set_action()` changes the underlying data but `on_load_action()` (UI refresh) is not called per widget. **Reading widget state mid-cascade may return stale values.** The header widget refreshes only after the coroutine completes. Multiple cascades can run simultaneously, tracked by `active_cascade_coroutine_count`.

## Getting Consumable Identity from a Column

`ConsumableInfoTableColumn` has a **public** `consumable_info` field of type `IConsumableUIItem`:

```csharp
public class ConsumableInfoTableColumn : CheckboxTableColumn
{
    public IConsumableUIItem consumable_info;
    public Func<GameObject, string> get_header_label;
    // ...
}
```

To get the consumable name/ID from a column:

```csharp
var col = column as ConsumableInfoTableColumn;
string id = col.consumable_info.ConsumableId;      // Prefab tag name
string name = col.consumable_info.ConsumableName;   // Localized display name
int major = col.consumable_info.MajorOrder;         // Category: food quality (-1..5), battery (500), medicine (1000+)
```

To distinguish food vs medicine vs battery: check `MajorOrder`. Food items have `MajorOrder` equal to their quality level (-1 to 5). Batteries have `MajorOrder = 500`. Medicines have `MajorOrder = 1000+`. Alternatively, type-check the `consumable_info` instance: `EdiblesManager.FoodInfo` for food, `MedicinalPillWorkable` for medicine, `Electrobank` for batteries.

## Detecting DividerColumns

`DividerColumn` is a minimal subclass of `TableColumn` that renders as a `Spacer` widget with no data:

```csharp
public class DividerColumn : TableColumn
{
    public override GameObject GetDefaultWidget(GameObject parent) =>
        Util.KInstantiateUI(Assets.UIPrefabs.TableScreenWidgets.Spacer, parent, force_active: true);
    // GetMinionWidget and GetHeaderWidget also return Spacer
}
```

Divider columns appear in two contexts:
1. **Explicit dividers** between food quality groups (inserted when `MajorOrder` changes between adjacent consumables)
2. **Scroller boundary dividers** auto-created by `TableScreen.StartScrollableContent()` with keys like `"scroller_spacer_consumableScroller"`

To skip dividers when navigating columns:

```csharp
// Type check — catches both explicit and scroller-boundary dividers:
if (column is DividerColumn) continue;

// Or only process data columns:
if (column is ConsumableInfoTableColumn consumableCol) { /* navigate to this column */ }
```

## Key Source Files

| File | Purpose |
|------|---------|
| `ConsumablesTableScreen.cs` | Main screen class |
| `TableScreen.cs` | Parent class — table layout, scroll sync, row management |
| `TableRow.cs` | Individual row with RowType enum |
| `TableColumn.cs` | Base column class |
| `CheckboxTableColumn.cs` | Checkbox column with MultiToggle states |
| `ConsumableInfoTableColumn.cs` | Per-consumable column with header label |
| `SuperCheckboxTableColumn.cs` | Toggle-all super-checkbox |
| `DividerColumn.cs` | Visual spacer between quality groups |
| `ConsumableConsumer.cs` | Per-duplicant permission storage |
| `ConsumerManager.cs` | Colony-wide defaults and discovery tracking |
| `EdiblesManager.cs` | Food type registry with FoodInfo |
| `MedicinalPillWorkable.cs` | Medicine consumable item |
| `Electrobank.cs` | Battery consumable item (DLC3) |
| `SymbolicConsumableItem.cs` | Wrapper for abstract consumables like oxygen tanks |
| `IConsumableUIItem.cs` | Common interface for all consumable types |

## TableScreen Internals

### columns Dictionary

`TableScreen.columns` is **protected** `Dictionary<string, TableColumn>`. No public accessor exists. External handlers must use reflection.

### Row Ordering

`all_sortable_rows` **reflects the current visual sort order** after `SortRows()` runs. The method clears the list, re-sorts per world group using `active_sort_method` (reversing if `sort_is_reversed`), then rebuilds `all_sortable_rows` in sorted order and calls `SetSiblingIndex()` on each row to match. When no sort is active, rows are grouped by world in insertion order.

### Column Ordering

`columns` is a plain `Dictionary<string, TableColumn>`. On .NET Framework 4.7.2 / Mono, `Dictionary` preserves insertion order when no removals occur. `columns` is only ever added to (via `RegisterColumn`), and columns are registered in `OnActivate()` in left-to-right visual order. So iterating `columns.Values` matches visual order in practice, though this is an implementation detail.

## OniAccess Status

As of this writing, no handler exists for the consumables screen in OniAccess. A handler would need to be registered in `ContextDetector.RegisterMenuHandlers()`.
