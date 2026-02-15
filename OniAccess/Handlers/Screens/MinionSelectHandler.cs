using System.Collections.Generic;
using Database;
using HarmonyLib;
using Klei.AI;

using OniAccess.Input;
using OniAccess.Widgets;
namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for MinionSelectScreen (initial colony start — full game start screen)
	/// and Printing Pod selection (recurring every 3 cycles).
	///
	/// Two-level navigation:
	/// TOP LEVEL (Up/Down): Colony name, Shuffle name, Select duplicants, Embark
	/// DUPE MODE (Up/Down within slot, Tab/Shift+Tab between slots):
	///   name, interests, traits, expectations (stress/joy), attributes, description, interest filter, reroll
	///
	/// Colony name is editable (Enter to edit, Enter to confirm, Escape to cancel).
	/// Shuffle name button clicks and speaks the new name.
	/// Select duplicants enters dupe mode.
	/// Tab/Shift+Tab in dupe mode preserves widget position across slots.
	///
	/// Per Pitfall 4: CharacterContainer inherits KScreen but is NOT pushed to
	/// KScreenManager -- ContextDetector ignores it. Navigation is handled entirely
	/// within this handler.
	///
	/// Per locked decisions:
	/// - Traits: full info upfront (name, effect, description all spoken together)
	/// - Attributes: one per arrow press ("Athletics 3")
	/// - After reroll: speak new name and interests automatically
	/// </summary>
	public class MinionSelectHandler: BaseMenuHandler {
		private int _currentSlot;
		private UnityEngine.Component[] _containers;
		private System.Action _pendingAnnounce;
		private bool _pendingColonyNameAnnounce;
		private bool _inDupeMode;
		private readonly TextEditHelper _textEdit = new TextEditHelper();

		private static readonly System.Type MinionSelectScreenType =
			HarmonyLib.AccessTools.TypeByName("MinionSelectScreen");

		/// <summary>
		/// Whether the screen is MinionSelectScreen (has colony naming).
		/// Printing Pod (ImmigrantScreen) does not have BaseNaming.
		/// </summary>
		private bool IsMinionSelectScreen =>
			_screen != null && _screen.GetType() == MinionSelectScreenType;

		/// <summary>
		/// CharacterContainer uses coroutines (DelayedGeneration, SetAttributes)
		/// that take multiple frames to complete.
		/// </summary>
		protected override int MaxDiscoveryRetries => 10;

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.MINION_SELECT;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public MinionSelectHandler(KScreen screen) : base(screen) {
			_currentSlot = 0;
			_inDupeMode = false;
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_DUPE_SLOT));
		}

		public override void OnActivate() {
			_inDupeMode = false;
			_pendingColonyNameAnnounce = false;
			_pendingAnnounce = null;
			base.OnActivate();
		}

		// ========================================
		// TAB NAVIGATION (switch between dupe slots)
		// ========================================

		protected override void NavigateTabForward() {
			if (!_inDupeMode) return;
			if (_containers == null || _containers.Length == 0) return;
			int savedIndex = _currentIndex;
			_currentSlot = (_currentSlot + 1) % _containers.Length;
			if (_currentSlot == 0) PlayWrapSound();

			RediscoverAndSpeakSlot(savedIndex);
		}

		protected override void NavigateTabBackward() {
			if (!_inDupeMode) return;
			if (_containers == null || _containers.Length == 0) return;
			int savedIndex = _currentIndex;
			int prev = _currentSlot;
			_currentSlot = (_currentSlot - 1 + _containers.Length) % _containers.Length;
			if (_currentSlot == _containers.Length - 1 && prev == 0) PlayWrapSound();

			RediscoverAndSpeakSlot(savedIndex);
		}

		/// <summary>
		/// Rediscover widgets for the current slot, preserving position index.
		/// </summary>
		private void RediscoverAndSpeakSlot(int savedIndex) {
			DiscoverWidgets(_screen);
			_currentIndex = System.Math.Min(savedIndex, _widgets.Count > 0 ? _widgets.Count - 1 : 0);
			if (_widgets.Count > 0) {
				Speech.SpeechPipeline.SpeakInterrupt(
					$"{string.Format(STRINGS.ONIACCESS.INFO.SLOT, _currentSlot + 1)}, {GetWidgetSpeechText(_widgets[_currentIndex])}");
			}
		}

		// ========================================
		// WIDGET DISCOVERY
		// ========================================

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			if (_inDupeMode) {
				return DiscoverDupeModeWidgets(screen);
			}

			return DiscoverTopLevelWidgets(screen);
		}

		/// <summary>
		/// Discover top-level widgets: colony name, shuffle, select dupes, embark.
		/// </summary>
		private bool DiscoverTopLevelWidgets(KScreen screen) {
			// Colony name (MinionSelectScreen only, via BaseNaming component)
			if (IsMinionSelectScreen) {
				try {
					// BaseNaming is on the same GameObject as MinionSelectScreen
					var baseNamingObj = screen.gameObject.GetComponent(
						HarmonyLib.AccessTools.TypeByName("BaseNaming"));
					if (baseNamingObj != null) {
						var bnt = Traverse.Create(baseNamingObj);
						var inputField = bnt.Field("inputField").GetValue<KInputTextField>();
						if (inputField != null) {
							_widgets.Add(new WidgetInfo {
								Label = $"{STRINGS.ONIACCESS.PANELS.COLONY_NAME}, {inputField.text}",
								Component = inputField,
								Type = WidgetType.TextInput,
								GameObject = inputField.gameObject,
								Tag = "colony_name"
							});

							// Shuffle colony name button
							var shuffleBtn = bnt.Field("shuffleBaseNameButton").GetValue<KButton>();
							if (shuffleBtn != null && shuffleBtn.gameObject.activeInHierarchy) {
								_widgets.Add(new WidgetInfo {
									Label = GetButtonLabel(shuffleBtn, (string)STRINGS.ONIACCESS.PANELS.SHUFFLE_NAME),
									Component = shuffleBtn,
									Type = WidgetType.Button,
									GameObject = shuffleBtn.gameObject,
									Tag = "colony_shuffle"
								});
							}
						}
					}
				} catch (System.Exception ex) {
					Util.Log.Debug($"MinionSelectHandler.DiscoverTopLevelWidgets(BaseNaming): {ex.Message}");
				}
			}

			// "Select duplicants" virtual button (enters dupe mode)
			// Verify containers exist before offering this option
			_containers = screen.GetComponentsInChildren<CharacterContainer>(true);
			if (_containers != null && _containers.Length > 0) {
				_widgets.Add(new WidgetInfo {
					Label = STRINGS.ONIACCESS.PANELS.SELECT_DUPLICANTS,
					Component = null,
					Type = WidgetType.Button,
					GameObject = screen.gameObject,
					Tag = "enter_dupe_mode"
				});
			}

			// Embark / Proceed button (from CharacterSelectionController)
			try {
				var proceedButton = Traverse.Create(screen).Field("proceedButton")
					.GetValue<KButton>();
				if (proceedButton != null && proceedButton.gameObject.activeInHierarchy) {
					string label = GetButtonLabel(proceedButton, (string)STRINGS.UI.IMMIGRANTSCREEN.EMBARK);
					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = proceedButton,
						Type = WidgetType.Button,
						GameObject = proceedButton.gameObject
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverTopLevelWidgets(proceedButton): {ex.Message}");
			}

			// Back button (MinionSelectScreen only)
			if (IsMinionSelectScreen) {
				try {
					var backButton = Traverse.Create(screen).Field("backButton")
						.GetValue<KButton>();
					if (backButton != null && backButton.gameObject.activeInHierarchy
						&& backButton.isInteractable) {
						string label = GetButtonLabel(backButton, (string)STRINGS.UI.SANDBOXTOOLS.FILTERS.BACK);
						_widgets.Add(new WidgetInfo {
							Label = label,
							Component = backButton,
							Type = WidgetType.Button,
							GameObject = backButton.gameObject
						});
					}
				} catch (System.Exception ex) {
					Util.Log.Debug($"MinionSelectHandler.DiscoverTopLevelWidgets(backButton): {ex.Message}");
				}
			}

			if (_widgets.Count == 0) {
				Util.Log.Debug("MinionSelectHandler.DiscoverTopLevelWidgets: 0 widgets");
				return false;
			}

			Util.Log.Debug($"MinionSelectHandler.DiscoverTopLevelWidgets: {_widgets.Count} widgets");
			return true;
		}

		/// <summary>
		/// Discover dupe mode widgets for the current slot.
		/// </summary>
		private bool DiscoverDupeModeWidgets(KScreen screen) {
			_containers = screen.GetComponentsInChildren<CharacterContainer>(true);
			if (_containers == null || _containers.Length == 0) {
				Util.Log.Debug("MinionSelectHandler.DiscoverDupeModeWidgets: no containers");
				return false;
			}

			if (_currentSlot >= _containers.Length) _currentSlot = 0;
			var container = _containers[_currentSlot] as CharacterContainer;
			if (container == null || !container.gameObject.activeInHierarchy) {
				Util.Log.Debug("MinionSelectHandler.DiscoverDupeModeWidgets: container null or inactive");
				return false;
			}

			// Check if character data is ready (stats populated by coroutine)
			var ct = Traverse.Create(container);
			var stats = ct.Field("stats").GetValue<object>();
			if (stats == null) {
				Util.Log.Debug("MinionSelectHandler.DiscoverDupeModeWidgets: stats null (coroutine pending)");
				return false;
			}

			DiscoverSlotWidgets(container);

			if (_widgets.Count == 0) {
				Util.Log.Debug("MinionSelectHandler.DiscoverDupeModeWidgets: 0 widgets after discovery");
				return false;
			}

			Util.Log.Debug($"MinionSelectHandler.DiscoverDupeModeWidgets: {_widgets.Count} widgets in slot {_currentSlot}");
			return true;
		}

		/// <summary>
		/// Build the widget list for a single CharacterContainer slot.
		/// Order: name, interests, traits (one per), attributes (one per),
		/// interest filter dropdown, reroll button.
		/// </summary>
		private void DiscoverSlotWidgets(CharacterContainer container) {
			var traverse = Traverse.Create(container);

			// (a) Name
			DiscoverNameWidget(container, traverse);

			// (b) Interests: one per interest with attribute bonus
			DiscoverInterestsWidget(container, traverse);

			// (c) Traits: one per trait, with full info
			DiscoverTraitWidgets(container, traverse);

			// (d) Expectations: stress reaction & overjoyed response
			DiscoverExpectationWidgets(container, traverse);

			// (e) Attributes: one per attribute
			DiscoverAttributeWidgets(container, traverse);

			// (f) Description / bio
			DiscoverDescriptionWidget(container, traverse);

			// (g) Interest filter dropdown (archetypeDropDown)
			DiscoverFilterDropdown(container, traverse);

			// (h) Reroll button (reshuffleButton)
			DiscoverRerollButton(container, traverse);
		}

		/// <summary>
		/// Discover the duplicant name widget from the CharacterContainer.
		/// Tries characterNameTitle (EditableTitleBar) first, then falls back
		/// to searching for LocText children with a name-like pattern.
		/// </summary>
		private void DiscoverNameWidget(CharacterContainer container, Traverse traverse) {
			try {
				var titleBar = traverse.Field("characterNameTitle").GetValue<object>();
				if (titleBar == null) return;
				var titleBarTraverse = Traverse.Create(titleBar);

				// Name label
				var locText = titleBarTraverse.Field("titleText").GetValue<LocText>();
				if (locText != null && !string.IsNullOrEmpty(locText.text)) {
					_widgets.Add(new WidgetInfo {
						Label = locText.text,
						Component = null,
						Type = WidgetType.Label,
						GameObject = locText.gameObject
					});
				}

				// Rename button (editNameButton)
				var editBtn = titleBarTraverse.Field("editNameButton").GetValue<KButton>();
				if (editBtn != null && editBtn.gameObject.activeInHierarchy) {
					_widgets.Add(new WidgetInfo {
						Label = STRINGS.ONIACCESS.PANELS.RENAME,
						Component = editBtn,
						Type = WidgetType.Button,
						GameObject = editBtn.gameObject,
						Tag = "dupe_rename"
					});
				}

				// Shuffle name button (randomNameButton)
				var randomBtn = titleBarTraverse.Field("randomNameButton").GetValue<KButton>();
				if (randomBtn != null) {
					_widgets.Add(new WidgetInfo {
						Label = STRINGS.ONIACCESS.PANELS.SHUFFLE_NAME,
						Component = randomBtn,
						Type = WidgetType.Button,
						GameObject = randomBtn.gameObject,
						Tag = "dupe_shuffle_name"
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverNameWidget: {ex.Message}");
			}
		}

		/// <summary>
		/// Discover the interests/aptitudes label. Combines all interests into
		/// one label: "Interests: Research, Cooking".
		/// </summary>
		private void DiscoverInterestsWidget(CharacterContainer container, Traverse traverse) {
			try {
				var aptitudeEntries = traverse.Field("aptitudeEntries")
					.GetValue<List<UnityEngine.GameObject>>();
				if (aptitudeEntries == null) return;
				foreach (var entryGo in aptitudeEntries) {
					if (entryGo == null || !entryGo.activeInHierarchy) continue;
					var locTexts = entryGo.GetComponentsInChildren<LocText>(false);
					if (locTexts == null || locTexts.Length == 0) continue;

					// Combine active LocTexts: interest name + attribute bonus
					var parts = new List<string>();
					foreach (var lt in locTexts) {
						if (lt == null || string.IsNullOrEmpty(lt.text)
							|| !lt.gameObject.activeInHierarchy) continue;
						parts.Add(lt.text.Trim());
					}

					if (parts.Count > 0) {
						_widgets.Add(new WidgetInfo {
							Label = $"{STRINGS.ONIACCESS.INFO.INTEREST}: {string.Join(", ", parts)}",
							Component = null,
							Type = WidgetType.Label,
							GameObject = entryGo,
							Tag = "interest"
						});
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverInterestsWidget: {ex.Message}");
			}
		}

		/// <summary>
		/// Discover trait widgets. Each trait gets its own widget with full info:
		/// name + effect + description combined into one label.
		/// Per locked decision: "Traits: full info upfront."
		/// </summary>
		private void DiscoverTraitWidgets(CharacterContainer container, Traverse traverse) {
			try {
				var stats = traverse.Field("stats").GetValue<MinionStartingStats>();
				if (stats == null) return;
				var traits = stats.Traits;
				if (traits == null) return;

				// Skip index 0 (same as game's SetInfoText does)
				for (int i = 1; i < traits.Count; i++) {
					var trait = traits[i];
					string name = trait.GetName();
					if (string.IsNullOrEmpty(name)) continue;

					string tooltip = trait.GetTooltip();
					string label = string.IsNullOrEmpty(tooltip)
						? $"{STRINGS.ONIACCESS.INFO.TRAIT}: {name}"
						: $"{STRINGS.ONIACCESS.INFO.TRAIT}: {name}, {tooltip}";

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = null,
						Type = WidgetType.Label,
						GameObject = container.gameObject
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverTraitWidgets: {ex.Message}");
			}
		}

		private void DiscoverExpectationWidgets(CharacterContainer container, Traverse traverse) {
			try {
				var labels = traverse.Field("expectationLabels")
					.GetValue<List<LocText>>();
				if (labels == null) return;
				foreach (var lt in labels) {
					if (lt == null || string.IsNullOrEmpty(lt.text)
						|| !lt.gameObject.activeInHierarchy) continue;

					string label = lt.text.Trim();
					var tooltip = lt.GetComponent<ToolTip>();
					if (tooltip != null) {
						try {
							string ttText = tooltip.GetMultiString(0);
							if (!string.IsNullOrEmpty(ttText)) {
								label = $"{label}, {ttText}";
							}
						} catch (System.Exception ex) {
							Util.Log.Debug($"MinionSelectHandler.DiscoverExpectationWidgets(tooltip): {ex.Message}");
						}
					}

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = null,
						Type = WidgetType.Label,
						GameObject = lt.gameObject
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverExpectationWidgets: {ex.Message}");
			}
		}

		private void DiscoverDescriptionWidget(CharacterContainer container, Traverse traverse) {
			try {
				var descLocText = traverse.Field("description")
					.GetValue<LocText>();
				if (descLocText != null && !string.IsNullOrEmpty(descLocText.text)) {
					_widgets.Add(new WidgetInfo {
						Label = descLocText.text.Trim(),
						Component = null,
						Type = WidgetType.Label,
						GameObject = descLocText.gameObject
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverDescriptionWidget: {ex.Message}");
			}
		}

		private void DiscoverAttributeWidgets(CharacterContainer container, Traverse traverse) {
			try {
				var iconGroups = traverse.Field("iconGroups")
					.GetValue<List<UnityEngine.GameObject>>();
				if (iconGroups == null) return;
				foreach (var go in iconGroups) {
					if (go == null || !go.activeInHierarchy) continue;
					var locText = go.GetComponentInChildren<LocText>();
					if (locText == null || string.IsNullOrEmpty(locText.text)) continue;

					string label = locText.text.Trim();

					// Tooltip is on the iconGroup GameObject itself (SetSimpleTooltip)
					var tooltip = go.GetComponent<ToolTip>();
					if (tooltip != null) {
						try {
							string ttText = tooltip.GetMultiString(0);
							if (!string.IsNullOrEmpty(ttText)) {
								string flat = ttText.Replace("\n", ", ").Replace("\r", "");
								label = $"{label}, {flat}";
							}
						} catch (System.Exception ex) {
							Util.Log.Debug($"MinionSelectHandler.DiscoverAttributeWidgets(tooltip): {ex.Message}");
						}
					}

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = null,
						Type = WidgetType.Label,
						GameObject = go
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverAttributeWidgets: {ex.Message}");
			}
		}

		private void DiscoverFilterDropdown(CharacterContainer container, Traverse traverse) {
			try {
				var dropdown = traverse.Field("archetypeDropDown")
					.GetValue<DropDown>();
				if (dropdown != null && dropdown.gameObject.activeInHierarchy) {
					_widgets.Add(new WidgetInfo {
						Label = GetInterestFilterLabel(),
						Component = dropdown,
						Type = WidgetType.Dropdown,
						GameObject = dropdown.gameObject,
						Tag = "interest_filter"
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverFilterDropdown: {ex.Message}");
			}
		}

		private string GetInterestFilterLabel() {
			try {
				var container = _containers[_currentSlot] as CharacterContainer;
				var ct = Traverse.Create(container);
				var aptId = ct.Field("guaranteedAptitudeID").GetValue<string>();
				if (string.IsNullOrEmpty(aptId)) {
					return $"{STRINGS.ONIACCESS.INFO.INTEREST_FILTER}, {STRINGS.ONIACCESS.STATES.ANY}";
				}
				var skillGroup = Db.Get().SkillGroups.TryGet(aptId);
				return skillGroup != null
					? $"{STRINGS.ONIACCESS.INFO.INTEREST_FILTER}, {skillGroup.Name}"
					: $"{STRINGS.ONIACCESS.INFO.INTEREST_FILTER}, {aptId}";
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.GetInterestFilterLabel: {ex.Message}");
				return (string)STRINGS.ONIACCESS.INFO.INTEREST_FILTER;
			}
		}

		protected override void CycleDropdown(WidgetInfo widget, int direction) {
			if (!(widget.Tag is string tag) || tag != "interest_filter") return;
			try {
				var container = _containers[_currentSlot] as CharacterContainer;
				var ct = Traverse.Create(container);
				var dropdown = ct.Field("archetypeDropDown").GetValue<DropDown>();
				if (dropdown == null) return;

				var entries = dropdown.Entries;
				if (entries == null || entries.Count == 0) return;

				var currentId = ct.Field("guaranteedAptitudeID").GetValue<string>();

				// Find current index (-1 = "Any" / no filter)
				int currentIdx = -1;
				if (!string.IsNullOrEmpty(currentId)) {
					for (int i = 0; i < entries.Count; i++) {
						if (entries[i] is SkillGroup sg && sg.Id == currentId) {
							currentIdx = i;
							break;
						}
					}
				}

				// Cycle: -1 (Any) -> 0 -> 1 -> ... -> Count-1 -> -1 (Any)
				int newIdx = currentIdx + direction;
				if (newIdx < -1) newIdx = entries.Count - 1;
				if (newIdx >= entries.Count) newIdx = -1;

				// Invoke the callback through the dropdown's onEntrySelectedAction
				var onSelect = Traverse.Create(dropdown)
					.Field("onEntrySelectedAction")
					.GetValue<System.Action<IListableOption, object>>();
				if (onSelect != null) {
					var selected = newIdx >= 0 ? entries[newIdx] : null;
					onSelect(selected, dropdown.targetData);
				}

				// Reshuffle triggers a coroutine — delay one frame then announce
				_pendingAnnounce = AnnounceAfterFilterChange;
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.CycleDropdown: {ex.Message}");
			}
		}

		private void DiscoverRerollButton(CharacterContainer container, Traverse traverse) {
			try {
				var reshuffleButton = traverse.Field("reshuffleButton")
					.GetValue<KButton>();
				if (reshuffleButton != null && reshuffleButton.gameObject.activeInHierarchy
					&& reshuffleButton.isInteractable) {
					_widgets.Add(new WidgetInfo {
						Label = (string)STRINGS.UI.IMMIGRANTSCREEN.SHUFFLE,
						Component = reshuffleButton,
						Type = WidgetType.Button,
						GameObject = reshuffleButton.gameObject,
						Tag = "reroll"
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Debug($"MinionSelectHandler.DiscoverRerollButton: {ex.Message}");
			}
		}

		// ========================================
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// Override to read colony name live from the input field,
		/// and read the interest filter's current selection.
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			if (widget.Tag is string tag) {
				if (tag == "colony_name" && widget.Component is KInputTextField tf) {
					if (string.IsNullOrEmpty(tf.text)) {
						_pendingColonyNameAnnounce = true;
						return STRINGS.ONIACCESS.PANELS.COLONY_NAME;
					}
					return $"{STRINGS.ONIACCESS.PANELS.COLONY_NAME}, {tf.text}";
				}
				if (tag == "interest_filter") {
					return GetInterestFilterLabel();
				}
			}
			return base.GetWidgetSpeechText(widget);
		}

		/// <summary>
		/// Allow the shuffle name button to be navigated even when its
		/// GameObject is inactive — the game hides randomNameButton by default
		/// but we can still click it programmatically.
		/// </summary>
		protected override bool IsWidgetValid(WidgetInfo widget) {
			if (widget.Tag is string tag && tag == "dupe_shuffle_name")
				return widget.Component != null;
			return base.IsWidgetValid(widget);
		}

		/// <summary>
		/// Suppress auto-tooltip for widgets that already bake tooltip into
		/// their label, or where the auto-discovered tooltip is wrong
		/// (e.g., enter_dupe_mode picks up editNameButton's tooltip).
		/// </summary>
		protected override string GetTooltipText(WidgetInfo widget) {
			if (widget.Tag is string tag) {
				switch (tag) {
					// These already have tooltip content in their Label
					case "interest":
					case "dupe_rename":
					case "dupe_shuffle_name":
					// This picks up an unrelated child tooltip
					case "enter_dupe_mode":
						return null;
				}
			}
			// In dupe mode, traits/expectations/attributes/description are all
			// label-only widgets with no tag — suppress tooltip for plain labels
			if (_inDupeMode && widget.Type == WidgetType.Label) return null;
			return base.GetTooltipText(widget);
		}

		// ========================================
		// WIDGET ACTIVATION (Enter key)
		// ========================================

		protected override void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];

			// Enter dupe mode
			if (widget.Tag is string tag && tag == "enter_dupe_mode") {
				_inDupeMode = true;
				_currentSlot = 0;
				bool ready = DiscoverWidgets(_screen);
				_currentIndex = 0;
				if (ready && _widgets.Count > 0) {
					Speech.SpeechPipeline.SpeakInterrupt(
						$"{string.Format(STRINGS.ONIACCESS.INFO.SLOT, _currentSlot + 1)}, {GetWidgetSpeechText(_widgets[0])}");
				} else {
					_pendingRediscovery = true;
				}
				return;
			}

			// Colony name text editing
			if (widget.Tag is string nameTag && nameTag == "colony_name"
				&& widget.Component is KInputTextField textField) {
				if (!_textEdit.IsEditing) {
					_textEdit.Begin(textField);
				} else {
					_textEdit.Confirm();
				}
				return;
			}

			// Colony shuffle button: click, then read new name
			if (widget.Tag is string shuffleTag && shuffleTag == "colony_shuffle") {
				base.ActivateCurrentWidget();
				// Read the updated colony name from BaseNaming.inputField
				try {
					var baseNamingObj = _screen.gameObject.GetComponent(
						HarmonyLib.AccessTools.TypeByName("BaseNaming"));
					if (baseNamingObj != null) {
						var inputField = Traverse.Create(baseNamingObj)
							.Field("inputField").GetValue<KInputTextField>();
						if (inputField != null) {
							Speech.SpeechPipeline.SpeakInterrupt(
								$"{STRINGS.ONIACCESS.PANELS.COLONY_NAME}, {inputField.text}");
						}
					}
				} catch (System.Exception ex) {
					Util.Log.Debug($"MinionSelectHandler.ActivateCurrentWidget(colony_shuffle): {ex.Message}");
				}
				return;
			}

			// Dupe rename button: enter text edit mode on EditableTitleBar.inputField
			if (widget.Tag is string renameTag && renameTag == "dupe_rename") {
				try {
					int slot = _currentSlot;
					_textEdit.Begin(() => {
						var c = _containers[slot] as CharacterContainer;
						var tb = Traverse.Create(c)
							.Field("characterNameTitle").GetValue<object>();
						return Traverse.Create(tb)
							.Field("inputField").GetValue<KInputTextField>();
					});
				} catch (System.Exception ex) {
					Util.Log.Debug($"MinionSelectHandler.ActivateCurrentWidget(dupe_rename): {ex.Message}");
				}
				return;
			}

			// Dupe shuffle name button: click, then read new name
			if (widget.Tag is string dupeShuffleTag && dupeShuffleTag == "dupe_shuffle_name") {
				var kbutton = widget.Component as KButton;
				kbutton?.SignalClick(KKeyCode.Mouse0);
				try {
					var container = _containers[_currentSlot] as CharacterContainer;
					var titleBar = Traverse.Create(container)
						.Field("characterNameTitle").GetValue<object>();
					var locText = Traverse.Create(titleBar).Field("titleText")
						.GetValue<LocText>();
					if (locText != null) {
						Speech.SpeechPipeline.SpeakInterrupt(locText.text);
					}
				} catch (System.Exception ex) {
					Util.Log.Debug($"MinionSelectHandler.ActivateCurrentWidget(dupe_shuffle_name): {ex.Message}");
				}
				return;
			}

			// Reroll button in dupe mode
			if (widget.Tag is string rerollTag && rerollTag == "reroll") {
				var kbutton = widget.Component as KButton;
				kbutton?.SignalClick(KKeyCode.Mouse0);
				// Delay announcement by one frame for SetAttributes coroutine
				_pendingAnnounce = AnnounceAfterReroll;
				return;
			}

			base.ActivateCurrentWidget();
		}

		/// <summary>
		/// After reroll, wait one frame then rediscover and announce.
		/// </summary>
		private void AnnounceAfterReroll() {
			DiscoverWidgets(_screen);
			_currentIndex = FindWidgetByTag("reroll");
			AnnounceNameAndInterests();
		}

		/// <summary>
		/// Find a widget by tag, returning its index or clamped fallback.
		/// </summary>
		private int FindWidgetByTag(string targetTag) {
			for (int i = 0; i < _widgets.Count; i++) {
				if (_widgets[i].Tag is string t && t == targetTag)
					return i;
			}
			return _widgets.Count > 0 ? _widgets.Count - 1 : 0;
		}

		/// <summary>
		/// Interrupt-speak name (first widget) then queue interest-tagged widgets.
		/// Does not change _currentIndex.
		/// </summary>
		private void AnnounceNameAndInterests() {
			if (_widgets.Count > 0)
				Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
			QueueNameAndInterests(includeName: false);
		}

		/// <summary>
		/// Queue-speak name (first widget) and all interest-tagged widgets.
		/// Does not change _currentIndex. Does not interrupt.
		/// </summary>
		private void QueueNameAndInterests(bool includeName = true) {
			bool seenInterest = false;
			for (int i = 0; i < _widgets.Count; i++) {
				var w = _widgets[i];
				bool isInterest = w.Tag is string tag && tag == "interest";

				if (i == 0 && includeName || isInterest) {
					Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(w));
					if (isInterest) seenInterest = true;
				} else if (seenInterest) {
					break;
				}
			}
		}

		private void AnnounceAfterFilterChange() {
			DiscoverWidgets(_screen);
			// Find the filter widget by tag — index shifts when trait/interest count changes
			_currentIndex = FindWidgetByTag("interest_filter");
			Speech.SpeechPipeline.SpeakInterrupt(GetInterestFilterLabel());
			// Queue name + interests after the filter label (don't interrupt)
			QueueNameAndInterests();
		}

		// ========================================
		// KEY HANDLING
		// ========================================

		/// <summary>
		/// Intercept Escape for dupe mode exit and text edit cancel.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			// Text editing: Escape cancels
			if (_textEdit.IsEditing) {
				if (e.TryConsume(Action.Escape)) {
					_textEdit.Cancel();
					return true;
				}
				return false;
			}

			// Dupe mode: Escape exits back to top level
			if (_inDupeMode) {
				if (e.TryConsume(Action.Escape)) {
					_inDupeMode = false;
					_search.Clear();
					DiscoverWidgets(_screen);
					_currentIndex = 0;
					if (_widgets.Count > 0)
						Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
					return true;
				}
			}

			if (base.HandleKeyDown(e)) return true;
			return false;
		}

		// ========================================
		// TICK: TEXT EDIT, DEFERRED ANNOUNCE
		// ========================================

		public override void Tick() {
			// Text edit mode: only handle Return to confirm
			if (_textEdit.IsEditing) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					_textEdit.Confirm();
				}
				return;
			}

			// Colony name not yet populated by BaseNaming.OnSpawn — re-announce
			if (_pendingColonyNameAnnounce && _currentIndex == 0 && _widgets.Count > 0) {
				var w = _widgets[0];
				if (w.Tag is string t && t == "colony_name" && w.Component is KInputTextField tf
					&& !string.IsNullOrEmpty(tf.text)) {
					_pendingColonyNameAnnounce = false;
					Speech.SpeechPipeline.SpeakInterrupt(
						$"{STRINGS.ONIACCESS.PANELS.COLONY_NAME}, {tf.text}");
				}
				// Don't return — allow other tick logic to run
			}

			// Deferred one-frame announce (reroll, filter change)
			if (_pendingAnnounce != null) {
				var action = _pendingAnnounce;
				_pendingAnnounce = null;
				action();
				return;
			}

			base.Tick();
		}
	}
}
