// Owns a cell index for tile-by-tile world navigation.
// Arrow key movement, world bounds clamping, KInputManager mouse lock,
// camera follow, coordinate reading. Speech content is delegated to
// GlanceComposer which runs the section pipeline.

namespace OniAccess.Handlers.Tiles

enum Direction (line 4)
  Up, Down, Left, Right

enum CoordinateMode (line 6)
  Off, Append, Prepend

class TileCursor (line 14)
  static TileCursor Instance { get; private set; } (line 15)

  private int _cell (line 17)
  private bool _wasPanning (line 18)
  private string _lastRoomName (line 19)
  private readonly Overlays.OverlayProfileRegistry _registry (line 20)

  GlanceComposer ActiveToolComposer { get; set; } (line 22)
    // When set (by a tool handler), overrides overlay-based composer selection
    // in BuildCellSpeech. Set to null to resume overlay-driven selection.

  TileCursor(Overlays.OverlayProfileRegistry registry) (line 24)

  static TileCursor Create(Overlays.OverlayProfileRegistry registry) (line 28)
    // Creates the singleton Instance and returns it.

  static void Destroy() (line 33)
    // Nulls the singleton and releases KInputManager.isMousePosLocked.

  CoordinateMode Mode { get; private set; } (line 38)
    // Initialized from ConfigManager.Config.CoordinateMode.

  int Cell { get; } (line 40)

  void Initialize() (line 46)
    // Sets cursor to the Printing Pod cell on the active world.
    // Falls back to world center if no telepad found.
    // Locks mouse and snaps camera.

  string Move(Direction direction) (line 60)
    // Moves one cell in direction. Returns speech string for new cell,
    // or null if the move was blocked by world bounds (also plays boundary sound).

  string ReadCoordinates() (line 76)
    // Returns world-relative coordinates (origin = bottom-left of active world)
    // formatted with STRINGS.ONIACCESS.TILE_CURSOR.COORDS.

  string CycleMode() (line 87)
    // Cycles Off -> Append -> Prepend -> Off.
    // Persists the new mode to ConfigManager. Returns spoken name of new mode.

  string SyncToCamera() (line 114)
    // Called every frame. Syncs _cell to camera center, re-locks mouse.
    // Returns tile speech string when a game-initiated pan finishes (wasPanning
    // transitions true->false), null otherwise.

  string JumpTo(int cell) (line 132)
    // Moves cursor directly to cell (no direction constraint).
    // Returns speech string, or null if cell is outside world bounds.

  private string BuildCellSpeech() (line 144)
    // Core speech assembly. Returns UNEXPLORED string for fog-of-war cells.
    // Selects composer: ActiveToolComposer > overlay registry lookup.
    // Falls back to element name + mass if composer returns null.
    // Prepends room name when Rooms overlay is active (only when room changes).
    // Attaches coordinates according to Mode.

  private int FindTelepadCell(WorldContainer world) (line 166)
    // Returns cell of first telepad on the world, or Grid.InvalidCell.
    // Exception-safe; logs warnings.

  private int FindWorldCenter(WorldContainer world) (line 177)
    // Returns cell at midpoint of world bounds. Falls back to cell 0 if invalid.

  internal static int GetNeighbor(int cell, Direction direction) (line 187)
    // Maps Direction enum to Grid.CellAbove/Below/Left/Right.

  private bool IsInWorldBounds(int cell) (line 197)
    // Returns true if cell is valid, belongs to activeWorld, and is within
    // minimumBounds..maximumBounds.

  private static void LockMouseToCell(int cell) (line 211)
    // Converts cell to screen position and sets KInputManager.isMousePosLocked.
    // Logs warning if Camera.main is null.

  private static void SnapCameraToCell(int cell) (line 222)
    // Calls CameraController.Instance.SnapTo() with cell world position.
    // Logs warning if CameraController.Instance is null.

  private string AttachCoordinates(string content) (line 231)
    // Appends or prepends ReadCoordinates() to content based on Mode.
    // Returns content unchanged in Off mode.

  void ResetRoomName() (line 242)
    // Clears _lastRoomName so the next Rooms-overlay cell read will always
    // prepend the room name regardless of whether it changed.

  private string PrependRoomName(string content) (line 246)
    // Gets room name from roomProber for current cell. Only prepends if the
    // room changed since last call (uses _lastRoomName as a one-entry cache).

  private static void PlayBoundarySound() (line 257)
    // Plays "Negative" UI sound. Logs error on exception.
