using System.Collections.Generic;
using OniAccess.Handlers.Build;

namespace OniAccess.Handlers.Tiles.Tools.Sections {
	/// <summary>
	/// Placement feedback for the build tool cursor. Prepends placement
	/// errors when the current cell is invalid. For utility buildings
	/// with an active start point, appends the cell count and "invalid"
	/// if the proposed line contains bad cells.
	/// </summary>
	public class BuildToolSection : ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var handler = BuildToolHandler.Instance;
			if (handler == null || handler._def == null)
				return System.Array.Empty<string>();

			var tokens = new List<string>();
			var def = handler._def;

			if (!BuildMenuData.IsUtilityBuilding(def)) {
				ReadRegularPlacementErrors(cell, def, tokens);
			}

			if (handler.UtilityStartSet)
				ReadUtilityLineStatus(cell, handler, tokens);

			return tokens;
		}

		private static void ReadRegularPlacementErrors(
				int cell, BuildingDef def, List<string> tokens) {
			if (BuildTool.Instance == null || BuildTool.Instance.visualizer == null)
				return;

			var pos = Grid.CellToPosCBC(cell, def.SceneLayer);
			var orientation = BuildMenuData.GetCurrentOrientation();
			string failReason;
			if (!def.IsValidPlaceLocation(
					BuildTool.Instance.visualizer, pos, orientation, out failReason)) {
				tokens.Add(failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);
			}
		}

		private static void ReadUtilityLineStatus(
				int cell, BuildToolHandler handler, List<string> tokens) {
			int startCell = handler.UtilityStartCell;
			int startCol = Grid.CellColumn(startCell);
			int startRow = Grid.CellRow(startCell);
			int endCol = Grid.CellColumn(cell);
			int endRow = Grid.CellRow(cell);
			bool sameCol = startCol == endCol;
			bool sameRow = startRow == endRow;

			int count;
			if (!sameCol && !sameRow) {
				tokens.Insert(0, (string)STRINGS.ONIACCESS.BUILD_MENU.INVALID_LINE);
				int dx = System.Math.Abs(endCol - startCol);
				int dy = System.Math.Abs(endRow - startRow);
				count = System.Math.Max(dx, dy) + 1;
			} else {
				count = sameRow
					? System.Math.Abs(endCol - startCol) + 1
					: System.Math.Abs(endRow - startRow) + 1;
			}

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.BUILD_MENU.LINE_CELLS, count));
		}
	}
}
