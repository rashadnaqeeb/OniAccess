namespace OniAccess.Handlers.Screens.Schedule

internal static class ScheduleHelper (line 6)

  // Brush groups in number-key order: 1=Work, 2=Bathtime, 3=Downtime, 4=Bedtime.
  internal static readonly string[] BrushGroupIds (line 10)
  // { "Worktime", "Hygene", "Recreation", "Sleep" }

  internal static string BuildDupeLabel(MinionIdentity mi) (line 14)
  // Builds "name, traitTag, scheduleName" or "name, scheduleName".
  // traitTag is appended only for NightOwl or EarlyBird traits.

  internal static string GetGroupName(string groupId) (line 33)
  // Looks up Db.Get().ScheduleGroups.Get(groupId).Name; falls back to groupId.

  internal static string BuildCellLabel(global::Schedule schedule, int timetableIdx, int col) (line 38)
  // Returns STRINGS.ONIACCESS.SCHEDULE.BLOCK_LABEL formatted with group name and column hour.
  // blockIdx = timetableIdx * 24 + col.

  internal static string BuildWarnings(global::Schedule schedule) (line 45)
  // Counts blocks per ScheduleBlockType. For any type with 0 blocks, adds a
  // STRINGS.UI.SCHEDULEGROUPS.NOTIME warning. Returns null if no warnings.

  internal static int GetTimetableCount(global::Schedule schedule) (line 64)
  // Returns schedule.GetBlocks().Count / 24.

  internal static ScheduleScreenEntry GetScreenEntry(ScheduleScreen screen, int scheduleIndex) (line 68)
  // Reads screen.scheduleEntries list via Traverse; returns entry at scheduleIndex.

  internal static KInputTextField GetEntryInputField(ScheduleScreen screen, int scheduleIndex) (line 80)
  // Gets the EditableTitleBar.inputField from a ScheduleScreenEntry via Traverse.

  internal static void PlayClickSound() (line 93)
  // Plays "HUD_Click"; logs on failure.

  internal static void PlayHoverSound() (line 98)
  // Plays "HUD_Mouseover"; logs on failure.

  internal static void PlayWrapSound() (line 103)
  // Plays "HUD_Click"; logs on failure.

  internal static void PlayPaintSound(int dragCount) (line 108)
  // Plays "ScheduleMenu_Select" at listener position with Drag_Count parameter.

  internal static void PlayPaintNoneSound() (line 121)
  // Plays "ScheduleMenu_Select_none" at listener position.

  private static bool _shiftUpToggle (line 132)
  private static bool _shiftDownToggle (line 133)
  // Toggle booleans alternate the shift sound variant on successive calls.

  internal static void PlayShiftUpSound() (line 135)
  // Alternates between "ScheduleMenu_Shift_up" and "ScheduleMenu_Shift_up_reset".

  internal static void PlayShiftDownSound() (line 148)
  // Alternates between "ScheduleMenu_Shift_down" and "ScheduleMenu_Shift_down_reset".
