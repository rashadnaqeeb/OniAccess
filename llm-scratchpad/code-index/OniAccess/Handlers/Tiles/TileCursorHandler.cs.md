// BaseScreenHandler bound to the Hud KScreen. Active when the game world
// is loaded and no modal menu is on top.
//
// Routes arrow keys to TileCursor movement, K to coordinate
// reading, Shift+K to coordinate mode cycling.
//
// CapturesAllInput = false: game hotkeys (overlays, tools, WASD camera,
// pause) pass through.

namespace OniAccess.Handlers.Tiles

class TileCursorHandler : BaseScreenHandler (line 17)
  private Overlays.OverlayProfileRegistry _overlayRegistry (line 18)
  private ScannerNavigator _scanner (line 19)
  private GameStateMonitor _monitor (line 20)
  private bool _hasActivated (line 21)
  private bool _overlaySubscribed (line 22)
  private int _queueNextOverlayTtl (line 23)
    // Countdown (set to 2) used to queue the next overlay announcement instead
    // of interrupting. Decremented each Tick(). Set by QueueNextOverlayAnnouncement().
  private HashedString _lastOverlayMode (line 24)

  void QueueNextOverlayAnnouncement() (line 26)
    // Sets _queueNextOverlayTtl = 2. Callers use this to request that the next
    // OnOverlayChanged fires as SpeakQueued rather than SpeakInterrupt.

  private static readonly ConsumedKey[] _consumedKeys (line 28)
    // Consumed keys: Tab, BackQuote, I, Shift+I, K, Shift+K, arrow keys,
    // End, Home, Ctrl/Shift/Alt/plain PageUp/Down, Q, Return

  override IReadOnlyList<ConsumedKey> ConsumedKeys { get; } (line 53)

  private static readonly IReadOnlyList<HelpEntry> _helpEntries (line 55)
    // Help entries covering: arrow keys, Tab, Enter, I, Shift+I, K, Shift+K,
    // End, Home, Ctrl/Shift/plain/Alt PageUp/Down, Q, backtick

  override string DisplayName { get; } (line 73)
    // Returns STRINGS.ONIACCESS.HANDLERS.COLONY_VIEW

  override bool CapturesAllInput { get; } (line 74)
    // false — game hotkeys pass through

  override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 75)

  TileCursorHandler(KScreen screen) (line 77)

  override void OnActivate() (line 80)
    // One-time init guard (_hasActivated): builds overlay registry, tool profiles,
    // creates TileCursor, ScannerNavigator, GameStateMonitor, speaks DisplayName,
    // and initializes the cursor position.
    // Every activation (including re-activations after deactivate): unsubscribes
    // then re-subscribes OnOverlayChanged to avoid double-subscription; subscribes
    // OnActiveToolChanged via Game event hash 1174281782.

  override void OnDeactivate() (line 110)
    // Unsubscribes OnActiveToolChanged, destroys TileCursor (releases mouse lock),
    // nulls scanner, unsubscribes OnOverlayChanged.

  private void OnOverlayChanged(HashedString newMode) (line 120)
    // Resets room name cache. Skips if mode is unchanged (guards against game
    // resetting overlay to None before opening a screen). Speaks overlay name
    // queued if _queueNextOverlayTtl > 0, otherwise interrupted.

  override bool Tick() (line 134)
    // Decrements _queueNextOverlayTtl. Late-subscribes OnOverlayChanged if
    // OverlayScreen appeared after OnActivate. Ticks scanner world-switch check
    // and game state monitor. Syncs cursor to camera and speaks if pan finished.
    // Handles: Return (entity picker), Tab (action menu), BackQuote (cycle speed),
    // arrow keys (move cursor), K/Shift+K (coords/mode), I/Shift+I (tooltip),
    // Q (cycle status), End/Home/PageUp/PageDown with modifiers (scanner).

  private void SpeakMove(Direction direction) (line 250)
    // Calls TileCursor.Instance.Move() and speaks the result if non-null.

  private void OpenActionMenu() (line 256)
    // Activates SelectTool if needed, pushes Build.ActionMenuHandler.

  private void OpenEntityPicker() (line 262)
    // Guards fog-of-war. Collects selectables at cursor cell.
    // If 0: speaks NOTHING_TO_SELECT.
    // If 1: selects directly without picker.
    // If 2+: pushes EntityPickerHandler.

  private void OnActiveToolChanged(object data) (line 284)
    // Event handler for game's active tool change (hash 1174281782).
    // Skips SelectTool, build tools, and any handler already managing
    // tools (ToolHandler, ToolFilterHandler, BuildToolHandler, ActionMenuHandler).
    // Otherwise pushes a new ToolHandler.

  private void ReadTooltipSummary() (line 295)
    // Guards fog-of-war. Gets priority summary from TooltipCapture and speaks it,
    // or speaks NO_TOOLTIP if nothing captured.

  private void OpenTooltipBrowser() (line 311)
    // Guards fog-of-war. Gets tooltip lines from TooltipCapture and pushes
    // TooltipBrowserHandler, or speaks NO_TOOLTIP if nothing captured.

  private static void PlaySpeedChangeSound(float speed) (line 326)
    // Plays "Speed_Change" sound with Speed parameter set to the given value.
