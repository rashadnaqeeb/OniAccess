using System.Collections.Generic;
using System.Text.RegularExpressions;
using OniAccess.Util;

namespace OniAccess.Speech {
	/// <summary>
	/// Rich text filtering pipeline for speech output.
	/// Strips Unity Rich Text, TextMeshPro, and ONI-specific markup,
	/// converting meaningful tags to spoken text and silently removing decorative ones.
	///
	/// All text must pass through FilterForSpeech before reaching SpeechEngine.
	/// Filter order is critical -- sprite conversion must happen before tag stripping.
	/// </summary>
	public static class TextFilter {
		// Sprite tag: <sprite name=warning> or <sprite name="warning"/> or <sprite name="warning" />
		private static readonly Regex SpriteTagRegex =
			new Regex(@"<sprite\s+name=""?([^"">]+)""?\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// Link tag: <link="LINK_ID">display text</link>
		private static readonly Regex LinkTagRegex =
			new Regex(@"<link=""[^""]*"">(.*?)</link>", RegexOptions.Compiled);

		// Hotkey placeholder: strip from {Hotkey} onward
		private static readonly Regex HotkeyPlaceholderRegex =
			new Regex(@"\s*\{Hotkey\}.*", RegexOptions.Compiled);

		// Catch-all for remaining rich text tags (bold, color, size, style, etc.)
		private static readonly Regex RichTextTagsRegex =
			new Regex("<[^>]+>", RegexOptions.Compiled);

		// TextMeshPro shorthand sprite tags like [icon_name]
		private static readonly Regex TmpSpriteTagRegex =
			new Regex(@"\[[^\]]+\]\s*", RegexOptions.Compiled);

		// Normalize whitespace (collapse multiple spaces/newlines/tabs)
		private static readonly Regex WhitespaceRegex =
			new Regex(@"\s+", RegexOptions.Compiled);

		// Sprite name -> spoken text mapping
		private static readonly Dictionary<string, string> _spriteTextMap =
			new Dictionary<string, string>();

		// Sprites already warned about (suppress repeated log spam)
		private static readonly HashSet<string> _warnedSprites = new HashSet<string>();

		/// <summary>
		/// Register a sprite name to spoken text mapping.
		/// Meaningful sprites (e.g., "warning") are converted to words;
		/// unregistered sprites are silently stripped (but logged).
		/// </summary>
		/// <param name="spriteName">The sprite name as it appears in markup (case-insensitive)</param>
		/// <param name="spokenText">The text to speak instead of the sprite</param>
		public static void RegisterSprite(string spriteName, string spokenText) {
			if (!string.IsNullOrEmpty(spriteName))
				_spriteTextMap[spriteName.ToLowerInvariant()] = spokenText ?? "";
		}

		/// <summary>
		/// Initialize default ONI sprite mappings.
		/// Called during mod startup to register known meaningful sprites.
		/// </summary>
		public static void InitializeDefaults() {
			RegisterSprite("warning", "warning:");
			RegisterSprite("logic_signal_green", "green signal");
			RegisterSprite("logic_signal_red", "red signal");
		}

		/// <summary>
		/// Main filtering pipeline. Strips all rich text markup and converts
		/// meaningful sprites to spoken words.
		///
		/// Filter order (critical):
		/// 1. Convert known sprite tags to spoken text (before stripping tags)
		/// 2. Extract link display text (before stripping tags)
		/// 3. Strip hotkey placeholders
		/// 4. Strip all remaining rich text tags
		/// 5. Strip TMP bracket sprites
		/// 6. Clean up empty brackets/parens
		/// 7. Normalize whitespace
		/// 8. Trim
		/// </summary>
		public static string FilterForSpeech(string text) {
			if (string.IsNullOrEmpty(text)) return "";

			// Fast path: skip regex pipeline for plain text (no markup)
			if (text.IndexOf('<') < 0 && text.IndexOf('[') < 0 && text.IndexOf('{') < 0)
				return WhitespaceRegex.Replace(text, " ").Trim();

			// 1. Convert known sprite tags to spoken text, log unrecognized ones
			text = SpriteTagRegex.Replace(text, match => {
				string spriteName = match.Groups[1].Value.Trim().ToLowerInvariant();
				if (_spriteTextMap.TryGetValue(spriteName, out string spoken)) {
					// Append space after spoken text so it separates from following content;
					// whitespace normalization in step 7 will collapse any extra spaces
					return string.IsNullOrEmpty(spoken) ? "" : spoken + " ";
				}
				if (_warnedSprites.Add(spriteName))
					Log.Debug($"Unrecognized sprite tag: {spriteName}");
				return "";
			});

			// 2. Extract link display text (keep inner text, remove link wrapper)
			text = LinkTagRegex.Replace(text, "$1");

			// 3. Strip hotkey placeholders (from {Hotkey} onward)
			text = HotkeyPlaceholderRegex.Replace(text, "");

			// 4. Strip all remaining rich text tags
			text = RichTextTagsRegex.Replace(text, "");

			// 5. Strip TMP bracket sprites
			text = TmpSpriteTagRegex.Replace(text, "");

			// 6. Clean up empty brackets/parens left behind
			text = text.Replace("[]", "");
			text = text.Replace("()", "");

			// 7. Normalize whitespace
			text = WhitespaceRegex.Replace(text, " ");

			// 8. Trim
			return text.Trim();
		}
	}
}
