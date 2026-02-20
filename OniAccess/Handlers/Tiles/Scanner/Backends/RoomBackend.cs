using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner.Backends {
	/// <summary>
	/// Backend for Zones > Rooms. Iterates Game.Instance.roomProber.rooms.
	/// Each room is one instance. Teleport target is the nearest cell
	/// in the room's cavity to the cursor.
	/// </summary>
	public class RoomBackend: IScannerBackend {

		public IEnumerable<ScanEntry> Scan(int worldId) {
			var rooms = Game.Instance.roomProber.rooms;
			for (int i = 0; i < rooms.Count; i++) {
				var room = rooms[i];
				if (room.cavity == null) continue;
				if (room.cavity.cells == null || room.cavity.cells.Count == 0) continue;

				int roomCell = room.cavity.cells[0];
				if (Grid.WorldIdx[roomCell] != (byte)worldId) continue;
				if (!Grid.IsVisible(roomCell)) continue;

				yield return new ScanEntry {
					Cell = roomCell,
					Backend = this,
					BackendData = room,
					Category = ScannerTaxonomy.Categories.Zones,
					Subcategory = ScannerTaxonomy.Subcategories.Rooms,
					ItemName = room.roomType.Name,
				};
			}
		}

		public bool ValidateEntry(ScanEntry entry, int cursorCell) {
			var room = (Room)entry.BackendData;
			if (!Game.Instance.roomProber.rooms.Contains(room))
				return false;

			int bestCell = room.cavity.cells[0];
			int bestDist = CellDistance(cursorCell, bestCell);
			var cells = room.cavity.cells;
			for (int i = 1; i < cells.Count; i++) {
				int dist = CellDistance(cursorCell, cells[i]);
				if (dist < bestDist) {
					bestDist = dist;
					bestCell = cells[i];
				}
			}
			entry.Cell = bestCell;
			return true;
		}

		public string FormatName(ScanEntry entry) {
			var room = (Room)entry.BackendData;
			return room.roomType.Name;
		}

		private static int CellDistance(int a, int b) {
			int dr = Grid.CellRow(a) - Grid.CellRow(b);
			int dc = Grid.CellColumn(a) - Grid.CellColumn(b);
			if (dr < 0) dr = -dr;
			if (dc < 0) dc = -dc;
			return dr + dc;
		}
	}
}
