using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.ToolProfiles.Sections {
	public class DigToolSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
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

			var element = Grid.Element[cell];
			if (element != null) {
				tokens.Add(element.name);
				string hardness = GameUtil.GetHardnessString(element);
				if (!string.IsNullOrEmpty(hardness))
					tokens.Add(hardness);
				float mass = Grid.Mass[cell];
				if (mass > 0f)
					tokens.Add(GameUtil.GetFormattedMass(mass));
			}

			return tokens;
		}
	}
}
