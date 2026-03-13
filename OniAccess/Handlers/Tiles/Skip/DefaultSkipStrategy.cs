namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Distinguishes cells by what occupies them: each building type,
	/// each tile type, each liquid element, or natural solid.
	/// All gases are treated as one zone.
	/// Used for the default view and all unmapped overlays.
	/// </summary>
	public class DefaultSkipStrategy: ISkipStrategy {
		public object GetSignature(int cell) {
			var building = Grid.Objects[cell, (int)ObjectLayer.Building];
			if (building != null)
				return building.PrefabID();

			var tile = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
			if (tile != null)
				return tile.PrefabID();

			var element = Grid.Element[cell];
			if (element.IsGas)
				return new Tag("gas");

			return element.tag;
		}
	}
}
