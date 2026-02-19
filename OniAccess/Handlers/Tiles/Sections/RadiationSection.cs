using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks radiation level at the cell. Always emits.
	/// </summary>
	public class RadiationSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			float rads = Grid.Radiation[cell];
			return new[] { GameUtil.GetFormattedRads(rads) };
		}
	}
}
