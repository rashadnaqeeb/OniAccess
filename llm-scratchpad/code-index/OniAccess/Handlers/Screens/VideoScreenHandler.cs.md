# VideoScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `VideoScreen` (`KModalScreen`): victory cinematics and intro videos.

Two phases:
- **Phase 1**: Unskippable video plays; no interactive elements. Announces "Video playing".
- **Phase 2**: Victory loop. `closeButton` + `proceedButton` become active; overlay text appears. Re-discovers and speaks the first widget.

For skippable intro videos, `closeButton` is already active so Phase 2 triggers immediately after the Phase 1 announcement — `DiscoverWidgets` finds just the close button, which is correct.

Widget discovery is gated on `_inVictoryLoop`. During Phase 1 there are no interactive elements, and the handler is pushed mid-`PlayVideo` before button states are configured (`Show()` fires at line 151, `SetActive` at line 172), so discovering during Phase 1 would pick up stale prefab state. Phase 2 transition is detected in `Tick()` by polling `closeButton.activeSelf`.

Lifecycle: Harmony patch on `VideoScreen.OnShow` (because `KModalScreen.OnActivate` calls `OnShow(true)` during prefab init).

---

## class VideoScreenHandler : BaseWidgetHandler (line 29)

  **Fields**
  - `private bool _announcedPlaying` (line 30) — set true after the "Video playing" announcement fires in Tick
  - `private bool _inVictoryLoop` (line 31) — set true when Phase 2 transition is detected

  **Properties**
  - `public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.VIDEO` (line 33)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 35)

  **Constructor**
  - `public VideoScreenHandler(KScreen screen) : base(screen)` (line 37)

  **Lifecycle**
  - `public override void OnActivate()` (line 41) — resets both phase flags, calls base

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 47) — returns empty widget list if not in victory loop; in Phase 2: reads overlay text from `overlayContainer` LocTexts (joined with ". "); adds `closeButton` and `proceedButton` widgets

  **Tick**
  - `public override bool Tick()` (line 85) — Phase 1: fires "Video playing" announcement on first tick; polls `closeButton.activeSelf` to detect Phase 2 transition and triggers `DiscoverWidgets` + speaks first widget; Phase 2: rediscovers each frame (buttons may come/go) and clamps `_currentIndex`; delegates to base
