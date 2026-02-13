using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for the main menu (MainMenu class).
	/// MainMenu inherits directly from KScreen (NOT KButtonMenu), so we cannot
	/// use the buttons array pattern. Instead, we walk the buttonParent transform
	/// to discover KButton instances with LocText labels.
	///
	/// Also checks the Button_ResumeGame serialized field, which is separate from
	/// the MakeButton buttons and appears only when a save file exists.
	/// </summary>
	public class MainMenuHandler: BaseMenuHandler {
		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.MAIN_MENU;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public MainMenuHandler(KScreen screen) : base(screen) {
			var entries = new List<HelpEntry>();
			entries.AddRange(MenuHelpEntries);
			entries.AddRange(ListNavHelpEntries);
			HelpEntries = entries;
		}

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// MainMenu has a separate Button_ResumeGame field (shown if a save exists)
			var resumeButton = Traverse.Create(screen).Field("Button_ResumeGame")
				.GetValue<KButton>();
			if (resumeButton != null && resumeButton.gameObject.activeInHierarchy
				&& resumeButton.isInteractable) {
				var resumeLabel = resumeButton.GetComponentInChildren<LocText>();
				string resumeText = resumeLabel != null ? resumeLabel.text : "Resume Game";
				_widgets.Add(new WidgetInfo {
					Label = resumeText,
					Component = resumeButton,
					Type = WidgetType.Button,
					GameObject = resumeButton.gameObject
				});
			}

			// Walk buttonParent children for MakeButton-created buttons
			// buttonParent is a GameObject (not Transform) per decompiled source
			var buttonParentGO = Traverse.Create(screen).Field("buttonParent")
				.GetValue<UnityEngine.GameObject>();
			UnityEngine.Transform parent = buttonParentGO != null
				? buttonParentGO.transform
				: screen.transform;

			for (int i = 0; i < parent.childCount; i++) {
				var child = parent.GetChild(i);
				if (child == null || !child.gameObject.activeInHierarchy) continue;

				var kbutton = child.GetComponent<KButton>();
				if (kbutton == null || !kbutton.isInteractable) continue;

				// Skip if this is the resume button (already added above)
				if (resumeButton != null && kbutton == resumeButton) continue;

				var locText = kbutton.GetComponentInChildren<LocText>();
				if (locText == null || string.IsNullOrEmpty(locText.text)) continue;

				_widgets.Add(new WidgetInfo {
					Label = locText.text,
					Component = kbutton,
					Type = WidgetType.Button,
					GameObject = kbutton.gameObject
				});
			}

			Util.Log.Debug($"MainMenuHandler.DiscoverWidgets: {_widgets.Count} widgets");
		}
	}
}
