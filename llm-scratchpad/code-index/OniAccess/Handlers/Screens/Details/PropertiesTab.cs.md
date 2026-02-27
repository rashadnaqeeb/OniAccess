namespace OniAccess.Handlers.Screens.Details

// Reads the AdditionalDetailsPanel (Properties tab) into structured sections.
// Eight CollapsibleDetailContentPanel sections, each with a header and DetailLabel children.
// All widgets use SpeechFunc for live text since the game updates labels every frame.

class PropertiesTab : IDetailTab (line 11)
  public string DisplayName { get; }  (line 12)  // => STRINGS.UI.DETAILTABS.DETAILS.NAME
  public int StartLevel { get; }      (line 13)  // => 0
  public string GameTabId { get; }    (line 14)  // => "DETAILS"

  public bool IsAvailable(GameObject target) (line 16)  // always returns true

  public void OnTabSelected() (line 18)  // no-op (game tab)

  private static readonly string[] SectionFields (line 20)
  // { "detailsPanel", "immuneSystemPanel", "diseaseSourcePanel", "currentGermsPanel",
  //   "overviewPanel", "generatorsPanel", "consumersPanel", "batteriesPanel" }

  public void Populate(GameObject target, List<DetailSection> sections) (line 31)
  // Calls panel.SetTarget(target), then iterates SectionFields reading each
  // CollapsibleDetailContentPanel via Traverse and delegating to CollapsiblePanelReader.BuildSection.

  private static AdditionalDetailsPanel FindPanel() (line 62)
  // Navigates DetailsScreen -> tabHeader -> tabPanels["DETAILS"].
