using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads duplicants (ObjectLayer.Minion), critters (ObjectLayer.Critter),
	/// and plants (ObjectLayer.Plants) at a cell. Traverses ObjectLayerListItem
	/// linked lists to find all entities per layer. Plants include growth state.
	/// </summary>
	public class EntitySection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			ReadMinions(cell, tokens);
			ReadCritters(cell, tokens);
			ReadPlants(cell, tokens);
			return tokens;
		}

		private static void ReadMinions(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Minion];
			if (go == null) return;
			var selectable = go.GetComponent<KSelectable>();
			if (selectable != null)
				tokens.Add(selectable.GetName());
		}

		private static void ReadCritters(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Critter];
			if (go == null) return;

			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) {
				var selectable = go.GetComponent<KSelectable>();
				if (selectable != null)
					tokens.Add(selectable.GetName());
				return;
			}

			var item = pickupable.objectLayerListItem;
			while (item != null) {
				var selectable = item.gameObject.GetComponent<KSelectable>();
				if (selectable != null)
					tokens.Add(selectable.GetName());
				item = item.nextItem;
			}
		}

		private static void ReadPlants(int cell, List<string> tokens) {
			var go = Grid.Objects[cell, (int)ObjectLayer.Plants];
			if (go == null) return;
			var selectable = go.GetComponent<KSelectable>();
			if (selectable == null) return;

			tokens.Add(selectable.GetName());
			var growing = go.GetComponent<Growing>();
			if (growing != null) {
				if (growing.IsGrown())
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.PLANT_GROWN);
				else if (growing.IsGrowing())
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.PLANT_GROWING);
				else
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.PLANT_STALLED);
			}
		}
	}
}
