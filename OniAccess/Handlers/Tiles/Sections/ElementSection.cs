using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Speaks Grid.Element[cell].name with a glance-friendly mass.
	/// Suppressed when a foundation tile is present (the tile IS the solid
	/// element, so announcing both is redundant), or when a foreground
	/// building is present and the element is gas. Exceptions:
	/// - Oxygen overlay: always speak the element.
	/// - Liquid on a building: submersion, gameplay-critical.
	/// - Solid on a building (not foundation tile): entombment, gameplay-critical.
	/// Still speaks when only a Backwall (drywall, tempshift plate) is present,
	/// since sighted players see the element through background buildings.
	/// </summary>
	public class ElementSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var element = Grid.Element[cell];
			if (OverlayScreen.Instance.GetMode() != OverlayModes.Oxygen.ID && !element.IsLiquid) {
				if (Grid.Objects[cell, (int)ObjectLayer.FoundationTile] != null)
					return System.Array.Empty<string>();
				if (!element.IsSolid && Grid.Objects[cell, (int)ObjectLayer.Building] != null)
					return System.Array.Empty<string>();
			}
			if (element == null) return System.Array.Empty<string>();
			if (element.IsVacuum)
				return new[] { element.name };
			float kg = Grid.Mass[cell];
			string text = $"{element.name}, {FormatGlanceMass(kg)}";
			if (Game.Instance.GetComponent<EntombedItemVisualizer>().IsEntombedItem(cell))
				text += ", " + (string)STRINGS.MISC.STATUSITEMS.BURIEDITEM.NAME;
			return new[] { text };
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
