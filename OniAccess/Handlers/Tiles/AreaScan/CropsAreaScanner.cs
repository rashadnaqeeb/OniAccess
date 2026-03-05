using System;
using System.Collections.Generic;
using System.Linq;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class CropsAreaScanner : IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				// Group plants by name, tracking count and sum of growth %
				var plantCounts = new Dictionary<string, int>();
				var growthSums = new Dictionary<string, float>();

				for (int i = 0; i < cells.Length; i++) {
					var go = Grid.Objects[cells[i], (int)ObjectLayer.Building];
					if (go == null) continue;
					var growing = go.GetComponent<Growing>();
					if (growing == null) continue;

					var selectable = go.GetComponent<KSelectable>();
					string name = selectable != null
						? selectable.GetName()
						: go.GetComponent<KPrefabID>().PrefabTag.ProperName();

					float growth = growing.PercentGrown();

					if (plantCounts.ContainsKey(name)) {
						plantCounts[name]++;
						growthSums[name] += growth;
					} else {
						plantCounts[name] = 1;
						growthSums[name] = growth;
					}
				}

				if (plantCounts.Count == 0) {
					tokens.Add((string)STRINGS.ONIACCESS.BIG_CURSOR.NO_PLANTS);
				} else {
					foreach (var kv in plantCounts.OrderByDescending(kv => kv.Value)) {
						float avgGrowth = growthSums[kv.Key] / kv.Value;
						int growthPct = (int)Math.Round(avgGrowth * 100f);
						tokens.Add(string.Format(
							STRINGS.ONIACCESS.BIG_CURSOR.PLANT_ENTRY,
							kv.Value, kv.Key, growthPct));
					}
				}

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"CropsAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
