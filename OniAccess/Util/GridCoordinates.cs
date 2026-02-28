namespace OniAccess.Util {
	/// <summary>
	/// Formats a grid cell as coordinates relative to the Printing Pod (0,0).
	/// Falls back to world center if no telepad exists.
	/// </summary>
	internal static class GridCoordinates {
		private static int? _originCell;

		internal static string Format(int cell) {
			if (_originCell == null)
				_originCell = ResolveOrigin();
			int originX = Grid.CellColumn(_originCell.Value);
			int originY = Grid.CellRow(_originCell.Value);
			int x = Grid.CellColumn(cell) - originX;
			int y = Grid.CellRow(cell) - originY;
			return string.Format((string)STRINGS.ONIACCESS.TILE_CURSOR.COORDS, x, y);
		}

		/// <summary>
		/// Returns the origin cell (telepad or world center), resolving lazily.
		/// Clears the cached origin first so it re-resolves for the current world.
		/// Used by TileCursor to set its starting position.
		/// </summary>
		internal static int GetOriginCell() {
			_originCell = null;
			_originCell = ResolveOrigin();
			return _originCell.Value;
		}

		internal static void ClearOrigin() {
			_originCell = null;
		}

		private static int ResolveOrigin() {
			var world = ClusterManager.Instance.activeWorld;
			int cell = FindTelepadCell(world);
			if (cell == Grid.InvalidCell)
				cell = FindWorldCenter(world);
			return cell;
		}

		private static int FindTelepadCell(WorldContainer world) {
			try {
				var telepads = Components.Telepads.GetWorldItems(world.id);
				if (telepads != null && telepads.Count > 0)
					return Grid.PosToCell(telepads[0].transform.GetPosition());
			} catch (System.Exception ex) {
				Log.Warn($"GridCoordinates.FindTelepadCell: {ex.Message}");
			}
			return Grid.InvalidCell;
		}

		private static int FindWorldCenter(WorldContainer world) {
			int x = (int)((world.minimumBounds.x + world.maximumBounds.x) / 2f);
			int y = (int)((world.minimumBounds.y + world.maximumBounds.y) / 2f);
			int cell = Grid.XYToCell(x, y);
			if (Grid.IsValidCell(cell))
				return cell;
			Log.Warn("GridCoordinates.FindWorldCenter: center cell invalid, using cell 0");
			return 0;
		}
	}
}
