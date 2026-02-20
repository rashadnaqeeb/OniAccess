using System.Collections.Generic;
using ProcGen;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for Zones > Biomes. Receives pre-clustered biome zones
	/// from GridScanner. Each contiguous same-ZoneType region is one instance.
	/// </summary>
	public class BiomeBackend : IScannerBackend {
		private List<BiomeCluster> _clusters;

		public void SetGridData(List<BiomeCluster> clusters) {
			_clusters = clusters;
		}

		public IEnumerable<ScanEntry> Scan(int worldId) {
			if (_clusters == null) yield break;
			foreach (var cluster in _clusters) {
				yield return new ScanEntry {
					Cell = cluster.Cells[0],
					Backend = this,
					BackendData = cluster,
					Category = ScannerTaxonomy.Categories.Zones,
					Subcategory = ScannerTaxonomy.Subcategories.Biomes,
					ItemName = cluster.DisplayName,
				};
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			var cluster = (BiomeCluster)entry.BackendData;
			int bestCell = -1;
			int bestDist = int.MaxValue;

			for (int i = cluster.Cells.Count - 1; i >= 0; i--) {
				int cell = cluster.Cells[i];
				var zone = World.Instance.zoneRenderData.GetSubWorldZoneType(cell);
				if (zone != cluster.ZoneType) {
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
			var cluster = (BiomeCluster)entry.BackendData;
			if (cluster.Cells.Count == 1) return cluster.DisplayName;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL,
				cluster.Cells.Count, cluster.DisplayName);
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
