# ConsumablesScreenHandler.cs

## File-level / class-level comment

2D grid handler for `ConsumablesTableScreen` (duplicant food/medicine permissions).

Builds a virtual table from live game state on every navigation event. Column 0 is QoL
Expectations; remaining columns are discovered consumables sorted by MajorOrder/MinorOrder
(food by quality, then batteries, then medicine).

Enter on a column header triggers the super-checkbox (toggle all duplicants for that consumable).
Enter on a data row toggles the individual permission.

Extends `BaseTableHandler`, not `BaseWidgetHandler`.

---

```
public class ConsumablesScreenHandler : BaseTableHandler (line 18)

  // Nested type
  struct ColumnDef (line 19)
    public string Name (line 20)
    public IConsumableUIItem ConsumableInfo (line 21)

  // Fields
  List<ColumnDef> _columns (line 24)

  // Properties
  public override string DisplayName (line 26)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 39)

  // Constructor
  public ConsumablesScreenHandler(KScreen screen) (line 28)

  // Help
  static readonly List<HelpEntry> _helpEntries (line 34)

  // Column building
  void BuildColumnList() (line 45)                                  -- adds QoL column at index 0; then all food types; then medicine (MedicinalPillWorkable); then DLC3 batteries (filtered for BionicIncompatibleBatteries and DeprecatedContent) + oxygen canister if DLC3 active; sorts by MajorOrder/MinorOrder; skips non-Display or unrevealed items

  // Row list building
  protected override void BuildRowList() (line 113)                 -- calls BuildColumnList; adds ColumnHeader row; then per-world WorldDivider (if ClusterSpace DLC) + Minion rows for live minions; then StoredMinions with optional divider; then Default row

  // Table shape
  protected override int GetColumnCount(TableRowKind kind) (line 152)
  protected override string GetColumnName(int col) (line 156)       -- returns null for header row (column names shown as cell values); returns column name otherwise
  protected override string GetRowLabel(RowEntry row) (line 164)    -- ColumnHeader -> null; Minion/StoredMinion -> GetProperName; Default -> JOBSCREEN_DEFAULT
  protected override string GetCellValue(RowEntry row) (line 178)   -- dispatches by row kind and column; ColumnHeader -> name + column status; Minion -> QoL or consumable value; StoredMinion -> NA + storage reason; Default -> default permission state

  // Cell value builders
  string GetColumnStatus(IConsumableUIItem consumable) (line 217)   -- returns MIXED/ALL_PERMITTED/ALL_FORBIDDEN/RESTRICTED by checking all Minion rows
  static string GetQoLValue(MinionIdentity mi) (line 239)
  static string GetConsumableValue(MinionIdentity mi, IConsumableUIItem consumable) (line 244) -- returns RESTRICTED+reason if diet-restricted; otherwise permitted/forbidden state; for FoodInfo appends quality description, adjusted morale, and description
  static string GetDefaultValue(IConsumableUIItem consumable) (line 282) -- checks ConsumerManager.DefaultForbiddenTagsList

  // Enter
  protected override void OnEnterPressed(RowEntry row) (line 294)   -- skips if QoL column; dispatches: Minion -> ToggleMinionPermission; Default -> ToggleDefaultPermission; StoredMinion -> SpeakCell
  void ToggleMinionPermission(RowEntry row, IConsumableUIItem consumable) (line 312) -- skips if diet-restricted; toggles IsPermitted; plays wrap sound; speaks cell
  void ToggleDefaultPermission(IConsumableUIItem consumable) (line 328) -- toggles tag in DefaultForbiddenTagsList; plays wrap sound; speaks cell

  // Super-checkbox (header Enter)
  public override bool Tick() (line 343)                            -- intercepts Return when on ColumnHeader row to call ToggleColumn; otherwise delegates to base
  void ToggleColumn() (line 354)                                    -- determines if all permitted; sets new state for all non-diet-restricted minions + updates DefaultForbiddenTagsList; plays wrap sound; speaks ALL_PERMITTED or ALL_FORBIDDEN
```
