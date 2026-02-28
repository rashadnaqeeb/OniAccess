using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace OniAccess.Widgets {
	static class SideScreenOverrides {
		public static void RegisterAll() {
			SideScreenWalker.RegisterOverride<PixelPackSideScreen>(WalkPixelPack);
			SideScreenWalker.RegisterOverride<CommandModuleSideScreen>(WalkCommandModule);
			SideScreenWalker.RegisterOverride<ConditionListSideScreen>(WalkConditionList);
			SideScreenWalker.RegisterOverride<AlarmSideScreen>(WalkAlarm);
			SideScreenWalker.RegisterOverride<FewOptionSideScreen>(WalkFewOption);
		}

		static void WalkPixelPack(PixelPackSideScreen pixelPack, List<Widget> items) {
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

			AddColorSlotGroup(pixelPack.activeColors, pixelPack.activeColorsContainer,
				(string)STRINGS.ONIACCESS.PIXEL_PACK.ACTIVE_COLORS, items);
			AddColorSlotGroup(pixelPack.standbyColors, pixelPack.standbyColorsContainer,
				(string)STRINGS.ONIACCESS.PIXEL_PACK.STANDBY_COLORS, items);

			// Action buttons (these have LocText labels)
			var buttons = new[] {
				pixelPack.copyActiveToStandbyButton,
				pixelPack.copyStandbyToActiveButton,
				pixelPack.swapColorsButton
			};
			foreach (var btn in buttons) {
				if (btn == null || !btn.gameObject.activeSelf) continue;
				var captured = btn;
				string label = SideScreenWalker.GetButtonLabel(captured, captured.transform.name);
				if (!SideScreenWalker.HasVisibleContent(label)) continue;
				items.Add(new ButtonWidget {
					Label = label,
					Component = captured,
					GameObject = captured.gameObject,
					SpeechFunc = () => SideScreenWalker.GetButtonLabel(captured, captured.transform.name)
				});
			}
		}

		private static void AddColorSlotGroup(
				List<GameObject> slots, GameObject container,
				string groupLabel, List<Widget> items) {
			var children = new List<Widget>();
			for (int i = 0; i < slots.Count; i++) {
				var slotGO = slots[i];
				var capturedSlot = slotGO;
				int slotIndex = i + 1;
				string slotLabel = string.Format(
					(string)STRINGS.ONIACCESS.PIXEL_PACK.PIXEL_SLOT, slotIndex);
				children.Add(new ButtonWidget {
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
				Label = groupLabel,
				GameObject = container,
				SuppressTooltip = true,
				Children = children,
				SpeechFunc = () => groupLabel
			});
		}

		static void WalkCommandModule(CommandModuleSideScreen screen, List<Widget> items) {
			SideScreenWalker.WalkConditionContainer(screen.conditionListContainer, items);

			var dest = screen.destinationButton;
			if (dest == null || !dest.gameObject.activeSelf) return;
			var captured = dest;
			var childLt = SideScreenWalker.FindChildLocText(dest.transform, null);
			string label = childLt != null
				? childLt.GetParsedText() : dest.transform.name;
			if (!SideScreenWalker.HasVisibleContent(label)) return;
			items.Add(new ToggleWidget {
				Label = label,
				Component = captured,
				GameObject = captured.gameObject,
				SpeechFunc = () => {
					string lbl = childLt != null
						? childLt.GetParsedText()
						: captured.transform.name;
					return $"{lbl}, {WidgetOps.GetMultiToggleState(captured)}";
				}
			});
		}

		static void WalkConditionList(ConditionListSideScreen screen, List<Widget> items) {
			SideScreenWalker.WalkConditionContainer(screen.rowContainer, items);
		}

		static void WalkAlarm(AlarmSideScreen alarm, List<Widget> items) {
			SideScreenWalker.WalkDefault(alarm, items);
			CollapseAlarmTypeButtons(alarm, items);
		}

		private static void CollapseAlarmTypeButtons(
				AlarmSideScreen alarm, List<Widget> items) {
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
			var members = new List<SideScreenWalker.RadioMember>();
			foreach (var type in validTypes) {
				if (!togglesByType.TryGetValue(type, out var mt)) continue;
				var tooltip = mt.GetComponent<ToolTip>();
				string label = tooltip != null
					? WidgetOps.ReadAllTooltipText(tooltip) : type.ToString();
				if (!SideScreenWalker.HasVisibleContent(label)) label = type.ToString();
				members.Add(new SideScreenWalker.RadioMember {
					Label = label,
					MultiToggleRef = mt,
					Tag = type
				});
			}
			if (members.Count == 0) return;

			string groupLabel = (string)STRINGS.UI.UISIDESCREENS.LOGICALARMSIDESCREEN.TOOLTIPS.TYPE;
			var radioMembers = members;
			var capturedAlarm = alarm;
			var buttonsParent = members[0].MultiToggleRef.transform.parent;
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

		static void WalkFewOption(FewOptionSideScreen fewOption, List<Widget> items) {
			SideScreenWalker.WalkDefault(fewOption, items);
			CollapseFewOptionRows(fewOption, items);
		}

		private static void CollapseFewOptionRows(
				FewOptionSideScreen fewOption, List<Widget> items) {
			var rows = fewOption.rows;
			if (rows == null || rows.Count == 0) return;

			FewOptionSideScreen.IFewOptionSideScreen target;
			try {
				target = Traverse.Create(fewOption)
					.Field<FewOptionSideScreen.IFewOptionSideScreen>("targetFewOptions").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"CollapseFewOptionRows: targetFewOptions read failed: {ex.Message}");
				return;
			}
			if (target == null) return;

			// Remove any items the walker already emitted for the row GameObjects
			var rowObjects = new HashSet<GameObject>();
			foreach (var kv in rows)
				rowObjects.Add(kv.Value);
			int insertIndex = -1;
			for (int i = items.Count - 1; i >= 0; i--) {
				if (items[i].GameObject != null && rowObjects.Contains(items[i].GameObject)) {
					insertIndex = i;
					items.RemoveAt(i);
				}
			}
			if (insertIndex < 0) insertIndex = items.Count;

			// Build RadioMember list from the rows
			var members = new List<SideScreenWalker.RadioMember>();
			foreach (var kv in rows) {
				var go = kv.Value;
				var href = go.GetComponent<HierarchyReferences>();
				LocText labelLt = href != null ? href.GetReference<LocText>("label") : null;
				string label = labelLt != null ? labelLt.GetParsedText() : kv.Key.ToString();
				members.Add(new SideScreenWalker.RadioMember {
					Label = label,
					MultiToggleRef = go.GetComponent<MultiToggle>(),
					Tag = kv.Key
				});
			}
			if (members.Count == 0) return;

			string groupLabel = fewOption.GetTitle();
			if (string.IsNullOrEmpty(groupLabel))
				groupLabel = members[0].Label;
			var radioMembers = members;
			var capturedTarget = target;
			var capturedRows = rows;
			items.Insert(insertIndex, new DropdownWidget {
				Label = groupLabel,
				Component = members[0].MultiToggleRef,
				SuppressTooltip = true,
				GameObject = fewOption.rowContainer != null
					? fewOption.rowContainer.gameObject
					: members[0].MultiToggleRef.gameObject,
				Tag = radioMembers,
				SpeechFunc = () => {
					var selectedTag = capturedTarget.GetSelectedOption();
					string selected = null;
					for (int k = 0; k < radioMembers.Count; k++) {
						if (radioMembers[k].Tag is Tag t && t == selectedTag) {
							selected = radioMembers[k].Label;
							break;
						}
					}
					if (selected == null) selected = radioMembers[0].Label;
					string speech = $"{groupLabel}, {selected}";
					// Read tooltip from the selected row for description
					if (capturedRows.TryGetValue(selectedTag, out var rowGO)) {
						var tooltip = rowGO.GetComponent<ToolTip>();
						if (tooltip != null) {
							string desc = WidgetOps.ReadAllTooltipText(tooltip);
							if (!string.IsNullOrEmpty(desc))
								speech += ", " + desc;
						}
					}
					return speech;
				}
			});
		}
	}
}
