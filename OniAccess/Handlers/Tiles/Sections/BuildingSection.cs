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
						tokens.Add(GetBuildingName(backwallGo, selectable));
				}

				ReadPortCell(cell, buildingGo, foundationGo, tokens);
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
			string displayName = GetBuildingName(go, selectable);
			if (constructable != null) {
				tokens.Add(string.Format(
					(string)STRINGS.ONIACCESS.GLANCE.UNDER_CONSTRUCTION,
					displayName));
			} else {
				tokens.Add(displayName);
			}

			bool isPlant = go.GetComponent<Growing>() != null;
			ReadStatusItems(selectable, isPlant, tokens);

			if (building != null && building.PlacementCells.Length > 1) {
				int origin = Grid.PosToCell(building.transform.GetPosition());
				ReadCellOfInterest(go, building, origin, cell, tokens);
			}
		}

		/// <summary>
		/// When the cursor is on a port cell that's outside the building's
		/// footprint, the building won't be found on ObjectLayer.Building.
		///
		/// For power and conduit overlays the game registers the building
		/// on port-specific object layers, so a direct lookup works.
		///
		/// For automation and radbolt overlays there is no port layer.
		/// We scan nearby cells on the Building and FoundationTile layers
		/// to find buildings whose ports resolve to the cursor cell.
		/// </summary>
		private static void ReadPortCell(
				int cell, GameObject buildingGo, GameObject foundationGo,
				List<string> tokens) {
			if (OverlayScreen.Instance == null) return;
			var activeMode = OverlayScreen.Instance.GetMode();

			if (activeMode == OverlayModes.Logic.ID
				|| activeMode == OverlayModes.Radiation.ID) {
				ScanNearbyForPorts(cell, buildingGo, foundationGo,
					activeMode, tokens);
				return;
			}

			ObjectLayer portLayer;
			if (activeMode == OverlayModes.Power.ID)
				portLayer = ObjectLayer.WireConnectors;
			else if (activeMode == OverlayModes.LiquidConduits.ID)
				portLayer = ObjectLayer.LiquidConduitConnection;
			else if (activeMode == OverlayModes.GasConduits.ID)
				portLayer = ObjectLayer.GasConduitConnection;
			else if (activeMode == OverlayModes.SolidConveyor.ID)
				portLayer = ObjectLayer.SolidConduitConnection;
			else
				return;

			var portGo = Grid.Objects[cell, (int)portLayer];
			if (portGo == null) return;

			// Already processed through building or foundation layers
			if (portGo == buildingGo || portGo == foundationGo) return;

			var building = portGo.GetComponent<Building>();
			if (building == null) return;

			int origin = Grid.PosToCell(building.transform.GetPosition());
			int beforeCount = tokens.Count;
			ReadOverlayDetails(portGo, building, origin, cell, tokens);

			if (tokens.Count > beforeCount) {
				var selectable = portGo.GetComponent<KSelectable>();
				if (selectable != null)
					tokens.Add(GetBuildingName(portGo, selectable));
			}
		}

		private const int PortScanRadius = 5;

		private static void ScanNearbyForPorts(
				int cell, GameObject buildingGo, GameObject foundationGo,
				HashedString activeMode, List<string> tokens) {
			int cx = Grid.CellColumn(cell);
			int cy = Grid.CellRow(cell);
			var seen = new HashSet<GameObject>();
			if (buildingGo != null) seen.Add(buildingGo);
			if (foundationGo != null) seen.Add(foundationGo);

			for (int dy = -PortScanRadius; dy <= PortScanRadius; dy++) {
				for (int dx = -PortScanRadius; dx <= PortScanRadius; dx++) {
					int nc = Grid.XYToCell(cx + dx, cy + dy);
					if (!Grid.IsValidCell(nc)) continue;

					CheckScanCell(nc, (int)ObjectLayer.Building,
						seen, cell, activeMode, tokens);
					CheckScanCell(nc, (int)ObjectLayer.FoundationTile,
						seen, cell, activeMode, tokens);
				}
			}
		}

		private static void CheckScanCell(
				int nearbyCell, int layer, HashSet<GameObject> seen,
				int targetCell, HashedString activeMode,
				List<string> tokens) {
			var go = Grid.Objects[nearbyCell, layer];
			if (go == null || !seen.Add(go)) return;

			var building = go.GetComponent<Building>();
			if (building == null) return;

			int origin = Grid.PosToCell(building.transform.GetPosition());
			int beforeCount = tokens.Count;

			if (activeMode == OverlayModes.Logic.ID)
				ReadAutomationPorts(building, origin, targetCell, tokens);
			else
				ReadRadboltPorts(building, origin, targetCell, tokens);

			if (tokens.Count > beforeCount) {
				var selectable = go.GetComponent<KSelectable>();
				if (selectable != null)
					tokens.Add(GetBuildingName(go, selectable));
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
			var conduitType = OverlayModeToConduitType(activeMode);

			// Collect all input labels matching this cell
			var inputs = new List<string>();
			if (def.InputConduitType != ConduitType.None
				&& def.InputConduitType == conduitType) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.UtilityInputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					inputs.Add(ConduitInputLabel(conduitType));
			}
			if (conduitType != ConduitType.None) {
				foreach (var sec in go.GetComponents<ISecondaryInput>()) {
					if (!sec.HasSecondaryConduitType(conduitType))
						continue;
					var offset = sec.GetSecondaryConduitOffset(conduitType);
					var rotated = Rotatable.GetRotatedCellOffset(offset, orientation);
					if (Grid.OffsetCell(origin, rotated) == cell)
						inputs.Add(ConduitInputLabel(conduitType));
				}
			}

			// Collect all output labels matching this cell
			var outputs = new List<string>();
			if (def.OutputConduitType != ConduitType.None
				&& def.OutputConduitType == conduitType) {
				var rotated = Rotatable.GetRotatedCellOffset(
					def.UtilityOutputOffset, orientation);
				if (Grid.OffsetCell(origin, rotated) == cell)
					outputs.Add(ConduitOutputLabel(conduitType));
			}
			if (conduitType != ConduitType.None) {
				foreach (var sec in go.GetComponents<ISecondaryOutput>()) {
					if (!sec.HasSecondaryConduitType(conduitType))
						continue;
					var offset = sec.GetSecondaryConduitOffset(conduitType);
					var rotated = Rotatable.GetRotatedCellOffset(offset, orientation);
					if (Grid.OffsetCell(origin, rotated) == cell)
						outputs.Add(ConduitOutputLabel(conduitType));
				}
			}

			// Count total inputs/outputs across all cells to decide numbering.
			// Primary gives 1 port; each secondary component gives 1 more.
			int totalInputs = (def.InputConduitType == conduitType ? 1 : 0)
				+ CountSecondaryPorts<ISecondaryInput>(go, conduitType);
			int totalOutputs = (def.OutputConduitType == conduitType ? 1 : 0)
				+ CountSecondaryPorts<ISecondaryOutput>(go, conduitType);

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
				GameObject go, ConduitType conduitType) where T : class {
			if (conduitType == ConduitType.None) return 0;
			int count = 0;
			foreach (var comp in go.GetComponents<T>()) {
				if (comp is ISecondaryInput input
					&& input.HasSecondaryConduitType(conduitType))
					count++;
				else if (comp is ISecondaryOutput output
					&& output.HasSecondaryConduitType(conduitType))
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

		private static ConduitType OverlayModeToConduitType(HashedString mode) {
			if (mode == OverlayModes.GasConduits.ID) return ConduitType.Gas;
			if (mode == OverlayModes.LiquidConduits.ID) return ConduitType.Liquid;
			if (mode == OverlayModes.SolidConveyor.ID) return ConduitType.Solid;
			return ConduitType.None;
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

		private static bool IsDecorOverlay() {
			return OverlayScreen.Instance != null
				&& OverlayScreen.Instance.GetMode() == OverlayModes.Decor.ID;
		}

		private static string GetBuildingName(GameObject go, KSelectable selectable) {
			if (IsDecorOverlay())
				return selectable.GetName();
			var facade = go.GetComponent<BuildingFacade>();
			if (facade != null && !facade.IsOriginal) {
				var building = go.GetComponent<Building>();
				if (building != null)
					return building.Def.Name;
			}
			return selectable.GetName();
		}
	}
}
