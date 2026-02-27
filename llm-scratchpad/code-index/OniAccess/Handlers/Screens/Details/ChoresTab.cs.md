namespace OniAccess.Handlers.Screens.Details

// Reads the BuildingChoresPanel (Errands tab) into structured sections.
// Each chore is a section (level 0) with dupe rows as items (level 1).
// Dupe rows are Button widgets — Enter pans the camera to the dupe.
//
// Section headers are enriched beyond the game's generic ChoreLabel text
// by reading the actual Chore objects from GlobalChoreProvider. For fetch
// chores, the header includes the fetch target name (e.g., "Cook Supply:
// Gristle Berry" instead of just "Cook Supply").

class ChoresTab : IDetailTab (line 18)
  public string DisplayName { get; }  (line 19)  // => STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME
  public int StartLevel { get; }      (line 20)  // => 0
  public string GameTabId { get; }    (line 21)  // => "BUILDINGCHORES"

  public bool IsAvailable(GameObject target) (line 23)  // always returns true

  public void OnTabSelected() (line 25)  // no-op (game tab)

  public void Populate(GameObject target, List<DetailSection> sections) (line 27)
  // Forces BuildingChoresPanel.Refresh() via Traverse, then walks EntriesContainer
  // children matching them with CollectChores() results for enriched headers.

  // Build a descriptive header for a chore. For FetchChore types with a
  // known fetch target, appends the target name. For operation chores on
  // buildings with a ComplexFabricator, appends the recipe name.
  // Otherwise uses the game's standard chore name.
  private static string GetChoreHeader(Chore chore) (line 116)

  // If the building has a ComplexFabricator, return the name of the
  // current or next recipe. Returns null if no fabricator or no order.
  private static string GetRecipeName(GameObject building) (line 143)

  // Collect the Chore objects for a building in the same order the game's
  // BuildingChoresPanel.RefreshDetails processes them: first from
  // choreWorldMap, then from fetchMap.
  private static List<Chore> CollectChores(GameObject target) (line 156)

  private static void AddNoErrandsSection(List<DetailSection> sections) (line 183)

  private static BuildingChoresPanel FindPanel() (line 189)
  // Navigates DetailsScreen -> tabHeader -> tabPanels["BUILDINGCHORES"].
