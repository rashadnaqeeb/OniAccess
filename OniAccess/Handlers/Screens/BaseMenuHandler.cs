using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// 1D list navigation layer extending ScreenHandler.
	/// Owns the widget list, cursor index, and all menu-specific behavior:
	/// - Widget discovery and lifecycle
	/// - Arrow navigation with wrap-around
	/// - Home/End, Enter activation, Left/Right adjustment
	/// - Tab stubs for tabbed screens
	/// - A-Z type-ahead search
	/// - Widget validity checking
	///
	/// All key detection happens in Tick() via UnityEngine.Input.GetKeyDown().
	/// HandleKeyDown is inherited from ScreenHandler (returns false -- Escape
	/// passes through to the game, which closes the screen, which pops the
	/// handler via Harmony patch).
	///
	/// Concrete list-based handlers extend this and implement only:
	/// - DiscoverWidgets (populate _widgets)
	/// - DisplayName (screen title for speech)
	/// - HelpEntries (composing from MenuHelpEntries
	///   + ListNavHelpEntries + screen-specific)
	///
	/// Per locked decisions:
	/// - Arrow keys navigate Up/Down between items with wrap-around
	/// - Home/End jump to first/last
	/// - Enter activates (KButton.SignalClick, KToggle.Click)
	/// - Left/Right adjust sliders and cycle dropdowns
	/// - Shift+Left/Right for large step adjustment
	/// - Tab/Shift+Tab for tabbed screens (virtual stubs)
	/// - Widget readout: label and value only, no type announcement
	/// - TextInput: Enter to begin editing, Enter to confirm, Escape to cancel
	///   (via TextEdit helper; subclasses using accessor-based Begin() override
	///   ActivateCurrentWidget for that widget)
	/// </summary>
	public abstract class BaseMenuHandler: BaseScreenHandler, ISearchable {
		protected readonly List<WidgetInfo> _widgets = new List<WidgetInfo>();
		protected int _currentIndex;
		protected readonly TypeAheadSearch _search = new TypeAheadSearch();
		private TextEditHelper _textEdit;
		protected TextEditHelper TextEdit => _textEdit ??= new TextEditHelper();
		protected bool IsTextEditing => _textEdit != null && _textEdit.IsEditing;

		/// <summary>
		/// When true, Tick() will retry DiscoverWidgets.
		/// Set when OnActivate finds zero widgets — this happens when our Harmony
		/// postfix fires inside base.OnSpawn() before the screen subclass finishes
		/// setting up its UI in its own OnSpawn override.
		/// Retries up to MaxDiscoveryRetries times (default 1).
		/// </summary>
		protected bool _pendingRediscovery;
		private bool _pendingSilentRefresh;
		private int _retryCount;

		/// <summary>
		/// Maximum number of frames to retry DiscoverWidgets when it returns false.
		/// Override in subclasses that need more time (e.g., coroutine-driven screens).
		/// </summary>
		protected virtual int MaxDiscoveryRetries => 1;

		protected BaseMenuHandler(KScreen screen) : base(screen) { }

		/// <summary>
		/// Menus are modal: block all input from reaching handlers below.
		/// </summary>
		public override bool CapturesAllInput => true;

		// ========================================
		// COMPOSABLE HELP ENTRY LISTS (menu-specific)
		// ========================================

		/// <summary>
		/// Help entries for menu-specific features (search).
		/// </summary>
		protected static readonly List<HelpEntry> MenuHelpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
		};

		/// <summary>
		/// Help entries for 1D list navigation.
		/// </summary>
		protected static readonly List<HelpEntry> ListNavHelpEntries = new List<HelpEntry> {
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			new HelpEntry("Left/Right", STRINGS.ONIACCESS.HELP.ADJUST_VALUE),
			new HelpEntry("Shift+Left/Right", STRINGS.ONIACCESS.HELP.ADJUST_VALUE_LARGE),
		};

		/// <summary>
		/// Build a help entry list combining menu + list-nav entries with optional extras.
		/// </summary>
		protected IReadOnlyList<HelpEntry> BuildHelpEntries(params HelpEntry[] extra) {
			var list = new List<HelpEntry>();
			list.AddRange(MenuHelpEntries);
			list.AddRange(ListNavHelpEntries);
			list.AddRange(extra);
			return list;
		}

		// ========================================
		// ABSTRACT: WIDGET DISCOVERY
		// ========================================

		/// <summary>
		/// Populate _widgets from the screen's UI hierarchy.
		/// Each subclass implements to enumerate that screen's interactive elements.
		/// </summary>
		/// <returns>
		/// true if discovery is complete and widgets are ready to speak;
		/// false if the screen isn't ready yet, BaseMenuHandler will retry next frame.
		/// </returns>
		public abstract bool DiscoverWidgets(KScreen screen);

		// ========================================
		// LIFECYCLE
		// ========================================

		/// <summary>
		/// Called when this handler becomes active on the stack.
		/// Speaks screen name, discovers widgets, queues first widget.
		/// </summary>
		public override void OnActivate() {
			base.OnActivate();
			_retryCount = 0;
			bool ready = DiscoverWidgets(_screen);
			_currentIndex = 0;
			_search.Clear();

			if (ready && _widgets.Count > 0) {
				_pendingRediscovery = false;
				_pendingSilentRefresh = true;
			} else {
				// Screen not ready yet (subclass signaled false, or zero widgets
				// because Harmony postfix fired before subclass OnSpawn). Retry
				// up to MaxDiscoveryRetries times on subsequent Ticks.
				_pendingRediscovery = true;
			}
		}

		/// <summary>
		/// Called when this handler is popped off the stack.
		/// </summary>
		public override void OnDeactivate() {
			base.OnDeactivate();
			_currentIndex = 0;
			_search.Clear();
		}

		// ========================================
		// TICK: ALL KEY DETECTION
		// ========================================

		/// <summary>
		/// Per-frame key detection for menu navigation, type-ahead search,
		/// tooltip reading, and widget interaction.
		/// </summary>
		public override void Tick() {
			if (_textEdit != null && _textEdit.IsEditing) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return))
					_textEdit.Confirm();
				return;
			}

			// Deferred first-widget announcement: rediscover to pick up widgets
			// that weren't activeInHierarchy on frame 0, then queue the first widget.
			// OnActivate defers this so the announcement reflects the complete list.
			if (_pendingSilentRefresh) {
				_pendingSilentRefresh = false;
				int oldCount = _widgets.Count;
				DiscoverWidgets(_screen);
				_currentIndex = System.Math.Min(_currentIndex, System.Math.Max(0, _widgets.Count - 1));
				if (_widgets.Count != oldCount)
					Util.Log.Debug($"{GetType().Name}: deferred refresh changed widget count {oldCount} → {_widgets.Count}");
				if (_widgets.Count > 0) {
					var w = _widgets[_currentIndex];
					string text = GetWidgetSpeechText(w);
					string tip = GetTooltipText(w);
					if (tip != null) text = $"{text}, {tip}";
					Speech.SpeechPipeline.SpeakQueued(text);
				}
			}

			// Deferred rediscovery: screen UI wasn't ready during OnActivate
			// (Harmony postfix fired inside base.OnSpawn before subclass setup).
			// Retries up to MaxDiscoveryRetries frames.
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
				} else if (_retryCount < MaxDiscoveryRetries) {
					_retryCount++;
					_pendingRediscovery = true;
				} else {
					_retryCount = 0;
					Util.Log.Warn($"{GetType().Name}: gave up retrying DiscoverWidgets after {MaxDiscoveryRetries} attempts");
				}
			}

			// Detect invalidated widgets: RefreshButtons (or similar) destroyed
			// cached references after DiscoverWidgets already captured them.
			// Re-trigger discovery so the next Tick picks up fresh references.
			if (!_pendingRediscovery && _widgets.Count > 0) {
				int idx = System.Math.Min(_currentIndex, _widgets.Count - 1);
				if (!IsWidgetValid(_widgets[idx])) {
					_pendingRediscovery = true;
					_retryCount = 0;
					return;
				}
			}

			bool ctrlHeld = InputUtil.CtrlHeld();
			bool altHeld = InputUtil.AltHeld();

			// Type-ahead search: route keys through _search.HandleKey first.
			// When active, captures Up/Down/Home/End/Backspace for result navigation.
			// When inactive, captures A-Z (no modifiers) to start a new search.
			// Escape is handled in HandleKeyDown via TryConsume.
			if (TryRouteToSearch(ctrlHeld, altHeld))
				return;

			// Navigation
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				NavigateNext();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				NavigatePrev();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Home)) {
				NavigateFirst();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.End)) {
				NavigateLast();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				ActivateCurrentWidget();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)) {
				if (InputUtil.ShiftHeld())
					NavigateTabBackward();
				else
					NavigateTabForward();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
				AdjustCurrentWidget(-1, InputUtil.ShiftHeld());
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
				AdjustCurrentWidget(1, InputUtil.ShiftHeld());
				return;
			}

		}

		private static readonly UnityEngine.KeyCode[] _searchNavKeys = {
			UnityEngine.KeyCode.UpArrow, UnityEngine.KeyCode.DownArrow,
			UnityEngine.KeyCode.Home, UnityEngine.KeyCode.End,
			UnityEngine.KeyCode.Backspace,
		};

		/// <summary>
		/// Route keys through _search.HandleKey before standard navigation.
		/// Returns true if the search consumed the key.
		/// </summary>
		private bool TryRouteToSearch(bool ctrlHeld, bool altHeld) {
			// A-Z (no modifiers) — start or continue search
			if (!ctrlHeld && !altHeld) {
				for (var k = UnityEngine.KeyCode.A; k <= UnityEngine.KeyCode.Z; k++) {
					if (UnityEngine.Input.GetKeyDown(k))
						return _search.HandleKey(k, ctrlHeld, altHeld, this);
				}
			}

			// Navigation keys captured by search when active
			for (int i = 0; i < _searchNavKeys.Length; i++) {
				if (UnityEngine.Input.GetKeyDown(_searchNavKeys[i]))
					return _search.HandleKey(_searchNavKeys[i], ctrlHeld, altHeld, this);
			}

			return false;
		}

		/// <summary>
		/// Intercept Escape via KButtonEvent when search is active.
		/// Clears search instead of letting the game close the screen.
		/// Subclasses that override must call base.HandleKeyDown first.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (_textEdit != null && _textEdit.IsEditing) {
				if (e.TryConsume(Action.Escape)) {
					_textEdit.Cancel();
					return true;
				}
				return false;
			}

			if (_search.IsSearchActive && e.TryConsume(Action.Escape)) {
				_search.Clear();
				Speech.SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SEARCH.CLEARED);
				return true;
			}

			return false;
		}

		// ========================================
		// WIDGET VALIDITY
		// ========================================

		/// <summary>
		/// Check whether a widget is still valid (not destroyed, active in hierarchy,
		/// and interactable where applicable). Guards against stale references when
		/// game UI changes after DiscoverWidgets.
		/// </summary>
		protected virtual bool IsWidgetValid(WidgetInfo widget) {
			if (widget == null) return false;
			// Some widgets (e.g., cluster entries) have null GameObject — valid by existence
			if (widget.GameObject != null && !widget.GameObject.activeInHierarchy) return false;

			// Check interactability for known component types.
			// When the component isn't the expected type for the WidgetType
			// (e.g., MultiToggle used as Button), fall through to the alive check
			// rather than returning false.
			switch (widget.Type) {
				case WidgetType.Label:
					return true;
				case WidgetType.Button: {
						var btn = widget.Component as KButton;
						if (btn != null) return btn.isInteractable;
						if (widget.Component is MultiToggle) return true;
						break;
					}
				case WidgetType.Toggle: {
						var toggle = widget.Component as KToggle;
						if (toggle != null) return toggle.IsInteractable();
						if (widget.Component is MultiToggle) return true;
						break;
					}
				case WidgetType.Slider: {
						var slider = widget.Component as KSlider;
						if (slider != null) return slider.interactable;
						break;
					}
			}

			// Fallback: component is alive, or widget has no component (label-only)
			return widget.Component != null || widget.GameObject != null;
		}

		/// <summary>
		/// Extract a button's label from its child LocText, or return a fallback.
		/// </summary>
		protected string GetButtonLabel(KButton button, string fallback = null) {
			var locText = button.GetComponentInChildren<LocText>();
			if (locText != null && !string.IsNullOrEmpty(locText.text))
				return locText.text;
			return fallback;
		}

		// ========================================
		// NAVIGATION METHODS
		// ========================================

		/// <summary>
		/// Move to next widget with wrap-around. Skips invalid widgets.
		/// Plays wrap sound when wrapping to first.
		/// </summary>
		protected void NavigateNext() {
			if (_widgets.Count == 0) return;
			int start = _currentIndex;
			for (int i = 0; i < _widgets.Count; i++) {
				int candidate = (start + 1 + i) % _widgets.Count;
				if (IsWidgetValid(_widgets[candidate])) {
					bool wrapped = candidate <= _currentIndex;
					_currentIndex = candidate;
					if (wrapped) PlayWrapSound();
					SpeakCurrentWidget();
					return;
				}
			}
		}

		/// <summary>
		/// Move to previous widget with wrap-around. Skips invalid widgets.
		/// Plays wrap sound when wrapping to last.
		/// </summary>
		protected void NavigatePrev() {
			if (_widgets.Count == 0) return;
			int start = _currentIndex;
			for (int i = 0; i < _widgets.Count; i++) {
				int candidate = (start - 1 - i + _widgets.Count) % _widgets.Count;
				if (IsWidgetValid(_widgets[candidate])) {
					bool wrapped = candidate >= _currentIndex;
					_currentIndex = candidate;
					if (wrapped) PlayWrapSound();
					SpeakCurrentWidget();
					return;
				}
			}
		}

		/// <summary>
		/// Jump to first widget.
		/// </summary>
		protected void NavigateFirst() {
			if (_widgets.Count == 0) return;
			_currentIndex = 0;
			SpeakCurrentWidget();
		}

		/// <summary>
		/// Jump to last widget.
		/// </summary>
		protected void NavigateLast() {
			if (_widgets.Count == 0) return;
			_currentIndex = _widgets.Count - 1;
			SpeakCurrentWidget();
		}

		/// <summary>
		/// Navigate to next tab section. No-op default for non-tabbed screens.
		/// Subclasses override for tabbed screens (e.g., colony setup panels).
		/// </summary>
		protected virtual void NavigateTabForward() { }

		/// <summary>
		/// Navigate to previous tab section. No-op default.
		/// </summary>
		protected virtual void NavigateTabBackward() { }

		// ========================================
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// Build speech text for a widget: "label, value" for sliders/toggles/dropdowns,
		/// just "label" for buttons/labels. No type announcement per locked decision.
		/// </summary>
		protected virtual string GetWidgetSpeechText(WidgetInfo widget) {
			if (widget.SpeechFunc != null) {
				string result = widget.SpeechFunc();
				if (result != null) return result;
			}

			switch (widget.Type) {
				case WidgetType.Toggle: {
						var toggle = widget.Component as KToggle;
						if (toggle != null) {
							string state = toggle.isOn ? (string)STRINGS.ONIACCESS.STATES.ON : (string)STRINGS.ONIACCESS.STATES.OFF;
							return $"{widget.Label}, {state}";
						}
						var mt = widget.Component as MultiToggle;
						if (mt != null) {
							string state = mt.CurrentState == 1 ? (string)STRINGS.ONIACCESS.STATES.ON : (string)STRINGS.ONIACCESS.STATES.OFF;
							return $"{widget.Label}, {state}";
						}
						return widget.Label;
					}
				case WidgetType.Slider: {
						var slider = widget.Component as KSlider;
						if (slider != null) {
							return $"{widget.Label}, {FormatSliderValue(slider)}";
						}
						return widget.Label;
					}
				case WidgetType.Dropdown:
					// Dropdown value reading is screen-specific.
					// Subclasses override GetWidgetSpeechText for dropdowns
					// or provide the value in the Label itself.
					return widget.Label;
				default:
					return widget.Label;
			}
		}

		/// <summary>
		/// Speak the currently focused widget via SpeakInterrupt.
		/// Appends tooltip text if available.
		/// </summary>
		protected void SpeakCurrentWidget() {
			if (_currentIndex >= 0 && _currentIndex < _widgets.Count) {
				var w = _widgets[_currentIndex];
				if (!IsWidgetValid(w)) return;
				string text = GetWidgetSpeechText(w);
				string tip = GetTooltipText(w);
				if (tip != null) text = $"{text}, {tip}";
				Speech.SpeechPipeline.SpeakInterrupt(text);
			}
		}

		// ========================================
		// WIDGET INTERACTION
		// ========================================

		/// <summary>
		/// Activate the currently focused widget. Dispatches by WidgetType:
		/// - Button: SignalClick (triggers onClick + plays button sound)
		/// - Toggle: Click() then speak new state
		/// - TextInput: Begin/Confirm via TextEdit (Enter toggles editing)
		/// </summary>
		protected virtual void ActivateCurrentWidget() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];
			if (!IsWidgetValid(widget)) return;

			switch (widget.Type) {
				case WidgetType.Button: {
						var kbutton = widget.Component as KButton;
						kbutton?.SignalClick(KKeyCode.Mouse0);
						break;
					}
				case WidgetType.Toggle: {
						var toggle = widget.Component as KToggle;
						if (toggle != null) {
							toggle.Click();
							string state = toggle.isOn ? (string)STRINGS.ONIACCESS.STATES.ON : (string)STRINGS.ONIACCESS.STATES.OFF;
							Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
						}
						break;
					}
				case WidgetType.TextInput: {
						var textField = widget.Component as KInputTextField;
						if (textField != null) {
							if (!TextEdit.IsEditing)
								TextEdit.Begin(textField);
							else
								TextEdit.Confirm();
						}
						break;
					}
			}
		}

		/// <summary>
		/// Adjust the currently focused widget's value. Dispatches by WidgetType:
		/// - Slider: step by wholeNumbers-aware increment, speak new value
		/// - Dropdown: delegate to CycleDropdown virtual method
		/// </summary>
		protected virtual void AdjustCurrentWidget(int direction, bool isLargeStep) {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];
			if (!IsWidgetValid(widget)) return;

			switch (widget.Type) {
				case WidgetType.Slider: {
						var slider = widget.Component as KSlider;
						if (slider == null) return;

						float step;
						if (slider.wholeNumbers) {
							step = isLargeStep ? 10f : 1f;
						} else {
							float range = slider.maxValue - slider.minValue;
							step = isLargeStep ? range * 0.1f : range * 0.01f;
						}

						slider.value = UnityEngine.Mathf.Clamp(
							slider.value + step * direction,
							slider.minValue, slider.maxValue);

						// KSlider.onValueChanged fires automatically from setting .value
						Speech.SpeechPipeline.SpeakInterrupt(
							$"{widget.Label}, {FormatSliderValue(slider)}");
						break;
					}
				case WidgetType.Dropdown:
					CycleDropdown(widget, direction);
					break;
			}
		}

		/// <summary>
		/// Cycle a dropdown widget's value. No-op default.
		/// Subclasses override for screen-specific dropdown cycling logic.
		/// </summary>
		protected virtual void CycleDropdown(WidgetInfo widget, int direction) { }

		// ========================================
		// TOOLTIP TEXT
		// ========================================

		/// <summary>
		/// Extract tooltip text from a widget's GameObject.
		/// Returns null if no tooltip is present or its text is empty.
		/// Subclasses override for widgets with non-standard tooltip locations
		/// (e.g., radio groups where tooltip lives on the active member).
		/// </summary>
		protected virtual string GetTooltipText(WidgetInfo widget) {
			if (widget.GameObject == null) return null;

			var tooltip = widget.GameObject.GetComponent<ToolTip>();
			if (tooltip == null)
				tooltip = widget.GameObject.GetComponentInChildren<ToolTip>();
			if (tooltip == null) return null;

			return ReadAllTooltipText(tooltip);
		}

		/// <summary>
		/// Rebuild a ToolTip's dynamic content and return all multiString
		/// entries joined with ", ". Shared by GetTooltipText and inline
		/// tooltip reads so every site gets fresh, complete text.
		/// </summary>
		protected static string ReadAllTooltipText(ToolTip tooltip) {
			tooltip.RebuildDynamicTooltip();

			if (tooltip.multiStringCount == 0) return null;

			if (tooltip.multiStringCount == 1) {
				string single = tooltip.GetMultiString(0);
				return string.IsNullOrEmpty(single) ? null : single;
			}

			var sb = new System.Text.StringBuilder();
			for (int i = 0; i < tooltip.multiStringCount; i++) {
				string entry = tooltip.GetMultiString(i);
				if (string.IsNullOrEmpty(entry)) continue;
				if (sb.Length > 0) sb.Append(", ");
				sb.Append(entry);
			}
			return sb.Length == 0 ? null : sb.ToString();
		}

		// ========================================
		// UTILITY METHODS
		// ========================================

		/// <summary>
		/// Play the wrap-around earcon sound when navigation wraps.
		/// </summary>
		protected void PlayWrapSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click_Close"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayWrapSound failed: {ex.Message}");
			}
		}

		/// <summary>
		/// Format a slider value for speech. Uses integer format for wholeNumbers sliders,
		/// percent format for 0-100 range, and one-decimal format otherwise.
		/// </summary>
		protected virtual string FormatSliderValue(KSlider slider) {
			if (slider.wholeNumbers) {
				return ((int)slider.value).ToString();
			}

			if (slider.minValue >= 0f && slider.maxValue <= 100f) {
				return GameUtil.GetFormattedPercent(slider.value);
			}

			return slider.value.ToString("F1");
		}

		// ========================================
		// ISearchable IMPLEMENTATION
		// ========================================

		public int SearchItemCount => _widgets.Count;

		public int SearchCurrentIndex => _currentIndex;

		public string GetSearchLabel(int index) {
			if (index < 0 || index >= _widgets.Count) return null;
			return _widgets[index].Label;
		}

		public void SearchMoveTo(int index) {
			if (index < 0 || index >= _widgets.Count) return;
			_currentIndex = index;
			SpeakCurrentWidget();
		}
	}
}
