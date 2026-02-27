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
			return GridUtil.ValidateCluster(cluster.Cells, cursorCell, entry,
				cell => Grid.IsValidCell(cell)
					&& Grid.Element[cell].id == cluster.ElementId);
		}

		public string FormatName(ScanEntry entry) {
			var cluster = (ElementCluster)entry.BackendData;
			if (cluster.Cells.Count == 1) return cluster.ElementName;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL,
				cluster.Cells.Count, cluster.ElementName);
		}

	}
}
