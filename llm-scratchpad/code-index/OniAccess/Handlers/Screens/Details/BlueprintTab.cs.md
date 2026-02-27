namespace OniAccess.Handlers.Screens.Details

class BlueprintTab : IDetailTab (line 8)
  // Reads the CosmeticsPanel (Blueprints side screen tab) into sections.
  // Branches on whether the selected entity is a dupe or a building.

  public string DisplayName { get; }  (line 9)  // => STRINGS.UI.DETAILTABS.COSMETICS.NAME
  public int StartLevel { get; }      (line 10)  // => 1
  public string GameTabId { get; }    (line 11)  // => null (side screen tab)

  public bool IsAvailable(GameObject target) (line 13)
  // Checks DetailsScreen.SidescreenTabTypes.Blueprints tab visibility.

  public void OnTabSelected() (line 19)
  // Clicks the Blueprints MultiToggle to activate the game's side tab.

  public void Populate(GameObject target, List<DetailSection> sections) (line 26)
  // Finds CosmeticsPanel, then dispatches to PopulateDupeMode or PopulateBuildingMode.

  private static CosmeticsPanel FindPanel() (line 37)

  private static void PopulateBuildingMode(CosmeticsPanel panel, List<DetailSection> sections) (line 47)
  // Reads nameLabel, descriptionLabel, and selectionPanel from CosmeticsPanel via Traverse.

  private static void PopulateDupeMode(CosmeticsPanel panel, List<DetailSection> sections) (line 69)
  // Reads nameLabel, editButton, selectionPanel, outfitCategories, selectedOutfitCategory via Traverse.

  private static void AddBuildingSection(LocText nameLabel, LocText descriptionLabel, FacadeSelectionPanel selectionPanel, List<DetailSection> sections) (line 99)

  private static void AddDupeInfoSection(LocText nameLabel, KButton editButton, Dictionary<ClothingOutfitUtility.OutfitType, GameObject> outfitCategories, List<DetailSection> sections) (line 133)

  private static void AddCategoryToggles(Dictionary<ClothingOutfitUtility.OutfitType, GameObject> outfitCategories, DetailSection section) (line 167)
  // Adds a ToggleWidget for each outfit category (Clothing, AtmoSuit, JetSuit).
  // SpeechFunc appends STRINGS.ONIACCESS.STATES.SELECTED when toggle state == 1.

  private static void AddFacadeGridSection(FacadeSelectionPanel selectionPanel, List<DetailSection> sections, string header) (line 197)

  private static void AddFacadeToggles(FacadeSelectionPanel selectionPanel, DetailSection section) (line 210)
  // Reads activeFacadeToggles dict from FacadeSelectionPanel via Traverse.
  // Each toggle's speech reads its tooltip (first line only) or falls back to GO name.
  // SpeechFunc appends STRINGS.ONIACCESS.STATES.SELECTED when capturedId == panel.SelectedFacade.

  private static string GetCategoryName(ClothingOutfitUtility.OutfitType type) (line 262)
  // Maps OutfitType enum to localized game string. Falls back to type.ToString().

  private static string ReadToggleName(GameObject toggleGO) (line 275)
  // Reads first line of ToolTip text; falls back to GO.name.
