using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class AttackToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Critter];
			if (go == null) return System.Array.Empty<string>();
			var sel = go.GetComponent<KSelectable>();
			if (sel == null) return System.Array.Empty<string>();
			var faction = go.GetComponent<FactionAlignment>();
			if (faction != null && faction.IsPlayerTargeted())
				return new[] { sel.GetName() + ", " +
					(string)STRINGS.ONIACCESS.TOOLS.MARKED_ATTACK };
			return new[] { sel.GetName() };
		}
	}
}
