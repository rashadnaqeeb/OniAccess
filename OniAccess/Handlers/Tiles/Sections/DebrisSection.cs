using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Lists loose items (pickupables) on the cell by proper name.
	/// Uses GetProperName (not GetUnitFormattedName) so no mass is spoken.
	/// Traverses the ObjectLayerListItem linked list for stacked items.
	/// </summary>
	public class DebrisSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (go == null) return System.Array.Empty<string>();

			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return System.Array.Empty<string>();

			var tokens = new List<string>();
			var item = pickupable.objectLayerListItem;
			while (item != null) {
				tokens.Add(item.gameObject.GetProperName());
				item = item.nextItem;
			}
			return tokens;
		}
	}
}
