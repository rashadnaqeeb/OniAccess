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
				if (locText != null && !string.IsNullOrEmpty(locText.text))
					label = locText.text;

				widgets.Add(new WidgetInfo {
					Label = label,
					Component = button,
					Type = WidgetType.Button,
					GameObject = button.gameObject
				});
			} catch (System.Exception ex) {
				Util.Log.Error($"WidgetDiscoveryUtil.TryAddButtonField({fieldName}): {ex.Message}");
			}
		}
	}
}
