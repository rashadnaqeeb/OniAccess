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

		private enum ButtonId {
			None,
			ViewOtherColonies,
			Close,
		}

		private struct Item {
			public ItemKind Kind;
			public string Label;
			public int DataIndex;
			public string AchievementId;
			public ButtonId Button;
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
			_currentSection = ExplorerSectionMain;

			_screenTraverse = Traverse.Create(_screen);
			CacheButtons();

			_isInGameContext = SaveGame.Instance != null;
			if (_isInGameContext) {
				_inColonyDetail = true;
				_currentSection = DetailSectionDuplicants;
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
					_allColonies = null;
				} else {
					_allColonies = _screenTraverse.Field("retiredColonyData")
						.GetValue<RetiredColonyData[]>();
					if (_allColonies == null)
						_allColonies = RetireColonyUtility.LoadRetiredColonies();
				}
				_colonyData = null;
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

			// Copy before sorting — never mutate the game's live data
			var sorted = new List<Tuple<string, int>>(data.buildings);
			sorted.Sort((a, b) => b.second.CompareTo(a.second));

			for (int i = 0; i < sorted.Count; i++) {
				var bldg = sorted[i];
				if (Assets.GetPrefab(new Tag(bldg.first)) == null) continue;
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
				if (!DlcManager.IsCorrectDlcSubscribed(achievement)) continue;
				if (_isInGameContext && Game.Instance != null) {
					if (!Game.IsCorrectDlcActiveForCurrentSave(achievement)) continue;
					if (!achievement.IsValidForSave()) continue;
				}

				if (!addedVictoryHeader && achievement.isVictoryCondition) {
					addedVictoryHeader = true;
					_items.Add(new Item {
						Kind = ItemKind.VictoryHeader,
						Label = STRINGS.ONIACCESS.PANELS.VICTORY_CONDITIONS
					});
				}

				string status = GetAchievementStatus(achievement, completedIds);
				string label = $"{status}, {achievement.Name}, {achievement.description}";

				if (achievement.isVictoryCondition)
					label = AppendVictoryRequirements(label, achievement);

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
					Label = STRINGS.ONIACCESS.BUTTONS.VIEW_OTHER_COLONIES,
					Button = ButtonId.ViewOtherColonies
				});
			}
			if (_closeScreenButton != null && _closeScreenButton.gameObject.activeSelf) {
				_items.Add(new Item {
					Kind = ItemKind.Button,
					Label = (string)STRINGS.UI.TOOLTIPS.CLOSETOOLTIP,
					Button = ButtonId.Close
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

		/// <summary>
		/// Each stat is a per-cycle time series. A sighted user sees a line graph.
		/// Show last cycle value and peak to convey the trend.
		/// </summary>
		private static string FormatStat(RetiredColonyData.RetiredColonyStatistic stat) {
			if (stat.value == null || stat.value.Length == 0)
				return $"{stat.name}, {(string)STRINGS.ONIACCESS.STATES.NONE}";

			float latest = stat.value[stat.value.Length - 1].second;
			float peak = stat.GetByMaxValue().second;
			string unit = stat.nameY ?? "";
			string lastCycle = (string)STRINGS.ONIACCESS.COLONY_STATS.LAST_CYCLE;
			string peakLabel = (string)STRINGS.ONIACCESS.COLONY_STATS.PEAK;

			if (peak > latest && peak > 0) {
				if (!string.IsNullOrEmpty(unit))
					return $"{stat.name}, {lastCycle} {latest:N0} {unit}, {peakLabel} {peak:N0} {unit}";
				return $"{stat.name}, {lastCycle} {latest:N0}, {peakLabel} {peak:N0}";
			}

			if (!string.IsNullOrEmpty(unit))
				return $"{stat.name}, {lastCycle} {latest:N0} {unit}";
			return $"{stat.name}, {lastCycle} {latest:N0}";
		}

		private static string AppendVictoryRequirements(string label, ColonyAchievement achievement) {
			var reqs = new List<string>();
			foreach (var req in achievement.requirementChecklist) {
				if (req is VictoryColonyAchievementRequirement victoryReq) {
					string desc = null;
					try {
						desc = victoryReq.Description();
					} catch (System.Exception ex) {
						Util.Log.Debug($"VictoryReq.Description() unavailable for {achievement.Id}: {ex.Message}");
					}
					if (string.IsNullOrEmpty(desc)) {
						try {
							desc = victoryReq.Name();
						} catch (System.Exception ex) {
							Util.Log.Warn($"VictoryReq.Name() failed for {achievement.Id}: {ex.Message}");
						}
					}
					if (!string.IsNullOrEmpty(desc))
						reqs.Add(desc);
				}
			}
			if (reqs.Count > 0)
				return label + ". " + string.Join(". ", reqs);
			return label;
		}

		private string GetAchievementStatus(ColonyAchievement achievement,
			HashSet<string> completedIds) {
			if (_inColonyDetail) {
				if (completedIds.Contains(achievement.Id))
					return (string)STRINGS.ONIACCESS.STATES.CONDITION_MET;
				if (_isInGameContext)
					return GetInGameAchievementStatus(achievement.Id);
				return (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
			}

			// Explorer view: check all retired colonies
			if (_allColonies != null) {
				foreach (var colony in _allColonies) {
					if (colony.achievements == null) continue;
					foreach (string id in colony.achievements) {
						if (id == achievement.Id)
							return (string)STRINGS.ONIACCESS.STATES.CONDITION_MET;
					}
				}
			}
			return (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
		}

		private static string GetInGameAchievementStatus(string achievementId) {
			if (SaveGame.Instance == null)
				return (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
			var tracker = SaveGame.Instance.GetComponent<ColonyAchievementTracker>();
			if (tracker == null)
				return (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
			if (!tracker.achievements.TryGetValue(achievementId, out var status))
				return (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
			if (status.success) return (string)STRINGS.ONIACCESS.STATES.CONDITION_MET;
			if (status.failed) return (string)STRINGS.ONIACCESS.STATES.CONDITION_FAILED;
			return (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
		}

		// ========================================
		// SPEECH
		// ========================================

		private string BuildSpeech(Item item) {
			if (item.Kind == ItemKind.Achievement && _isInGameContext)
				return BuildAchievementSpeechLive(item);
			return item.Label;
		}

		/// <summary>
		/// For in-game achievements, read per-requirement progress from the game's
		/// AchievementWidget. The game populates these widgets via ShowProgress()
		/// during LoadColony. Read lazily at speech time to ensure the screen has
		/// finished populating.
		/// </summary>
		private string BuildAchievementSpeechLive(Item item) {
			var entries = _screenTraverse.Field("achievementEntries")
				.GetValue<Dictionary<string, UnityEngine.GameObject>>();
			if (entries == null || !entries.TryGetValue(item.AchievementId, out var achGO))
				return item.Label;

			var progress = ReadWidgetProgress(achGO);
			if (progress.Count > 0) {
				string sep = item.Label.EndsWith(".") ? " " : ". ";
				return item.Label + sep + string.Join(". ", progress);
			}
			return item.Label;
		}

		private static List<string> ReadWidgetProgress(UnityEngine.GameObject achGO) {
			var result = new List<string>();
			var widget = achGO.GetComponent<AchievementWidget>();
			if (widget == null) return result;

			var progressParent = Traverse.Create(widget).Field("progressParent")
				.GetValue<UnityEngine.RectTransform>();
			if (progressParent == null || progressParent.childCount == 0) return result;

			bool wasActive = progressParent.gameObject.activeSelf;
			if (!wasActive)
				progressParent.gameObject.SetActive(true);
			UnityEngine.Canvas.ForceUpdateCanvases();

			for (int i = 0; i < progressParent.childCount; i++) {
				var child = progressParent.GetChild(i);
				if (child == null || !child.gameObject.activeSelf) continue;
				var refs = child.GetComponent<HierarchyReferences>();
				if (refs == null || !refs.HasReference("Desc")) continue;
				var descLoc = refs.GetReference<LocText>("Desc");
				if (descLoc == null) continue;
				string text = descLoc.GetParsedText();
				if (!string.IsNullOrEmpty(text))
					result.Add(text);
			}

			if (!wasActive)
				progressParent.gameObject.SetActive(false);
			return result;
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
			switch (item.Button) {
				case ButtonId.ViewOtherColonies:
					ReturnToExplorerView();
					break;
				case ButtonId.Close:
					if (_closeScreenButton != null)
						Widgets.WidgetOps.ClickButton(_closeScreenButton);
					break;
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
		// HELPERS
		// ========================================

		private RetiredColonyData GetActiveColonyData() {
			// In-game: always re-query — colony data changes as the game progresses
			if (_isInGameContext)
				return RetireColonyUtility.GetCurrentColonyRetiredColonyData();
			return _colonyData;
		}

	}
}
