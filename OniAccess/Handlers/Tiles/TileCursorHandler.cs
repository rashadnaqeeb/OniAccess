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
		private readonly TileCursor _cursor = new TileCursor();
		private bool _hasActivated;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Arrow keys", (string)STRINGS.ONIACCESS.HELP.MOVE_CURSOR),
			new HelpEntry("`", (string)STRINGS.ONIACCESS.HELP.READ_COORDS),
			new HelpEntry("Shift+`", (string)STRINGS.ONIACCESS.HELP.CYCLE_COORD_MODE),
		}.AsReadOnly();

		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.COLONY_VIEW;
		public override bool CapturesAllInput => false;
		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public TileCursorHandler(KScreen screen) : base(screen) {
		}

		public override void OnActivate() {
			if (_hasActivated) return;
			_hasActivated = true;
			SpeechPipeline.SpeakInterrupt(DisplayName);
			try {
				_cursor.Initialize();
			} catch (System.Exception ex) {
				Util.Log.Error($"TileCursorHandler.OnActivate: cursor init failed: {ex}");
			}
		}

		public override void OnDeactivate() {
			KInputManager.isMousePosLocked = false;
		}

		public override void Tick() {
			_cursor.SyncToCamera();

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
					SpeechPipeline.SpeakInterrupt(_cursor.CycleMode());
				else
					SpeechPipeline.SpeakInterrupt(_cursor.ReadCoordinates());
				return;
			}
		}

		private void SpeakMove(Direction direction) {
			string speech = _cursor.Move(direction);
			if (speech != null)
				SpeechPipeline.SpeakInterrupt(speech);
		}
	}
}
