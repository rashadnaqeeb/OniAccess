namespace OniAccess.Handlers.Screens.Details

// Reads the SimpleInfoScreen (Status tab) into structured sections.
// The most information-dense tab: status items, storage, vitals, effects,
// requirements, stress, fertility, process conditions, world panels, etc.
// All widgets use SpeechFunc for live text — the game updates labels every frame.

class StatusTab : IDetailTab (line 14)
  public string DisplayName { get; }  (line 15)  // => STRINGS.UI.DETAILTABS.SIMPLEINFO.NAME
  public int StartLevel { get; }      (line 16)  // => 0
  public string GameTabId { get; }    (line 17)  // => "SIMPLEINFO"

  public bool IsAvailable(GameObject target) (line 19)  // always returns true

  public void OnTabSelected() (line 21)  // no-op (game tab)

  public void Populate(GameObject target, List<DetailSection> sections) (line 23)
  // Calls panel.SetTarget(target), then delegates to the section-specific helpers
  // in this order: process conditions, status items, spacePOIPanel, spaceHexCellStoragePanel,
  // rocketStatusContainer, vitals, fertilityPanel, mooFertilityPanel, infoPanel,
  // requirementsPanel/requirementContent, effectsPanel/effectsContent,
  // worldMeteorShowersPanel, worldElementsPanel, worldGeysersPanel, worldTraitsPanel,
  // worldBiomesPanel, worldLifePanel, storage, stressPanel, movePanel.

  // --- STATUS ITEMS ---

  private static void AddStatusItems(SimpleInfoScreen panel, List<DetailSection> sections) (line 59)
  // Reads panel.statusItems via Traverse and panel.statusItemPanel.
  // Clickable entries (button != null && button.enabled) become ButtonWidgets;
  // non-clickable become LabelWidgets. Both read text live from the LocText field.

  // --- STORAGE ---

  private static void AddStorage(SimpleInfoScreen panel, List<DetailSection> sections) (line 115)
  // Reads panel.StoragePanel. Iterates children skipping groupRows (already captured
  // as children of their DetailCollapsableLabel). Dispatches to AddStorageGroup,
  // AddStorageItem, or plain LabelWidget.

  private static void AddStorageItem(DetailLabelWithButton item, DetailSection section) (line 172)
  // Wraps a DetailLabelWithButton as a ButtonWidget. SpeechFunc calls BuildStorageItemText.

  private static void AddStorageGroup(DetailCollapsableLabel group, DetailSection section) (line 182)
  // Creates a LabelWidget for the group header (name + value) with LabelWidget.Children
  // populated from contentRows. Calls ManualTriggerOnExpanded() to populate rows
  // from live storage data before reading them.

  // Concatenates label, label2, and label3 from a DetailLabelWithButton.
  private static string BuildStorageItemText(DetailLabelWithButton item) (line 212)

  // --- VITALS ---

  private static void AddVitals(SimpleInfoScreen panel, List<DetailSection> sections) (line 227)
  // Reads panel.vitalsPanel via Traverse. Processes amountsLines, attributesLines,
  // and checkboxLines. Checkbox items prepend CONDITION_MET or CONDITION_NOT_MET
  // based on the "Check" HierarchyReference's active state.

  private static void AddVitalLines(IEnumerable<MinionVitalsPanel.AmountLine> lines, DetailSection section) (line 270)

  private static void AddVitalLines(IEnumerable<MinionVitalsPanel.AttributeLine> lines, DetailSection section) (line 283)

  // --- COLLAPSIBLE SECTION (generic) ---

  private static void AddCollapsibleSection(SimpleInfoScreen panel, string fieldName, List<DetailSection> sections) (line 300)
  // Reads a named CollapsibleDetailContentPanel field via Traverse, delegates to BuildCollapsibleSection.

  private static DetailSection BuildCollapsibleSection(CollapsibleDetailContentPanel gameSection) (line 317)
  // Iterates Content children; DetailLabelWithButton -> ButtonWidget, DetailLabel -> LabelWidget.

  // --- DESCRIPTOR SECTIONS (effects, requirements) ---

  private static void AddDescriptorSection(SimpleInfoScreen panel, string panelFieldName, string contentFieldName, List<DetailSection> sections) (line 362)
  // Reads a wrapper CollapsibleDetailContentPanel and a DescriptorPanel via Traverse.
  // Indented LocText children (leading space) are folded into the preceding item's SpeechFunc.

  // --- PROCESS CONDITIONS ---

  private static void AddProcessConditions(SimpleInfoScreen panel, List<DetailSection> sections) (line 452)
  // Reads panel.processConditionContainer via Traverse.
  // Children with no "Box" HierarchyReference are section headers;
  // children with "Box" are condition rows (with checkbox).

  // --- WORLD PANELS ---

  private static void AddWorldPanel(SimpleInfoScreen panel, string fieldName, List<DetailSection> sections) (line 516)
  // Reads a named CollapsibleDetailContentPanel field via Traverse.
  // Children with HierarchyReferences -> AddWorldRow; fallback to plain DetailLabel.

  private static void AddWorldRow(HierarchyReferences refs, GameObject go, DetailSection section) (line 562)
  // Reads NameLabel, ValueLabel, DescriptionLabel refs. SpeechFunc concatenates
  // active labels. Returns without adding if NameLabel is missing.

  // --- PANEL LOOKUP ---

  private static SimpleInfoScreen FindPanel() (line 603)
  // Navigates DetailsScreen -> tabHeader -> tabPanels["SIMPLEINFO"].
