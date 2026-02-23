using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Widgets {
	/// <summary>
	/// Recursively walks a SideScreenContent's widget hierarchy and emits
	/// WidgetInfo items. Priority order per node: KSlider, KToggle,
	/// MultiToggle, KNumberInputField, KInputField, KButton, LocText.
	/// When an interactive component is found on a node, that node is
	/// consumed and its children are not walked further.
	/// Inactive GameObjects and mouse-only controls are skipped.
	/// All SpeechFuncs read live component state via GetParsedText().
	/// </summary>
	public static class SideScreenWalker {
		/// <summary>
		/// Walk the ContentContainer of a SideScreenContent (or its
		/// root transform if ContentContainer is null/inactive).
		/// Appends discovered widgets to <paramref name="items"/>.
		/// </summary>
		public static void Walk(SideScreenContent screen, List<WidgetInfo> items) {
			var root = screen.ContentContainer != null
				&& screen.ContentContainer.activeInHierarchy
				? screen.ContentContainer.transform
				: screen.transform;
			WalkTransform(root, items);
		}

		private static void WalkTransform(Transform parent, List<WidgetInfo> items) {
			for (int i = 0; i < parent.childCount; i++) {
				var child = parent.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				if (IsSkipped(child.gameObject.name)) continue;

				if (TryAddWidget(child, items))
					continue;
				WalkTransform(child, items);
			}
		}

		/// <summary>
		/// Try to emit a widget for the given transform. Returns true if a
		/// component was found (caller should not recurse into children).
		/// </summary>
		private static bool TryAddWidget(Transform t, List<WidgetInfo> items) {
			var go = t.gameObject;

			// KSlider (catches NonLinearSlider which extends KSlider)
			var slider = go.GetComponent<KSlider>();
			if (slider != null) {
				var captured = slider;
				var labelLt = FindChildLocText(t, null) ?? FindSiblingLocText(t);
				items.Add(new WidgetInfo {
					Label = ReadLocText(labelLt, t.name),
					Type = WidgetType.Slider,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						return $"{lbl}, {WidgetOps.FormatSliderValue(captured)}";
					}
				});
				return true;
			}

			// KToggle
			var ktoggle = go.GetComponent<KToggle>();
			if (ktoggle != null) {
				var captured = ktoggle;
				var labelLt = FindChildLocText(t, null) ?? FindSiblingLocText(t);
				items.Add(new WidgetInfo {
					Label = ReadLocText(labelLt, t.name),
					Type = WidgetType.Toggle,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						string state = captured.isOn
							? (string)STRINGS.ONIACCESS.STATES.ON
							: (string)STRINGS.ONIACCESS.STATES.OFF;
						return $"{lbl}, {state}";
					}
				});
				return true;
			}

			// MultiToggle — skip if a sibling KToggle or preceding MultiToggle
			// already represents this row's toggle (suppresses expand arrows).
			var multiToggle = go.GetComponent<MultiToggle>();
			if (multiToggle != null) {
				if (IsRedundantMultiToggle(t))
					return true;
				var captured = multiToggle;
				var labelLt = FindChildLocText(t, null) ?? FindSiblingLocText(t);
				items.Add(new WidgetInfo {
					Label = ReadLocText(labelLt, t.name),
					Type = WidgetType.Toggle,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						return $"{lbl}, {WidgetOps.GetMultiToggleState(captured)}";
					}
				});
				return true;
			}

			// KNumberInputField (extends KInputField — check first)
			var knum = go.GetComponent<KNumberInputField>();
			if (knum != null) {
				var captured = knum;
				var labelLt = FindSiblingLocText(t);
				var unitsLt = FindFollowingSiblingLocText(t);
				items.Add(new WidgetInfo {
					Label = ReadLocText(labelLt, t.name),
					Type = WidgetType.TextInput,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						string val = captured.field != null
							? captured.field.text : "";
						string units = unitsLt != null
							? unitsLt.GetParsedText() : null;
						if (!string.IsNullOrEmpty(units))
							return $"{lbl}, {val} {units}";
						return $"{lbl}, {val}";
					}
				});
				return true;
			}

			// KInputField (AlarmSideScreen text input — Category B)
			var kinput = go.GetComponent<KInputField>();
			if (kinput != null) {
				var captured = kinput;
				var labelLt = FindSiblingLocText(t);
				items.Add(new WidgetInfo {
					Label = ReadLocText(labelLt, t.name),
					Type = WidgetType.TextInput,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						string val = captured.field != null
							? captured.field.text : "";
						return $"{lbl}, {val}";
					}
				});
				return true;
			}

			// KButton (skip if inside an already-handled parent widget type)
			var kbutton = go.GetComponent<KButton>();
			if (kbutton != null) {
				var captured = kbutton;
				items.Add(new WidgetInfo {
					Label = GetButtonLabel(captured, t.name),
					Type = WidgetType.Button,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => GetButtonLabel(captured, captured.transform.name)
				});
				return true;
			}

			// LocText (standalone label — not inside any interactive widget).
			// Skip if a sibling interactive widget will claim this as its label.
			var locText = go.GetComponent<LocText>();
			if (locText != null) {
				if (HasSiblingInteractive(t))
					return false;
				var captured = locText;
				string text = captured.GetParsedText();
				if (!string.IsNullOrEmpty(text)) {
					items.Add(new WidgetInfo {
						Label = text,
						Type = WidgetType.Label,
						GameObject = go,
						SpeechFunc = () => captured.GetParsedText()
					});
					return true;
				}
			}

			return false;
		}

		// ========================================
		// SIBLING CHECKS
		// ========================================

		/// <summary>
		/// Returns true if this MultiToggle is redundant: either a sibling
		/// KToggle already owns the row's checkbox, or a preceding sibling
		/// MultiToggle already represents the row's toggle (the second
		/// MultiToggle in a row is typically an expand/collapse arrow).
		/// </summary>
		private static bool IsRedundantMultiToggle(Transform t) {
			if (t.parent == null) return false;
			var parent = t.parent;
			int myIndex = t.GetSiblingIndex();

			for (int i = 0; i < parent.childCount; i++) {
				var sibling = parent.GetChild(i);
				if (sibling == t) continue;
				if (!sibling.gameObject.activeSelf) continue;
				if (sibling.GetComponentInChildren<KToggle>() != null)
					return true;
			}

			for (int i = 0; i < myIndex; i++) {
				var sibling = parent.GetChild(i);
				if (!sibling.gameObject.activeSelf) continue;
				if (sibling.GetComponent<MultiToggle>() != null)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if any sibling (or sibling descendant) has an
		/// interactive component that would claim a LocText as its label
		/// via FindSiblingLocText. Used to suppress standalone LocText
		/// emission when the text is already a widget's label.
		/// </summary>
		private static bool HasSiblingInteractive(Transform t) {
			if (t.parent == null) return false;
			var parent = t.parent;
			for (int i = 0; i < parent.childCount; i++) {
				var sibling = parent.GetChild(i);
				if (sibling == t) continue;
				if (!sibling.gameObject.activeSelf) continue;
				var sgo = sibling.gameObject;
				if (sgo.GetComponent<KSlider>() != null) return true;
				if (sgo.GetComponent<KToggle>() != null) return true;
				if (sgo.GetComponent<MultiToggle>() != null) return true;
				if (sgo.GetComponent<KNumberInputField>() != null) return true;
				if (sgo.GetComponent<KInputField>() != null) return true;
			}
			return false;
		}

		// ========================================
		// LABEL RESOLUTION
		// ========================================

		/// <summary>
		/// Find the first active child LocText on the given transform.
		/// Skips the excluded component's GameObject if provided.
		/// Returns the LocText reference (for live reading), or null.
		/// </summary>
		private static LocText FindChildLocText(Transform t, Component exclude) {
			for (int i = 0; i < t.childCount; i++) {
				var child = t.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				if (exclude != null && child.gameObject == exclude.gameObject) continue;
				var lt = child.GetComponent<LocText>();
				if (lt != null) {
					string text = lt.GetParsedText();
					if (!string.IsNullOrEmpty(text))
						return lt;
				}
			}
			return null;
		}

		/// <summary>
		/// Find a sibling LocText for text input fields. Searches preceding
		/// siblings first (closest label), then following siblings.
		/// </summary>
		private static LocText FindSiblingLocText(Transform t) {
			if (t.parent == null) return null;
			var parent = t.parent;
			int myIndex = t.GetSiblingIndex();

			for (int i = myIndex - 1; i >= 0; i--) {
				var sibling = parent.GetChild(i);
				if (!sibling.gameObject.activeSelf) continue;
				var lt = sibling.GetComponent<LocText>();
				if (lt == null) lt = sibling.GetComponentInChildren<LocText>();
				if (lt != null) {
					string text = lt.GetParsedText();
					if (!string.IsNullOrEmpty(text))
						return lt;
				}
			}

			for (int i = myIndex + 1; i < parent.childCount; i++) {
				var sibling = parent.GetChild(i);
				if (!sibling.gameObject.activeSelf) continue;
				var lt = sibling.GetComponent<LocText>();
				if (lt == null) lt = sibling.GetComponentInChildren<LocText>();
				if (lt != null) {
					string text = lt.GetParsedText();
					if (!string.IsNullOrEmpty(text))
						return lt;
				}
			}

			return null;
		}

		/// <summary>
		/// Find the first LocText among following siblings only. Used to
		/// capture a units suffix (e.g., "kg") for number input fields.
		/// </summary>
		private static LocText FindFollowingSiblingLocText(Transform t) {
			if (t.parent == null) return null;
			var parent = t.parent;
			int myIndex = t.GetSiblingIndex();

			for (int i = myIndex + 1; i < parent.childCount; i++) {
				var sibling = parent.GetChild(i);
				if (!sibling.gameObject.activeSelf) continue;
				var lt = sibling.GetComponent<LocText>();
				if (lt == null) lt = sibling.GetComponentInChildren<LocText>();
				if (lt != null) {
					string text = lt.GetParsedText();
					if (!string.IsNullOrEmpty(text))
						return lt;
				}
			}

			return null;
		}

		/// <summary>
		/// Read a LocText reference, falling back to a name-based label.
		/// </summary>
		private static string ReadLocText(LocText lt, string fallback) {
			if (lt != null) {
				string text = lt.GetParsedText();
				if (!string.IsNullOrEmpty(text))
					return text;
			}
			return fallback;
		}

		/// <summary>
		/// Read a KButton's label from its child LocText using GetParsedText().
		/// </summary>
		private static string GetButtonLabel(KButton button, string fallback) {
			var lt = button.GetComponentInChildren<LocText>();
			if (lt != null) {
				string text = lt.GetParsedText();
				if (!string.IsNullOrEmpty(text))
					return text;
			}
			return fallback;
		}

		/// <summary>
		/// Filter out mouse-only UI elements that are irrelevant for
		/// keyboard navigation.
		/// </summary>
		private static bool IsSkipped(string name) {
			if (name.IndexOf("Scrollbar", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.IndexOf("ScrollRect", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.IndexOf("Drag", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.IndexOf("Resize", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			return false;
		}
	}
}
