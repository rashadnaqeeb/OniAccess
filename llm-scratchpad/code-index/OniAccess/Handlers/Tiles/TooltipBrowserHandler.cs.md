// Navigable browser for tooltip lines. Pushed onto the HandlerStack when
// Shift+I is pressed in TileCursorHandler. Extends BaseMenuHandler for 1D
// navigation with type-ahead search, Home/End, and wrap sounds. No KScreen.
// Escape or I closes the browser and returns to the tile cursor.

namespace OniAccess.Handlers.Tiles

class TooltipBrowserHandler : BaseMenuHandler (line 11)
  private readonly IReadOnlyList<string> _lines (line 12)

  override string DisplayName { get; } (line 14)
    // Returns STRINGS.ONIACCESS.HANDLERS.TOOLTIP_BROWSER

  override IReadOnlyList<HelpEntry> HelpEntries { get; } (line 16)
    // Initialized via BuildBrowserHelpEntries()

  TooltipBrowserHandler(IReadOnlyList<string> lines) (line 19)

  override int ItemCount { get; } (line 23)

  override string GetItemLabel(int index) (line 25)
    // Returns null for out-of-range indices; otherwise the raw line string.

  override void SpeakCurrentItem(string parentContext = null) (line 30)
    // Speaks the current line through TextFilter.FilterForSpeech().

  override void OnActivate() (line 36)
    // Plays HUD_Click_Open, resets index/search, speaks first line through
    // TextFilter. Does not announce handler name (unlike most handlers).

  override void OnDeactivate() (line 45)
    // Plays HUD_Click_Close then calls base.

  override bool Tick() (line 50)
    // Checks for I keydown to close (delegates rest to base.Tick()).

  override bool HandleKeyDown(KButtonEvent e) (line 58)
    // Delegates to base; additionally handles Escape to close.

  private void Close() (line 68)
    // Speaks TOOLTIP.CLOSED and pops the handler stack.

  private static void PlaySound(string name) (line 74)
    // Wraps KFMOD.PlayUISound; logs error on exception.

  private static IReadOnlyList<HelpEntry> BuildBrowserHelpEntries() (line 82)
    // Builds help list: A-Z (search), Up/Down (navigate), Home/End (jump),
    // Escape (close), I (close).
