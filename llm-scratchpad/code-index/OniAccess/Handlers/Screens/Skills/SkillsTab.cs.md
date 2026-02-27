namespace OniAccess.Handlers.Screens.Skills

// Tab 2: NestedMenuHandler with categories (Dupe Info, Available, Locked, Mastered, Boosters).
// Level 0 = categories, level 1 = items within category, level 2 = hat list (only under hat entry).

internal class SkillsTab : NestedMenuHandler, ISkillsTab (line 13)
  private readonly SkillsScreenHandler _parent (line 14)

  // Category indices
  private const int CAT_DUPE_INFO = 0 (line 17)
  private const int CAT_AVAILABLE = 1 (line 18)
  private const int CAT_LOCKED   = 2 (line 19)
  private const int CAT_MASTERED = 3 (line 20)
  private const int CAT_BOOSTERS = 4 (line 21)

  // Dupe Info item indices
  private const int INFO_NAME_POINTS = 0 (line 24)
  private const int INFO_INTERESTS   = 1 (line 25)
  private const int INFO_MORALE      = 2 (line 26)
  private const int INFO_MORALE_NEED = 3 (line 27)
  private const int INFO_XP          = 4 (line 28)
  private const int INFO_HAT         = 5 (line 29)

  internal SkillsTab(SkillsScreenHandler parent) (line 31)  // base(screen: null)

  public string TabName { get; }          (line 35)  // => STRINGS.ONIACCESS.SKILLS.SKILLS_TAB
  public override string DisplayName { get; } (line 37)  // => TabName
  public override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 39)
  // Extends NestedNavHelpEntries with Tab/Shift+Tab, Space (jump to tree), Enter (learn), +/- (boosters).

  // --- ISkillsTab ---

  public void OnTabActivated(bool announce) (line 51)
  // Calls ResetState, speaks tab name if announce, speaks first item label.

  public void OnTabDeactivated() (line 62)
  // Clears search buffer.

  public bool HandleInput() (line 66)
  // Space at Level 1 (non-dupe-info/booster): jumps to SkillsTreeTab for current skill.
  // +/= or KeypadPlus at Level 1 booster: HandleBoosterAssign.
  // -/KeypadMinus at Level 1 booster: HandleBoosterUnassign.
  // Otherwise delegates to base.Tick().

  public new bool HandleKeyDown(KButtonEvent e) (line 93)
  // Delegates to base.HandleKeyDown(e).

  // --- NestedMenuHandler abstracts ---

  protected override int MaxLevel { get; }    (line 101)  // => 2
  protected override int SearchLevel { get; } (line 102)  // => 1
  protected override int StartLevel { get; }  (line 103)  // => 1

  protected override int GetItemCount(int level, int[] indices) (line 105)
  // Level 0: GetCategoryCount. Level 1: GetLevel1Count. Level 2: GetLevel2Count.

  protected override string GetItemLabel(int level, int[] indices) (line 112)
  // Level 0: category name. Level 1: GetLevel1Label. Level 2: GetLevel2Label.

  protected override string GetParentLabel(int level, int[] indices) (line 119)
  // Level >= 1: returns category name.

  protected override void ActivateLeafItem(int[] indices) (line 124)
  // Level 2, CAT_DUPE_INFO: SelectHat.
  // Level 1, CAT_DUPE_INFO, INFO_HAT: no-op (drill-down handled by NestedMenuHandler).
  // Level 1, booster category: speaks BOOSTER_HINT.
  // Level 1, skill category: calls TryLearnSkill.

  protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) (line 152)
  // Always returns 1.

  // --- Search across all skill categories (excluding Dupe Info and Boosters) ---

  protected override int GetSearchItemCount(int[] indices) (line 160)
  // Returns GetAllSearchableSkills().Count.

  protected override string GetSearchItemLabel(int flatIndex) (line 164)
  // Returns skill.Name from GetAllSearchableSkills()[flatIndex].

  protected override void MapSearchIndex(int flatIndex, int[] outIndices) (line 170)
  // Determines which category (Available/Locked/Mastered) and index-within-bucket
  // the skill maps to. Falls back to CAT_AVAILABLE, idx=0 on timing edge cases.

  // --- Categories ---

  private int GetCategoryCount() (line 199)
  // Returns 5 if ShowBoosters(), else 4.

  private string GetCategoryName(int cat) (line 203)
  // Maps CAT_* constants to STRINGS.ONIACCESS.SKILLS.BUCKET_* strings.

  private int GetBoosterCategoryIndex() (line 214)
  // Returns CAT_BOOSTERS if ShowBoosters(), else -1.

  private bool ShowBoosters() (line 218)
  // Returns true only if selected dupe is non-null, not stored, and IsBionic().

  // --- Level 1 items ---

  private int GetLevel1Count(int cat) (line 229)
  // CAT_DUPE_INFO: 1 (stored) or 6 (live). Skill cats: GetSkillsInBucket.Count.
  // CAT_BOOSTERS: GetBoosterItemCount().

  private string GetLevel1Label(int cat, int idx) (line 249)
  // CAT_DUPE_INFO: BuildDupeInfoLabels[idx]. Skill cats: BuildSkillLabel.
  // CAT_BOOSTERS: GetBoosterLabel(idx).

  // --- Level 2: Hat list ---

  private int GetLevel2Count(int cat, int idx) (line 278)
  // Non-zero only for CAT_DUPE_INFO at INFO_HAT; returns GetAvailableHats(resume).Count.

  private string GetLevel2Label(int cat, int idx, int subIdx) (line 287)
  // Returns hat name from GetAvailableHats at subIdx for the hat row only.

  private void SelectHat(int hatIdx) (line 298)
  // Empty hat ID: calls resume.SetHats(currentHat, null) then ApplyTargetHat (removes immediately).
  // Actual hat: calls resume.SetHats and creates PutOnHatChore if resume owns the hat.
  // Refreshes SkillsScreen, plays click, speaks HAT_SELECTED or HAT_QUEUED.

  // --- Skill actions ---

  private Skill GetCurrentSkill() (line 331)
  // Returns current skill at Level 1 for skill categories; null for dupe info or boosters.

  private Skill GetSkillAtLevel1(int cat, int idx) (line 338)
  // Returns the skill from the appropriate bucket list at the given index.

  private void TryLearnSkill(Skill skill) (line 348)
  // Delegates to SkillsHelper.TryLearnSkill.

  // --- Boosters ---

  private int GetBoosterItemCount() (line 355)
  // Returns 1 (slot summary) + GetBoosterEntries.Count.

  private string GetBoosterLabel(int idx) (line 365)
  // idx == 0: BuildSlotSummary. idx >= 1: BuildBoosterLabel(entries[idx-1]).

  private void HandleBoosterAssign() (line 377)
  // Gets booster entry at GetIndex(1)-1. Checks AvailableCount > 0.
  // Calls TryAssignBooster; refreshes screen; speaks BOOSTER_ASSIGNED or NO_EMPTY_SLOTS.

  private void HandleBoosterUnassign() (line 409)
  // Gets booster entry at GetIndex(1)-1. Checks AssignedCount > 0.
  // Calls TryUnassignBooster; refreshes screen; speaks BOOSTER_UNASSIGNED.

  private void RefreshGameScreen() (line 439)
  // Calls SkillsScreen.RefreshAll() if parent.Screen is a SkillsScreen.

  // --- Helpers ---

  private static SkillsHelper.Bucket CategoryToBucket(int cat) (line 449)
  // Maps CAT_AVAILABLE->Available, CAT_LOCKED->Locked, CAT_MASTERED->Mastered, else DupeInfo.

  private List<Skill> GetAllSearchableSkills() (line 458)
  // Returns SkillsHelper.GetSkillsForModel for the selected dupe's model.
