using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OniAccess.Handlers.Tiles;
using OniAccess.Handlers.Tiles.Tools;
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
	public class BuildToolHandler : BaseScreenHandler {
		public static BuildToolHandler Instance { get; private set; }

		private HashedString _category;
		private int _buildingIndex;
		internal BuildingDef _def;
		private bool _isUtility;

		// Utility placement state
		private int _utilityStartCell = Grid.InvalidCell;
		internal bool UtilityStartSet => _utilityStartCell != Grid.InvalidCell;
		internal int UtilityStartCell => _utilityStartCell;

		private bool _suppressReactivation;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
			new ConsumedKey(KKeyCode.Space, Modifier.Shift),
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

		public override string DisplayName => BuildMenuData.BuildPlacementAnnouncement(_def);
		public override bool CapturesAllInput => false;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Space", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_PLACE),
			new HelpEntry("R", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_ROTATE),
			new HelpEntry("Tab", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_BUILDING_LIST),
			new HelpEntry("I", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_INFO),
			new HelpEntry("0-9", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.SET_PRIORITY),
			new HelpEntry("Shift+Space", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_CANCEL_CONSTRUCTION),
			new HelpEntry("Escape", (string)STRINGS.ONIACCESS.HELP.CLOSE),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public BuildToolHandler(HashedString category, int buildingIndex, BuildingDef def) {
			_category = category;
			_buildingIndex = buildingIndex;
			_def = def;
			_isUtility = BuildMenuData.IsUtilityBuilding(def);
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			Instance = this;

			if (_suppressReactivation) {
				_suppressReactivation = false;
				return;
			}

			if (TileCursor.Instance != null) {
				var composer = ToolProfileRegistry.Instance.GetComposer(
					_isUtility ? GetUtilityToolType() : typeof(BuildTool));
				TileCursor.Instance.ActiveToolComposer = composer;
			}

			if (Game.Instance != null) {
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
				Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
			}

			SpeechPipeline.SpeakInterrupt(DisplayName);
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
			if (data is SelectTool) {
				if (_def != null && _def.OnePerWorld)
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.PLACED);
				else
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.CANCELED);
				PlayDeactivateSound();
				QueueOverlayAndPop();
			}
		}

		// ========================================
		// KEY HANDLING
		// ========================================

		public override void Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)) {
				if (InputUtil.ShiftHeld())
					QuickCancel();
				else if (!InputUtil.AnyModifierHeld()) {
					if (_isUtility)
						UtilityPlacement();
					else
						RegularPlacement();
				}
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R)
				&& !InputUtil.AnyModifierHeld()) {
				Rotate();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)
				&& !InputUtil.AnyModifierHeld()) {
				ReturnToBuildingList();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)
				&& !InputUtil.AnyModifierHeld()) {
				OpenInfoPanel();
				return;
			}

			if (!InputUtil.AnyModifierHeld()) {
				for (int i = 0; i <= 9; i++) {
					if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha0 + i)) {
						SetPriority(i);
						return;
					}
				}
			}
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
			var pos = Grid.CellToPosCBC(cell, _def.SceneLayer);
			var orientation = BuildMenuData.GetCurrentOrientation();
			string failReason;
			if (!_def.IsValidPlaceLocation(BuildTool.Instance.visualizer, pos, orientation, out failReason)) {
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(failReason ?? (string)STRINGS.ONIACCESS.BUILD_MENU.OBSTRUCTED);
				return;
			}

			BuildTool.Instance.OnLeftClickDown(pos);
			BuildTool.Instance.OnLeftClickUp(pos);
			// OnePerWorld buildings auto-dismiss the tool, triggering
			// OnActiveToolChanged which announces "placed" and pops.
			if (!_def.OnePerWorld)
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.PLACED);
		}

		// ========================================
		// UTILITY PLACEMENT
		// ========================================

		private static MethodInfo _checkValidPathPiece;

		private void UtilityPlacement() {
			int cell = TileCursor.Instance.Cell;

			if (!UtilityStartSet) {
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

			if (!ValidateUtilityPath(path, tool)) {
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

		private bool ValidateUtilityPath(List<int> path, BaseUtilityBuildTool tool) {
			if (_checkValidPathPiece == null)
				_checkValidPathPiece = AccessTools.Method(
					typeof(BaseUtilityBuildTool), "CheckValidPathPiece");

			foreach (int cell in path) {
				try {
					bool valid = (bool)_checkValidPathPiece.Invoke(tool, new object[] { cell });
					if (!valid) return false;
				} catch (Exception ex) {
					Util.Log.Error($"BuildToolHandler.ValidateUtilityPath: {ex}");
					return false;
				}
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
			_suppressReactivation = true;
			HandlerStack.Push(new BuildingListHandler(_category, _buildingIndex, this));
		}

		internal void SwitchBuilding(BuildingDef newDef, int newIndex) {
			_def = newDef;
			_buildingIndex = newIndex;
			_isUtility = BuildMenuData.IsUtilityBuilding(newDef);
			_utilityStartCell = Grid.InvalidCell;

			if (TileCursor.Instance != null) {
				var composer = ToolProfileRegistry.Instance.GetComposer(
					_isUtility ? GetUtilityToolType() : typeof(BuildTool));
				TileCursor.Instance.ActiveToolComposer = composer;
			}

			SpeechPipeline.SpeakInterrupt(BuildMenuData.BuildPlacementAnnouncement(newDef));
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

			ToolMenu.Instance.PriorityScreen.SetScreenPriority(setting, false);
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
