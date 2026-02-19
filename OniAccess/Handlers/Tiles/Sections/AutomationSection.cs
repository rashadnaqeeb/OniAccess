using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads automation infrastructure (wires, gates) at a cell.
	/// </summary>
	public class AutomationSection : ICellSection {
		private static readonly int[] _layers = {
			(int)ObjectLayer.LogicWire, (int)ObjectLayer.LogicGate
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
