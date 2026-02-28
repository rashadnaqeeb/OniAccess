using System.Collections.Generic;
using Database;
using HarmonyLib;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for RetiredColonyInfoScreen (colony summary accessible from main menu
	/// and pause menu). Covers MENU-09 requirement.
	///
	/// Data-driven: reads RetiredColonyData directly instead of scraping UI widgets.
	/// The only widget interaction is clicking KButtons for view transitions.
	///
	/// The screen has two views:
	/// 1. Explorer view (main menu): grid of colony buttons for past/retired colonies
	/// 2. Colony detail view: duplicants, buildings, statistics, achievements
	///
	/// Navigation:
	/// - Explorer view: Up/Down navigates colony entries, Enter opens colony detail,
	///   Tab switches between colonies and achievements sections
	/// - Detail view: Up/Down navigates entries within current section, Tab switches sections
	/// - Escape returns from detail to explorer (main menu) or closes (in-game)
	/// </summary>
	public class ColonySummaryHandler : BaseMenuHandler {
		private const int ExplorerSectionMain = 0;
		private const int ExplorerSectionAchievements = 1;
		private const int ExplorerSectionCount = 2;

		private const int DetailSectionDuplicants = 0;
		private const int DetailSectionBuildings = 1;
		private const int DetailSectionStats = 2;
		private const int DetailSectionAchievements = 3;
		private const int DetailSectionCount = 4;

		private enum ItemKind {
			Colony,
			Duplicant,
			Building,
			Stat,
			Achievement,
			VictoryHeader,
			Button,
		}

		private struct Item {
			public ItemKind Kind;
			public string Label;
			public int DataIndex;
			public string AchievementId;
		}

		private bool _inColonyDetail;
		private bool _isInGameContext;
		private int _currentSection;

		private RetiredColonyData[] _allColonies;
		private RetiredColonyData _colonyData;
		private List<Item> _items = new List<Item>();

		private KButton _viewOtherColoniesButton;
		private KButton _closeScreenButton;
		private Traverse _explorerGridField;
		private Traverse _screenTraverse;

		private int SectionCount => _inColonyDetail ? DetailSectionCount : ExplorerSectionCount;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.COLONY_SUMMARY;

		public override int ItemCount => _items.Count;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ColonySummaryHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		public override string GetItemLabel(int index) {
			if (index < 0 || index >= _items.Count) return null;
			return _items[index].Label;
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			if (_currentIndex < 0 || _currentIndex >= _items.Count) return;
			var item = _items[_currentIndex];
			string speech = BuildSpeech(item);
			Speech.SpeechPipeline.SpeakInterrupt(speech);
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			_inColonyDetail = false;
			_isInGameContext = false;
			_currentSection = ExplorerSectionMain;

			_screenTraverse = Traverse.Create(_screen);
			CacheButtons();

			try {
				var explorerRoot = _screenTraverse.Field("explorerRoot")
					.GetValue<UnityEngine.GameObject>();
				if (explorerRoot != null && !explorerRoot.activeSelf) {
					_isInGameContext = true;
					_inColonyDetail = true;
					_currentSection = DetailSectionDuplicants;
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.OnActivate: {ex.Message}");
			}

			LoadColonyData();
			BuildItems();
			base.OnActivate();

			if (_items.Count > 0)
				Speech.SpeechPipeline.SpeakQueued(BuildSpeech(_items[0]));
		}

		private void CacheButtons() {
			try {
				_viewOtherColoniesButton = _screenTraverse.Field("viewOtherColoniesButton")
					.GetValue<KButton>();
				_closeScreenButton = _screenTraverse.Field("closeScreenButton")
					.GetValue<KButton>();
				_explorerGridField = _screenTraverse.Field("explorerGrid");
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.CacheButtons: {ex.Message}");
			}
		}

		private void LoadColonyData() {
			try {
				if (_isInGameContext) {
					_colonyData = RetireColonyUtility.GetCurrentColonyRetiredColonyData();
					_allColonies = null;
				} else {
					_allColonies = _screenTraverse.Field("retiredColonyData")
						.GetValue<RetiredColonyData[]>();
					if (_allColonies == null)
						_allColonies = RetireColonyUtility.LoadRetiredColonies();
					_colonyData = null;
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.LoadColonyData: {ex.Message}");
			}
		}

		// ========================================
		// ITEM BUILDING
		// ========================================

		private void BuildItems() {
			_items.Clear();

			if (!_inColonyDetail) {
				if (_currentSection == ExplorerSectionAchievements)
					BuildAchievementItems();
				else
					BuildExplorerItems();
			} else {
				switch (_currentSection) {
					case DetailSectionDuplicants:
						BuildDuplicantItems();
						break;
					case DetailSectionBuildings:
						BuildBuildingItems();
						break;
					case DetailSectionStats:
						BuildStatItems();
						break;
					case DetailSectionAchievements:
						BuildAchievementItems();
						break;
				}
			}

			AddButtons();
		}

		private void BuildExplorerItems() {
			if (_allColonies == null) return;
			for (int i = 0; i < _allColonies.Length; i++) {
				var colony = _allColonies[i];
				string label = FormatColonyEntry(colony);
				_items.Add(new Item { Kind = ItemKind.Colony, Label = label, DataIndex = i });
			}
		}

		private void BuildDuplicantItems() {
			var data = GetActiveColonyData();
			if (data?.Duplicants == null) return;
			for (int i = 0; i < data.Duplicants.Length; i++) {
				var dup = data.Duplicants[i];
				string label = FormatDuplicant(dup);
				_items.Add(new Item { Kind = ItemKind.Duplicant, Label = label, DataIndex = i });
			}
		}

		private void BuildBuildingItems() {
			var data = GetActiveColonyData();
			if (data?.buildings == null) return;
			for (int i = 0; i < data.buildings.Count; i++) {
				var bldg = data.buildings[i];
				string label = FormatBuilding(bldg);
				_items.Add(new Item { Kind = ItemKind.Building, Label = label, DataIndex = i });
			}
		}

		private void BuildStatItems() {
			var data = GetActiveColonyData();
			if (data?.Stats == null) return;
			for (int i = 0; i < data.Stats.Length; i++) {
				var stat = data.Stats[i];
				string label = FormatStat(stat);
				_items.Add(new Item { Kind = ItemKind.Stat, Label = label, DataIndex = i });
			}
		}

		private void BuildAchievementItems() {
			var achievements = Db.Get().ColonyAchievements.resources;
			var data = _inColonyDetail ? GetActiveColonyData() : null;

			var completedIds = new HashSet<string>();
			if (data?.achievements != null) {
				foreach (string id in data.achievements)
					completedIds.Add(id);
			}

			bool addedVictoryHeader = false;
			foreach (var achievement in achievements) {
				if (!IsAchievementValid(achievement)) continue;

				if (!addedVictoryHeader && achievement.isVictoryCondition) {
					addedVictoryHeader = true;
					_items.Add(new Item {
						Kind = ItemKind.VictoryHeader,
						Label = STRINGS.ONIACCESS.PANELS.VICTORY_CONDITIONS
					});
				}

				string label = FormatAchievement(achievement, completedIds, data);
				_items.Add(new Item {
					Kind = ItemKind.Achievement,
					Label = label,
					AchievementId = achievement.Id
				});
			}
		}

		private void AddButtons() {
			if (_inColonyDetail && !_isInGameContext
				&& _viewOtherColoniesButton != null
				&& _viewOtherColoniesButton.gameObject.activeSelf) {
				_items.Add(new Item {
					Kind = ItemKind.Button,
					Label = STRINGS.ONIACCESS.BUTTONS.VIEW_OTHER_COLONIES
				});
			}
			if (_closeScreenButton != null && _closeScreenButton.gameObject.activeSelf) {
				_items.Add(new Item {
					Kind = ItemKind.Button,
					Label = (string)STRINGS.UI.TOOLTIPS.CLOSETOOLTIP
				});
			}
		}

		// ========================================
		// LABEL FORMATTING
		// ========================================

		private static string FormatColonyEntry(RetiredColonyData colony) {
			string cycles = string.Format(STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.CYCLE_COUNT,
				colony.cycleCount.ToString());
			if (!string.IsNullOrEmpty(colony.date))
				return $"{colony.colonyName}, {cycles}, {colony.date}";
			return $"{colony.colonyName}, {cycles}";
		}

		private static string FormatDuplicant(RetiredColonyData.RetiredDuplicantData dup) {
			string age = string.Format(STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.DUPLICANT_AGE,
				dup.age.ToString());
			string skill = string.Format(STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.SKILL_LEVEL,
				dup.skillPointsGained.ToString());
			return $"{dup.name}, {age}, {skill}";
		}

		private static string FormatBuilding(Tuple<string, int> bldg) {
			string name;
			var prefab = Assets.GetPrefab(new Tag(bldg.first));
			name = prefab != null ? prefab.GetProperName() : bldg.first;
			string count = string.Format(STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.BUILDING_COUNT,
				bldg.second.ToString());
			return $"{name}, {count}";
		}

		private static string FormatStat(RetiredColonyData.RetiredColonyStatistic stat) {
			if (stat.value != null && stat.value.Length > 0) {
				float lastValue = stat.value[stat.value.Length - 1].second;
				if (!string.IsNullOrEmpty(stat.nameY))
					return $"{stat.name}, {lastValue:N0} {stat.nameY}";
				return $"{stat.name}, {lastValue:N0}";
			}
			return $"{stat.name}, {(string)STRINGS.ONIACCESS.STATES.NONE}";
		}

		private string FormatAchievement(ColonyAchievement achievement,
			HashSet<string> completedIds, RetiredColonyData data) {
			string label = $"{achievement.Name}, {achievement.description}";

			if (_inColonyDetail) {
				if (completedIds.Contains(achievement.Id))
					label += $", {(string)STRINGS.ONIACCESS.STATES.CONDITION_MET}";
				else if (_isInGameContext)
					label += GetAchievementFailedStatus(achievement.Id);
			}
			return label;
		}

		private static string GetAchievementFailedStatus(string achievementId) {
			if (SaveGame.Instance == null) return "";
			var tracker = SaveGame.Instance.GetComponent<ColonyAchievementTracker>();
			if (tracker == null) return "";
			if (!tracker.achievements.TryGetValue(achievementId, out var status)) return "";
			if (status.failed)
				return $", {(string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET}";
			return "";
		}

		// ========================================
		// SPEECH
		// ========================================

		private string BuildSpeech(Item item) {
			if (item.Kind == ItemKind.Achievement && _isInGameContext && _inColonyDetail)
				return BuildAchievementSpeechLive(item);
			return item.Label;
		}

		/// <summary>
		/// For in-game achievements, re-query status at speech time since
		/// achievement progress can change while the screen is open.
		/// </summary>
		private string BuildAchievementSpeechLive(Item item) {
			var achievement = Db.Get().ColonyAchievements.TryGet(item.AchievementId);
			if (achievement == null) return item.Label;

			string label = $"{achievement.Name}, {achievement.description}";
			if (SaveGame.Instance == null) return label;

			var tracker = SaveGame.Instance.GetComponent<ColonyAchievementTracker>();
			if (tracker == null) return label;
			if (!tracker.achievements.TryGetValue(item.AchievementId, out var status))
				return label;

			if (status.success)
				return label + $", {(string)STRINGS.ONIACCESS.STATES.CONDITION_MET}";
			if (status.failed)
				return label + $", {(string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET}";
			return label;
		}

		// ========================================
		// VIEW TRANSITIONS
		// ========================================

		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _items.Count) return;
			var item = _items[_currentIndex];

			if (item.Kind == ItemKind.Colony && !_inColonyDetail) {
				OpenColonyDetail(item.DataIndex);
				return;
			}

			if (item.Kind == ItemKind.Button) {
				ActivateButton(item);
				return;
			}

			if (item.Kind == ItemKind.Achievement && _isInGameContext && _inColonyDetail) {
				SpeakAchievementProgress(item.AchievementId);
				return;
			}
		}

		private void OpenColonyDetail(int colonyIndex) {
			if (_allColonies == null || colonyIndex < 0 || colonyIndex >= _allColonies.Length) return;
			_colonyData = _allColonies[colonyIndex];

			// Click the matching KButton in the explorer grid to trigger the game's
			// LoadColony, which sets up the detail view UI.
			ClickExplorerButton(colonyIndex);

			_inColonyDetail = true;
			_currentSection = DetailSectionDuplicants;

			string header = FormatDetailHeader(_colonyData);
			string sectionName = GetSectionName(_currentSection);
			Speech.SpeechPipeline.SpeakInterrupt($"{header}, {sectionName}");

			BuildItems();
			_currentIndex = 0;
			if (_items.Count > 0)
				Speech.SpeechPipeline.SpeakQueued(BuildSpeech(_items[0]));
		}

		private void ClickExplorerButton(int index) {
			try {
				var explorerGridGO = _explorerGridField.GetValue<UnityEngine.GameObject>();
				if (explorerGridGO == null) return;
				var grid = explorerGridGO.transform;
				int buttonIndex = 0;
				for (int i = 0; i < grid.childCount; i++) {
					var child = grid.GetChild(i);
					if (child == null || !child.gameObject.activeInHierarchy) continue;
					var kbutton = child.GetComponent<KButton>();
					if (kbutton == null) continue;
					if (buttonIndex == index) {
						Widgets.WidgetOps.ClickButton(kbutton);
						return;
					}
					buttonIndex++;
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.ClickExplorerButton: {ex.Message}");
			}
		}

		private void ActivateButton(Item item) {
			if (item.Label == (string)STRINGS.ONIACCESS.BUTTONS.VIEW_OTHER_COLONIES) {
				ReturnToExplorerView();
			} else if (item.Label == (string)STRINGS.UI.TOOLTIPS.CLOSETOOLTIP) {
				if (_closeScreenButton != null)
					Widgets.WidgetOps.ClickButton(_closeScreenButton);
			}
		}

		private static string FormatDetailHeader(RetiredColonyData data) {
			string cycles = string.Format(STRINGS.UI.RETIRED_COLONY_INFO_SCREEN.CYCLE_COUNT,
				data.cycleCount.ToString());
			return $"{data.colonyName}, {cycles}";
		}

		/// <summary>
		/// Intercept Escape in colony detail view to go back to explorer
		/// instead of closing the screen.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e)) return true;

			if (_inColonyDetail && !_isInGameContext && e.TryConsume(global::Action.Escape)) {
				ReturnToExplorerView();
				return true;
			}

			return false;
		}

		private void ReturnToExplorerView() {
			if (_isInGameContext) return;

			try {
				if (_viewOtherColoniesButton != null)
					Widgets.WidgetOps.ClickButton(_viewOtherColoniesButton);
			} catch (System.Exception ex) {
				Util.Log.Error($"ColonySummaryHandler.ReturnToExplorerView: {ex.Message}");
			}

			_inColonyDetail = false;
			_colonyData = null;
			_currentSection = ExplorerSectionMain;
			BuildItems();
			_currentIndex = 0;

			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
			if (_items.Count > 0)
				Speech.SpeechPipeline.SpeakQueued(BuildSpeech(_items[0]));
		}

		// ========================================
		// TAB NAVIGATION
		// ========================================

		protected override void NavigateTabForward() {
			int count = SectionCount;
			_currentSection = (_currentSection + 1) % count;
			if (_currentSection == 0) PlayWrapSound();
			RebuildForCurrentSection();
		}

		protected override void NavigateTabBackward() {
			int count = SectionCount;
			int prev = _currentSection;
			_currentSection = (_currentSection - 1 + count) % count;
			if (_currentSection == count - 1 && prev == 0) PlayWrapSound();
			RebuildForCurrentSection();
		}

		private void RebuildForCurrentSection() {
			BuildItems();
			_currentIndex = 0;
			string sectionName = GetSectionName(_currentSection);
			Speech.SpeechPipeline.SpeakInterrupt(sectionName);
			if (_items.Count > 0)
				Speech.SpeechPipeline.SpeakQueued(BuildSpeech(_items[0]));
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
		// ACHIEVEMENT PROGRESS (in-game only)
		// ========================================

		/// <summary>
		/// Speak per-requirement progress for an in-game achievement.
		/// Reads directly from ColonyAchievementStatus.Requirements.
		/// </summary>
		private static void SpeakAchievementProgress(string achievementId) {
			if (SaveGame.Instance == null) return;
			var tracker = SaveGame.Instance.GetComponent<ColonyAchievementTracker>();
			if (tracker == null) return;
			if (!tracker.achievements.TryGetValue(achievementId, out var status)) return;

			var entries = new List<string>();
			foreach (var req in status.Requirements) {
				bool complete = req.Success();
				string progress = req.GetProgress(complete);
				if (!string.IsNullOrEmpty(progress)) {
					string prefix = complete
						? (string)STRINGS.ONIACCESS.STATES.CONDITION_MET
						: (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
					entries.Add($"{prefix}: {progress}");
				}
			}

			if (entries.Count > 0)
				Speech.SpeechPipeline.SpeakInterrupt(string.Join(". ", entries));
		}

		// ========================================
		// HELPERS
		// ========================================

		private RetiredColonyData GetActiveColonyData() {
			if (_colonyData != null) return _colonyData;
			if (_isInGameContext) {
				_colonyData = RetireColonyUtility.GetCurrentColonyRetiredColonyData();
				return _colonyData;
			}
			return null;
		}

		private bool IsAchievementValid(ColonyAchievement achievement) {
			if (!DlcManager.IsCorrectDlcSubscribed(achievement))
				return false;
			if (_isInGameContext && Game.Instance != null) {
				if (!Game.IsCorrectDlcActiveForCurrentSave(achievement))
					return false;
				if (!achievement.IsValidForSave())
					return false;
			}
			return true;
		}
	}
}
