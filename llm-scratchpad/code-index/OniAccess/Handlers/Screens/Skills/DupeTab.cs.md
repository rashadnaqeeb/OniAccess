namespace OniAccess.Handlers.Screens.Skills

// Tab 1: flat list of all duplicants with type-ahead search.
// Enter selects a dupe and jumps to the Skills tab.

internal class DupeTab : BaseMenuHandler, ISkillsTab (line 10)
  private readonly SkillsScreenHandler _parent (line 11)

  internal DupeTab(SkillsScreenHandler parent) (line 13)  // base(screen: null)

  public string TabName { get; }          (line 17)  // => STRINGS.ONIACCESS.SKILLS.DUPES_TAB
  public override string DisplayName { get; } (line 19)  // => TabName
  public override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 21)
  // Extends MenuHelpEntries with Up/Down, Home/End, Enter=select.

  // --- ISkillsTab ---

  public void OnTabActivated(bool announce) (line 32)
  // Resets to index 0, clears search, suppresses search this frame.
  // If parent has a SelectedDupe, positions cursor to that dupe.
  // Speaks tab name if announce, then speaks BuildDupeLabel for current dupe.

  public void OnTabDeactivated() (line 52)
  // Clears search buffer.

  public bool HandleInput() (line 56)
  // Delegates to base.Tick().

  public new bool HandleKeyDown(KButtonEvent e) (line 60)
  // Delegates to base.HandleKeyDown(e).

  // --- BaseMenuHandler abstracts ---

  public override int ItemCount { get; } (line 68)  // => GetDupeList().Count

  public override string GetItemLabel(int index) (line 70)
  // Returns dupes[index].GetProperName() (plain name for type-ahead matching).

  public override void SpeakCurrentItem(string parentContext = null) (line 76)
  // Auto-selects dupe under cursor via _parent.SetSelectedDupe.
  // Speaks SkillsHelper.BuildDupeLabel (name, skill points, morale).

  protected override void ActivateCurrentItem() (line 87)
  // Dupe is already selected by navigation; calls _parent.JumpToSkillsTab().

  // --- Data ---

  private List<IAssignableIdentity> GetDupeList() (line 96)
  // Collects live MinionIdentities, plus stored minions from SkillsScreen.sortableRows
  // (read via Traverse). Uses SkillsHelper.IsStored to filter stored-only entries.
  // Sorted alphabetically by GetProperName().
