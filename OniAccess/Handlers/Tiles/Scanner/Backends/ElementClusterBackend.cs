using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for natural elements (Solids, Liquids, Gases).
	/// Receives pre-clustered data from GridScanner.
	/// </summary>
	public class ElementClusterBackend: IScannerBackend {
		private List<ElementCluster> _clusters;

		public void SetGridData(List<ElementCluster> clusters) {
			_clusters = clusters;
		}

		public IEnumerable<ScanEntry> Scan(int worldId) {
			if (_clusters == null) yield break;
			foreach (var cluster in _clusters) {
				yield return new ScanEntry {
					Cell = cluster.Cells[0],
					Backend = this,
					BackendData = cluster,
					Category = cluster.Category,
					Subcategory = cluster.Subcategory,
					ItemName = cluster.ElementName,
				};
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			var cluster = (ElementCluster)entry.BackendData;
			int bestCell = -1;
			int bestDist = int.MaxValue;

			for (int i = cluster.Cells.Count - 1; i >= 0; i--) {
				int cell = cluster.Cells[i];
				if (!Grid.IsValidCell(cell)
					|| Grid.Element[cell].id != cluster.ElementId) {
					cluster.Cells.RemoveAt(i);
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

		public string FormatName(ScanEntry entry) {
			var cluster = (ElementCluster)entry.BackendData;
			if (cluster.Cells.Count == 1) return cluster.ElementName;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL,
				cluster.Cells.Count, cluster.ElementName);
		}

		private static int CellDistance(int a, int b) {
			int dr = Grid.CellRow(a) - Grid.CellRow(b);
			int dc = Grid.CellColumn(a) - Grid.CellColumn(b);
			if (dr < 0) dr = -dr;
			if (dc < 0) dc = -dc;
			return dr + dc;
		}
	}
}
