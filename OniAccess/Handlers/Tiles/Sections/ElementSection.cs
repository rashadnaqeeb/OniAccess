using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks Grid.Element[cell].name with a glance-friendly mass.
	/// Suppressed when a foreground building (ObjectLayer.Building) or foundation
	/// tile (ObjectLayer.FoundationTile) is present. Still speaks when only a
	/// Backwall (drywall, tempshift plate) is present, since sighted players
	/// see the element through background buildings.
	/// </summary>
	public class ElementSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			if (Grid.Objects[cell, (int)ObjectLayer.Building] != null)
				return System.Array.Empty<string>();
			if (Grid.Objects[cell, (int)ObjectLayer.FoundationTile] != null)
				return System.Array.Empty<string>();
			var element = Grid.Element[cell];
			if (element == null) return System.Array.Empty<string>();
			float kg = Grid.Mass[cell];
			return new[] { $"{element.name}, {FormatGlanceMass(kg)}" };
		}

		/// <summary>
		/// Compact mass for glance speech. Grid.Mass is in kg.
		/// Under 0.1 kg: grams with one decimal ("52.3 g").
		/// 0.1 to 10 kg: kg with two decimals ("1.54 kg").
		/// Over 10 kg: whole kg ("688 kg").
		/// </summary>
		internal static string FormatGlanceMass(float kg) {
			if (kg < 0.1f) {
				float g = kg * 1000f;
				return $"{g:0} g";
			}
			if (kg <= 10f)
				return $"{kg:0.00} kg";
			return $"{kg:0} kg";
		}
	}
}
