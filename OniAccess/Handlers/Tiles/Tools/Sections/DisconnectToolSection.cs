using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class DisconnectToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			ReadConduit(cell, ConduitType.Liquid, tokens);
			ReadConduit(cell, ConduitType.Gas, tokens);
			ReadConduit(cell, ConduitType.Solid, tokens);
			ReadPowerConnection(cell, tokens);
			return tokens;
		}

		private static void ReadConduit(int cell, ConduitType type, List<string> tokens) {
			var flow = Conduit.GetFlowManager(type);
			if (flow == null || !flow.HasConduit(cell)) return;
			tokens.Add(ConduitName(type));
		}

		private static void ReadPowerConnection(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Wire];
			if (go != null)
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.WIRE);
		}

		internal static string ConduitName(ConduitType type) {
			switch (type) {
				case ConduitType.Liquid: return (string)STRINGS.ONIACCESS.GLANCE.CONDUIT_LIQUID;
				case ConduitType.Gas: return (string)STRINGS.ONIACCESS.GLANCE.CONDUIT_GAS;
				case ConduitType.Solid: return (string)STRINGS.ONIACCESS.GLANCE.CONDUIT_SOLID;
				default: return type.ToString();
			}
		}
	}
}
