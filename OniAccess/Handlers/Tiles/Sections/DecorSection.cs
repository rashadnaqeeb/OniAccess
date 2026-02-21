using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks decor value at the cell with sign prefix. Always emits.
	/// Clamps to the game's maximum decor value (DecorMonitor.MAXIMUM_DECOR_VALUE)
	/// to match what actually affects duplicants.
	/// </summary>
	public class DecorSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			float decor = GameUtil.GetDecorAtCell(cell);
			decor = Math.Min(decor, DecorMonitor.MAXIMUM_DECOR_VALUE);
			string sign = decor > 0f ? "+" : "";
			return new[] { string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.OVERLAY_DECOR,
				sign, (int)decor) };
		}
	}
}
