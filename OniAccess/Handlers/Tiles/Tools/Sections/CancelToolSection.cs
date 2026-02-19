using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class CancelToolSection: ICellSection {
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
			(int)ObjectLayer.DigPlacer,
			(int)ObjectLayer.MopPlacer,
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
				if (ReadCancellable(go, tokens)
					&& System.Array.IndexOf(BuildingLayers, layer) >= 0)
					ctx.Claimed.Add(go);
			}
			return tokens;
		}

		private static bool ReadCancellable(UnityEngine.GameObject go, List<string> tokens) {
			var diggable = go.GetComponent<Diggable>();
			if (diggable != null) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.DIG_ORDER);
				return true;
			}

			var moppable = go.GetComponent<Moppable>();
			if (moppable != null) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MOP_ORDER);
				return true;
			}

			var deconstructable = go.GetComponent<Deconstructable>();
			if (deconstructable != null && deconstructable.IsMarkedForDeconstruction()) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT);
				return true;
			}

			var constructable = go.GetComponent<Constructable>();
			if (constructable != null) {
				var sel = go.GetComponent<KSelectable>();
				string name = sel != null ? sel.GetName() : "";
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION, name));
				return true;
			}
			return false;
		}
	}
}
