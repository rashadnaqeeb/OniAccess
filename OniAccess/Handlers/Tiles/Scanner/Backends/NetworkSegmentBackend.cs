using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for wire/conduit segment clusters and bridge instances.
	/// Segments are clustered; bridges are individual instances.
	/// Both coexist within the same scanner subcategory.
	/// </summary>
	public class NetworkSegmentBackend : IScannerBackend {
		private List<NetworkSegmentCluster> _segments;
		private List<BridgeInstance> _bridges;

		public void SetGridData(
				List<NetworkSegmentCluster> segments,
				List<BridgeInstance> bridges) {
			_segments = segments;
			_bridges = bridges;
		}

		public IEnumerable<ScanEntry> Scan(int worldId) {
			if (_segments != null) {
				foreach (var cluster in _segments) {
					string segmentName = GetSegmentName(cluster);
					yield return new ScanEntry {
						Cell = cluster.Cells[0],
						Backend = this,
						BackendData = cluster,
						Category = cluster.ScannerCategory,
						Subcategory = cluster.ScannerSubcategory,
						ItemName = segmentName,
					};
				}
			}

			if (_bridges != null) {
				foreach (var bridge in _bridges) {
					string bridgeName = bridge.Go.GetComponent<KSelectable>()?.GetName()
						?? bridge.Go.name;
					yield return new ScanEntry {
						Cell = bridge.Cell,
						Backend = this,
						BackendData = bridge,
						Category = bridge.ScannerCategory,
						Subcategory = bridge.ScannerSubcategory,
						ItemName = bridgeName,
					};
				}
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			if (entry.BackendData is BridgeInstance bridge)
				return bridge.Go != null && !bridge.Go.IsNullOrDestroyed();

			var cluster = (NetworkSegmentCluster)entry.BackendData;
			int bestCell = -1;
			int bestDist = int.MaxValue;

			for (int i = cluster.Cells.Count - 1; i >= 0; i--) {
				int cell = cluster.Cells[i];
				if (!IsSegmentStillPresent(cell, cluster)) {
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
			if (entry.BackendData is NetworkSegmentCluster cluster) {
				if (cluster.Cells.Count == 1) return entry.ItemName;
				return string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL,
					cluster.Cells.Count, entry.ItemName);
			}
			return entry.ItemName;
		}

		private static string GetSegmentName(NetworkSegmentCluster cluster) {
			int layer = FindSegmentLayer(cluster);
			if (layer < 0) return cluster.PrefabId;

			foreach (int cell in cluster.Cells) {
				var go = Grid.Objects[cell, layer];
				if (go == null) continue;
				var selectable = go.GetComponent<KSelectable>();
				if (selectable != null) return selectable.GetName();
			}
			return cluster.PrefabId;
		}

		private static int FindSegmentLayer(NetworkSegmentCluster cluster) {
			for (int i = 0; i < Routing.NetworkLayerConfig.Types.Length; i++) {
				var net = Routing.NetworkLayerConfig.Types[i];
				if (net.ScannerCategory == cluster.ScannerCategory
					&& net.ScannerSubcategory == cluster.ScannerSubcategory)
					return (int)net.SegmentLayer;
			}
			return -1;
		}

		private static bool IsSegmentStillPresent(
				int cell, NetworkSegmentCluster cluster) {
			// Check all network segment layers for a building matching the prefab
			for (int i = 0; i < Routing.NetworkLayerConfig.Types.Length; i++) {
				var net = Routing.NetworkLayerConfig.Types[i];
				if (net.ScannerCategory != cluster.ScannerCategory
					|| net.ScannerSubcategory != cluster.ScannerSubcategory)
					continue;
				var go = Grid.Objects[cell, (int)net.SegmentLayer];
				if (go == null) continue;
				var building = go.GetComponent<Building>();
				if (building != null && building.Def.PrefabID == cluster.PrefabId)
					return true;
			}
			return false;
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
