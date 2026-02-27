# SkillsScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `SkillsScreen`. Manages three tabs:
- **Duplicants** (`DupeTab`): flat dupe list
- **Skills** (`SkillsTab`): categorized skill browser with search
- **Tree** (`TreeTab`): DAG graph navigation

Tab cycling via Tab/Shift+Tab. Tracks the currently selected duplicant as shared state (`_selectedDupe`) — tab objects read it via `SkillsScreenHandler.SelectedDupe`. `CapturesAllInput = true`. Lifecycle: Harmony `Show`-patch on `SkillsScreen.Show(bool)`.

Tab implementations live in `OniAccess.Handlers.Screens.Skills`.

---

## class SkillsScreenHandler : BaseScreenHandler (line 20)

  **Private Enum**
  - `private enum TabId { Duplicants, Skills, Tree }` (line 21)

  **Fields**
  - `private readonly DupeTab _dupeTab` (line 23)
  - `private readonly SkillsTab _skillsTab` (line 24)
  - `private readonly TreeTab _treeTab` (line 25)
  - `private readonly ISkillsTab[] _tabs` (line 26)
  - `private TabId _activeTab` (line 28)
  - `private IAssignableIdentity _selectedDupe` (line 29) — shared state read by all three tab objects

  **Constructor**
  - `public SkillsScreenHandler(KScreen screen) : base(screen)` (line 31)

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.SKILLS.HANDLER_NAME` (line 38)
  - `public override bool CapturesAllInput => true` (line 40)
  - `internal IAssignableIdentity SelectedDupe => _selectedDupe` (line 42)
  - `private static readonly List<HelpEntry> _helpEntries` (line 44)
  - `public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries` (line 55)

  **Lifecycle**
  - `public override void OnActivate()` (line 61) — picks initial dupe from screen's current selection or first live minion; activates Duplicants tab (no announcement)
  - `public override void OnDeactivate()` (line 75) — deactivates active tab, then base

  **Input**
  - `public override bool Tick()` (line 84) — intercepts Tab for `CycleTab`; delegates to `ActiveTab.HandleInput()`
  - `public override bool HandleKeyDown(KButtonEvent e)` (line 96) — delegates to `ActiveTab.HandleKeyDown(e)`

  **Tab Management**
  - `internal void SelectDupeAndJumpToSkills(IAssignableIdentity dupe)` (line 104) — sets dupe and immediately jumps to Skills tab; called by DupeTab on Enter
  - `internal void JumpToSkillsTab()` (line 109) — deactivates active tab, switches to Skills, plays hover sound, activates with announcement
  - `internal void JumpToTreeTab(Skill skill)` (line 116) — deactivates active tab, switches to Tree, calls `_treeTab.OnTabActivatedAt(skill)`
  - `internal void SetSelectedDupe(IAssignableIdentity dupe)` (line 122) — sets `_selectedDupe` and syncs with `SkillsScreen.CurrentlySelectedMinion`
  - `private ISkillsTab ActiveTab => _tabs[(int)_activeTab]` (line 130)
  - `private void CycleTab(int direction)` (line 132) — deactivates current tab, advances index, plays wrap or hover sound, activates new tab with announcement

  **Sounds**
  - `static void PlayHoverSound()` (line 146)
  - `static void PlayWrapSound()` (line 151)
