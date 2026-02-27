# ActionMenuHandler.cs

## File-level notes
Unified action menu combining tools and build categories. Tools appear first at level 0 index 0, with individual tools at level 1 (leaf). Build categories follow at indices 1+, with subcategories (level 1) and buildings (level 2). Type-ahead searches both tools and buildings.

---

```
class ActionMenuHandler : NestedMenuHandler (line 13)

  // Fields
  private readonly HashedString _initialCategory (line 14)
  private readonly BuildingDef _initialDef (line 15)
  private List<BuildMenuData.CategoryGroup> _tree (line 16)
  private const int ToolsCategoryIndex = 0 (line 18)
  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 20)
  private int _restoreFlatIndex = -1 (line 625)

  // Static constructor — builds _helpEntries from NestedNavHelpEntries plus Escape
  static ActionMenuHandler() (line 22)

  // Properties
  public override IReadOnlyList<HelpEntry> HelpEntries (line 29)
  public override string DisplayName (line 31)

  // Constructor — open fresh from tile cursor; _initialDef = null
  public ActionMenuHandler() (line 36)

  // Constructor — return from placement (Tab in BuildToolHandler); cursor starts on the given building
  public ActionMenuHandler(HashedString category, BuildingDef initialDef) (line 45)

  // Returns true when level-0 category index refers to the tools pseudo-category
  private static bool IsToolsCategory(int catIndex) (line 50)

  // Converts level-0 category index to _tree index (build categories occupy indices 1..N)
  private static int TreeIndex(int catIndex) (line 56)

  // NestedMenuHandler abstracts

  protected override int MaxLevel (line 62)           // => 2
  protected override int SearchLevel (line 63)        // => 2

  // Returns item count at the given level given parent indices
  protected override int GetItemCount(int level, int[] indices) (line 65)

  // Returns display label for item at [level, indices]
  protected override string GetItemLabel(int level, int[] indices) (line 82)

  // Returns the parent container label for the current position (used when crossing boundaries)
  protected override string GetParentLabel(int level, int[] indices) (line 114)

  // Activates the leaf item: tools route to ActivateToolItem; buildings push BuildToolHandler and call SelectBuilding
  protected override void ActivateLeafItem(int[] indices) (line 137)

  // Routes tool selection: opens ToolFilterHandler for mode-first tools, otherwise activates directly
  private void ActivateToolItem(int toolIndex) (line 169)

  // Level-2 cross-boundary navigation — walks forward across subcategories and categories, wrapping
  protected override void NavigateNext() (line 186)

  // Level-2 cross-boundary navigation — walks backward across subcategories and categories, wrapping
  protected override void NavigatePrev() (line 258)

  // Level-2 first — finds first building in entire tree
  protected override void NavigateFirst() (line 328)

  // Level-2 last — finds last building in entire tree
  protected override void NavigateLast() (line 350)

  // Level-2 group jump forward — jumps to first item in next non-empty subcategory
  protected override void JumpNextGroup() (line 376)

  // Level-2 group jump backward — jumps to last item in previous non-empty subcategory
  protected override void JumpPrevGroup() (line 400)

  // Scans _tree forward for next non-empty subcategory; wraps around. outCat/outSub are _tree indices
  private bool FindNextSubcategory(int cat, int sub, out int outCat, out int outSub) (line 428)

  // Scans _tree backward for previous non-empty subcategory; wraps around. outCat/outSub are _tree indices
  private bool FindPrevSubcategory(int cat, int sub, out int outCat, out int outSub) (line 468)

  // Returns total building count across all subcategories (excludes tools)
  private int GetBuildingSearchCount() (line 509)

  // Returns total search item count: tools + all buildings flat
  protected override int GetSearchItemCount(int[] indices) (line 520)

  // Returns label for flat search index (tools first, then buildings in tree order)
  protected override string GetSearchItemLabel(int flatIndex) (line 524)

  // Maps flat search index back to [catIndex, subIndex, bldIndex] level-0 indices
  protected override void MapSearchIndex(int flatIndex, int[] outIndices) (line 545)

  // Returns the target level for a search result (level 1 for tools, SearchLevel for buildings)
  protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) (line 573)

  // Lifecycle: plays open sound, builds tree, restores prior position or announces Tools category
  public override void OnActivate() (line 583)

  public override void OnDeactivate() (line 601)

  // Intercepts Escape to announce "closed" and pop; delegates other keys to base
  public override bool HandleKeyDown(KButtonEvent e) (line 610)

  // Finds the flat search index of a specific BuildingDef within a category; falls back to any category
  private void FindDefFlatIndex(BuildingDef def, HashedString category) (line 627)

  // Speaks current item with subcategory name prepended as context
  private void SpeakWithSubcategoryContext() (line 649)

  // Speaks current item with both category and subcategory names prepended as context
  private void SpeakWithCategoryContext() (line 656)

  private static void PlayOpenSound() (line 664)
  private static void PlayCloseSound() (line 668)
  private static void PlayNegativeSound() (line 672)
```
