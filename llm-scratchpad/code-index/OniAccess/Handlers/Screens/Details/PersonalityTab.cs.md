namespace OniAccess.Handlers.Screens.Details

// Reads the MinionPersonalityPanel (Bio tab) into structured sections.
// Six CollapsibleDetailContentPanel sections (bio, traits, attributes,
// resume, amenities, equipment), all populated via SetLabel/Commit.
// Same reading strategy as PropertiesTab via CollapsiblePanelReader.

class PersonalityTab : IDetailTab (line 12)
  public string DisplayName { get; }  (line 13)  // => STRINGS.UI.DETAILTABS.PERSONALITY.NAME
  public int StartLevel { get; }      (line 14)  // => 0
  public string GameTabId { get; }    (line 15)  // => "PERSONALITY"

  public bool IsAvailable(GameObject target) (line 17)  // always returns true

  public void OnTabSelected() (line 19)  // no-op (game tab)

  private static readonly string[] SectionFields (line 21)
  // { "bioPanel", "traitsPanel", "attributesPanel", "resumePanel", "amenitiesPanel", "equipmentPanel" }

  public void Populate(GameObject target, List<DetailSection> sections) (line 30)
  // Calls panel.SetTarget(target), then iterates SectionFields reading each
  // CollapsibleDetailContentPanel via Traverse and delegating to CollapsiblePanelReader.BuildSection.

  private static MinionPersonalityPanel FindPanel() (line 58)
  // Navigates DetailsScreen -> tabHeader -> tabPanels["PERSONALITY"].
