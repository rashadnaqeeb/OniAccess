using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class DeconstructToolSection : ICellSection {
		private static readonly int[] Layers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
			(int)ObjectLayer.Backwall,
			(int)ObjectLayer.Gantry,
			(int)ObjectLayer.Wire,
			(int)ObjectLayer.WireConnectors,
			(int)ObjectLayer.LiquidConduit,
			(int)ObjectLayer.LiquidConduitConnection,
			(int)ObjectLayer.GasConduit,
			(int)ObjectLayer.GasConduitConnection,
			(int)ObjectLayer.SolidConduit,
			(int)ObjectLayer.SolidConduitConnection,
			(int)ObjectLayer.LogicGate,
			(int)ObjectLayer.LogicWire,
		};

		private static readonly int[] BuildingLayers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
			(int)ObjectLayer.Backwall,
		};

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tool = PlayerController.Instance.ActiveTool as FilteredDragTool;
			var tokens = new List<string>();
			foreach (int layer in Layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null) continue;
				if (tool != null) {
					string filterLayer = tool.GetFilterLayerFromGameObject(go);
					if (!tool.IsActiveLayer(filterLayer)) continue;
				}
				var deconstructable = go.GetComponent<Deconstructable>();
				if (deconstructable == null) continue;
				var sel = go.GetComponent<KSelectable>();
				if (sel == null) continue;

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
				if (System.Array.IndexOf(BuildingLayers, layer) >= 0)
					ctx.Claimed.Add(go);
			}
			return tokens;
		}
	}
}
