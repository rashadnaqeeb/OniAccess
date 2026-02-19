using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Tiles.Sections {
	/// <summary>
	/// Reads all buildings at a cell across ObjectLayer.Building,
	/// ObjectLayer.FoundationTile, and ObjectLayer.Backwall.
	/// Plants also occupy ObjectLayer.Building.
	///
	/// For each building: utility ports (when overlay active), name,
	/// status items, construction state. Ports come first so the
	/// overlay-specific info is the first thing the player hears.
	/// Door access state comes through status items automatically.
	/// </summary>
	public class BuildingSection: ICellSection {
		public IEnumerable<string> Read(int cell, CellContext ctx) {
			var tokens = new List<string>();
			try {
				var buildingGo = Grid.Objects[cell, (int)ObjectLayer.Building];
				var foundationGo = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
				var backwallGo = Grid.Objects[cell, (int)ObjectLayer.Backwall];

				if (buildingGo != null && !ctx.Claimed.Contains(buildingGo))
					ReadBuilding(buildingGo, cell, tokens);

				if (foundationGo != null && foundationGo != buildingGo
					&& !ctx.Claimed.Contains(foundationGo))
					ReadBuilding(foundationGo, cell, tokens);

				if (backwallGo != null && !ctx.Claimed.Contains(backwallGo)) {
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

			var building = go.GetComponent<Building>();
			if (building != null)
				ReadPorts(go, building, cell, tokens);

			var constructable = go.GetComponent<Constructable>();
			if (constructable != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION,
					selectable.GetName()));
			} else {
				tokens.Add(selectable.GetName());
			}

			bool isPlant = go.GetComponent<Growing>() != null;
			ReadStatusItems(selectable, isPlant, tokens);

			if (building != null && building.PlacementCells.Length > 1) {
				int origin = Grid.PosToCell(building.transform.GetPosition());
				ReadCellOfInterest(go, building, origin, cell, tokens);
			}
		}

		private static void ReadStatusItems(
				KSelectable selectable, bool includeNeutral, List<string> tokens) {
			var group = selectable.GetStatusItemGroup();
			if (group == null) return;
			var enumerator = group.GetEnumerator();
			try {
				while (enumerator.MoveNext()) {
					var entry = enumerator.Current;
					var severity = entry.item.notificationType;
					if (severity != NotificationType.Bad
						&& severity != NotificationType.BadMinor
						&& !(includeNeutral && severity == NotificationType.Neutral))
						continue;
					string name = entry.GetName();
					if (!string.IsNullOrEmpty(name))
						tokens.Add(name);
				}
			} finally {
				enumerator.Dispose();
			}
		}

		private static void ReadPorts(
				GameObject go, Building building, int cell, List<string> tokens) {
			int origin = Grid.PosToCell(building.transform.GetPosition());
			ReadOverlayDetails(go, building, origin, cell, tokens);
			ReadAutomationPorts(building, origin, cell, tokens);
			ReadRadboltPorts(building, origin, cell, tokens);
		}

		private static void ReadCellOfInterest(
				GameObject go, Building building, int origin, int cell,
				List<string> tokens) {
			Vector3 originPos = building.transform.GetPosition();

			bool isAccess = (cell == origin);

			int outputCell = origin;
			var fabricator = go.GetComponent<ComplexFabricator>();
			if (fabricator != null && fabricator.outputOffset != Vector3.zero)
				outputCell = Grid.PosToCell(originPos + fabricator.outputOffset);

			var geyser = go.GetComponent<Geyser>();
			if (geyser != null && (geyser.outputOffset.x != 0 || geyser.outputOffset.y != 0))
				outputCell = Grid.OffsetCell(origin,
					new CellOffset(geyser.outputOffset.x, geyser.outputOffset.y));

			var storage = go.GetComponent<Storage>();
			if (storage != null && storage.dropOffset != Vector2.zero)
				outputCell = Grid.PosToCell(
					originPos + new Vector3(storage.dropOffset.x, storage.dropOffset.y, 0f));

			var dispenser = go.GetComponent<ObjectDispenser>();
			if (dispenser != null) {
				var rotatable = go.GetComponent<Rotatable>();
				CellOffset resolved = (rotatable != null)
					? rotatable.GetRotatedCellOffset(dispenser.dropOffset)
					: dispenser.dropOffset;
				outputCell = Grid.OffsetCell(origin, resolved);
			}

			bool isOutput = (cell == outputCell);

			if (!isAccess && !isOutput) return;

			if (isAccess && isOutput) {
				tokens.Add((string)STRINGS.ONIACCESS.GLANCE.TILE_OF_INTEREST);
			} else {
				if (isAccess)
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.ACCESS_POINT);
				if (isOutput)
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.OUTPUT_POINT);
			}
		}

		private static void ReadOverlayDetails(
				GameObject go, Building building, int origin, int cell,
				List<string> tokens) {
			if (OverlayScreen.Instance == null) return;

			var activeMode = OverlayScreen.Instance.GetMode();
			var def = building.Def;
			var orientation = building.Orientation;

			// Collect all input labels matching this cell
			var inputs = new List<string>();
			if (def.InputConduitType != ConduitType.None
				&& activeMode == ConduitTypeToOverlayMode(def.InputConduitType)) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.UtilityInputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					inputs.Add(ConduitInputLabel(def.InputConduitType));
			}
			foreach (var sec in go.GetComponents<ISecondaryInput>()) {
				var portInfo = (sec as ConduitSecondaryInput).portInfo;
				if (activeMode != ConduitTypeToOverlayMode(portInfo.conduitType))
					continue;
				var rotated = Rotatable.GetRotatedCellOffset(
					portInfo.offset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					inputs.Add(ConduitInputLabel(portInfo.conduitType));
			}

			// Collect all output labels matching this cell
			var outputs = new List<string>();
			if (def.OutputConduitType != ConduitType.None
				&& activeMode == ConduitTypeToOverlayMode(def.OutputConduitType)) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.UtilityOutputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					outputs.Add(ConduitOutputLabel(def.OutputConduitType));
			}
			foreach (var sec in go.GetComponents<ISecondaryOutput>()) {
				var portInfo = (sec as ConduitSecondaryOutput).portInfo;
				if (activeMode != ConduitTypeToOverlayMode(portInfo.conduitType))
					continue;
				var rotated = Rotatable.GetRotatedCellOffset(
					portInfo.offset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					outputs.Add(ConduitOutputLabel(portInfo.conduitType));
			}

			// Count total inputs/outputs across all cells to decide numbering.
			// Primary gives 1 port; each secondary component gives 1 more.
			int totalInputs = (def.InputConduitType != ConduitType.None
				&& activeMode == ConduitTypeToOverlayMode(def.InputConduitType) ? 1 : 0)
				+ CountSecondaryPorts<ISecondaryInput>(go, activeMode);
			int totalOutputs = (def.OutputConduitType != ConduitType.None
				&& activeMode == ConduitTypeToOverlayMode(def.OutputConduitType) ? 1 : 0)
				+ CountSecondaryPorts<ISecondaryOutput>(go, activeMode);

			AddNumberedLabels(inputs, totalInputs, tokens);
			AddNumberedLabels(outputs, totalOutputs, tokens);

			// Power ports (never have duplicates, no numbering needed)
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

			if (activeMode == OverlayModes.TileMode.ID) {
				var pe = go.GetComponent<PrimaryElement>();
				if (pe != null)
					tokens.Add(pe.Element.name);
			}
		}

		private static int CountSecondaryPorts<T>(
				GameObject go, HashedString activeMode) where T : class {
			int count = 0;
			foreach (var comp in go.GetComponents<T>()) {
				ConduitType type;
				if (comp is ISecondaryInput input)
					type = (input as ConduitSecondaryInput).portInfo.conduitType;
				else
					type = ((comp as ISecondaryOutput) as ConduitSecondaryOutput)
						.portInfo.conduitType;
				if (activeMode == ConduitTypeToOverlayMode(type))
					count++;
			}
			return count;
		}

		private static void AddNumberedLabels(
				List<string> labels, int totalOfKind, List<string> tokens) {
			if (labels.Count == 0) return;
			if (totalOfKind <= 1) {
				tokens.AddRange(labels);
			} else {
				for (int i = 0; i < labels.Count; i++)
					tokens.Add(string.Format(
						(string)STRINGS.ONIACCESS.GLANCE.NUMBERED_PORT,
						labels[i], i + 1));
			}
		}

		private static void ReadAutomationPorts(
				Building building, int origin, int cell, List<string> tokens) {
			if (OverlayScreen.Instance == null) return;
			if (OverlayScreen.Instance.GetMode() != OverlayModes.Logic.ID) return;

			var logicPorts = building.GetComponent<LogicPorts>();
			if (logicPorts == null) return;

			var orientation = building.Orientation;

			ReadAutomationPortArray(
				logicPorts.inputPortInfo, orientation, origin, cell, tokens);
			ReadAutomationPortArray(
				logicPorts.outputPortInfo, orientation, origin, cell, tokens);
		}

		private static void ReadAutomationPortArray(
				LogicPorts.Port[] ports, Orientation orientation,
				int origin, int cell, List<string> tokens) {
			if (ports == null) return;

			// Count how many ports share each description (across all cells)
			var descCounts = new Dictionary<string, int>();
			foreach (var port in ports) {
				string desc = port.description;
				if (descCounts.ContainsKey(desc))
					descCounts[desc]++;
				else
					descCounts[desc] = 1;
			}

			// Track per-description ordinal for numbering
			var descOrdinals = new Dictionary<string, int>();
			foreach (var port in ports) {
				string desc = port.description;
				if (!descOrdinals.ContainsKey(desc))
					descOrdinals[desc] = 1;
				else
					descOrdinals[desc]++;

				var rotated = Rotatable.GetRotatedCellOffset(
					port.cellOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell) {
					if (descCounts[desc] > 1)
						tokens.Add(string.Format(
							(string)STRINGS.ONIACCESS.GLANCE.NUMBERED_PORT,
							desc, descOrdinals[desc]));
					else
						tokens.Add(desc);
				}
			}
		}

		private static void ReadRadboltPorts(
				Building building, int origin, int cell, List<string> tokens) {
			if (OverlayScreen.Instance == null) return;
			if (OverlayScreen.Instance.GetMode() != OverlayModes.Radiation.ID) return;

			var def = building.Def;
			var orientation = building.Orientation;

			if (def.UseHighEnergyParticleInputPort) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.HighEnergyParticleInputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.RADBOLT_INPUT);
			}
			if (def.UseHighEnergyParticleOutputPort) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.HighEnergyParticleOutputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					tokens.Add((string)STRINGS.ONIACCESS.GLANCE.RADBOLT_OUTPUT);
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
