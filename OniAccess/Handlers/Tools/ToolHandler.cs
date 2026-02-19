using System;
using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Handlers.Tiles;
using OniAccess.Handlers.Tiles.Tools;
using OniAccess.Input;
using OniAccess.Patches;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tools {
	/// <summary>
	/// Non-modal handler for tool mode. Sits on top of TileCursorHandler,
	/// intercepts tool-specific keys (Space, Enter, Escape, 0-9, F, Delete,
	/// Ctrl+Arrows) and passes everything else through to the tile cursor.
	///
	/// Manages rectangle selection state. On confirm, submits each rectangle
	/// to the game via DragTool.OnLeftClickDown/OnLeftClickUp.
	/// </summary>
	public class ToolHandler : BaseScreenHandler {
		public static ToolHandler Instance { get; private set; }

		private int _pendingFirstCorner = Grid.InvalidCell;
		private readonly List<RectCorners> _rectangles = new List<RectCorners>();
		private ModToolInfo _toolInfo;
		private string _lastFilterKey;

		private struct RectCorners {
			public int Cell1;
			public int Cell2;

			public void GetBounds(out int minX, out int maxX, out int minY, out int maxY) {
				minX = Math.Min(Grid.CellColumn(Cell1), Grid.CellColumn(Cell2));
				maxX = Math.Max(Grid.CellColumn(Cell1), Grid.CellColumn(Cell2));
				minY = Math.Min(Grid.CellRow(Cell1), Grid.CellRow(Cell2));
				maxY = Math.Max(Grid.CellRow(Cell1), Grid.CellRow(Cell2));
			}

			public bool Contains(int cell) {
				GetBounds(out int minX, out int maxX, out int minY, out int maxY);
				int cx = Grid.CellColumn(cell);
				int cy = Grid.CellRow(cell);
				return cx >= minX && cx <= maxX && cy >= minY && cy <= maxY;
			}
		}

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
			new ConsumedKey(KKeyCode.Return),
			new ConsumedKey(KKeyCode.Delete),
			new ConsumedKey(KKeyCode.Backspace),
			new ConsumedKey(KKeyCode.F),
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
			new ConsumedKey(KKeyCode.UpArrow, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.DownArrow, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.LeftArrow, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.RightArrow, Modifier.Ctrl),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		public override string DisplayName => BuildActivationAnnouncement();
		public override bool CapturesAllInput => false;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Space", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.SET_CORNER),
			new HelpEntry("Enter", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.CONFIRM_TOOL),
			new HelpEntry("Escape", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.CANCEL_TOOL),
			new HelpEntry("0-9", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.SET_PRIORITY),
			new HelpEntry("F", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.OPEN_FILTER),
			new HelpEntry("Delete", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.CLEAR_RECT),
			new HelpEntry("Ctrl+Arrows", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.JUMP_SELECTION),
		}.AsReadOnly();

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			Instance = this;

			var activeTool = PlayerController.Instance.ActiveTool;
			_toolInfo = FindToolInfo(activeTool);

			if (TileCursor.Instance != null && _toolInfo != null) {
				var composer = ToolProfileRegistry.Instance.GetComposer(_toolInfo.ToolType);
				TileCursor.Instance.ActiveToolComposer = composer;
			}

			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
			Game.Instance.Subscribe(1174281782, OnActiveToolChanged);

			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged -= OnOverlayChanged;
			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged += OnOverlayChanged;

			if (_toolInfo != null && _toolInfo.HasFilterMenu) {
				var mode = OverlayScreen.Instance != null
					? OverlayScreen.Instance.GetMode()
					: OverlayModes.None.ID;
				_lastFilterKey = FilterKeyForOverlay(mode);
			}

			SpeechPipeline.SpeakInterrupt(DisplayName);
		}

		public override void OnDeactivate() {
			Instance = null;

			if (TileCursor.Instance != null)
				TileCursor.Instance.ActiveToolComposer = null;

			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);

			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged -= OnOverlayChanged;

			_lastFilterKey = null;
			_rectangles.Clear();
			_pendingFirstCorner = Grid.InvalidCell;
		}

		private void OnActiveToolChanged(object data) {
			if (data is SelectTool) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CANCELED);
				PlayDeactivateSound();
				HandlerStack.Pop();
			}
		}

		private void OnOverlayChanged(HashedString newMode) {
			if (_toolInfo == null || !_toolInfo.HasFilterMenu)
				return;

			string newKey = FilterKeyForOverlay(newMode);
			if (newKey == _lastFilterKey)
				return;

			_lastFilterKey = newKey;

			bool hadSelection = HasSelection;
			ClearSelection();

			string announcement = newKey == ToolParameterMenu.FILTERLAYERS.ALL
				? (string)STRINGS.ONIACCESS.TOOLS.FILTER_REMOVED
				: (string)STRINGS.ONIACCESS.TOOLS.FILTERED;
			if (hadSelection)
				announcement += ", " + (string)STRINGS.ONIACCESS.TOOLS.SELECTION_CLEARED;
			SpeechPipeline.SpeakQueued(announcement);
		}

		/// <summary>
		/// Maps an overlay mode to the filter key the game will apply.
		/// Mirrors FilteredDragTool.OnOverlayChanged logic so we don't
		/// depend on reading ToolParameterMenu after the game updates it.
		/// </summary>
		private static string FilterKeyForOverlay(HashedString overlay) {
			if (overlay == OverlayModes.Power.ID)
				return ToolParameterMenu.FILTERLAYERS.WIRES;
			if (overlay == OverlayModes.LiquidConduits.ID)
				return ToolParameterMenu.FILTERLAYERS.LIQUIDCONDUIT;
			if (overlay == OverlayModes.GasConduits.ID)
				return ToolParameterMenu.FILTERLAYERS.GASCONDUIT;
			if (overlay == OverlayModes.SolidConveyor.ID)
				return ToolParameterMenu.FILTERLAYERS.SOLIDCONDUIT;
			if (overlay == OverlayModes.Logic.ID)
				return ToolParameterMenu.FILTERLAYERS.LOGIC;
			return ToolParameterMenu.FILTERLAYERS.ALL;
		}

		// ========================================
		// KEY HANDLING
		// ========================================

		public override void Tick() {
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)
				&& !InputUtil.AnyModifierHeld()) {
				SetCorner();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)
				&& !InputUtil.AnyModifierHeld()) {
				ConfirmOrCancel();
				return;
			}

			if ((UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Delete)
				|| UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Backspace))
				&& !InputUtil.AnyModifierHeld()) {
				ClearRectAtCursor();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F)
				&& !InputUtil.AnyModifierHeld()) {
				OpenFilterMenu();
				return;
			}

			if (!InputUtil.AnyModifierHeld()) {
				for (int i = 0; i <= 9; i++) {
					if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Alpha0 + i)) {
						if (_toolInfo != null && _toolInfo.SupportsPriority)
							SetPriority(i);
						return;
					}
				}
			}

			if (InputUtil.CtrlHeld() && !InputUtil.ShiftHeld() && !InputUtil.AltHeld()) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
					JumpToSelectionBoundary(Tiles.Direction.Up);
					return;
				}
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
					JumpToSelectionBoundary(Tiles.Direction.Down);
					return;
				}
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
					JumpToSelectionBoundary(Tiles.Direction.Left);
					return;
				}
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
					JumpToSelectionBoundary(Tiles.Direction.Right);
					return;
				}
			}
		}

		public override bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CANCELED);
				PlayDeactivateSound();
				DeactivateToolAndPop();
				return true;
			}
			return false;
		}

		// ========================================
		// RECTANGLE SELECTION
		// ========================================

		private void SetCorner() {
			int cell = TileCursor.Instance.Cell;

			if (_pendingFirstCorner == Grid.InvalidCell) {
				_pendingFirstCorner = cell;
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CORNER_SET);
				PlayDragSound(1);
				return;
			}

			if (_toolInfo != null && _toolInfo.IsLineMode) {
				int col1 = Grid.CellColumn(_pendingFirstCorner);
				int row1 = Grid.CellRow(_pendingFirstCorner);
				int col2 = Grid.CellColumn(cell);
				int row2 = Grid.CellRow(cell);
				bool sameCol = col1 == col2;
				bool sameRow = row1 == row2;
				if (!sameCol && !sameRow) {
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.DISCONNECT_TOO_FAR);
					return;
				}
				int dist = sameRow ? Math.Abs(col2 - col1) : Math.Abs(row2 - row1);
				if (dist > 1) {
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.DISCONNECT_TOO_FAR);
					return;
				}
			}

			var rect = new RectCorners { Cell1 = _pendingFirstCorner, Cell2 = cell };
			_rectangles.Add(rect);
			_pendingFirstCorner = Grid.InvalidCell;

			int area = ComputeArea(rect);
			PlayDragSound(area);
			SpeechPipeline.SpeakInterrupt(BuildRectSummary(rect));
		}

		private void ConfirmOrCancel() {
			if (_rectangles.Count == 0) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CANCELED);
				PlayDeactivateSound();
				DeactivateToolAndPop();
				return;
			}
			SubmitRectangles();
		}

		private void SubmitRectangles() {
			var activeTool = PlayerController.Instance.ActiveTool as DragTool;
			if (activeTool == null) {
				Util.Log.Error("ToolHandler.SubmitRectangles: ActiveTool is not a DragTool");
				return;
			}

			DragToolPatches.SuppressConfirmSound = true;
			try {
				for (int i = 0; i < _rectangles.Count; i++) {
					if (i == _rectangles.Count - 1)
						DragToolPatches.SuppressConfirmSound = false;
					var r = _rectangles[i];
					var pos1 = Grid.CellToPosCCC(r.Cell1, Grid.SceneLayer.Move);
					var pos2 = Grid.CellToPosCCC(r.Cell2, Grid.SceneLayer.Move);
					activeTool.OnLeftClickDown(pos1);
					activeTool.OnLeftClickUp(pos2);
				}
			} catch (Exception ex) {
				Util.Log.Error($"ToolHandler.SubmitRectangles: {ex}");
				DragToolPatches.SuppressConfirmSound = false;
			}

			string summary = BuildConfirmSummary(out int total);
			if (total == 0) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.NO_VALID_CELLS);
				PlayNegativeSound();
				DeactivateToolAndPop();
				return;
			}

			SpeechPipeline.SpeakInterrupt(summary);
			DeactivateToolAndPop();
		}

		// ========================================
		// CELL SELECTION QUERIES
		// ========================================

		public bool IsCellSelected(int cell) {
			for (int i = 0; i < _rectangles.Count; i++) {
				if (_rectangles[i].Contains(cell))
					return true;
			}
			return false;
		}

		private void ClearRectAtCursor() {
			int cell = TileCursor.Instance.Cell;
			for (int i = _rectangles.Count - 1; i >= 0; i--) {
				if (_rectangles[i].Contains(cell)) {
					_rectangles.RemoveAt(i);
					SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.RECT_CLEARED);
					return;
				}
			}
		}

		// ========================================
		// JUMP NAVIGATION
		// ========================================

		private void JumpToSelectionBoundary(Tiles.Direction direction) {
			int current = TileCursor.Instance.Cell;
			bool currentSelected = IsCellSelected(current);
			int walk = current;

			while (true) {
				int next = TileCursor.GetNeighbor(walk, direction);
				if (next == Grid.InvalidCell || !Grid.IsValidCell(next))
					break;
				if (IsCellSelected(next) != currentSelected) {
					string speech = TileCursor.Instance.JumpTo(next);
					if (speech != null)
						SpeechPipeline.SpeakInterrupt(speech);
					return;
				}
				walk = next;
			}

			SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.NO_CHANGE);
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
		// FILTER MENU
		// ========================================

		private void OpenFilterMenu() {
			if (_toolInfo == null || !_toolInfo.HasFilterMenu)
				return;
			HandlerStack.Push(new ToolFilterHandler(this));
		}

		internal bool HasSelection => _rectangles.Count > 0 || _pendingFirstCorner != Grid.InvalidCell;

		internal void ClearSelection() {
			_rectangles.Clear();
			_pendingFirstCorner = Grid.InvalidCell;
		}

		// ========================================
		// ANNOUNCEMENTS
		// ========================================

		private string BuildActivationAnnouncement() {
			string label = _toolInfo != null ? _toolInfo.Label : "tool";
			string priorityText = null;
			string filterText = null;

			if (_toolInfo != null && _toolInfo.SupportsPriority) {
				try {
					var priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();
					if (priority.priority_class == PriorityScreen.PriorityClass.topPriority)
						priorityText = (string)STRINGS.ONIACCESS.TOOLS.PRIORITY_EMERGENCY;
					else
						priorityText = string.Format((string)STRINGS.ONIACCESS.TOOLS.PRIORITY_BASIC, priority.priority_value);
				} catch (Exception ex) {
					Util.Log.Warn($"ToolHandler.BuildActivationAnnouncement: priority read failed: {ex.Message}");
				}
			}

			if (_toolInfo != null && _toolInfo.HasFilterMenu) {
				try {
					filterText = ReadActiveFilterName();
				} catch (Exception ex) {
					Util.Log.Warn($"ToolHandler.BuildActivationAnnouncement: filter read failed: {ex.Message}");
				}
			}

			if (filterText != null && priorityText != null)
				return string.Format((string)STRINGS.ONIACCESS.TOOLS.ACTIVATION_WITH_FILTER, label, filterText, priorityText);
			if (priorityText != null)
				return string.Format((string)STRINGS.ONIACCESS.TOOLS.ACTIVATION, label, priorityText);
			if (filterText != null)
				return string.Format((string)STRINGS.ONIACCESS.TOOLS.ACTIVATION, label, filterText);
			return string.Format((string)STRINGS.ONIACCESS.TOOLS.ACTIVATION_PLAIN, label);
		}

		private static string ReadActiveFilterName() {
			var menuTraverse = Traverse.Create(ToolMenu.Instance.toolParameterMenu);
			var parameters = menuTraverse
				.Field<Dictionary<string, ToolParameterMenu.ToggleState>>("currentParameters")
				.Value;
			if (parameters == null) return null;
			foreach (var kv in parameters) {
				if (kv.Value == ToolParameterMenu.ToggleState.On) {
					return Strings.Get("STRINGS.UI.TOOLS.FILTERLAYERS." + kv.Key + ".NAME");
				}
			}
			return null;
		}

		private static string ReadActiveFilterKey() {
			var menuTraverse = Traverse.Create(ToolMenu.Instance.toolParameterMenu);
			var parameters = menuTraverse
				.Field<Dictionary<string, ToolParameterMenu.ToggleState>>("currentParameters")
				.Value;
			if (parameters == null) return null;
			foreach (var kv in parameters) {
				if (kv.Value == ToolParameterMenu.ToggleState.On)
					return kv.Key;
			}
			return null;
		}

		private string BuildRectSummary(RectCorners rect) {
			if (_toolInfo != null
				&& (_toolInfo.ToolType == typeof(AttackTool) || _toolInfo.ToolType == typeof(CaptureTool))) {
				var singleRect = new List<RectCorners> { rect };
				int count = CountEntitiesInRectangles(singleRect);
				return string.Format((string)STRINGS.ONIACCESS.TOOLS.ENTITY_RECT_SUMMARY, count);
			}

			rect.GetBounds(out int minX, out int maxX, out int minY, out int maxY);
			int width = maxX - minX + 1;
			int height = maxY - minY + 1;
			int valid = 0;
			int invalid = 0;

			for (int y = minY; y <= maxY; y++) {
				for (int x = minX; x <= maxX; x++) {
					int cell = Grid.XYToCell(x, y);
					if (Grid.IsValidCell(cell) && Grid.IsVisible(cell))
						valid++;
					else
						invalid++;
				}
			}

			return string.Format((string)STRINGS.ONIACCESS.TOOLS.RECT_SUMMARY, width, height, valid, invalid);
		}

		private int CountEntitiesInRectangles(IReadOnlyList<RectCorners> rects) {
			int count = 0;
			if (_toolInfo != null && _toolInfo.ToolType == typeof(CaptureTool)) {
				foreach (var item in Components.Capturables.Items) {
					if (!item.allowCapture) continue;
					int cell = Grid.PosToCell(item.transform.GetPosition());
					for (int i = 0; i < rects.Count; i++) {
						if (rects[i].Contains(cell)) {
							count++;
							break;
						}
					}
				}
			} else {
				foreach (var item in Components.FactionAlignments.Items) {
					if (item.IsNullOrDestroyed()) continue;
					if (FactionManager.Instance.GetDisposition(FactionManager.FactionID.Duplicant, item.Alignment)
						== FactionManager.Disposition.Assist) continue;
					int cell = Grid.PosToCell(item.transform.GetPosition());
					for (int i = 0; i < rects.Count; i++) {
						if (rects[i].Contains(cell)) {
							count++;
							break;
						}
					}
				}
			}
			return count;
		}

		private string BuildConfirmSummary(out int total) {
			bool isEntityTool = _toolInfo != null
				&& (_toolInfo.ToolType == typeof(AttackTool) || _toolInfo.ToolType == typeof(CaptureTool));

			if (isEntityTool) {
				total = CountEntitiesInRectangles(_rectangles);
			} else {
				var cells = new HashSet<int>();
				foreach (var r in _rectangles) {
					r.GetBounds(out int minX, out int maxX, out int minY, out int maxY);
					for (int y = minY; y <= maxY; y++)
						for (int x = minX; x <= maxX; x++) {
							int cell = Grid.XYToCell(x, y);
							if (Grid.IsValidCell(cell))
								cells.Add(cell);
						}
				}
				total = cells.Count;
			}
			string priorityText = "";
			if (_toolInfo != null && _toolInfo.SupportsPriority) {
				try {
					var priority = ToolMenu.Instance.PriorityScreen.GetLastSelectedPriority();
					if (priority.priority_class == PriorityScreen.PriorityClass.topPriority)
						priorityText = (string)STRINGS.ONIACCESS.TOOLS.PRIORITY_EMERGENCY;
					else
						priorityText = priority.priority_value.ToString();
				} catch (Exception ex) {
					Util.Log.Warn($"ToolHandler.BuildConfirmSummary: priority read failed: {ex.Message}");
				}
			}

			return GetConfirmString(total, priorityText);
		}

		private string GetConfirmString(int count, string priority) {
			string format = _toolInfo != null
				? _toolInfo.ConfirmFormat
				: (string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DIG;
			return string.Format(format, count, priority);
		}

		// ========================================
		// AREA COMPUTATION
		// ========================================

		private static int ComputeArea(RectCorners rect) {
			int width = Math.Abs(Grid.CellColumn(rect.Cell2) - Grid.CellColumn(rect.Cell1)) + 1;
			int height = Math.Abs(Grid.CellRow(rect.Cell2) - Grid.CellRow(rect.Cell1)) + 1;
			return width * height;
		}

		// ========================================
		// SOUNDS
		// ========================================

		private void PlayDragSound(int tileCount) {
			try {
				string soundName = _toolInfo?.DragSound ?? "Tile_Drag";
				var pos = Grid.CellToPosCCC(TileCursor.Instance.Cell, Grid.SceneLayer.Move);
				var ev = KFMOD.BeginOneShot(GlobalAssets.GetSound(soundName), pos);
				ev.setParameterByName("tileCount", tileCount);
				KFMOD.EndOneShot(ev);
			} catch (Exception ex) {
				Util.Log.Error($"ToolHandler.PlayDragSound: {ex}");
			}
		}

		private void DeactivateToolAndPop() {
			Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
			ToolMenu.Instance.ClearSelection();
			SelectTool.Instance.Activate();
			HandlerStack.Pop();
		}

		private static void PlayDeactivateSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Tile_Cancel"));
			} catch (Exception ex) {
				Util.Log.Error($"ToolHandler.PlayDeactivateSound: {ex}");
			}
		}

		private static void PlayNegativeSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Negative"));
			} catch (Exception ex) {
				Util.Log.Error($"ToolHandler.PlayNegativeSound: {ex}");
			}
		}

		// ========================================
		// TOOL INFO REGISTRY
		// ========================================

		internal static IReadOnlyList<ModToolInfo> AllTools => _allTools ?? (_allTools = BuildAllTools());
		private static IReadOnlyList<ModToolInfo> _allTools;

		private static Dictionary<Type, ModToolInfo> _toolMap;

		private static Dictionary<Type, ModToolInfo> GetToolMap() {
			if (_toolMap != null) return _toolMap;
			_toolMap = new Dictionary<Type, ModToolInfo>();
			foreach (var tool in AllTools)
				_toolMap[tool.ToolType] = tool;
			return _toolMap;
		}

		internal static ModToolInfo FindToolInfo(InterfaceTool activeTool) {
			if (activeTool == null) return null;
			GetToolMap().TryGetValue(activeTool.GetType(), out var info);
			return info;
		}

		private static IReadOnlyList<ModToolInfo> BuildAllTools() {
			return new List<ModToolInfo> {
				new ModToolInfo("DigTool",
					Strings.Get("STRINGS.UI.TOOLS.DIG.TOOLNAME"),
					typeof(DigTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DIG),
				new ModToolInfo("CancelTool",
					Strings.Get("STRINGS.UI.TOOLS.CANCEL.TOOLNAME"),
					typeof(CancelTool), true, false, false, false,
					"Tile_Drag_NegativeTool", "Tile_Confirm_NegativeTool",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_CANCEL),
				new ModToolInfo("DeconstructTool",
					Strings.Get("STRINGS.UI.TOOLS.DECONSTRUCT.TOOLNAME"),
					typeof(DeconstructTool), true, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DECONSTRUCT),
				new ModToolInfo("PrioritizeTool",
					Strings.Get("STRINGS.UI.TOOLS.PRIORITIZE.TOOLNAME"),
					typeof(PrioritizeTool), true, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_PRIORITIZE),
				new ModToolInfo("DisinfectTool",
					Strings.Get("STRINGS.UI.TOOLS.DISINFECT.TOOLNAME"),
					typeof(DisinfectTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DISINFECT),
				new ModToolInfo("ClearTool",
					Strings.Get("STRINGS.UI.TOOLS.MARKFORSTORAGE.TOOLNAME"),
					typeof(ClearTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_SWEEP),
				new ModToolInfo("AttackTool",
					Strings.Get("STRINGS.UI.TOOLS.ATTACK.TOOLNAME"),
					typeof(AttackTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_ATTACK),
				new ModToolInfo("MopTool",
					Strings.Get("STRINGS.UI.TOOLS.MOP.TOOLNAME"),
					typeof(MopTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_MOP),
				new ModToolInfo("CaptureTool",
					Strings.Get("STRINGS.UI.TOOLS.CAPTURE.TOOLNAME"),
					typeof(CaptureTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_CAPTURE),
				new ModToolInfo("HarvestTool",
					Strings.Get("STRINGS.UI.TOOLS.HARVEST.TOOLNAME"),
					typeof(HarvestTool), true, true, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_HARVEST),
				new ModToolInfo("EmptyPipeTool",
					Strings.Get("STRINGS.UI.TOOLS.EMPTY_PIPE.TOOLNAME"),
					typeof(EmptyPipeTool), true, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_EMPTY_PIPE),
				new ModToolInfo("DisconnectTool",
					Strings.Get("STRINGS.UI.TOOLS.DISCONNECT.TOOLNAME"),
					typeof(DisconnectTool), true, false, false, true,
					"Tile_Drag_NegativeTool", "OutletDisconnected",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DISCONNECT),
			}.AsReadOnly();
		}
	}
}
