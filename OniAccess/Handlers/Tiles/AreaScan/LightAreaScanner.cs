using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class LightAreaScanner: IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				long sum = 0;
				for (int i = 0; i < cells.Length; i++)
					sum += Grid.LightIntensity[cells[i]];
				int avg = (int)(sum / cells.Length);

				tokens.Add(string.Format(
					STRINGS.ONIACCESS.BIG_CURSOR.AVG_LUX,
					GameUtil.GetFormattedLux(avg)));
				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"LightAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
