namespace OniAccess.Util {
	/// <summary>
	/// Formats a grid cell as coordinates relative to the Printing Pod (0,0).
	/// Falls back to world center if no telepad exists yet, but keeps
	/// retrying until a telepad is found so early callers (before
	/// Telepad.OnSpawn) don't lock in the wrong origin.
	/// </summary>
	internal static class GridCoordinates {
		private static int? _originCell;
		private static bool _telepadFound;

		internal static string Format(int cell) {
			int origin = GetOrResolveOrigin();
			int originX = Grid.CellColumn(origin);
			int originY = Grid.CellRow(origin);
			int x = Grid.CellColumn(cell) - originX;
			int y = Grid.CellRow(cell) - originY;
			return string.Format((string)STRINGS.ONIACCESS.TILE_CURSOR.COORDS, x, y);
		}

		/// <summary>
		/// Returns the origin cell (telepad or world center).
		/// Always re-resolves so TileCursor starts at the freshest position.
		/// </summary>
		internal static int GetOriginCell() {
			_originCell = null;
			_telepadFound = false;
			return GetOrResolveOrigin();
		}

		internal static void ClearOrigin() {
			_originCell = null;
			_telepadFound = false;
		}

		private static int GetOrResolveOrigin() {
			if (_originCell != null && _telepadFound)
				return _originCell.Value;
			var world = ClusterManager.Instance.activeWorld;
			int cell = FindTelepadCell(world);
			if (cell != Grid.InvalidCell) {
				_originCell = cell;
				_telepadFound = true;
				return cell;
			}
			if (_originCell != null)
				return _originCell.Value;
			cell = FindWorldCenter(world);
			_originCell = cell;
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
