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
			if (tokens.Count > 0)
				tokens.Add(FormatConnections(
					_getManager().GetConnections(cell, true)));
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
		/// Formats a UtilityConnections flags value as a shape name
		/// (e.g. "vertical", "up right corner"). Returns "unconnected"
		/// when no connections exist.
		/// </summary>
		internal static string FormatConnections(UtilityConnections connections) {
			bool up = (connections & UtilityConnections.Up) != 0;
			bool down = (connections & UtilityConnections.Down) != 0;
			bool left = (connections & UtilityConnections.Left) != 0;
			bool right = (connections & UtilityConnections.Right) != 0;
			int count = (up ? 1 : 0) + (down ? 1 : 0)
				+ (left ? 1 : 0) + (right ? 1 : 0);

			switch (count) {
				case 0:
					return STRINGS.ONIACCESS.GLANCE.SHAPE_ALONE;
				case 1:
					string dir = up ? STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN
						: down ? STRINGS.ONIACCESS.SCANNER.DIRECTION_UP
						: left ? STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT
						: STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT;
					return string.Format(
						STRINGS.ONIACCESS.GLANCE.SHAPE_END, dir);
				case 2:
					if (up && down)
						return STRINGS.ONIACCESS.GLANCE.SHAPE_VERTICAL;
					if (left && right)
						return STRINGS.ONIACCESS.GLANCE.SHAPE_HORIZONTAL;
					string d1 = up
						? STRINGS.ONIACCESS.SCANNER.DIRECTION_UP
						: STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN;
					string d2 = left
						? STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT
						: STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT;
					return string.Format(
						STRINGS.ONIACCESS.GLANCE.SHAPE_CORNER, d1, d2);
				case 3:
					string branch = !up
						? STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN
						: !down ? STRINGS.ONIACCESS.SCANNER.DIRECTION_UP
						: !left ? STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT
						: STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT;
					return string.Format(
						STRINGS.ONIACCESS.GLANCE.SHAPE_T, branch);
				default:
					return STRINGS.ONIACCESS.GLANCE.SHAPE_CROSS;
			}
		}
	}
}
