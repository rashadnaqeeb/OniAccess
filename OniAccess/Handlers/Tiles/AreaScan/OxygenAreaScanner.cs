using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class OxygenAreaScanner : IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				var o2Masses = new List<float>();
				var po2Masses = new List<float>();

				for (int i = 0; i < cells.Length; i++) {
					int cell = cells[i];
					var element = Grid.Element[cell];
					if (element.id == SimHashes.Oxygen)
						o2Masses.Add(Grid.Mass[cell]);
					else if (element.id == SimHashes.ContaminatedOxygen)
						po2Masses.Add(Grid.Mass[cell]);
				}

				if (o2Masses.Count > 0) {
					int pct = (int)Math.Round(100.0 * o2Masses.Count / totalCells);
					if (pct == 0) pct = 1;
					string name = ElementLoader.FindElementByHash(SimHashes.Oxygen).name;
					float median = AreaScanUtil.Median(o2Masses);
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.ELEMENT_MASS_PCT,
						name, pct, AreaScanUtil.FormatMass(median)));
				}
				if (po2Masses.Count > 0) {
					int pct = (int)Math.Round(100.0 * po2Masses.Count / totalCells);
					if (pct == 0) pct = 1;
					string name = ElementLoader.FindElementByHash(SimHashes.ContaminatedOxygen).name;
					float median = AreaScanUtil.Median(po2Masses);
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.ELEMENT_MASS_PCT,
						name, pct, AreaScanUtil.FormatMass(median)));
				}

				if (o2Masses.Count == 0 && po2Masses.Count == 0) {
					string name = ElementLoader.FindElementByHash(SimHashes.Oxygen).name;
					tokens.Add(string.Format(
						STRINGS.ONIACCESS.BIG_CURSOR.ELEMENT_MASS_PCT,
						name, 0, AreaScanUtil.FormatMass(0f)));
				}

				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"OxygenAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
