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
	public class ToolHandler: BaseScreenHandler {
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

			if (Game.Instance != null) {
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
				Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
			}

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
			if (_rectangles.Count == 0 && _pendingFirstCorner == Grid.InvalidCell) {
				int cell = TileCursor.Instance.Cell;
				_rectangles.Add(new RectCorners { Cell1 = cell, Cell2 = cell });
			}

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

			string summary = BuildConfirmSummary(out int total);
			if (total == 0) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.NO_VALID_CELLS);
				PlayNegativeSound();
				DeactivateToolAndPop();
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
			string label = _toolInfo != null ? _toolInfo.Label : (string)STRINGS.ONIACCESS.TOOLS.FALLBACK_LABEL;
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
			rect.GetBounds(out int minX, out int maxX, out int minY, out int maxY);
			int width = maxX - minX + 1;
			int height = maxY - minY + 1;
			int valid = 0;
			int invalid = 0;

			for (int y = minY; y <= maxY; y++) {
				for (int x = minX; x <= maxX; x++) {
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidCell(cell) || !Grid.IsVisible(cell)) {
						invalid++;
						continue;
					}
					if (_toolInfo != null && _toolInfo.CountTargets != null) {
						int count = _toolInfo.CountTargets(cell);
						if (count > 0)
							valid += count;
						else
							invalid++;
					} else {
						valid++;
					}
				}
			}

			string format = invalid > 0
				? (string)STRINGS.ONIACCESS.TOOLS.RECT_SUMMARY_INVALID
				: (string)STRINGS.ONIACCESS.TOOLS.RECT_SUMMARY;
			return string.Format(format, width, height, valid, invalid);
		}

		private string BuildConfirmSummary(out int total) {
			var cells = new HashSet<int>();
			total = 0;
			foreach (var r in _rectangles) {
				r.GetBounds(out int minX, out int maxX, out int minY, out int maxY);
				for (int y = minY; y <= maxY; y++)
					for (int x = minX; x <= maxX; x++) {
						int cell = Grid.XYToCell(x, y);
						if (!Grid.IsValidCell(cell) || !Grid.IsVisible(cell)) continue;
						if (!cells.Add(cell)) continue;
						if (_toolInfo != null && _toolInfo.CountTargets != null)
							total += _toolInfo.CountTargets(cell);
						else
							total++;
					}
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
			string noun = count == 1 ? "item" : "items";
			return string.Format(format, count, priority, noun);
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
			for (int i = HandlerStack.Count - 1; i >= 0; i--) {
				if (HandlerStack.GetAt(i) is TileCursorHandler tch) {
					tch.QueueNextOverlayAnnouncement();
					break;
				}
			}
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
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DIG,
					CountDigTargets),
				new ModToolInfo("CancelTool",
					Strings.Get("STRINGS.UI.TOOLS.CANCEL.TOOLNAME"),
					typeof(CancelTool), true, false, false, false,
					"Tile_Drag_NegativeTool", "Tile_Confirm_NegativeTool",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_CANCEL,
					CountCancelTargets),
				new ModToolInfo("DeconstructTool",
					Strings.Get("STRINGS.UI.TOOLS.DECONSTRUCT.TOOLNAME"),
					typeof(DeconstructTool), true, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DECONSTRUCT,
					CountDeconstructTargets),
				new ModToolInfo("PrioritizeTool",
					Strings.Get("STRINGS.UI.TOOLS.PRIORITIZE.TOOLNAME"),
					typeof(PrioritizeTool), true, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_PRIORITIZE,
					CountPrioritizeTargets),
				new ModToolInfo("DisinfectTool",
					Strings.Get("STRINGS.UI.TOOLS.DISINFECT.TOOLNAME"),
					typeof(DisinfectTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DISINFECT,
					CountDisinfectTargets),
				new ModToolInfo("ClearTool",
					Strings.Get("STRINGS.UI.TOOLS.MARKFORSTORAGE.TOOLNAME"),
					typeof(ClearTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_SWEEP,
					CountSweepTargets),
				new ModToolInfo("AttackTool",
					Strings.Get("STRINGS.UI.TOOLS.ATTACK.TOOLNAME"),
					typeof(AttackTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_ATTACK,
					CountAttackTargets),
				new ModToolInfo("MopTool",
					Strings.Get("STRINGS.UI.TOOLS.MOP.TOOLNAME"),
					typeof(MopTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_MOP,
					CountMopTargets),
				new ModToolInfo("CaptureTool",
					Strings.Get("STRINGS.UI.TOOLS.CAPTURE.TOOLNAME"),
					typeof(CaptureTool), false, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_CAPTURE,
					CountCaptureTargets),
				new ModToolInfo("HarvestTool",
					Strings.Get("STRINGS.UI.TOOLS.HARVEST.TOOLNAME"),
					typeof(HarvestTool), true, true, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_HARVEST,
					CountHarvestTargets),
				new ModToolInfo("EmptyPipeTool",
					Strings.Get("STRINGS.UI.TOOLS.EMPTY_PIPE.TOOLNAME"),
					typeof(EmptyPipeTool), true, false, true, false,
					"Tile_Drag", "Tile_Confirm",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_EMPTY_PIPE,
					CountEmptyPipeTargets),
				new ModToolInfo("DisconnectTool",
					Strings.Get("STRINGS.UI.TOOLS.DISCONNECT.TOOLNAME"),
					typeof(DisconnectTool), true, false, false, true,
					"Tile_Drag_NegativeTool", "OutletDisconnected",
					(string)STRINGS.ONIACCESS.TOOLS.CONFIRM_DISCONNECT,
					CountDisconnectTargets),
			}.AsReadOnly();
		}

		// ========================================
		// CELL VALIDATORS
		// ========================================

		private static int CountDigTargets(int cell) {
			if (!Grid.Solid[cell] || Grid.Foundation[cell]) return 0;
			if (Grid.Objects[cell, 7] != null) return 0;
			return 1;
		}

		private static int CountMopTargets(int cell) {
			if (Grid.Objects[cell, 8] != null) return 0;
			if (Grid.Solid[cell]) return 0;
			if (!Grid.Element[cell].IsLiquid) return 0;
			int below = Grid.CellBelow(cell);
			if (!Grid.IsValidCell(below) || !Grid.Solid[below]) return 0;
			return Grid.Mass[cell] <= 150f ? 1 : 0;
		}

		private static int CountSweepTargets(int cell) {
			var go = Grid.Objects[cell, 3];
			if (go == null) return 0;
			var pickupable = go.GetComponent<Pickupable>();
			if (pickupable == null) return 0;
			var listItem = pickupable.objectLayerListItem;
			while (listItem != null) {
				var itemGo = listItem.gameObject;
				listItem = listItem.nextItem;
				if (itemGo == null) continue;
				var p = itemGo.GetComponent<Pickupable>();
				if (p == null || p.KPrefabID.HasTag(GameTags.BaseMinion)) continue;
				var clearable = itemGo.GetComponent<Clearable>();
				if (clearable != null && clearable.isClearable) return 1;
			}
			return 0;
		}

		private static int CountDisinfectTargets(int cell) {
			for (int i = 0; i < 45; i++) {
				var go = Grid.Objects[cell, i];
				if (go == null) continue;
				if (go.GetComponent<Disinfectable>() == null) continue;
				if (go.GetComponent<PrimaryElement>().DiseaseCount > 0) return 1;
			}
			return 0;
		}

		private static int CountHarvestTargets(int cell) {
			foreach (var item in Components.HarvestDesignatables.Items) {
				if (Grid.PosToCell(item) == cell) return 1;
				var area = item.area;
				if (area != null && area.CheckIsOccupying(cell)) return 1;
			}
			return 0;
		}

		private static int CountDeconstructTargets(int cell) {
			var targets = ReadFilterTargets();
			for (int i = 0; i < 45; i++) {
				var go = Grid.Objects[cell, i];
				if (go == null) continue;
				if (go.GetComponent<Deconstructable>() == null) continue;
				string layer = GetFilterLayer(go);
				if (IsFilterLayerActive(targets, layer)) return 1;
			}
			return 0;
		}

		private static int CountCancelTargets(int cell) {
			var targets = ReadFilterTargets();
			for (int i = 0; i < 45; i++) {
				var go = Grid.Objects[cell, i];
				if (go == null) continue;
				string layer = GetFilterLayer(go);
				if (IsFilterLayerActive(targets, layer)) return 1;
			}
			return 0;
		}

		private static int CountPrioritizeTargets(int cell) {
			var targets = ReadFilterTargets();
			for (int i = 0; i < 45; i++) {
				var go = Grid.Objects[cell, i];
				if (go == null) continue;
				var pickupable = go.GetComponent<Pickupable>();
				if (pickupable != null) {
					var listItem = pickupable.objectLayerListItem;
					while (listItem != null) {
						var itemGo = listItem.gameObject;
						listItem = listItem.nextItem;
						if (itemGo == null || itemGo.GetComponent<MinionIdentity>() != null) continue;
						string layer = GetPrioritizeFilterLayer(itemGo);
						if (!IsFilterLayerActive(targets, layer)) continue;
						var p = itemGo.GetComponent<Prioritizable>();
						if (p != null && p.showIcon && p.IsPrioritizable()) return 1;
					}
				} else {
					string layer = GetPrioritizeFilterLayer(go);
					if (!IsFilterLayerActive(targets, layer)) continue;
					var p = go.GetComponent<Prioritizable>();
					if (p != null && p.showIcon && p.IsPrioritizable()) return 1;
				}
			}
			return 0;
		}

		private static int CountEmptyPipeTargets(int cell) {
			var targets = ReadFilterTargets();
			for (int i = 0; i < 45; i++) {
				if (!IsObjectLayerActive(targets, i)) continue;
				var go = Grid.Objects[cell, i];
				if (go == null) continue;
				if (go.GetComponent<IEmptyConduitWorkable>() != null) return 1;
			}
			return 0;
		}

		private static int CountDisconnectTargets(int cell) {
			var targets = ReadFilterTargets();
			for (int i = 0; i < 45; i++) {
				var go = Grid.Objects[cell, i];
				if (go == null) continue;
				string layer = GetFilterLayer(go);
				if (!IsFilterLayerActive(targets, layer)) continue;
				var building = go.GetComponent<Building>();
				if (building == null) continue;
				if (building.Def.BuildingComplete.GetComponent<IHaveUtilityNetworkMgr>() != null) return 1;
			}
			return 0;
		}

		private static int CountAttackTargets(int cell) {
			int count = 0;
			foreach (var item in Components.FactionAlignments.Items) {
				if (item.IsNullOrDestroyed()) continue;
				if (FactionManager.Instance.GetDisposition(FactionManager.FactionID.Duplicant, item.Alignment)
					== FactionManager.Disposition.Assist) continue;
				if (Grid.PosToCell(item.transform.GetPosition()) == cell) count++;
			}
			return count;
		}

		private static int CountCaptureTargets(int cell) {
			int count = 0;
			foreach (var item in Components.Capturables.Items) {
				if (!item.allowCapture) continue;
				if (Grid.PosToCell(item.transform.GetPosition()) == cell) count++;
			}
			return count;
		}

		// ========================================
		// FILTER HELPERS
		// ========================================

		private static Dictionary<string, ToolParameterMenu.ToggleState> ReadFilterTargets() {
			var tool = PlayerController.Instance.ActiveTool as FilteredDragTool;
			if (tool == null) return null;
			return Traverse.Create(tool)
				.Field<Dictionary<string, ToolParameterMenu.ToggleState>>("currentFilterTargets").Value;
		}

		private static bool IsFilterLayerActive(
			Dictionary<string, ToolParameterMenu.ToggleState> targets, string layer) {
			if (targets == null) return true;
			if (targets.TryGetValue(ToolParameterMenu.FILTERLAYERS.ALL, out var allState)
				&& allState == ToolParameterMenu.ToggleState.On)
				return true;
			return targets.TryGetValue(layer.ToUpper(), out var state)
				&& state == ToolParameterMenu.ToggleState.On;
		}

		private static bool IsObjectLayerActive(
			Dictionary<string, ToolParameterMenu.ToggleState> targets, int layerIndex) {
			if (targets == null) return true;
			if (targets.TryGetValue(ToolParameterMenu.FILTERLAYERS.ALL, out var allState)
				&& allState == ToolParameterMenu.ToggleState.On)
				return true;
			foreach (var kv in targets) {
				if (kv.Value != ToolParameterMenu.ToggleState.On) continue;
				int? mapped = ObjectLayerFromFilterKey(kv.Key);
				if (mapped.HasValue && mapped.Value == layerIndex) return true;
			}
			return false;
		}

		private static string GetFilterLayer(UnityEngine.GameObject go) {
			var bc = go.GetComponent<BuildingComplete>();
			if (bc != null) return FilterKeyFromObjectLayer(bc.Def.ObjectLayer);
			var buc = go.GetComponent<BuildingUnderConstruction>();
			if (buc != null) return FilterKeyFromObjectLayer(buc.Def.ObjectLayer);
			if (go.GetComponent<Clearable>() != null || go.GetComponent<Moppable>() != null)
				return "CleanAndClear";
			if (go.GetComponent<Diggable>() != null)
				return "DigPlacer";
			return "Default";
		}

		private static string GetPrioritizeFilterLayer(UnityEngine.GameObject go) {
			var constructable = go.GetComponent<Constructable>();
			var decon = go.GetComponent<Deconstructable>();
			if (constructable != null || (decon != null && decon.IsMarkedForDeconstruction()))
				return ToolParameterMenu.FILTERLAYERS.CONSTRUCTION;
			if (go.GetComponent<Diggable>() != null)
				return ToolParameterMenu.FILTERLAYERS.DIG;
			if (go.GetComponent<Clearable>() != null || go.GetComponent<Moppable>() != null
				|| go.GetComponent<StorageLocker>() != null)
				return ToolParameterMenu.FILTERLAYERS.CLEAN;
			return ToolParameterMenu.FILTERLAYERS.OPERATE;
		}

		private static string FilterKeyFromObjectLayer(ObjectLayer layer) {
			switch (layer) {
				case ObjectLayer.Building:
				case ObjectLayer.Gantry:
					return "Buildings";
				case ObjectLayer.Wire:
				case ObjectLayer.WireConnectors:
					return "Wires";
				case ObjectLayer.LiquidConduit:
				case ObjectLayer.LiquidConduitConnection:
					return "LiquidPipes";
				case ObjectLayer.GasConduit:
				case ObjectLayer.GasConduitConnection:
					return "GasPipes";
				case ObjectLayer.SolidConduit:
				case ObjectLayer.SolidConduitConnection:
					return "SolidConduits";
				case ObjectLayer.FoundationTile:
					return "Tiles";
				case ObjectLayer.LogicGate:
				case ObjectLayer.LogicWire:
					return "Logic";
				case ObjectLayer.Backwall:
					return "BackWall";
				default:
					return "Default";
			}
		}

		private static int? ObjectLayerFromFilterKey(string key) {
			switch (key.ToLower()) {
				case "buildings": return (int)ObjectLayer.Building;
				case "wires": return (int)ObjectLayer.Wire;
				case "liquidpipes": return (int)ObjectLayer.LiquidConduit;
				case "gaspipes": return (int)ObjectLayer.GasConduit;
				case "solidconduits": return (int)ObjectLayer.SolidConduit;
				case "tiles": return (int)ObjectLayer.FoundationTile;
				case "logic": return (int)ObjectLayer.LogicWire;
				case "backwall": return (int)ObjectLayer.Backwall;
				default: return null;
			}
		}
	}
}
