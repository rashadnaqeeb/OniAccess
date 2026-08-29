using System.Collections.Generic;
using OniAccess.Handlers.Build;

namespace OniAccess.Handlers.Tiles.ToolProfiles.Sections {
	/// <summary>
	/// Utility line feedback for the build tool cursor. When a utility
	/// start point is set, reports either "invalid" or the cell count.
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

			if (startCol != endCol && startRow != endRow)
				return new[] { (string)STRINGS.ONIACCESS.BUILD_MENU.INVALID_LINE };

			if (!BuildToolHandler.IsUtilityLineValid(startCell, cell))
				return new[] { (string)STRINGS.ONIACCESS.BUILD_MENU.INVALID_LINE };

			int count = startRow == endRow
				? System.Math.Abs(endCol - startCol) + 1
				: System.Math.Abs(endRow - startRow) + 1;
			return new[] { string.Format(
				(string)STRINGS.ONIACCESS.BUILD_MENU.LINE_CELLS, count) };
		}
	}

	/// <summary>
	/// Delegates to the conduit section of the active utility overlay, so
	/// the wires, pipes, rails, or automation wire the overlay shows stay
	/// in the readout while a building is being placed. Sensors, valves,
	/// pumps, and every powered machine switch the game to their overlay
	/// without living on a conduit layer, so the overlay decides. With no
	/// utility overlay on, falls back to the layer of the utility being
	/// placed. No-op for regular buildings outside the utility overlays.
	/// </summary>
	public class UtilityLayerSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var handler = BuildToolHandler.Instance;
			if (handler == null || handler._def == null)
				return System.Array.Empty<string>();

			HashedString overlay = OverlayScreen.Instance != null
				? OverlayScreen.Instance.GetMode()
				: OverlayModes.None.ID;
			var section = Resolve(overlay, handler._def.ObjectLayer);
			if (section == null)
				return System.Array.Empty<string>();

			return section.Read(cell, ctx);
		}

		/// <summary>
		/// The active utility overlay's conduit section, else the section
		/// for the layer of the utility being placed, else null.
		/// </summary>
		private static ICellSection Resolve(HashedString overlay, ObjectLayer placingLayer) {
			return MapOverlayToSection(overlay) ?? MapDefToSection(placingLayer);
		}

		private static ICellSection MapOverlayToSection(HashedString mode) {
			if (mode == OverlayModes.Power.ID) return GlanceComposer.Power;
			if (mode == OverlayModes.GasConduits.ID) return GlanceComposer.Ventilation;
			if (mode == OverlayModes.LiquidConduits.ID) return GlanceComposer.Plumbing;
			if (mode == OverlayModes.SolidConveyor.ID) return GlanceComposer.Conveyor;
			if (mode == OverlayModes.Logic.ID) return GlanceComposer.Automation;
			return null;
		}

		private static ICellSection MapDefToSection(ObjectLayer layer) {
			switch (layer) {
				case ObjectLayer.Wire:
				case ObjectLayer.WireConnectors:
					return GlanceComposer.Power;
				case ObjectLayer.GasConduit:
				case ObjectLayer.GasConduitConnection:
					return GlanceComposer.Ventilation;
				case ObjectLayer.LiquidConduit:
				case ObjectLayer.LiquidConduitConnection:
					return GlanceComposer.Plumbing;
				case ObjectLayer.SolidConduit:
				case ObjectLayer.SolidConduitConnection:
					return GlanceComposer.Conveyor;
				case ObjectLayer.LogicWire:
				case ObjectLayer.LogicGate:
					return GlanceComposer.Automation;
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

				return new[] { Widgets.PriorityWidget.FormatPriority(
					prioritizable.GetMasterPriority()) };
			}
			return System.Array.Empty<string>();
		}
	}
}
