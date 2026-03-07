using System;
using System.Collections.Generic;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// Checks whether the currently selected dupe can reach the cursor tile.
	/// When unreachable, searches toward the dupe to find the nearest
	/// reachable cell and reports its offset from the cursor.
	/// </summary>
	public class PathabilityChecker {
		private const int MaxSearchRadius = 20;

		public string Check(DupeNavigator dupeNav) {
			var mi = dupeNav.GetCurrentDupe();
			if (mi == null) {
				BaseScreenHandler.PlaySound("Negative");
				return (string)STRINGS.ONIACCESS.DUPES.NO_DUPLICANTS;
			}

			try {
				int cursorCell = TileCursor.Instance.Cell;
				int dupeCell = Grid.PosToCell(mi);

				if (cursorCell == dupeCell)
					return (string)STRINGS.ONIACCESS.DUPES.PATHABILITY.HERE;

				var navigator = mi.GetComponent<Navigator>();
				int cost = navigator.GetNavigationCost(cursorCell);
				if (cost != -1)
					return string.Format(
						(string)STRINGS.ONIACCESS.DUPES.PATHABILITY.REACHABLE, cost);

				string nearest = FindNearestReachable(cursorCell, dupeCell, navigator);
				if (nearest != null)
					return string.Format(
						(string)STRINGS.ONIACCESS.DUPES.PATHABILITY.UNREACHABLE_NEAREST,
						nearest);

				return (string)STRINGS.ONIACCESS.DUPES.PATHABILITY.UNREACHABLE_NO_NEARBY;
			} catch (Exception ex) {
				Log.Error($"PathabilityChecker.Check: {ex}");
				return (string)STRINGS.ONIACCESS.DUPES.PATHABILITY.UNREACHABLE_NO_NEARBY;
			}
		}

		/// <summary>
		/// Expands outward from the cursor in concentric rings, searching
		/// only the half-plane toward the dupe. Finds the nearest reachable
		/// cell; ties broken by proximity to the dupe.
		/// </summary>
		private static string FindNearestReachable(
			int cursorCell, int dupeCell, Navigator navigator) {
			int cursorX = Grid.CellColumn(cursorCell);
			int cursorY = Grid.CellRow(cursorCell);
			int dupeX = Grid.CellColumn(dupeCell);
			int dupeY = Grid.CellRow(dupeCell);

			// Direction vector from cursor to dupe (unnormalized is fine
			// for dot product sign checks)
			int toDupeX = dupeX - cursorX;
			int toDupeY = dupeY - cursorY;

			int bestCell = Grid.InvalidCell;
			int bestDist = int.MaxValue;
			int bestDupeDist = int.MaxValue;

			for (int ring = 1; ring <= MaxSearchRadius; ring++) {
				if (bestCell != Grid.InvalidCell && ring > bestDist)
					break;

				for (int dx = -ring; dx <= ring; dx++) {
					for (int dy = -ring; dy <= ring; dy++) {
						if (Math.Abs(dx) != ring && Math.Abs(dy) != ring)
							continue;

						// Only search the dupe's half-plane
						if (dx * toDupeX + dy * toDupeY <= 0)
							continue;

						int x = cursorX + dx;
						int y = cursorY + dy;
						if (x < 0 || x >= Grid.WidthInCells
							|| y < 0 || y >= Grid.HeightInCells)
							continue;

						int cell = Grid.XYToCell(x, y);
						if (!TileCursor.IsInWorldBounds(cell)) continue;

						int dist = Math.Abs(dx) + Math.Abs(dy);
						if (dist > bestDist) continue;

						if (navigator.GetNavigationCost(cell) != -1) {
							int dupeDist = Math.Abs(x - dupeX) + Math.Abs(y - dupeY);
							if (dist < bestDist || dupeDist < bestDupeDist) {
								bestCell = cell;
								bestDist = dist;
								bestDupeDist = dupeDist;
							}
						}
					}
				}
			}

			if (bestCell == Grid.InvalidCell) return null;

			int fdx = Grid.CellColumn(bestCell) - cursorX;
			int fdy = Grid.CellRow(bestCell) - cursorY;
			return FormatOffset(fdx, fdy);
		}

		private static string FormatOffset(int dx, int dy) {
			var parts = new List<string>(2);
			if (dx != 0)
				parts.Add(Math.Abs(dx) + " " + (dx > 0
					? (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT
					: (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT));
			if (dy != 0)
				parts.Add(Math.Abs(dy) + " " + (dy > 0
					? (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_UP
					: (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN));
			return parts.Count > 0 ? string.Join(" ", parts) : null;
		}
	}
}
