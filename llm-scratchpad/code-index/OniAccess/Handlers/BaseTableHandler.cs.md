# BaseTableHandler.cs

## File-level comment
Abstract base for 2D table screen handlers. Provides shared infrastructure:
row list with world dividers, 2D cursor navigation, cell speech with
row/column deduplication, sort cycling, and sound effects.

Subclasses implement `BuildRowList`, `GetColumnCount`, `GetColumnName`,
`GetCellValue`, and `GetRowLabel` to describe their specific table.

---

```
abstract class BaseTableHandler : BaseScreenHandler (line 15)

  // Nested type: row kind enum
  protected enum TableRowKind (line 16)
    Toolbar       (line 17)
    ColumnHeader  (line 18)
    WorldDivider  (line 19)
    Minion        (line 20)
    StoredMinion  (line 21)
    Default       (line 22)

  // Nested type: row data
  protected struct RowEntry (line 25)
    TableRowKind Kind       (line 26)
    IAssignableIdentity Identity (line 27)
    int WorldId             (line 28)

  // 2D cursor state
  protected int _row, _col                                (line 32)
  protected int _lastSpokenRow                            (line 33)
  protected int _lastSpokenCol                            (line 34)

  // Row list — rebuilt on every navigation event, never cached between calls
  protected List<RowEntry> _rows                          (line 37)

  // Sort state
  protected int _sortColumn                               (line 40)
  protected bool _sortAscending                           (line 41)

  // Properties
  override bool CapturesAllInput { get; }                 (line 43)
  // Always true.

  // Constructor
  protected BaseTableHandler(KScreen screen)              (line 45)

  // Abstract members
  protected abstract void BuildRowList()                  (line 51)
  protected abstract int GetColumnCount(TableRowKind kind) (line 52)
  protected abstract string GetColumnName(int col)        (line 53)
  protected abstract string GetCellValue(RowEntry row)    (line 54)
  protected abstract string GetRowLabel(RowEntry row)     (line 55)

  // Virtual members
  protected virtual bool ColumnWraps(TableRowKind kind)   (line 61)
  // Returns true for all row kinds except Toolbar.

  protected virtual void OnEnterPressed(RowEntry row)     (line 62)
  // No-op default; called when Enter is pressed on a non-header row.

  protected virtual bool IsColumnSortable(int col)        (line 63)
  // Returns true by default.

  protected const int StoredMinionWorldId = 255           (line 65)

  protected virtual string GetWorldName(int worldId)      (line 67)
  // Returns world proper name; falls back to worldId.ToString().
  // Returns STRINGS.ONIACCESS.TABLE.STORED for worldId 255 (stored minions).

  protected virtual bool IsRowSkipped(TableRowKind kind)  (line 73)
  // Returns true for WorldDivider rows (used to skip dividers during navigation).

  protected virtual bool HandleModifiedUpDown(int direction)   (line 75)
  // Called for Ctrl+Up/Down. No-op returning false by default.

  protected virtual bool HandleModifiedLeftRight(int direction) (line 76)
  // Called for Ctrl+Left/Right. No-op returning false by default.

  protected virtual void OnTableActivate()                (line 77)
  // Called at the start of OnActivate before row list is built.

  protected virtual int FindInitialRow()                  (line 79)
  // Returns the first non-toolbar, non-header, non-divider row index.
  // Falls back to 0.

  // Shared queries
  protected static List<IAssignableIdentity> GetLiveMinionsForWorld(int worldId) (line 94)
  // Returns live minion identities for a given world.

  protected static List<StoredMinionIdentity> GetStoredMinions()  (line 103)
  // Returns stored (hibernated/pod) minion identities across all storages.

  // Lifecycle
  override void OnActivate()                              (line 120)
  // Calls OnTableActivate, resets sort, builds row list, finds initial row,
  // resets cursor and deduplication trackers, calls base (speaks DisplayName),
  // then queues full cell context speech.

  // Sounds
  protected void PlayHoverSound()                         (line 137)
  protected void PlayWrapSound()                          (line 145)

  // Speech
  protected void SpeakCell()                              (line 157)
  // Speaks the current cell via SpeakInterrupt, calling BuildCellParts.

  protected string BuildCellParts(bool forceFullContext)  (line 162)
  // Builds the spoken string for the current cell. Includes row label when
  // forceFullContext=true or row changed since last speech. Includes column
  // name when forceFullContext=true or column changed. Always appends cell
  // value. Updates _lastSpokenRow/_lastSpokenCol to deduplicate future calls.

  // Navigation
  protected void NavigateRow(int direction)               (line 190)
  // Moves cursor by direction (1=down, -1=up). Rebuilds row list. When landing
  // on a WorldDivider, skips it and announces the world name + full cell context.

  protected void NavigateCol(int direction)               (line 222)
  // Moves cursor left/right. Wraps if ColumnWraps() is true for the current row.

  protected void NavigateHome()                           (line 247)
  // Jumps to first non-toolbar, non-header, non-skipped row.

  protected void NavigateEnd()                            (line 262)
  // Jumps to last non-skipped row.

  void ClampCol()                                         (line 274)
  // Clamps _col to [0, GetColumnCount-1] for the current row. Private.

  // Sort
  protected void CycleSort()                              (line 286)
  // Cycles the sort state for the current column: none -> descending ->
  // ascending -> none. Announces the new sort state and rebuilds the row list.

  // Help entries
  protected static readonly List<HelpEntry> TableNavHelpEntries   (line 313)
  protected static readonly HelpEntry TableSortHelpEntry          (line 318)

  // Tick
  override bool Tick()                                    (line 325)
  // Handles: Up/Down (with Ctrl modifier hook), Left/Right (with Ctrl modifier
  // hook), Home, End, Enter (CycleSort on header rows, OnEnterPressed otherwise).
```
