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
	/// ColonyDestinationSelectScreen: five tabbed panels with Tab/Shift+Tab switching:
	///   Clusters → Story Traits → Mixing → Settings → Actions
	///
	/// Per locked decisions:
	/// - Game mode entries speak name + description together
	/// - Cluster entries speak name, description, difficulty, moons, traits, selected
	/// - Story traits speak name + guaranteed/forbidden state; Enter toggles
	/// - Mixing DLC toggles speak name + enabled/disabled; Enter toggles
	/// - Mixing cyclers speak name + value; Left/Right cycles
	/// - Settings speak "label, value" with Left/Right cycling
	/// - Actions panel: Back, Shuffle, Coordinate field, Launch
	/// - Tab/Shift+Tab switches panels on destination screen only
	/// </summary>
	public class ColonySetupHandler: BaseMenuHandler {
		/// <summary>
		/// The five logical panels on ColonyDestinationSelectScreen.
		/// </summary>
		private const int PanelClusters = 0;
		private const int PanelStoryTraits = 1;
		private const int PanelMixing = 2;
		private const int PanelSettings = 3;
		private const int PanelActions = 4;
		private const int PanelCount = 5;

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
		/// Whether we are in the Shift+I info submenu for a cluster.
		/// </summary>
		private bool _inInfoSubmenu;

		/// <summary>
		/// Cluster key stored when entering the info submenu, used to restore position on exit.
		/// </summary>
		private string _infoClusterKey;

		/// <summary>
		/// Index of the cluster in the list before entering info submenu.
		/// </summary>
		private int _infoReturnIndex;

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
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_PANEL));
		}

		// ========================================
		// TAB NAVIGATION (ColonyDestinationSelectScreen only)
		// ========================================

		protected override void NavigateTabForward() {
			if (IsClusterCategoryScreen || IsModeSelectScreen) return;

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
		/// Sync the game's visible tab with our _currentPanel index.
		/// The game hides non-selected tab panels via SetActive(false), so we
		/// must tell it to switch before discovering widgets.
		/// </summary>
		private void SyncGameTab() {
			if (IsClusterCategoryScreen || IsModeSelectScreen) return;
			if (_currentPanel > PanelSettings) return; // Actions has no game tab
			int gameTabIdx = _currentPanel + 1; // Our 0-3 maps to game's 1-4
			var st = Traverse.Create(_screen);
			st.Field("selectedMenuTabIdx").SetValue(gameTabIdx);
			st.Method("RefreshMenuTabs").GetValue();
		}

		/// <summary>
		/// Re-discover widgets for the current panel and announce it.
		/// </summary>
		private void RediscoverForCurrentPanel() {
			_inInfoSubmenu = false;
			_search.Clear();
			SyncGameTab();
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
				case PanelStoryTraits: return STRINGS.ONIACCESS.PANELS.STORY_TRAITS;
				case PanelMixing: return STRINGS.ONIACCESS.PANELS.MIXING;
				case PanelSettings: return STRINGS.ONIACCESS.PANELS.SETTINGS;
				case PanelActions: return STRINGS.ONIACCESS.PANELS.ACTIONS;
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
						if (_inInfoSubmenu)
							DiscoverClusterInfoWidgets(screen);
						else
							DiscoverClusterWidgets(screen);
						break;
					case PanelStoryTraits:
						DiscoverStoryTraitWidgets(screen);
						break;
					case PanelMixing:
						DiscoverMixingWidgets(screen);
						break;
					case PanelSettings:
						DiscoverSettingsWidgets(screen);
						break;
					case PanelActions:
						DiscoverActionWidgets(screen);
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
			int selectedIndex = pt.Field("selectedIndex").GetValue<int>();

			if (clusterKeys == null || asteroidData == null) return;

			for (int i = 0; i < clusterKeys.Count; i++) {
				string key = clusterKeys[i];
				if (!asteroidData.TryGetValue(key, out var belt)) continue;

				// Cluster name: properName is a string table key, resolve via Strings.Get
				string name = "";
				string rawName = belt.properName;
				if (!string.IsNullOrEmpty(rawName))
					name = Strings.Get(rawName);
				if (string.IsNullOrEmpty(name))
					name = belt.startWorldName;

				// Difficulty from survivalOptions
				int diffIdx = UnityEngine.Mathf.Clamp(
					belt.difficulty, 0,
					ColonyDestinationAsteroidBeltData.survivalOptions.Count - 1);
				string difficulty = ColonyDestinationAsteroidBeltData.survivalOptions[diffIdx].first;

				// Trait count — only actual world traits (colored entries)
				var traits = belt.GetTraitDescriptors();
				int traitCount = 0;
				foreach (var trait in traits) {
					string t = trait.text?.Trim() ?? "";
					if (t.StartsWith("<color")) traitCount++;
				}

				// Moon count (Spaced Out only)
				int moonCount = belt.worlds != null ? belt.worlds.Count : 0;

				// Build slim label: [selected], name, difficulty, N traits, N moons, Shift I for info
				string label = "";
				if (i == selectedIndex)
					label = $"{STRINGS.ONIACCESS.STATES.SELECTED}, ";
				label += Speech.TextFilter.FilterForSpeech(name);
				label += $", {Speech.TextFilter.FilterForSpeech(difficulty)}";
				label += $", {traitCount} {STRINGS.ONIACCESS.INFO.TRAITS}";
				if (moonCount > 0)
					label += $", {moonCount} {STRINGS.ONIACCESS.INFO.MOONS}";
				label += $", {STRINGS.ONIACCESS.INFO.SHIFT_I_HINT}";

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
		/// Build info submenu widgets for the cluster stored in _infoClusterKey.
		/// Shows description, difficulty, moons, and per-world traits as read-only labels.
		/// </summary>
		private void DiscoverClusterInfoWidgets(KScreen screen) {
			var panelTraverse = Traverse.Create(screen).Field("destinationMapPanel");
			var panel = panelTraverse.GetValue<object>();
			if (panel == null) return;

			var pt = Traverse.Create(panel);
			var asteroidData = pt.Field("asteroidData")
				.GetValue<System.Collections.Generic.Dictionary<string, ColonyDestinationAsteroidBeltData>>();
			if (asteroidData == null || !asteroidData.TryGetValue(_infoClusterKey, out var belt)) return;

			// Description
			var startWorld = belt.GetStartWorld;
			if (startWorld != null) {
				string desc = Strings.Get(startWorld.description);
				if (!string.IsNullOrEmpty(desc)) {
					_widgets.Add(new WidgetInfo {
						Label = $"{STRINGS.ONIACCESS.INFO.DESCRIPTION}: {Speech.TextFilter.FilterForSpeech(desc)}",
						Type = WidgetType.Label
					});
				}
			}

			// Difficulty + flavor text
			int diffIdx = UnityEngine.Mathf.Clamp(
				belt.difficulty, 0,
				ColonyDestinationAsteroidBeltData.survivalOptions.Count - 1);
			string diffName = ColonyDestinationAsteroidBeltData.survivalOptions[diffIdx].first;
			string diffTooltip = ColonyDestinationAsteroidBeltData.survivalOptions[diffIdx].second;
			string diffLabel = $"{STRINGS.ONIACCESS.INFO.DIFFICULTY}: {Speech.TextFilter.FilterForSpeech(diffName)}";
			if (!string.IsNullOrEmpty(diffTooltip))
				diffLabel += $" — {Speech.TextFilter.FilterForSpeech(diffTooltip)}";
			_widgets.Add(new WidgetInfo {
				Label = diffLabel,
				Type = WidgetType.Label
			});

			// Moons (Spaced Out only)
			if (belt.worlds != null && belt.worlds.Count > 0) {
				var moonNames = new List<string>();
				foreach (var world in belt.worlds) {
					string moonName = Strings.Get(world.name);
					if (!string.IsNullOrEmpty(moonName))
						moonNames.Add(Speech.TextFilter.FilterForSpeech(moonName));
				}
				string moonsLabel = moonNames.Count > 0
					? $"{STRINGS.ONIACCESS.INFO.MOONS}: {string.Join(", ", moonNames)}"
					: $"{STRINGS.ONIACCESS.INFO.MOONS}: {STRINGS.ONIACCESS.INFO.NO_MOONS}";
				_widgets.Add(new WidgetInfo {
					Label = moonsLabel,
					Type = WidgetType.Label
				});
			}

			// World traits header
			_widgets.Add(new WidgetInfo {
				Label = $"{STRINGS.ONIACCESS.INFO.WORLD_TRAITS}:",
				Type = WidgetType.Label
			});

			// Traits — grouped per-world in Spaced Out (bold world name headers)
			var traitDescriptors = belt.GetTraitDescriptors();
			foreach (var trait in traitDescriptors) {
				string text = trait.text?.Trim() ?? "";
				if (string.IsNullOrEmpty(text)) continue;

				// Bold world name headers → emit as standalone header widget
				if (text.StartsWith("<b>")) {
					string worldName = Speech.TextFilter.FilterForSpeech(text);
					_widgets.Add(new WidgetInfo {
						Label = $"{worldName} {STRINGS.ONIACCESS.INFO.TRAITS}:",
						Type = WidgetType.Label
					});
					continue;
				}

				// Game's "No Traits" placeholder (<i> italic text) — use actual game text
				if (text.StartsWith("<i>")) {
					_widgets.Add(new WidgetInfo {
						Label = Speech.TextFilter.FilterForSpeech(text),
						Type = WidgetType.Label
					});
					continue;
				}

				// Skip plain-text entries — these are story traits injected by the game
				if (!text.StartsWith("<color")) continue;

				// Actual world trait entry — name + tooltip description
				string traitLabel = Speech.TextFilter.FilterForSpeech(text);
				string tooltip = trait.tooltip?.Trim() ?? "";
				if (!string.IsNullOrEmpty(tooltip))
					traitLabel += $" — {Speech.TextFilter.FilterForSpeech(tooltip)}";
				_widgets.Add(new WidgetInfo {
					Label = traitLabel,
					Type = WidgetType.Label
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
				if (widget == null || !widget.gameObject.activeSelf) continue;

				if (widget is CustomGameSettingListWidget listWidget) {
					var wt = Traverse.Create(listWidget);
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
				} else if (widget is CustomGameSettingToggleWidget toggleWidget) {
					var wt = Traverse.Create(toggleWidget);
					var labelText = wt.Field("Label").GetValue<LocText>();
					var toggle = wt.Field("Toggle").GetValue<MultiToggle>();
					string name = labelText != null ? labelText.text : "";
					if (string.IsNullOrEmpty(name)) continue;
					string state = (toggle != null && toggle.CurrentState == 1) ? "enabled" : "disabled";
					_widgets.Add(new WidgetInfo {
						Label = $"{name}, {state}",
						Component = widget,
						Type = WidgetType.Toggle,
						GameObject = widget.gameObject
					});
				} else if (widget is CustomGameSettingSeed seedWidget) {
					var wt = Traverse.Create(seedWidget);
					var labelText = wt.Field("Label").GetValue<LocText>();
					var inputField = wt.Field("Input").GetValue<KInputTextField>();
					string name = labelText != null ? labelText.text : (string)STRINGS.ONIACCESS.PANELS.SEED;
					string value = inputField != null ? inputField.text : "";
					_widgets.Add(new WidgetInfo {
						Label = $"{name}, {value}",
						Component = widget,
						Type = WidgetType.Button, // Enter randomizes seed
						GameObject = widget.gameObject
					});
				}
			}
		}

		/// <summary>
		/// Discover story trait entries from StoryContentPanel.
		/// Each row has a label and a checkbox MultiToggle for toggling
		/// Forbidden/Guaranteed state.
		/// </summary>
		private void DiscoverStoryTraitWidgets(KScreen screen) {
			// storyTraitShuffleButton lives on ColonyDestinationSelectScreen
			WidgetDiscoveryUtil.TryAddButtonField(screen, "storyTraitShuffleButton", null, _widgets);

			var storyPanel = Traverse.Create(screen).Field("storyContentPanel").GetValue<object>();
			if (storyPanel == null) {
				Util.Log.Debug("StoryTraits: storyContentPanel is null");
				return;
			}

			var spt = Traverse.Create(storyPanel);

			// Walk storyRowContainer children instead of the storyRows dictionary.
			// The private storyStates dict uses a private enum value type which
			// Traverse can't extract as IDictionary. Active children are spawned
			// in Db.Get().Stories.resources order; child 0 is the inactive prefab
			// template, so we track a separate story index for active rows only.
			var containerGO = spt.Field("storyRowContainer").GetValue<UnityEngine.GameObject>();
			if (containerGO == null) {
				Util.Log.Debug("StoryTraits: storyRowContainer is null");
				return;
			}

			var stories = Db.Get().Stories.resources;
			var container = containerGO.transform;
			bool isPureVanilla = DlcManager.IsPureVanilla();
			int storyIdx = 0;

			for (int i = 0; i < container.childCount; i++) {
				try {
					var rowGO = container.GetChild(i).gameObject;
					if (rowGO == null || !rowGO.activeSelf) continue;
					if (storyIdx >= stories.Count) break;

					var hierRef = rowGO.GetComponent<HierarchyReferences>();
					if (hierRef == null) { storyIdx++; continue; }

					// Get name from the database — LocText.text is empty at discovery time
					string name = "";
					var storyTrait = stories[storyIdx].StoryTrait;
					if (storyTrait != null)
						name = Strings.Get(storyTrait.name);
					if (string.IsNullOrEmpty(name)) { storyIdx++; continue; }

					MultiToggle checkbox = null;
					if (hierRef.HasReference("checkbox"))
						checkbox = hierRef.GetReference<MultiToggle>("checkbox");
					if (checkbox == null) { storyIdx++; continue; }

					string storyId = stories[storyIdx].Id;

					// Read state from public CustomGameSettings API, not the private dict
					string state = STRINGS.ONIACCESS.STATES.FORBIDDEN;
					try {
						var level = CustomGameSettings.Instance.GetCurrentStoryTraitSetting(storyId);
						if (level != null && level.id == "Guaranteed")
							state = STRINGS.ONIACCESS.STATES.GUARANTEED;
					} catch (System.Exception) { }

					// Build label with description
					string label = $"{name}, {state}";
					try {
						if (storyTrait != null) {
							string desc = isPureVanilla
								? Strings.Get(storyTrait.description + "_SHORT")
								: Strings.Get(storyTrait.description);
							if (!string.IsNullOrEmpty(desc))
								label = $"{name}, {state} — {Speech.TextFilter.FilterForSpeech(desc)}";
						}
					} catch (System.Exception) { }

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = checkbox,
						Type = WidgetType.Toggle,
						GameObject = checkbox.gameObject,
						Tag = storyId
					});
					storyIdx++;
				} catch (System.Exception) {
					storyIdx++;
				}
			}
		}

		/// <summary>
		/// Discover mixing widgets from MixingContentPanel.
		/// Walks contentPanel children (sections) instead of the flat widget list
		/// so that section headers (DLC Content, Asteroid Mixing, Biome Mixing)
		/// are announced before each group of widgets.
		/// </summary>
		private void DiscoverMixingWidgets(KScreen screen) {
			var mixingPanel = Traverse.Create(screen).Field("mixingPanel").GetValue<object>();
			if (mixingPanel == null) return;

			var contentPanelGO = Traverse.Create(mixingPanel).Field("contentPanel")
				.GetValue<UnityEngine.GameObject>();
			if (contentPanelGO == null) return;

			var contentPanel = contentPanelGO.transform;
			for (int s = 0; s < contentPanel.childCount; s++) {
				var section = contentPanel.GetChild(s);
				if (section == null || !section.gameObject.activeSelf) continue;

				// Read section title from "Title/Title Text"
				var titleTransform = section.Find("Title");
				if (titleTransform == null) continue;
				var titleTextTransform = titleTransform.Find("Title Text");
				string sectionName = "";
				if (titleTextTransform != null) {
					var titleLocText = titleTextTransform.GetComponent<LocText>();
					if (titleLocText != null) {
						sectionName = titleLocText.text ?? "";
						if (string.IsNullOrEmpty(sectionName)) {
							// Fallback: LocText.key stores the string table key
							string key = Traverse.Create(titleLocText).Field("key").GetValue<string>();
							if (!string.IsNullOrEmpty(key))
								sectionName = Strings.Get(new StringKey(key));
						}
						if (string.IsNullOrEmpty(sectionName))
							Util.Log.Debug($"Mixing section {s}: text='{titleLocText.text}' key='{Traverse.Create(titleLocText).Field("key").GetValue<string>()}'");
					}
				}

				// Check if section content is visible (collapsible toggle)
				var contentTransform = section.Find("Content");
				if (contentTransform == null || !contentTransform.gameObject.activeSelf) continue;

				var gridTransform = contentTransform.Find("Grid");
				if (gridTransform == null) continue;

				// Count visible children in the grid
				int visibleCount = 0;
				for (int c = 0; c < gridTransform.childCount; c++) {
					if (gridTransform.GetChild(c).gameObject.activeSelf) visibleCount++;
				}

				// If no visible widgets, check for "no options" label
				if (visibleCount == 0) {
					var noOptionsTransform = contentTransform.Find("LabelNoOptions");
					if (noOptionsTransform != null && noOptionsTransform.gameObject.activeSelf)
						continue; // Skip empty section entirely
				}

				// Emit section header
				if (!string.IsNullOrEmpty(sectionName)) {
					_widgets.Add(new WidgetInfo {
						Label = sectionName,
						Type = WidgetType.Label
					});
				}

				// Walk grid children for actual setting widgets
				for (int c = 0; c < gridTransform.childCount; c++) {
					var widgetGO = gridTransform.GetChild(c);
					if (widgetGO == null || !widgetGO.gameObject.activeSelf) continue;

					var widget = widgetGO.GetComponent<CustomGameSettingWidget>();
					if (widget == null) continue;

					// DLC toggle: has "Checkbox" child
					var checkboxTransform = widgetGO.Find("Checkbox");
					if (checkboxTransform != null) {
						var overlayDisabled = checkboxTransform.Find("OverlayDisabled");
						if (overlayDisabled != null && overlayDisabled.gameObject.activeSelf) continue;

						var toggle = checkboxTransform.GetComponent<MultiToggle>();
						if (toggle == null) continue;

						var labelLocText = widgetGO.Find("Label");
						string name = "";
						if (labelLocText != null) {
							var lt = labelLocText.GetComponent<LocText>();
							if (lt != null) name = lt.text;
						}
						if (string.IsNullOrEmpty(name)) continue;

						string state = toggle.CurrentState == 0 ? "enabled" : "disabled";
						_widgets.Add(new WidgetInfo {
							Label = $"{name}, {state}",
							Component = toggle,
							Type = WidgetType.Toggle,
							GameObject = widget.gameObject
						});
						continue;
					}

					// Cycler widget: has "Cycler" child
					var cyclerTransform = widgetGO.Find("Cycler");
					if (cyclerTransform != null) {
						var overlayDisabled = cyclerTransform.Find("OverlayDisabled");
						if (overlayDisabled != null && overlayDisabled.gameObject.activeSelf) continue;

						var labelLocText = widgetGO.Find("Label");
						string name = "";
						if (labelLocText != null) {
							var lt = labelLocText.GetComponent<LocText>();
							if (lt != null) name = lt.text;
						}
						if (string.IsNullOrEmpty(name)) continue;

						string value = "";
						var boxTransform = cyclerTransform.Find("Box");
						if (boxTransform != null) {
							var valueLabelTransform = boxTransform.Find("Value Label");
							if (valueLabelTransform != null) {
								var vlt = valueLabelTransform.GetComponent<LocText>();
								if (vlt != null) value = vlt.text;
							}
						}

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
			}
		}

		/// <summary>
		/// Discover action buttons and coordinate field on ColonyDestinationSelectScreen.
		/// </summary>
		private void DiscoverActionWidgets(KScreen screen) {
			WidgetDiscoveryUtil.TryAddButtonField(screen, "backButton", null, _widgets);
			WidgetDiscoveryUtil.TryAddButtonField(screen, "shuffleButton", null, _widgets);

			// Coordinate text field
			try {
				var coordinate = Traverse.Create(screen).Field("coordinate")
					.GetValue<KInputTextField>();
				if (coordinate != null && coordinate.gameObject.activeInHierarchy) {
					string currentValue = coordinate.text ?? "";
					_widgets.Add(new WidgetInfo {
						Label = $"{STRINGS.ONIACCESS.PANELS.COORDINATE}, {currentValue}",
						Component = coordinate,
						Type = WidgetType.TextInput,
						GameObject = coordinate.gameObject
					});
				}
			} catch (System.Exception) { }

			WidgetDiscoveryUtil.TryAddButtonField(screen, "launchButton", null, _widgets);
		}

		// ========================================
		// WIDGET VALIDITY
		// ========================================

		/// <summary>
		/// Accept MultiToggle as valid Toggle (story traits, mixing DLC toggles).
		/// Cluster entries have null GameObject — base handles them as Label.
		/// </summary>
		protected override bool IsWidgetValid(WidgetInfo widget) {
			if (widget == null) return false;
			if (widget.Type == WidgetType.Toggle) {
				var mt = widget.Component as MultiToggle;
				if (mt != null) {
					if (widget.GameObject == null) return false;
					return widget.GameObject.activeInHierarchy;
				}
			}
			return base.IsWidgetValid(widget);
		}

		// ========================================
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// Read widget state live for each panel type.
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			// Story trait toggles: re-read state live via CustomGameSettings
			if (_currentPanel == PanelStoryTraits && widget.Type == WidgetType.Toggle
				&& widget.Tag is string storyId) {
				string state = STRINGS.ONIACCESS.STATES.FORBIDDEN;
				try {
					var level = CustomGameSettings.Instance.GetCurrentStoryTraitSetting(storyId);
					if (level != null && level.id == "Guaranteed")
						state = STRINGS.ONIACCESS.STATES.GUARANTEED;
				} catch (System.Exception) { }

				// Re-read the name from the database (LocText.text may be empty)
				string name = "";
				try {
					var story = Db.Get().Stories.Get(storyId);
					if (story?.StoryTrait != null)
						name = Strings.Get(story.StoryTrait.name);
				} catch (System.Exception) { }
				if (string.IsNullOrEmpty(name)) name = widget.Label;

				// Include trait description
				string label = $"{name}, {state}";
				try {
					bool isPureVanilla = DlcManager.IsPureVanilla();
					var story = Db.Get().Stories.Get(storyId);
					if (story?.StoryTrait != null) {
						string desc = isPureVanilla
							? Strings.Get(story.StoryTrait.description + "_SHORT")
							: Strings.Get(story.StoryTrait.description);
						if (!string.IsNullOrEmpty(desc))
							label = $"{name}, {state} — {Speech.TextFilter.FilterForSpeech(desc)}";
					}
				} catch (System.Exception) { }
				return label;
			}

			// Mixing DLC toggles: read CurrentState live
			if (_currentPanel == PanelMixing && widget.Type == WidgetType.Toggle) {
				var mt = widget.Component as MultiToggle;
				if (mt != null) {
					string state = mt.CurrentState == 0 ? "enabled" : "disabled";
					// Read label from widget's "Label" child LocText
					string name = "";
					if (widget.GameObject != null) {
						var labelTransform = widget.GameObject.transform.Find("Label");
						if (labelTransform != null) {
							var lt = labelTransform.GetComponent<LocText>();
							if (lt != null) name = lt.text;
						}
					}
					if (string.IsNullOrEmpty(name)) name = widget.Label;
					return $"{name}, {state}";
				}
			}

			// Mixing cyclers and game settings dropdowns
			if (widget.Type == WidgetType.Dropdown && widget.Component is CustomGameSettingWidget settingWidget) {
				// Try standard settings fields first (Label + ValueLabel)
				var wt = Traverse.Create(settingWidget);
				var labelText = wt.Field("Label").GetValue<LocText>();
				var valueText = wt.Field("ValueLabel").GetValue<LocText>();
				string name = labelText != null ? labelText.text : "";
				string value = valueText != null ? valueText.text : "";

				// Fallback for mixing cyclers: read from "Cycler/Box/Value Label"
				if (string.IsNullOrEmpty(value) && settingWidget.gameObject != null) {
					var cyclerTransform = settingWidget.transform.Find("Cycler");
					if (cyclerTransform != null) {
						var boxTransform = cyclerTransform.Find("Box");
						if (boxTransform != null) {
							var valueLabelTransform = boxTransform.Find("Value Label");
							if (valueLabelTransform != null) {
								var vlt = valueLabelTransform.GetComponent<LocText>();
								if (vlt != null) value = vlt.text;
							}
						}
					}
				}

				return !string.IsNullOrEmpty(value) ? $"{name}, {value}" : name;
			}

			// Settings toggle widgets: read Toggle.CurrentState live
			if (_currentPanel == PanelSettings && widget.Component is CustomGameSettingToggleWidget toggleWidget) {
				var twt = Traverse.Create(toggleWidget);
				var labelText = twt.Field("Label").GetValue<LocText>();
				var toggle = twt.Field("Toggle").GetValue<MultiToggle>();
				string name = labelText != null ? labelText.text : widget.Label;
				string state = (toggle != null && toggle.CurrentState == 1) ? "enabled" : "disabled";
				return $"{name}, {state}";
			}

			// Settings seed widget: read Input.text live
			if (_currentPanel == PanelSettings && widget.Component is CustomGameSettingSeed seedWidget) {
				var swt = Traverse.Create(seedWidget);
				var labelText = swt.Field("Label").GetValue<LocText>();
				var inputField = swt.Field("Input").GetValue<KInputTextField>();
				string name = labelText != null ? labelText.text : (string)STRINGS.ONIACCESS.PANELS.SEED;
				string value = inputField != null ? inputField.text : "";
				return $"{name}, {value}";
			}

			// Coordinate text field
			if (widget.Type == WidgetType.TextInput && widget.Component is KInputTextField textField) {
				return $"{STRINGS.ONIACCESS.PANELS.COORDINATE}, {textField.text}";
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

			// Story trait toggle: invoke checkbox onClick, then speak new state
			if (_currentPanel == PanelStoryTraits && widget.Type == WidgetType.Toggle
				&& widget.Component is MultiToggle storyCheckbox) {
				storyCheckbox.onClick?.Invoke();
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(widget));
				return;
			}

			// Mixing DLC toggle: invoke MultiToggle onClick, then speak new state
			if (_currentPanel == PanelMixing && widget.Type == WidgetType.Toggle
				&& widget.Component is MultiToggle mixingToggle) {
				mixingToggle.onClick?.Invoke();
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(widget));
				return;
			}

			// Settings toggle: call ToggleSetting(), then speak new state
			if (_currentPanel == PanelSettings && widget.Component is CustomGameSettingToggleWidget settingsToggle) {
				settingsToggle.ToggleSetting();
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(widget));
				return;
			}

			// Settings seed: randomize, then speak new value
			if (_currentPanel == PanelSettings && widget.Component is CustomGameSettingSeed settingsSeed) {
				var randomizeBtn = Traverse.Create(settingsSeed).Field("RandomizeButton")
					.GetValue<KButton>();
				if (randomizeBtn != null)
					randomizeBtn.SignalClick(KKeyCode.Mouse0);
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(widget));
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
				string selectedName = Strings.Get(belt.properName);
				if (string.IsNullOrEmpty(selectedName))
					selectedName = belt.startWorldName;
				Speech.SpeechPipeline.SpeakInterrupt($"Selected, {Speech.TextFilter.FilterForSpeech(selectedName)}");
			}
		}

		/// <summary>
		/// Cycle dropdown for settings widgets.
		/// CustomGameSettingListWidget has CycleLeft/CycleRight KButtons.
		/// We invoke the appropriate cycle direction.
		/// </summary>
		protected override void CycleDropdown(WidgetInfo widget, int direction) {
			if (!(widget.Component is CustomGameSettingWidget settingWidget)) return;

			var wt = Traverse.Create(settingWidget);
			bool cycled = false;

			// Try standard CycleLeft/CycleRight fields (game settings)
			if (direction > 0) {
				var cycleRight = wt.Field("CycleRight").GetValue<KButton>();
				if (cycleRight != null && cycleRight.isInteractable) {
					cycleRight.SignalClick(KKeyCode.Mouse0);
					cycled = true;
				}
			} else {
				var cycleLeft = wt.Field("CycleLeft").GetValue<KButton>();
				if (cycleLeft != null && cycleLeft.isInteractable) {
					cycleLeft.SignalClick(KKeyCode.Mouse0);
					cycled = true;
				}
			}

			// Fallback: try Cycler/Arrow_Left and Cycler/Arrow_Right (mixing widgets)
			if (!cycled) {
				var cyclerTransform = settingWidget.transform.Find("Cycler");
				if (cyclerTransform != null) {
					string arrowName = direction > 0 ? "Arrow_Right" : "Arrow_Left";
					var arrowTransform = cyclerTransform.Find(arrowName);
					if (arrowTransform != null) {
						var arrowButton = arrowTransform.GetComponent<KButton>();
						if (arrowButton != null && arrowButton.isInteractable)
							arrowButton.SignalClick(KKeyCode.Mouse0);
					}
				}
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

			// Shift+I opens cluster info submenu
			if (!IsClusterCategoryScreen && !IsModeSelectScreen
				&& _currentPanel == PanelClusters && !_inInfoSubmenu
				&& UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)
				&& (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
					|| UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift))) {
				if (_currentIndex >= 0 && _currentIndex < _widgets.Count
					&& _widgets[_currentIndex].Tag is string clusterKey) {
					_inInfoSubmenu = true;
					_infoClusterKey = clusterKey;
					_infoReturnIndex = _currentIndex;
					_search.Clear();
					DiscoverWidgets(_screen);
					Speech.SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.INFO.PANEL_NAME);
					if (_widgets.Count > 0) {
						_currentIndex = 0;
						Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
					}
				}
				return;
			}

			base.Tick();
		}

		/// <summary>
		/// When in text edit mode, intercept Escape to cancel editing
		/// and restore the cached value.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (_isEditingText) {
				// Escape cancels text edit — check before base so it doesn't
				// get consumed as a search-clear
				if (e.TryConsume(Action.Escape)) {
					CancelTextEdit();
					return true;
				}
				// Let all other keys pass through to the input field
				return false;
			}

			// Escape exits info submenu back to cluster list
			if (_inInfoSubmenu) {
				if (e.TryConsume(Action.Escape)) {
					_inInfoSubmenu = false;
					_search.Clear();
					DiscoverWidgets(_screen);
					_currentIndex = UnityEngine.Mathf.Clamp(_infoReturnIndex, 0,
						_widgets.Count > 0 ? _widgets.Count - 1 : 0);
					if (_currentIndex < _widgets.Count)
						Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[_currentIndex]));
					return true;
				}
			}

			if (base.HandleKeyDown(e)) return true;

			return false;
		}

		private void CancelTextEdit() {
			_isEditingText = false;
			if (_currentIndex >= 0 && _currentIndex < _widgets.Count
				&& _widgets[_currentIndex].Component is KInputTextField textField) {
				textField.text = _cachedTextValue;
				textField.DeactivateInputField();
			}
			Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedTextValue}");
		}

		private void ConfirmTextEdit() {
			_isEditingText = false;
			if (_currentIndex >= 0 && _currentIndex < _widgets.Count
				&& _widgets[_currentIndex].Component is KInputTextField textField) {
				textField.DeactivateInputField();
				Speech.SpeechPipeline.SpeakInterrupt($"Confirmed, {textField.text}");
			} else {
				Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedTextValue}");
			}
		}
	}
}
