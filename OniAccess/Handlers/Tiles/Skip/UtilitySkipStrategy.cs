namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Skips along utility networks (power, plumbing, ventilation,
	/// conveyor, automation). Stops at buildings, junctions, network
	/// boundaries, and transitions between empty and occupied cells.
	/// Parameterized by object layers and a cell-to-network accessor.
	/// </summary>
	public class UtilitySkipStrategy: ISkipStrategy {
		private static readonly object Empty = new object();

		private readonly int[] _layers;
		private readonly System.Func<int, UtilityNetwork> _getNetwork;

		public UtilitySkipStrategy(int[] layers,
				System.Func<int, UtilityNetwork> getNetwork) {
			_layers = layers;
			_getNetwork = getNetwork;
		}

		public object GetSignature(int cell) {
			UnityEngine.GameObject go = FindObject(cell);
			if (go == null) return Empty;

			var building = go.GetComponent<Building>();
			if (building == null) return Empty;

			if (!building.Def.isUtility)
				return go.GetInstanceID();

			int networkId = _getNetwork(cell)?.id ?? -1;
			bool isJunction = CountSameNetworkNeighbors(cell, networkId) >= 3;
			return (networkId, isJunction);
		}

		private UnityEngine.GameObject FindObject(int cell) {
			foreach (int layer in _layers) {
				var go = Grid.Objects[cell, layer];
				if (go != null
					&& !Sections.ConduitSection.IsPortRegistration(go, layer))
					return go;
			}
			return null;
		}

		private int CountSameNetworkNeighbors(int cell, int networkId) {
			int count = 0;
			if (HasSameNetwork(Grid.CellAbove(cell), networkId)) count++;
			if (HasSameNetwork(Grid.CellBelow(cell), networkId)) count++;
			if (HasSameNetwork(Grid.CellLeft(cell), networkId)) count++;
			if (HasSameNetwork(Grid.CellRight(cell), networkId)) count++;
			return count;
		}

		private bool HasSameNetwork(int neighbor, int networkId) {
			if (!Grid.IsValidCell(neighbor)) return false;
			var net = _getNetwork(neighbor);
			return net != null && net.id == networkId;
		}
	}
}
