namespace OniAccess.Handlers.Screens.Skills

// Tab 3: navigable DAG of skills using NavigableGraph.
// Up moves to the first prerequisite. Down moves to the first dependent.
// Left/Right cycles among siblings from the last Up/Down move.
// Enter learns the current skill.

internal class TreeTab : ISkillsTab (line 14)
  private readonly SkillsScreenHandler _parent (line 15)
  private NavigableGraph<Skill> _graph (line 16)
  private Tag _lastModel (line 17)  // used by EnsureGraphCurrent to detect dupe model change

  internal TreeTab(SkillsScreenHandler parent) (line 19)

  public string TabName { get; }  (line 23)  // => STRINGS.ONIACCESS.SKILLS.TREE_TAB

  // --- ISkillsTab ---

  public void OnTabActivated(bool announce) (line 29)
  // Calls RebuildGraph, speaks tab name if announce.
  // Moves to first root with root sibling context; speaks its full skill label.

  internal void OnTabActivatedAt(Skill skill) (line 42)
  // Entry point when jumping from SkillsTab via Space.
  // Calls RebuildGraph, establishes sibling context from the skill's parents
  // (or root siblings if no parents), then speaks the skill's label.

  public void OnTabDeactivated() (line 59)  // no-op

  public bool HandleInput() (line 61)
  // Down: NavigateDown() -> speaks label or DEAD_END.
  // Up: NavigateUp() -> speaks label or ROOT_NODE.
  // Right: CycleSibling(1) -> speaks label (wrap sound if wrapped).
  // Left: CycleSibling(-1) -> speaks label (wrap sound if wrapped).
  // Enter: calls SkillsHelper.TryLearnSkill for current node.
  // Calls EnsureGraphCurrent before any navigation to handle dupe model changes.

  public bool HandleKeyDown(KButtonEvent e) (line 121)  // always returns false

  // --- Graph management ---

  private void RebuildGraph() (line 127)
  // Creates NavigableGraph<Skill> with getParents=SkillsHelper.GetParents,
  // getChildren=SkillsHelper.GetChildren, getRoots=()->GetRootSkills(model).
  // Stores model in _lastModel.

  private void EnsureGraphCurrent() (line 136)
  // If graph is null or dupe model has changed, calls RebuildGraph and
  // positions the graph at the first root with root sibling context.

  // --- Sounds ---

  static void PlayHoverSound() (line 150)
  // Plays "HUD_Mouseover"; logs on failure.

  static void PlayWrapSound() (line 155)
  // Plays "HUD_Click"; logs on failure.
