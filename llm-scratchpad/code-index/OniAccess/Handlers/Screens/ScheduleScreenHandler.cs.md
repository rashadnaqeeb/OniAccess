# ScheduleScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `ScheduleScreen`. Manages two tabs:
- **Schedules** (`SchedulesTab`): 2D timetable grid with painting and reordering
- **Duplicants** (`DupesTab`): flat dupe list with schedule reassignment

Tab cycling via Tab/Shift+Tab. `CapturesAllInput = true`. Lifecycle: Harmony `OnShow`-patch on `ScheduleScreen.OnShow(bool)`.

Tab implementations live in `OniAccess.Handlers.Screens.Schedule`. Help entries differ per tab; `HelpEntries` property returns the active tab's set.

---

## class ScheduleScreenHandler : BaseScreenHandler (line 17)

  **Private Enum**
  - `private enum TabId { Schedules, Duplicants }` (line 18)

  **Fields**
  - `private readonly SchedulesTab _schedulesTab` (line 20)
  - `private readonly DupesTab _dupesTab` (line 21)
  - `private readonly IScheduleTab[] _tabs` (line 22)
  - `private TabId _activeTab` (line 24)

  **Constructor**
  - `public ScheduleScreenHandler(KScreen screen) : base(screen)` (line 26)

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.SCHEDULE.HANDLER_NAME` (line 32)
  - `public override bool CapturesAllInput => true` (line 34)
  - `private static readonly List<HelpEntry> _schedulesHelpEntries` (line 36) — Up/Down, Left/Right, Home/End, 1/2/3/4, Space, Shift+Left/Right, Shift+Home/End, Ctrl+Up/Down, Ctrl+Left/Right, Enter, Tab/Shift+Tab
  - `private static readonly List<HelpEntry> _dupesHelpEntries` (line 50) — A-Z, Up/Down, Home/End, Left/Right, Tab/Shift+Tab
  - `public override IReadOnlyList<HelpEntry> HelpEntries` (line 58) — returns `_schedulesHelpEntries` or `_dupesHelpEntries` based on `_activeTab`

  **Lifecycle**
  - `public override void OnActivate()` (line 65) — activates Schedules tab (no announcement)
  - `public override void OnDeactivate()` (line 71) — deactivates active tab, then base

  **Input**
  - `public override bool Tick()` (line 80) — intercepts Tab for `CycleTab`; delegates to `ActiveTab.HandleInput()`
  - `public override bool HandleKeyDown(KButtonEvent e)` (line 92) — delegates to `ActiveTab.HandleKeyDown(e)`

  **Tab Management**
  - `internal ScheduleScreen ScheduleScreen => _screen as ScheduleScreen` (line 100) — exposes the typed screen reference for tab objects
  - `private IScheduleTab ActiveTab => _tabs[(int)_activeTab]` (line 102)
  - `private void CycleTab(int direction)` (line 104) — deactivates current tab, advances index, plays wrap or hover sound via `ScheduleHelper`, activates new tab with announcement
