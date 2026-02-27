namespace OniAccess.Handlers.Screens.Research

// Queue tab: flat read-only list of the current research queue.
// First entry is the global point inventory (always present).
// Remaining entries are queued TechInstances in tier order.
// Enter cancels the selected tech. Space jumps to Tree tab.

internal class QueueTab : BaseMenuHandler, IResearchTab (line 12)
  private readonly ResearchScreenHandler _parent (line 13)

  internal QueueTab(ResearchScreenHandler parent) (line 15)  // base(screen: null)

  public string TabName { get; }          (line 19)  // => STRINGS.ONIACCESS.RESEARCH.QUEUE_TAB
  public override string DisplayName { get; } (line 21)  // => TabName
  public override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 23)
  // Extends MenuHelpEntries with Up/Down, Home/End, Enter=cancel help.

  // --- IResearchTab ---

  public void OnTabActivated(bool announce) (line 34)
  // Resets index to 0, clears search, suppresses search this frame.
  // Speaks tab name if announce, then speaks first item or QUEUE_EMPTY.

  public void OnTabDeactivated() (line 46)
  // Clears search buffer.

  public bool HandleInput() (line 50)
  // Space: jumps to TreeTab for the selected queue entry (index - 1 for the point row offset).
  // Otherwise delegates to base.Tick().

  public new bool HandleKeyDown(KButtonEvent e) (line 63)
  // Delegates to base.HandleKeyDown(e).

  // --- BaseMenuHandler abstracts ---

  public override int ItemCount { get; } (line 71)
  // Returns 1 + Research.Instance.GetResearchQueue().Count
  // (the +1 accounts for the always-present point inventory row).

  public override string GetItemLabel(int index) (line 78)
  // Index 0: BuildPointInventoryLabel(). Index n: queue[n-1].tech.Name.

  public override void SpeakCurrentItem(string parentContext = null) (line 87)
  // Gets current label, optionally prepends parentContext, speaks via SpeakInterrupt.

  protected override void ActivateCurrentItem() (line 95)
  // Index 0 (point inventory): re-speaks current item, no action.
  // Other indices: cancels the tech via Research.Instance.CancelResearch.
  // Announces cascade removal count if more than 1 tech was removed.
  // Clamps cursor after removal.

  // --- Helpers ---

  string GetCurrentLabel() (line 131)  // private; not a typo — method has no access modifier
  // Index 0: BuildPointInventoryLabel(). Other: BuildQueuedTechLabel for active/non-active entry.

  static string BuildPointInventoryLabel() (line 147)
  // Calls ResearchHelper.BuildPointInventoryString(), falls back to NO_BANKED_POINTS string.
