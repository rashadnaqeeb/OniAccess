using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads gas ventilation infrastructure (pipes, bridges) at a cell.
	/// </summary>
	public class VentilationSection : ICellSection {
		private static readonly int[] _layers = {
			(int)ObjectLayer.GasConduit, (int)ObjectLayer.GasConduitConnection
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
