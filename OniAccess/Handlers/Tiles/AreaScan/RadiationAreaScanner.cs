using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class RadiationAreaScanner: IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				float sum = 0f;
				for (int i = 0; i < cells.Length; i++)
					sum += Grid.Radiation[cells[i]];
				float avg = sum / cells.Length;

				tokens.Add(string.Format(
					STRINGS.ONIACCESS.BIG_CURSOR.AVG_RADS,
					GameUtil.GetFormattedRads(avg)));
				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"RadiationAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
