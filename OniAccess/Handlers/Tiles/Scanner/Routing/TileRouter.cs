namespace OniAccess.Handlers.Tiles.Scanner.Routing {
	/// <summary>
	/// Routes KAnimTile buildings (plain construction tiles) to their
	/// scanner category. All KAnimTiles go to Solids > Tiles.
	/// Functional tile buildings (doors, farms, ladders, etc.) are
	/// handled by EntityBackend via BuildingRouter instead.
	/// </summary>
	public static class TileRouter {
		public static (string category, string subcategory) Route(string prefabId) {
			return (ScannerTaxonomy.Categories.Solids, ScannerTaxonomy.Subcategories.Tiles);
		}
	}
}
