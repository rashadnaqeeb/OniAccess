using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class DiseaseAreaScanner: IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				// Accumulate total germ count and cell count per disease type
				var germTotals = new Dictionary<byte, long>();
				var germCellCounts = new Dictionary<byte, int>();

				for (int i = 0; i < cells.Length; i++) {
					int cell = cells[i];
					Accumulate(germTotals, germCellCounts,
						Grid.DiseaseIdx[cell], Grid.DiseaseCount[cell]);
					AccumulateBuildings(cell, germTotals, germCellCounts);
					AccumulatePickupables(cell, germTotals, germCellCounts);
					AccumulateConduits(cell, germTotals, germCellCounts);
				}

				if (germTotals.Count == 0) {
					tokens.Add((string)STRINGS.ONIACCESS.BIG_CURSOR.DISEASE_CLEAR);
				} else {
					foreach (var pair in germTotals) {
						string name = Db.Get().Diseases[pair.Key].Name;
						int avg = (int)(pair.Value / germCellCounts[pair.Key]);
						tokens.Add(string.Format(
							STRINGS.ONIACCESS.BIG_CURSOR.AVG_DISEASE,
							name,
							GameUtil.GetFormattedDiseaseAmount(avg)));
					}
				}

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"DiseaseAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}

		private static void Accumulate(Dictionary<byte, long> totals,
				Dictionary<byte, int> cellCounts, byte idx, int count) {
			if (idx == byte.MaxValue || count <= 0) return;
			if (totals.ContainsKey(idx)) {
				totals[idx] += count;
				cellCounts[idx]++;
			} else {
				totals[idx] = count;
				cellCounts[idx] = 1;
			}
		}

		private static void AccumulateBuildings(int cell,
				Dictionary<byte, long> totals,
				Dictionary<byte, int> cellCounts) {
			AccumulateBuildingLayer(cell, ObjectLayer.Building, totals, cellCounts);
			AccumulateBuildingLayer(cell, ObjectLayer.FoundationTile, totals, cellCounts);
			AccumulateStorage(cell, ObjectLayer.Building, totals, cellCounts);
			AccumulateStorage(cell, ObjectLayer.FoundationTile, totals, cellCounts);
		}

		private static void AccumulateBuildingLayer(int cell, ObjectLayer layer,
				Dictionary<byte, long> totals,
				Dictionary<byte, int> cellCounts) {
			var go = Grid.Objects[cell, (int)layer];
			if (go == null) return;
			var pe = go.GetComponent<PrimaryElement>();
			if (pe == null) return;
			Accumulate(totals, cellCounts, pe.DiseaseIdx, pe.DiseaseCount);
		}

		private static void AccumulateStorage(int cell, ObjectLayer layer,
				Dictionary<byte, long> totals,
				Dictionary<byte, int> cellCounts) {
			var go = Grid.Objects[cell, (int)layer];
			if (go == null) return;
			var storage = go.GetComponent<Storage>();
			if (storage == null) return;
			foreach (var item in storage.items) {
				if (item == null) continue;
				var pe = item.GetComponent<PrimaryElement>();
				if (pe == null) continue;
				Accumulate(totals, cellCounts, pe.DiseaseIdx, pe.DiseaseCount);
			}
		}

		private static void AccumulatePickupables(int cell,
				Dictionary<byte, long> totals,
				Dictionary<byte, int> cellCounts) {
			var headGo = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (headGo == null) return;
			var pickupable = headGo.GetComponent<Pickupable>();
			if (pickupable == null) return;
			for (var item = pickupable.objectLayerListItem;
				item != null;
				item = item.nextItem) {
				var pe = item.gameObject.GetComponent<PrimaryElement>();
				if (pe == null) continue;
				Accumulate(totals, cellCounts, pe.DiseaseIdx, pe.DiseaseCount);
			}
		}

		private static void AccumulateConduits(int cell,
				Dictionary<byte, long> totals,
				Dictionary<byte, int> cellCounts) {
			var liquidContents = Game.Instance.liquidConduitFlow.GetContents(cell);
			Accumulate(totals, cellCounts,
				liquidContents.diseaseIdx, liquidContents.diseaseCount);

			var gasContents = Game.Instance.gasConduitFlow.GetContents(cell);
			Accumulate(totals, cellCounts,
				gasContents.diseaseIdx, gasContents.diseaseCount);

			var solidContents = Game.Instance.solidConduitFlow.GetContents(cell);
			if (!solidContents.pickupableHandle.IsValid()) return;
			var solidPickupable = Game.Instance.solidConduitFlow.GetPickupable(
				solidContents.pickupableHandle);
			if (solidPickupable == null) return;
			var solidPe = solidPickupable.GetComponent<PrimaryElement>();
			if (solidPe == null) return;
			Accumulate(totals, cellCounts, solidPe.DiseaseIdx, solidPe.DiseaseCount);
		}
	}
}
