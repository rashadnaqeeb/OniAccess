# WorldGenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `WorldGenScreen` (world generation progress display). Extends `BaseScreenHandler` with no interactive widgets. `CapturesAllInput = true` blocks all keys during generation (the game also blocks Escape). `Tick()` polls progress each frame at a 2-second interval and speaks announcements at 25% increments.

Locked decisions: periodic progress updates ("25 percent... 50 percent... 75 percent... Done"); no user interaction during generation.

---

## class WorldGenHandler : BaseScreenHandler (line 17)

  **Fields**
  - `private float _lastSpokenPercent = -1f` (line 18)
  - `private float _lastPollTime` (line 19)
  - `private const float PollInterval = 2f` (line 20) — seconds between progress polls
  - `private const float SpeechInterval = 25f` (line 21) — speak every 25% increment

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.WORLD_GEN` (line 23)
  - `public override bool CapturesAllInput => true` (line 28)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; } = new List<HelpEntry>().AsReadOnly()` (line 33) — empty list; nothing to do during world gen

  **Constructor**
  - `public WorldGenHandler(KScreen screen) : base(screen)` (line 36)

  **Lifecycle**
  - `public override void OnActivate()` (line 41) — calls base (which speaks the display name); initializes `_lastSpokenPercent = 0f` and `_lastPollTime`

  **Tick**
  - `public override bool Tick()` (line 51) — polls at `PollInterval` intervals; gets percent from `GetCurrentPercent()`; speaks "complete" when >= 100%; speaks "N percent" when the rounded value crosses the next 25% threshold
  - `private float GetCurrentPercent()` (line 79) — primary path: reads `screen.offlineWorldGen.currentPercent` (private float 0.0-1.0) via Traverse; fallback: reads `offlineWorldGen.percentText` LocText and parses the "45%" string; returns -1f on failure
