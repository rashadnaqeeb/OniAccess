using System;
using System.Collections.Generic;

namespace OniAccess.Handlers {
	/// <summary>
	/// Two-press rectangle accumulation state machine. Tracks pending
	/// first corners and completed rectangles. Game-independent: the
	/// handler validates cells (visibility, big cursor, line mode)
	/// before calling into this class.
	/// </summary>
	public class RectangleSelection {
		public struct RectCorners {
			public int Cell1;
			public int Cell2;

			public void GetBounds(out int minX, out int maxX, out int minY, out int maxY) {
				minX = Math.Min(Grid.CellColumn(Cell1), Grid.CellColumn(Cell2));
				maxX = Math.Max(Grid.CellColumn(Cell1), Grid.CellColumn(Cell2));
				minY = Math.Min(Grid.CellRow(Cell1), Grid.CellRow(Cell2));
				maxY = Math.Max(Grid.CellRow(Cell1), Grid.CellRow(Cell2));
			}

			public bool Contains(int cell) {
				GetBounds(out int minX, out int maxX, out int minY, out int maxY);
				int cx = Grid.CellColumn(cell);
				int cy = Grid.CellRow(cell);
				return cx >= minX && cx <= maxX && cy >= minY && cy <= maxY;
			}
		}

		public enum SetCornerResult { FirstCornerSet, RectangleComplete }

		private int _pendingFirstCorner = Grid.InvalidCell;
		private int _lastDragCell = Grid.InvalidCell;
		private readonly List<RectCorners> _rectangles = new List<RectCorners>();

		public bool HasSelection => _rectangles.Count > 0 || _pendingFirstCorner != Grid.InvalidCell;
		public int PendingFirstCorner => _pendingFirstCorner;
		public int LastDragCell {
			get => _lastDragCell;
			set => _lastDragCell = value;
		}
		public int RectangleCount => _rectangles.Count;

		/// <summary>
		/// Records a corner. First call sets the pending first corner;
		/// second call completes a rectangle and adds it to the list.
		/// Returns the result and (for RectangleComplete) the new rect.
		/// </summary>
		public SetCornerResult SetCorner(int cell, out RectCorners rect) {
			rect = default;
			if (_pendingFirstCorner == Grid.InvalidCell) {
				_pendingFirstCorner = cell;
				_lastDragCell = cell;
				return SetCornerResult.FirstCornerSet;
			}

			rect = new RectCorners { Cell1 = _pendingFirstCorner, Cell2 = cell };
			_rectangles.Add(rect);
			_pendingFirstCorner = Grid.InvalidCell;
			return SetCornerResult.RectangleComplete;
		}

		/// <summary>
		/// Adds a rectangle directly (for big cursor or programmatic use).
		/// </summary>
		public void AddRectangle(int cell1, int cell2) {
			_rectangles.Add(new RectCorners { Cell1 = cell1, Cell2 = cell2 });
			_pendingFirstCorner = Grid.InvalidCell;
		}

		/// <summary>
		/// Creates a single-cell rectangle (for Enter with no selection).
		/// </summary>
		public void AutoSelectSingle(int cell) {
			_rectangles.Add(new RectCorners { Cell1 = cell, Cell2 = cell });
		}

		public bool IsCellSelected(int cell) {
			for (int i = 0; i < _rectangles.Count; i++) {
				if (_rectangles[i].Contains(cell))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the last rectangle containing the given cell.
		/// Returns true if a rectangle was removed.
		/// </summary>
		public bool ClearRectAtCursor(int cell) {
			for (int i = _rectangles.Count - 1; i >= 0; i--) {
				if (_rectangles[i].Contains(cell)) {
					_rectangles.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public void ClearAll() {
			_rectangles.Clear();
			_pendingFirstCorner = Grid.InvalidCell;
			_lastDragCell = Grid.InvalidCell;
		}

		public IReadOnlyList<RectCorners> GetRectangles() => _rectangles;

		// ========================================
		// STATIC HELPERS
		// ========================================

		public static int ComputeArea(int cell1, int cell2) {
			int width = Math.Abs(Grid.CellColumn(cell2) - Grid.CellColumn(cell1)) + 1;
			int height = Math.Abs(Grid.CellRow(cell2) - Grid.CellRow(cell1)) + 1;
			return width * height;
		}

		public static int TileCountBetween(int cell1, int cell2) {
			int width = Math.Abs(Grid.CellColumn(cell2) - Grid.CellColumn(cell1)) + 1;
			int height = Math.Abs(Grid.CellRow(cell2) - Grid.CellRow(cell1)) + 1;
			return width + height - 1;
		}

		public static string BuildRectSummary(int cell1, int cell2, Func<int, int> countTargets) {
			int minX = Math.Min(Grid.CellColumn(cell1), Grid.CellColumn(cell2));
			int maxX = Math.Max(Grid.CellColumn(cell1), Grid.CellColumn(cell2));
			int minY = Math.Min(Grid.CellRow(cell1), Grid.CellRow(cell2));
			int maxY = Math.Max(Grid.CellRow(cell1), Grid.CellRow(cell2));
			int width = maxX - minX + 1;
			int height = maxY - minY + 1;
			int valid = 0;
			int invalid = 0;

			for (int y = minY; y <= maxY; y++) {
				for (int x = minX; x <= maxX; x++) {
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidCell(cell) || !Grid.IsVisible(cell)) {
						invalid++;
						continue;
					}
					if (countTargets != null) {
						int count = countTargets(cell);
						if (count > 0)
							valid += count;
						else
							invalid++;
					} else {
						valid++;
					}
				}
			}

			string format = invalid > 0
				? (string)STRINGS.ONIACCESS.TOOLS.RECT_SUMMARY_INVALID
				: (string)STRINGS.ONIACCESS.TOOLS.RECT_SUMMARY;
			return string.Format(format, width, height, valid, invalid);
		}

		public static void PlayDragSound(string soundName, int tileCount) {
			try {
				var pos = Grid.CellToPosCCC(
					Tiles.TileCursor.Instance.Cell, Grid.SceneLayer.Move);
				var ev = KFMOD.BeginOneShot(GlobalAssets.GetSound(soundName), pos);
				ev.setParameterByName("tileCount", tileCount);
				KFMOD.EndOneShot(ev);
			} catch (Exception ex) {
				Util.Log.Warn($"RectangleSelection.PlayDragSound: {ex}");
			}
		}
	}
}
