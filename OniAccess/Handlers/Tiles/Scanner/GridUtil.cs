namespace OniAccess.Handlers.Tiles.Scanner {
	internal static class GridUtil {
		internal static int CellDistance(int a, int b) {
			int dr = Grid.CellRow(a) - Grid.CellRow(b);
			int dc = Grid.CellColumn(a) - Grid.CellColumn(b);
			if (dr < 0) dr = -dr;
			if (dc < 0) dc = -dc;
			return dr + dc;
		}
	}
}
