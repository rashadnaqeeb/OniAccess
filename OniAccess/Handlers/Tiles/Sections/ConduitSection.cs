using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads conduit/wire infrastructure at a cell. Skips buildings that
	/// are registered on a layer only for port tracking (handled by
	/// BuildingSection). Parameterized by the object layers to scan.
	/// </summary>
	public class ConduitSection: ICellSection {
		private readonly int[] _layers;
		private readonly Func<IUtilityNetworkMgr> _getManager;

		public ConduitSection(Func<IUtilityNetworkMgr> getManager,
				params int[] layers) {
			_getManager = getManager;
			_layers = layers;
		}

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tokens = new List<string>();
			foreach (int layer in _layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null || ctx.Claimed.Contains(go)) continue;
				if (IsPortRegistration(go, layer)) continue;
				ctx.Claimed.Add(go);
				var sel = go.GetComponent<KSelectable>();
				if (sel != null)
					tokens.Add(sel.GetName());
			}
			if (tokens.Count > 0) {
				var conn = FormatConnections(
					_getManager().GetConnections(cell, true));
				if (conn != null)
					tokens.Add(conn);
			}
			return tokens;
		}

		/// <summary>
		/// True when a building was registered on this layer for port
		/// tracking rather than being infrastructure that lives here.
		/// </summary>
		internal static bool IsPortRegistration(
				UnityEngine.GameObject go, int layer) {
			var building = go.GetComponent<Building>();
			return building != null
				&& (int)building.Def.ObjectLayer != layer;
		}

		/// <summary>
		/// Formats a UtilityConnections flags value as "connects up, down"
		/// etc. Returns null if no connections.
		/// </summary>
		internal static string FormatConnections(UtilityConnections connections) {
			if (connections == 0) return null;
			var dirs = new List<string>(4);
			if ((connections & UtilityConnections.Up) != 0)
				dirs.Add(STRINGS.ONIACCESS.SCANNER.DIRECTION_UP);
			if ((connections & UtilityConnections.Down) != 0)
				dirs.Add(STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN);
			if ((connections & UtilityConnections.Left) != 0)
				dirs.Add(STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT);
			if ((connections & UtilityConnections.Right) != 0)
				dirs.Add(STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT);
			if (dirs.Count == 0) return null;
			return string.Format(
				STRINGS.ONIACCESS.GLANCE.CONNECTS,
				string.Join(", ", dirs));
		}
	}
}
