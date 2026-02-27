# SaveLoadHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for `LoadScreen` (save/load screen). Three-level navigation:
1. **ColonyList**: browse colonies by name, cycle count, date; management buttons (Save Info, Convert All Cloud/Local, Load More)
2. **SaveList**: individual saves for a selected colony
3. **SaveDetail**: info fields (Title, Date, InfoWorld, InfoCycles, InfoDupes, FileSize, Filename, AutoInfo), Load button, Delete button

Enter on a colony drills into its saves. Enter on a save drills into detail. Enter on Load/Delete activates them. Escape goes back one level.

`_pendingViewTransition` handles the case where `colonyViewRoot` is not yet active when Enter is pressed on a colony (transition happens asynchronously).

---

## class SaveLoadHandler : BaseWidgetHandler (line 17)

  **Private Enum**
  - `private enum ViewLevel { ColonyList, SaveList, SaveDetail }` (line 18)

  **Fields**
  - `private ViewLevel _viewLevel` (line 19)
  - `private bool _pendingViewTransition` (line 20) — set true when a colony click doesn't immediately activate colonyViewRoot; checked each Tick
  - `private HierarchyReferences _selectedSaveEntry` (line 21) — the HierarchyReferences of the currently selected save row (held for detail view)
  - `private int _saveListCursorIndex` (line 22) — cursor index within the save list, preserved when going into detail and restored on back

  **Properties**
  - `public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.SAVE_LOAD` (line 24)
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 26)

  **Constructor**
  - `public SaveLoadHandler(KScreen screen) : base(screen)` (line 28)

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 37) — dispatches by `_viewLevel`
  - `private void DiscoverColonyList(KScreen screen)` (line 61) — adds management buttons (colonyInfoButton, colonyCloudButton, colonyLocalButton); walks `saveButtonRoot` children for colony entry `HierarchyReferences`; adds `loadMoreButton`; falls back to `DiscoverColonyListFallback` if `saveButtonRoot` inaccessible
  - `private void AddTraverseButton(Traverse traverse, string fieldName, string fallbackLabel)` (line 133) — adds a ButtonWidget from a named Traverse field; reads child LocText for label, falls back to provided string
  - `private string BuildColonyEntryLabel(HierarchyReferences entry)` (line 161) — assembles "HeaderTitle, SaveTitle, HeaderDate, LocationText" from named HierRef fields
  - `private string GetReferenceText(HierarchyReferences refs, string refName)` (line 193) — reads LocText from a named HierRef reference; uses non-generic `GetReference` because `LoadScreen` stores references as `RectTransform` not `LocText`
  - `private void AddDetailField(HierarchyReferences refs, string refName)` (line 213) — adds a LabelWidget from a named HierRef field, scoped to the field's own GameObject
  - `private void DiscoverColonyListFallback(KScreen screen)` (line 235) — fallback: walks all `KButton` children with LocText labels
  - `private void DiscoverColonySaves(KScreen screen)` (line 263) — reads `colonyViewRoot`, accesses `Content` container via HierarchyReferences, walks clone children; falls back to `DiscoverColonySavesFallback`
  - `private void DiscoverSaveDetail(KScreen screen)` (line 334) — guards against destroyed `_selectedSaveEntry`; reads standard detail fields from `colonyViewRoot` HierRefs; adds AutoInfo if active; adds Load button from the save entry clone's `LoadButton` ref; adds Delete button from the view panel's `DeleteButton` ref
  - `private string BuildSaveEntryLabel(HierarchyReferences entry)` (line 448) — assembles "[newest] [auto-save] SaveText, DateText"
  - `private bool IsLabelActive(HierarchyReferences refs, string refName)` (line 474) — checks if a named HierRef reference's gameObject is `activeInHierarchy`
  - `private void DiscoverColonySavesFallback(UnityEngine.GameObject viewRoot)` (line 489) — fallback: walks all `KButton` children with LocText labels

  **View Transitions (Enter/Activate)**
  - `protected override void ActivateCurrentItem()` (line 518) — ColonyList: clicks colony_entry button and transitions to SaveView (or sets `_pendingViewTransition`); other ColonyList buttons use base; SaveList: transitions to SaveDetail; SaveDetail: uses base
  - `public override bool Tick()` (line 557) — checks `_pendingViewTransition`; detects stale (destroyed) widget GameObjects and rediscovers; then delegates to base
  - `public override bool HandleKeyDown(KButtonEvent e)` (line 587) — Escape in SaveDetail -> back to SaveList; Escape in SaveList -> back to ColonyList
  - `private void TransitionToSaveView()` (line 606) — sets SaveList level, rediscovers, speaks first widget
  - `private void TransitionToSaveDetail()` (line 620) — saves cursor index; stores `_selectedSaveEntry`; clicks row button to trigger `ShowColonySave()`; switches to SaveDetail level
  - `private void TransitionToSaveList()` (line 643) — clears `_selectedSaveEntry`; switches to SaveList; restores `_saveListCursorIndex`
  - `private void TransitionToColonyList()` (line 663) — clicks the `Back` button from `colonyViewRoot` HierRefs; switches to ColonyList; speaks display name then first widget
  - `private bool IsColonyViewRootActive()` (line 695) — checks `colonyViewRoot.activeInHierarchy` via Traverse
