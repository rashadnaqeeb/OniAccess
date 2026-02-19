using System.Collections.Generic;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tiles {
	/// <summary>
	/// BaseScreenHandler bound to the Hud KScreen. Active when the game world
	/// is loaded and no modal menu is on top.
	///
	/// Routes arrow keys to TileCursor movement, backtick to coordinate
	/// reading, Shift+backtick to coordinate mode cycling.
	///
	/// CapturesAllInput = false: game hotkeys (overlays, tools, WASD camera,
	/// pause) pass through.
	/// </summary>
	public class TileCursorHandler : BaseScreenHandler {
		private Overlays.OverlayProfileRegistry _overlayRegistry;
		private bool _hasActivated;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.T),
			new ConsumedKey(KKeyCode.Q),
			new ConsumedKey(KKeyCode.BackQuote),
			new ConsumedKey(KKeyCode.BackQuote, Modifier.Shift),
			new ConsumedKey(KKeyCode.UpArrow),
			new ConsumedKey(KKeyCode.DownArrow),
			new ConsumedKey(KKeyCode.LeftArrow),
			new ConsumedKey(KKeyCode.RightArrow),
		};
		public override IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Arrow keys", (string)STRINGS.ONIACCESS.HELP.MOVE_CURSOR),
			new HelpEntry("T", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.OPEN_TOOL_MENU),
			new HelpEntry("Q", (string)STRINGS.ONIACCESS.HELP.READ_TOOLTIP),
			new HelpEntry("`", (string)STRINGS.ONIACCESS.HELP.READ_COORDS),
			new HelpEntry("Shift+`", (string)STRINGS.ONIACCESS.HELP.CYCLE_COORD_MODE),
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
			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged += OnOverlayChanged;
			if (Game.Instance != null)
				Game.Instance.Subscribe(1174281782, OnActiveToolChanged);
		}

		public override void OnDeactivate() {
			if (Game.Instance != null)
				Game.Instance.Unsubscribe(1174281782, OnActiveToolChanged);
			TileCursor.Destroy();
			if (OverlayScreen.Instance != null)
				OverlayScreen.Instance.OnOverlayChanged -= OnOverlayChanged;
		}

		private void OnOverlayChanged(HashedString newMode) {
			SpeechPipeline.SpeakInterrupt(_overlayRegistry.GetOverlayName(newMode));
		}

		public override void Tick() {
			string arrived = TileCursor.Instance.SyncToCamera();
			if (arrived != null)
				SpeechPipeline.SpeakInterrupt(arrived);

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
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.BackQuote)) {
				if (InputUtil.ShiftHeld())
					SpeechPipeline.SpeakInterrupt(TileCursor.Instance.CycleMode());
				else
					SpeechPipeline.SpeakInterrupt(TileCursor.Instance.ReadCoordinates());
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Q)
				&& !InputUtil.AnyModifierHeld()) {
				OpenTooltipBrowser();
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

		private void OnActiveToolChanged(object data) {
			var tool = data as InterfaceTool;
			if (tool == null || tool is SelectTool) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolHandler) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolPickerHandler) return;
			if (HandlerStack.ActiveHandler is OniAccess.Handlers.Tools.ToolFilterHandler) return;
			HandlerStack.Push(new OniAccess.Handlers.Tools.ToolHandler());
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
