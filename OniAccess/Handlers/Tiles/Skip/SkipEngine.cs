namespace OniAccess.Handlers.Tiles.Skip {
	/// <summary>
	/// Walks cells in a direction until the active strategy's signature
	/// changes. Handles unexplored cells and world boundaries.
	/// Returns the speech string with tile count prepended.
	/// </summary>
	public class SkipEngine {
		private readonly SkipStrategyRegistry _registry;

		public SkipEngine(SkipStrategyRegistry registry) {
			_registry = registry;
		}

		/// <summary>
		/// Skip from the current cursor position in the given direction.
		/// Returns the speech string to be spoken.
		/// </summary>
		public string Skip(Direction direction) {
			try {
				return SkipCore(direction);
			} catch (System.Exception ex) {
				Util.Log.Error($"SkipEngine.Skip: {ex}");
				return (string)STRINGS.ONIACCESS.SKIP.NO_CHANGE_BOUNDARY;
			}
		}

		private string SkipCore(Direction direction) {
			var cursor = TileCursor.Instance;
			int startCell = cursor.Cell;

			HashedString mode = OverlayModes.None.ID;
			var overlayScreen = OverlayScreen.Instance;
			if (overlayScreen != null)
				mode = overlayScreen.GetMode();

			ISkipStrategy strategy = _registry.GetStrategy(mode);
			bool startedUnexplored = !Grid.IsVisible(startCell);
			object startSignature = strategy.GetSignature(startCell);

			int current = startCell;
			int steps = 0;
			while (true) {
				int next = TileCursor.GetNeighbor(current, direction);
				if (next == Grid.InvalidCell || !TileCursor.IsInWorldBounds(next))
					break;

				steps++;
				current = next;

				if (startedUnexplored && Grid.IsVisible(current)) {
					string cellSpeech = cursor.JumpTo(current);
					return FormatTileCount(steps) + ", " + cellSpeech;
				}

				if (!startedUnexplored && !Grid.IsVisible(current)) {
					string speech = cursor.JumpTo(current);
					return FormatTileCount(steps) + ", " + speech;
				}

				object sig = strategy.GetSignature(current);
				if (!object.Equals(startSignature, sig)) {
					string cellSpeech = cursor.JumpTo(current);
					return FormatTileCount(steps) + ", " + cellSpeech;
				}
			}

			return (string)STRINGS.ONIACCESS.SKIP.NO_CHANGE_BOUNDARY;
		}

		private static string FormatTileCount(int count) {
			if (count == 1)
				return string.Format(
					(string)STRINGS.ONIACCESS.SKIP.TILE_FORMAT, count);
			return string.Format(
				(string)STRINGS.ONIACCESS.SKIP.TILES_FORMAT, count);
		}
	}
}
