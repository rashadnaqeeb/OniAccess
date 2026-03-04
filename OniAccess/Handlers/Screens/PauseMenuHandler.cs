using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the pause menu (PauseScreen class).
	/// PauseScreen inherits KModalButtonMenu (which inherits KButtonMenu), so we
	/// use the buttons array pattern: KButtonMenu.buttons provides ButtonInfo labels,
	/// KButtonMenu.buttonObjects provides the GameObjects with KButton components.
	///
	/// Per Pitfall 3: RefreshButtons() destroys cached references. The base class
	/// already calls DiscoverWidgets on OnActivate, so references are always fresh.
	/// </summary>
	public class PauseMenuHandler: BaseWidgetHandler {
		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.PAUSE_MENU;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public PauseMenuHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override void OnActivate() {
			base.OnActivate();
			try {
				string coords = CustomGameSettings.Instance.GetSettingsCoordinate();
				if (!string.IsNullOrEmpty(coords)) {
					Speech.SpeechPipeline.SpeakQueued(
						string.Format(STRINGS.UI.FRONTEND.PAUSE_SCREEN.WORLD_SEED, coords));
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"PauseMenuHandler: failed to read world seed: {ex}");
			}
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// KButtonMenu.buttons is an IList of ButtonInfo structs with .text labels
			var buttons = Traverse.Create(screen).Field("buttons")
				.GetValue<System.Collections.IList>();
			// KButtonMenu.buttonObjects is the array of instantiated GameObjects
			var buttonObjects = Traverse.Create(screen).Field("buttonObjects")
				.GetValue<UnityEngine.GameObject[]>();

			if (buttons == null || buttonObjects == null) return true;

			int count = System.Math.Min(buttons.Count, buttonObjects.Length);
			for (int i = 0; i < count; i++) {
				if (buttonObjects[i] == null || !buttonObjects[i].activeInHierarchy) continue;

				var kbutton = buttonObjects[i].GetComponent<KButton>();
				if (kbutton == null || !kbutton.isInteractable) continue;

				string label = Traverse.Create(buttons[i]).Field("text")
					.GetValue<string>();
				if (string.IsNullOrEmpty(label)) continue;

				_widgets.Add(new ButtonWidget {
					Label = label,
					Component = kbutton,
					GameObject = buttonObjects[i]
				});
			}

			Util.Log.Debug($"PauseMenuHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}
	}
}
