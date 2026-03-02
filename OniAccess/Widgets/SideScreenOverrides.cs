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
			SideScreenWalker.RegisterOverride<TreeFilterableSideScreen>(WalkTreeFilter);
			SideScreenWalker.RegisterOverride<ComplexFabricatorSideScreen>(WalkComplexFabricator);
			SideScreenWalker.RegisterOverride<AccessControlSideScreen>(WalkAccessControl);
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

		static void WalkTreeFilter(TreeFilterableSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkTreeFilter: Traverse create failed: {ex.Message}");
				return;
			}

			// All toggle
			try {
				var allCheckBox = tv.Field<MultiToggle>("allCheckBox").Value;
				var allCheckBoxLabel = tv.Field<LocText>("allCheckBoxLabel").Value;
				if (allCheckBox != null) {
					var capturedBox = allCheckBox;
					var capturedLabel = allCheckBoxLabel;
					items.Add(new ToggleWidget {
						Label = capturedLabel != null
							? capturedLabel.GetParsedText() : "All",
						Component = capturedBox,
						GameObject = capturedBox.gameObject,
						SpeechFunc = () => {
							string lbl = capturedLabel != null
								? capturedLabel.GetParsedText() : "All";
							return $"{lbl}, {WidgetOps.GetMultiToggleState(capturedBox)}";
						}
					});
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkTreeFilter: allCheckBox read failed: {ex.Message}");
			}

			// Sweep Only toggle
			try {
				var transportRow = tv.Field<GameObject>("onlyallowTransportItemsRow").Value;
				if (transportRow != null && transportRow.activeSelf) {
					var transportCheckBox = tv.Field<MultiToggle>(
						"onlyAllowTransportItemsCheckBox").Value;
					if (transportCheckBox != null) {
						var captured = transportCheckBox;
						string label = (string)STRINGS.UI.UISIDESCREENS
							.TREEFILTERABLESIDESCREEN.ONLYALLOWTRANSPORTITEMSBUTTON;
						items.Add(new ToggleWidget {
							Label = label,
							Component = captured,
							GameObject = captured.gameObject,
							SpeechFunc = () =>
								$"{label}, {WidgetOps.GetMultiToggleState(captured)}"
						});
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkTreeFilter: transport row read failed: {ex.Message}");
			}

			// Seasoned Food Only toggle
			try {
				var spicedRow = tv.Field<GameObject>("onlyallowSpicedItemsRow").Value;
				if (spicedRow != null && spicedRow.activeSelf) {
					var spicedCheckBox = tv.Field<MultiToggle>(
						"onlyAllowSpicedItemsCheckBox").Value;
					if (spicedCheckBox != null) {
						var captured = spicedCheckBox;
						string label = (string)STRINGS.UI.UISIDESCREENS
							.TREEFILTERABLESIDESCREEN.ONLYALLOWSPICEDITEMSBUTTON;
						items.Add(new ToggleWidget {
							Label = label,
							Component = captured,
							GameObject = captured.gameObject,
							SpeechFunc = () =>
								$"{label}, {WidgetOps.GetMultiToggleState(captured)}"
						});
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkTreeFilter: spiced row read failed: {ex.Message}");
			}

			// Category rows from rowGroup
			GameObject rowGroup;
			try { rowGroup = tv.Field<GameObject>("rowGroup").Value; } catch (System.Exception ex) {
				Util.Log.Warn($"WalkTreeFilter: rowGroup read failed: {ex.Message}");
				return;
			}
			if (rowGroup == null) return;

			var rowGroupT = rowGroup.transform;
			var capturedScreen = screen;
			for (int i = 0; i < rowGroupT.childCount; i++) {
				var child = rowGroupT.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var row = child.GetComponent<TreeFilterableSideScreenRow>();
				if (row == null) continue;

				try {
					AddTreeFilterRow(row, capturedScreen, items);
				} catch (System.Exception ex) {
					Util.Log.Warn(
						$"WalkTreeFilter: row '{child.name}' failed: {ex.Message}");
				}
			}
		}

		private static void AddTreeFilterRow(
				TreeFilterableSideScreenRow row,
				TreeFilterableSideScreen screen,
				List<Widget> items) {
			var rowTv = Traverse.Create(row);
			var checkBoxToggle = rowTv.Field<MultiToggle>("checkBoxToggle").Value;
			var elementNameLt = rowTv.Field<LocText>("elementName").Value;
			var rowElements = rowTv
				.Field<List<TreeFilterableSideScreenElement>>("rowElements").Value;

			var capturedRow = row;
			var capturedToggle = checkBoxToggle;
			var capturedNameLt = elementNameLt;

			string label = capturedNameLt != null
				? capturedNameLt.GetParsedText() : row.transform.name;

			// Build children for individual elements
			List<Widget> children = null;
			if (rowElements != null && rowElements.Count > 0) {
				children = new List<Widget>();
				var capturedScreen = screen;
				foreach (var elem in rowElements) {
					if (elem == null || !elem.gameObject.activeSelf) continue;
					var capturedElem = elem;
					var elemTag = capturedElem.GetElementTag();
					string elemLabel = elemTag.ProperName();
					children.Add(new ToggleWidget {
						Label = elemLabel,
						Component = capturedElem.GetCheckboxToggle(),
						GameObject = capturedElem.gameObject,
						SuppressTooltip = true,
						SpeechFunc = () => {
							string name = capturedElem.GetElementTag().ProperName();
							string state = capturedElem.IsSelected
								? (string)STRINGS.ONIACCESS.STATES.ON
								: (string)STRINGS.ONIACCESS.STATES.OFF;
							if (capturedScreen.IsStorage) {
								float mass = capturedScreen.GetAmountInStorage(
									capturedElem.GetElementTag());
								string massText = GameUtil.GetFormattedMass(mass);
								return $"{name}, {massText}, {state}";
							}
							return $"{name}, {state}";
						}
					});
				}
			}

			items.Add(new ToggleWidget {
				Label = label,
				Component = capturedToggle,
				GameObject = row.gameObject,
				SuppressTooltip = true,
				Children = children,
				SpeechFunc = () => {
					string name = capturedNameLt != null
						? capturedNameLt.GetParsedText() : capturedRow.transform.name;
					return $"{name}, {RowStateToString(capturedRow.GetState())}";
				}
			});
		}

		private static string RowStateToString(TreeFilterableSideScreenRow.State state) {
			switch (state) {
				case TreeFilterableSideScreenRow.State.On:
					return (string)STRINGS.ONIACCESS.STATES.ON;
				case TreeFilterableSideScreenRow.State.Off:
					return (string)STRINGS.ONIACCESS.STATES.OFF;
				default:
					return (string)STRINGS.ONIACCESS.STATES.MIXED;
			}
		}

		static void WalkComplexFabricator(ComplexFabricatorSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkComplexFabricator: Traverse create failed: {ex.Message}");
				return;
			}

			// Order status labels
			try {
				var currentOrderLabel = tv.Field<LocText>("currentOrderLabel").Value;
				if (currentOrderLabel != null && currentOrderLabel.gameObject.activeSelf) {
					var captured = currentOrderLabel;
					string text = captured.GetParsedText();
					if (SideScreenWalker.HasVisibleContent(text)) {
						items.Add(new LabelWidget {
							Label = text,
							GameObject = captured.gameObject,
							SpeechFunc = () => captured.GetParsedText()
						});
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkComplexFabricator: currentOrderLabel read failed: {ex.Message}");
			}

			try {
				var nextOrderLabel = tv.Field<LocText>("nextOrderLabel").Value;
				if (nextOrderLabel != null && nextOrderLabel.gameObject.activeSelf) {
					var captured = nextOrderLabel;
					string text = captured.GetParsedText();
					if (SideScreenWalker.HasVisibleContent(text)) {
						items.Add(new LabelWidget {
							Label = text,
							GameObject = captured.gameObject,
							SpeechFunc = () => captured.GetParsedText()
						});
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkComplexFabricator: nextOrderLabel read failed: {ex.Message}");
			}

			// No recipes fallback
			try {
				var noRecipesLabel = tv.Field<LocText>("noRecipesDiscoveredLabel").Value;
				if (noRecipesLabel != null && noRecipesLabel.gameObject.activeSelf) {
					var captured = noRecipesLabel;
					string text = captured.GetParsedText();
					if (SideScreenWalker.HasVisibleContent(text)) {
						items.Add(new LabelWidget {
							Label = text,
							GameObject = captured.gameObject,
							SpeechFunc = () => captured.GetParsedText()
						});
						return;
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkComplexFabricator: noRecipesDiscoveredLabel read failed: {ex.Message}");
			}

			// Recipe toggles
			List<GameObject> recipeToggles;
			Dictionary<GameObject, List<ComplexRecipe>> recipeCategoryToggleMap;
			try {
				recipeToggles = tv.Field<List<GameObject>>("recipeToggles").Value;
				recipeCategoryToggleMap = tv
					.Field<Dictionary<GameObject, List<ComplexRecipe>>>("recipeCategoryToggleMap").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkComplexFabricator: recipe fields read failed: {ex.Message}");
				return;
			}
			if (recipeToggles == null || recipeCategoryToggleMap == null) return;

			foreach (var toggleGO in recipeToggles) {
				if (toggleGO == null || !toggleGO.activeSelf) continue;
				var href = toggleGO.GetComponent<HierarchyReferences>();
				if (href == null) continue;

				var toggle = toggleGO.GetComponent<KToggle>();
				if (toggle == null) continue;

				var labelLt = href.GetReference<LocText>("Label");
				string label = labelLt != null ? labelLt.GetParsedText() : toggleGO.name;
				if (!SideScreenWalker.HasVisibleContent(label)) continue;

				var capturedGO = toggleGO;
				var capturedHref = href;
				var capturedLabelLt = labelLt;
				items.Add(new ButtonWidget {
					Label = label,
					Component = toggle,
					GameObject = capturedGO,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string name = capturedLabelLt != null
							? capturedLabelLt.GetParsedText() : capturedGO.name;

						// Queue status
						string queueStatus;
						var infiniteIcon = capturedHref
							.GetReference<RectTransform>("InfiniteIcon");
						if (infiniteIcon != null && infiniteIcon.gameObject.activeSelf) {
							queueStatus = (string)STRINGS.ONIACCESS.FABRICATOR.CONTINUOUS;
						} else {
							var countLabel = capturedHref.GetReference<LocText>("CountLabel");
							string countText = countLabel != null
								? countLabel.GetParsedText() : "";
							if (string.IsNullOrEmpty(countText) || countText == "0") {
								queueStatus = (string)STRINGS.ONIACCESS.FABRICATOR.NOT_QUEUED;
							} else {
								queueStatus = string.Format(
									(string)STRINGS.ONIACCESS.FABRICATOR.QUEUED, countText);
							}
						}

						string speech = $"{name}, {queueStatus}";

						// Availability via label color
						if (capturedLabelLt != null && capturedLabelLt.color.r >= 0.1f) {
							speech += $", {(string)STRINGS.ONIACCESS.FABRICATOR.UNAVAILABLE}";
						}

						// Tech required
						var techRequired = capturedHref
							.GetReference<RectTransform>("TechRequired");
						if (techRequired != null && techRequired.gameObject.activeSelf) {
							speech += $", {(string)STRINGS.ONIACCESS.FABRICATOR.UNAVAILABLE}";
						}

						return speech;
					}
				});
			}
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

		static void WalkAccessControl(AccessControlSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkAccessControl: Traverse create failed: {ex.Message}");
				return;
			}

			AccessControl target;
			try { target = tv.Field<AccessControl>("target").Value; } catch (System.Exception ex) {
				Util.Log.Warn($"WalkAccessControl: target read failed: {ex.Message}");
				return;
			}
			if (target == null) return;

			if (target.overrideAccess == Door.ControlState.Locked) {
				items.Add(new LabelWidget {
					Label = (string)STRINGS.ONIACCESS.ACCESS_CONTROL.LOCKED,
					GameObject = screen.gameObject,
					SpeechFunc = () => (string)STRINGS.ONIACCESS.ACCESS_CONTROL.LOCKED
				});
				return;
			}

			var building = target.GetComponent<Building>();
			bool isRotated = building != null
				&& building.Orientation != Orientation.Neutral;

			var sections = new[] {
				("standardMinionSectionHeader", "standardMinionSectionContent", false),
				("bionicMinionSectionHeader", "bionicMinionSectionContent", false),
				("robotSectionHeader", "robotSectionContent", true)
			};

			foreach (var (headerField, contentField, isRobot) in sections) {
				try {
					AddAccessSection(tv, target, isRotated, headerField, contentField,
						isRobot, items);
				} catch (System.Exception ex) {
					Util.Log.Warn(
						$"WalkAccessControl: {headerField} failed: {ex.Message}");
				}
			}
		}

		private static void AddAccessSection(
				Traverse tv, AccessControl target, bool isRotated,
				string headerField, string contentField, bool isRobot,
				List<Widget> items) {
			GameObject header;
			GameObject content;
			try {
				header = tv.Field<GameObject>(headerField).Value;
				content = tv.Field<GameObject>(contentField).Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkAccessControl: {headerField} read failed: {ex.Message}");
				return;
			}
			if (header == null || !header.activeSelf) return;

			var href = header.GetComponent<HierarchyReferences>();
			var categoryLabel = href.GetReference<LocText>("CategoryLabel");
			var headerToggleLeft = href.GetReference<MultiToggle>("ToggleLeft");
			var headerToggleRight = href.GetReference<MultiToggle>("ToggleRight");
			var collapseToggle = href.GetReference<MultiToggle>("CollapseToggle");

			string leftDir = isRotated
				? (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_UP
				: (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_LEFT;
			string rightDir = isRotated
				? (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_DOWN
				: (string)STRINGS.ONIACCESS.SCANNER.DIRECTION_RIGHT;
			string defaultLeftLabel = isRotated
				? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.DEFAULT_UP
				: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.DEFAULT_LEFT;
			string defaultRightLabel = isRotated
				? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.DEFAULT_DOWN
				: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.DEFAULT_RIGHT;

			var children = new List<Widget>();

			var capturedHeaderLeft = headerToggleLeft;
			children.Add(new ToggleWidget {
				Label = defaultLeftLabel,
				Component = capturedHeaderLeft,
				GameObject = capturedHeaderLeft.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string state = capturedHeaderLeft.CurrentState == 0
						? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.ALLOWED
						: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.BLOCKED;
					return $"{defaultLeftLabel}, {state}";
				}
			});

			var capturedHeaderRight = headerToggleRight;
			children.Add(new ToggleWidget {
				Label = defaultRightLabel,
				Component = capturedHeaderRight,
				GameObject = capturedHeaderRight.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string state = capturedHeaderRight.CurrentState == 0
						? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.ALLOWED
						: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.BLOCKED;
					return $"{defaultRightLabel}, {state}";
				}
			});

			if (content != null) {
				var contentT = content.transform;
				for (int i = 0; i < contentT.childCount; i++) {
					var rowGO = contentT.GetChild(i).gameObject;
					if (!rowGO.activeSelf) continue;
					try {
						AddAccessRow(rowGO, isRotated, isRobot, leftDir, rightDir,
							children);
					} catch (System.Exception ex) {
						Util.Log.Warn(
							$"WalkAccessControl: row {i} failed: {ex.Message}");
					}
				}
			}

			var capturedCatLabel = categoryLabel;
			items.Add(new ToggleWidget {
				Label = capturedCatLabel.GetParsedText(),
				Component = collapseToggle,
				GameObject = header,
				SuppressTooltip = true,
				Children = children,
				SpeechFunc = () => {
					string catName = capturedCatLabel.GetParsedText();
					string leftState = capturedHeaderLeft.CurrentState == 0
						? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.ALLOWED
						: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.BLOCKED;
					string rightState = capturedHeaderRight.CurrentState == 0
						? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.ALLOWED
						: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.BLOCKED;
					return $"{catName}, {leftDir} {leftState}, {rightDir} {rightState}";
				}
			});
		}

		private static void AddAccessRow(
				GameObject rowGO, bool isRotated, bool isRobot,
				string leftDir, string rightDir,
				List<Widget> children) {
			var href = rowGO.GetComponent<HierarchyReferences>();
			if (href == null) return;

			var useDefaultBtn = href.GetReference<MultiToggle>("UseDefaultButton");
			var toggleLeft = href.GetReference<MultiToggle>("ToggleLeft");
			var toggleRight = href.GetReference<MultiToggle>("ToggleRight");
			var directionToggles = href.GetReference<RectTransform>("DirectionToggles");

			var capturedHref = href;
			var capturedRowGO = rowGO;
			children.Add(new ToggleWidget {
				Label = GetRowEntityName(href, isRobot, rowGO),
				Component = useDefaultBtn,
				GameObject = rowGO,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string name = GetRowEntityName(capturedHref, isRobot, capturedRowGO);
					string defaultState = useDefaultBtn.CurrentState == 1
						? (string)STRINGS.UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.USING_DEFAULT
						: (string)STRINGS.UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.USING_CUSTOM;
					return $"{name}, {defaultState}";
				}
			});

			if (directionToggles != null && directionToggles.gameObject.activeSelf) {
				children.Add(new ToggleWidget {
					Label = leftDir,
					Component = toggleLeft,
					GameObject = toggleLeft.gameObject,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string state = toggleLeft.CurrentState == 0
							? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.ALLOWED
							: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.BLOCKED;
						return $"{leftDir}, {state}";
					}
				});

				children.Add(new ToggleWidget {
					Label = rightDir,
					Component = toggleRight,
					GameObject = toggleRight.gameObject,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string state = toggleRight.CurrentState == 0
							? (string)STRINGS.ONIACCESS.ACCESS_CONTROL.ALLOWED
							: (string)STRINGS.ONIACCESS.ACCESS_CONTROL.BLOCKED;
						return $"{rightDir}, {state}";
					}
				});
			}
		}

		private static string GetRowEntityName(
				HierarchyReferences href, bool isRobot, GameObject fallback) {
			if (isRobot) {
				var nameLabel = href.GetReference<LocText>("NameLabel");
				return nameLabel.GetParsedText();
			}
			var portrait = href.GetReference<CrewPortrait>("Portrait");
			var identity = portrait.identityObject;
			return identity != null ? identity.GetProperName() : fallback.name;
		}
	}
}
