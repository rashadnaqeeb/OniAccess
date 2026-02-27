# ToolHandler.cs

## File-level notes
Non-modal handler for tool mode. Sits on top of TileCursorHandler, intercepts tool-specific keys (Space, Enter, Escape, 0-9, F, Delete, Ctrl+Arrows) and passes everything else through to the tile cursor. Manages rectangle selection state. On confirm, submits each accumulated rectangle to the game via DragTool.OnLeftClickDown/OnLeftClickUp.

---

```
class ToolHandler : BaseScreenHandler (line 19)

  // Fields
  public static ToolHandler Instance { get; private set; } (line 20)
  private int _pendingFirstCorner = Grid.InvalidCell (line 22)   // first corner of an in-progress rectangle
  private readonly List<RectCorners> _rectangles (line 23)       // committed rectangles not yet submitted
  private ModToolInfo _toolInfo (line 24)
  private string _lastFilterKey (line 25)

  // Private struct holding two cell corners of a rectangle
  private struct RectCorners (line 27)
    public int Cell1 (line 28)
    public int Cell2 (line 29)
    // Computes min/max X and Y from the two corner cells
    public void GetBounds(out int minX, out int maxX, out int minY, out int maxY) (line 31)
    // Returns true if the cell falls within the rectangle bounds
    public bool Contains(int cell) (line 38)

  private static readonly ConsumedKey[] _consumedKeys (line 46)
  public override IReadOnlyList<ConsumedKey> ConsumedKeys (line 67)
  public override string DisplayName (line 69)        // => BuildActivationAnnouncement()
  public override bool CapturesAllInput (line 70)     // => false

  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 72)
  public override IReadOnlyList<HelpEntry> HelpEntries (line 82)

  // Lifecycle

  // Registers tool-changed and overlay-changed events; sets TileCursor.ActiveToolComposer;
  // initializes _lastFilterKey; speaks DisplayName
  public override void OnActivate() (line 88)

  // Unregisters events; clears ActiveToolComposer, _lastFilterKey, rectangles, pending corner
  public override void OnDeactivate() (line 119)

  // If SelectTool becomes active (user dismissed tool via UI), announces "canceled" and pops
  private void OnActiveToolChanged(object data) (line 136)

  // Detects overlay changes and maps them to filter keys.
  // Clears selection if filter changed, announces new filter or "filter removed".
  private void OnOverlayChanged(HashedString newMode) (line 144)

  // Maps overlay mode HashedString to the ToolParameterMenu filter key the game will apply.
  // Mirrors FilteredDragTool.OnOverlayChanged logic so the mod doesn't need to read ToolParameterMenu after the fact.
  private static string FilterKeyForOverlay(HashedString overlay) (line 170)

  // Key handling

  // Space: SetCorner; Enter: ConfirmOrCancel; Delete/Backspace: ClearRectAtCursor;
  // F: OpenFilterMenu; 0-9: SetPriority (if tool supports it); Ctrl+Arrows: JumpToSelectionBoundary
  public override bool Tick() (line 188)

  // Escape: announces "canceled", plays deactivate sound, deactivates tool and pops
  public override bool HandleKeyDown(KButtonEvent e) (line 245)

  // Rectangle selection

  // Space handler: sets first corner on first press; on second press validates (line mode check) and adds rectangle.
  // Line mode (IsLineMode flag): rejects second corner if not orthogonally adjacent.
  private void SetCorner() (line 259)

  // Enter handler: if no rectangles and no pending corner, creates a 1x1 rect at cursor.
  // If still no rectangles after that, cancels. Otherwise calls SubmitRectangles.
  private void ConfirmOrCancel() (line 302)

  // Submits all accumulated rectangles to the active DragTool via OnLeftClickDown/OnLeftClickUp pairs.
  // Suppresses the game's confirm sound for all but the last rectangle.
  private void SubmitRectangles() (line 322)

  // Cell selection queries

  // Returns true if the cell is inside any committed rectangle
  public bool IsCellSelected(int cell) (line 361)

  // Delete/Backspace handler: removes the last rectangle that contains the cursor cell
  private void ClearRectAtCursor() (line 369)

  // Jump navigation

  // Ctrl+Arrow handler: walks cells in direction until the selected/unselected state changes,
  // then jumps the tile cursor to that boundary cell
  private void JumpToSelectionBoundary(Tiles.Direction direction) (line 384)

  // Priority

  // 0-9 handler: 0 maps to topPriority/emergency, 1-9 to basic priority; updates ToolMenu.PriorityScreen
  private void SetPriority(int value) (line 409)

  // Filter menu

  // F handler: pushes ToolFilterHandler if tool has a filter menu
  private void OpenFilterMenu() (line 429)

  internal bool HasSelection (line 435)    // true if any rectangles or a pending first corner
  internal void ClearSelection() (line 437)

  // Announcements

  // Builds the activation announcement: "ToolName, filter, priority" in various combinations.
  // Reads live priority from ToolMenu.PriorityScreen and live filter from ToolParameterMenu.
  private string BuildActivationAnnouncement() (line 446)

  // Reads the currently active filter's display name from ToolParameterMenu currentParameters
  private static string ReadActiveFilterName() (line 480)

  // Reads the currently active filter's key string from ToolParameterMenu currentParameters
  private static string ReadActiveFilterKey() (line 494)

  // Builds the summary spoken after setting a rectangle: "W x H, N valid, M invalid"
  private string BuildRectSummary(RectCorners rect) (line 507)

  // Builds the summary spoken on confirm: unique cell count + optional priority text.
  // Uses CountTargets delegate if available to count valid targets per cell.
  private string BuildConfirmSummary(out int total) (line 539)

  // Formats the confirm string using tool's ConfirmFormat, count, priority, and singular/plural noun
  private string GetConfirmString(int count, string priority) (line 571)

  // Area computation

  private static int ComputeArea(RectCorners rect) (line 585)

  // Sounds

  // Plays the tool's DragSound with a tileCount parameter (FMOD event)
  private void PlayDragSound(int tileCount) (line 595)

  // Unsubscribes from tool-changed event, queues TileCursorHandler overlay announcement,
  // clears ToolMenu selection, activates SelectTool, then pops the handler
  private void DeactivateToolAndPop() (line 607)

  private static void PlayDeactivateSound() (line 620)  // plays "Tile_Cancel"
  private static void PlayNegativeSound() (line 628)    // plays "Negative"

  // Tool info registry

  // Lazily built, immutable list of all known toolbar tools
  internal static IReadOnlyList<ModToolInfo> AllTools (line 640)
  private static IReadOnlyList<ModToolInfo> _allTools (line 641)
  private static Dictionary<Type, ModToolInfo> _toolMap (line 643)

  // Lazily builds a Type -> ModToolInfo dictionary for fast lookup
  private static Dictionary<Type, ModToolInfo> GetToolMap() (line 645)

  // Looks up ModToolInfo for the given active tool's runtime type
  internal static ModToolInfo FindToolInfo(InterfaceTool activeTool) (line 653)

  // Constructs the hardcoded list of 12 ModToolInfo entries (Dig, Cancel, Deconstruct, Prioritize,
  // Disinfect, Clear/Sweep, Attack, Mop, Capture, Harvest, EmptyPipe, Disconnect)
  private static IReadOnlyList<ModToolInfo> BuildAllTools() (line 659)

  // Cell validators (all take a cell index, return count of valid targets at that cell)

  private static int CountDigTargets(int cell) (line 740)       // solid non-foundation, no existing dig placer
  private static int CountMopTargets(int cell) (line 746)       // liquid <= 150g on solid floor, no mop placer
  private static int CountSweepTargets(int cell) (line 755)     // clearable non-minion pickupables
  private static int CountDisinfectTargets(int cell) (line 773) // any disinfectable with disease count > 0
  private static int CountHarvestTargets(int cell) (line 783)   // HarvestDesignatable at cell or occupying it
  private static int CountDeconstructTargets(int cell) (line 792) // deconstructable matching active filter layer
  private static int CountCancelTargets(int cell) (line 804)    // any object matching active filter layer
  private static int CountPrioritizeTargets(int cell) (line 815) // prioritizable object matching filter (handles pickupable lists)
  private static int CountEmptyPipeTargets(int cell) (line 842)  // IEmptyConduitWorkable on active layer
  private static int CountDisconnectTargets(int cell) (line 853) // building with IHaveUtilityNetworkMgr on active layer
  private static int CountAttackTargets(int cell) (line 867)    // non-Duplicant faction alignments at cell
  private static int CountCaptureTargets(int cell) (line 878)   // capturable components at cell

  // Filter helpers

  // Reads currentFilterTargets from the active FilteredDragTool via Traverse reflection
  private static Dictionary<string, ToolParameterMenu.ToggleState> ReadFilterTargets() (line 891)

  // Returns true if the given filter layer string is active (ALL layer counts as active for everything)
  private static bool IsFilterLayerActive(
      Dictionary<string, ToolParameterMenu.ToggleState> targets, string layer) (line 898)

  // Returns true if the given object layer index is active in the current filter set
  private static bool IsObjectLayerActive(
      Dictionary<string, ToolParameterMenu.ToggleState> targets, int layerIndex) (line 908)

  // Maps a GameObject to a filter layer key based on its component type (BuildingComplete, Diggable, etc.)
  private static string GetFilterLayer(UnityEngine.GameObject go) (line 922)

  // Maps a GameObject to a prioritize filter layer key (Construction, Dig, Clean, or Operate)
  private static string GetPrioritizeFilterLayer(UnityEngine.GameObject go) (line 934)

  // Maps ObjectLayer enum to the filter key string used by ToolParameterMenu
  private static string FilterKeyFromObjectLayer(ObjectLayer layer) (line 947)

  // Maps a filter key string back to an ObjectLayer index (inverse of FilterKeyFromObjectLayer)
  private static int? ObjectLayerFromFilterKey(string key) (line 976)
```
