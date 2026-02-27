namespace OniAccess.Handlers.Screens.Research

// Tree tab: navigable DAG of technologies using NavigableGraph.
// Up moves to the first prerequisite. Down moves to the first dependent.
// Left/Right cycles among siblings from the last Up/Down move.
// Enter queues the current tech for research.

internal class TreeTab : IResearchTab (line 12)
  private readonly ResearchScreenHandler _parent (line 13)
  private readonly NavigableGraph<Tech> _graph (line 14)

  internal TreeTab(ResearchScreenHandler parent) (line 16)
  // Constructs NavigableGraph<Tech> with getParents=tech.requiredTech,
  // getChildren=tech.unlockedTech, getRoots=ResearchHelper.GetRootTechs.

  public string TabName { get; }  (line 24)  // => STRINGS.ONIACCESS.RESEARCH.TREE_TAB

  // --- IResearchTab ---

  public void OnTabActivated(bool announce) (line 30)
  // Speaks tab name if announce. Defaults to first root with root sibling context.
  // Speaks the root's full tech label queued.

  // Enter the tree focused on a specific tech (from Space in Browse/Queue).
  // No sibling context until the first Up or Down.
  internal void OnTabActivatedAt(Tech tech) (line 45)

  public void OnTabDeactivated() (line 51)  // no-op

  public bool HandleInput() (line 53)
  // Down: NavigateDown() -> speaks label or DEAD_END.
  // Up: NavigateUp() -> speaks label or ROOT_NODE.
  // Right: CycleSibling(1) -> speaks label (wrap sound if wrapped).
  // Left: CycleSibling(-1) -> speaks label (wrap sound if wrapped).
  // Enter: queues tech via Research.Instance.SetActiveResearch if not complete;
  //        if complete, plays reject sound and speaks COMPLETED.

  public bool HandleKeyDown(KButtonEvent e) (line 110)  // always returns false

  // --- Sounds ---

  static void PlayHoverSound() (line 116)
  // Plays "HUD_Mouseover"; logs on failure.

  static void PlayWrapSound() (line 121)
  // Plays "HUD_Click"; logs on failure.
