// Modal picker for selecting one of multiple KSelectable entities at a tile.
// Pushed by TileCursorHandler when Enter is pressed on a cell with 2+ entities.
// Selecting an item calls SelectTool.Instance.Select() to open the details screen.

namespace OniAccess.Handlers.Tiles

class EntityPickerHandler : BaseMenuHandler (line 10)
  private readonly IReadOnlyList<KSelectable> _selectables (line 11)

  override string DisplayName { get; } (line 13)
    // Returns STRINGS.ONIACCESS.HANDLERS.ENTITY_PICKER

  override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 16)
    // Reuses ToolPickerHandler.ModalMenuHelp

  EntityPickerHandler(IReadOnlyList<KSelectable> selectables) (line 19)

  override int ItemCount { get; } (line 23)

  override string GetItemLabel(int index) (line 25)
    // Returns null for out-of-range indices; otherwise the KSelectable's GetName()

  override void SpeakCurrentItem(string parentContext = null) (line 30)

  override void OnActivate() (line 35)
    // Plays HUD_Click_Open, resets index/search, speaks SELECT_OBJECT then first item name

  override void OnDeactivate() (line 45)
    // Plays HUD_Click_Close then calls base

  protected override void ActivateCurrentItem() (line 50)
    // Pops handler BEFORE calling Select() so that DetailsScreenHandler pushed
    // synchronously by Select() ends up on top rather than being immediately popped.

  override bool HandleKeyDown(KButtonEvent e) (line 62)
    // Delegates to base; additionally handles Escape to pop and speak TOOLTIP.CLOSED

  static List<KSelectable> CollectSelectables(int cell) (line 78)
    // Queries GameScenePartitioner.collisionLayer for the cell, filters by
    // intersection with cell center, deduplicates by GameObject, skips
    // alternateSelectionObject duplicates, and sorts by EntitySortKey.

  private static int EntitySortKey(KSelectable ks) (line 113)
    // Sort order: 0 = Building layer building, 1 = non-Building layer building,
    // 2 = other (e.g. Duplicant), 3 = CellSelectionObject (tile/element)

  private static void PlaySound(string name) (line 121)
    // Wraps KFMOD.PlayUISound; logs error on exception
