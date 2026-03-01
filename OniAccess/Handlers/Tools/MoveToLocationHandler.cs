using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Handlers.Tiles;
using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Tools {
	/// <summary>
	/// Handler for MoveToLocationTool. Sits on top of TileCursorHandler
	/// (the game pops DetailsScreenHandler when the tool activates).
	/// CapturesAllInput = false so arrow keys fall through to TileCursorHandler.
	/// Space/Enter confirms the destination, Escape cancels.
	///
	/// On exit, sets DetailsScreenHandler.SuppressNextActivation so the
	/// details screen reopens silently (the player already knows which
	/// entity they're looking at).
	/// </summary>
	public class MoveToLocationHandler: IAccessHandler {
		public string DisplayName => BuildActivationName();
		public bool CapturesAllInput => false;

		private static readonly ConsumedKey[] _consumedKeys = {
			new ConsumedKey(KKeyCode.Space),
			new ConsumedKey(KKeyCode.Return),
		};
		public IReadOnlyList<ConsumedKey> ConsumedKeys => _consumedKeys;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Space/Enter", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.CONFIRM_TOOL),
			new HelpEntry("Escape", (string)STRINGS.ONIACCESS.HELP.TOOLS_HELP.CANCEL_TOOL),
		}.AsReadOnly();
		public IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public void OnActivate() {
			SpeechPipeline.SpeakInterrupt(DisplayName);
		}

		public void OnDeactivate() {
		}

		public bool Tick() {
			if ((UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)
				|| UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return))
				&& !InputUtil.AnyModifierHeld()) {
				TryConfirm();
				return true;
			}
			return false;
		}

		public bool HandleKeyDown(KButtonEvent e) {
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLS.CANCELED);
				PlayCancelSound();
				ExitTool();
				return true;
			}
			return false;
		}

		private void TryConfirm() {
			var tool = MoveToLocationTool.Instance;
			if (tool == null) {
				ExitTool();
				return;
			}

			int cell = TileCursor.Instance.Cell;
			if (!tool.CanMoveTo(cell)) {
				BaseScreenHandler.PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.TOOLS.MOVE_TO_UNREACHABLE);
				return;
			}

			Traverse.Create(tool).Method("SetMoveToLocation", cell).GetValue(cell);
			PlayConfirmSound();
			ExitTool();
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.TOOLS.MOVE_TO_CONFIRMED);
		}

		private static void ExitTool() {
			Screens.DetailsScreenHandler.SuppressNextActivation = true;
			HandlerStack.Pop();
			SelectTool.Instance.Activate();
		}

		private static string BuildActivationName() {
			var tool = MoveToLocationTool.Instance;
			if (tool == null)
				return Strings.Get("STRINGS.UI.TOOLS.MOVETOLOCATION.TOOLNAME");

			string entityName = null;
			var nav = Traverse.Create(tool).Field("targetNavigator").GetValue<Navigator>();
			if (nav != null) {
				var sel = nav.gameObject.GetComponent<KSelectable>();
				entityName = sel?.GetName();
			} else {
				var movable = Traverse.Create(tool).Field("targetMovable").GetValue<Movable>();
				if (movable != null) {
					var sel = movable.gameObject.GetComponent<KSelectable>();
					entityName = sel?.GetName();
				}
			}

			if (string.IsNullOrEmpty(entityName))
				return Strings.Get("STRINGS.UI.TOOLS.MOVETOLOCATION.TOOLNAME");
			return string.Format(
				(string)STRINGS.ONIACCESS.TOOLS.MOVE_TO_ACTIVATION, entityName);
		}

		private static void PlayConfirmSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click"));
			} catch (System.Exception ex) {
				Util.Log.Error($"MoveToLocationHandler.PlayConfirmSound: {ex.Message}");
			}
		}

		private static void PlayCancelSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Tile_Cancel"));
			} catch (System.Exception ex) {
				Util.Log.Error($"MoveToLocationHandler.PlayCancelSound: {ex.Message}");
			}
		}
	}
}
