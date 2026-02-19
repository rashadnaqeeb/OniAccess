using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads power infrastructure (wires, bridges) at a cell.
	/// </summary>
	public class PowerSection: ICellSection {
		private static readonly int[] _layers = {
			(int)ObjectLayer.Wire, (int)ObjectLayer.WireConnectors
		};

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tokens = new List<string>();
			foreach (int layer in _layers) {
				var go = Grid.Objects[cell, layer];
				if (go != null && ctx.Claimed.Add(go)) {
					var sel = go.GetComponent<KSelectable>();
					if (sel != null)
						tokens.Add(sel.GetName());
				}
			}
			return tokens;
		}
	}
}
