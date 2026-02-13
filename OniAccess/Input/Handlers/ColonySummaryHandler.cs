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
		private bool _inColonyDetail;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.COLONY_SUMMARY;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public ColonySummaryHandler(KScreen screen) : base(screen) {
			var entries = new List<HelpEntry>();
			entries.AddRange(MenuHelpEntries);
			entries.AddRange(ListNavHelpEntries);
			HelpEntries = entries;
		}

		public override void OnActivate() {
			_inColonyDetail = false;
			base.OnActivate();
		}

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (_inColonyDetail) {
				DiscoverDetailViewWidgets(screen);
			} else {
				DiscoverExplorerViewWidgets(screen);
			}

			Util.Log.Debug($"ColonySummaryHandler.DiscoverWidgets: {_widgets.Count} widgets");
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
			AddScreenButton(screen, "closeScreenButton", "Close");
		}

		// ========================================
		// DETAIL VIEW (colony stats and achievements)
		// ========================================

		/// <summary>
		/// Discover widgets in the colony detail view: colony header,
		/// stat blocks, achievement entries, and navigation buttons.
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
			var statsContainer = Traverse.Create(screen).Field("statsContainer")
				.GetValue<UnityEngine.Transform>();
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

			// Find achievement entries in achievementsContainer
			var achievementsContainer = Traverse.Create(screen).Field("achievementsContainer")
				.GetValue<UnityEngine.Transform>();
			if (achievementsContainer != null) {
				for (int i = 0; i < achievementsContainer.childCount; i++) {
					var child = achievementsContainer.GetChild(i);
					if (child == null || !child.gameObject.activeInHierarchy) continue;

					string achievementLabel = BuildAchievementLabel(child);
					if (string.IsNullOrEmpty(achievementLabel)) continue;

					_widgets.Add(new WidgetInfo {
						Label = achievementLabel,
						Component = null,
						Type = WidgetType.Label,
						GameObject = child.gameObject
					});
				}
			}

			// Navigation buttons: "View other colonies" and "Close"
			AddScreenButton(screen, "viewOtherColoniesButton", "View other colonies");
			AddScreenButton(screen, "closeScreenButton", "Close");
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
				if (kbutton != null && widget.Label != "Close") {
					// Click the colony entry to open detail view
					kbutton.SignalClick(KKeyCode.Mouse0);
					_inColonyDetail = true;

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

			if (_inColonyDetail && widget.Label == "View other colonies") {
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
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
			}
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

		/// <summary>
		/// Build an achievement label from an achievement entry's LocText children.
		/// Combines name and status (locked/unlocked) into a single label.
		/// </summary>
		private string BuildAchievementLabel(UnityEngine.Transform entry) {
			var locTexts = entry.GetComponentsInChildren<LocText>();
			if (locTexts == null || locTexts.Length == 0) return null;

			string name = null;
			string status = null;

			foreach (var lt in locTexts) {
				if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
				if (name == null) {
					name = lt.text;
				} else if (status == null) {
					status = lt.text;
				}
			}

			if (string.IsNullOrEmpty(name)) return null;
			if (!string.IsNullOrEmpty(status))
				return $"{name}, {status}";
			return name;
		}

		/// <summary>
		/// Try to add a KButton from a named field on the screen as a widget.
		/// </summary>
		private void AddScreenButton(KScreen screen, string fieldName, string fallbackLabel) {
			try {
				var button = Traverse.Create(screen).Field(fieldName)
					.GetValue<KButton>();
				if (button != null && button.gameObject.activeInHierarchy) {
					string label = fallbackLabel;
					var locText = button.GetComponentInChildren<LocText>();
					if (locText != null && !string.IsNullOrEmpty(locText.text))
						label = locText.text;

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = button,
						Type = WidgetType.Button,
						GameObject = button.gameObject
					});
				}
			} catch (System.Exception) {
				// Field may not exist -- skip silently
			}
		}
	}
}
