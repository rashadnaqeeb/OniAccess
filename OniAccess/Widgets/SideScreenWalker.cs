using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace OniAccess.Widgets {
	/// <summary>
	/// Recursively walks a SideScreenContent's widget hierarchy and emits
	/// Widget items. Priority order per node: KSlider, KToggle,
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
			public MultiToggle MultiToggleRef;
			public object Tag;
		}

		/// <summary>
		/// Walk the ContentContainer of a SideScreenContent (or its
		/// root transform if ContentContainer is null/inactive).
		/// Appends discovered widgets to <paramref name="items"/>.
		/// </summary>
		public static void Walk(SideScreenContent screen, List<Widget> items) {
			var claimedLabels = new HashSet<LocText>();

			var pixelPack = screen as PixelPackSideScreen;
			if (pixelPack != null) {
				WalkPixelPackScreen(pixelPack, items, claimedLabels);
				return;
			}

			var root = screen.ContentContainer != null
				&& screen.ContentContainer.activeInHierarchy
				? screen.ContentContainer.transform
				: screen.transform;
			WalkTransform(root, items, claimedLabels);

			// Pick up widgets outside ContentContainer (e.g., AutomatableSideScreen)
			if (root != screen.transform) {
				var screenT = screen.transform;
				for (int i = 0; i < screenT.childCount; i++) {
					var child = screenT.GetChild(i);
					if (child == root.transform) continue;
					if (!child.gameObject.activeSelf) continue;
					if (IsSkipped(child.gameObject.name)) continue;
					if (IsChrome(child)) continue;
					if (TryAddWidget(child, items, claimedLabels))
						continue;
					WalkTransform(child, items, claimedLabels);
				}
			}

			// Remove LocTexts that were claimed as labels by interactive widgets
			items.RemoveAll(item => {
				if (!(item is LabelWidget)) return false;
				var lt = item.GameObject?.GetComponent<LocText>();
				return lt != null && claimedLabels.Contains(lt);
			});

			var alarm = screen as AlarmSideScreen;
			if (alarm != null)
				CollapseAlarmTypeButtons(alarm, items, claimedLabels);

			CollapseRadioToggles(items, screen.GetTitle(), screen.transform, claimedLabels);
		}

		private static void WalkPixelPackScreen(
				PixelPackSideScreen pixelPack, List<Widget> items,
				HashSet<LocText> claimedLabels) {
			// Palette group (drillable)
			var swatchContainer = pixelPack.colorSwatchContainer.transform;
			var paletteChildren = new List<Widget>();
			for (int i = 0; i < swatchContainer.childCount; i++) {
				var child = swatchContainer.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var swatchGO = child.gameObject;
				var capturedGO = swatchGO;
				var img = swatchGO.GetComponent<Image>();
				if (img == null) continue;
				string label = ColorNameUtil.GetColorName(img.color) ?? capturedGO.name;
				paletteChildren.Add(new ButtonWidget {
					Label = label,
					Component = swatchGO.GetComponent<KButton>(),
					GameObject = capturedGO,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string name = ColorNameUtil.GetColorName(capturedGO.GetComponent<Image>().color)
							?? capturedGO.name;
						var href = capturedGO.GetComponent<HierarchyReferences>();
						var selectedRef = href.GetReference("selected");
						bool isSelected = selectedRef != null && selectedRef.gameObject.activeSelf;
						var usedImage = href.GetReference("used").GetComponentInChildren<Image>();
						bool inUse = usedImage != null && usedImage.gameObject.activeSelf;
						string speech = name;
						if (isSelected)
							speech += $", {(string)STRINGS.ONIACCESS.STATES.SELECTED}";
						if (inUse)
							speech += $", {(string)STRINGS.ONIACCESS.PIXEL_PACK.IN_USE}";
						return speech;
					}
				});
			}
			var capturedContainer = swatchContainer;
			items.Add(new LabelWidget {
				Label = (string)STRINGS.ONIACCESS.PIXEL_PACK.PALETTE,
				GameObject = pixelPack.colorSwatchContainer,
				SuppressTooltip = true,
				Children = paletteChildren,
				SpeechFunc = () => {
					int count = 0;
					for (int i = 0; i < capturedContainer.childCount; i++) {
						if (capturedContainer.GetChild(i).gameObject.activeSelf) count++;
					}
					string countText = string.Format(
						(string)STRINGS.ONIACCESS.PIXEL_PACK.PALETTE_COUNT, count);
					return $"{(string)STRINGS.ONIACCESS.PIXEL_PACK.PALETTE}, {countText}";
				}
			});

			// Active colors group (drillable)
			var activeChildren = new List<Widget>();
			for (int i = 0; i < pixelPack.activeColors.Count; i++) {
				var slotGO = pixelPack.activeColors[i];
				var capturedSlot = slotGO;
				int slotIndex = i + 1;
				string slotLabel = string.Format(
					(string)STRINGS.ONIACCESS.PIXEL_PACK.PIXEL_SLOT, slotIndex);
				activeChildren.Add(new ButtonWidget {
					Label = slotLabel,
					Component = slotGO.GetComponent<KButton>(),
					GameObject = slotGO,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string colorName = ColorNameUtil.GetColorName(
							capturedSlot.GetComponent<Image>().color) ?? capturedSlot.name;
						return string.Format(
							(string)STRINGS.ONIACCESS.PIXEL_PACK.PIXEL_SLOT, slotIndex)
							+ ", " + colorName;
					}
				});
			}
			items.Add(new LabelWidget {
				Label = (string)STRINGS.ONIACCESS.PIXEL_PACK.ACTIVE_COLORS,
				GameObject = pixelPack.activeColorsContainer,
				SuppressTooltip = true,
				Children = activeChildren,
				SpeechFunc = () => (string)STRINGS.ONIACCESS.PIXEL_PACK.ACTIVE_COLORS
			});

			// Standby colors group (drillable)
			var standbyChildren = new List<Widget>();
			for (int i = 0; i < pixelPack.standbyColors.Count; i++) {
				var slotGO = pixelPack.standbyColors[i];
				var capturedSlot = slotGO;
				int slotIndex = i + 1;
				string slotLabel = string.Format(
					(string)STRINGS.ONIACCESS.PIXEL_PACK.PIXEL_SLOT, slotIndex);
				standbyChildren.Add(new ButtonWidget {
					Label = slotLabel,
					Component = slotGO.GetComponent<KButton>(),
					GameObject = slotGO,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string colorName = ColorNameUtil.GetColorName(
							capturedSlot.GetComponent<Image>().color) ?? capturedSlot.name;
						return string.Format(
							(string)STRINGS.ONIACCESS.PIXEL_PACK.PIXEL_SLOT, slotIndex)
							+ ", " + colorName;
					}
				});
			}
			items.Add(new LabelWidget {
				Label = (string)STRINGS.ONIACCESS.PIXEL_PACK.STANDBY_COLORS,
				GameObject = pixelPack.standbyColorsContainer,
				SuppressTooltip = true,
				Children = standbyChildren,
				SpeechFunc = () => (string)STRINGS.ONIACCESS.PIXEL_PACK.STANDBY_COLORS
			});

			// Action buttons (these have LocText labels)
			var buttons = new[] {
				pixelPack.copyActiveToStandbyButton,
				pixelPack.copyStandbyToActiveButton,
				pixelPack.swapColorsButton
			};
			foreach (var btn in buttons) {
				if (btn == null || !btn.gameObject.activeSelf) continue;
				var captured = btn;
				string label = GetButtonLabel(captured, captured.transform.name);
				if (!HasVisibleContent(label)) continue;
				items.Add(new ButtonWidget {
					Label = label,
					Component = captured,
					GameObject = captured.gameObject,
					SpeechFunc = () => GetButtonLabel(captured, captured.transform.name)
				});
			}
		}

		private static void WalkTransform(Transform parent, List<Widget> items, HashSet<LocText> claimedLabels) {
			for (int i = 0; i < parent.childCount; i++) {
				var child = parent.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				if (IsSkipped(child.gameObject.name)) {
					continue;
				}

				if (TryAddCategoryContainer(child, items, claimedLabels))
					continue;
				if (TryAddWidget(child, items, claimedLabels))
					continue;
				WalkTransform(child, items, claimedLabels);
			}
		}

		/// <summary>
		/// Try to emit a widget for the given transform. Returns true if a
		/// component was found (caller should not recurse into children).
		/// </summary>
		private static bool TryAddWidget(Transform t, List<Widget> items, HashSet<LocText> claimedLabels) {
			var go = t.gameObject;

			// ReceptacleToggle: compound widget (title + amount + selection toggle).
			// Must be checked first — contains child MultiToggle/LocText that would
			// otherwise be matched individually.
			var receptacleToggle = go.GetComponent<ReceptacleToggle>();
			if (receptacleToggle != null && receptacleToggle.toggle != null) {
				var captured = receptacleToggle;
				if (captured.title != null) claimedLabels.Add(captured.title);
				if (captured.amount != null) claimedLabels.Add(captured.amount);

				string label = captured.title != null
					? captured.title.GetParsedText() : t.name;
				if (!HasVisibleContent(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (ReceptacleToggle)");
					return true;
				}

				items.Add(new ToggleWidget {
					Label = label,
					Component = captured.toggle,
					GameObject = go,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string name = captured.title != null
							? captured.title.GetParsedText() : captured.transform.name;
						string count = captured.amount != null
							? captured.amount.GetParsedText() : null;
						string speech = name;
						if (!string.IsNullOrEmpty(count))
							speech += $", {count} {(string)STRINGS.ONIACCESS.STATES.AVAILABLE}";
						int state = captured.toggle.CurrentState;
						if (state == 1 || state == 3)
							speech += $", {(string)STRINGS.ONIACCESS.STATES.SELECTED}";
						string desc = GetReceptacleDescription(captured);
						if (desc != null)
							speech += $", {desc}";
						return speech;
					}
				});
				return true;
			}

			// KSlider (catches NonLinearSlider which extends KSlider)
			var slider = go.GetComponent<KSlider>();
			if (slider != null) {
				var captured = slider;
				var labelLt = FindChildLocText(t, null)
					?? FindSiblingLocText(t) ?? FindSiblingLocText(t.parent);
				if (labelLt != null) claimedLabels.Add(labelLt);
				string label = ReadLocText(labelLt, t.name);
				if (!HasVisibleContent(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KSlider) parent={t.parent?.name}");
					return true;
				}
				items.Add(new SliderWidget {
					Label = label,
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
				if (labelLt != null) claimedLabels.Add(labelLt);
				string label = ReadLocText(labelLt, t.name);
				if (!HasVisibleContent(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KToggle) parent={t.parent?.name}");
					return true;
				}
				items.Add(new ToggleWidget {
					Label = label,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => {
						string lbl = ReadLocText(labelLt, captured.transform.name);
						string state = IsToggleActive(captured)
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
				if (redundant)
					return true;
				var captured = multiToggle;
				var childLt = FindChildLocText(t, null);
				var sibLt = FindSiblingLocText(t);
				var parentSibLt = FindSiblingLocText(t.parent);
				var labelLt = childLt ?? sibLt ?? parentSibLt;
				if (labelLt != null) claimedLabels.Add(labelLt);
				string label = ReadLocText(labelLt, t.name);
				if (!HasVisibleContent(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (MultiToggle) parent={t.parent?.name}");
					return true;
				}
				items.Add(new ToggleWidget {
					Label = label,
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
				if (labelLt != null) claimedLabels.Add(labelLt);
				string label = ReadLocText(labelLt, t.name);
				if (!HasVisibleContent(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KNumberInputField) parent={t.parent?.name}");
					return true;
				}
				var unitsLt = FindFollowingSiblingLocText(t)
					?? FindFollowingSiblingLocText(t.parent);
				if (unitsLt != null) claimedLabels.Add(unitsLt);
				items.Add(new TextInputWidget {
					Label = label,
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
				if (labelLt != null) claimedLabels.Add(labelLt);
				string label = ReadLocText(labelLt, t.name);
				if (!HasVisibleContent(label)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KInputField) parent={t.parent?.name}");
					return true;
				}
				items.Add(new TextInputWidget {
					Label = label,
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

			// KButton — check for PlayerControlledToggleSideScreen first,
			// where a KButton acts as an on/off toggle with animation only.
			var kbutton = go.GetComponent<KButton>();
			if (kbutton != null) {
				var captured = kbutton;
				var toggleScreen = go.GetComponentInParent<PlayerControlledToggleSideScreen>();
				if (toggleScreen != null && toggleScreen.target != null) {
					var capturedTarget = toggleScreen.target;
					string label = GetButtonLabel(captured, t.name);
					items.Add(new ToggleWidget {
						Label = label,
						Component = captured,
						GameObject = go,
						SpeechFunc = () => {
							string l = GetButtonLabel(captured, captured.transform.name);
							bool on = capturedTarget.ToggleRequested
								? !capturedTarget.ToggledOn()
								: capturedTarget.ToggledOn();
							string state = on
								? (string)STRINGS.ONIACCESS.STATES.ON
								: (string)STRINGS.ONIACCESS.STATES.OFF;
							return $"{l}, {state}";
						}
					});
					return true;
				}
				string label2 = GetButtonLabel(captured, t.name);
				if (!HasVisibleContent(label2)) {
					Util.Log.Warn($"Walker: blank label for {t.name} (KButton) parent={t.parent?.name}");
					return true;
				}
				items.Add(new ButtonWidget {
					Label = label2,
					Component = captured,
					GameObject = go,
					SpeechFunc = () => GetButtonLabel(captured, captured.transform.name)
				});
				return true;
			}

			// LocText (standalone label — emit all non-empty; claimed labels
			// are removed post-walk via claimedLabels set).
			var locText = go.GetComponent<LocText>();
			if (locText != null) {
				var captured = locText;
				string text = captured.GetParsedText();
				if (HasVisibleContent(text)) {
					items.Add(new LabelWidget {
						Label = text,
						GameObject = go,
						SpeechFunc = () => captured.GetParsedText()
					});
					return true;
				}
			}

			return false;
		}

		// ========================================
		// RECEPTACLE HELPERS
		// ========================================

		/// <summary>
		/// Detect a ReceptacleSideScreen category container: HierarchyReferences
		/// with "HeaderLabel" and "GridLayout" whose grid children have
		/// ReceptacleToggle. Emits a single drillable parent Widget with
		/// Children for the seed rows inside.
		/// </summary>
		private static bool TryAddCategoryContainer(Transform t, List<Widget> items, HashSet<LocText> claimedLabels) {
			var href = t.GetComponent<HierarchyReferences>();
			if (href == null) return false;
			if (!href.HasReference("HeaderLabel") || !href.HasReference("GridLayout"))
				return false;

			var grid = href.GetReference<GridLayoutGroup>("GridLayout");
			if (grid == null) return false;

			// Verify at least one grid child has ReceptacleToggle
			bool hasReceptacle = false;
			var gridT = grid.transform;
			for (int i = 0; i < gridT.childCount; i++) {
				var child = gridT.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				if (child.GetComponent<ReceptacleToggle>() != null) {
					hasReceptacle = true;
					break;
				}
			}
			if (!hasReceptacle) return false;

			// Build child widget list from grid contents
			var children = new List<Widget>();
			for (int i = 0; i < gridT.childCount; i++) {
				var child = gridT.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				TryAddWidget(child, children, claimedLabels);
			}

			var headerLt = href.GetReference<LocText>("HeaderLabel");
			if (headerLt != null) claimedLabels.Add(headerLt);

			var capturedHeader = headerLt;
			var capturedGrid = gridT;
			items.Add(new LabelWidget {
				Label = headerLt != null ? headerLt.GetParsedText() : t.name,
				GameObject = t.gameObject,
				SuppressTooltip = true,
				Children = children,
				SpeechFunc = () => {
					string header = capturedHeader != null
						? capturedHeader.GetParsedText() : t.name;
					int activeCount = 0;
					for (int i = 0; i < capturedGrid.childCount; i++) {
						if (capturedGrid.GetChild(i).gameObject.activeSelf)
							activeCount++;
					}
					string countText = string.Format(
						(string)STRINGS.ONIACCESS.RECEPTACLE.SEED_COUNT, activeCount);
					return $"{header}, {countText}";
				}
			});
			return true;
		}

		/// <summary>
		/// Extract the description from a ReceptacleToggle's tooltip.
		/// The tooltip format is "{name}\n\n{description}". Returns just
		/// the description with rich text tags stripped, or null.
		/// </summary>
		private static string GetReceptacleDescription(ReceptacleToggle rt) {
			var tooltip = rt.GetComponent<ToolTip>();
			if (tooltip == null) return null;

			string text = WidgetOps.ReadAllTooltipText(tooltip);
			if (string.IsNullOrEmpty(text)) return null;

			string[] parts = text.Split(new[] { "\n\n" }, System.StringSplitOptions.None);
			if (parts.Length < 2) return null;

			// Skip the first segment (seed name), take the description
			string desc = parts[1];
			// Strip Unity rich text tags
			desc = Regex.Replace(desc, "<[^>]+>", "");
			desc = desc.Trim();
			return string.IsNullOrEmpty(desc) ? null : desc;
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
		/// Returns true if the string contains at least one character that
		/// would produce visible output in speech or on screen. Rejects
		/// null, empty, whitespace-only, and strings made entirely of
		/// Unicode format/zero-width characters (U+200B, U+FEFF, etc.)
		/// that TextMeshPro inserts.
		/// </summary>
		private static bool HasVisibleContent(string text) {
			if (string.IsNullOrEmpty(text)) return false;
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				if (char.IsWhiteSpace(c)) continue;
				var cat = char.GetUnicodeCategory(c);
				if (cat == System.Globalization.UnicodeCategory.Format) continue;
				if (cat == System.Globalization.UnicodeCategory.OtherNotAssigned) continue;
				return true;
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
				if (lt != null && HasVisibleContent(lt.GetParsedText()))
					return lt;
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
				var lt = FindDirectOrSafeChildLocText(sibling);
				if (lt != null && HasVisibleContent(lt.GetParsedText()))
					return lt;
			}

			for (int i = myIndex + 1; i < parent.childCount; i++) {
				var sibling = parent.GetChild(i);
				if (!sibling.gameObject.activeSelf) continue;
				var lt = FindDirectOrSafeChildLocText(sibling);
				if (lt != null && HasVisibleContent(lt.GetParsedText()))
					return lt;
			}

			return null;
		}

		/// <summary>
		/// Get the LocText directly on a sibling transform. Does not search
		/// children — a LocText nested inside a container (StateIndicator,
		/// input field internals, etc.) belongs to that container, not to
		/// the widget searching for a label.
		/// </summary>
		private static LocText FindDirectOrSafeChildLocText(Transform sibling) {
			return sibling.GetComponent<LocText>();
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
				if (HasInteractiveDescendant(sibling)) break;
				var lt = FindDirectOrSafeChildLocText(sibling);
				if (lt != null && HasVisibleContent(lt.GetParsedText())) {
					// If the next active sibling contains a widget, this
					// LocText is a label for that widget, not a units suffix
					if (NextActiveSiblingHasWidget(parent, i))
						return null;
					return lt;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns true if the next active sibling after <paramref name="afterIndex"/>
		/// contains an interactive widget. Used to distinguish units suffixes
		/// from widget labels in FindFollowingSiblingLocText.
		/// </summary>
		private static bool NextActiveSiblingHasWidget(Transform parent, int afterIndex) {
			for (int i = afterIndex + 1; i < parent.childCount; i++) {
				var sibling = parent.GetChild(i);
				if (!sibling.gameObject.activeSelf) continue;
				return HasInteractiveDescendant(sibling);
			}
			return false;
		}

		/// <summary>
		/// Returns true if the KToggle is in the "active/selected" state.
		/// Prefers ImageToggleState.GetIsActive() which reflects the true
		/// visual state. Some screens (ThresholdSwitchSideScreen) use isOn
		/// inversely — the visually active toggle has isOn=false.
		/// Falls back to KToggle.isOn when no ImageToggleState is present.
		/// </summary>
		internal static bool IsToggleActive(KToggle toggle) {
			var its = toggle.GetComponent<ImageToggleState>();
			if (its != null) return its.GetIsActive();
			return toggle.isOn;
		}

		private static bool HasInteractiveDescendant(Transform t) {
			if (t.GetComponentInChildren<KSlider>() != null) return true;
			if (t.GetComponentInChildren<KToggle>() != null) return true;
			if (t.GetComponentInChildren<MultiToggle>() != null) return true;
			if (t.GetComponentInChildren<KNumberInputField>() != null) return true;
			if (t.GetComponentInChildren<KInputField>() != null) return true;
			return false;
		}

		/// <summary>
		/// Read a LocText reference, falling back to a name-based label.
		/// </summary>
		private static string ReadLocText(LocText lt, string fallback) {
			if (lt != null) {
				string text = lt.GetParsedText();
				if (HasVisibleContent(text))
					return text;
			}
			return fallback;
		}

		/// <summary>
		/// Read a KButton's label from its child LocText using GetParsedText().
		/// Falls back to the enclosing SideScreenContent title when the
		/// button has no text (e.g., animated icon-only buttons).
		/// </summary>
		private static string GetButtonLabel(KButton button, string fallback) {
			var lt = button.GetComponentInChildren<LocText>();
			if (lt != null) {
				string text = lt.GetParsedText();
				if (!string.IsNullOrEmpty(text))
					return text;
			}
			var screen = button.GetComponentInParent<SideScreenContent>();
			if (screen != null) {
				string title = screen.GetTitle();
				if (!string.IsNullOrEmpty(title))
					return title;
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
			// ThresholdSwitchSideScreen uses abbreviated names for its
			// mouse-only +/- step buttons: "Inc Major", "Inc Minor", etc.
			if (name.StartsWith("Inc ", System.StringComparison.OrdinalIgnoreCase)) return true;
			if (name.StartsWith("Dec ", System.StringComparison.OrdinalIgnoreCase)) return true;
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
				List<Widget> items, string screenTitle, Transform screenRoot,
				HashSet<LocText> claimedLabels) {
			// Group consecutive KToggle items by parent transform
			var groups = new List<(Transform parent, int start, int count)>();
			int i = 0;
			while (i < items.Count) {
				var w = items[i];
				if (!(w is ToggleWidget) || !(w.Component is KToggle)) { i++; continue; }

				var parent = w.GameObject.transform.parent;
				int start = i;
				int count = 1;
				while (i + count < items.Count
					&& items[i + count] is ToggleWidget
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

				// Verify exactly one active (confirms mutual exclusivity)
				int onCount = 0;
				for (int j = start; j < start + count; j++) {
					if (IsToggleActive((KToggle)items[j].Component))
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
				var descriptionLt = FindOrphanDescription(screenRoot, emittedObjects, claimedLabels);

				string groupLabel = screenTitle ?? items[start].Label;
				var radioMembers = members;
				items[start] = new DropdownWidget {
					Label = groupLabel,
					Component = members[0].Toggle,
					GameObject = parent.gameObject,
					Tag = radioMembers,
					SpeechFunc = () => {
						string selected = null;
						for (int k = 0; k < radioMembers.Count; k++) {
							if (radioMembers[k].Toggle != null && IsToggleActive(radioMembers[k].Toggle)) {
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

		// ========================================
		// ALARM SIDE SCREEN OVERRIDE
		// ========================================

		/// <summary>
		/// AlarmSideScreen has 3 icon-only MultiToggle buttons for notification
		/// type (Bad, Neutral, DuplicantThreatening). The generic walker only
		/// emits the first one (IsRedundantMultiToggle kills siblings). This
		/// method replaces that single item with a Dropdown built from the
		/// screen's toggles_by_type dictionary, using tooltip text as labels.
		/// </summary>
		private static void CollapseAlarmTypeButtons(
				AlarmSideScreen alarm, List<Widget> items,
				HashSet<LocText> claimedLabels) {
			Dictionary<NotificationType, MultiToggle> togglesByType;
			try {
				togglesByType = Traverse.Create(alarm)
					.Field<Dictionary<NotificationType, MultiToggle>>("toggles_by_type").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"CollapseAlarmTypeButtons: toggles_by_type read failed: {ex.Message}");
				return;
			}
			if (togglesByType == null || togglesByType.Count == 0) return;

			// Find the existing MultiToggle item from the type buttons
			var typeToggles = new HashSet<MultiToggle>();
			foreach (var kv in togglesByType)
				typeToggles.Add(kv.Value);

			int insertIndex = -1;
			for (int i = items.Count - 1; i >= 0; i--) {
				var mt = items[i].Component as MultiToggle;
				if (mt != null && typeToggles.Contains(mt)) {
					if (insertIndex < 0) insertIndex = i;
					items.RemoveAt(i);
				}
			}
			if (insertIndex < 0) insertIndex = items.Count;

			// Build RadioMember list in the game's validTypes order
			var validTypes = new[] {
				NotificationType.Bad,
				NotificationType.Neutral,
				NotificationType.DuplicantThreatening
			};
			var members = new List<RadioMember>();
			foreach (var type in validTypes) {
				if (!togglesByType.TryGetValue(type, out var mt)) continue;
				var tooltip = mt.GetComponent<ToolTip>();
				string label = tooltip != null
					? WidgetOps.ReadAllTooltipText(tooltip) : type.ToString();
				if (!HasVisibleContent(label)) label = type.ToString();
				members.Add(new RadioMember {
					Label = label,
					MultiToggleRef = mt,
					Tag = type
				});
			}
			if (members.Count == 0) return;

			// Claim the "Type:" label LocText so it doesn't appear as a
			// standalone label alongside the Dropdown
			var buttonsParent = members[0].MultiToggleRef.transform.parent;
			if (buttonsParent != null && buttonsParent.parent != null) {
				var typeLt = FindSiblingLocText(buttonsParent);
				if (typeLt != null) claimedLabels.Add(typeLt);
			}

			string groupLabel = (string)STRINGS.UI.UISIDESCREENS.LOGICALARMSIDESCREEN.TOOLTIPS.TYPE;
			var radioMembers = members;
			var capturedAlarm = alarm;
			items.Insert(insertIndex, new DropdownWidget {
				Label = groupLabel,
				Component = members[0].MultiToggleRef,
				SuppressTooltip = true,
				GameObject = buttonsParent != null ? buttonsParent.gameObject
					: members[0].MultiToggleRef.gameObject,
				Tag = radioMembers,
				SpeechFunc = () => {
					string selected = radioMembers[0].Label;
					var activeType = capturedAlarm.targetAlarm.notificationType;
					for (int k = 0; k < radioMembers.Count; k++) {
						if (radioMembers[k].Tag is NotificationType nt && nt == activeType) {
							selected = radioMembers[k].Label;
							break;
						}
					}
					return $"{groupLabel}, {selected}";
				}
			});
		}

		/// <summary>
		/// Search the screen's full transform tree for a LocText that wasn't
		/// emitted as a widget item — a candidate description label. Skips
		/// LocTexts that are children of interactive widgets (those are labels,
		/// not descriptions).
		/// </summary>
		private static LocText FindOrphanDescription(
				Transform root, HashSet<GameObject> emittedObjects,
				HashSet<LocText> claimedLabels) {
			var allLocTexts = root.GetComponentsInChildren<LocText>(false);
			foreach (var lt in allLocTexts) {
				if (emittedObjects.Contains(lt.gameObject)) continue;
				if (claimedLabels.Contains(lt)) continue;

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

	}
}
