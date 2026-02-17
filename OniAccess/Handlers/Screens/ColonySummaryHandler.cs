using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
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
	/// - Detail view: Up/Down navigates entries within current section, Tab switches sections
	/// - Escape returns from detail to explorer, or closes the screen from explorer
	///
	/// Detail view sections (Tab navigation):
	/// 0 = Duplicants, 1 = Buildings, 2 = Statistics, 3 = Achievements
	/// Explorer view sections: 0 = Colonies, 1 = Achievements
	///
	/// Duplicant, building, and stat widgets use live reading at speech time
	/// because the game populates LocTexts via SetText() which updates TMP's
	/// internal buffer but not m_text. GetParsedText() needs a frame to catch up.
	/// </summary>
	public class ColonySummaryHandler: BaseMenuHandler {
		private const int ExplorerSectionMain = 0;
		private const int ExplorerSectionAchievements = 1;
		private const int ExplorerSectionCount = 2;

		private const int DetailSectionDuplicants = 0;
		private const int DetailSectionBuildings = 1;
		private const int DetailSectionStats = 2;
		private const int DetailSectionAchievements = 3;
		private const int DetailSectionCount = 4;

		private bool _inColonyDetail;
		private int _currentSection;
		private KButton _viewOtherColoniesButton;

		protected override int MaxDiscoveryRetries => 5;

		private int SectionCount => _inColonyDetail ? DetailSectionCount : ExplorerSectionCount;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.COLONY_SUMMARY;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ColonySummaryHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		public override void OnActivate() {
			_inColonyDetail = false;
			_currentSection = ExplorerSectionMain;
			CacheButtons();
			base.OnActivate();
		}

		private void CacheButtons() {
			try {
				_viewOtherColoniesButton = Traverse.Create(_screen).Field("viewOtherColoniesButton")
					.GetValue<KButton>();
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.CacheButtons: {ex.Message}");
			}
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (!_inColonyDetail) {
				if (_currentSection == ExplorerSectionAchievements)
					DiscoverAchievementWidgets(screen);
				else
					DiscoverExplorerViewWidgets(screen);
			} else {
				switch (_currentSection) {
					case DetailSectionDuplicants:
						DiscoverDuplicantWidgets(screen);
						break;
					case DetailSectionBuildings:
						DiscoverBuildingWidgets(screen);
						break;
					case DetailSectionStats:
						DiscoverStatWidgets(screen);
						break;
					case DetailSectionAchievements:
						DiscoverAchievementWidgets(screen);
						break;
				}
			}
			return _widgets.Count > 0;
		}

		// ========================================
		// EXPLORER VIEW (colony list)
		// ========================================

		private void DiscoverExplorerViewWidgets(KScreen screen) {
			var explorerGridGO = Traverse.Create(screen).Field("explorerGrid")
				.GetValue<UnityEngine.GameObject>();
			var explorerGrid = explorerGridGO != null ? explorerGridGO.transform : null;

			if (explorerGrid != null) {
				for (int i = 0; i < explorerGrid.childCount; i++) {
					var child = explorerGrid.GetChild(i);
					if (child == null || !child.gameObject.activeInHierarchy) continue;

					var kbutton = child.GetComponent<KButton>();
					if (kbutton == null) continue;

					string label = ReadColonyEntryLabel(child);
					if (string.IsNullOrEmpty(label)) continue;

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = kbutton,
						Type = WidgetType.Button,
						GameObject = kbutton.gameObject
					});
				}
			}

			AddDetailButtons(screen);
		}

		/// <summary>
		/// Read colony entry label from HierarchyReferences using GetParsedText()
		/// to avoid the SetText()/TMP buffer quirk where .text returns stale data.
		/// Format: "ColonyName, Cycle Count: X, date"
		/// </summary>
		private static string ReadColonyEntryLabel(UnityEngine.Transform entry) {
			var hierRef = entry.GetComponent<HierarchyReferences>();
			if (hierRef == null) return null;

			string name = null;
			string cycles = null;
			string date = null;

			if (hierRef.HasReference("ColonyNameLabel")) {
				var label = hierRef.GetReference<LocText>("ColonyNameLabel");
				if (label != null) name = label.GetParsedText();
			}
			if (hierRef.HasReference("CycleCountLabel")) {
				var label = hierRef.GetReference<LocText>("CycleCountLabel");
				if (label != null) cycles = label.GetParsedText();
			}
			if (hierRef.HasReference("DateLabel")) {
				var label = hierRef.GetReference<LocText>("DateLabel");
				if (label != null) date = label.GetParsedText();
			}

			if (string.IsNullOrEmpty(name)) return null;

			var parts = new List<string> { name };
			if (!string.IsNullOrEmpty(cycles)) parts.Add(cycles);
			if (!string.IsNullOrEmpty(date)) parts.Add(date);
			return string.Join(", ", parts);
		}

		// ========================================
		// DETAIL VIEW - DUPLICANTS (section 0)
		// ========================================

		private void DiscoverDuplicantWidgets(KScreen screen) {
			var activeWidgets = Traverse.Create(screen).Field("activeColonyWidgets")
				.GetValue<Dictionary<string, UnityEngine.GameObject>>();
			if (activeWidgets == null || !activeWidgets.TryGetValue("duplicants", out var dupBlock))
				return;

			var hierRef = dupBlock.GetComponent<HierarchyReferences>();
			if (hierRef == null || !hierRef.HasReference("Content")) return;

			var contentTransform = hierRef.GetReference("Content").transform;
			if (contentTransform == null) return;

			for (int i = 0; i < contentTransform.childCount; i++) {
				var child = contentTransform.GetChild(i);
				if (child == null || !child.gameObject.activeInHierarchy) continue;

				// Pagination creates empty placeholder GameObjects without HierarchyReferences
				if (child.GetComponent<HierarchyReferences>() == null) continue;

				var dupGO = child.gameObject;
				_widgets.Add(new WidgetInfo {
					Label = "",
					Component = null,
					Type = WidgetType.Label,
					GameObject = dupGO,
					SpeechFunc = () => ReadDuplicantEntry(dupGO.transform)
				});
			}

			AddDetailButtons(screen);
		}

		/// <summary>
		/// Read duplicant entry live from HierarchyReferences at speech time.
		/// </summary>
		private static string ReadDuplicantEntry(UnityEngine.Transform entry) {
			var refs = entry.GetComponent<HierarchyReferences>();
			if (refs == null) return null;

			string name = null;
			string age = null;
			string skill = null;

			if (refs.HasReference("NameLabel")) {
				var label = refs.GetReference<LocText>("NameLabel");
				if (label != null) name = label.GetParsedText();
			}
			if (refs.HasReference("AgeLabel")) {
				var label = refs.GetReference<LocText>("AgeLabel");
				if (label != null) age = label.GetParsedText();
			}
			if (refs.HasReference("SkillLabel")) {
				var label = refs.GetReference<LocText>("SkillLabel");
				if (label != null) skill = label.GetParsedText();
			}

			if (string.IsNullOrEmpty(name)) return null;

			var parts = new List<string> { name };
			if (!string.IsNullOrEmpty(age)) parts.Add(age);
			if (!string.IsNullOrEmpty(skill)) parts.Add(skill);
			return string.Join(", ", parts);
		}

		// ========================================
		// DETAIL VIEW - BUILDINGS (section 1)
		// ========================================

		private void DiscoverBuildingWidgets(KScreen screen) {
			var activeWidgets = Traverse.Create(screen).Field("activeColonyWidgets")
				.GetValue<Dictionary<string, UnityEngine.GameObject>>();
			if (activeWidgets == null || !activeWidgets.TryGetValue("buildings", out var bldgBlock))
				return;

			var hierRef = bldgBlock.GetComponent<HierarchyReferences>();
			if (hierRef == null || !hierRef.HasReference("Content")) return;

			var contentTransform = hierRef.GetReference("Content").transform;
			if (contentTransform == null) return;

			for (int i = 0; i < contentTransform.childCount; i++) {
				var child = contentTransform.GetChild(i);
				if (child == null || !child.gameObject.activeInHierarchy) continue;

				if (child.GetComponent<HierarchyReferences>() == null) continue;

				var bldgGO = child.gameObject;
				_widgets.Add(new WidgetInfo {
					Label = "",
					Component = null,
					Type = WidgetType.Label,
					GameObject = bldgGO,
					SpeechFunc = () => ReadBuildingEntry(bldgGO.transform)
				});
			}

			AddDetailButtons(screen);
		}

		/// <summary>
		/// Read building entry live from HierarchyReferences at speech time.
		/// </summary>
		private static string ReadBuildingEntry(UnityEngine.Transform entry) {
			var refs = entry.GetComponent<HierarchyReferences>();
			if (refs == null) return null;

			string name = null;
			string count = null;

			if (refs.HasReference("NameLabel")) {
				var label = refs.GetReference<LocText>("NameLabel");
				if (label != null) name = label.GetParsedText();
			}
			if (refs.HasReference("CountLabel")) {
				var label = refs.GetReference<LocText>("CountLabel");
				if (label != null) count = label.GetParsedText();
			}

			if (string.IsNullOrEmpty(name)) return null;
			if (!string.IsNullOrEmpty(count))
				return $"{name}, {count}";
			return name;
		}

		// ========================================
		// DETAIL VIEW - STATISTICS (section 2)
		// ========================================

		private void DiscoverStatWidgets(KScreen screen) {
			var activeWidgets = Traverse.Create(screen).Field("activeColonyWidgets")
				.GetValue<Dictionary<string, UnityEngine.GameObject>>();
			if (activeWidgets != null) {
				foreach (var kvp in activeWidgets) {
					if (kvp.Key == "timelapse" || kvp.Key == "duplicants" || kvp.Key == "buildings")
						continue;

					var statGO = kvp.Value;
					string statKey = kvp.Key;
					_widgets.Add(new WidgetInfo {
						Label = statKey,
						Component = null,
						Type = WidgetType.Label,
						GameObject = statGO,
						SpeechFunc = () => ReadStatEntry(statGO, statKey)
					});
				}
			}

			AddDetailButtons(screen);
		}

		/// <summary>
		/// Read stat name and latest value live from the GraphBase component.
		/// Each stat graph widget has a GraphBase with graphName and a LineLayer
		/// containing a GraphedLine with the data points (cycle, value).
		/// When no data points exist, appends "None" so the user knows the stat
		/// is present but empty rather than thinking the mod failed to read it.
		/// </summary>
		private static string ReadStatEntry(UnityEngine.GameObject go, string fallbackName) {
			var graph = go.GetComponentInChildren<GraphBase>();
			string name = graph != null ? graph.graphName : null;
			if (string.IsNullOrEmpty(name)) name = fallbackName;
			if (string.IsNullOrEmpty(name)) return null;

			if (graph != null) {
				var lineLayer = go.GetComponentInChildren<LineLayer>();
				if (lineLayer != null) {
					var lines = Traverse.Create(lineLayer).Field("lines")
						.GetValue<List<GraphedLine>>();
					if (lines != null && lines.Count > 0) {
						var points = Traverse.Create(lines[0]).Field("points")
							.GetValue<UnityEngine.Vector2[]>();
						if (points != null && points.Length > 0) {
							float lastValue = points[points.Length - 1].y;
							return $"{name}, {lastValue:N1}";
						}
					}
				}
			}

			return $"{name}, {(string)STRINGS.ONIACCESS.STATES.NONE}";
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

				var achGO = child.gameObject;
				_widgets.Add(new WidgetInfo {
					Label = (string)STRINGS.ONIACCESS.INFO.ACHIEVEMENT,
					Component = null,
					Type = WidgetType.Label,
					GameObject = achGO,
					SpeechFunc = () => ReadAchievementText(achGO.transform)
				});
			}

			if (_inColonyDetail)
				AddDetailButtons(screen);
		}

		// ========================================
		// SHARED BUTTONS
		// ========================================

		/// <summary>
		/// Add navigation buttons common to all detail view sections.
		/// Also used by explorer view for close/quit buttons.
		/// </summary>
		private void AddDetailButtons(KScreen screen) {
			WidgetDiscoveryUtil.TryAddButtonField(screen, "viewOtherColoniesButton", (string)STRINGS.ONIACCESS.BUTTONS.VIEW_OTHER_COLONIES, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeScreenButton", (string)STRINGS.UI.TOOLTIPS.CLOSETOOLTIP, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "quitToMainMenuButton", (string)STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.BUTTONS.QUIT_TO_MENU, _widgets);
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
				// Colony entries have HierarchyReferences with "ColonyNameLabel"
				var hierRef = widget.GameObject != null
					? widget.GameObject.GetComponent<HierarchyReferences>() : null;
				if (hierRef != null && hierRef.HasReference("ColonyNameLabel")) {
					var kbutton = widget.Component as KButton;
					kbutton.SignalClick(KKeyCode.Mouse0);
					_inColonyDetail = true;
					_currentSection = DetailSectionDuplicants;

					// Defer widget discovery: LoadColony uses coroutines to populate
					// the detail view. Widgets aren't ready until a later frame.
					_pendingRediscovery = true;
					SpeakDetailHeader();
					return;
				}
			}

			if (_inColonyDetail && widget.Component is KButton btn && btn == _viewOtherColoniesButton) {
				ReturnToExplorerView();
				return;
			}

			base.ActivateCurrentWidget();
		}

		/// <summary>
		/// Speak the colony name and cycle count header when entering detail view.
		/// Uses .text here (not GetParsedText) because LoadColony sets colonyName.text
		/// and cycleCount.text directly via assignment, not via SetText().
		/// </summary>
		private void SpeakDetailHeader() {
			var colonyNameLoc = Traverse.Create(_screen).Field("colonyName")
				.GetValue<LocText>();
			var cycleCountLoc = Traverse.Create(_screen).Field("cycleCount")
				.GetValue<LocText>();

			string name = colonyNameLoc != null ? colonyNameLoc.text : null;
			string cycles = cycleCountLoc != null ? cycleCountLoc.text : null;

			var parts = new List<string>();
			if (!string.IsNullOrEmpty(name)) parts.Add(name);
			if (!string.IsNullOrEmpty(cycles)) parts.Add(cycles);
			string sectionName = GetSectionName(_currentSection);
			parts.Add(sectionName);

			Speech.SpeechPipeline.SpeakInterrupt(string.Join(", ", parts));
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
			try {
				if (_viewOtherColoniesButton != null) {
					_viewOtherColoniesButton.SignalClick(KKeyCode.Mouse0);
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.ReturnToExplorerView: {ex.Message}");
			}

			_inColonyDetail = false;
			_currentSection = ExplorerSectionMain;
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
			int count = SectionCount;
			_currentSection = (_currentSection + 1) % count;
			if (_currentSection == 0) PlayWrapSound();
			RediscoverForCurrentSection();
		}

		protected override void NavigateTabBackward() {
			int count = SectionCount;
			int prev = _currentSection;
			_currentSection = (_currentSection - 1 + count) % count;
			if (_currentSection == count - 1 && prev == 0) PlayWrapSound();
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

		private string GetSectionName(int section) {
			if (_inColonyDetail) {
				switch (section) {
					case DetailSectionDuplicants: return STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.TITLES.DUPLICANTS;
					case DetailSectionBuildings: return STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.TITLES.BUILDINGS;
					case DetailSectionStats: return STRINGS.ONIACCESS.PANELS.STATS;
					case DetailSectionAchievements: return STRINGS.ONIACCESS.PANELS.ACHIEVEMENTS;
				}
			} else {
				switch (section) {
					case ExplorerSectionAchievements: return STRINGS.ONIACCESS.PANELS.ACHIEVEMENTS;
				}
			}
			return DisplayName;
		}

		// ========================================
		// WIDGET SPEECH
		// ========================================


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

	}
}
