# MainMenuHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for the main menu (`MainMenu` class). `MainMenu` inherits directly from `KScreen` (NOT `KButtonMenu`), so buttons are discovered by walking the `buttonParent` transform for `KButton` instances with `LocText` labels. Also checks the `Button_ResumeGame` serialized field, which only appears when a save file exists.

Three Tab-navigable sections:
- `SectionButtons` (0): main menu button list (Resume, New Game, Load, etc.)
- `SectionDLC` (1): 4 DLC logos with name + ownership/activation status
- `SectionNews` (2): MOTD boxes with headlines from Klei's server (async-loaded)

---

## class MainMenuHandler : BaseWidgetHandler (line 20)

  **Constants**
  - `private const int SectionButtons = 0` (line 21)
  - `private const int SectionDLC = 1` (line 22)
  - `private const int SectionNews = 2` (line 23)
  - `private const int SectionCount = 3` (line 24)

  **Fields**
  - `private int _currentSection` (line 26)
  - `private static readonly string[] DlcFieldNames` (line 28) — field names on MainMenu for each DLC logo HierarchyReferences
  - `private static readonly string[] DlcIds` (line 29)
  - `private static readonly string[] MotdBoxFields` (line 30) — field names for the three MOTD boxes on the MOTD component

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.MAIN_MENU` (line 32)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 34)

  **Constructor**
  - `public MainMenuHandler(KScreen screen) : base(screen)` (line 36)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 44) — dispatches to section-specific discovery based on `_currentSection`
  - `private void DiscoverButtonWidgets(KScreen screen)` (line 60) — discovers Resume Game button (separate field) + MakeButton-created buttons from `buttonParent`
  - `private void DiscoverDLCWidgets(KScreen screen)` (line 108) — discovers 4 DLC logo entries; each widget label is `"name, status"`
  - `private static string GetDlcStatus(string dlcId)` (line 133) — returns active/owned-not-active/not-owned status string; uses mod strings because the game's `CONTENT_OWNED_NOTINSTALLED_LABEL` is empty
  - `private void DiscoverNewsWidgets(KScreen screen)` (line 145) — discovers MOTD news boxes; uses `GetParsedText()` instead of `.text` because `SetText()` updates TMP's internal buffer but not `m_text`

  **Tab Navigation**
  - `protected override void NavigateTabForward()` (line 184)
  - `protected override void NavigateTabBackward()` (line 190)
  - `private void RediscoverForCurrentSection()` (line 197) — rediscovers widgets for the new section, speaks section name then first widget; speaks "no news" if news section is empty
  - `private static string GetSectionName(int section)` (line 209)

  **Widget Interaction**
  - `protected override void ActivateCurrentItem()` (line 229) — DLC section: fires `MultiToggle.OnPointerClick` via `ClickMultiToggle`; News section: finds `URLOpenFunction.triggerButton` and clicks it; Buttons section: delegates to base
