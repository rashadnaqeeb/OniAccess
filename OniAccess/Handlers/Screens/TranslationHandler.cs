using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KMod;
using OniAccess.Util;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for LanguageOptionsScreen: language/translation selection from the options menu.
	///
	/// Discovers language buttons from the screen's private `buttons` list, which contains
	/// only real dynamically-created buttons (not stale prefab templates that also live in
	/// the containers). Action buttons (Workshop, Uninstall, Dismiss, Close) are separate
	/// serialized fields.
	///
	/// Language buttons are dynamically created in OnSpawn, which runs after our Harmony
	/// postfix on Activate — BaseMenuHandler's deferred rediscovery handles this automatically.
	///
	/// All widgets are KButtons, so base IsWidgetValid and ActivateCurrentItem handle
	/// them correctly. GetWidgetSpeechText is overridden to prefix "selected" on the
	/// active language.
	/// </summary>
	public class TranslationHandler: BaseWidgetHandler {
		public override string DisplayName => (string)STRINGS.ONIACCESS.HANDLERS.TRANSLATIONS;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public TranslationHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			// Determine which button name corresponds to the active language.
			string selectedButtonName = GetSelectedButtonName();

			// Use the screen's private buttons list — contains only the real
			// dynamically-created language buttons, not stale prefab templates.
			// Order is preinstalled first, then UGC (workshop) language packs.
			var buttonsList = Traverse.Create(screen)
				.Field<List<UnityEngine.GameObject>>("buttons").Value;

			if (buttonsList != null) {
				foreach (var go in buttonsList) {
					if (go == null || !go.activeInHierarchy) continue;

					var kbutton = go.GetComponent<KButton>();
					if (kbutton == null) continue;

					var hierRef = go.GetComponent<HierarchyReferences>();
					string label = null;
					if (hierRef != null && hierRef.HasReference("Title")) {
						var titleText = hierRef.GetReference<LocText>("Title");
						if (titleText != null)
							label = titleText.text;
					}

					if (string.IsNullOrEmpty(label))
						label = go.name;

					bool isSelected = selectedButtonName != null && go.name == selectedButtonName;
					string widgetLabel = label;
					_widgets.Add(new ButtonWidget {
						Label = widgetLabel,
						Component = kbutton,
						GameObject = go,
						Tag = isSelected,
						SpeechFunc = isSelected
							? () => $"{STRINGS.ONIACCESS.STATES.SELECTED}, {widgetLabel}"
							: (System.Func<string>)null
					});
				}
			}

			// Append action buttons
			WidgetDiscoveryUtil.TryAddButtonField(screen, "workshopButton", null, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "uninstallButton", null, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "dismissButton", null, _widgets);

			Log.Debug($"TranslationHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		/// <summary>
		/// Returns the go.name of the button representing the currently active language,
		/// or null if it cannot be determined.
		/// Preinstalled buttons are named "{code}_button", UGC buttons "{mod.title}_button".
		/// </summary>
		private string GetSelectedButtonName() {
			var langType = Localization.GetSelectedLanguageType();
			switch (langType) {
				case Localization.SelectedLanguageType.None:
				case Localization.SelectedLanguageType.Preinstalled: {
						var code = Localization.GetCurrentLanguageCode();
						return !string.IsNullOrEmpty(code) ? code + "_button" : null;
					}
				case Localization.SelectedLanguageType.UGC: {
						var modId = LanguageOptionsScreen.GetSavedLanguageMod();
						if (modId == null) return null;
						var mod = Global.Instance.modManager.mods
							.FirstOrDefault(m => m.label.id == modId);
						return mod != null ? mod.title + "_button" : null;
					}
				default:
					return null;
			}
		}

	}
}
