using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads all buildings at a cell across ObjectLayer.Building,
	/// ObjectLayer.FoundationTile, and ObjectLayer.Backwall.
	///
	/// For each building: name, all status items, construction state.
	/// Door access state comes through status items automatically.
	/// Multi-tile buildings annotate utility ports (only when the
	/// matching overlay is active).
	/// </summary>
	public class BuildingSection : ICellSection {
		public IEnumerable<string> Read(int cell) {
			var tokens = new List<string>();
			try {
				var buildingGo = Grid.Objects[cell, (int)ObjectLayer.Building];
				var foundationGo = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
				var backwallGo = Grid.Objects[cell, (int)ObjectLayer.Backwall];

				if (buildingGo != null)
					ReadBuilding(buildingGo, cell, tokens);

				if (foundationGo != null && foundationGo != buildingGo)
					ReadBuilding(foundationGo, cell, tokens);

				if (backwallGo != null) {
					var selectable = backwallGo.GetComponent<KSelectable>();
					if (selectable != null)
						tokens.Add(selectable.GetName());
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"BuildingSection.Read: {ex}");
			}
			return tokens;
		}

		private static void ReadBuilding(GameObject go, int cell, List<string> tokens) {
			var selectable = go.GetComponent<KSelectable>();
			if (selectable == null) return;

			var constructable = go.GetComponent<Constructable>();
			if (constructable != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION,
					selectable.GetName()));
			} else {
				tokens.Add(selectable.GetName());
			}

			ReadStatusItems(selectable, tokens);
			ReadMultiTileAnnotations(go, cell, tokens);
		}

		private static void ReadStatusItems(KSelectable selectable, List<string> tokens) {
			var group = selectable.GetStatusItemGroup();
			if (group == null) return;
			var enumerator = group.GetEnumerator();
			try {
				while (enumerator.MoveNext()) {
					string name = enumerator.Current.GetName();
					if (!string.IsNullOrEmpty(name))
						tokens.Add(name);
				}
			} finally {
				enumerator.Dispose();
			}
		}

		private static void ReadMultiTileAnnotations(
				GameObject go, int cell, List<string> tokens) {
			var building = go.GetComponent<Building>();
			if (building == null) return;

			int[] cells = building.PlacementCells;
			if (cells.Length <= 1) return;

			int origin = Grid.PosToCell(building.transform.GetPosition());

			ReadUtilityPorts(building, origin, cell, tokens);
			ReadAutomationPorts(building, origin, cell, tokens);
		}

		private static void ReadUtilityPorts(
				Building building, int origin, int cell, List<string> tokens) {
			if (OverlayScreen.Instance == null) return;

			var activeMode = OverlayScreen.Instance.GetMode();
			var def = building.Def;
			var orientation = building.Orientation;

			if (def.InputConduitType != ConduitType.None
				&& activeMode == ConduitTypeToOverlayMode(def.InputConduitType)) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.UtilityInputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					tokens.Add(ConduitInputLabel(def.InputConduitType));
			}

			if (def.OutputConduitType != ConduitType.None
				&& activeMode == ConduitTypeToOverlayMode(def.OutputConduitType)) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.UtilityOutputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					tokens.Add(ConduitOutputLabel(def.OutputConduitType));
			}

			if (activeMode == OverlayModes.Power.ID) {
				if (def.RequiresPowerInput) {
					var rotated = Rotatable.GetRotatedCellOffset(
						def.PowerInputOffset, orientation);
					if (Grid.OffsetCell(origin, rotated) == cell)
						tokens.Add((string)STRINGS.ONIACCESS.GLANCE.POWER_INPUT);
				}
				if (def.RequiresPowerOutput) {
					var rotated = Rotatable.GetRotatedCellOffset(
						def.PowerOutputOffset, orientation);
					if (Grid.OffsetCell(origin, rotated) == cell)
						tokens.Add((string)STRINGS.ONIACCESS.GLANCE.POWER_OUTPUT);
				}
			}
		}

		private static void ReadAutomationPorts(
				Building building, int origin, int cell, List<string> tokens) {
			if (OverlayScreen.Instance == null) return;
			if (OverlayScreen.Instance.GetMode() != OverlayModes.Logic.ID) return;

			var logicPorts = building.GetComponent<LogicPorts>();
			if (logicPorts == null) return;

			var orientation = building.Orientation;

			if (logicPorts.inputPortInfo != null) {
				foreach (var port in logicPorts.inputPortInfo) {
					var rotated = Rotatable.GetRotatedCellOffset(
						port.cellOffset, orientation);
					if (Grid.OffsetCell(origin, rotated) == cell)
						tokens.Add(port.description);
				}
			}
			if (logicPorts.outputPortInfo != null) {
				foreach (var port in logicPorts.outputPortInfo) {
					var rotated = Rotatable.GetRotatedCellOffset(
						port.cellOffset, orientation);
					if (Grid.OffsetCell(origin, rotated) == cell)
						tokens.Add(port.description);
				}
			}
		}

		private static HashedString ConduitTypeToOverlayMode(ConduitType type) {
			switch (type) {
				case ConduitType.Gas: return OverlayModes.GasConduits.ID;
				case ConduitType.Liquid: return OverlayModes.LiquidConduits.ID;
				case ConduitType.Solid: return OverlayModes.SolidConveyor.ID;
				default: return OverlayModes.None.ID;
			}
		}

		private static string ConduitInputLabel(ConduitType type) {
			switch (type) {
				case ConduitType.Gas: return (string)STRINGS.ONIACCESS.GLANCE.GAS_INPUT;
				case ConduitType.Liquid: return (string)STRINGS.ONIACCESS.GLANCE.LIQUID_INPUT;
				case ConduitType.Solid: return (string)STRINGS.ONIACCESS.GLANCE.SOLID_INPUT;
				default: return (string)STRINGS.ONIACCESS.GLANCE.INPUT_PORT;
			}
		}

		private static string ConduitOutputLabel(ConduitType type) {
			switch (type) {
				case ConduitType.Gas: return (string)STRINGS.ONIACCESS.GLANCE.GAS_OUTPUT;
				case ConduitType.Liquid: return (string)STRINGS.ONIACCESS.GLANCE.LIQUID_OUTPUT;
				case ConduitType.Solid: return (string)STRINGS.ONIACCESS.GLANCE.SOLID_OUTPUT;
				default: return (string)STRINGS.ONIACCESS.GLANCE.OUTPUT_PORT;
			}
		}
	}
}
