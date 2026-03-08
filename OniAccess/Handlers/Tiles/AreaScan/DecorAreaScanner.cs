using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	public class DecorAreaScanner: IAreaScanner {
		public string Scan(int[] cells, int totalCells, int unexploredCount) {
			try {
				var tokens = new List<string>();
				AreaScanUtil.AddUnexploredToken(tokens, totalCells, unexploredCount);

				if (cells.Length == 0) return string.Join(", ", tokens);

				float sum = 0f;
				for (int i = 0; i < cells.Length; i++) {
					float decor = GameUtil.GetDecorAtCell(cells[i]);
					sum += Math.Min(decor, DecorMonitor.MAXIMUM_DECOR_VALUE);
				}
				int avg = (int)(sum / cells.Length);
				string sign = avg > 0 ? "+" : "";

				tokens.Add(string.Format(
					STRINGS.ONIACCESS.BIG_CURSOR.AVG_DECOR,
					sign + avg));
				return string.Join(", ", tokens);
			} catch (Exception ex) {
				Util.Log.Error($"DecorAreaScanner.Scan: {ex}");
				return (string)STRINGS.ONIACCESS.BIG_CURSOR.SCAN_ERROR;
			}
		}
	}
}
