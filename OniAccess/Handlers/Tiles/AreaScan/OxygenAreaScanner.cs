using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class OxygenAreaScanner : IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				int o2Count = 0;
				float o2Mass = 0f;
				int po2Count = 0;
				float po2Mass = 0f;

				for (int i = 0; i < cells.Length; i++) {
					int cell = cells[i];
					var element = Grid.Element[cell];
					if (element.id == SimHashes.Oxygen) {
						o2Count++;
						o2Mass += Grid.Mass[cell];
					} else if (element.id == SimHashes.ContaminatedOxygen) {
						po2Count++;
						po2Mass += Grid.Mass[cell];
					}
				}

				if (o2Count > 0) {
					int pct = (int)Math.Round(100.0 * o2Count / totalCells);
					if (pct == 0) pct = 1;
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.OXYGEN_ENTRY,
						pct, AreaScanUtil.FormatMass(o2Mass)));
				}
				if (po2Count > 0) {
					int pct = (int)Math.Round(100.0 * po2Count / totalCells);
					if (pct == 0) pct = 1;
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.POLLUTED_O2_ENTRY,
						pct, AreaScanUtil.FormatMass(po2Mass)));
				}

				if (o2Count == 0 && po2Count == 0)
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.OXYGEN_ENTRY,
						0, AreaScanUtil.FormatMass(0f)));

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"OxygenAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
