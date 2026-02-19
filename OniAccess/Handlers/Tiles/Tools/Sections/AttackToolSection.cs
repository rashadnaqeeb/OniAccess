using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	public class AttackToolSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return System.Array.Empty<string>();

			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return System.Array.Empty<string>();

			var tokens = new List<string>();
			var item = pickupable.objectLayerListItem;
			while (item != null) {
				var faction = item.gameObject.GetComponent<FactionAlignment>();
				if (faction != null && faction.IsAlignmentActive()
					&& FactionManager.Instance.GetDisposition(
						FactionManager.FactionID.Duplicant, faction.Alignment)
						!= FactionManager.Disposition.Assist) {
					var sel = item.gameObject.GetComponent<KSelectable>();
					if (sel != null) {
						if (faction.IsPlayerTargeted())
							tokens.Add(sel.GetName() + ", " +
								(string)STRINGS.ONIACCESS.TOOLS.MARKED_ATTACK);
						else
							tokens.Add(sel.GetName());
					}
				}
				item = item.nextItem;
			}
			return tokens;
		}
	}
}
