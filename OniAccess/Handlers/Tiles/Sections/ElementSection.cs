using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks Grid.Element[cell].name.
	/// Suppressed when a foreground building (ObjectLayer.Building) or foundation
	/// tile (ObjectLayer.FoundationTile) is present. Still speaks when only a
	/// Backwall (drywall, tempshift plate) is present, since sighted players
	/// see the element through background buildings.
	/// </summary>
	public class ElementSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			if (Grid.Objects[cell, (int)ObjectLayer.Building] != null)
				return System.Array.Empty<string>();
			if (Grid.Objects[cell, (int)ObjectLayer.FoundationTile] != null)
				return System.Array.Empty<string>();
			var element = Grid.Element[cell];
			if (element == null) return System.Array.Empty<string>();
			float mass = Grid.Mass[cell];
			string formatted = GameUtil.GetFormattedMass(mass);
			return new[] { $"{element.name}, {formatted}" };
		}
	}
}
