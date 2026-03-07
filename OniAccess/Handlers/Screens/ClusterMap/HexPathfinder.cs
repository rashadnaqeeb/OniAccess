using System.Collections.Generic;

namespace OniAccess.Handlers.Screens.ClusterMap {
	/// <summary>
	/// Lightweight BFS pathfinder for the cluster hex grid.
	/// Two passes: visible-only (no scanner module) and through-fog.
	/// </summary>
	public static class HexPathfinder {
		public struct PathResult {
			public int VisiblePathLength;
			public int FogPathLength;
			public int FogCellCount;
			public bool HasVisiblePath;
			public bool HasFogPath;
		}

		/// <summary>
		/// Run BFS from start to end. Returns path lengths for both
		/// visible-only and through-fog passes.
		/// </summary>
		public static PathResult FindPath(AxialI start, AxialI end) {
			var result = new PathResult();
			result.VisiblePathLength = BFS(start, end, allowFog: false);
			result.HasVisiblePath = result.VisiblePathLength >= 0;

			int fogPath = BFS(start, end, allowFog: true);
			result.HasFogPath = fogPath >= 0;
			if (result.HasFogPath) {
				result.FogPathLength = fogPath;
				result.FogCellCount = CountFogCells(start, end);
			}

			return result;
		}

		/// <summary>
		/// Format the path result as a spoken announcement per the plan.
		/// </summary>
		public static string FormatResult(PathResult result) {
			if (!result.HasVisiblePath && !result.HasFogPath)
				return (string)STRINGS.ONIACCESS.CLUSTER_MAP.NO_PATH;

			if (result.HasVisiblePath && result.HasFogPath) {
				if (result.FogPathLength < result.VisiblePathLength) {
					return string.Format(
						(string)STRINGS.ONIACCESS.CLUSTER_MAP.PATH_FOG_WITH_ALT,
						result.FogPathLength, result.FogCellCount,
						result.VisiblePathLength);
				}
				return string.Format(
					(string)STRINGS.ONIACCESS.CLUSTER_MAP.PATH_RESULT,
					result.VisiblePathLength);
			}

			if (result.HasFogPath) {
				return string.Format(
					(string)STRINGS.ONIACCESS.CLUSTER_MAP.PATH_THROUGH_FOG,
					result.FogPathLength, result.FogCellCount);
			}

			return string.Format(
				(string)STRINGS.ONIACCESS.CLUSTER_MAP.PATH_RESULT,
				result.VisiblePathLength);
		}

		private static int BFS(AxialI start, AxialI end, bool allowFog) {
			if (start == end) return 0;
			var grid = ClusterGrid.Instance;

			var visited = new HashSet<AxialI> { start };
			var queue = new Queue<KeyValuePair<AxialI, int>>();
			queue.Enqueue(new KeyValuePair<AxialI, int>(start, 0));

			while (queue.Count > 0) {
				var current = queue.Dequeue();
				foreach (var dir in AxialI.DIRECTIONS) {
					var neighbor = current.Key + dir;
					if (!grid.IsValidCell(neighbor)) continue;
					if (!visited.Add(neighbor)) continue;

					// Can't path through visible asteroids (except start/end)
					if (neighbor != end && grid.HasVisibleAsteroidAtCell(neighbor))
						continue;

					// Fog check: if not allowing fog, cell must be visible
					if (!allowFog && !grid.IsCellVisible(neighbor))
						continue;

					int dist = current.Value + 1;
					if (neighbor == end) return dist;
					queue.Enqueue(new KeyValuePair<AxialI, int>(neighbor, dist));
				}
			}
			return -1;
		}

		private static int CountFogCells(AxialI start, AxialI end) {
			// Run BFS again tracking fog cells on shortest path
			var grid = ClusterGrid.Instance;
			var visited = new HashSet<AxialI> { start };
			var queue = new Queue<KeyValuePair<AxialI, int>>();
			var parent = new Dictionary<AxialI, AxialI>();
			queue.Enqueue(new KeyValuePair<AxialI, int>(start, 0));

			bool found = false;
			while (queue.Count > 0 && !found) {
				var current = queue.Dequeue();
				foreach (var dir in AxialI.DIRECTIONS) {
					var neighbor = current.Key + dir;
					if (!grid.IsValidCell(neighbor)) continue;
					if (!visited.Add(neighbor)) continue;
					if (neighbor != end && grid.HasVisibleAsteroidAtCell(neighbor))
						continue;
					parent[neighbor] = current.Key;
					if (neighbor == end) { found = true; break; }
					queue.Enqueue(new KeyValuePair<AxialI, int>(
						neighbor, current.Value + 1));
				}
			}

			if (!found) return 0;

			int fogCount = 0;
			var cell = end;
			while (cell != start) {
				if (!grid.IsCellVisible(cell)) fogCount++;
				cell = parent[cell];
			}
			return fogCount;
		}
	}
}
