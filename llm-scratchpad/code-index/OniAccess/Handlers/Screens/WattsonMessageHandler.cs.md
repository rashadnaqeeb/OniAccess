# WattsonMessageHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `WattsonMessage`, the welcome screen shown at the start of every new colony. Presents the welcome narrative as a Label and the dismiss button.

`LocText.text` returns a `MISSING.STRINGS` key because TMP's `SetText` (used by `OnPrefabInit`) doesn't update the `m_text` field that the `text` getter reads. The welcome message is read directly from game data (`CustomGameSettings.GetCurrentClusterLayout().welcomeMessage`) rather than from the LocText component.

---

## class WattsonMessageHandler : BaseWidgetHandler (line 15)

  **Properties**
  - `public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.WELCOME_MESSAGE` (line 16)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 19)

  **Constructor**
  - `public WattsonMessageHandler(KScreen screen) : base(screen)` (line 21)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 25) — calls `ReadWelcomeText()` and adds Label widget; reads `button` field and adds ButtonWidget with OK fallback label
  - `private static string ReadWelcomeText()` (line 53) — replicates `WattsonMessage.OnPrefabInit` logic: reads `CustomGameSettings.GetCurrentClusterLayout().welcomeMessage`; resolves via `Strings.TryGet`; falls back to Spaced Out or base game welcome message strings
