namespace OniAccess.Widgets {
	/// <summary>
	/// Stateless utility methods for widget speech, tooltip reading, validity
	/// checking, and programmatic interaction. Extracted from BaseWidgetHandler
	/// so that any handler (including NestedMenuHandler-based ones like
	/// DetailsScreenHandler) can reuse them without inheritance.
	/// </summary>
	public static class WidgetOps {
		// ========================================
		// SPEECH
		// ========================================

		/// <summary>
		/// Build speech text for a widget: "label, value" for sliders/toggles,
		/// just "label" for buttons/labels. Checks SpeechFunc first.
		/// </summary>
		public static string GetSpeechText(WidgetInfo widget) {
			if (widget.SpeechFunc != null) {
				string result = widget.SpeechFunc()?.Trim();
				if (!string.IsNullOrEmpty(result)) return result;
			}

			switch (widget.Type) {
				case WidgetType.Toggle: {
						var toggle = widget.Component as KToggle;
						if (toggle != null) {
							string state = SideScreenWalker.IsToggleActive(toggle) ? (string)STRINGS.ONIACCESS.STATES.ON : (string)STRINGS.ONIACCESS.STATES.OFF;
							return $"{widget.Label}, {state}";
						}
						var mt = widget.Component as MultiToggle;
						if (mt != null) {
							return $"{widget.Label}, {GetMultiToggleState(mt)}";
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
				default:
					return widget.Label;
			}
		}

		// ========================================
		// TOOLTIP
		// ========================================

		/// <summary>
		/// Look up tooltip text for a widget via its GameObject's ToolTip component.
		/// Returns null if suppressed, missing, or empty.
		/// </summary>
		public static string GetTooltipText(WidgetInfo widget) {
			if (widget.SuppressTooltip) return null;
			if (widget.GameObject == null) return null;

			var tooltip = widget.GameObject.GetComponent<ToolTip>();
			if (tooltip == null)
				tooltip = widget.GameObject.GetComponentInChildren<ToolTip>();
			if (tooltip == null)
				tooltip = widget.GameObject.GetComponentInParent<ToolTip>();
			if (tooltip == null) return null;

			return ReadAllTooltipText(tooltip);
		}

		/// <summary>
		/// Append tooltip text to speech text, skipping the tooltip if it
		/// duplicates an existing comma-separated segment of the speech.
		/// </summary>
		public static string AppendTooltip(string speech, string tooltip) {
			if (tooltip == null) return speech;
			foreach (string segment in speech.Split(new[] { ", " }, System.StringSplitOptions.None)) {
				if (segment == tooltip) return speech;
			}
			return $"{speech}, {tooltip}";
		}

		/// <summary>
		/// Rebuild a ToolTip's dynamic content and return all multiString
		/// entries joined with ", ".
		/// </summary>
		public static string ReadAllTooltipText(ToolTip tooltip) {
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
		// VALIDITY
		// ========================================

		/// <summary>
		/// Check whether a widget is still valid (not destroyed, active in hierarchy,
		/// and interactable where applicable).
		/// </summary>
		public static bool IsValid(WidgetInfo widget) {
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

		// ========================================
		// MULTI-TOGGLE STATE
		// ========================================

		/// <summary>
		/// Map a MultiToggle's CurrentState to a speech string.
		/// State 0 = off, last state = on, anything between = mixed.
		/// Handles both 2-state (on/off) and 3-state (off/mixed/on) toggles.
		/// </summary>
		public static string GetMultiToggleState(MultiToggle mt) {
			int last = mt.states != null ? mt.states.Length - 1 : 1;
			if (mt.CurrentState <= 0)
				return (string)STRINGS.ONIACCESS.STATES.OFF;
			if (mt.CurrentState >= last)
				return (string)STRINGS.ONIACCESS.STATES.ON;
			return (string)STRINGS.ONIACCESS.STATES.MIXED;
		}

		// ========================================
		// SLIDER FORMATTING
		// ========================================

		public static string FormatSliderValue(KSlider slider) {
			if (slider.wholeNumbers) {
				return ((int)slider.value).ToString();
			}

			if (slider.minValue >= 0f && slider.maxValue <= 100f) {
				return GameUtil.GetFormattedPercent(slider.value);
			}

			return slider.value.ToString("F1");
		}

		// ========================================
		// INTERACTION
		// ========================================

		public static void ClickButton(KButton button) {
			button.PlayPointerDownSound();
			button.SignalClick(KKeyCode.Mouse0);
		}

		public static void ClickMultiToggle(MultiToggle toggle) {
			var eventData = new UnityEngine.EventSystems.PointerEventData(
				UnityEngine.EventSystems.EventSystem.current) {
				button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
				clickCount = 1
			};
			toggle.OnPointerDown(eventData);
			toggle.OnPointerClick(eventData);
		}

		/// <summary>
		/// Extract a button's label from its child LocText, or return a fallback.
		/// </summary>
		public static string GetButtonLabel(KButton button, string fallback = null) {
			var locText = button.GetComponentInChildren<LocText>();
			if (locText != null && !string.IsNullOrEmpty(locText.text))
				return locText.text;
			return fallback;
		}
	}
}
