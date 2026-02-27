// Captures the content drawn by HoverTextDrawer during each hover tooltip
// render pass. Harmony patches call BeginFrame/BeginBlock/AppendText/
// AppendIcon/AppendNewLine/EndFrame. TileCursorHandler reads
// GetTooltipText() on Q press.
//
// Text segments within a visual line are concatenated directly (no separator).
// NewLine boundaries create separate lines, joined with ", " within a block.
// Blocks (shadow bars) are joined with ", ".
// Sprites are emitted as synthetic sprite tags that TextFilter resolves
// during SpeechPipeline.SpeakInterrupt().
//
// _capturedText holds the assembled string from the most recent completed
// draw pass. This is a deliberate exception to the "never cache game state"
// rule: HoverTextDrawer has no queryable API, so capture-and-store is the
// only way to read tooltip content. Staleness is at most one frame behind
// the cursor position (drawing runs in LateUpdate, Q is read in Update).

namespace OniAccess.Handlers.Tiles

internal static class TooltipCapture (line 23)
  private static readonly List<List<string>> _blocks (line 24)
  private static List<string> _currentBlock (line 25)
  private static string _currentLine (line 26)
  private static string _capturedText (line 27)
  private static IReadOnlyList<string> _capturedLines (line 28)
  private static bool _capturing (line 29)

  internal static void BeginFrame() (line 31)
    // Called by Harmony patch at start of each HoverTextDrawer draw pass.
    // Clears all state and sets _capturing = true.

  internal static void BeginBlock() (line 40)
    // Called when HoverTextDrawer starts a new shadow-bar block.
    // Flushes current line, creates a new block list, and appends it to _blocks.

  internal static void AppendText(string text) (line 46)
    // Appends text to _currentLine. No-ops if not capturing, no current block,
    // or text is null/whitespace.

  internal static void AppendIcon(Sprite icon) (line 52)
    // Appends a synthetic "<sprite name="...">" tag to _currentLine so that
    // TextFilter can later resolve sprite names to spoken words.

  internal static void AppendNewLine() (line 57)
    // Flushes _currentLine into _currentBlock (line boundary).

  internal static void EndFrame() (line 62)
    // Flushes final line, sets _capturing = false.
    // Assembles _capturedText (blocks joined by ", ") and _capturedLines
    // (list of per-block strings). Both are null if no blocks were captured.

  internal static string GetTooltipText() (line 80)
    // Returns the full assembled tooltip as a single string, or null.

  internal static IReadOnlyList<string> GetTooltipLines() (line 82)
    // Returns per-block lines list, or null if nothing captured.

  internal static string GetPrioritySummary(int cell) (line 90)
    // Returns the most relevant tooltip block for a quick I-key summary.
    // Priority: overlay-specific block (lines[0] when overlay produces one
    // and it doesn't look like a building name), then first block matching
    // a non-backwall building name, then first non-backwall line, then lines[0].
    // Returns null when _capturedLines is empty.

  private static bool HasOverlayTooltipBlock(int cell) (line 131)
    // Returns true if the active overlay draws its own block before the entity
    // loop in SelectToolHoverTextCard. Checks: Decor, Light, Radiation, Logic,
    // Rooms (with non-null cavity), Temperature HeatFlow on non-solid tiles.

  private static List<string> GetNonBackwallBuildingNames(int cell) (line 152)
    // Returns display names for Building and FoundationTile layer objects at cell.

  private static string GetBackwallName(int cell) (line 159)
    // Returns display name of the Backwall layer object, or null.

  private static void AddBuildingName(int cell, int layer, List<string> names) (line 166)
    // Appends the proper name of Grid.Objects[cell, layer] to names if non-null.

  private static bool MatchesAnyName(string line, List<string> names) (line 175)
    // Returns true if line starts with any name in names (OrdinalIgnoreCase).

  internal static void Reset() (line 184)
    // Full reset to initial state (used in tests and teardown).

  private static void FlushLine() (line 193)
    // If _currentBlock is non-null and _currentLine is non-whitespace,
    // trims and adds to _currentBlock. Always resets _currentLine to "".
