# BuildToolHandler.cs

## File-level notes
Non-modal handler for build tool placement. Sits on top of TileCursorHandler, intercepts build-specific keys (Space, R, Tab, I, 0-9, Shift+Space) and passes arrows through to the tile cursor. Handles both regular buildings (single-cell via BuildTool) and utility buildings (straight-line path via UtilityBuildTool/WireBuildTool).

---

```
class BuildToolHandler : BaseScreenHandler (line 19)

  // Fields
  public static BuildToolHandler Instance { get; private set; } (line 20)
  private HashedString _category (line 22)
  internal BuildingDef _def (line 23)
  private bool _isUtility (line 24)
  internal bool SuppressToolEvents { get; set; } (line 26)
  private int _utilityStartCell = Grid.InvalidCell (line 29)
  internal bool UtilityStartSet (line 30)       // => _utilityStartCell != Grid.InvalidCell
  internal int UtilityStartCell (line 31)       // => _utilityStartCell

  private static readonly ConsumedKey[] _consumedKeys (line 33)  // Space, Shift+Space, Return, R, Tab, I, Alpha0-9
  public override IReadOnlyList<ConsumedKey> ConsumedKeys (line 51)
  public override string DisplayName (line 53)        // => BuildMenuData.BuildNameAnnouncement(_def)
  public override bool CapturesAllInput (line 54)     // => false

  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 56)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 67)

  public BuildToolHandler(HashedString category, BuildingDef def) (line 69)

  // Lifecycle

  // Registers for game's OnActiveToolChanged event (hash 1174281782)
  public override void OnActivate() (line 79)

  // Called by ActionMenuHandler after SelectBuilding returns; announces building name +
  // prebuild error or switches to build mode and announces material summary.
  internal void AnnounceInitialState() (line 92)

  // Sets TileCursor.ActiveToolComposer to the appropriate tool profile composer
  private void SetupBuildMode() (line 106)

  // Unregisters tool-changed event, clears ActiveToolComposer, resets utility start cell
  public override void OnDeactivate() (line 114)

  // Handles game's tool-changed event: SelectTool -> announce placed/canceled + pop;
  // BuildTool/UtilityBuildTool -> setup build mode + queue material;
  // PrebuildTool -> clear composer + speak prebuild error
  private void OnActiveToolChanged(object data) (line 126)

  private bool IsInPrebuildMode() (line 154)   // => PlayerController.Instance.ActiveTool is PrebuildTool
  private string GetPrebuildError() (line 157) // Reads PrebuildToolHoverTextCard.errorMessage

  // Key handling

  // Handles Space (place / utility two-phase), Shift+Space (cancel construction / clear utility start),
  // Return (place+exit), R (rotate), Tab (back to list), I (info panel), 0-9 (priority)
  public override bool Tick() (line 166)

  // Intercepts Escape (close everything), RotateBuilding action, and Plan1-Plan14 actions to suppress them
  public override bool HandleKeyDown(KButtonEvent e) (line 239)

  // Regular placement

  // Places building at cursor cell via BuildTool click simulation; announces placed/obstructed/material status
  private void RegularPlacement() (line 257)

  // Places building and exits build mode; same validation as RegularPlacement
  private void RegularPlaceAndExit() (line 286)

  // Checks material availability via def.MaterialsAvailable; returns true on error (fail-open)
  private bool HasSufficientMaterials() (line 315)

  // Utility placement

  // Two-phase: first Space sets start cell, second Space validates straight line and places all cells
  private void UtilityPlacement() (line 332)

  // Places a single cell utility segment and exits build mode
  private void UtilityPlaceAndExit() (line 390)

  // Builds list of grid cells along a straight horizontal or vertical line from startCell to endCell
  private static List<int> BuildLinePath(int startCell, int endCell) (line 418)

  // Public entry for ToolProfiles to check whether a utility line is fully valid before displaying it.
  // Returns true if every cell in the line passes IsValidPlaceLocation.
  internal static bool IsUtilityLineValid(int startCell, int endCell) (line 444)

  // Validates that every cell in path passes IsValidPlaceLocation
  private bool ValidateUtilityPath(List<int> path) (line 451)

  // Reflected access to BaseUtilityBuildTool.OnDragTool (private in game code)
  private static readonly MethodInfo _onDragTool (line 461)

  // Simulates the game's drag interaction: OnLeftClickDown on first cell,
  // OnDragTool for intermediate cells via reflection, OnLeftClickUp on last cell
  private void SimulateUtilityDrag(List<int> path, BaseUtilityBuildTool tool) (line 464)

  // Returns the active WireBuildTool, UtilityBuildTool, or BaseUtilityBuildTool from PlayerController
  private BaseUtilityBuildTool GetActiveUtilityTool() (line 482)

  // Returns typeof(WireBuildTool) if def has a Wire component, else typeof(UtilityBuildTool)
  private Type GetUtilityToolType() (line 490)

  // Rotation

  // Calls BuildTool.TryRotate, reads new orientation, validates placement, appends extent text
  private void Rotate() (line 500)

  // Builds a human-readable extent description for buildings larger than 1x1, e.g. "extends 1 left, 2 up".
  // Rotates each PlacementOffset by the given orientation before computing min/max bounds.
  internal static string BuildExtentText(Orientation orientation) (line 529)

  // Quick cancel

  // Cancels a queued construction at the cursor cell by triggering GameHashes.Cancel on its GameObject
  private void QuickCancel() (line 568)

  // Searches all relevant ObjectLayers at cell for a BuildingUnderConstruction matching _def
  private UnityEngine.GameObject FindMatchingConstruction(int cell) (line 584)

  // Navigation

  // Replaces handler with ActionMenuHandler seeded to restore cursor to current building
  private void ReturnToBuildingList() (line 609)

  // Pushes BuildInfoHandler for the current def
  private void OpenInfoPanel() (line 613)

  // Priority

  // Sets build priority via materialSelectionPanel.PriorityScreen; 0 maps to topPriority/emergency
  private void SetPriority(int value) (line 621)

  // Close and cleanup

  // Escape handler: unsubscribes event, queues overlay announcement, pops, activates SelectTool, speaks "canceled"
  private void CloseEverything() (line 642)

  // Used after place+exit: same as CloseEverything but without speaking "canceled"
  private void ExitBuildMode() (line 658)

  // Walks handler stack to find TileCursorHandler and queue its next overlay announcement before popping
  private void QueueOverlayAndPop() (line 673)

  // Sounds

  private static void PlayDeactivateSound() (line 687)   // plays "Tile_Cancel"
  private static void PlayNegativeSound() (line 695)     // plays "Negative"
  private static void PlayCancelSound() (line 703)       // plays "Tile_Confirm_NegativeTool"
```
