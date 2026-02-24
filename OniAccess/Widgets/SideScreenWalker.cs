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
		public class RadioMember {
			public string Label;
			public KToggle Toggle;
		}

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

			// Pick up widgets outside ContentContainer (e.g., AutomatableSideScreen)
			if (root != screen.transform) {
				var screenT = screen.transform;
				for (int i = 0; i < screenT.childCount; i++) {
					var child = screenT.GetChild(i);
					if (child == root.transform) continue;
					if (!child.gameObject.activeSelf) continue;
					if (IsSkipped(child.gameObject.name)) continue;
					if (IsChrome(child)) continue;
					if (TryAddWidget(child, items))
						continue;
					WalkTransform(child, items);
				}
			}

			CollapseRadioToggles(items, screen.GetTitle(), screen.transform);
			LogOrphanLocTexts(screen, items);
		}

		private static void WalkTransform(Transform parent, List<WidgetInfo> items) {
			for (int i = 0; i < parent.childCount; i++) {
				var child = parent.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				if (IsSkipped(child.gameObject.name)) {
					Util.Log.Debug($"WalkerSkip '{child.gameObject.name}' parent='{parent.name}'");
					continue;
				}

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
				var labelLt = FindChildLocText(t, null)
					?? FindSiblingLocText(t) ?? FindSiblingLocText(t.parent);
				string label = ReadLocText(labelLt, t.name);
				if (string.IsNullOrWhiteSpace(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KSlider) parent={t.parent?.name}");
					return true;
				}
				items.Add(new WidgetInfo {
					Label = label,
					Type = WidgetType.Slider,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						return $"{lbl}, {WidgetOps.FormatSliderValue(captured)}, {(string)STRINGS.ONIACCESS.STATES.SLIDER}";
					}
				});
				return true;
			}

			// KToggle
			var ktoggle = go.GetComponent<KToggle>();
			if (ktoggle != null) {
				var captured = ktoggle;
				var labelLt = FindChildLocText(t, null)
					?? FindSiblingLocText(t) ?? FindSiblingLocText(t.parent);
				string label = ReadLocText(labelLt, t.name);
				if (string.IsNullOrWhiteSpace(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KToggle) parent={t.parent?.name}");
					return true;
				}
				items.Add(new WidgetInfo {
					Label = label,
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
				bool redundant = IsRedundantMultiToggle(t);
				Util.Log.Debug($"WalkerTrace MultiToggle '{t.name}' parent='{t.parent?.name}' redundant={redundant}");
				if (redundant)
					return true;
				var captured = multiToggle;
				var childLt = FindChildLocText(t, null);
				var sibLt = FindSiblingLocText(t);
				var parentSibLt = FindSiblingLocText(t.parent);
				var labelLt = childLt ?? sibLt ?? parentSibLt;
				string label = ReadLocText(labelLt, t.name);
				Util.Log.Debug($"  label search: child={childLt?.GetParsedText() ?? "null"} sib={sibLt?.GetParsedText() ?? "null"} parentSib={parentSibLt?.GetParsedText() ?? "null"} => '{label}'");
				if (string.IsNullOrWhiteSpace(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (MultiToggle) parent={t.parent?.name}");
					return true;
				}
				items.Add(new WidgetInfo {
					Label = label,
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
				var labelLt = FindSiblingLocText(t) ?? FindSiblingLocText(t.parent);
				string label = ReadLocText(labelLt, t.name);
				if (string.IsNullOrWhiteSpace(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KNumberInputField) parent={t.parent?.name}");
					return true;
				}
				var unitsLt = FindFollowingSiblingLocText(t)
					?? FindFollowingSiblingLocText(t.parent);
				items.Add(new WidgetInfo {
					Label = label,
					Type = WidgetType.TextInput,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						string val = captured.field != null
							? captured.field.text : "";
						string units = unitsLt != null
							? unitsLt.GetParsedText() : null;
						string ifl = (string)STRINGS.ONIACCESS.STATES.INPUT_FIELD;
						if (!string.IsNullOrEmpty(units))
							return $"{lbl}, {val} {units}, {ifl}";
						return $"{lbl}, {val}, {ifl}";
					}
				});
				return true;
			}

			// KInputField (AlarmSideScreen text input — Category B)
			var kinput = go.GetComponent<KInputField>();
			if (kinput != null) {
				var captured = kinput;
				var labelLt = FindSiblingLocText(t) ?? FindSiblingLocText(t.parent);
				string label = ReadLocText(labelLt, t.name);
				if (string.IsNullOrWhiteSpace(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KInputField) parent={t.parent?.name}");
					return true;
				}
				items.Add(new WidgetInfo {
					Label = label,
					Type = WidgetType.TextInput,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						string val = captured.field != null
							? captured.field.text : "";
						return $"{lbl}, {val}, {(string)STRINGS.ONIACCESS.STATES.INPUT_FIELD}";
					}
				});
				return true;
			}

			// KButton (skip if inside an already-handled parent widget type)
			var kbutton = go.GetComponent<KButton>();
			if (kbutton != null) {
				var captured = kbutton;
				string label = GetButtonLabel(captured, t.name);
				if (string.IsNullOrWhiteSpace(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KButton) parent={t.parent?.name}");
					return true;
				}
				items.Add(new WidgetInfo {
					Label = label,
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
				if (sibling.GetComponentInChildren<KSlider>() != null) return true;
				if (sibling.GetComponentInChildren<KToggle>() != null) return true;
				if (sibling.GetComponentInChildren<MultiToggle>() != null) return true;
				if (sibling.GetComponentInChildren<KNumberInputField>() != null) return true;
				if (sibling.GetComponentInChildren<KInputField>() != null) return true;
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
				if (sibling.GetComponentInChildren<KSlider>() != null) break;
				if (sibling.GetComponentInChildren<KToggle>() != null) break;
				if (sibling.GetComponentInChildren<MultiToggle>() != null) break;
				if (sibling.GetComponentInChildren<KNumberInputField>() != null) break;
				if (sibling.GetComponentInChildren<KInputField>() != null) break;
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
			if (name.IndexOf("Drag", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.IndexOf("Resize", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.StartsWith("increment", System.StringComparison.OrdinalIgnoreCase)) return true;
			if (name.StartsWith("decrement", System.StringComparison.OrdinalIgnoreCase)) return true;
			return false;
		}

		/// <summary>
		/// Filter out screen-level chrome when walking outside ContentContainer.
		/// Skips title bars and close buttons that shouldn't be navigable widgets.
		/// </summary>
		private static bool IsChrome(Transform t) {
			string name = t.gameObject.name;
			if (name.IndexOf("Title", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.IndexOf("Header", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (name.IndexOf("CloseButton", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
			return false;
		}

		// ========================================
		// RADIO GROUP COLLAPSE
		// ========================================

		/// <summary>
		/// Detect consecutive KToggle widgets sharing the same parent where
		/// exactly one is isOn (radio-style mutual exclusion). Replace with a
		/// single Dropdown widget that cycles between members.
		/// </summary>
		private static void CollapseRadioToggles(
				List<WidgetInfo> items, string screenTitle, Transform screenRoot) {
			// Group consecutive KToggle items by parent transform
			var groups = new List<(Transform parent, int start, int count)>();
			int i = 0;
			while (i < items.Count) {
				var w = items[i];
				if (w.Type != WidgetType.Toggle || !(w.Component is KToggle))
				{ i++; continue; }

				var parent = w.GameObject.transform.parent;
				int start = i;
				int count = 1;
				while (i + count < items.Count
					&& items[i + count].Type == WidgetType.Toggle
					&& items[i + count].Component is KToggle
					&& items[i + count].GameObject.transform.parent == parent)
					count++;

				if (count >= 2)
					groups.Add((parent, start, count));
				i += count;
			}

			// Collect GameObjects already represented in items (for orphan detection)
			var emittedObjects = new HashSet<GameObject>();
			foreach (var item in items) {
				if (item.GameObject != null)
					emittedObjects.Add(item.GameObject);
			}

			// Process groups in reverse to preserve indices
			for (int g = groups.Count - 1; g >= 0; g--) {
				var (parent, start, count) = groups[g];

				// Verify exactly one isOn (confirms mutual exclusivity)
				int onCount = 0;
				for (int j = start; j < start + count; j++) {
					if (((KToggle)items[j].Component).isOn)
						onCount++;
				}
				if (onCount != 1) continue;

				// Build member list
				var members = new List<RadioMember>();
				for (int j = start; j < start + count; j++) {
					members.Add(new RadioMember {
						Label = items[j].Label,
						Toggle = (KToggle)items[j].Component
					});
				}

				// Search the full screen tree for an orphan description LocText
				var descriptionLt = FindOrphanDescription(screenRoot, emittedObjects);

				string groupLabel = screenTitle ?? items[start].Label;
				var radioMembers = members;
				items[start] = new WidgetInfo {
					Label = groupLabel,
					Component = members[0].Toggle,
					Type = WidgetType.Dropdown,
					GameObject = parent.gameObject,
					Tag = radioMembers,
					SpeechFunc = () => {
						string selected = null;
						for (int k = 0; k < radioMembers.Count; k++) {
							if (radioMembers[k].Toggle != null && radioMembers[k].Toggle.isOn) {
								selected = radioMembers[k].Label;
								break;
							}
						}
						string speech = selected != null
							? $"{groupLabel}, {selected}" : groupLabel;
						if (descriptionLt != null) {
							string desc = descriptionLt.GetParsedText();
							if (!string.IsNullOrEmpty(desc))
								speech += ", " + desc;
						}
						return speech;
					}
				};

				// Remove the collapsed items
				items.RemoveRange(start + 1, count - 1);
			}
		}

		/// <summary>
		/// Search the screen's full transform tree for a LocText that wasn't
		/// emitted as a widget item — a candidate description label. Skips
		/// LocTexts that are children of interactive widgets (those are labels,
		/// not descriptions).
		/// </summary>
		private static LocText FindOrphanDescription(
				Transform root, HashSet<GameObject> emittedObjects) {
			var allLocTexts = root.GetComponentsInChildren<LocText>(false);
			foreach (var lt in allLocTexts) {
				if (emittedObjects.Contains(lt.gameObject)) continue;

				string text = lt.GetParsedText();
				if (string.IsNullOrEmpty(text)) continue;

				// Skip LocTexts inside interactive widgets (they're labels)
				if (lt.GetComponentInParent<KToggle>() != null) continue;
				if (lt.GetComponentInParent<KButton>() != null) continue;
				if (lt.GetComponentInParent<KSlider>() != null) continue;
				if (lt.GetComponentInParent<MultiToggle>() != null) continue;

				// Skip the screen title (GetTitle() reads from a LocText too)
				var ssc = root.GetComponent<SideScreenContent>();
				if (ssc != null) {
					string title = ssc.GetTitle();
					if (text == title) continue;
				}

				return lt;
			}
			return null;
		}

		/// <summary>
		/// Debug: log all LocTexts on the screen that weren't emitted as
		/// widget items. Temporary — remove after audit is complete.
		/// </summary>
		private static void LogOrphanLocTexts(SideScreenContent screen, List<WidgetInfo> items) {
			var emitted = new HashSet<GameObject>();
			foreach (var item in items) {
				if (item.GameObject != null)
					emitted.Add(item.GameObject);
			}

			string screenName = screen.GetType().Name;
			string title = screen.GetTitle();
			var allLocTexts = screen.transform.GetComponentsInChildren<LocText>(false);
			bool any = false;
			foreach (var lt in allLocTexts) {
				if (emitted.Contains(lt.gameObject)) continue;
				string text = lt.GetParsedText();
				if (string.IsNullOrEmpty(text)) continue;
				if (text == title) continue;

				string parentWidget = null;
				if (lt.GetComponentInParent<KToggle>() != null) parentWidget = "KToggle";
				else if (lt.GetComponentInParent<KButton>() != null) parentWidget = "KButton";
				else if (lt.GetComponentInParent<KSlider>() != null) parentWidget = "KSlider";
				else if (lt.GetComponentInParent<MultiToggle>() != null) parentWidget = "MultiToggle";

				string path = lt.transform.name;
				var p = lt.transform.parent;
				for (int i = 0; i < 3 && p != null && p != screen.transform; i++) {
					path = p.name + "/" + path;
					p = p.parent;
				}

				string truncated = text.Length > 80 ? text.Substring(0, 80) + "..." : text;
				string inside = parentWidget != null ? $" [inside {parentWidget}]" : " [ORPHAN]";
				Util.Log.Debug($"OrphanAudit [{screenName}] {path}{inside}: {truncated}");
				any = true;
			}
			if (!any)
				Util.Log.Debug($"OrphanAudit [{screenName}]: no orphan LocTexts");
		}
	}
}
