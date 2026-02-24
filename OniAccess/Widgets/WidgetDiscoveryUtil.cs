using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Widgets {
	/// <summary>
	/// Shared helpers for widget discovery across handlers.
	/// </summary>
	public static class WidgetDiscoveryUtil {
		/// <summary>
		/// Try to add a KButton from a named field on the screen as a widget.
		/// Reads the button's LocText for a label, falling back to fallbackLabel.
		/// Silently skips if the field doesn't exist or the button is inactive.
		/// </summary>
		public static void TryAddButtonField(KScreen screen, string fieldName, string fallbackLabel, List<WidgetInfo> widgets) {
			try {
				var button = Traverse.Create(screen).Field(fieldName)
					.GetValue<KButton>();
				if (button == null || !button.gameObject.activeInHierarchy) return;

				string label = fallbackLabel;
				var locText = button.GetComponentInChildren<LocText>();
				if (locText != null) {
					string parsed = locText.GetParsedText();
					if (!string.IsNullOrEmpty(parsed)) label = parsed;
					else if (!string.IsNullOrEmpty(locText.text)) label = locText.text;
				}

				var captured = button;
				widgets.Add(new WidgetInfo {
					Label = label,
					Component = captured,
					Type = WidgetType.Button,
					GameObject = captured.gameObject,
					SpeechFunc = () => WidgetOps.GetButtonLabel(captured, fallbackLabel)
				});
			} catch (System.Exception ex) {
				Util.Log.Error($"WidgetDiscoveryUtil.TryAddButtonField({fieldName}): {ex.Message}");
			}
		}
	}
}
