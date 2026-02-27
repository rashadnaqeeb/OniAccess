using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner {
	internal static class GridUtil {
		internal static int CellDistance(int a, int b) {
			int dr = Grid.CellRow(a) - Grid.CellRow(b);
			int dc = Grid.CellColumn(a) - Grid.CellColumn(b);
			if (dr < 0) dr = -dr;
			if (dc < 0) dc = -dc;
			return dr + dc;
		}

		/// <summary>
		/// Shared validation for cluster-backed scan entries. Prunes stale
		/// cells, finds the closest surviving cell, and updates the entry.
		/// </summary>
		internal static bool ValidateCluster(
				List<int> cells, int cursorCell, ScanEntry entry,
				System.Func<int, bool> isStillPresent) {
			int bestCell = -1;
			int bestDist = int.MaxValue;

			for (int i = cells.Count - 1; i >= 0; i--) {
				int cell = cells[i];
				if (!isStillPresent(cell)) {
					cells.RemoveAt(i);
					continue;
				}
				int dist = CellDistance(cursorCell, cell);
				if (dist < bestDist) {
					bestDist = dist;
					bestCell = cell;
				}
			}

			if (bestCell < 0) return false;
			entry.Cell = bestCell;
			return true;
		}
	}
}
