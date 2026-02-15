using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;

namespace OniAccess.Input.Handlers {
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
	public class MinionSelectHandler : BaseMenuHandler {
		private int _currentSlot;
		private UnityEngine.Component[] _containers;
		private bool _pendingRerollAnnounce;
		private bool _inDupeMode;
		private bool _isEditingText;
		private string _cachedTextValue;
		private int _retryCount;
		private const int MaxRetries = 10;

		/// <summary>
		/// Whether the screen is MinionSelectScreen (has colony naming).
		/// Printing Pod (ImmigrantScreen) does not have BaseNaming.
		/// </summary>
		private bool IsMinionSelectScreen =>
			_screen != null && _screen.GetType().Name == "MinionSelectScreen";

		public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.MINION_SELECT;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public MinionSelectHandler(KScreen screen) : base(screen) {
			_currentSlot = 0;
			_inDupeMode = false;
			_isEditingText = false;
			_retryCount = 0;
			HelpEntries = BuildHelpEntries(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_DUPE_SLOT));
		}

		public override void OnActivate() {
			_retryCount = 0;
			_inDupeMode = false;
			_isEditingText = false;
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
					$"Slot {_currentSlot + 1}, {GetWidgetSpeechText(_widgets[_currentIndex])}");
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
							string currentName = inputField.text ?? "";
							_widgets.Add(new WidgetInfo {
								Label = $"{STRINGS.ONIACCESS.PANELS.COLONY_NAME}, {currentName}",
								Component = inputField,
								Type = WidgetType.TextInput,
								GameObject = inputField.gameObject,
								Tag = "colony_name"
							});

