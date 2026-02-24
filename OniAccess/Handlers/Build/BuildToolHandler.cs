using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OniAccess.Handlers.Tiles;
using OniAccess.Handlers.Tiles.ToolProfiles;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Non-modal handler for build tool placement. Sits on top of
	/// TileCursorHandler, intercepts build-specific keys (Space, R, Tab,
	/// I, 0-9, Shift+Space) and passes arrows through to the tile cursor.
	///
	/// Handles both regular buildings (single-cell placement via BuildTool)
	/// and utility buildings (straight-line path via UtilityBuildTool/WireBuildTool).
	/// </summary>
	public class BuildToolHandler: BaseScreenHandler {
		public static BuildToolHandler Instance { get; private set; }

		private HashedString _category;
		internal BuildingDef _def;
		private bool _isUtility;

		internal bool SuppressToolEvents { get; set; }

		// Utility placement state
		private int _utilityStartCell = Grid.InvalidCell;
		internal bool UtilityStartSet => _utilityStartCell != Grid.InvalidCell;
		internal int UtilityStartCell => _utilityStartCell;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
			new ConsumedKey(KKeyCode.Space, Modifier.Shift),
			new ConsumedKey(KKeyCode.Return),
			new ConsumedKey(KKeyCode.R),
			new ConsumedKey(KKeyCode.Tab),
			new ConsumedKey(KKeyCode.I),
			new ConsumedKey(KKeyCode.Alpha0),
			new ConsumedKey(KKeyCode.Alpha1),
			new ConsumedKey(KKeyCode.Alpha2),
			new ConsumedKey(KKeyCode.Alpha3),
			new ConsumedKey(KKeyCode.Alpha4),
			new ConsumedKey(KKeyCode.Alpha5),
			new ConsumedKey(KKeyCode.Alpha6),
			new ConsumedKey(KKeyCode.Alpha7),
			new ConsumedKey(KKeyCode.Alpha8),
			new ConsumedKey(KKeyCode.Alpha9),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		public override string DisplayName => BuildMenuData.BuildNameAnnouncement(_def);
		public override bool CapturesAllInput => false;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Space", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_PLACE),
			new HelpEntry("Enter", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_PLACE_AND_EXIT),
			new HelpEntry("R", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_ROTATE),
			new HelpEntry("Tab", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_BUILDING_LIST),
			new HelpEntry("I", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_INFO),
			new HelpEntry("0-9", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.SET_PRIORITY),
			new HelpEntry("Shift+Space", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_CANCEL_CONSTRUCTION),
			new HelpEntry("Escape", (string)STRINGS.ONIACCESS.HELP.CLOSE),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public BuildToolHandler(HashedString category, BuildingDef def) {
			_category = category;
			_def = def;
			_isUtility = BuildMenuData.IsUtilityBuilding(def);
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			Instance = this;

			if (Game.Instance != null) {
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
				Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
			}
		}

		/// <summary>
		/// Called by BuildMenuHandler after SelectBuilding returns.
		/// At this point the active tool is known (PrebuildTool or BuildTool).
		/// </summary>
		internal void AnnounceInitialState() {
			string announcement = BuildMenuData.BuildNameAnnouncement(_def);
			if (IsInPrebuildMode()) {
				string error = GetPrebuildError();
				if (!string.IsNullOrEmpty(error))
					announcement += ", " + error;
				SpeechPipeline.SpeakInterrupt(announcement);
			} else {
				SetupBuildMode();
				SpeechPipeline.SpeakInterrupt(announcement);
				SpeechPipeline.SpeakQueued(BuildMenuData.GetMaterialSummary(_def));
			}
		}

		private void SetupBuildMode() {
			if (TileCursor.Instance != null) {
				var composer = ToolProfileRegistry.Instance.GetComposer(
					_isUtility ? GetUtilityToolType() : typeof(BuildTool));
				TileCursor.Instance.ActiveToolComposer = composer;
			}
		}

		public override void OnDeactivate() {
			Instance = null;

			if (TileCursor.Instance != null)
				TileCursor.Instance.ActiveToolComposer = null;

			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);

			_utilityStartCell = Grid.InvalidCell;
		}

		private void OnActiveToolChanged(object data) {
			if (SuppressToolEvents) return;

			if (data is SelectTool) {
				if (_def != null && _def.OnePerWorld)
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.PLACED);
				else
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.CANCELED);
				PlayDeactivateSound();
				QueueOverlayAndPop();
				return;
			}

			if (data is BuildTool || data is UtilityBuildTool || data is WireBuildTool) {
				SetupBuildMode();
				SpeechPipeline.SpeakQueued(BuildMenuData.GetMaterialSummary(_def));
				return;
			}

			if (data is PrebuildTool) {
				if (TileCursor.Instance != null)
					TileCursor.Instance.ActiveToolComposer = null;
				string error = GetPrebuildError();
				if (!string.IsNullOrEmpty(error))
					SpeechPipeline.SpeakInterrupt(error);
			}
		}

		private bool IsInPrebuildMode() =>
			PlayerController.Instance.ActiveTool is PrebuildTool;

		private string GetPrebuildError() {
			var card = PrebuildTool.Instance.GetComponent<PrebuildToolHoverTextCard>();
			return card.errorMessage;
		}

		// ========================================
		// KEY HANDLING
		// ========================================

		public override bool Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)) {
				if (InputUtil.ShiftHeld()) {
					if (_isUtility && UtilityStartSet) {
						_utilityStartCell = Grid.InvalidCell;
						SpeechPipeline.SpeakInterrupt(
							(string)STRINGS.ONIACCESS.BUILD_MENU.START_CLEARED);
					} else
						QuickCancel();
				} else if (!InputUtil.AnyModifierHeld()) {
					if (IsInPrebuildMode()) {
						PlayNegativeSound();
						string error = GetPrebuildError();
						SpeechPipeline.SpeakInterrupt(
							error ?? (string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
					} else if (_isUtility)
						UtilityPlacement();
					else
						RegularPlacement();
				}
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)
				&& !InputUtil.AnyModifierHeld()) {
				if (IsInPrebuildMode()) {
					PlayNegativeSound();
					string error = GetPrebuildError();
					SpeechPipeline.SpeakInterrupt(
						error ?? (string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
				} else if (_isUtility)
					UtilityPlaceAndExit();
				else
					RegularPlaceAndExit();
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R)
				&& !InputUtil.AnyModifierHeld()) {
				if (IsInPrebuildMode()) {
					PlayNegativeSound();
					string error = GetPrebuildError();
					SpeechPipeline.SpeakInterrupt(
						error ?? (string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
				} else {
					Rotate();
				}
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)
				&& !InputUtil.AnyModifierHeld()) {
				ReturnToBuildingList();
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)
				&& !InputUtil.AnyModifierHeld()) {
				OpenInfoPanel();
				return true;
			}

			if (!InputUtil.AnyModifierHeld()) {
				for (int i = 0; i <= 9; i++) {
					if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha0 + i)) {
						SetPriority(i);
						return true;
					}
				}
			}
			return false;
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				CloseEverything();
				return true;
			}
			if (e.TryConsume(Action.RotateBuilding))
				return true;
			for (int i = (int)Action.Plan1; i <= (int)Action.Plan14; i++) {
				if (e.TryConsume((Action)i))
					return true;
			}
			return false;
		}

		// ========================================
		// REGULAR PLACEMENT
		// ========================================

		private void RegularPlacement() {
			int cell = TileCursor.Instance.Cell;
			if (!Grid.IsVisible(cell)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}
			var pos = Grid.CellToPosCBC(cell, _def.SceneLayer);
			var orientation = BuildMenuData.GetCurrentOrientation();
			string failReason;
			if (!_def.IsValidPlaceLocation(BuildTool.Instance.visualizer, pos, orientation, out failReason)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);
				return;
			}

			bool hasMaterials = HasSufficientMaterials();
			BuildTool.Instance.OnLeftClickDown(pos);
			BuildTool.Instance.OnLeftClickUp(pos);
			// OnePerWorld buildings auto-dismiss the tool, triggering
			// OnActiveToolChanged which announces "placed" and pops.
			if (!_def.OnePerWorld) {
				if (!hasMaterials)
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.PLACED_NO_MATERIAL);
				else
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.PLACED);
			}
		}

		private void RegularPlaceAndExit() {
			int cell = TileCursor.Instance.Cell;
			if (!Grid.IsVisible(cell)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}
			var pos = Grid.CellToPosCBC(cell, _def.SceneLayer);
			var orientation = BuildMenuData.GetCurrentOrientation();
			string failReason;
			if (!_def.IsValidPlaceLocation(BuildTool.Instance.visualizer, pos, orientation, out failReason)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);
				return;
			}

			bool hasMaterials = HasSufficientMaterials();
			BuildTool.Instance.OnLeftClickDown(pos);
			BuildTool.Instance.OnLeftClickUp(pos);
			if (_def.OnePerWorld)
				return;

			string announcement = hasMaterials
				? (string)STRINGS.ONIACCESS.BUILD_MENU.PLACED
				: (string)STRINGS.ONIACCESS.BUILD_MENU.PLACED_NO_MATERIAL;
			SpeechPipeline.SpeakInterrupt(announcement);
			ExitBuildMode();
		}

		private bool HasSufficientMaterials() {
			try {
				var panel = PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel;
				if (panel.CurrentSelectedElement == null)
					return true;
				return _def.MaterialsAvailable(panel.GetSelectedElementAsList, ClusterManager.Instance.activeWorld)
					|| DebugHandler.InstantBuildMode;
			} catch (Exception ex) {
				Util.Log.Warn($"BuildToolHandler.HasSufficientMaterials: {ex.Message}");
				return true;
			}
		}

		// ========================================
		// UTILITY PLACEMENT
		// ========================================

		private void UtilityPlacement() {
			int cell = TileCursor.Instance.Cell;

			if (!Grid.IsVisible(cell)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}

			if (!UtilityStartSet) {
				var pos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
				string failReason;
				if (!_def.IsValidPlaceLocation(null, pos, Orientation.Neutral, out failReason)) {
					PlayNegativeSound();
					SpeechPipeline.SpeakInterrupt(
						failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);
					return;
				}
				_utilityStartCell = cell;
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.START_SET);
				return;
			}

			int startCol = Grid.CellColumn(_utilityStartCell);
			int startRow = Grid.CellRow(_utilityStartCell);
			int endCol = Grid.CellColumn(cell);
			int endRow = Grid.CellRow(cell);
			bool sameCol = startCol == endCol;
			bool sameRow = startRow == endRow;

			if (!sameCol && !sameRow) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.BUILD_MENU.MUST_BE_STRAIGHT);
				return;
			}

			var path = BuildLinePath(_utilityStartCell, cell);
			var tool = GetActiveUtilityTool();
			if (tool == null) {
				Util.Log.Error("BuildToolHandler.UtilityPlacement: no active utility tool");
				return;
			}

			if (!ValidateUtilityPath(path)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.BUILD_MENU.INVALID_LINE);
				return;
			}

			SimulateUtilityDrag(path, tool);
			_utilityStartCell = Grid.InvalidCell;
			SpeechPipeline.SpeakInterrupt(
				string.Format((string)STRINGS.ONIACCESS.BUILD_MENU.LINE_CELLS, path.Count)
				+ ", " + (string)STRINGS.ONIACCESS.BUILD_MENU.PLACED);
		}

		private void UtilityPlaceAndExit() {
			int cell = TileCursor.Instance.Cell;
			if (!Grid.IsVisible(cell)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}
			var pos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
			string failReason;
			if (!_def.IsValidPlaceLocation(null, pos, Orientation.Neutral, out failReason)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);
				return;
			}

			var path = new List<int> { cell };
			var tool = GetActiveUtilityTool();
			if (tool == null) {
				Util.Log.Error("BuildToolHandler.UtilityPlaceAndExit: no active utility tool");
				return;
			}

			SimulateUtilityDrag(path, tool);
			SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.PLACED);
			ExitBuildMode();
		}

		private static List<int> BuildLinePath(int startCell, int endCell) {
			var path = new List<int>();
			int startCol = Grid.CellColumn(startCell);
			int startRow = Grid.CellRow(startCell);
			int endCol = Grid.CellColumn(endCell);
			int endRow = Grid.CellRow(endCell);

			if (startRow == endRow) {
				int step = startCol <= endCol ? 1 : -1;
				for (int x = startCol; x != endCol + step; x += step)
					path.Add(Grid.XYToCell(x, startRow));
			} else {
				int step = startRow <= endRow ? 1 : -1;
				for (int y = startRow; y != endRow + step; y += step)
					path.Add(Grid.XYToCell(startCol, y));
			}
			return path;
		}

		/// <summary>
		/// Checks whether every cell in a straight line from start to end is
		/// a valid placement location. Uses the same IsValidPlaceLocation
		/// check that the game's TryPlace uses, so the result matches what
		/// will actually happen when the line is placed.
		/// Used by BuildToolSection for live glance feedback.
		/// </summary>
		internal static bool IsUtilityLineValid(int startCell, int endCell) {
			var handler = Instance;
			if (handler == null) return true;
			var path = BuildLinePath(startCell, endCell);
			return handler.ValidateUtilityPath(path);
		}

		private bool ValidateUtilityPath(List<int> path) {
			foreach (int cell in path) {
				var pos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Building);
				string failReason;
				if (!_def.IsValidPlaceLocation(null, pos, Orientation.Neutral, out failReason))
					return false;
			}
			return true;
		}

		private static readonly MethodInfo _onDragTool = AccessTools.Method(
			typeof(BaseUtilityBuildTool), "OnDragTool");

		private void SimulateUtilityDrag(List<int> path, BaseUtilityBuildTool tool) {
			if (path.Count == 0) return;

			var startPos = Grid.CellToPosCCC(path[0], Grid.SceneLayer.Move);
			tool.OnLeftClickDown(startPos);

			for (int i = 1; i < path.Count; i++) {
				try {
					_onDragTool.Invoke(tool, new object[] { path[i], i });
				} catch (Exception ex) {
					Util.Log.Error($"BuildToolHandler.SimulateUtilityDrag: {ex}");
				}
			}

			var endPos = Grid.CellToPosCCC(path[path.Count - 1], Grid.SceneLayer.Move);
			tool.OnLeftClickUp(endPos);
		}

		private BaseUtilityBuildTool GetActiveUtilityTool() {
			var active = PlayerController.Instance.ActiveTool;
			if (active is WireBuildTool wbt) return wbt;
			if (active is UtilityBuildTool ubt) return ubt;
			if (active is BaseUtilityBuildTool bubt) return bubt;
			return null;
		}

		private Type GetUtilityToolType() {
			if (_def.BuildingComplete.GetComponent<Wire>() != null)
				return typeof(WireBuildTool);
			return typeof(UtilityBuildTool);
		}

		// ========================================
		// ROTATION
		// ========================================

		private void Rotate() {
			if (_isUtility || _def.PermittedRotations == PermittedRotations.Unrotatable) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.BUILD_MENU.NOT_ROTATABLE);
				return;
			}

			BuildTool.Instance.TryRotate();
			var orientation = BuildMenuData.GetCurrentOrientation();
			string announcement = BuildMenuData.GetOrientationName(orientation);

			int cell = TileCursor.Instance.Cell;
			var pos = Grid.CellToPosCBC(cell, _def.SceneLayer);
			string failReason;
			if (!_def.IsValidPlaceLocation(
					BuildTool.Instance.visualizer, pos, orientation, out failReason))
				announcement += ", " + (failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);

			string extent = BuildExtentText(orientation);
			if (extent != null)
				announcement += ", " + extent;

			SpeechPipeline.SpeakInterrupt(announcement);
		}

		/// <summary>
		/// Builds an extent description like "extends 1 left, 1 right, 1 up"
		/// for buildings larger than 1x1.
		/// </summary>
		internal static string BuildExtentText(Orientation orientation) {
			var handler = Instance;
			if (handler == null || handler._def == null) return null;
			var offsets = handler._def.PlacementOffsets;
			if (offsets == null || offsets.Length <= 1) return null;

			int minX = 0, maxX = 0, minY = 0, maxY = 0;
			foreach (var offset in offsets) {
				var rotated = Rotatable.GetRotatedCellOffset(offset, orientation);
				if (rotated.x < minX) minX = rotated.x;
				if (rotated.x > maxX) maxX = rotated.x;
				if (rotated.y < minY) minY = rotated.y;
				if (rotated.y > maxY) maxY = rotated.y;
			}

			var parts = new List<string>();
			if (minX < 0)
				parts.Add(string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.EXTENT_LEFT, -minX));
			if (maxX > 0)
				parts.Add(string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.EXTENT_RIGHT, maxX));
			if (maxY > 0)
				parts.Add(string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.EXTENT_UP, maxY));
			if (minY < 0)
				parts.Add(string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.EXTENT_DOWN, -minY));

			if (parts.Count == 0) return null;
			return string.Format(
				(string)STRINGS.ONIACCESS.BUILD_MENU.EXTENT_FORMAT,
				string.Join(", ", parts));
		}

		// ========================================
		// QUICK CANCEL
		// ========================================

		private void QuickCancel() {
			int cell = TileCursor.Instance.Cell;
			var go = FindMatchingConstruction(cell);
			if (go == null) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.BUILD_MENU.NO_CONSTRUCTION);
				return;
			}

			go.Trigger((int)GameHashes.Cancel);
			PlayCancelSound();
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.BUILD_MENU.CANCEL_CONSTRUCTION);
		}

		private UnityEngine.GameObject FindMatchingConstruction(int cell) {
			int[] layers = {
				(int)ObjectLayer.Building,
				(int)ObjectLayer.FoundationTile,
				(int)ObjectLayer.Backwall,
				(int)ObjectLayer.Wire,
				(int)ObjectLayer.LiquidConduit,
				(int)ObjectLayer.GasConduit,
				(int)ObjectLayer.SolidConduit,
				(int)ObjectLayer.LogicWire,
			};
			foreach (int layer in layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null) continue;
				var buc = go.GetComponent<BuildingUnderConstruction>();
				if (buc != null && buc.Def == _def)
					return go;
			}
			return null;
		}

		// ========================================
		// NAVIGATION
		// ========================================

		private void ReturnToBuildingList() {
			HandlerStack.Replace(new BuildMenuHandler(_category, _def));
		}

		private void OpenInfoPanel() {
			HandlerStack.Push(new BuildInfoHandler(_def));
		}

		// ========================================
		// PRIORITY
		// ========================================

		private void SetPriority(int value) {
			PrioritySetting setting;
			string announcement;
			if (value == 0) {
				setting = new PrioritySetting(PriorityScreen.PriorityClass.topPriority, 1);
				announcement = (string)STRINGS.ONIACCESS.TOOLS.PRIORITY_EMERGENCY;
			} else {
				setting = new PrioritySetting(PriorityScreen.PriorityClass.basic, value);
				announcement = string.Format((string)STRINGS.ONIACCESS.TOOLS.PRIORITY_BASIC, value);
			}

			PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel.PriorityScreen
			.SetScreenPriority(setting, false);
			PriorityScreen.PlayPriorityConfirmSound(setting);
			SpeechPipeline.SpeakInterrupt(announcement);
		}

		// ========================================
		// CLOSE AND CLEANUP
		// ========================================

		private void CloseEverything() {
			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);

			QueueOverlayAndPop();

			try {
				SelectTool.Instance.Activate();
			} catch (Exception ex) {
				Util.Log.Error($"BuildToolHandler.CloseEverything: {ex}");
			}

			SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.CANCELED);
			PlayDeactivateSound();
		}

		private void ExitBuildMode() {
			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);

			QueueOverlayAndPop();

			try {
				SelectTool.Instance.Activate();
			} catch (Exception ex) {
				Util.Log.Error($"BuildToolHandler.ExitBuildMode: {ex}");
			}

			PlayDeactivateSound();
		}

		private void QueueOverlayAndPop() {
			for (int i = HandlerStack.Count - 1; i >= 0; i--) {
				if (HandlerStack.GetAt(i) is TileCursorHandler tch) {
					tch.QueueNextOverlayAnnouncement();
					break;
				}
			}
			HandlerStack.Pop();
		}

		// ========================================
		// SOUNDS
		// ========================================

		private static void PlayDeactivateSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Tile_Cancel"));
			} catch (Exception ex) {
				Util.Log.Error($"BuildToolHandler.PlayDeactivateSound: {ex}");
			}
		}

		private static void PlayNegativeSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Negative"));
			} catch (Exception ex) {
				Util.Log.Error($"BuildToolHandler.PlayNegativeSound: {ex}");
			}
		}

		private static void PlayCancelSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Tile_Confirm_NegativeTool"));
			} catch (Exception ex) {
				Util.Log.Error($"BuildToolHandler.PlayCancelSound: {ex}");
			}
		}
	}
}
