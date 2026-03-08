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
	}
}
