using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for ModeSelectScreen (Survival vs No Sweat),
	/// ClusterCategorySelectionScreen (game mode select), and
	/// ColonyDestinationSelectScreen (asteroid selection + settings).
	///
	/// All three screens are part of the new game setup flow. This handler serves
	/// them all because they share semantics -- behavioral differences flow from
	/// which widgets are present, not from fundamentally different navigation.
	///
	/// ModeSelectScreen: two MultiToggle buttons (Survival / No Sweat).
	/// ClusterCategorySelectionScreen: simple list of MultiToggle buttons for game modes.
	/// ColonyDestinationSelectScreen: tabbed panels (clusters, settings, seed) with
	/// Tab/Shift+Tab switching between panels.
	///
	/// Per locked decisions:
	/// - Game mode entries speak name + description together
	/// - Cluster entries speak name, difficulty, and world traits
	/// - Settings speak "label, value" with Left/Right cycling
	/// - Tab/Shift+Tab switches panels on destination screen only
	/// </summary>
	public class ColonySetupHandler: BaseMenuHandler {
		/// <summary>
		/// The three logical panels on ColonyDestinationSelectScreen.
		/// </summary>
		private const int PanelClusters = 0;
		private const int PanelSettings = 1;
		private const int PanelSeed = 2;
		private const int PanelCount = 3;

		/// <summary>
		/// Current panel index for ColonyDestinationSelectScreen.
		/// Ignored for ClusterCategorySelectionScreen.
		/// </summary>
		private int _currentPanel;

		/// <summary>
		/// Cached pre-edit value for text input Escape rollback.
		/// </summary>
		private string _cachedTextValue;

		/// <summary>
		/// Whether we are currently editing a text input field (seed input).
		/// </summary>
		private bool _isEditingText;

		/// <summary>
		/// Display name changes based on which screen is active.
		/// </summary>
		public override string DisplayName {
			get {
				if (IsClusterCategoryScreen || IsModeSelectScreen)
					return STRINGS.ONIACCESS.HANDLERS.GAME_MODE;
				return STRINGS.ONIACCESS.HANDLERS.COLONY_DESTINATION;
			}
		}

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		/// <summary>
		/// Whether the active screen is ClusterCategorySelectionScreen.
		/// </summary>
		private bool IsClusterCategoryScreen =>
			_screen != null && _screen.GetType().Name == "ClusterCategorySelectionScreen";

		/// <summary>
		/// Whether the active screen is ModeSelectScreen (Survival vs No Sweat).
		/// </summary>
		private bool IsModeSelectScreen =>
			_screen != null && _screen.GetType().Name == "ModeSelectScreen";

		public ColonySetupHandler(KScreen screen) : base(screen) {
			var entries = new List<HelpEntry>();
			entries.AddRange(MenuHelpEntries);
			entries.AddRange(ListNavHelpEntries);
			entries.Add(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
			HelpEntries = entries;
		}

		// ========================================
		// TAB NAVIGATION (ColonyDestinationSelectScreen only)
		// ========================================

		protected override void NavigateTabForward() {
			if (IsClusterCategoryScreen || IsModeSelectScreen) return;

			int prev = _currentPanel;
			_currentPanel = (_currentPanel + 1) % PanelCount;
			if (_currentPanel == 0) PlayWrapSound();
			RediscoverForCurrentPanel();
		}

		protected override void NavigateTabBackward() {
			if (IsClusterCategoryScreen || IsModeSelectScreen) return;

			int prev = _currentPanel;
			_currentPanel = (_currentPanel - 1 + PanelCount) % PanelCount;
			if (_currentPanel == PanelCount - 1 && prev == 0) PlayWrapSound();
			RediscoverForCurrentPanel();
		}

		/// <summary>
		/// Re-discover widgets for the current panel and announce it.
		/// </summary>
		private void RediscoverForCurrentPanel() {
			DiscoverWidgets(_screen);
			string panelName = GetPanelName(_currentPanel);
			Speech.SpeechPipeline.SpeakInterrupt(panelName);
			if (_widgets.Count > 0) {
				_currentIndex = 0;
				Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
			}
		}

		private static string GetPanelName(int panel) {
			switch (panel) {
				case PanelClusters: return STRINGS.ONIACCESS.PANELS.CLUSTERS;
				case PanelSettings: return STRINGS.ONIACCESS.PANELS.SETTINGS;
				case PanelSeed: return STRINGS.ONIACCESS.PANELS.SEED;
				default: return "";
			}
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override void DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (IsModeSelectScreen) {
				DiscoverModeSelectWidgets(screen);
			} else if (IsClusterCategoryScreen) {
				DiscoverGameModeWidgets(screen);
			} else {
				switch (_currentPanel) {
					case PanelClusters:
						DiscoverClusterWidgets(screen);
						break;
					case PanelSettings:
						DiscoverSettingsWidgets(screen);
						break;
					case PanelSeed:
						DiscoverSeedWidgets(screen);
						break;
				}
			}

			Util.Log.Debug($"ColonySetupHandler.DiscoverWidgets: {_widgets.Count} widgets");
		}

		/// <summary>
		/// Discover game mode buttons on ClusterCategorySelectionScreen.
		/// Each ButtonConfig has a MultiToggle with a headerLabel (name) and
		/// a description shown on hover. We build composite: "name, description".
		/// </summary>
		private void DiscoverGameModeWidgets(KScreen screen) {
			// ClusterCategorySelectionScreen has named ButtonConfig fields:
			// vanillaStyle, classicStyle, spacedOutStyle, eventStyle
			string[] configNames = { "vanillaStyle", "classicStyle", "spacedOutStyle", "eventStyle" };

			foreach (var configName in configNames) {
				var buttonConfig = Traverse.Create(screen).Field(configName).GetValue<object>();
				if (buttonConfig == null) continue;

				var configTraverse = Traverse.Create(buttonConfig);
				var multiToggle = configTraverse.Field("button").GetValue<MultiToggle>();
				if (multiToggle == null || !multiToggle.gameObject.activeInHierarchy) continue;

				var headerLabel = configTraverse.Field("headerLabel").GetValue<LocText>();
				var hoverDesc = configTraverse.Field("hoverDescriptionText").GetValue<string>();

				string name = headerLabel != null ? headerLabel.text : configName;
				string label = !string.IsNullOrEmpty(hoverDesc)
					? $"{name}, {Speech.TextFilter.FilterForSpeech(hoverDesc)}"
					: name;

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = multiToggle,
					Type = WidgetType.Button,
					GameObject = multiToggle.gameObject
				});
			}
		}

		/// <summary>
		/// Discover Survival / No Sweat buttons on ModeSelectScreen.
		/// Each button is a MultiToggle field. We use the game's localized title
		/// strings and pair with description strings for a composite label.
		/// </summary>
		private void DiscoverModeSelectWidgets(KScreen screen) {
			var screenTraverse = Traverse.Create(screen);

			var survivalToggle = screenTraverse.Field("survivalButton").GetValue<MultiToggle>();
			if (survivalToggle != null && survivalToggle.gameObject.activeInHierarchy) {
				string name = STRINGS.UI.FRONTEND.MODESELECTSCREEN.SURVIVAL_TITLE;
				string desc = STRINGS.UI.FRONTEND.MODESELECTSCREEN.SURVIVAL_DESC;
				string label = $"{name}, {Speech.TextFilter.FilterForSpeech(desc)}";
				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = survivalToggle,
					Type = WidgetType.Button,
					GameObject = survivalToggle.gameObject
				});
			}

			var nosweatToggle = screenTraverse.Field("nosweatButton").GetValue<MultiToggle>();
			if (nosweatToggle != null && nosweatToggle.gameObject.activeInHierarchy) {
				string name = STRINGS.UI.FRONTEND.MODESELECTSCREEN.NOSWEAT_TITLE;
				string desc = STRINGS.UI.FRONTEND.MODESELECTSCREEN.NOSWEAT_DESC;
				string label = $"{name}, {Speech.TextFilter.FilterForSpeech(desc)}";
				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = nosweatToggle,
					Type = WidgetType.Button,
					GameObject = nosweatToggle.gameObject
				});
			}
		}

		/// <summary>
		/// Discover cluster/asteroid entries on ColonyDestinationSelectScreen.
		/// Uses DestinationSelectPanel's clusterKeys and asteroidData to build
		/// composite labels: "name, difficulty, traits".
		/// </summary>
		private void DiscoverClusterWidgets(KScreen screen) {
			var panelTraverse = Traverse.Create(screen).Field("destinationMapPanel");
			var panel = panelTraverse.GetValue<object>();
			if (panel == null) return;

			var pt = Traverse.Create(panel);
			var clusterKeys = pt.Field("clusterKeys").GetValue<System.Collections.Generic.List<string>>();
			var asteroidData = pt.Field("asteroidData")
				.GetValue<System.Collections.Generic.Dictionary<string, ColonyDestinationAsteroidBeltData>>();

			if (clusterKeys == null || asteroidData == null) return;

			for (int i = 0; i < clusterKeys.Count; i++) {
				string key = clusterKeys[i];
				if (!asteroidData.TryGetValue(key, out var belt)) continue;

				// Build composite label: name, difficulty, traits
				string name = belt.properName;
				if (string.IsNullOrEmpty(name))
					name = belt.startWorldName;

				// Difficulty from survivalOptions
				int diffIdx = UnityEngine.Mathf.Clamp(
					belt.difficulty, 0,
					ColonyDestinationAsteroidBeltData.survivalOptions.Count - 1);
				string difficulty = ColonyDestinationAsteroidBeltData.survivalOptions[diffIdx].first;

				// Get trait descriptors
				var traits = belt.GetTraitDescriptors();
				var traitNames = new List<string>();
				foreach (var trait in traits) {
					string traitText = Speech.TextFilter.FilterForSpeech(trait.text);
					if (!string.IsNullOrEmpty(traitText))
						traitNames.Add(traitText);
				}

				string traitStr = traitNames.Count > 0
					? string.Join(", ", traitNames)
					: "";

				string label = !string.IsNullOrEmpty(traitStr)
					? $"{Speech.TextFilter.FilterForSpeech(name)}, {Speech.TextFilter.FilterForSpeech(difficulty)}, {traitStr}"
					: $"{Speech.TextFilter.FilterForSpeech(name)}, {Speech.TextFilter.FilterForSpeech(difficulty)}";

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = null, // No single clickable component; activation selects via panel
					Type = WidgetType.Label,
					GameObject = null,
					Tag = key // Store cluster key for activation
				});
			}
		}

		/// <summary>
		/// Discover game settings widgets from NewGameSettingsPanel.
		/// Each CustomGameSettingListWidget has Label and ValueLabel LocTexts.
		/// </summary>
		private void DiscoverSettingsWidgets(KScreen screen) {
			var settingsPanel = Traverse.Create(screen).Field("newGameSettingsPanel").GetValue<object>();
			if (settingsPanel == null) return;

			var widgets = Traverse.Create(settingsPanel).Field("widgets")
				.GetValue<System.Collections.Generic.List<CustomGameSettingWidget>>();
			if (widgets == null) return;

			foreach (var widget in widgets) {
				if (widget == null || !widget.gameObject.activeInHierarchy) continue;

				// CustomGameSettingListWidget has Label and ValueLabel fields
				var wt = Traverse.Create(widget);
				var labelText = wt.Field("Label").GetValue<LocText>();
				var valueText = wt.Field("ValueLabel").GetValue<LocText>();

				string name = labelText != null ? labelText.text : "";
				string value = valueText != null ? valueText.text : "";

				if (string.IsNullOrEmpty(name)) continue;

				string label = !string.IsNullOrEmpty(value)
					? $"{name}, {value}"
					: name;

				_widgets.Add(new WidgetInfo {
					Label = label,
					Component = widget,
					Type = WidgetType.Dropdown,
					GameObject = widget.gameObject
				});
			}
		}

		/// <summary>
		/// Discover the seed input field on ColonyDestinationSelectScreen.
		/// </summary>
		private void DiscoverSeedWidgets(KScreen screen) {
			var coordinate = Traverse.Create(screen).Field("coordinate")
				.GetValue<KInputTextField>();
			if (coordinate == null) return;

			string currentValue = coordinate.text ?? "";
			_widgets.Add(new WidgetInfo {
				Label = $"{STRINGS.ONIACCESS.PANELS.SEED}, {currentValue}",
				Component = coordinate,
				Type = WidgetType.TextInput,
				GameObject = coordinate.gameObject
			});
		}

		// ========================================
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// For Dropdown (settings), read current label + value live.
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			if (widget.Type == WidgetType.Dropdown && widget.Component is CustomGameSettingWidget settingWidget) {
				var wt = Traverse.Create(settingWidget);
				var labelText = wt.Field("Label").GetValue<LocText>();
				var valueText = wt.Field("ValueLabel").GetValue<LocText>();
				string name = labelText != null ? labelText.text : "";
				string value = valueText != null ? valueText.text : "";
				return !string.IsNullOrEmpty(value) ? $"{name}, {value}" : name;
			}

			if (widget.Type == WidgetType.TextInput && widget.Component is KInputTextField textField) {
				return $"{STRINGS.ONIACCESS.PANELS.SEED}, {textField.text}";
			}

			return base.GetWidgetSpeechText(widget);
		}

		// ========================================
		// WIDGET INTERACTION
		// ========================================

		/// <summary>
		/// Activate the current widget:
		/// - Game mode buttons: invoke MultiToggle.onClick (selects mode, may auto-advance)
		/// - Cluster entries: select that cluster via DestinationSelectPanel
		/// - Text input: enter edit mode
		/// - Other: base behavior
		/// </summary>
		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			// Game mode: MultiToggle click
			if ((IsClusterCategoryScreen || IsModeSelectScreen) && widget.Component is MultiToggle toggle) {
				toggle.onClick?.Invoke();
				return;
			}

			// Cluster entry: select via panel
			if (!IsClusterCategoryScreen && _currentPanel == PanelClusters && widget.Tag is string clusterKey) {
				SelectCluster(clusterKey);
				return;
			}

			// Text input: enter edit mode
			if (widget.Type == WidgetType.TextInput && widget.Component is KInputTextField textField) {
				if (!_isEditingText) {
					_cachedTextValue = textField.text;
					_isEditingText = true;
					textField.ActivateInputField();
					Speech.SpeechPipeline.SpeakInterrupt($"Editing, {textField.text}");
				} else {
					// Enter confirms
					_isEditingText = false;
					textField.DeactivateInputField();
					Speech.SpeechPipeline.SpeakInterrupt($"Confirmed, {textField.text}");
				}
				return;
			}

			base.ActivateCurrentWidget();
		}

		/// <summary>
		/// Select a cluster by key via the DestinationSelectPanel.
		/// Fires OnAsteroidClicked which updates the screen.
		/// </summary>
		private void SelectCluster(string clusterKey) {
			var panelTraverse = Traverse.Create(_screen).Field("destinationMapPanel");
			var panel = panelTraverse.GetValue<object>();
			if (panel == null) return;

			var pt = Traverse.Create(panel);
			var asteroidData = pt.Field("asteroidData")
				.GetValue<System.Collections.Generic.Dictionary<string, ColonyDestinationAsteroidBeltData>>();

			if (asteroidData != null && asteroidData.TryGetValue(clusterKey, out var belt)) {
				// Fire the OnAsteroidClicked event which updates the screen
				var onClicked = pt.Field("OnAsteroidClicked")
					.GetValue<System.Action<ColonyDestinationAsteroidBeltData>>();
				onClicked?.Invoke(belt);
				Speech.SpeechPipeline.SpeakInterrupt($"Selected, {Speech.TextFilter.FilterForSpeech(belt.properName)}");
			}
		}

		/// <summary>
		/// Cycle dropdown for settings widgets.
		/// CustomGameSettingListWidget has CycleLeft/CycleRight KButtons.
		/// We invoke the appropriate cycle direction.
		/// </summary>
		protected override void CycleDropdown(WidgetInfo widget, int direction) {
			if (!(widget.Component is CustomGameSettingWidget settingWidget)) return;

			// CustomGameSettingListWidget has CycleLeft and CycleRight KButtons
			var wt = Traverse.Create(settingWidget);

			if (direction > 0) {
				var cycleRight = wt.Field("CycleRight").GetValue<KButton>();
				if (cycleRight != null && cycleRight.isInteractable)
					cycleRight.SignalClick(KKeyCode.Mouse0);
			} else {
				var cycleLeft = wt.Field("CycleLeft").GetValue<KButton>();
				if (cycleLeft != null && cycleLeft.isInteractable)
					cycleLeft.SignalClick(KKeyCode.Mouse0);
			}

			// Read the new value after cycling
			Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(widget));
		}

		// ========================================
		// TICK: TEXT EDIT MODE
		// ========================================

		/// <summary>
		/// When in text edit mode, only check Return (to confirm edit).
		/// Otherwise, delegate to base for normal menu navigation.
		/// </summary>
		public override void Tick() {
			if (_isEditingText) {
				// Return/Enter confirms text edit
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					ConfirmTextEdit();
				}
				// Block all other key handling during text edit
				return;
			}

			base.Tick();
		}

		/// <summary>
		/// When in text edit mode, intercept Escape to cancel editing
		/// and restore the cached value.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e)) return true;

			if (_isEditingText) {
				// Escape cancels text edit
				if (e.TryConsume(Action.Escape)) {
					CancelTextEdit();
					return true;
				}
				// Let all other keys pass through to the input field
				return false;
			}

			return false;
		}

		private void CancelTextEdit() {
			_isEditingText = false;
			if (_currentIndex >= 0 && _currentIndex < _widgets.Count) {
				var widget = _widgets[_currentIndex];
				if (widget.Component is KInputTextField textField) {
					textField.text = _cachedTextValue;
					textField.DeactivateInputField();
				}
			}
			Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedTextValue}");
		}

		private void ConfirmTextEdit() {
			_isEditingText = false;
			if (_currentIndex >= 0 && _currentIndex < _widgets.Count) {
				var widget = _widgets[_currentIndex];
				if (widget.Component is KInputTextField textField) {
					textField.DeactivateInputField();
					Speech.SpeechPipeline.SpeakInterrupt($"Confirmed, {textField.text}");
				}
			}
		}
	}
}
