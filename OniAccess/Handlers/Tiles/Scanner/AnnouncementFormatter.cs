namespace OniAccess.Handlers.Tiles.Scanner {
	/// <summary>
	/// Static methods for building scanner speech strings.
	/// Distance is exact tile offset from cursor, vertical first then horizontal.
	/// </summary>
	public static class AnnouncementFormatter {
		public static string FormatEntityInstance(
			string name, int cursorCell, int targetCell, int index, int count) {
			string distance = FormatDistance(cursorCell, targetCell);
			string position = string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.INSTANCE_OF, index, count);
			if (distance.Length > 0)
				return string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.INSTANCE_WITH_DISTANCE,
					name, distance, position);
			return string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.INSTANCE_NO_DISTANCE,
				name, position);
		}

		public static string FormatClusterInstance(
			int tileCount, string name, int cursorCell, int targetCell,
			int index, int count) {
			if (tileCount == 1)
				return FormatEntityInstance(name, cursorCell, targetCell, index, count);
			string label = string.Format(
				(string)STRINGS.ONIACCESS.SCANNER.CLUSTER_LABEL, tileCount, name);
			return FormatEntityInstance(label, cursorCell, targetCell, index, count);
		}

		public static string FormatOrderClusterInstance(
			int tileCount, string orderType, string targetName,
			int cursorCell, int targetCell, int index, int count) {
			string label;
			if (tileCount == 1)
				label = string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.ORDER_LABEL,
					orderType, targetName);
			else
				label = string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.ORDER_CLUSTER_LABEL,
					tileCount, orderType, targetName);
			return FormatEntityInstance(label, cursorCell, targetCell, index, count);
		}

		public static string FormatDistance(int cursorCell, int targetCell) {
			int cursorRow = Grid.CellRow(cursorCell);
			int cursorCol = Grid.CellColumn(cursorCell);
			int targetRow = Grid.CellRow(targetCell);
			int targetCol = Grid.CellColumn(targetCell);

			int dy = targetRow - cursorRow;
			int dx = targetCol - cursorCol;

			string vertical = null;
			string horizontal = null;

			if (dy > 0)
				vertical = string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.DISTANCE_VERTICAL,
					dy, (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_UP);
			else if (dy < 0)
				vertical = string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.DISTANCE_VERTICAL,
					-dy, (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN);

			if (dx > 0)
				horizontal = string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.DISTANCE_HORIZONTAL,
					dx, (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT);
			else if (dx < 0)
				horizontal = string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.DISTANCE_HORIZONTAL,
					-dx, (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT);

			if (vertical != null && horizontal != null)
				return string.Format(
					(string)STRINGS.ONIACCESS.SCANNER.DISTANCE_BOTH,
					vertical, horizontal);
			if (vertical != null)
				return vertical;
			if (horizontal != null)
				return horizontal;
			return "";
		}
	}
}
