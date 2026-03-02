namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Categorizes cells into Building, Liquid, Impassable, or Empty.
	/// Used for the default view and all unmapped overlays.
	/// </summary>
	public class DefaultSkipStrategy: ISkipStrategy {
		private enum Category { Building, Liquid, Impassable, Empty }

		public object GetSignature(int cell) {
			if (Grid.Objects[cell, (int)ObjectLayer.Building] != null)
				return Category.Building;
			Element element = Grid.Element[cell];
			if (element.IsLiquid)
				return Category.Liquid;
			if (Grid.Solid[cell]
				|| Grid.Objects[cell, (int)ObjectLayer.FoundationTile] != null)
				return Category.Impassable;
			return Category.Empty;
		}
	}
}
