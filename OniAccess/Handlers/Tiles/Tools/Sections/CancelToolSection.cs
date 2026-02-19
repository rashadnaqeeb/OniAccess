using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class CancelToolSection : ICellSection {
		private static readonly int[] Layers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
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
			(int)ObjectLayer.Backwall,
			(int)ObjectLayer.DigPlacer,
			(int)ObjectLayer.MopPlacer,
		};

		public IEnumerable<string> Read(int cell) {
			var tool = PlayerController.Instance.ActiveTool as FilteredDragTool;
			var tokens = new List<string>();
			foreach (int layer in Layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null) continue;
				if (tool != null) {
					string filterLayer = tool.GetFilterLayerFromGameObject(go);
					if (!tool.IsActiveLayer(filterLayer)) continue;
				}
				ReadCancellable(go, tokens);
			}
			return tokens;
		}

		private static void ReadCancellable(UnityEngine.GameObject go, List<string> tokens) {
			var diggable = go.GetComponent<Diggable>();
			if (diggable != null) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.DIG_ORDER);
				return;
			}

			var moppable = go.GetComponent<Moppable>();
			if (moppable != null) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MOP_ORDER);
				return;
			}

			var deconstructable = go.GetComponent<Deconstructable>();
			if (deconstructable != null && deconstructable.IsMarkedForDeconstruction()) {
				tokens.Add((string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT);
				return;
			}

			var constructable = go.GetComponent<Constructable>();
			if (constructable != null) {
				var sel = go.GetComponent<KSelectable>();
				string name = sel != null ? sel.GetName() : "";
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION, name));
			}
		}
	}
}
