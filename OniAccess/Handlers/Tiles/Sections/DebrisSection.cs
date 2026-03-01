using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Lists loose items (pickupables) on the cell by proper name.
	/// Uses GetProperName (not GetUnitFormattedName) so no mass is spoken.
	/// Traverses the ObjectLayerListItem linked list for stacked items.
	/// Skips duplicants and critters (handled by EntitySection).
	/// </summary>
	public class DebrisSection: ICellSection {
		private static bool IsBottle(KPrefabID prefabId) {
			return prefabId.HasTag(GameTags.Liquid)
				|| prefabId.HasTag(GameTags.Breathable)
				|| prefabId.HasTag(GameTags.Unbreathable);
		}

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return System.Array.Empty<string>();

			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return System.Array.Empty<string>();

			var tokens = new List<string>();
			var item = pickupable.objectLayerListItem;
			while (item != null) {
				if (item.gameObject.GetComponent<MinionIdentity>() == null
					&& item.gameObject.GetComponent<CreatureBrain>() == null) {
					string name = item.gameObject.GetProperName();
					var prefabId = item.gameObject.GetComponent<KPrefabID>();
					if (prefabId != null && IsBottle(prefabId))
						name = (string)STRINGS.ONIACCESS.SCANNER.BOTTLE_PREFIX + name;
					tokens.Add(name);
				}
				item = item.nextItem;
			}
			return tokens;
		}
	}
}
