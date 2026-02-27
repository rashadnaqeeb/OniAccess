# ResearchScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `ResearchScreen`. Manages three tabs composed as separate objects:
- **Browse** (`BrowseTab`): categorized tech list with search
- **Queue** (`QueueTab`): current research queue
- **Tree** (`TreeTab`): DAG graph navigation

Tab cycling via Tab/Shift+Tab. `CapturesAllInput = true`. Lifecycle: Harmony `Show`-patch on `ResearchScreen.OnShow(bool)` (not `OnShow` on its own, because `KModalScreen.OnActivate` calls `OnShow(true)` during prefab init).

Tab implementations live in `OniAccess.Handlers.Screens.Research`.

---

## class ResearchScreenHandler : BaseScreenHandler (line 18)

  **Private Enum**
  - `private enum TabId { Browse, Queue, Tree }` (line 19)

  **Fields**
  - `private readonly BrowseTab _browseTab` (line 21)
  - `private readonly QueueTab _queueTab` (line 22)
  - `private readonly TreeTab _treeTab` (line 23)
  - `private readonly IResearchTab[] _tabs` (line 24)
  - `private TabId _activeTab` (line 26)

  **Constructor**
  - `public ResearchScreenHandler(KScreen screen) : base(screen)` (line 28) — constructs all three tab objects

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.RESEARCH.HANDLER_NAME` (line 35)
  - `public override bool CapturesAllInput => true` (line 37)
  - `private static readonly List<HelpEntry> _helpEntries` (line 39)
  - `public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries` (line 50)

  **Lifecycle**
  - `public override void OnActivate()` (line 56) — activates Browse tab (no announcement), queues research point inventory string
  - `public override void OnDeactivate()` (line 66) — deactivates the currently active tab, then calls base

  **Input**
  - `public override bool Tick()` (line 75) — intercepts Tab (with Shift for direction) to call `CycleTab`; delegates to `ActiveTab.HandleInput()` for all other input
  - `public override bool HandleKeyDown(KButtonEvent e)` (line 90) — delegates to `ActiveTab.HandleKeyDown(e)`

  **Tab Management**
  - `internal void JumpToTreeTab(Tech tech)` (line 102) — deactivates active tab, switches to Tree tab, calls `_treeTab.OnTabActivatedAt(tech)`. Called by Browse and Queue tabs when player presses Space.
  - `private IResearchTab ActiveTab => _tabs[(int)_activeTab]` (line 108)
  - `private void CycleTab(int direction)` (line 110) — deactivates current tab, advances index, plays wrap or hover sound, activates new tab with announcement

  **Sounds**
  - `static void PlayHoverSound()` (line 124)
  - `static void PlayWrapSound()` (line 128)
