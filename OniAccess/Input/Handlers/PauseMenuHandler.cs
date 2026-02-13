using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for the pause menu (PauseScreen class).
	/// PauseScreen inherits KModalButtonMenu (which inherits KButtonMenu), so we
	/// use the buttons array pattern: KButtonMenu.buttons provides ButtonInfo labels,
	/// KButtonMenu.buttonObjects provides the GameObjects with KButton components.
	///
	/// Per Pitfall 3: RefreshButtons() destroys cached references. The base class
	/// already calls DiscoverWidgets on OnActivate, so references are always fresh.
	/// </summary>
	public class PauseMenuHandler: BaseMenuHandler {
		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.PAUSE_MENU;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public PauseMenuHandler(KScreen screen) : base(screen) {
			var entries = new List<HelpEntry>();
			entries.AddRange(CommonHelpEntries);
			entries.AddRange(MenuHelpEntries);
			entries.AddRange(ListNavHelpEntries);
			HelpEntries = entries;
		}

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// KButtonMenu.buttons is an IList of ButtonInfo structs with .text labels
			var buttons = Traverse.Create(screen).Field("buttons")
				.GetValue<System.Collections.IList>();
			// KButtonMenu.buttonObjects is the array of instantiated GameObjects
			var buttonObjects = Traverse.Create(screen).Field("buttonObjects")
				.GetValue<UnityEngine.GameObject[]>();

			if (buttons == null || buttonObjects == null) return;

			int count = System.Math.Min(buttons.Count, buttonObjects.Length);
			for (int i = 0; i < count; i++) {
				if (buttonObjects[i] == null || !buttonObjects[i].activeInHierarchy) continue;

				var kbutton = buttonObjects[i].GetComponent<KButton>();
				if (kbutton == null || !kbutton.isInteractable) continue;

				// Access ButtonInfo.text via Traverse (it's a property on the ButtonInfo struct)
				string label = Traverse.Create(buttons[i]).Property("text")
					.GetValue<string>();
				if (string.IsNullOrEmpty(label)) continue;

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = kbutton,
					Type = WidgetType.Button,
					GameObject = buttonObjects[i]
				});
			}
		}
	}
}
