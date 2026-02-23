using System.Collections.Generic;
using OniAccess.Handlers.Build;

namespace OniAccess.Handlers.Tiles.ToolProfiles.Sections {
	/// <summary>
	/// Utility line feedback for the build tool cursor. When a utility
	/// start point is set, reports the cell count and "invalid" if the
	/// proposed line contains bad cells.
	/// </summary>
	public class BuildToolSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var handler = BuildToolHandler.Instance;
			if (handler == null || !handler.UtilityStartSet)
				return System.Array.Empty<string>();

			return ReadUtilityLineStatus(cell, handler);
		}

		private static IEnumerable<string> ReadUtilityLineStatus(
				int cell, BuildToolHandler handler) {
			int startCell = handler.UtilityStartCell;
			int startCol = Grid.CellColumn(startCell);
			int startRow = Grid.CellRow(startCell);
			int endCol = Grid.CellColumn(cell);
			int endRow = Grid.CellRow(cell);
			bool sameCol = startCol == endCol;
			bool sameRow = startRow == endRow;

			int count;
			bool invalid = false;
			if (!sameCol && !sameRow) {
				invalid = true;
				int dx = System.Math.Abs(endCol - startCol);
				int dy = System.Math.Abs(endRow - startRow);
				count = System.Math.Max(dx, dy) + 1;
			} else {
				count = sameRow
					? System.Math.Abs(endCol - startCol) + 1
					: System.Math.Abs(endRow - startRow) + 1;
				if (!BuildToolHandler.IsUtilityLineValid(startCell, cell))
					invalid = true;
			}

			var tokens = new List<string>();
			if (invalid)
				tokens.Add((string)STRINGS.ONIACCESS.BUILD_MENU.INVALID_LINE);

			tokens.Add(string.Format(
				(string)STRINGS.ONIACCESS.BUILD_MENU.LINE_CELLS, count));
			return tokens;
		}
	}

	/// <summary>
	/// Delegates to the overlay section that matches the utility type
	/// being placed. When placing gas pipes reads existing gas pipes,
	/// when placing wire reads existing wires, etc. No-op for regular
	/// (non-utility) buildings.
	/// </summary>
	public class UtilityLayerSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var handler = BuildToolHandler.Instance;
			if (handler == null || handler._def == null)
				return System.Array.Empty<string>();

			var section = MapDefToSection(handler._def.ObjectLayer);
			if (section == null)
				return System.Array.Empty<string>();

			return section.Read(cell, ctx);
		}

		private static ICellSection MapDefToSection(ObjectLayer layer) {
			switch (layer) {
				case ObjectLayer.Wire: return GlanceComposer.Power;
				case ObjectLayer.GasConduit: return GlanceComposer.Ventilation;
				case ObjectLayer.LiquidConduit: return GlanceComposer.Plumbing;
				case ObjectLayer.SolidConduit: return GlanceComposer.Conveyor;
				case ObjectLayer.LogicWire: return GlanceComposer.Automation;
				default: return null;
			}
		}
	}

	/// <summary>
	/// Reads the construction priority of a pending build order at the
	/// cursor cell. Lets the player check what priority their queued
	/// buildings have while the build tool is active.
	/// </summary>
	public class BuildPrioritySection: ICellSection {
		private static readonly int[] _layers = {
			(int)ObjectLayer.Building,
			(int)ObjectLayer.FoundationTile,
			(int)ObjectLayer.Wire,
			(int)ObjectLayer.LiquidConduit,
			(int)ObjectLayer.GasConduit,
			(int)ObjectLayer.SolidConduit,
			(int)ObjectLayer.LogicWire,
		};

		public IEnumerable<string> Read(int cell, CellContext ctx) {
			foreach (int layer in _layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null) continue;

				var constructable = go.GetComponent<Constructable>();
				if (constructable == null) continue;

				var prioritizable = go.GetComponent<Prioritizable>();
				if (prioritizable == null) continue;

				var setting = prioritizable.GetMasterPriority();
				return new[] { string.Format(
					(string)STRINGS.ONIACCESS.TOOLS.PRIORITY_BASIC,
					setting.priority_value.ToString()) };
			}
			return System.Array.Empty<string>();
		}
	}
}
