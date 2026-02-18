using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Captures the content drawn by HoverTextDrawer during each hover tooltip
	/// render pass. Harmony patches call BeginFrame/BeginBlock/AppendText/
	/// AppendIcon/AppendNewLine/EndFrame. TileCursorHandler reads
	/// GetTooltipText() on Q press.
	///
	/// Text segments within a visual line are concatenated directly (no separator).
	/// NewLine boundaries create separate lines, joined with ", " within a block.
	/// Blocks (shadow bars) are joined with ", ".
	/// Sprites are emitted as synthetic sprite tags that TextFilter resolves
	/// during SpeechPipeline.SpeakInterrupt().
	///
	/// _capturedText holds the assembled string from the most recent completed
	/// draw pass. This is a deliberate exception to the "never cache game state"
	/// rule: HoverTextDrawer has no queryable API, so capture-and-store is the
	/// only way to read tooltip content. Staleness is at most one frame behind
	/// the cursor position (drawing runs in LateUpdate, Q is read in Update).
	/// </summary>
	internal static class TooltipCapture {
		private static readonly List<List<string>> _blocks = new List<List<string>>();
		private static List<string> _currentBlock;
		private static string _currentLine = "";
		private static string _capturedText;
		private static System.Collections.Generic.IReadOnlyList<string> _capturedLines;
		private static bool _capturing;

		internal static void BeginFrame() {
			_blocks.Clear();
			_currentBlock = null;
			_currentLine = "";
			_capturedText = null;
			_capturedLines = null;
			_capturing = true;
		}

		internal static void BeginBlock() {
			FlushLine();
			_currentBlock = new List<string>();
			_blocks.Add(_currentBlock);
		}

		internal static void AppendText(string text) {
			if (!_capturing || _currentBlock == null || string.IsNullOrWhiteSpace(text))
				return;
			_currentLine += text;
		}

		internal static void AppendIcon(Sprite icon) {
			if (!_capturing || _currentBlock == null || icon == null) return;
			_currentLine += $"<sprite name=\"{icon.name}\">";
		}

		internal static void AppendNewLine() {
			if (!_capturing || _currentBlock == null) return;
			FlushLine();
		}

		internal static void EndFrame() {
			FlushLine();
			_capturing = false;
			if (_blocks.Count == 0) {
				_capturedText = null;
				_capturedLines = null;
				return;
			}
			var blockTexts = new List<string>(_blocks.Count);
			foreach (var block in _blocks) {
				string blockText = string.Join(", ", block);
				if (!string.IsNullOrEmpty(blockText))
					blockTexts.Add(blockText);
			}
			_capturedText = blockTexts.Count > 0 ? string.Join(", ", blockTexts) : null;
			_capturedLines = blockTexts.Count > 0 ? blockTexts.AsReadOnly() : null;
		}

		internal static string GetTooltipText() => _capturedText;

		internal static System.Collections.Generic.IReadOnlyList<string> GetTooltipLines()
			=> _capturedLines;

		internal static void Reset() {
			_blocks.Clear();
			_currentBlock = null;
			_currentLine = "";
			_capturedText = null;
			_capturedLines = null;
			_capturing = false;
		}

		private static void FlushLine() {
			if (_currentBlock != null && !string.IsNullOrWhiteSpace(_currentLine))
				_currentBlock.Add(_currentLine.Trim());
			_currentLine = "";
		}
	}
}
