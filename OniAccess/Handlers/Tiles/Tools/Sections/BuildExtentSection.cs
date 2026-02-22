using System.Collections.Generic;
using OniAccess.Handlers.Build;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	/// <summary>
	/// Appends building extent directions for buildings larger than 1x1.
	/// E.g., "extends 1 left, 1 right, 1 up". Only changes on rotation,
	/// but always appended so experienced players hear it last and can
	/// interrupt before it plays.
	/// </summary>
	public class BuildExtentSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var orientation = BuildMenuData.GetCurrentOrientation();
			string extent = BuildToolHandler.BuildExtentText(orientation);
			if (extent != null)
				return new[] { extent };
			return System.Array.Empty<string>();
		}
	}
}
