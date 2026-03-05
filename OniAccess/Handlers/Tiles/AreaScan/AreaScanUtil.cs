using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.AreaScan {
	/// <summary>
	/// Shared helpers for area scanner implementations.
	/// </summary>
	internal static class AreaScanUtil {
		/// <summary>
		/// Prepends an "X% unexplored" token if any cells are unexplored.
		/// </summary>
		internal static void AddUnexploredToken(List<string> tokens,
				int totalCells, int unexploredCount) {
			if (unexploredCount <= 0) return;
			int pct = (int)Math.Round(100.0 * unexploredCount / totalCells);
			if (pct == 0) pct = 1;
			tokens.Add(string.Format(
				STRINGS.ONIACCESS.BIG_CURSOR.UNEXPLORED_PCT, pct));
		}

		internal static string FormatMass(float kg) {
			if (kg < 1f)
				return $"{kg * 1000f:0} g";
			if (kg < 1000f)
				return $"{kg:0} kg";
			return $"{kg / 1000f:0.#} t";
		}
	}
}
