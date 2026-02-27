namespace OniAccess.Handlers.Screens.Research

// Browse tab: two-level NestedMenuHandler.
// Level 0 = buckets (Available, Locked, Completed).
// Level 1 = techs within the selected bucket.
// Type-ahead search across all techs at level 1.
// Enter queues research. Space jumps to Tree tab.

internal class BrowseTab : NestedMenuHandler, IResearchTab (line 13)
  private readonly ResearchScreenHandler _parent (line 14)

  internal BrowseTab(ResearchScreenHandler parent) (line 16)  // base(screen: null)

  public string TabName { get; }          (line 20)  // => STRINGS.ONIACCESS.RESEARCH.BROWSE_TAB
  public override string DisplayName { get; } (line 22)  // => TabName
  public override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 24)  // => NestedNavHelpEntries

  // --- IResearchTab ---

  public void OnTabActivated(bool announce) (line 30)
  // Resets state, speaks tab name if announce, then speaks first item label.

  public void OnTabDeactivated() (line 41)
  // Clears search buffer.

  public bool HandleInput() (line 45)
  // Space at Level 1: jumps to TreeTab for the current tech.
  // Otherwise delegates to base.Tick() for Up/Down/Home/End/Enter/Left/Right/Search.

  public new bool HandleKeyDown(KButtonEvent e) (line 59)
  // Delegates to base.HandleKeyDown(e).

  // --- NestedMenuHandler abstracts ---

  protected override int MaxLevel { get; }    (line 67)  // => 1
  protected override int SearchLevel { get; } (line 68)  // => 1
  protected override int StartLevel { get; }  (line 69)  // => 1

  protected override int GetItemCount(int level, int[] indices) (line 71)
  // Level 0: 3 (fixed bucket count). Level 1: ResearchHelper.GetTechsInBucket(indices[0]).Count.

  protected override string GetItemLabel(int level, int[] indices) (line 76)
  // Level 0: bucket name. Level 1: ResearchHelper.BuildTechLabel.

  protected override string GetParentLabel(int level, int[] indices) (line 84)
  // Level >= 1: returns bucket name as parent context.

  protected override void ActivateLeafItem(int[] indices) (line 90)
  // If tech is complete: plays reject sound and speaks name + COMPLETED.
  // Otherwise: plays click sound and calls Research.Instance.SetActiveResearch,
  // speaks the QUEUED format string.

  // --- Search across all techs (flat, spanning all buckets) ---

  protected override int GetSearchItemCount(int[] indices) (line 112)
  // Returns ResearchHelper.GetAllTechs().Count.

  protected override string GetSearchItemLabel(int flatIndex) (line 116)
  // Returns tech.Name from GetAllTechs()[flatIndex].

  protected override void MapSearchIndex(int flatIndex, int[] outIndices) (line 122)
  // Determines which bucket the tech belongs to (0=available, 1=locked, 2=complete)
  // and finds its index within that bucket's list.

  // --- Speech ---

  public override void SpeakCurrentItem(string parentContext = null) (line 143)
  // At Level 0: calls base. At Level 1: builds full tech label via BuildTechLabel,
  // prepends parentContext if provided, and speaks via SpeakInterrupt.
