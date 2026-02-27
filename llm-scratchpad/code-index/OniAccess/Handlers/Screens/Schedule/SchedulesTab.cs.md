namespace OniAccess.Handlers.Screens.Schedule

// Schedules tab: 2D grid of timetable rows x hour blocks.
// Handles navigation, painting, reordering, and an inline options submenu.
// Does not extend BaseMenuHandler — the 2D cursor and custom key routing
// conflict with 1D list navigation.

internal class SchedulesTab : IScheduleTab (line 15)
  private readonly ScheduleScreenHandler _parent (line 16)

  // 2D cursor
  private int _row (line 19)
  private int _col (line 20)
  private int _lastSpokenScheduleIndex (line 21)  // -1 = no schedule spoken yet; used to detect boundary crossing

  // Brush
  private string _brushGroupId (line 24)
  private int _paintCounter (line 25)

  // Inline options submenu
  private enum OptionId { Rename, Alarm, Duplicate, DeleteSchedule, AddRow, DeleteRow } (line 28)
  private bool _inOptions (line 29)
  private int _optionIndex (line 30)
  private List<(OptionId id, string label)> _currentOptions (line 31)

  // Rename
  private readonly TextEditHelper _renameHelper (line 34)

  public string TabName { get; }  (line 36)  // => STRINGS.ONIACCESS.SCHEDULE.SCHEDULES_TAB

  internal SchedulesTab(ScheduleScreenHandler parent) (line 38)

  // --- ROW MODEL ---

  private struct GridRow (line 46)
    public bool IsAddButton (line 47)
    public global::Schedule Schedule (line 48)
    public int TimetableIndex (line 49)
    public int ScheduleIndex (line 50)

  private int TotalRows { get; } (line 53)
  // Sums GetTimetableCount for all schedules plus 1 for the Add button row.

  private GridRow GetRow(int flatRow) (line 62)
  // Maps a flat row index to a GridRow by iterating schedules and their timetable counts.
  // Returns IsAddButton=true if flatRow equals the sum of all timetable counts.

  private int FindScheduleStartRow(int scheduleIndex) (line 80)
  // Returns the flat row index at which scheduleIndex begins.

  // --- TAB LIFECYCLE ---

  public void OnTabActivated(bool announce) (line 92)
  // Resets options state and _lastSpokenScheduleIndex.
  // Restores brush from game's ScheduleScreen.SelectedPaint, defaults to Worktime.
  // Sets _row=0, _col=current game hour (via ScheduleManager.GetCurrentHour()).
  // Speaks opening cell announcement with brush name appended.

  public void OnTabDeactivated() (line 129)
  // Cancels any active rename; clears options state.

  // --- INPUT ROUTING ---

  public bool HandleInput() (line 139)
  // Routes to HandleRenameInput, HandleOptionsInput, or HandleGridInput
  // based on current state.

  public bool HandleKeyDown(KButtonEvent e) (line 149)
  // Intercepts Escape during rename (cancels it) and during options (exits them).

  // --- RENAME INPUT ---

  private bool HandleRenameInput() (line 167)
  // Enter confirms rename and speaks current cell. All other keys pass through.

  // --- OPTIONS INPUT ---

  private bool HandleOptionsInput() (line 182)
  // Up/Down navigate options; Enter activates; Left exits. Consumes all other keys.

  private void ExitOptions() (line 203)
  // Clears _inOptions, plays hover sound, speaks current cell.

  private List<(OptionId id, string label)> BuildOptionsList(GridRow gr) (line 209)
  // Returns options for the current row: Rename, Alarm (toggle label), Duplicate,
  // DeleteSchedule, AddRow, and DeleteRow (only if timetable count > 1).

  private void NavigateOption(int direction) (line 230)
  // Moves option cursor; wraps with PlayWrapSound, otherwise PlayHoverSound.
  // Speaks the new option label.

  private void ActivateOption() (line 247)
  // Dispatches to the appropriate option action by OptionId.

  // --- OPTION ACTIONS ---

  private void BeginRename(GridRow gr) (line 266)
  // Starts TextEditHelper with a factory that fetches the input field via ScheduleHelper.

  private void ToggleAlarm(GridRow gr) (line 273)
  // Flips schedule.alarmActivated; speaks the new enabled/disabled state string.

  private void DuplicateSchedule(GridRow gr) (line 283)
  // Calls ScheduleManager.DuplicateSchedule; moves cursor to the new schedule; speaks name.

  private void DeleteSchedule(GridRow gr) (line 301)
  // Guards against deleting the last schedule. Calls ScheduleManager.DeleteSchedule;
  // resets cursor to row 0; speaks SCHEDULE_DELETED.

  private void AddTimetableRow(GridRow gr) (line 319)
  // Duplicates the current timetable row's blocks and inserts them after current row.
  // Moves cursor to the new row; speaks TIMETABLE_ROW_ADDED.

  private void DeleteTimetableRow(GridRow gr) (line 339)
  // Guards against deleting the last row. Calls schedule.RemoveTimetable; clamps cursor;
  // speaks TIMETABLE_ROW_DELETED.

  // --- GRID INPUT ---

  private bool HandleGridInput() (line 359)
  // Number keys 1-4: TrySelectBrush.
  // Up/Down: Ctrl=ReorderSchedule, Shift=ShiftTimetableRow, else NavigateRow.
  // Left/Right: Ctrl=RotateBlocks, Shift=PaintAndMove, else NavigateCol.
  // Home/End: Shift=PaintRange, else jump to col 0/23.
  // Space: PaintCurrentCell.
  // Enter: open options menu or add new schedule if on Add button row.

  // --- NAVIGATION ---

  private void NavigateRow(int direction) (line 434)
  // Clamps to valid rows; plays hover sound; speaks current cell.

  private void NavigateCol(int direction) (line 445)
  // Wraps at 0/23 with PlayWrapSound; otherwise PlayHoverSound.
  // Speaks current cell without row context (includeRowContext=false).

  // --- BRUSH SELECTION ---

  private bool TrySelectBrush() (line 468)
  // Checks Alpha1-Alpha4; sets _brushGroupId and game's SelectedPaint;
  // calls screen.RefreshAllPaintButtons(); speaks the group name.

  // --- PAINTING ---

  private void PaintCurrentCell() (line 492)
  // Paints the cell at (_row, _col) with current brush.
  // If already that group: plays none sound, announces already-painted, resets counter.

  private void PaintAndMove(int direction) (line 516)
  // Moves col first (with wrapping), then paints the new cell. Increments _paintCounter.

  private void PaintRange(int targetCol) (line 540)
  // Paints all cells from min(_col, targetCol) to max(_col, targetCol).
  // Moves cursor to targetCol; announces painted range.

  // --- REORDERING ---

  private void ReorderSchedule(int direction) (line 565)
  // Swaps schedule at gr.ScheduleIndex with its neighbour in the live schedules list.
  // Fires onSchedulesChanged delegate via Traverse. Updates cursor to follow schedule.
  // Speaks "name, moved up/down".

  private void ShiftTimetableRow(bool up) (line 599)
  // Calls schedule.ShiftTimetable; updates cursor by -1/+1; plays shift sound.
  // Speaks ROW_LABEL with new timetable index + 1.

  private void RotateBlocks(bool directionLeft) (line 619)
  // Calls schedule.RotateBlocks; speaks current cell without row context.

  // --- ADD NEW SCHEDULE ---

  private void AddNewSchedule() (line 632)
  // Calls ScheduleManager.AddDefaultSchedule; moves cursor to last row before Add button;
  // speaks new schedule name then current cell.

  // --- SPEECH ---

  private void SpeakCurrentCell(bool includeRowContext = true) (line 650)
  // Clamps cursor, then:
  // Add button row: speaks ADD_SCHEDULE string.
  // Otherwise: calls BuildFullCellAnnouncement and speaks interrupt.
  // Updates _lastSpokenScheduleIndex.

  private string BuildFullCellAnnouncement(GridRow gr, bool forceScheduleName, bool includeRowContext = true) (line 666)
  // Builds announcement parts:
  // - Schedule name (with row number if multi-timetable) when crossing boundary or forced.
  // - Or just row number if same schedule and multi-row.
  // - Cell label via ScheduleHelper.BuildCellLabel.
  // - Warnings via ScheduleHelper.BuildWarnings (if any).
  // Joined with ". ".

  // --- UTILITIES ---

  private void ClampCursor() (line 702)
  // Clamps _row to [0, TotalRows-1] and _col to [0, 23].
