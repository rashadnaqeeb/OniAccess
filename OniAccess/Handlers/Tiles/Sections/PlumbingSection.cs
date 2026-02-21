using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads liquid plumbing infrastructure (pipes, bridges) at a cell.
	/// The game also registers buildings on LiquidConduitConnection at
	/// their port cells; skip those so BuildingSection handles them.
	/// </summary>
	public class PlumbingSection: ICellSection {
		private static readonly int[] _layers = {
			(int)ObjectLayer.LiquidConduit, (int)ObjectLayer.LiquidConduitConnection
		};

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tokens = new List<string>();
			foreach (int layer in _layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null || ctx.Claimed.Contains(go)) continue;
				if (PowerSection.IsPortRegistration(go, layer)) continue;
				ctx.Claimed.Add(go);
				var sel = go.GetComponent<KSelectable>();
				if (sel != null)
					tokens.Add(sel.GetName());
			}
			return tokens;
		}
	}
}
