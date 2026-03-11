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
			var bridgeConnections = (UtilityConnections)0;
			foreach (int layer in _layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null || ctx.Claimed.Contains(go)) continue;
				if (IsPortRegistration(go, layer)) continue;
				if (IsBridgeEndpoint(go)) {
					bridgeConnections |= GetBridgeDirection(go, cell);
					continue;
				}
				ctx.Claimed.Add(go);
				var sel = go.GetComponent<KSelectable>();
				if (sel != null)
					tokens.Add(sel.GetName());
			}
			if (tokens.Count > 0)
				tokens.Add(FormatConnections(
					_getManager().GetConnections(cell, true)
					| bridgeConnections));
			FindBridgeMiddle(cell, _layers, ctx, tokens);
			return tokens;
		}

		internal static UtilityConnections GetBridgeDirection(
				UnityEngine.GameObject go, int cell) {
			var building = go.GetComponent<Building>();
			int origin = Grid.PosToCell(building.transform.GetPosition());
			int dx = Grid.CellColumn(origin) - Grid.CellColumn(cell);
			int dy = Grid.CellRow(origin) - Grid.CellRow(cell);
			if (dx > 0) return UtilityConnections.Right;
			if (dx < 0) return UtilityConnections.Left;
			if (dy > 0) return UtilityConnections.Up;
			return UtilityConnections.Down;
		}

		/// <summary>
		/// Bridges (Conduit, WireBridge, LogicBridge build rules) are not
		/// registered on any object layer at their middle cell. Scan
		/// adjacent cells on the given layers for buildings whose
		/// PlacementCells include the current cell.
		/// </summary>
		internal static void FindBridgeMiddle(
				int cell, int[] layers, CellContext ctx,
				List<string> tokens) {
			int cx = Grid.CellColumn(cell);
			int cy = Grid.CellRow(cell);
			foreach (int layer in layers) {
				CheckBridgeNeighbor(Grid.XYToCell(cx - 1, cy),
					layer, cell, ctx, tokens);
				CheckBridgeNeighbor(Grid.XYToCell(cx + 1, cy),
					layer, cell, ctx, tokens);
				CheckBridgeNeighbor(Grid.XYToCell(cx, cy - 1),
					layer, cell, ctx, tokens);
				CheckBridgeNeighbor(Grid.XYToCell(cx, cy + 1),
					layer, cell, ctx, tokens);
			}
		}

		private static void CheckBridgeNeighbor(
				int neighbor, int layer, int targetCell,
				CellContext ctx, List<string> tokens) {
			if (!Grid.IsValidCell(neighbor)) return;
			var go = Grid.Objects[neighbor, layer];
			if (go == null || ctx.Claimed.Contains(go)) return;
			if (!IsBridgeEndpoint(go)) return;
			var building = go.GetComponent<Building>();
			if (!building.PlacementCellsContainCell(targetCell)) return;
			ctx.Claimed.Add(go);
			var sel = go.GetComponent<KSelectable>();
			if (sel != null)
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.BRIDGE_MIDDLE,
					sel.GetName()));
		}

		/// <summary>
		/// True when the object is a bridge endpoint. Bridge endpoints
		/// are handled by BuildingSection (port labels + name), not here.
		/// </summary>
		internal static bool IsBridgeEndpoint(UnityEngine.GameObject go) {
			var building = go.GetComponent<Building>();
			if (building == null) return false;
			var rule = building.Def.BuildLocationRule;
			return rule == BuildLocationRule.Conduit
				|| rule == BuildLocationRule.WireBridge
				|| rule == BuildLocationRule.LogicBridge;
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
