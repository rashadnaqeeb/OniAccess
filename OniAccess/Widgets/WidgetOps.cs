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
		/// Build speech text for a widget by delegating to its virtual GetSpeechText().
		/// </summary>
		public static string GetSpeechText(Widget widget) {
			return widget.GetSpeechText();
		}

		// ========================================
		// TOOLTIP
		// ========================================

		/// <summary>
		/// Look up tooltip text for a widget via its GameObject's ToolTip component.
		/// Returns null if suppressed, missing, or empty.
		/// </summary>
		public static string GetTooltipText(Widget widget) {
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
		public static bool IsValid(Widget widget) {
			if (widget == null) return false;
			return widget.IsValid();
		}

		// ========================================
		// MULTI-TOGGLE STATE
		// ========================================

		/// <summary>
		/// Map a MultiToggle's CurrentState to a speech string.
		/// 4-state toggles (ReceptacleSideScreen, mutation panel):
		///   0=Inactive, 1=Active(selected), 2=Disabled, 3=DisabledActive
		/// 2/3-state toggles: 0=off, last=on, middle=mixed.
		/// </summary>
		public static string GetMultiToggleState(MultiToggle mt) {
			int stateCount = mt.states != null ? mt.states.Length : 2;

			if (stateCount == 4) {
				bool selected = mt.CurrentState == 1 || mt.CurrentState == 3;
				bool disabled = mt.CurrentState == 2 || mt.CurrentState == 3;
				if (selected && disabled)
					return $"{(string)STRINGS.ONIACCESS.STATES.SELECTED}, {(string)STRINGS.ONIACCESS.STATES.DISABLED}";
				if (selected) return (string)STRINGS.ONIACCESS.STATES.SELECTED;
				if (disabled) return (string)STRINGS.ONIACCESS.STATES.DISABLED;
				return (string)STRINGS.ONIACCESS.STATES.OFF;
			}

			int last = stateCount - 1;
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
			if (locText != null) {
				string parsed = locText.GetParsedText();
				if (!string.IsNullOrEmpty(parsed)) return parsed;
				if (!string.IsNullOrEmpty(locText.text)) return locText.text;
			}
			return fallback;
		}
	}
}
