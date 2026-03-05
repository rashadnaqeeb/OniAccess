using System;
using System.Collections.Generic;
using System.Linq;

namespace OniAccess.Handlers.Tiles.AreaScan {
	/// <summary>
	/// Area scan for the Materials (TileMode) overlay.
	/// Full element breakdown by percentage, descending.
	/// </summary>
	public class MaterialsAreaScanner : IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				var masses = new Dictionary<string, List<float>>();
				for (int i = 0; i < cells.Length; i++) {
					int cell = cells[i];
					var element = Grid.Element[cell];
					string name = element.name;
					if (!masses.ContainsKey(name))
						masses[name] = new List<float>();
					if (!element.IsVacuum)
						masses[name].Add(Grid.Mass[cell]);
				}

				foreach (var kv in masses.OrderByDescending(kv => kv.Value.Count)) {
					int pct = (int)Math.Round(100.0 * kv.Value.Count / totalCells);
					if (pct == 0) pct = 1;
					if (kv.Value.Count > 0) {
						float median = AreaScanUtil.Median(kv.Value);
						tokens.Add(string.Format(
							STRINGS.ONIACCESS.BIG_CURSOR.ELEMENT_MASS_PCT,
							kv.Key, pct, AreaScanUtil.FormatMass(median)));
					} else {
						tokens.Add(string.Format(
							STRINGS.ONIACCESS.BIG_CURSOR.ELEMENT_PCT,
							kv.Key, pct));
					}
				}

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"MaterialsAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
