// Maps exact Color values to localized color name strings for the PixelPackSideScreen palette.
// Names come from STRINGS.ONIACCESS.COLORS. The dictionary is initialized lazily on first use.
// Covers four rows of palette colors (~50 entries): dark tones, vivid tones, pastels, and grays.

namespace OniAccess.Widgets

static class ColorNameUtil (line 6)
  private static Dictionary<Color, Func<string>> colorNames (line 7)
    // Lazily initialized map from exact Color value to a lambda returning a localized name string.

  static string GetColorName(Color color) (line 9)
    // Initializes colorNames if null, then does exact Color key lookup.
    // Returns localized name string or null if the color is not in the table.
  private static void Initialize() (line 17)
    // Populates colorNames with ~50 Color-to-name-lambda entries arranged in four palette rows.
    // Note: the duplicate (0,0,0) black entry in Row 2 is skipped (comment at line 48).
