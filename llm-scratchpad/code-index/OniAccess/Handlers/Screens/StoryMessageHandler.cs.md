# StoryMessageHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `StoryMessageScreen`, a blocking popup shown during victory sequences (`ColonyAchievementTracker.BeginVictorySequence`). Presents achievement title + body as a Label and the dismiss button.

`titleLabel` and `bodyLabel` are set via property setters after `StartScreen + Show`, so `DiscoverWidgets` skips the first call (defers by one frame) and allows up to 3 retries via `MaxDiscoveryRetries`. This mirrors the `_firstDiscovery` pattern used in `ConfirmDialogHandler`.

---

## class StoryMessageHandler : BaseWidgetHandler (line 16)

  **Fields**
  - `private bool _firstDiscovery = true` (line 17) — set false on first `DiscoverWidgets` call so the next call actually reads labels

  **Properties**
  - `public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.STORY_MESSAGE` (line 19)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 22)
  - `protected override int MaxDiscoveryRetries => 3` (line 24)

  **Constructor**
  - `public StoryMessageHandler(KScreen screen) : base(screen)` (line 26)

  **Lifecycle**
  - `public override void OnActivate()` (line 30) — resets `_firstDiscovery = true`, then calls base

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 35) — returns false on first call (defers); on subsequent calls, reads `titleLabel` and `bodyLabel` LocText fields, combines them as "title. body"; adds combined Label widget and dismiss `button` widget
