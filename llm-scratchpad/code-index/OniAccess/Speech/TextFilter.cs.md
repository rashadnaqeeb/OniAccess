// Rich text filtering pipeline. Strips Unity RichText, TextMeshPro, and ONI-specific markup
// before text reaches SpeechEngine. Converts meaningful sprite tags to spoken words.
// All text must pass through FilterForSpeech before reaching SpeechEngine.
// Filter order is critical -- sprite conversion must happen before tag stripping.
static class TextFilter (line 14)
  // Compiled regexes
  private static readonly Regex SpriteTagRegex (line 16)         // <sprite name="..."> or <sprite name=.../>
  private static readonly Regex LinkTagRegex (line 21)           // <link="...">display</link>
  private static readonly Regex HotkeyPlaceholderRegex (line 25) // strips {Hotkey} and everything after
  private static readonly Regex RichTextTagsRegex (line 28)      // catch-all for bold, color, size, etc.
  private static readonly Regex NumericBracketRegex (line 31)    // [45%] -> 45%
  private static readonly Regex TmpSpriteTagRegex (line 36)      // [icon_name] TMP shorthand sprites
  private static readonly Regex WhitespaceRegex (line 40)        // normalizes runs of whitespace

  private static readonly Dictionary<string, string> _spriteTextMap (line 44)  // sprite name -> spoken text
  private static readonly HashSet<string> _warnedSprites (line 48)             // suppress repeat log spam

  // Registers a sprite name -> spoken text mapping. Names are case-insensitive.
  static void RegisterSprite(string spriteName, string spokenText) (line 57)

  // Registers default ONI sprite mappings: "warning", "logic_signal_green", "logic_signal_red".
  // Called from Mod.OnLoad.
  static void InitializeDefaults() (line 66)

  // Main pipeline. Applies all 9 filter steps in order:
  // 1. Strip control chars (null bytes from TMP truncate speech output)
  // 2. Replace masculine ordinal (U+00BA) with degree sign (U+00B0) -- screen readers mispronounce it
  // 3. Fast path: skip regex pipeline for plain text (no <, [, or {)
  // 4. Convert known sprite tags to spoken text; log (once) unrecognized ones
  // 5. Extract link display text
  // 6. Strip {Hotkey} placeholders
  // 7. Strip all remaining rich text tags
  // 8. Extract numeric bracket content [45%] -> 45%
  // 9. Strip TMP bracket sprites
  // 10. Clean up empty [] and ()
  // 11. Normalize whitespace, trim
  static string FilterForSpeech(string text) (line 87)

  // Strips ASCII control characters (< 0x20) except \n, \r, \t.
  // Optimized: scans for first bad char before allocating a StringBuilder.
  private static string StripControlChars(string text) (line 142)
