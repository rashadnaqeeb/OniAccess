using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for the main menu (MainMenu class).
	/// MainMenu inherits directly from KScreen (NOT KButtonMenu), so we cannot
	/// use the buttons array pattern. Instead, we walk the buttonParent transform
	/// to discover KButton instances with LocText labels.
	///
	/// Also checks the Button_ResumeGame serialized field, which is separate from
	/// the MakeButton buttons and appears only when a save file exists.
	///
	/// Three sections reachable via Tab/Shift+Tab:
	/// - Buttons: the main menu button list (Resume, New Game, Load, etc.)
	/// - DLC: 4 DLC logos showing name + ownership/activation status
	/// - News: MOTD boxes with headlines from Klei's server (async-loaded)
	/// </summary>
	public class MainMenuHandler: BaseWidgetHandler {
		private const int SectionButtons = 0;
		private const int SectionDLC = 1;
		private const int SectionNews = 2;
		private const int SectionCount = 3;

		private int _currentSection;

		private static readonly string[] DlcFieldNames = { "logoDLC1", "logoDLC2", "logoDLC3", "logoDLC4" };
		private static readonly string[] DlcIds = { "EXPANSION1_ID", "DLC2_ID", "DLC3_ID", "DLC4_ID" };
		private static readonly string[] MotdBoxFields = { "boxA", "boxB", "boxC" };

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.MAIN_MENU;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public MainMenuHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			switch (_currentSection) {
				case SectionButtons: DiscoverButtonWidgets(screen); break;
				case SectionDLC: DiscoverDLCWidgets(screen); break;
				case SectionNews: DiscoverNewsWidgets(screen); break;
			}

			Util.Log.Debug($"MainMenuHandler.DiscoverWidgets: section={_currentSection}, {_widgets.Count} widgets");
			return true;
		}

		/// <summary>
		/// Discover the main menu buttons (Resume Game + MakeButton-created buttons).
		/// </summary>
		private void DiscoverButtonWidgets(KScreen screen) {
			// MainMenu has a separate Button_ResumeGame field (shown if a save exists)
			var resumeButton = Traverse.Create(screen).Field("Button_ResumeGame")
				.GetValue<KButton>();
			if (resumeButton != null && resumeButton.gameObject.activeInHierarchy
				&& resumeButton.isInteractable) {
				var resumeLabel = resumeButton.GetComponentInChildren<LocText>();
				string resumeText = resumeLabel != null ? resumeLabel.text : (string)STRINGS.UI.FRONTEND.MAINMENU.RESUMEGAME;
				_widgets.Add(new ButtonWidget {
					Label = resumeText,
					Component = resumeButton,
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

				_widgets.Add(new ButtonWidget {
					Label = locText.text,
					Component = kbutton,
					GameObject = kbutton.gameObject
				});
			}
		}

		/// <summary>
		/// Discover DLC logo entries. Each has a name (from DlcManager) and
		/// ownership/activation status.
		/// </summary>
		private void DiscoverDLCWidgets(KScreen screen) {
			var screenTraverse = Traverse.Create(screen);

			for (int i = 0; i < DlcFieldNames.Length; i++) {
				var hierRef = screenTraverse.Field(DlcFieldNames[i])
					.GetValue<HierarchyReferences>();
				if (hierRef == null) continue;
				if (!hierRef.gameObject.activeInHierarchy) continue;

				string dlcId = DlcIds[i];
				string name = DlcManager.GetDlcTitleNoFormatting(dlcId);
				string status = GetDlcStatus(dlcId);

				_widgets.Add(new LabelWidget {
					Label = $"{name}, {status}",
					GameObject = hierRef.gameObject,
					Tag = DlcFieldNames[i]
				});
			}
		}

		/// <summary>
		/// Get the DLC status string. Uses our own strings because the game's
		/// CONTENT_OWNED_NOTINSTALLED_LABEL is an empty string.
		/// </summary>
		private static string GetDlcStatus(string dlcId) {
			if (DlcManager.IsContentSubscribed(dlcId))
				return STRINGS.ONIACCESS.DLC.ACTIVE;
			if (DlcManager.IsContentOwned(dlcId))
				return STRINGS.ONIACCESS.DLC.OWNED_NOT_ACTIVE;
			return STRINGS.ONIACCESS.DLC.NOT_OWNED;
		}

		/// <summary>
		/// Discover MOTD news boxes. These are fetched async from Klei's server,
		/// so boxes may not be active yet (data still loading).
		/// </summary>
		private void DiscoverNewsWidgets(KScreen screen) {
			var motd = Traverse.Create(screen).Field("motd").GetValue<object>();
			if (motd == null) return;

			var motdTraverse = Traverse.Create(motd);

			foreach (var boxField in MotdBoxFields) {
				var box = motdTraverse.Field(boxField).GetValue<MotdBox>();
				if (box == null || !box.gameObject.activeInHierarchy) continue;

				var boxTraverse = Traverse.Create(box);
				var headerLabel = boxTraverse.Field("headerLabel").GetValue<LocText>();
				var imageLabel = boxTraverse.Field("imageLabel").GetValue<LocText>();

				// Use GetParsedText() instead of .text because SetText() updates
				// TMP's internal char buffer but not m_text.
				string header = headerLabel != null ? headerLabel.GetParsedText() : null;
				if (string.IsNullOrEmpty(header)) continue;

				string body = null;
				if (imageLabel != null && imageLabel.gameObject.activeInHierarchy)
					body = imageLabel.GetParsedText();

				string label = !string.IsNullOrEmpty(body)
					? $"{header}. {body}"
					: header;

				_widgets.Add(new LabelWidget {
					Label = label,
					Component = box,
					GameObject = box.gameObject
				});
			}
		}

		// ========================================
		// TAB NAVIGATION
		// ========================================

		protected override void NavigateTabForward() {
			_currentSection = (_currentSection + 1) % SectionCount;
			if (_currentSection == 0) PlayWrapSound();
			RediscoverForCurrentSection();
		}

		protected override void NavigateTabBackward() {
			int prev = _currentSection;
			_currentSection = (_currentSection - 1 + SectionCount) % SectionCount;
			if (_currentSection == SectionCount - 1 && prev == 0) PlayWrapSound();
			RediscoverForCurrentSection();
		}

		private void RediscoverForCurrentSection() {
			DiscoverWidgets(_screen);
			string sectionName = GetSectionName(_currentSection);
			Speech.SpeechPipeline.SpeakInterrupt(sectionName);
			if (_widgets.Count > 0) {
				_currentIndex = 0;
				Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
			} else if (_currentSection == SectionNews) {
				Speech.SpeechPipeline.SpeakQueued(STRINGS.ONIACCESS.PANELS.NO_NEWS);
			}
		}

		private static string GetSectionName(int section) {
			switch (section) {
				case SectionDLC: return STRINGS.ONIACCESS.PANELS.DLC;
				case SectionNews: return STRINGS.ONIACCESS.PANELS.NEWS;
				default: return STRINGS.ONIACCESS.PANELS.BUTTONS;
			}
		}

		// ========================================
		// WIDGET INTERACTION
		// ========================================

		/// <summary>
		/// DLC section: re-fetch the MultiToggle from the screen's HierarchyReferences
		/// and fire OnPointerClick to trigger the game-wired onClick delegate.
		/// DLC1 owned: opens activate/deactivate dialog. DLC1 not owned: opens store.
		/// DLC2-4: always opens store page.
		/// News section: click the URLOpenFunction's triggerButton to open in browser.
		/// Buttons section: default KButton.SignalClick behavior.
		/// </summary>
		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (_currentSection == SectionDLC && widget.Tag is string dlcFieldName) {
				var hierRef = Traverse.Create(_screen).Field(dlcFieldName)
					.GetValue<HierarchyReferences>();
				if (hierRef != null) {
					var multiToggle = hierRef.GetReference<MultiToggle>("multitoggle");
					if (multiToggle != null)
						ClickMultiToggle(multiToggle);
				}
				return;
			}

			if (_currentSection == SectionNews) {
				var box = widget.Component as MotdBox;
				if (box != null) {
					var urlOpener = Traverse.Create(box).Field("urlOpener")
						.GetValue<URLOpenFunction>();
					if (urlOpener != null) {
						var triggerButton = Traverse.Create(urlOpener).Field("triggerButton")
							.GetValue<KButton>();
						if (triggerButton != null)
							ClickButton(triggerButton);
					}
				}
				return;
			}

			base.ActivateCurrentItem();
		}

		// ========================================
		// WIDGET VALIDITY
		// ========================================

	}
}
