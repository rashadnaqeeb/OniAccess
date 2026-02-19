using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Aggregates germs by disease type across all sources at a cell:
	/// tile surface, buildings, pickupables, and pipe contents.
	/// Returns one token per disease type, or "clean" if total is zero.
	/// </summary>
	public class DiseaseSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var totals = new Dictionary<byte, int>();
			AddTileSurface(cell, totals);
			AddBuildings(cell, totals);
			AddPickupables(cell, totals);
			AddConduits(cell, totals);

			if (totals.Count == 0)
				return new[] { (string)STRINGS.ONIACCESS.GLANCE.DISEASE_CLEAR };

			var tokens = new List<string>(totals.Count);
			foreach (var pair in totals) {
				string name = Db.Get().Diseases[pair.Key].Name;
				string amount = GameUtil.GetFormattedDiseaseAmount(pair.Value);
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.DISEASE_ENTRY,
					name, amount));
			}
			return tokens;
		}

		private static void Accumulate(
				Dictionary<byte, int> totals, byte idx, int count) {
			if (idx == byte.MaxValue || count <= 0) return;
			if (totals.ContainsKey(idx))
				totals[idx] += count;
			else
				totals[idx] = count;
		}

		private static void AddTileSurface(int cell, Dictionary<byte, int> totals) {
			Accumulate(totals, Grid.DiseaseIdx[cell], Grid.DiseaseCount[cell]);
		}

		private static void AddBuildings(int cell, Dictionary<byte, int> totals) {
			AddBuildingLayer(cell, ObjectLayer.Building, totals);
			AddBuildingLayer(cell, ObjectLayer.FoundationTile, totals);
		}

		private static void AddBuildingLayer(
				int cell, ObjectLayer layer, Dictionary<byte, int> totals) {
			var go = Grid.Objects[cell, (int)layer];
			if (go == null) return;
			var pe = go.GetComponent<PrimaryElement>();
			if (pe == null) return;
			Accumulate(totals, pe.DiseaseIdx, pe.DiseaseCount);
		}

		private static void AddPickupables(int cell, Dictionary<byte, int> totals) {
			var headGo = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (headGo == null) return;
			var pickupable = headGo.GetComponent<Pickupable>();
			if (pickupable == null) return;
			for (var item = pickupable.objectLayerListItem;
				item != null;
				item = item.nextItem) {
				var pe = item.gameObject.GetComponent<PrimaryElement>();
				if (pe == null) continue;
				Accumulate(totals, pe.DiseaseIdx, pe.DiseaseCount);
			}
		}

		private static void AddConduits(int cell, Dictionary<byte, int> totals) {
			AddConduitFlow(Game.Instance.liquidConduitFlow, cell, totals);
			AddConduitFlow(Game.Instance.gasConduitFlow, cell, totals);
		}

		private static void AddConduitFlow(
				ConduitFlow flow, int cell, Dictionary<byte, int> totals) {
			var contents = flow.GetContents(cell);
			Accumulate(totals, contents.diseaseIdx, contents.diseaseCount);
		}
	}
}
