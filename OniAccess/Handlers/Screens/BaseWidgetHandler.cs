using System.Collections.Generic;

using OniAccess.Input;
using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Screen-bound widget handler extending BaseMenuHandler with widget discovery,
	/// interaction, speech, and lifecycle management.
	///
	/// Provides the bridge between BaseMenuHandler's abstract list navigation and
	/// concrete WidgetInfo-based screen handlers. Implements ItemCount, GetItemLabel,
	/// SpeakCurrentItem, IsItemValid by delegating to the _widgets list.
	///
	/// Concrete screen handlers extend this and implement only:
	/// - DiscoverWidgets (populate _widgets)
	/// - DisplayName (screen title for speech)
	/// - HelpEntries (composing from MenuHelpEntries + ListNavHelpEntries + screen-specific)
	///
	/// Per locked decisions:
	/// - Enter activates (ClickButton for KButton, KToggle.Click)
	/// - Left/Right adjust sliders and cycle dropdowns
	/// - Shift+Left/Right for large step adjustment
	/// - Tab/Shift+Tab for tabbed screens (virtual stubs)
	/// - Widget readout: label and value only, no type announcement
	/// - TextInput: Enter to begin editing, Enter to confirm, Escape to cancel
	///   (via TextEdit helper; subclasses using accessor-based Begin() override
	///   ActivateCurrentItem for that widget)
	/// </summary>
	public abstract class BaseWidgetHandler: BaseMenuHandler {
		protected readonly List<WidgetInfo> _widgets = new List<WidgetInfo>();
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

		protected BaseWidgetHandler(KScreen screen) : base(screen) { }

		// ========================================
		// BaseMenuHandler ABSTRACT IMPLEMENTATIONS
		// ========================================

		public override int ItemCount => _widgets.Count;

		public override string GetItemLabel(int index) {
			if (index < 0 || index >= _widgets.Count) return null;
			return _widgets[index].Label;
		}

		public override void SpeakCurrentItem() {
			SpeakCurrentWidget();
		}

		protected override bool IsItemValid(int index) {
			if (index < 0 || index >= _widgets.Count) return false;
			return IsWidgetValid(_widgets[index]);
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
		/// false if the screen isn't ready yet, BaseWidgetHandler will retry next frame.
		/// </returns>
		public abstract bool DiscoverWidgets(KScreen screen);

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			base.OnActivate();
			_retryCount = 0;
			bool ready = DiscoverWidgets(_screen);

			if (ready && _widgets.Count > 0) {
				_pendingRediscovery = false;
				_pendingSilentRefresh = true;
			} else {
				_pendingRediscovery = true;
			}
		}

		public override void OnDeactivate() {
			base.OnDeactivate();
			_widgets.Clear();
		}

		// ========================================
		// TICK
		// ========================================

		public override void Tick() {
			if (_textEdit != null && _textEdit.IsEditing) {
				if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
					_textEdit.Confirm();
					QueueCurrentWidget();
				}
				return;
			}

			// Deferred first-widget announcement: rediscover to pick up widgets
			// that weren't activeInHierarchy on frame 0, then queue the first widget.
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
			if (_pendingRediscovery) {
				_pendingRediscovery = false;
				bool ready = DiscoverWidgets(_screen);
				_currentIndex = 0;
				if (ready && _widgets.Count > 0) {
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

			// Detect invalidated widgets — do not reset _retryCount here so the
			// retry limit still applies if rediscovery keeps finding the same invalid widget.
			if (!_pendingRediscovery && _widgets.Count > 0) {
				int idx = System.Math.Min(_currentIndex, _widgets.Count - 1);
				if (!IsWidgetValid(_widgets[idx])) {
					_pendingRediscovery = true;
					return;
				}
			}

			base.Tick();
		}

		// ========================================
		// HANDLE KEY DOWN
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (_textEdit != null && _textEdit.IsEditing) {
				if (e.TryConsume(Action.Escape)) {
					_textEdit.Cancel();
					QueueCurrentWidget();
					return true;
				}
				return false;
			}

			return base.HandleKeyDown(e);
		}

		// ========================================
		// WIDGET INTERACTION (ActivateCurrentItem / AdjustCurrentItem overrides)
		// ========================================

		/// <summary>
		/// Activate the currently focused widget. Dispatches by WidgetType:
		/// - Button: ClickButton (triggers onClick + plays button sound)
		/// - Toggle: Click() then speak new state
		/// - TextInput: Begin/Confirm via TextEdit (Enter toggles editing)
		/// </summary>
		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
			var widget = _widgets[_currentIndex];
			if (!IsWidgetValid(widget)) return;

			switch (widget.Type) {
				case WidgetType.Button: {
						var kbutton = widget.Component as KButton;
						if (kbutton != null) {
							ClickButton(kbutton);
						} else {
							var mt = widget.Component as MultiToggle;
							if (mt != null)
								ClickMultiToggle(mt);
						}
						break;
					}
				case WidgetType.Toggle: {
						var toggle = widget.Component as KToggle;
						if (toggle != null) {
							toggle.Click();
							string state = toggle.isOn ? (string)STRINGS.ONIACCESS.STATES.ON : (string)STRINGS.ONIACCESS.STATES.OFF;
							Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
						} else {
							var mt = widget.Component as MultiToggle;
							if (mt != null) {
								ClickMultiToggle(mt);
								string state = mt.CurrentState == 1 ? (string)STRINGS.ONIACCESS.STATES.ON : (string)STRINGS.ONIACCESS.STATES.OFF;
								Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
							}
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
		protected override void AdjustCurrentItem(int direction, bool isLargeStep) {
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

						float oldValue = slider.value;
						slider.value = UnityEngine.Mathf.Clamp(
							slider.value + step * direction,
							slider.minValue, slider.maxValue);

						if (slider.value <= slider.minValue && direction < 0)
							PlaySliderSound("Slider_Boundary_Low");
						else if (slider.value >= slider.maxValue && direction > 0)
							PlaySliderSound("Slider_Boundary_High");
						else if (slider.value != oldValue)
							PlaySliderSound("Slider_Move");

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
		/// </summary>
		protected virtual void CycleDropdown(WidgetInfo widget, int direction) { }

		// ========================================
		// WIDGET VALIDITY
		// ========================================

		/// <summary>
		/// Check whether a widget is still valid (not destroyed, active in hierarchy,
		/// and interactable where applicable).
		/// </summary>
		protected virtual bool IsWidgetValid(WidgetInfo widget) {
			if (widget == null) return false;
			if (widget.GameObject != null && !widget.GameObject.activeInHierarchy) return false;

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
		// WIDGET SPEECH
		// ========================================

		/// <summary>
		/// Build speech text for a widget: "label, value" for sliders/toggles/dropdowns,
		/// just "label" for buttons/labels.
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

		/// <summary>
		/// Queue the currently focused widget via SpeakQueued so it follows
		/// a preceding SpeakInterrupt (e.g., after text-edit confirm/cancel).
		/// </summary>
		protected void QueueCurrentWidget() {
			if (_currentIndex >= 0 && _currentIndex < _widgets.Count) {
				var w = _widgets[_currentIndex];
				if (!IsWidgetValid(w)) return;
				string text = GetWidgetSpeechText(w);
				string tip = GetTooltipText(w);
				if (tip != null) text = $"{text}, {tip}";
				Speech.SpeechPipeline.SpeakQueued(text);
			}
		}

		// ========================================
		// TOOLTIP TEXT
		// ========================================

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
		/// entries joined with ", ".
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

		protected static void ClickButton(KButton button) {
			button.PlayPointerDownSound();
			button.SignalClick(KKeyCode.Mouse0);
		}

		protected static void ClickMultiToggle(MultiToggle toggle) {
			var eventData = new UnityEngine.EventSystems.PointerEventData(
				UnityEngine.EventSystems.EventSystem.current) {
				button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
				clickCount = 1
			};
			toggle.OnPointerDown(eventData);
			toggle.OnPointerClick(eventData);
		}

		private void PlaySliderSound(string soundName) {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound(soundName));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlaySliderSound({soundName}) failed: {ex.Message}");
			}
		}

		protected virtual string FormatSliderValue(KSlider slider) {
			if (slider.wholeNumbers) {
				return ((int)slider.value).ToString();
			}

			if (slider.minValue >= 0f && slider.maxValue <= 100f) {
				return GameUtil.GetFormattedPercent(slider.value);
			}

			return slider.value.ToString("F1");
		}
	}
}