							// Shuffle colony name button
							var shuffleBtn = bnt.Field("shuffleBaseNameButton").GetValue<KButton>();
							if (shuffleBtn != null && shuffleBtn.gameObject.activeInHierarchy) {
								_widgets.Add(new WidgetInfo {
									Label = GetButtonLabel(shuffleBtn, "Shuffle name"),
									Component = shuffleBtn,
									Type = WidgetType.Button,
									GameObject = shuffleBtn.gameObject,
									Tag = "colony_shuffle"
								});
							}
						}
					}
				} catch (System.Exception) { }
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
					string label = GetButtonLabel(proceedButton, "Embark");
					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = proceedButton,
						Type = WidgetType.Button,
						GameObject = proceedButton.gameObject
					});
				}
			} catch (System.Exception) { }

			// Back button (MinionSelectScreen only)
			if (IsMinionSelectScreen) {
				try {
					var backButton = Traverse.Create(screen).Field("backButton")
						.GetValue<KButton>();
					if (backButton != null && backButton.gameObject.activeInHierarchy
						&& backButton.isInteractable) {
						string label = GetButtonLabel(backButton, "Back");
						_widgets.Add(new WidgetInfo {
							Label = label,
							Component = backButton,
							Type = WidgetType.Button,
							GameObject = backButton.gameObject
						});
					}
				} catch (System.Exception) { }
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
			} catch (System.Exception) { }
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
							Label = string.Join(", ", parts),
							Component = null,
							Type = WidgetType.Label,
							GameObject = entryGo,
							Tag = "interest"
						});
					}
				}
			} catch (System.Exception) { }
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
						? name
						: $"{name}, {tooltip}";

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = null,
						Type = WidgetType.Label,
						GameObject = container.gameObject
					});
				}
			} catch (System.Exception) { }
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
						} catch (System.Exception) { }
					}

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = null,
						Type = WidgetType.Label,
						GameObject = lt.gameObject
					});
				}
			} catch (System.Exception) { }
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
			} catch (System.Exception) { }
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

					// Tooltip is on the parent GameObject, not the LocText
					var tooltip = go.GetComponent<ToolTip>();
					if (tooltip != null) {
						try {
							string ttText = tooltip.GetMultiString(0);
							if (!string.IsNullOrEmpty(ttText)) {
								string flat = ttText.Replace("\n", ", ").Replace("\r", "");
								label = $"{label}, {flat}";
							}
						} catch (System.Exception) { }
					}

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = null,
						Type = WidgetType.Label,
						GameObject = go
					});
				}
			} catch (System.Exception) { }
		}

		private void DiscoverFilterDropdown(CharacterContainer container, Traverse traverse) {
			try {
				var dropdown = traverse.Field("archetypeDropDown")
					.GetValue<UnityEngine.Component>();
				if (dropdown != null && dropdown.gameObject.activeInHierarchy) {
					string label = "Interest filter";
					var locText = dropdown.GetComponentInChildren<LocText>();
					if (locText != null && !string.IsNullOrEmpty(locText.text)) {
						label = locText.text;
					}

					_widgets.Add(new WidgetInfo {
						Label = label,
						Component = dropdown,
						Type = WidgetType.Dropdown,
						GameObject = dropdown.gameObject
					});
				}
			} catch (System.Exception) { }
		}

		private void DiscoverRerollButton(CharacterContainer container, Traverse traverse) {
			try {
				var reshuffleButton = traverse.Field("reshuffleButton")
					.GetValue<KButton>();
				if (reshuffleButton != null && reshuffleButton.gameObject.activeInHierarchy
					&& reshuffleButton.isInteractable) {
					_widgets.Add(new WidgetInfo {
						Label = "Reroll",
						Component = reshuffleButton,
						Type = WidgetType.Button,
						GameObject = reshuffleButton.gameObject
					});
				}
			} catch (System.Exception) { }
		}

		// ========================================
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// Override to read colony name live from the input field.
		/// </summary>
		protected override string GetWidgetSpeechText(WidgetInfo widget) {
			if (widget.Tag is string tag && tag == "colony_name"
				&& widget.Component is KInputTextField tf) {
				return $"{STRINGS.ONIACCESS.PANELS.COLONY_NAME}, {tf.text}";
			}
			return base.GetWidgetSpeechText(widget);
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
				_retryCount = 0;
				bool ready = DiscoverWidgets(_screen);
				_currentIndex = 0;
				if (ready && _widgets.Count > 0) {
					Speech.SpeechPipeline.SpeakInterrupt(
						$"Slot {_currentSlot + 1}, {GetWidgetSpeechText(_widgets[0])}");
				} else {
					_pendingRediscovery = true;
				}
				return;
			}

			// Colony name text editing
			if (widget.Tag is string nameTag && nameTag == "colony_name"
				&& widget.Component is KInputTextField textField) {
				if (!_isEditingText) {
					_cachedTextValue = textField.text;
					_isEditingText = true;
					textField.ActivateInputField();
					Speech.SpeechPipeline.SpeakInterrupt($"Editing, {textField.text}");
				} else {
					_isEditingText = false;
					textField.DeactivateInputField();
					Speech.SpeechPipeline.SpeakInterrupt($"Confirmed, {textField.text}");
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
				} catch (System.Exception) { }
				return;
			}

			// Dupe rename button: enter text edit mode on EditableTitleBar.inputField
			if (widget.Tag is string renameTag && renameTag == "dupe_rename") {
				try {
					var container = _containers[_currentSlot] as CharacterContainer;
					var titleBar = Traverse.Create(container)
						.Field("characterNameTitle").GetValue<object>();
					var tbt = Traverse.Create(titleBar);
					var inputField = tbt.Field("inputField").GetValue<KInputTextField>();
					if (inputField != null) {
						_cachedTextValue = inputField.text;
						_isEditingText = true;
						// Activate editing state via the EditableTitleBar
						inputField.gameObject.SetActive(true);
						inputField.text = _cachedTextValue;
						inputField.Select();
						inputField.ActivateInputField();
						Speech.SpeechPipeline.SpeakInterrupt($"Editing, {_cachedTextValue}");
					}
				} catch (System.Exception) { }
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
				} catch (System.Exception) { }
				return;
			}

			// Reroll button in dupe mode
			if (_inDupeMode && widget.Type == WidgetType.Button && widget.Label == "Reroll") {
				var kbutton = widget.Component as KButton;
				kbutton?.SignalClick(KKeyCode.Mouse0);
				// Delay announcement by one frame for SetAttributes coroutine
				_pendingRerollAnnounce = true;
				return;
			}

			base.ActivateCurrentWidget();
		}

		/// <summary>
		/// After reroll, wait one frame then rediscover and announce.
		/// </summary>
		private void AnnounceAfterReroll() {
			DiscoverWidgets(_screen);
			_currentIndex = 0;

			// Announce only name (first widget) and interest-tagged widgets
			bool seenInterest = false;
			for (int i = 0; i < _widgets.Count; i++) {
				var w = _widgets[i];
				bool isInterest = w.Tag is string tag && tag == "interest";

				// First widget is always the name
				if (i == 0) {
					Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(w));
					continue;
				}

				if (isInterest) {
					seenInterest = true;
					Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(w));
				} else if (seenInterest) {
					// Past the interest block, stop
					break;
				}
			}

			_pendingRerollAnnounce = false;
		}

		// ========================================
		// KEY HANDLING
		// ========================================

		/// <summary>
		/// Intercept Escape for dupe mode exit and text edit cancel.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			// Text editing: Escape cancels
			if (_isEditingText) {
				if (e.TryConsume(Action.Escape)) {
					CancelTextEdit();
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
		// TICK: RETRY, TEXT EDIT, REROLL
		// ========================================

		public override void Tick() {
			// Text edit mode: only handle Return to confirm
			if (_isEditingText) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					ConfirmTextEdit();
				}
				return;
			}

			// Multi-frame retry: override base class single-retry behavior.
			// CharacterContainer uses coroutines (DelayedGeneration, SetAttributes)
			// that take multiple frames to complete.
			if (_pendingRediscovery) {
				_pendingRediscovery = false;
				bool ready = DiscoverWidgets(_screen);
				_currentIndex = 0;
				if (ready && _widgets.Count > 0) {
					_retryCount = 0;
					var w = _widgets[0];
					string text = GetWidgetSpeechText(w);
					string tip = GetTooltipText(w);
					if (tip != null) text = $"{text}, {tip}";
					Speech.SpeechPipeline.SpeakQueued(text);
				} else if (_retryCount < MaxRetries) {
					_retryCount++;
					_pendingRediscovery = true;
				} else {
					_retryCount = 0;
					Util.Log.Warn("MinionSelectHandler: gave up retrying DiscoverWidgets");
				}
				return;
			}

			// Pending reroll announce (one-frame delay for SetAttributes coroutine)
			if (_pendingRerollAnnounce) {
				AnnounceAfterReroll();
				return;
			}

			base.Tick();
		}

		// ========================================
		// TEXT EDITING
		// ========================================

		private KInputTextField GetActiveInputField() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return null;
			var widget = _widgets[_currentIndex];

			// Colony name: component IS the text field
			if (widget.Component is KInputTextField tf) return tf;

			// Dupe rename: get inputField from EditableTitleBar
			if (widget.Tag is string tag && tag == "dupe_rename" && _inDupeMode) {
				try {
					var container = _containers[_currentSlot] as CharacterContainer;
					var titleBar = Traverse.Create(container)
						.Field("characterNameTitle").GetValue<object>();
					return Traverse.Create(titleBar)
						.Field("inputField").GetValue<KInputTextField>();
				} catch (System.Exception) { }
			}
			return null;
		}

		private void CancelTextEdit() {
			_isEditingText = false;
			var textField = GetActiveInputField();
			if (textField != null) {
				textField.text = _cachedTextValue;
				textField.DeactivateInputField();
			}
			Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedTextValue}");
		}

		private void ConfirmTextEdit() {
			_isEditingText = false;
			var textField = GetActiveInputField();
			if (textField != null) {
				textField.DeactivateInputField();
				Speech.SpeechPipeline.SpeakInterrupt($"Confirmed, {textField.text}");
			} else {
				Speech.SpeechPipeline.SpeakInterrupt($"Cancelled, {_cachedTextValue}");
			}
		}
	}
}
