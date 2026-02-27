# ModsHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `ModsScreen` (mod management from the main menu). Discovers mod entries from `entryParent` children, each with `HierarchyReferences` containing a `"Title"` LocText and an `"EnabledToggle"` MultiToggle. Also discovers per-mod `"ManageButton"` (Browse for local mods, Subscription for Workshop), and action buttons: Toggle All, Workshop, Close.

Toggling a mod or clicking Toggle All triggers `BuildDisplay` in the game, which destroys and recreates all entries. `RediscoverAndRestore` re-discovers and restores cursor by label match.

---

## class ModsHandler : BaseWidgetHandler (line 18)

  **Properties**
  - `public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.MODS` (line 19)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 21)

  **Constructor**
  - `public ModsHandler(KScreen screen) : base(screen)` (line 23)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 27) — walks `entryParent` children; adds a `ToggleWidget` (with `SpeechFunc` reading enabled/disabled state) and optionally a `ButtonWidget` for `ManageButton` per mod; appends `toggleAllButton` with a live-reading `SpeechFunc`, `workshopButton`, and `closeButton`

  **Widget Activation**
  - `protected override void ActivateCurrentItem()` (line 115) — mod toggle: clicks MultiToggle and calls `RediscoverAndRestore(label)` to re-find the same mod; `toggleAllButton`: clicks and calls `RediscoverAndRestore(null)` (clamp by index); other buttons: base
  - `private string GetButtonFieldName(KButton button)` (line 147) — identifies whether a KButton is `toggleAllButton`, `workshopButton`, or `closeButton` by field comparison
  - `private void RediscoverAndRestore(string targetLabel)` (line 160) — rediscovers after BuildDisplay; if `targetLabel` given, finds widget by label match; otherwise clamps to previous index
