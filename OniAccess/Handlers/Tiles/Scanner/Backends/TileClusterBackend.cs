using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for constructed tiles (Solids > Tiles, plus routed exceptions
	/// like FarmTile → Buildings > Farming). Receives pre-clustered data
	/// from GridScanner.
	/// </summary>
	public class TileClusterBackend: IScannerBackend {
		private List<TileCluster> _clusters;

		public void SetGridData(List<TileCluster> clusters) {
			_clusters = clusters;
		}

		public IEnumerable<ScanEntry> Scan(int worldId) {
			if (_clusters == null) yield break;
			foreach (var cluster in _clusters) {
				string tileName = GetTileName(cluster);
				yield return new ScanEntry {
					Cell = cluster.Cells[0],
					Backend = this,
					BackendData = cluster,
					Category = cluster.Category,
					Subcategory = cluster.Subcategory,
					ItemName = tileName,
				};
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			var cluster = (TileCluster)entry.BackendData;
			return GridUtil.ValidateCluster(cluster.Cells, cursorCell, entry,
				cell => IsTileStillPresent(cell, cluster.PrefabId));
		}

		public string FormatName(ScanEntry entry) {
			var cluster = (TileCluster)entry.BackendData;
			if (cluster.Cells.Count == 1) return entry.ItemName;
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL,
				cluster.Cells.Count, entry.ItemName);
		}

		private static string GetTileName(TileCluster cluster) {
			foreach (int cell in cluster.Cells) {
				var go = Grid.Objects[cell, (int)ObjectLayer.FoundationTile]
					?? Grid.Objects[cell, (int)ObjectLayer.LadderTile];
				if (go == null) continue;
				var selectable = go.GetComponent<KSelectable>();
				if (selectable != null) return selectable.GetName();
			}
			return cluster.PrefabId;
		}

		private static bool IsTileStillPresent(int cell, string expectedPrefabId) {
			var go = Grid.Objects[cell, (int)ObjectLayer.FoundationTile]
				?? Grid.Objects[cell, (int)ObjectLayer.LadderTile];
			if (go == null) return false;
			var building = go.GetComponent<Building>();
			return building != null && building.Def.PrefabID == expectedPrefabId;
		}

	}
}
