using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads power infrastructure (wires, bridges) at a cell.
	/// The game registers buildings on WireConnectors at their power
	/// port cells; skip those so BuildingSection handles them.
	/// </summary>
	public class PowerSection: ICellSection {
		private static readonly int[] _layers = {
			(int)ObjectLayer.Wire, (int)ObjectLayer.WireConnectors
		};

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
	}
}
