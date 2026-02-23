using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.ToolProfiles.Sections {
	public class DisconnectToolSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tool = PlayerController.Instance.ActiveTool as FilteredDragTool;
			var tokens = new List<string>();
			ReadConduit(cell, ConduitType.Liquid, ObjectLayer.LiquidConduit, tool, tokens);
			ReadConduit(cell, ConduitType.Gas, ObjectLayer.GasConduit, tool, tokens);
			ReadConduit(cell, ConduitType.Solid, ObjectLayer.SolidConduit, tool, tokens);
			ReadPowerConnection(cell, tool, tokens);
			return tokens;
		}

		private static void ReadConduit(int cell, ConduitType type, ObjectLayer layer,
			FilteredDragTool tool, List<string> tokens) {
			if (tool != null && !tool.IsActiveLayer(layer)) return;
			var flow = Conduit.GetFlowManager(type);
			if (flow == null || !flow.HasConduit(cell)) return;
			tokens.Add(ConduitName(type));
		}

		private static void ReadPowerConnection(int cell, FilteredDragTool tool, List<string> tokens) {
			if (tool != null && !tool.IsActiveLayer(ObjectLayer.Wire)) return;
			var go = Grid.Objects[cell, (int)ObjectLayer.Wire];
			if (go != null)
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.WIRE);
		}

		internal static string ConduitName(ConduitType type) {
			switch (type) {
				case ConduitType.Liquid: return (string)STRINGS.ONIACCESS.GLANCE.CONDUIT_LIQUID;
				case ConduitType.Gas: return (string)STRINGS.ONIACCESS.GLANCE.CONDUIT_GAS;
				case ConduitType.Solid: return (string)STRINGS.ONIACCESS.GLANCE.CONDUIT_SOLID;
				default: return (string)STRINGS.ONIACCESS.GLANCE.UNKNOWN_ELEMENT;
			}
		}
	}
}
