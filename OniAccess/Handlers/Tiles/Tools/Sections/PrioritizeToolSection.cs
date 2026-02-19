using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class PrioritizeToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();

			var digGo = Grid.Objects[cell, (int)ObjectLayer.DigPlacer];
			if (digGo != null && digGo.GetComponent<Diggable>() != null) {
				var pri = digGo.GetComponent<Prioritizable>();
				if (pri != null)
					tokens.Add(string.Format(
						(string)STRINGS.ONIACCESS.TOOLS.DIG_ORDER_PRIORITY,
						pri.GetMasterPriority().priority_value));
				else
					tokens.Add((string)STRINGS.ONIACCESS.TOOLS.DIG_ORDER);
			}

			CheckConstructionOrder(cell, (int)ObjectLayer.Building, tokens);
			CheckConstructionOrder(cell, (int)ObjectLayer.FoundationTile, tokens);
			CheckDeconstructOrder(cell, (int)ObjectLayer.Building, tokens);
			CheckDeconstructOrder(cell, (int)ObjectLayer.FoundationTile, tokens);
			return tokens;
		}

		private static void CheckConstructionOrder(int cell, int layer, List<string> tokens) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			var c = go.GetComponent<Constructable>();
			if (c == null) return;
			var selectable = go.GetComponent<KSelectable>();
			string name = selectable != null ? selectable.GetName() : "building";
			string label = string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION, name);
			var pri = go.GetComponent<Prioritizable>();
			if (pri != null)
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.ORDER_PRIORITY,
					label, pri.GetMasterPriority().priority_value));
			else
				tokens.Add(label);
		}

		private static void CheckDeconstructOrder(int cell, int layer, List<string> tokens) {
			var go = Grid.Objects[cell, layer];
			if (go == null) return;
			var d = go.GetComponent<Deconstructable>();
			if (d != null && d.IsMarkedForDeconstruction()) {
				var pri = go.GetComponent<Prioritizable>();
				if (pri != null)
					tokens.Add(string.Format(
						(string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT_PRIORITY,
						pri.GetMasterPriority().priority_value));
				else
					tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT);
			}
		}
	}
}
