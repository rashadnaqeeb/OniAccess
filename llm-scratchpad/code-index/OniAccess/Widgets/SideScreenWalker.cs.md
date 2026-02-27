// Recursively walks a SideScreenContent's widget hierarchy and emits
// Widget items. Priority order per node: KSlider, KToggle,
// MultiToggle, KNumberInputField, KInputField, KButton, LocText.
// When an interactive component is found on a node, that node is
// consumed and its children are not walked further.
// Inactive GameObjects and mouse-only controls are skipped.
// All SpeechFuncs read live component state via GetParsedText().

namespace OniAccess.Widgets

static class SideScreenWalker (line 17)

  class RadioMember (line 18)
    // Data bag used to represent one option in a radio group / collapsed dropdown.
    string Label (line 19)
    KToggle Toggle (line 20)
    MultiToggle MultiToggleRef (line 21)
    object Tag (line 22)

  // ---- PUBLIC API ----

  static void Walk(SideScreenContent screen, List<Widget> items) (line 30)
    // Entry point. Dispatches to specialized walkers for PixelPackSideScreen,
    // CommandModuleSideScreen, and ConditionListSideScreen. Otherwise walks
    // ContentContainer (falling back to screen root), then picks up widgets
    // outside ContentContainer (e.g., AutomatableSideScreen extras).
    // After walking: removes claimed label widgets, then applies
    // CollapseAlarmTypeButtons (AlarmSideScreen), CollapseFewOptionRows
    // (FewOptionSideScreen), and CollapseRadioToggles for all screens.

  internal static bool IsToggleActive(KToggle toggle) (line 909)
    // Returns true when the toggle is visually "on/selected". Prefers
    // ImageToggleState.GetIsActive() because some screens (ThresholdSwitchSideScreen)
    // use KToggle.isOn inversely. Falls back to toggle.isOn when no ImageToggleState.

  // ---- SPECIALIZED SCREEN WALKERS (private) ----

  private static void WalkPixelPackScreen(PixelPackSideScreen pixelPack, List<Widget> items, HashSet<LocText> claimedLabels) (line 91)
    // Builds palette swatch group (drillable LabelWidget with ButtonWidget children),
    // active colors group, standby colors group, and action buttons (copy/swap).
    // Uses ColorNameUtil.GetColorName for swatch labels.

  private static void WalkConditionContainer(GameObject container, List<Widget> items, HashSet<LocText> claimedLabels) (line 224)
    // Iterates active children of a container and calls TryAddConditionRow on each.
    // Used for CommandModuleSideScreen and ConditionListSideScreen.

  private static void WalkCommandModuleExtras(CommandModuleSideScreen screen, List<Widget> items, HashSet<LocText> claimedLabels) (line 236)
    // Adds the destinationButton as a ToggleWidget with MultiToggle state speech.

  private static void WalkTransform(Transform parent, List<Widget> items, HashSet<LocText> claimedLabels) (line 260)
    // Recursive DFS. Per child: skips inactive/skipped nodes, then tries
    // TryAddConditionRow, TryAddCategoryContainer, TryAddSelectionCategoryContainer,
    // TryAddWidget. Recurses only when none of those succeeds.

  // ---- WIDGET DETECTION (private) ----

  private static bool TryAddWidget(Transform t, List<Widget> items, HashSet<LocText> claimedLabels) (line 277)
    // Attempts to emit a widget for a single transform. Returns true if a component was matched
    // (so the caller knows not to recurse into children). Priority order:
    //   1. ReceptacleToggle (compound: title + amount + selection state)
    //   2. SingleItemSelectionRow (compound: labelText + button + IsSelected)
    //   3. KSlider -> SliderWidget
    //   4. KToggle -> ToggleWidget
    //   5. MultiToggle -> ToggleWidget (skipped if IsRedundantMultiToggle)
    //   6. KNumberInputField -> TextInputWidget (captures following sibling units LocText)
    //   7. KInputField -> TextInputWidget
    //   8. KButton -> ToggleWidget (for PlayerControlledToggleSideScreen) or ButtonWidget
    //   9. LocText -> LabelWidget (if text has visible content)
    // All SpeechFuncs close over live component references for current-state reads.

  private static bool TryAddConditionRow(Transform t, List<Widget> items, HashSet<LocText> claimedLabels) (line 565)
    // Detects a launch condition row: HierarchyReferences with "Label" (LocText) and
    // "Check" (Image, active = condition met). Emits a LabelWidget with "met/not met, {text}" speech.

  private static bool TryAddCategoryContainer(Transform t, List<Widget> items, HashSet<LocText> claimedLabels) (line 609)
    // Detects a ReceptacleSideScreen category: HierarchyReferences with "HeaderLabel" +
    // "GridLayout" whose grid children have ReceptacleToggle. Emits a drillable LabelWidget
    // with Children built from the grid's ReceptacleToggle rows.

  private static bool TryAddSelectionCategoryContainer(Transform t, List<Widget> items, HashSet<LocText> claimedLabels) (line 671)
    // Detects a SingleItemSelectionSideScreenBase category: HierarchyReferences with
    // "Label" + "Entries" refs whose children have SingleItemSelectionRow. Emits a
    // drillable LabelWidget with Children from the entries list.

  // ---- RADIO / DROPDOWN COLLAPSING (private) ----

  private static void CollapseRadioToggles(List<Widget> items, string screenTitle, Transform screenRoot, HashSet<LocText> claimedLabels) (line 995)
    // Detects consecutive KToggle ToggleWidgets sharing the same parent where exactly one
    // is active (radio-style mutual exclusion). Replaces the run with a single DropdownWidget
    // whose Tag is List<RadioMember>. Appends an orphan description LocText to speech if found.

  private static void CollapseAlarmTypeButtons(AlarmSideScreen alarm, List<Widget> items, HashSet<LocText> claimedLabels) (line 1092)
    // AlarmSideScreen has 3 icon-only MultiToggle type buttons. IsRedundantMultiToggle kills
    // siblings so the walker only emits one. This replaces that item with a DropdownWidget
    // built from toggles_by_type (read via Harmony Traverse), using tooltip text as labels.
    // Reads notificationType live from alarm.targetAlarm for current selection.

  private static void CollapseFewOptionRows(FewOptionSideScreen fewOption, List<Widget> items, HashSet<LocText> claimedLabels) (line 1180)
    // FewOptionSideScreen spawns MultiToggle rows as siblings. IsRedundantMultiToggle kills
    // all but the first. Replaces all row items with a single DropdownWidget built from
    // fewOption.rows and targetFewOptions.GetSelectedOption() for current selection.
    // Reads tooltip from selected row for description suffix.

  // ---- RECEPTACLE HELPERS (private) ----

  private static string GetReceptacleDescription(ReceptacleToggle rt) (line 732)
    // Reads tooltip text from a ReceptacleToggle, splits on "\n\n", strips Unity rich
    // text tags from the second segment (the description), and returns it, or null.

  // ---- SIBLING/REDUNDANCY CHECKS (private) ----

  private static bool IsRedundantMultiToggle(Transform t) (line 760)
    // Returns true if a sibling KToggle already represents this row's toggle,
    // or a preceding sibling MultiToggle already represents the row (second MT = expand arrow).

  private static bool HasVisibleContent(string text) (line 790)
    // Returns true if text contains at least one character that is not whitespace,
    // Unicode Format, or OtherNotAssigned. Rejects zero-width/format chars injected by TextMeshPro.

  // ---- LABEL RESOLUTION (private) ----

  private static LocText FindChildLocText(Transform t, Component exclude) (line 812)
    // Finds first active child LocText with visible parsed text. Skips excluded component's GameObject.

  private static LocText FindSiblingLocText(Transform t) (line 828)
    // Finds a sibling LocText for input field labels. Searches preceding siblings first,
    // then following siblings. Uses FindDirectOrSafeChildLocText (no deep descent).

  private static LocText FindDirectOrSafeChildLocText(Transform sibling) (line 858)
    // Returns the LocText directly on a sibling transform (GetComponent, not GetComponentInChildren).
    // Intentionally shallow: a nested LocText inside a container belongs to that container.

  private static LocText FindFollowingSiblingLocText(Transform t) (line 866)
    // Finds first LocText among following siblings only. Used to capture a units suffix
    // (e.g. "kg") for number input fields. Stops if a sibling has interactive descendants,
    // or if the next active sibling after the candidate contains a widget (it's a label, not units).

  private static bool NextActiveSiblingHasWidget(Transform parent, int afterIndex) (line 893)
    // Returns true if the next active sibling after afterIndex has an interactive descendant.
    // Used by FindFollowingSiblingLocText to distinguish units suffixes from widget labels.

  private static bool HasInteractiveDescendant(Transform t) (line 915)
    // Returns true if the transform's subtree contains KSlider, KToggle, MultiToggle,
    // KNumberInputField, or KInputField.

  private static string ReadLocText(LocText lt, string fallback) (line 927)
    // Returns GetParsedText() if it has visible content, otherwise returns fallback.

  private static string GetButtonLabel(KButton button, string fallback) (line 941)
    // Reads label from button's child LocText via GetParsedText(). Falls back to the
    // enclosing SideScreenContent's GetTitle(), then to fallback.

  private static bool IsSkipped(string name) (line 961)
    // Returns true for mouse-only UI element names: Scrollbar, Drag, Resize,
    // increment/decrement prefixes, and "Inc "/"Dec " abbreviated step button prefixes.

  private static bool IsChrome(Transform t) (line 978)
    // Returns true for screen-level chrome names containing Title, Header, or CloseButton.
    // Used when walking outside ContentContainer to exclude non-navigable top chrome.

  private static LocText FindOrphanDescription(Transform root, HashSet<GameObject> emittedObjects, HashSet<LocText> claimedLabels) (line 1271)
    // Searches full screen tree for a LocText not yet emitted or claimed.
    // Skips LocTexts inside interactive widgets (those are labels) and the screen title.
    // Used by CollapseRadioToggles to append a description to a radio Dropdown widget.
