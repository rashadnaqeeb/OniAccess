using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Scanner;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// BaseScreenHandler bound to the Hud KScreen. Active when the game world
	/// is loaded and no modal menu is on top.
	///
	/// Routes arrow keys to TileCursor movement, K to coordinate
	/// reading, Shift+K to coordinate mode cycling.
	///
	/// CapturesAllInput = false: game hotkeys (overlays, tools, WASD camera,
	/// pause) pass through.
	/// </summary>
	public class TileCursorHandler: BaseScreenHandler {
		private Overlays.OverlayProfileRegistry _overlayRegistry;
		private ScannerNavigator _scanner;
		private GameStateMonitor _monitor;
		private bool _hasActivated;
		private bool _overlaySubscribed;
		private bool _queueNextOverlay;

		public void QueueNextOverlayAnnouncement() => _queueNextOverlay = true;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Tab),
			new ConsumedKey(KKeyCode.BackQuote),
			new ConsumedKey(KKeyCode.T),
			new ConsumedKey(KKeyCode.I),
			new ConsumedKey(KKeyCode.I, Modifier.Shift),
			new ConsumedKey(KKeyCode.K),
			new ConsumedKey(KKeyCode.K, Modifier.Shift),
			new ConsumedKey(KKeyCode.UpArrow),
			new ConsumedKey(KKeyCode.DownArrow),
			new ConsumedKey(KKeyCode.LeftArrow),
			new ConsumedKey(KKeyCode.RightArrow),
			// Scanner keybinds
			new ConsumedKey(KKeyCode.End),
			new ConsumedKey(KKeyCode.Home),
			new ConsumedKey(KKeyCode.PageUp, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.PageDown, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.PageUp, Modifier.Shift),
			new ConsumedKey(KKeyCode.PageDown, Modifier.Shift),
			new ConsumedKey(KKeyCode.PageUp),
			new ConsumedKey(KKeyCode.PageDown),
			new ConsumedKey(KKeyCode.PageUp, Modifier.Alt),
			new ConsumedKey(KKeyCode.PageDown, Modifier.Alt),
			new ConsumedKey(KKeyCode.Q),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Arrow keys", (string)STRINGS.ONIACCESS.HELP.MOVE_CURSOR),
			new HelpEntry("Tab", "Open build menu"),
			new HelpEntry("T", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.OPEN_TOOL_MENU),
			new HelpEntry("I", (string)STRINGS.ONIACCESS.HELP.READ_TOOLTIP_SUMMARY),
			new HelpEntry("Shift+I", (string)STRINGS.ONIACCESS.HELP.READ_TOOLTIP),
			new HelpEntry("K", (string)STRINGS.ONIACCESS.HELP.READ_COORDS),
			new HelpEntry("Shift+K", (string)STRINGS.ONIACCESS.HELP.CYCLE_COORD_MODE),
			new HelpEntry("End", (string)STRINGS.ONIACCESS.SCANNER.HELP.REFRESH),
			new HelpEntry("Home", (string)STRINGS.ONIACCESS.SCANNER.HELP.TELEPORT),
			new HelpEntry("Ctrl+PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_CATEGORY),
			new HelpEntry("Shift+PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_SUBCATEGORY),
			new HelpEntry("PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_ITEM),
			new HelpEntry("Alt+PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_INSTANCE),
			new HelpEntry("Q", (string)STRINGS.ONIACCESS.GAME_STATE.READ_CYCLE_STATUS),
		}.AsReadOnly();

		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.COLONY_VIEW;
		public override bool CapturesAllInput => false;
		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public TileCursorHandler(KScreen screen) : base(screen) {
		}

		public override void OnActivate() {
			if (!_hasActivated) {
				_hasActivated = true;
				_overlayRegistry = Overlays.OverlayProfileRegistry.Build();
				Tools.ToolProfileRegistry.Build();
				TileCursor.Create(_overlayRegistry);
				_scanner = new ScannerNavigator();
				_monitor = new GameStateMonitor();
				SpeechPipeline.SpeakQueued(DisplayName);
				try {
					TileCursor.Instance.Initialize();
				} catch (System.Exception ex) {
					Util.Log.Error($"TileCursorHandler.OnActivate: cursor init failed: {ex}");
				}
			}
			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged -= OnOverlayChanged;
			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
			if (OverlayScreen.Instance != null) {
				OverlayScreen.Instance.OnOverlayChanged += OnOverlayChanged;
				_overlaySubscribed = true;
			} else {
				_overlaySubscribed = false;
			}
			if (Game.Instance != null)
				Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
		}

		public override void OnDeactivate() {
			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
			TileCursor.Destroy();
			_scanner = null;
			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged -= OnOverlayChanged;
			_overlaySubscribed = false;
		}

		private void OnOverlayChanged(HashedString newMode) {
			TileCursor.Instance.ResetRoomName();
			if (_queueNextOverlay) {
				_queueNextOverlay = false;
				SpeechPipeline.SpeakQueued(_overlayRegistry.GetOverlayName(newMode));
			} else {
				SpeechPipeline.SpeakInterrupt(_overlayRegistry.GetOverlayName(newMode));
			}
		}

		public override void Tick() {
			if (!_overlaySubscribed && OverlayScreen.Instance != null) {
				OverlayScreen.Instance.OnOverlayChanged += OnOverlayChanged;
				_overlaySubscribed = true;
			}

			_scanner.CheckWorldSwitch();
			_monitor.Tick();

			string arrived = TileCursor.Instance.SyncToCamera();
			if (arrived != null)
				SpeechPipeline.SpeakInterrupt(arrived);

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)
				&& !InputUtil.AnyModifierHeld()) {
				OpenBuildMenu();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.BackQuote)
				&& !InputUtil.AnyModifierHeld()) {
				if (SpeedControlScreen.Instance != null)
					SpeedControlScreen.Instance.TogglePause();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.T)
				&& !InputUtil.AnyModifierHeld()) {
				OpenToolPicker();
				return;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Up);
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Down);
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Left);
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Right);
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.K)) {
				if (InputUtil.ShiftHeld())
					SpeechPipeline.SpeakInterrupt(TileCursor.Instance.CycleMode());
				else
					SpeechPipeline.SpeakInterrupt(TileCursor.Instance.ReadCoordinates());
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)) {
				if (InputUtil.ShiftHeld())
					OpenTooltipBrowser();
				else
					ReadTooltipSummary();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Q)
				&& !InputUtil.AnyModifierHeld()) {
				_monitor.SpeakCycleStatus();
				return;
			}

			// Scanner keybinds
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.End)
				&& !InputUtil.AnyModifierHeld()) {
				_scanner.Refresh();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Home)
				&& !InputUtil.AnyModifierHeld()) {
				_scanner.Teleport();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.PageUp)) {
				if (InputUtil.CtrlHeld())
					_scanner.CycleCategory(-1);
				else if (InputUtil.ShiftHeld())
					_scanner.CycleSubcategory(-1);
				else if (InputUtil.AltHeld())
					_scanner.CycleInstance(-1);
				else
					_scanner.CycleItem(-1);
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.PageDown)) {
				if (InputUtil.CtrlHeld())
					_scanner.CycleCategory(1);
				else if (InputUtil.ShiftHeld())
					_scanner.CycleSubcategory(1);
				else if (InputUtil.AltHeld())
					_scanner.CycleInstance(1);
				else
					_scanner.CycleItem(1);
				return;
			}
		}

		private void SpeakMove(Direction direction) {
			string speech = TileCursor.Instance.Move(direction);
			if (speech != null)
				SpeechPipeline.SpeakInterrupt(speech);
		}

		private void OpenToolPicker() {
			if (!(PlayerController.Instance.ActiveTool is SelectTool))
				SelectTool.Instance.Activate();
			HandlerStack.Push(new OniAccess.Handlers.Tools.ToolPickerHandler());
		}

		private void OpenBuildMenu() {
			if (!(PlayerController.Instance.ActiveTool is SelectTool))
				SelectTool.Instance.Activate();
			HandlerStack.Push(new Build.BuildCategoryHandler());
		}

		private void OnActiveToolChanged(object data) {
			var tool = data as InterfaceTool;
			if (tool == null || tool is SelectTool) return;
			if (tool is BuildTool || tool is UtilityBuildTool || tool is WireBuildTool) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolHandler) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolPickerHandler) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolFilterHandler) return;
			if (HandlerStack.ActiveHandler is Build.BuildToolHandler) return;
			HandlerStack.Push(new OniAccess.Handlers.Tools.ToolHandler());
		}

		private void ReadTooltipSummary() {
			if (!Grid.IsVisible(TileCursor.Instance.Cell)) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}
			string summary = TooltipCapture.GetPrioritySummary(
				TileCursor.Instance.Cell);
			if (summary == null) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TOOLTIP.NO_TOOLTIP);
				return;
			}
			SpeechPipeline.SpeakInterrupt(summary);
		}

		private void OpenTooltipBrowser() {
			if (!Grid.IsVisible(TileCursor.Instance.Cell)) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}
			var lines = TooltipCapture.GetTooltipLines();
			if (lines == null || lines.Count == 0) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TOOLTIP.NO_TOOLTIP);
				return;
			}
			HandlerStack.Push(new TooltipBrowserHandler(lines));
		}
	}
}
