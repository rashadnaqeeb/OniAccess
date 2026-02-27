# OptionsMenuHandler.cs

Namespace: `OniAccess.Handlers.Screens`

## File-level notes

Handler for the options screen family: `OptionsMenuScreen` (top-level, `KModalButtonMenu`), `AudioOptionsScreen`, `GraphicsOptionsScreen`, `GameOptionsScreen`, `FeedbackScreen`, `MetricsOptionsScreen`, and `CreditsScreen`.

Sub-screens discover sliders, `HierarchyReferences`-based toggles (with CheckMark pattern), `KToggle` checkboxes, `MultiToggle` controls, Unity `Dropdown`s, and `KButton`s. HierRef toggles sharing the same parent and same GameObject name are collapsed into a single radio-group widget (e.g., Celsius/Kelvin/Fahrenheit -> "Temperature Units"). Display name is computed from the screen type at activation.

---

## class OptionsMenuHandler : BaseWidgetHandler (line 22)

  **Static Type References**
  - `private static readonly Type OptionsMenuScreenType` (line 23)
  - `private static readonly Type AudioOptionsScreenType` (line 24)
  - `private static readonly Type GraphicsOptionsScreenType` (line 25)
  - `private static readonly Type GameOptionsScreenType` (line 26)
  - `private static readonly Type CreditsScreenType` (line 27)
  - `private static readonly Type FeedbackScreenType` (line 28)
  - `private static readonly Type MetricsOptionsScreenType` (line 29)

  **Fields**
  - `private string _displayName` (line 31)
  - `private static HashSet<string> _ambiguousLabels` (line 45) — lazy-initialized set of generic button labels (OK, Cancel, Close, Back, etc.) that should not be used as toggle labels

  **Properties**
  - `public override string DisplayName` (line 33) — returns `_displayName` if set, else Options fallback
  - `public override IReadOnlyList<HelpEntry> HelpEntries { get; }` (line 35)

  **Constructor**
  - `public OptionsMenuHandler(KScreen screen) : base(screen)` (line 37)

  **Private Nested Classes**
  - `private class RadioGroupInfo` (line 64) — holds a collapsed radio group: list of `RadioMember` and the current selection index; stored as `Widget.Tag`
    - `public List<RadioMember> Members` (line 65)
    - `public int CurrentIndex` (line 66)
  - `private class RadioMember` (line 69)
    - `public string Label` (line 70)
    - `public KButton Button` (line 71)
    - `public HierarchyReferences HierRef` (line 72)

  **Helpers**
  - `private static HashSet<string> GetAmbiguousLabels()` (line 47) — lazily constructs and returns the ambiguous labels set

  **Lifecycle**
  - `public override void OnActivate()` (line 75) — sets `_displayName` from screen type then calls base

  **Widget Discovery**
  - `public override bool DiscoverWidgets(KScreen screen)` (line 80) — dispatches to `DiscoverButtonMenuWidgets` for `OptionsMenuScreen`, or `DiscoverOptionWidgets` for sub-screens
  - `private void DiscoverButtonMenuWidgets(KScreen screen)` (line 99) — uses `buttons` IList + `buttonObjects` array pattern from `KModalButtonMenu`
  - `private void DiscoverOptionWidgets(KScreen screen)` (line 131) — routes Credits to `DiscoverCreditsWidgets`; prepends description labels for Feedback/Metrics screens; then discovers: (1) HierRef toggles with CheckMark pattern -> set `hierToggleButtons` to avoid duplication; (2) calls `CollapseRadioGroups()`; (3) `KSlider` controls; (4) `KToggle` controls; (5) `MultiToggle` controls; (6) Unity `Dropdown` controls; (7) `KButton` controls, skipping those already captured
  - `private void DiscoverScreenDescription(KScreen screen)` (line 351) — finds long LocText components (>= 25 chars) on Feedback/Metrics screens and adds them as Label widgets before interactive widgets
  - `private void DiscoverCreditsWidgets(KScreen screen)` (line 375) — walks `entryContainer` children; groups team members into one Label widget per team; appends CloseButton

  **Widget Validation**
  - `protected override bool IsWidgetValid(Widget widget)` (line 433) — extends base with Dropdown/RadioGroupInfo interactability checks

  **Dropdown Cycling**
  - `protected override void CycleDropdown(Widget widget, int direction)` (line 451) — radio group: clicks next/prev member's button and speaks new selection; Unity Dropdown: sets `value` directly and refreshes

  **Tooltip**
  - `protected override string GetTooltipText(Widget widget)` (line 481) — for radio groups, reads tooltip from the currently selected member's HierRef rather than the parent container

  **Radio Group Collapsing**
  - `private void CollapseRadioGroups()` (line 501) — groups HierRef toggle widgets by parent transform; collapses groups where all members share the same GameObject name (same prefab) into a single `DropdownWidget` with `RadioGroupInfo` tag; replaces the first widget, removes the rest; finds group label via `FindPrecedingLabel`

  **Slider Formatting**
  - `protected override string FormatSliderValue(KSlider slider)` (line 594) — reads `SliderContainer.valueLabel` for audio volume sliders; 0-1 range -> percent; range > 1 -> integer

  **Label Discovery Helpers**
  - `private string FindWidgetLabel(UnityEngine.GameObject widgetObj)` (line 620) — checks self/children LocText, then parent's sibling LocTexts; uses `CleanLabel`
  - `private string FindSiblingLabel(UnityEngine.GameObject widgetObj)` (line 647) — only checks siblings (not children of widgetObj); used for dropdowns to avoid capturing `captionText`
  - `private string FindPrecedingLabel(UnityEngine.GameObject widgetObj)` (line 674) — searches backwards through preceding siblings (direct LocText only, not deep children), then parent's own LocText, then grandparent-level preceding siblings; used for radio group labels
  - `private LocText FindGrandparentLocText(UnityEngine.GameObject widgetObj)` (line 724) — like FindPrecedingLabel but returns the LocText component itself; used for sliders to hold a reference to the game-managed value display
  - `private string StripValueSuffix(string text)` (line 760) — strips trailing "value" from "Label: value" text; returns the label part or null if no pattern
  - `private string CleanLabel(string text)` (line 777) — returns text as-is if valid; extracts last segment of `MISSING.STRINGS.*` keys and title-cases it; returns null if empty
  - `private string LabelFromGameObjectName(string name)` (line 796) — strips common suffixes (Button, Toggle, Slider, Dropdown) then splits PascalCase/camelCase into words
  - `private bool IsMouseOnlyControl(UnityEngine.GameObject obj)` (line 826) — returns true for drag handles, resize handles, scrollbars
  - `private string GetDisplayNameForScreen(KScreen screen)` (line 837) — maps screen type to display name string
