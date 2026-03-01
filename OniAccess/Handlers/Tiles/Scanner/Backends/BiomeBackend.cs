using System.Collections.Generic;
using ProcGen;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for Zones > Biomes. Receives pre-clustered biome zones
	/// from GridScanner. Each contiguous same-ZoneType region is one instance.
	/// </summary>
	public class BiomeBackend: IScannerBackend {
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
			return GridUtil.ValidateCluster(cluster.Cells, cursorCell, entry,
				cell => World.Instance.zoneRenderData.GetSubWorldZoneType(cell)
					== cluster.ZoneType);
		}

		public string FormatName(ScanEntry entry) {
			var cluster = (BiomeCluster)entry.BackendData;
			string name = cluster.DisplayName
				+ (string)STRINGS.ONIACCESS.SCANNER.BIOME_SUFFIX;
			if (cluster.Cells.Count == 1) return name;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL,
				cluster.Cells.Count, name);
		}

	}
}
