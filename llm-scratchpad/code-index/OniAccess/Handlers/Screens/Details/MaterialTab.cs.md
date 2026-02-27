namespace OniAccess.Handlers.Screens.Details

class MaterialTab : IDetailTab (line 8)
  // Reads the DetailsScreenMaterialPanel (Material side screen tab) into sections.
  // Two sections: current material info and change-material selector.

  public string DisplayName { get; }  (line 9)   // => STRINGS.UI.DETAILTABS.MATERIAL.NAME
  public int StartLevel { get; }      (line 10)  // => 1
  public string GameTabId { get; }    (line 11)  // => null (side screen tab)

  public bool IsAvailable(GameObject target) (line 13)
  // Checks DetailsScreen.SidescreenTabTypes.Material tab visibility.

  public void OnTabSelected() (line 19)
  // Clicks the Material tab MultiToggle.

  public void Populate(GameObject target, List<DetailSection> sections) (line 26)
  // Calls AddCurrentMaterialSection and AddChangeMaterialSection.

  private static DetailsScreenMaterialPanel FindPanel() (line 34)

  private static void AddCurrentMaterialSection(DetailsScreenMaterialPanel panel, List<DetailSection> sections) (line 40)
  // Reads currentMaterialLabel, currentMaterialDescription, descriptorPanel via Traverse.
  // Adds LabelWidgets for label and description, then descriptor labels.

  private static void AddDescriptorLabels(DescriptorPanel descriptorPanel, DetailSection section) (line 84)
  // Reads DescriptorPanel.labels list via Traverse.
  // Skips the "Effects Header" label (it's implicit from section structure).

  private static void AddChangeMaterialSection(DetailsScreenMaterialPanel panel, List<DetailSection> sections) (line 119)
  // Reads materialSelectionPanel, openChangeMaterialPanelButton, orderChangeMaterialButton via Traverse.
  // If selection panel is active: shows material toggles + order button.
  // Otherwise if open button is active and interactable: shows it as a button widget.

  private static void AddMaterialToggles(MaterialSelectionPanel selectionPanel, DetailSection section) (line 158)
  // Reads materialSelectors via Traverse; uses only selectors[0].
  // SpeechFunc calls BuildToggleSpeech for live mass + selected state.

  // Builds "name, mass, selected" or "name, mass" for a material toggle.
  // Queries world inventory for live mass on every speech call.
  private static string BuildToggleSpeech(Tag tag, MaterialSelector selector) (line 190)

  private static void AddOrderButton(KButton orderButton, DetailSection section) (line 201)
  // Only adds the button if it is interactable.
