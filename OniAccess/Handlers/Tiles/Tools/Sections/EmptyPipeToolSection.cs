using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class EmptyPipeToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			ReadConduit(cell, ConduitType.Liquid, tokens);
			ReadConduit(cell, ConduitType.Gas, tokens);
			ReadConduit(cell, ConduitType.Solid, tokens);
			return tokens;
		}

		private static void ReadConduit(int cell, ConduitType type, List<string> tokens) {
			var flow = Conduit.GetFlowManager(type);
			if (flow == null) return;
			if (!flow.HasConduit(cell)) return;
			string typeName = DisconnectToolSection.ConduitName(type);
			var contents = flow.GetContents(cell);
			if (contents.mass <= 0f) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.PIPE_EMPTY, typeName));
			} else {
				var element = ElementLoader.FindElementByHash(contents.element);
				string elementName = element != null
					? element.name
					: (string)STRINGS.ONIACCESS.GLANCE.UNKNOWN_ELEMENT;
				string mass = Tiles.Sections.ElementSection.FormatGlanceMass(contents.mass);
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.PIPE_CONTENTS,
					typeName, elementName, mass));
			}
		}

	}
}
