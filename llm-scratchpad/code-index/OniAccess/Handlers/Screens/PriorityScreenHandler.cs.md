# PriorityScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

2D grid handler for `JobsTableScreen` (duplicant priority management). Builds a virtual table from live game state on every navigation event. `_choreGroups` is a filtered snapshot of `Db.ChoreGroups.resources` (only `userPrioritizable` ones) — these are immutable database objects, safe to hold for the screen lifetime.

Rows: Toolbar (Reset Settings + Proximity toggle), Column Header, World Dividers (Spaced Out only), Minions, Stored Minions, Default row.
Columns: one per `ChoreGroup`.

Special keys: 0-5 sets cell priority; Shift+0-5 sets entire column; Ctrl+Left/Right adjusts all priorities in the current row; Ctrl+Up/Down adjusts all priorities in the current column.

---

## class PriorityScreenHandler : BaseTableHandler (line 15)

  **Fields**
  - `List<ChoreGroup> _choreGroups` (line 16)

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.PRIORITY_SCREEN.HANDLER_NAME` (line 18)
  - `static readonly List<HelpEntry> _helpEntries` (line 26) — extends `TableNavHelpEntries` with sort, 0-5, Shift+0-5, Ctrl+Left/Right, Ctrl+Up/Down entries
  - `public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries` (line 34)

  **Constructor**
  - `public PriorityScreenHandler(KScreen screen) : base(screen)` (line 20)

  **Table Setup**
  - `protected override void OnTableActivate()` (line 40) — populates `_choreGroups` from `Db.Get().ChoreGroups.resources` filtered to `userPrioritizable`

  **Row List Building**
  - `protected override void BuildRowList()` (line 50) — adds Toolbar, ColumnHeader rows; iterates worlds (Spaced Out adds WorldDivider rows); sorts minions by the current sort column's priority; adds Minion rows; adds StoredMinion rows; adds Default row

  **Table Shape**
  - `protected override int GetColumnCount(TableRowKind kind)` (line 104) — Toolbar has 2 columns; all other rows have `_choreGroups.Count` columns
  - `protected override bool ColumnWraps(TableRowKind kind)` (line 108) — false only for Toolbar rows
  - `protected override string GetColumnName(int col)` (line 110) — returns null for Toolbar row; otherwise returns `ChoreGroup.Name`
  - `protected override string GetRowLabel(RowEntry row)` (line 118) — Toolbar: "Toolbar"; ColumnHeader: null; Minion/StoredMinion: dupe name; Default: game's default row string

  **Cell Values**
  - `protected override string GetCellValue(RowEntry row)` (line 134) — Toolbar col 0: Reset Settings label; col 1: Proximity on/off + tooltip; ColumnHeader: group description + affected errand names; Minion: disabled-by-trait string or "priority, skill level"; StoredMinion: disabled or priority name; Default: Immigration priority

  **Enter / Toolbar**
  - `protected override void OnEnterPressed(RowEntry row)` (line 190) — only acts on Toolbar rows
  - `void ActivateToolbarItem()` (line 195) — col 0: invokes `OnResetSettingsClicked` via Traverse; col 1: toggles `Game.Instance.advancedPersonalPriorities` and announces

  **Ctrl+Arrow Overrides**
  - `protected override bool HandleModifiedUpDown(int direction)` (line 211) — calls `AdjustColumn`
  - `protected override bool HandleModifiedLeftRight(int direction)` (line 216) — calls `AdjustRow`

  **Priority Manager Access**
  - `IPersonalPriorityManager GetPriorityManager(RowEntry row)` (line 225) — returns `ChoreConsumer` for Minion, `StoredMinionIdentity` for StoredMinion, `Immigration.Instance` for Default
  - `static string GetPriorityName(int value)` (line 238) — maps 0-5 to game priority name strings (Disabled, VeryLow, Low, Standard, High, VeryHigh)
  - `string GetDisablingTraitName(IAssignableIdentity identity, ChoreGroup group)` (line 250) — reads `Traits.IsChoreGroupDisabled` to find the specific trait name; falls back to generic "disabled" string

  **Editing**
  - `void SetCellPriority(int value)` (line 267) — sets priority for the current cell; rejects Toolbar/Header rows; speaks "stored, can't adjust" message for StoredMinions; otherwise calls `manager.SetPersonalPriority`
  - `void AdjustRow(int delta)` (line 295) — increments/decrements all non-disabled chore group priorities for the current row; announces "row increased/decreased"
  - `void SetColumnPriority(int value)` (line 317) — sets all Minion and Default rows in the current column to `value`; announces column name and new priority name
  - `void AdjustColumn(int delta)` (line 334) — increments/decrements all Minion and Default priorities in the current column

  **Tick**
  - `public override bool Tick()` (line 359) — delegates base first; then checks number keys 0-5 (with Shift for column, without for cell)
