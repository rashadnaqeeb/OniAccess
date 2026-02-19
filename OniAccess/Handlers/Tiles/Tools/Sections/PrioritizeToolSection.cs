using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class PrioritizeToolSection : ICellSection {
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
			(int)ObjectLayer.Pickupables,
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

				if (layer == (int)ObjectLayer.Pickupables) {
					var pickupable = go.GetComponent<Pickupable>();
					if (pickupable == null) continue;
					var item = pickupable.objectLayerListItem;
					while (item != null) {
						var obj = item.gameObject;
						item = item.nextItem;
						if (obj == null) continue;
						if (obj.GetComponent<MinionIdentity>() != null) continue;
						ReadPrioritizable(obj, tool, tokens);
					}
				} else {
					if (ReadPrioritizable(go, tool, tokens)
						&& System.Array.IndexOf(BuildingLayers, layer) >= 0)
						ctx.Claimed.Add(go);
				}
			}
			return tokens;
		}

		private static bool ReadPrioritizable(UnityEngine.GameObject go,
			FilteredDragTool tool, List<string> tokens) {
			if (tool != null) {
				string filterLayer = tool.GetFilterLayerFromGameObject(go);
				if (!tool.IsActiveLayer(filterLayer)) return false;
			}

			var pri = go.GetComponent<Prioritizable>();
			if (pri == null || !pri.showIcon || !pri.IsPrioritizable()) return false;

			int priority = pri.GetMasterPriority().priority_value;

			var diggable = go.GetComponent<Diggable>();
			if (diggable != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.DIG_ORDER_PRIORITY, priority));
				return true;
			}

			var constructable = go.GetComponent<Constructable>();
			if (constructable != null) {
				var sel = go.GetComponent<KSelectable>();
				string name = sel != null ? sel.GetName() : "";
				string label = string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION, name);
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.ORDER_PRIORITY, label, priority));
				return true;
			}

			var deconstructable = go.GetComponent<Deconstructable>();
			if (deconstructable != null && deconstructable.IsMarkedForDeconstruction()) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.MARKED_DECONSTRUCT_PRIORITY, priority));
				return true;
			}

			var clearable = go.GetComponent<Clearable>();
			if (clearable != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.MARKED_SWEEP_PRIORITY, priority));
				return true;
			}

			var moppable = go.GetComponent<Moppable>();
			if (moppable != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.MOP_ORDER_PRIORITY, priority));
				return true;
			}

			var sel2 = go.GetComponent<KSelectable>();
			if (sel2 != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.ORDER_PRIORITY,
					sel2.GetName(), priority));
				return true;
			}
			return false;
		}
	}
}
