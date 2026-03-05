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

				var counts = new Dictionary<string, int>();
				for (int i = 0; i < cells.Length; i++) {
					string name = Grid.Element[cells[i]].name;
					if (counts.ContainsKey(name))
						counts[name]++;
					else
						counts[name] = 1;
				}

				foreach (var kv in counts.OrderByDescending(kv => kv.Value)) {
					int pct = (int)Math.Round(100.0 * kv.Value / totalCells);
					if (pct == 0) pct = 1;
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.ELEMENT_PCT,
						kv.Key, pct));
				}

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"MaterialsAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
