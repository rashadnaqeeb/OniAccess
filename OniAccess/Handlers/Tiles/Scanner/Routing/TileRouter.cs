using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Routes constructed tile buildings by prefab ID to their scanner
	/// category/subcategory. Most tiles go to Solids > Tiles; a few
	/// exceptions route to Buildings subcategories.
	/// </summary>
	public static class TileRouter {
		private static readonly Dictionary<string, (string category, string subcategory)> _overrides =
			new Dictionary<string, (string, string)> {
				{ "FarmTile", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Farming) },
				{ "Ladder", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ "FirePole", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ "LadderFast", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Infrastructure) },
				{ "StorageTile", (ScannerTaxonomy.Categories.Buildings, ScannerTaxonomy.Subcategories.Storage) },
			};

		public static (string category, string subcategory) Route(string prefabId) {
			if (_overrides.TryGetValue(prefabId, out var dest))
				return dest;
			return (ScannerTaxonomy.Categories.Solids, ScannerTaxonomy.Subcategories.Tiles);
		}
	}
}
