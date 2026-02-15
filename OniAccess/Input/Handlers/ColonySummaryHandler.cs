using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for RetiredColonyInfoScreen (colony summary accessible from main menu
	/// and pause menu). Covers MENU-09 requirement.
	///
	/// The screen has two views:
	/// 1. Explorer view (explorerRoot): a grid of colony buttons showing past/retired colonies
	/// 2. Colony detail view (colonyDataRoot): stats, achievements, duplicants for a selected colony
	///
	/// Navigation:
	/// - Explorer view: Up/Down navigates colony entries, Enter opens colony detail
	/// - Detail view: Up/Down navigates stat/achievement entries, Escape returns to explorer
	/// - "View other colonies" button in detail view also returns to explorer
	/// </summary>
	public class ColonySummaryHandler: BaseMenuHandler {
		private const int SectionMain = 0;
		private const int SectionAchievements = 1;
		private const int SectionCount = 2;

		private bool _inColonyDetail;
		private int _currentSection;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.COLONY_SUMMARY;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ColonySummaryHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		public override void OnActivate() {
			_inColonyDetail = false;
			_currentSection = SectionMain;
			base.OnActivate();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (!_inColonyDetail) {
				if (_currentSection == SectionAchievements)
					DiscoverAchievementWidgets(screen);
				else
					DiscoverExplorerViewWidgets(screen);
			} else {
				if (_currentSection == SectionAchievements)
					DiscoverAchievementWidgets(screen);
				else
					DiscoverDetailViewWidgets(screen);
			}
			return true;
		}

		// ========================================
		// EXPLORER VIEW (colony list)
		// ========================================

		/// <summary>
		/// Discover colony entry buttons in the explorer view.
		/// Each colony entry in the explorerGrid has a KButton and LocText with the colony name.
		/// </summary>
		private void DiscoverExplorerViewWidgets(KScreen screen) {
			// Walk the explorerGrid to find colony entry buttons
			// explorerGrid is a GameObject in RetiredColonyInfoScreen
			var explorerGridGO = Traverse.Create(screen).Field("explorerGrid")
				.GetValue<UnityEngine.GameObject>();
			var explorerGrid = explorerGridGO != null ? explorerGridGO.transform : null;

			if (explorerGrid != null) {
				for (int i = 0; i < explorerGrid.childCount; i++) {
					var child = explorerGrid.GetChild(i);
					if (child == null || !child.gameObject.activeInHierarchy) continue;

					var kbutton = child.GetComponent<KButton>();
					if (kbutton == null) continue;

					// Find colony name from child LocText
					string colonyName = GetColonyEntryLabel(child);
					if (string.IsNullOrEmpty(colonyName)) continue;

					_widgets.Add(new WidgetInfo {
						Label = colonyName,
						Component = kbutton,
						Type = WidgetType.Button,
						GameObject = kbutton.gameObject
					});
				}
			}

			// Add close/navigation buttons at the end
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeScreenButton", (string)STRINGS.ONIACCESS.BUTTONS.CLOSE, _widgets);
		}

		// ========================================
		// DETAIL VIEW (colony stats)
		// ========================================

		/// <summary>
		/// Discover widgets in the colony detail view: colony header,
		/// stat blocks, and navigation buttons.
		/// </summary>
		private void DiscoverDetailViewWidgets(KScreen screen) {
			// Colony name header
			var colonyName = Traverse.Create(screen).Field("colonyName")
				.GetValue<LocText>();
			string header = colonyName != null ? colonyName.text : null;

			// Cycle count
			var cycleCount = Traverse.Create(screen).Field("cycleCount")
				.GetValue<LocText>();
			string cycles = cycleCount != null ? cycleCount.text : null;

			// Add header as a Label widget (colony name + cycle count)
			string headerText = header;
			if (!string.IsNullOrEmpty(cycles)) {
				headerText = string.IsNullOrEmpty(header)
					? cycles
					: $"{header}, {cycles}";
			}
			if (!string.IsNullOrEmpty(headerText)) {
				_widgets.Add(new WidgetInfo {
					Label = headerText,
					Component = null,
					Type = WidgetType.Label,
					GameObject = screen.gameObject
				});
			}

			// Find stat entries in statsContainer
			var statsContainerGO = Traverse.Create(screen).Field("statsContainer")
				.GetValue<UnityEngine.GameObject>();
			var statsContainer = statsContainerGO != null ? statsContainerGO.transform : null;
			if (statsContainer != null) {
				for (int i = 0; i < statsContainer.childCount; i++) {
					var child = statsContainer.GetChild(i);
					if (child == null || !child.gameObject.activeInHierarchy) continue;

					var locText = child.GetComponentInChildren<LocText>();
					if (locText == null || string.IsNullOrEmpty(locText.text)) continue;

					_widgets.Add(new WidgetInfo {
						Label = locText.text,
						Component = null,
						Type = WidgetType.Label,
						GameObject = child.gameObject
					});
				}
			}

			// Navigation buttons: "View other colonies" and "Close"
			WidgetDiscoveryUtil.TryAddButtonField(screen, "viewOtherColoniesButton", (string)STRINGS.ONIACCESS.BUTTONS.VIEW_OTHER_COLONIES, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeScreenButton", (string)STRINGS.ONIACCESS.BUTTONS.CLOSE, _widgets);
		}

		// ========================================
		// ACHIEVEMENTS VIEW
		// ========================================

		/// <summary>
		/// Discover achievement entries from the achievementsContainer.
		/// Inserts "Victory conditions" and "Achievements" headers to separate
		/// the two groups (victory conditions are placed first by the game).
		/// Texts are read live at speech time via GetWidgetSpeechText.
		/// </summary>
		private void DiscoverAchievementWidgets(KScreen screen) {
			var achievementsContainerGO = Traverse.Create(screen).Field("achievementsContainer")
				.GetValue<UnityEngine.GameObject>();
			var achievementsContainer = achievementsContainerGO != null
				? achievementsContainerGO.transform : null;
			if (achievementsContainer == null) return;

			// Build set of victory condition GameObjects via achievementEntries dict
			var victoryGOs = new HashSet<UnityEngine.GameObject>();
			var achievementEntries = Traverse.Create(screen).Field("achievementEntries")
				.GetValue<System.Collections.Generic.Dictionary<string, UnityEngine.GameObject>>();
			if (achievementEntries != null) {
				foreach (var kvp in achievementEntries) {
					var achievement = Db.Get().ColonyAchievements.TryGet(kvp.Key);
					if (achievement != null && achievement.isVictoryCondition)
						victoryGOs.Add(kvp.Value);
				}
			}

			bool addedVictoryHeader = false;
			for (int i = 0; i < achievementsContainer.childCount; i++) {
				var child = achievementsContainer.GetChild(i);
				if (child == null || !child.gameObject.activeInHierarchy) continue;

				if (!addedVictoryHeader && victoryGOs.Contains(child.gameObject)) {
					addedVictoryHeader = true;
					_widgets.Add(new WidgetInfo {
						Label = STRINGS.ONIACCESS.PANELS.VICTORY_CONDITIONS,
						Component = null,
						Type = WidgetType.Label,
						GameObject = screen.gameObject
					});
				}

				_widgets.Add(new WidgetInfo {
					Label = (string)STRINGS.ONIACCESS.INFO.ACHIEVEMENT,
					Component = null,
					Type = WidgetType.Label,
					GameObject = child.gameObject
				});
			}
		}

		// ========================================
		// VIEW TRANSITIONS
		// ========================================

		/// <summary>
		/// Override ActivateCurrentWidget to handle view transitions.
		/// When Enter is pressed on a colony in the explorer view, open colony detail.
		/// When "View other colonies" is pressed in detail view, return to explorer.
		/// </summary>
		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			if (!_inColonyDetail && widget.Type == WidgetType.Button) {
				// Check if this is a colony entry (not the close button)
				var kbutton = widget.Component as KButton;
				if (kbutton != null && widget.Label != (string)STRINGS.ONIACCESS.BUTTONS.CLOSE) {
					// Click the colony entry to open detail view
					kbutton.SignalClick(KKeyCode.Mouse0);
					_inColonyDetail = true;
					_currentSection = SectionMain;

					// Rediscover widgets for the detail view
					DiscoverWidgets(_screen);
					_currentIndex = 0;

					// Speak colony name and first stat
					Speech.SpeechPipeline.SpeakInterrupt(widget.Label);
					if (_widgets.Count > 0) {
						Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
					}
					return;
				}
			}

			if (_inColonyDetail && widget.Label == (string)STRINGS.ONIACCESS.BUTTONS.VIEW_OTHER_COLONIES) {
				// Return to explorer view
				ReturnToExplorerView();
				return;
			}

			// Default behavior for other buttons (e.g., Close)
			base.ActivateCurrentWidget();
		}

		/// <summary>
		/// Intercept Escape in colony detail view to go back to explorer
		/// instead of closing the screen.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e)) return true;

			if (_inColonyDetail && e.TryConsume(global::Action.Escape)) {
				ReturnToExplorerView();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Return from colony detail view to explorer view.
		/// Clicks "View other colonies" button to trigger the game's own transition,
		/// then rediscovers widgets and speaks the explorer view.
		/// </summary>
		private void ReturnToExplorerView() {
			// Click the viewOtherColoniesButton to trigger game transition
			var viewButton = Traverse.Create(_screen).Field("viewOtherColoniesButton")
				.GetValue<KButton>();
			if (viewButton != null) {
				viewButton.SignalClick(KKeyCode.Mouse0);
			}

			_inColonyDetail = false;
			_currentSection = SectionMain;
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
			}
		}

		// ========================================
		// TAB NAVIGATION (section switching)
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
			}
		}

		private static string GetSectionName(int section) {
			switch (section) {
				case SectionAchievements: return STRINGS.ONIACCESS.PANELS.ACHIEVEMENTS;
				default: return STRINGS.ONIACCESS.PANELS.STATS;
			}
		}

		// ========================================
		// WIDGET VALIDATION
		// ========================================

		// ========================================
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// For achievement widgets, read LocTexts live because the game
		/// populates them after our DiscoverWidgets runs.
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			if (_currentSection == SectionAchievements
				&& widget.GameObject != null
				&& widget.Label == (string)STRINGS.ONIACCESS.INFO.ACHIEVEMENT) {
				return ReadAchievementText(widget.GameObject.transform);
			}
			return base.GetWidgetSpeechText(widget);
		}

		/// <summary>
		/// Read achievement name and description live from HierarchyReferences.
		/// Uses GetParsedText() instead of .text because the game sets text via
		/// LocText.SetText() which updates TMP's internal char buffer but not m_text.
		/// </summary>
		private string ReadAchievementText(UnityEngine.Transform entry) {
			string name = null;
			string desc = null;

			var hierRef = entry.GetComponent<HierarchyReferences>();
			if (hierRef != null) {
				if (hierRef.HasReference("nameLabel")) {
					var nameLabel = hierRef.GetReference<LocText>("nameLabel");
					if (nameLabel != null)
						name = nameLabel.GetParsedText();
				}
				if (hierRef.HasReference("descriptionLabel")) {
					var descLabel = hierRef.GetReference<LocText>("descriptionLabel");
					if (descLabel != null)
						desc = descLabel.GetParsedText();
				}
			}

			if (string.IsNullOrEmpty(name)) return (string)STRINGS.ONIACCESS.INFO.ACHIEVEMENT;
			if (!string.IsNullOrEmpty(desc) && desc != name)
				return $"{name}, {desc}";
			return name;
		}

		// ========================================
		// UTILITY METHODS
		// ========================================

		/// <summary>
		/// Get the colony name label from an explorer grid entry.
		/// Colony entries have LocText children with the colony name.
		/// </summary>
		private string GetColonyEntryLabel(UnityEngine.Transform entry) {
			var locTexts = entry.GetComponentsInChildren<LocText>();
			if (locTexts != null && locTexts.Length > 0) {
				// First LocText is typically the colony name
				foreach (var lt in locTexts) {
					if (lt != null && !string.IsNullOrEmpty(lt.text))
						return lt.text;
				}
			}
			return null;
		}

	}
}
