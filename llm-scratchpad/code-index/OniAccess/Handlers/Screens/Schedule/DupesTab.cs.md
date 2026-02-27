namespace OniAccess.Handlers.Screens.Schedule

// Duplicants tab: flat list of all duplicants with type-ahead search.
// Left/Right cycles schedule assignment for the current dupe.

internal class DupesTab : BaseMenuHandler, IScheduleTab (line 10)
  private readonly ScheduleScreenHandler _parent (line 11)

  internal DupesTab(ScheduleScreenHandler parent) (line 13)  // base(screen: null)

  public string TabName { get; }          (line 17)  // => STRINGS.ONIACCESS.SCHEDULE.DUPES_TAB
  public override string DisplayName { get; } (line 19)  // => TabName
  public override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 21)
  // Extends MenuHelpEntries with Left/Right = HELP_CHANGE_SCHEDULE.

  // --- IScheduleTab ---

  public void OnTabActivated(bool announce) (line 30)
  // Resets index to 0, clears search, suppresses search this frame.
  // Speaks tab name if announce, then speaks BuildDupeLabel for first dupe.

  public void OnTabDeactivated() (line 41)
  // Clears search buffer.

  public bool HandleInput() (line 45)
  // Delegates entirely to base.Tick().

  public new bool HandleKeyDown(KButtonEvent e) (line 49)
  // Delegates to base.HandleKeyDown(e).

  // --- BaseMenuHandler abstracts ---

  public override int ItemCount { get; } (line 57)  // => GetDupeList().Count

  public override string GetItemLabel(int index) (line 59)
  // Returns dupes[index].GetProperName() (plain name, not full label).

  public override void SpeakCurrentItem(string parentContext = null) (line 65)
  // Speaks ScheduleHelper.BuildDupeLabel for the current dupe (includes trait + schedule name).

  protected override void AdjustCurrentItem(int direction, int stepLevel) (line 74)
  // Left/Right cycles the dupe's schedule assignment through all available schedules.
  // Unassigns from current schedule, assigns to next, speaks the new schedule name.

  // --- Data ---

  private List<MinionIdentity> GetDupeList() (line 101)
  // Returns all live MinionIdentities (non-null) from Components.LiveMinionIdentities.Items.
