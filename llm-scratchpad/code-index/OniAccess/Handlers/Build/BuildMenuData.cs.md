# BuildMenuData.cs

## File-level notes
Static helpers for querying PlanScreen categories, buildings, materials, and programmatically selecting buildings in the game. All methods re-query live game data on every call — no caching.

---

```
static class BuildMenuData (line 9)

  // Structs

  struct CategoryEntry (line 10)
    public HashedString Category (line 11)
    public string DisplayName (line 12)

  struct BuildingEntry (line 15)
    public BuildingDef Def (line 16)
    public PlanScreen.RequirementsState State (line 17)
    public string Label (line 18)

  struct SubcategoryGroup (line 21)
    public string Name (line 22)
    public List<BuildingEntry> Buildings (line 23)

  struct CategoryGroup (line 26)
    public HashedString Category (line 27)
    public string DisplayName (line 28)
    public List<SubcategoryGroup> Subcategories (line 29)

  // Returns visible top-level categories from TUNING.BUILDINGS.PLANORDER.
  // Hides categories with hideIfNotResearched when no building in them is researched.
  public static List<CategoryEntry> GetVisibleCategories() (line 37)

  // Returns visible buildings in a single category, filtering out unresearched (Tech state) and invalid.
  public static List<BuildingEntry> GetVisibleBuildings(HashedString category) (line 57)

  // Returns buildings in a category grouped by subcategory string, preserving game definition order.
  // Subcategory display names come from STRINGS.UI.NEWBUILDCATEGORIES.
  public static List<SubcategoryGroup> GetGroupedBuildings(HashedString category) (line 91)

  // Returns the full 3-level tree: categories -> subcategories -> buildings.
  // Categories with no visible buildings after filtering are excluded entirely.
  public static List<CategoryGroup> GetFullBuildTree() (line 137)

  // Programmatically selects a building in PlanScreen via OpenCategoryByName + OnSelectBuilding.
  // Triggers the full game chain: ProductInfoScreen config, material auto-selection, build tool activation.
  public static bool SelectBuilding(BuildingDef def, HashedString category) (line 161)

  // Returns true when a building is a KAnim utility tile (conduits, wires, etc.)
  public static bool IsUtilityBuilding(BuildingDef def) (line 177)

  // Maps Orientation enum to a localized direction string (Up/Right/Down/Left)
  public static string GetOrientationName(Orientation orientation) (line 181)

  // Returns a brief material summary for the auto-selected material, e.g. "copper, 25 kg".
  // Reads the live materialSelectionPanel's CurrentSelectedElement.
  public static string GetMaterialSummary(BuildingDef def) (line 197)

  // Returns building name plus facing direction string; omits direction for Unrotatable buildings.
  // Used for the immediate interrupt announcement; material is queued separately.
  public static string BuildNameAnnouncement(BuildingDef def) (line 217)

  // Reads current build orientation from BuildTool.Instance.visualizer's Rotatable component.
  public static Orientation GetCurrentOrientation() (line 230)

  // Returns true if any building in the plan info is researched (not Tech state).
  private static bool HasAnyResearchedBuilding(PlanScreen.PlanInfo planInfo) (line 238)

  // Looks up subcategory display name from STRINGS.UI.NEWBUILDCATEGORIES.<KEY>.BUILDMENUTITLE;
  // falls back to the raw key if not found.
  private static string GetSubcategoryDisplayName(string subcategoryKey) (line 251)

  // Looks up category display name from STRINGS.UI.BUILDCATEGORIES.<NAME>.NAME with link formatting stripped.
  private static string GetCategoryDisplayName(HashedString category) (line 258)

  // Builds the full label for a BuildingEntry: "Name, Effect" plus appended state details
  // (missing materials list, or tooltip text for other non-complete states).
  private static string BuildLabel(BuildingDef def, PlanScreen.RequirementsState state) (line 264)

  // Formats missing materials as "Missing resources: Tag1 mass, Tag2 mass".
  // Omits per-ingredient mass when all ingredients share the same amount (appends shared mass at end).
  private static string FormatMissingMaterials(BuildingDef def) (line 279)
```
