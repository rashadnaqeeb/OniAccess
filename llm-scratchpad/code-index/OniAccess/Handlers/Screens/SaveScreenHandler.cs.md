# SaveScreenHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `SaveScreen` (save game dialog from Pause menu). Flat list: "New Save" button, existing save entries (overwrite targets from `oldSavesRoot`), and close button. Each save entry shows filename + date from `HierarchyReferences` "Title" and "Date" refs (stored as `RectTransform`, resolved to `LocText`).

---

## class SaveScreenHandler : BaseWidgetHandler (line 14)

  **Properties**
  - `public override string DisplayName => (string)STRINGS.UI.FRONTEND.SAVESCREEN.TITLE` (line 15)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 17)

  **Constructor**
  - `public SaveScreenHandler(KScreen screen) : base(screen)` (line 19)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 23) — adds `newSaveButton`; walks `oldSavesRoot` children for `HierarchyReferences`-based save entries; adds `closeButton`
  - `private string BuildSaveEntryLabel(HierarchyReferences refs)` (line 70) — assembles "Title, Date" via `GetReferenceText`
  - `private string GetReferenceText(HierarchyReferences refs, string refName)` (line 82) — resolves a named HierRef reference to LocText text; same `RectTransform -> LocText` pattern as `SaveLoadHandler`
