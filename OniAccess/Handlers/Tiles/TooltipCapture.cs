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

		/// <summary>
		/// Return the most relevant tooltip block for a quick summary.
		/// Priority: overlay-specific block, then building, then first block.
		/// Returns null when no tooltip is captured.
		/// </summary>
		internal static string GetPrioritySummary(int cell) {
			var lines = _capturedLines;
			if (lines == null || lines.Count == 0) return null;
			if (lines.Count == 1) return lines[0];

			var buildingNames = GetNonBackwallBuildingNames(cell);
			var backwallName = GetBackwallName(cell);
			if (backwallName != null && buildingNames.Count > 0)
				buildingNames.Add(backwallName);

			// Overlay blocks are drawn first. If the active overlay produced
			// one, lines[0] is the overlay block. Guard against false
			// positives by checking lines[0] doesn't start with a building
			// name (overlay titles like "Decor" never match building names).
			if (HasOverlayTooltipBlock(cell)
				&& !MatchesAnyName(lines[0], buildingNames))
				return lines[0];

			if (buildingNames.Count > 0) {
				for (int i = 0; i < lines.Count; i++) {
					if (MatchesAnyName(lines[i], buildingNames))
						return lines[i];
				}
			}

			// When only a backwall building exists, skip past its tooltip line
			if (backwallName != null) {
				for (int i = 0; i < lines.Count; i++) {
					if (!lines[i].StartsWith(backwallName,
							System.StringComparison.OrdinalIgnoreCase))
						return lines[i];
				}
			}

			return lines[0];
		}

		/// <summary>
		/// Whether the active overlay draws its own tooltip block before
		/// the entity loop in SelectToolHoverTextCard.UpdateHoverElements.
		/// </summary>
		private static bool HasOverlayTooltipBlock(int cell) {
			if (OverlayScreen.Instance == null) return false;
			var mode = OverlayScreen.Instance.GetMode();

			if (mode == OverlayModes.Decor.ID) return true;
			if (mode == OverlayModes.Light.ID) return true;
			if (mode == OverlayModes.Radiation.ID) return true;
			if (mode == OverlayModes.Logic.ID) return true;
			if (mode == OverlayModes.Rooms.ID)
				return Game.Instance != null
					&& Game.Instance.roomProber != null
					&& Game.Instance.roomProber.GetCavityForCell(cell) != null;
			if (mode == OverlayModes.Temperature.ID
				&& Game.Instance != null
				&& Game.Instance.temperatureOverlayMode
					== Game.TemperatureOverlayModes.HeatFlow
				&& !Grid.Solid[cell])
				return true;
			return false;
		}

		private static List<string> GetNonBackwallBuildingNames(int cell) {
			var names = new List<string>(2);
			AddBuildingName(cell, (int)ObjectLayer.Building, names);
			AddBuildingName(cell, (int)ObjectLayer.FoundationTile, names);
			return names;
		}

		private static string GetBackwallName(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Backwall];
			if (go == null) return null;
			string name = go.GetProperName();
			return string.IsNullOrEmpty(name) ? null : name;
		}

		private static void AddBuildingName(
				int cell, int layer, List<string> names) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			string name = go.GetProperName();
			if (!string.IsNullOrEmpty(name))
				names.Add(name);
		}

		private static bool MatchesAnyName(string line, List<string> names) {
			for (int i = 0; i < names.Count; i++) {
				if (line.StartsWith(names[i],
						System.StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

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
