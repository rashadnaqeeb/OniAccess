using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class DeconstructToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			ReadLayer(cell, (int)ObjectLayer.Building, tokens);
			ReadLayer(cell, (int)ObjectLayer.FoundationTile, tokens);
			return tokens;
		}

		private static void ReadLayer(int cell, int layer, List<string> tokens) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			var deconstructable = go.GetComponent<Deconstructable>();
			if (deconstructable == null) return;
			var sel = go.GetComponent<KSelectable>();
			if (sel == null) return;

			if (deconstructable.IsMarkedForDeconstruction()) {
				var pri = go.GetComponent<Prioritizable>();
				if (pri != null)
					tokens.Add(string.Format(
						(string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT_PRIORITY,
						pri.GetMasterPriority().priority_value));
				else
					tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT);
			}
			tokens.Add(sel.GetName());
		}
	}
}
