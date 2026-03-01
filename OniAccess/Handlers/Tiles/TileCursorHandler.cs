using System.Collections.Generic;
using OniAccess.Handlers.Notifications;
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
		private CursorBookmarks _bookmarks;
		private GameStateMonitor _monitor;
		private NotificationTracker _notificationTracker;
		private NotificationAnnouncer _notificationAnnouncer;
		private bool _hasActivated;
		private bool _overlaySubscribed;
		private int _queueNextOverlayTtl;
		private HashedString _lastOverlayMode;

		public void QueueNextOverlayAnnouncement() => _queueNextOverlayTtl = 2;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Tab),
			new ConsumedKey(KKeyCode.BackQuote),
			new ConsumedKey(KKeyCode.F, Modifier.Ctrl),
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
			new ConsumedKey(KKeyCode.Home, Modifier.Shift),
			new ConsumedKey(KKeyCode.PageUp, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.PageDown, Modifier.Ctrl),
			new ConsumedKey(KKeyCode.PageUp, Modifier.Shift),
			new ConsumedKey(KKeyCode.PageDown, Modifier.Shift),
			new ConsumedKey(KKeyCode.PageUp),
			new ConsumedKey(KKeyCode.PageDown),
			new ConsumedKey(KKeyCode.PageUp, Modifier.Alt),
			new ConsumedKey(KKeyCode.PageDown, Modifier.Alt),
			new ConsumedKey(KKeyCode.Q),
			new ConsumedKey(KKeyCode.Return),
			new ConsumedKey(KKeyCode.N, Modifier.Shift),
			// Bookmark keybinds
			new ConsumedKey(KKeyCode.H),
			new ConsumedKey(KKeyCode.Alpha1, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha2, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha3, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha4, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha5, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha6, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha7, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha8, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha9, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha0, Modifier.Shift),
			new ConsumedKey(KKeyCode.Alpha1, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha2, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha3, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha4, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha5, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha6, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha7, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha8, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha9, Modifier.Alt),
			new ConsumedKey(KKeyCode.Alpha0, Modifier.Alt),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Arrow keys", (string)STRINGS.ONIACCESS.HELP.MOVE_CURSOR),
			new HelpEntry("Tab", (string)STRINGS.ONIACCESS.BUILD_MENU.HELP_OPEN_ACTION_MENU),
			new HelpEntry("Enter", (string)STRINGS.ONIACCESS.HELP.SELECT_ENTITY),
			new HelpEntry("I", (string)STRINGS.ONIACCESS.HELP.READ_TOOLTIP_SUMMARY),
			new HelpEntry("Shift+I", (string)STRINGS.ONIACCESS.HELP.READ_TOOLTIP),
			new HelpEntry("K", (string)STRINGS.ONIACCESS.HELP.READ_COORDS),
			new HelpEntry("Shift+K", (string)STRINGS.ONIACCESS.HELP.CYCLE_COORD_MODE),
			new HelpEntry("End", (string)STRINGS.ONIACCESS.SCANNER.HELP.REFRESH),
			new HelpEntry("Home", (string)STRINGS.ONIACCESS.SCANNER.HELP.TELEPORT),
			new HelpEntry("Shift+Home", (string)STRINGS.ONIACCESS.SCANNER.HELP.TOGGLE_AUTO_MOVE),
			new HelpEntry("Ctrl+PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_CATEGORY),
			new HelpEntry("Shift+PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_SUBCATEGORY),
			new HelpEntry("PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_ITEM),
			new HelpEntry("Alt+PageUp/Down", (string)STRINGS.ONIACCESS.SCANNER.HELP.CYCLE_INSTANCE),
			new HelpEntry("Ctrl+F", (string)STRINGS.ONIACCESS.SCANNER.HELP.SEARCH),
			new HelpEntry("Q", (string)STRINGS.ONIACCESS.GAME_STATE.READ_CYCLE_STATUS),
			new HelpEntry("`", (string)STRINGS.ONIACCESS.HELP.CYCLE_GAME_SPEED),
			new HelpEntry("Shift+N", (string)STRINGS.ONIACCESS.NOTIFICATIONS.OPEN_MENU_HELP),
			new HelpEntry("H", (string)STRINGS.ONIACCESS.BOOKMARKS.HELP_HOME),
			new HelpEntry("Ctrl+1-0", (string)STRINGS.ONIACCESS.BOOKMARKS.HELP_SET_BOOKMARK),
			new HelpEntry("Shift+1-0", (string)STRINGS.ONIACCESS.BOOKMARKS.HELP_GOTO_BOOKMARK),
			new HelpEntry("Alt+1-0", (string)STRINGS.ONIACCESS.BOOKMARKS.HELP_ORIENT_BOOKMARK),
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
				ToolProfiles.ToolProfileRegistry.Build();
				TileCursor.Create(_overlayRegistry);
				_scanner = new ScannerNavigator();
				_bookmarks = new CursorBookmarks();
				_monitor = new GameStateMonitor();
				if (NotificationManager.Instance != null) {
					_notificationTracker = new NotificationTracker();
					_notificationTracker.Attach();
					_notificationAnnouncer = new NotificationAnnouncer(_notificationTracker);
				}
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
				_lastOverlayMode = OverlayScreen.Instance.mode;
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
			_notificationAnnouncer?.Detach();
			_notificationAnnouncer = null;
			_notificationTracker?.Detach();
			_notificationTracker = null;
			TileCursor.Destroy();
			_scanner = null;
			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged -= OnOverlayChanged;
			_overlaySubscribed = false;
		}

		private void OnOverlayChanged(HashedString newMode) {
			TileCursor.Instance.ResetRoomName();
			// Skip redundant announcements when the game resets the overlay to None
			// (e.g., ManagementMenu.ToggleScreen resets overlay before opening a screen).
			if (newMode == _lastOverlayMode) return;
			_lastOverlayMode = newMode;
			if (_queueNextOverlayTtl > 0) {
				_queueNextOverlayTtl = 0;
				SpeechPipeline.SpeakQueued(_overlayRegistry.GetOverlayName(newMode));
			} else {
				SpeechPipeline.SpeakInterrupt(_overlayRegistry.GetOverlayName(newMode));
			}
		}

		public override bool Tick() {
			if (_queueNextOverlayTtl > 0)
				_queueNextOverlayTtl--;

			if (!_overlaySubscribed && OverlayScreen.Instance != null) {
				OverlayScreen.Instance.OnOverlayChanged += OnOverlayChanged;
				_overlaySubscribed = true;
			}

			_scanner.CheckWorldSwitch();
			_monitor.Tick();
			_notificationAnnouncer?.Tick();

			string arrived = TileCursor.Instance.SyncToCamera();
			if (arrived != null)
				SpeechPipeline.SpeakInterrupt(arrived);

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)
				&& !InputUtil.AnyModifierHeld()) {
				OpenEntityPicker();
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)
				&& !InputUtil.AnyModifierHeld()) {
				OpenActionMenu();
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.BackQuote)
				&& !InputUtil.AnyModifierHeld()) {
				if (SpeedControlScreen.Instance != null) {
					var scs = SpeedControlScreen.Instance;
					int newSpeed = (scs.GetSpeed() + 1) % 3;
					PlaySpeedChangeSound(newSpeed + 1);
					scs.SetSpeed(newSpeed);
					scs.OnSpeedChange();
				}
				return true;
			}

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Up);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Down);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Left);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)
				&& !InputUtil.AnyModifierHeld()) {
				SpeakMove(Direction.Right);
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.K)) {
				if (InputUtil.ShiftHeld())
					SpeechPipeline.SpeakInterrupt(TileCursor.Instance.CycleMode());
				else
					SpeechPipeline.SpeakInterrupt(TileCursor.Instance.ReadCoordinates());
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)) {
				if (InputUtil.ShiftHeld())
					OpenTooltipBrowser();
				else
					ReadTooltipSummary();
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Q)
				&& !InputUtil.AnyModifierHeld()) {
				_monitor.SpeakCycleStatus();
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.N)
				&& InputUtil.ShiftHeld()) {
				OpenNotificationMenu();
				return true;
			}

			// Bookmark keybinds
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.H)
				&& !InputUtil.AnyModifierHeld()) {
				SpeechPipeline.SpeakInterrupt(CursorBookmarks.JumpHome());
				return true;
			}
			for (var kc = UnityEngine.KeyCode.Alpha0; kc <= UnityEngine.KeyCode.Alpha9; kc++) {
				if (!UnityEngine.Input.GetKeyDown(kc)) continue;
				int idx = CursorBookmarks.DigitKeyToIndex(kc);
				if (InputUtil.ShiftHeld()) {
					SpeechPipeline.SpeakInterrupt(_bookmarks.Goto(idx));
					return true;
				}
				if (InputUtil.AltHeld()) {
					SpeechPipeline.SpeakInterrupt(_bookmarks.Orient(idx));
					return true;
				}
				if (InputUtil.CtrlHeld()) {
					SpeechPipeline.SpeakQueued(string.Format(
						(string)STRINGS.ONIACCESS.BOOKMARKS.BOOKMARK_SET, idx + 1));
					return false;
				}
			}

			// Scanner keybinds
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F)
				&& InputUtil.CtrlHeld()) {
				HandlerStack.Push(new SearchInputHandler(_scanner));
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.End)
				&& !InputUtil.AnyModifierHeld()) {
				_scanner.Refresh();
				return true;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Home)) {
				if (InputUtil.ShiftHeld()) {
					SpeechPipeline.SpeakInterrupt(_scanner.ToggleAutoMove());
					return true;
				}
				if (!InputUtil.AnyModifierHeld()) {
					_scanner.Teleport();
					return true;
				}
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
				return true;
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
				return true;
			}
			return false;
		}

		private void SpeakMove(Direction direction) {
			string speech = TileCursor.Instance.Move(direction);
			if (speech != null)
				SpeechPipeline.SpeakInterrupt(speech);
		}

		private void OpenActionMenu() {
			if (!(PlayerController.Instance.ActiveTool is SelectTool))
				SelectTool.Instance.Activate();
			HandlerStack.Push(new Build.ActionMenuHandler());
		}

		private void OpenEntityPicker() {
			int cell = TileCursor.Instance.Cell;
			if (!Grid.IsVisible(cell)) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TILE_CURSOR.UNEXPLORED);
				return;
			}
			var selectables = EntityPickerHandler.CollectSelectables(cell);
			if (selectables.Count == 0) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TILE_CURSOR.NOTHING_TO_SELECT);
				return;
			}
			if (selectables.Count == 1) {
				if (!(PlayerController.Instance.ActiveTool is SelectTool))
					SelectTool.Instance.Activate();
				SelectTool.Instance.Select(selectables[0]);
				return;
			}
			HandlerStack.Push(new EntityPickerHandler(selectables));
		}

		private void OpenNotificationMenu() {
			if (_notificationTracker == null) return;
			if (_notificationTracker.Groups.Count == 0) {
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.NOTIFICATIONS.EMPTY);
				return;
			}
			HandlerStack.Push(
				new Notifications.NotificationMenuHandler(_notificationTracker));
		}

		private void OnActiveToolChanged(object data) {
			var tool = data as InterfaceTool;
			if (tool == null || tool is SelectTool) return;
			if (tool is BuildTool || tool is UtilityBuildTool || tool is WireBuildTool) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolHandler) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolFilterHandler) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.MoveToLocationHandler) return;
			if (HandlerStack.ActiveHandler is Build.BuildToolHandler) return;
			if (HandlerStack.ActiveHandler is Build.ActionMenuHandler) return;
			if (tool is MoveToLocationTool) {
				HandlerStack.Push(new OniAccess.Handlers.Tools.MoveToLocationHandler());
				return;
			}
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

		private static void PlaySpeedChangeSound(float speed) {
			string sound = GlobalAssets.GetSound("Speed_Change");
			if (sound != null) {
				var instance = SoundEvent.BeginOneShot(sound, UnityEngine.Vector3.zero);
				instance.setParameterByName("Speed", speed);
				SoundEvent.EndOneShot(instance);
			}
		}
	}
}
