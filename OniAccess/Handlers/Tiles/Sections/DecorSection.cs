using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks decor value at the cell with sign prefix. Always emits.
	/// </summary>
	public class DecorSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			float decor = GameUtil.GetDecorAtCell(cell);
			string sign = decor > 0f ? "+" : "";
			return new[] { string.Format(
				(string)STRINGS.ONIACCESS.GLANCE.OVERLAY_DECOR,
				sign, (int)decor) };
		}
	}
}
