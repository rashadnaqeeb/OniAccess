using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks lux value at the cell. Always emits (0 lux is useful info).
	/// </summary>
	public class LightSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			int lux = Grid.LightIntensity[cell];
			return new[] { GameUtil.GetFormattedLux(lux) };
		}
	}
}
