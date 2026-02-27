namespace OniAccess.Handlers.Screens.Details

class ErrandsSideTab : IDetailTab (line 8)
  // Reads the MinionTodoSideScreen (Errands side tab for dupes) into sections.
  // Produces three section types: schedule shift, current task, and priority groups.

  public string DisplayName { get; }  (line 9)   // => STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME
  public int StartLevel { get; }      (line 10)  // => 1
  public string GameTabId { get; }    (line 11)  // => null (side screen tab)

  public bool IsAvailable(GameObject target) (line 13)
  // Checks DetailsScreen.SidescreenTabTypes.Errands tab visibility.

  public void OnTabSelected() (line 19)
  // Clicks the Errands tab MultiToggle.

  public void Populate(GameObject target, List<DetailSection> sections) (line 26)
  // Finds MinionTodoSideScreen via ConfigSideTab.GetActiveScreens(Errands),
  // then calls AddScheduleSection, AddCurrentTaskSection, AddPriorityGroupSections.

  private static void AddScheduleSection(MinionTodoSideScreen screen, List<DetailSection> sections) (line 43)
  // Reads screen.currentShiftLabel.text. Header is STRINGS.ONIACCESS.DETAILS.SCHEDULE.

  private static void AddCurrentTaskSection(MinionTodoSideScreen screen, List<DetailSection> sections) (line 57)
  // Reads screen.currentTask. Header is STRINGS.ONIACCESS.DETAILS.CURRENT_TASK.
  // SuppressTooltip = true; speech is snapshotted at populate time via BuildEntrySpeech.

  private static void AddPriorityGroupSections(MinionTodoSideScreen screen, List<DetailSection> sections) (line 75)
  // Reads screen.priorityGroups via Traverse. Each group becomes a section.
  // Speech is snapshotted at populate time (not live) because MinionTodoChoreEntry
  // objects are continuously recycled by Apply() even while paused.

  // Builds speech for a MinionTodoChoreEntry: label, subLabel (if not redundant),
  // priority, moreLabel, then the "Total Priority:" portion of the tooltip.
  private static string BuildEntrySpeech(MinionTodoChoreEntry entry) (line 128)

  // Reads ToolTip text, filters it, and extracts the "Total Priority:" substring.
  private static string GetPriorityTooltip(MinionTodoChoreEntry entry) (line 150)

  private static string StripNulls(string text) (line 162)
  // Removes null bytes and trims; returns empty string for null/empty input.

  private static void AppendNonEmpty(System.Text.StringBuilder sb, string text) (line 168)
  // Appends text to sb with ", " separator if sb is non-empty.
