using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers {
	/// <summary>
	/// Handler for MinionSelectScreen (initial colony start — full game start screen)
	/// and Printing Pod selection (recurring every 3 cycles).
	///
	/// Two-level navigation:
	/// TOP LEVEL (Up/Down): Colony name, Shuffle name, Select duplicants, Embark
	/// DUPE MODE (Up/Down within slot, Tab/Shift+Tab between slots):
	///   name, interests, traits, attributes, interest filter, reroll
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
	/// - After reroll: speak new name, interests, and traits automatically
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

			// (a) Name: get from characterNameTitle or EditableTitleBar
			DiscoverNameWidget(container, traverse);

			// (b) Interests: combined as one label
			DiscoverInterestsWidget(container, traverse);

			// (c) Traits: one per trait, with full info (name + effect + description)
			DiscoverTraitWidgets(container, traverse);

			// (d) Attributes: one per attribute
			DiscoverAttributeWidgets(container, traverse);

			// (e) Interest filter dropdown (archetypeDropDown)
			DiscoverFilterDropdown(container, traverse);

			// (f) Reroll button (reshuffleButton)
			DiscoverRerollButton(container, traverse);
		}

		/// <summary>
		/// Discover the duplicant name widget from the CharacterContainer.
		/// Tries characterNameTitle (EditableTitleBar) first, then falls back
		/// to searching for LocText children with a name-like pattern.
		/// </summary>
		private void DiscoverNameWidget(CharacterContainer container, Traverse traverse) {
			string name = null;
			UnityEngine.GameObject nameGo = null;

			// Try EditableTitleBar.text via characterNameTitle field
			try {
				var titleBar = traverse.Field("characterNameTitle").GetValue<object>();
				if (titleBar != null) {
					var titleBarTraverse = Traverse.Create(titleBar);
					// EditableTitleBar stores its text in a LocText child
					var locText = titleBarTraverse.Field("titleText")
						.GetValue<LocText>();
					if (locText != null && !string.IsNullOrEmpty(locText.text)) {
						name = locText.text;
						nameGo = locText.gameObject;
					}
				}
			} catch (System.Exception) {
				// Field may not exist or be a different type
			}

			// Fallback: look for any prominent LocText that might be the name
			if (string.IsNullOrEmpty(name)) {
				try {
					var nameTitle = traverse.Field("nameTitle")
						.GetValue<LocText>();
					if (nameTitle != null && !string.IsNullOrEmpty(nameTitle.text)) {
						name = nameTitle.text;
						nameGo = nameTitle.gameObject;
					}
				} catch (System.Exception) { }
			}

			if (!string.IsNullOrEmpty(name)) {
				_widgets.Add(new WidgetInfo {
					Label = name,
					Component = null,
					Type = WidgetType.Label,
					GameObject = nameGo ?? container.gameObject
				});
			}
		}

		/// <summary>
		/// Discover the interests/aptitudes label. Combines all interests into
		/// one label: "Interests: Research, Cooking".
		/// </summary>
		private void DiscoverInterestsWidget(CharacterContainer container, Traverse traverse) {
			try {
				var aptitudeLabel = traverse.Field("aptitudeLabel")
					.GetValue<LocText>();
				if (aptitudeLabel != null && !string.IsNullOrEmpty(aptitudeLabel.text)) {
					_widgets.Add(new WidgetInfo {
						Label = aptitudeLabel.text,
						Component = null,
						Type = WidgetType.Label,
						GameObject = aptitudeLabel.gameObject
					});
					return;
				}
			} catch (System.Exception) { }

			// Fallback: look for interest entries in the container hierarchy
			try {
				var aptitudeEntries = traverse.Field("aptitudeEntries")
					.GetValue<System.Collections.IList>();
				if (aptitudeEntries != null && aptitudeEntries.Count > 0) {
					var interestNames = new List<string>();
					foreach (var entry in aptitudeEntries) {
						var entryTraverse = Traverse.Create(entry);
						var locText = entryTraverse.Field("label")
							.GetValue<LocText>();
						if (locText != null && !string.IsNullOrEmpty(locText.text)) {
							interestNames.Add(locText.text);
						}
					}

					if (interestNames.Count > 0) {
						string combined = "Interests: " + string.Join(", ", interestNames);
						_widgets.Add(new WidgetInfo {
							Label = combined,
							Component = null,
							Type = WidgetType.Label,
							GameObject = container.gameObject
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
				var traitEntries = traverse.Field("traitEntries")
					.GetValue<System.Collections.IList>();
				if (traitEntries != null) {
					foreach (var entry in traitEntries) {
						AddTraitWidget(entry);
					}
					return;
				}
			} catch (System.Exception) { }

			// Fallback: look for a traits panel and walk its LocText children
			try {
				var traitPanel = traverse.Field("traitsPanel")
					.GetValue<UnityEngine.GameObject>();
				if (traitPanel == null) {
					traitPanel = traverse.Field("traitsList")
						.GetValue<UnityEngine.GameObject>();
				}
				if (traitPanel != null) {
					var locTexts = traitPanel.GetComponentsInChildren<LocText>(false);
					foreach (var lt in locTexts) {
						if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
						AddTraitFromLocText(lt);
					}
				}
			} catch (System.Exception) { }
		}

		private void AddTraitWidget(object entry) {
			if (entry == null) return;

			try {
				var entryTraverse = Traverse.Create(entry);

				LocText locText = null;

				if (entry is UnityEngine.GameObject entryGo) {
					locText = entryGo.GetComponentInChildren<LocText>();
				} else if (entry is UnityEngine.Component entryComp) {
					locText = entryComp.GetComponentInChildren<LocText>();
				} else {
					locText = entryTraverse.Field("label").GetValue<LocText>();
				}

				if (locText != null) {
					AddTraitFromLocText(locText);
				}
			} catch (System.Exception) { }
		}

		private void AddTraitFromLocText(LocText locText) {
			if (locText == null || string.IsNullOrEmpty(locText.text)) return;

			string traitText = locText.text.Trim();
			string tooltipText = null;

			var tooltip = locText.GetComponent<ToolTip>();
			if (tooltip != null) {
				try {
					tooltipText = tooltip.GetMultiString(0);
				} catch (System.Exception) { }
			}

			string label;
			if (!string.IsNullOrEmpty(tooltipText)) {
				string trimmedTooltip = TruncateToFirstSentence(tooltipText, 120);
				label = $"{traitText}, {trimmedTooltip}";
			} else {
				label = traitText;
			}

			_widgets.Add(new WidgetInfo {
				Label = label,
				Component = null,
				Type = WidgetType.Label,
				GameObject = locText.gameObject
			});
		}

		private static string TruncateToFirstSentence(string text, int maxLength) {
			if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;

			int periodIdx = text.IndexOf('.');
			if (periodIdx > 0 && periodIdx < maxLength) {
				return text.Substring(0, periodIdx + 1);
			}

			return text.Substring(0, maxLength);
		}

		private void DiscoverAttributeWidgets(CharacterContainer container, Traverse traverse) {
			try {
				var iconGroups = traverse.Field("iconGroups")
					.GetValue<System.Collections.IList>();
				if (iconGroups != null) {
					foreach (var group in iconGroups) {
						AddAttributeFromGroup(group);
					}
					return;
				}
			} catch (System.Exception) { }

			try {
				var attrLabels = traverse.Field("attributeLabels")
					.GetValue<LocText[]>();
				if (attrLabels != null) {
					foreach (var lt in attrLabels) {
						if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
						_widgets.Add(new WidgetInfo {
							Label = lt.text,
							Component = null,
							Type = WidgetType.Label,
							GameObject = lt.gameObject
						});
					}
					return;
				}
			} catch (System.Exception) { }

			try {
				var attrPanel = traverse.Field("attributesPanel")
					.GetValue<UnityEngine.GameObject>();
				if (attrPanel == null) {
					attrPanel = traverse.Field("attributesList")
						.GetValue<UnityEngine.GameObject>();
				}
				if (attrPanel != null) {
					var locTexts = attrPanel.GetComponentsInChildren<LocText>(false);
					foreach (var lt in locTexts) {
						if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
						_widgets.Add(new WidgetInfo {
							Label = lt.text,
							Component = null,
							Type = WidgetType.Label,
							GameObject = lt.gameObject
						});
					}
				}
			} catch (System.Exception) { }
		}

		private void AddAttributeFromGroup(object group) {
			if (group == null) return;
			try {
				LocText locText = null;
				UnityEngine.GameObject go = null;

				if (group is UnityEngine.GameObject groupGo) {
					locText = groupGo.GetComponentInChildren<LocText>();
					go = groupGo;
				} else if (group is UnityEngine.Component groupComp) {
					locText = groupComp.GetComponentInChildren<LocText>();
					go = groupComp.gameObject;
				} else {
					var groupTraverse = Traverse.Create(group);
					locText = groupTraverse.Field("label").GetValue<LocText>();
					if (locText != null) go = locText.gameObject;
				}

				if (locText != null && !string.IsNullOrEmpty(locText.text)) {
					_widgets.Add(new WidgetInfo {
						Label = locText.text,
						Component = null,
						Type = WidgetType.Label,
						GameObject = go ?? locText.gameObject
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

			bool first = true;
			for (int i = 0; i < _widgets.Count; i++) {
				var w = _widgets[i];
				if (w.Type != WidgetType.Label
					&& w.Type != WidgetType.Dropdown
					&& w.Type != WidgetType.Button) {
					break;
				}

				if (w.Type == WidgetType.Label && LooksLikeAttribute(w.Label)) {
					break;
				}

				if (first) {
					Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(w));
					first = false;
				} else {
					Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(w));
				}
			}

			_pendingRerollAnnounce = false;
		}

		private static bool LooksLikeAttribute(string label) {
			if (string.IsNullOrEmpty(label)) return false;
			if (label.Length >= 2 && (label[0] == '+' || label[0] == '-')
				&& char.IsDigit(label[1])) {
				return true;
			}
			return false;
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
