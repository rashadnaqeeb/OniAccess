using System.Collections.Generic;
using Database;
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
			SideScreenWalker.RegisterOverride<OwnablesSidescreen>(WalkOwnables);
			SideScreenWalker.RegisterOverride<AssignableSideScreen>(WalkAssignable);
			SideScreenWalker.RegisterOverride<BionicSideScreen>(WalkBionic);
			SideScreenWalker.RegisterOverride<ArtableSelectionSideScreen>(WalkArtableSelection);
			SideScreenWalker.RegisterOverride<MonumentSideScreen>(WalkMonument);
			SideScreenWalker.RegisterOverride<CritterSensorSideScreen>(WalkCritterSensor);
			SideScreenWalker.RegisterOverride<HighEnergyParticleDirectionSideScreen>(WalkHEPDirection);
			SideScreenWalker.RegisterOverride<TelepadSideScreen>(WalkTelepad);
			SideScreenWalker.RegisterOverride<ClusterDestinationSideScreen>(WalkClusterDestination);
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

		static void WalkOwnables(OwnablesSidescreen screen, List<Widget> items) {
			OwnablesSidescreenCategoryRow[] categoryRows;
			try {
				categoryRows = Traverse.Create(screen)
					.Field<OwnablesSidescreenCategoryRow[]>("categoryRows").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkOwnables: categoryRows read failed: {ex.Message}");
				return;
			}
			if (categoryRows == null) return;

			foreach (var categoryRow in categoryRows) {
				if (categoryRow == null || !categoryRow.gameObject.activeSelf) continue;

				OwnablesSidescreenItemRow[] itemRows;
				try {
					itemRows = Traverse.Create(categoryRow)
						.Field<OwnablesSidescreenItemRow[]>("itemRows").Value;
				} catch (System.Exception ex) {
					Util.Log.Warn($"WalkOwnables: itemRows read failed: {ex.Message}");
					continue;
				}

				var children = new List<Widget>();
				if (itemRows != null) {
					foreach (var row in itemRows) {
						if (row == null || !row.gameObject.activeSelf) continue;
						AddOwnableItemRow(row, children);
					}
				}

				var capturedCategory = categoryRow;
				items.Add(new LabelWidget {
					Label = capturedCategory.titleLabel.text,
					GameObject = capturedCategory.gameObject,
					SuppressTooltip = true,
					Children = children,
					SpeechFunc = () => capturedCategory.titleLabel.text
				});
			}
		}

		static void WalkAssignable(AssignableSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkAssignable: Traverse create failed: {ex.Message}");
				return;
			}

			// Dupe rows (currentOwnerText is always "-", skip it)
			GameObject rowGroup;
			try { rowGroup = tv.Field<GameObject>("rowGroup").Value; } catch (System.Exception ex) {
				Util.Log.Warn($"WalkAssignable: rowGroup read failed: {ex.Message}");
				return;
			}
			if (rowGroup == null) return;

			var rowGroupT = rowGroup.transform;
			for (int i = 0; i < rowGroupT.childCount; i++) {
				var child = rowGroupT.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var row = child.GetComponent<AssignableSideScreenRow>();
				if (row == null) continue;
				try {
					AddAssignableRow(row, items);
				} catch (System.Exception ex) {
					Util.Log.Warn($"WalkAssignable: row '{child.name}' failed: {ex.Message}");
				}
			}
		}

		private static void AddAssignableRow(
				AssignableSideScreenRow row, List<Widget> items) {
			var capturedRow = row;
			var toggle = row.GetComponent<MultiToggle>();
			LocText assignmentText;
			try {
				assignmentText = Traverse.Create(row)
					.Field<LocText>("assignmentText").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"AddAssignableRow: assignmentText read failed: {ex.Message}");
				return;
			}
			var capturedAssignmentText = assignmentText;

			string label = row.targetIdentity != null
				? row.targetIdentity.GetProperName() : row.transform.name;

			items.Add(new ButtonWidget {
				Label = label,
				Component = toggle,
				GameObject = row.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string name = capturedRow.targetIdentity != null
						? capturedRow.targetIdentity.GetProperName()
						: capturedRow.transform.name;
					string state;
					switch (capturedRow.currentState) {
						case AssignableSideScreenRow.AssignableState.Selected:
							state = (string)STRINGS.ONIACCESS.STATES.ASSIGNED;
							break;
						case AssignableSideScreenRow.AssignableState.Unassigned:
							state = (string)STRINGS.ONIACCESS.STATES.UNASSIGNED;
							break;
						case AssignableSideScreenRow.AssignableState.Disabled:
							state = (string)STRINGS.UI.UISIDESCREENS
								.ASSIGNABLESIDESCREEN.DISABLED;
							break;
						case AssignableSideScreenRow.AssignableState.AssignedToOther:
							state = capturedAssignmentText != null
								? capturedAssignmentText.GetParsedText()
								: (string)STRINGS.ONIACCESS.STATES.UNASSIGNED;
							break;
						default:
							state = (string)STRINGS.ONIACCESS.STATES.UNASSIGNED;
							break;
					}
					return $"{name}, {state}";
				}
			});
		}

		static void WalkBionic(BionicSideScreen screen, List<Widget> items) {
			List<BionicSideScreenUpgradeSlot> slots;
			try {
				slots = Traverse.Create(screen)
					.Field<List<BionicSideScreenUpgradeSlot>>("bionicSlots").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkBionic: bionicSlots read failed: {ex.Message}");
				return;
			}
			if (slots == null) return;

			foreach (var slot in slots) {
				if (slot == null || !slot.gameObject.activeSelf) continue;
				var capturedSlot = slot;
				items.Add(new ButtonWidget {
					Label = slot.label.GetParsedText(),
					Component = slot.toggle,
					GameObject = slot.gameObject,
					SuppressTooltip = true,
					SpeechFunc = () => {
						var upgrade = capturedSlot.upgradeSlot;
						if (upgrade.IsLocked)
							return (string)STRINGS.ONIACCESS.STATES.LOCKED;
						if (upgrade.HasUpgradeInstalled)
							return $"{upgrade.installedUpgradeComponent.GetProperName()}, {capturedSlot.label.GetParsedText()}";
						if (upgrade.HasUpgradeComponentAssigned)
							return $"{upgrade.assignedUpgradeComponent.GetProperName()}, {capturedSlot.label.GetParsedText()}";
						return capturedSlot.label.GetParsedText();
					}
				});
			}
		}

		private static void AddOwnableItemRow(
				OwnablesSidescreenItemRow row, List<Widget> children) {
			var capturedRow = row;
			children.Add(new ButtonWidget {
				Label = capturedRow.textLabel.GetParsedText(),
				Component = capturedRow.toggle,
				GameObject = capturedRow.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string text = capturedRow.textLabel.GetParsedText();
					if (capturedRow.IsLocked)
						text += $", {(string)STRINGS.ONIACCESS.STATES.LOCKED}";
					else if (capturedRow.SlotIsAssigned)
						text += $", {(string)STRINGS.ONIACCESS.STATES.ASSIGNED}";
					return text;
				}
			});
		}

		static void WalkArtableSelection(ArtableSelectionSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkArtableSelection: Traverse create failed: {ex.Message}");
				return;
			}

			Artable target;
			Dictionary<string, MultiToggle> buttons;
			try {
				target = tv.Field<Artable>("target").Value;
				buttons = tv.Field<Dictionary<string, MultiToggle>>("buttons").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkArtableSelection: field read failed: {ex.Message}");
				return;
			}
			if (target == null || buttons == null) return;

			var prefabId = target.GetComponent<KPrefabID>().PrefabID();
			var stages = Db.GetArtableStages().GetPrefabStages(prefabId);
			var stageById = new Dictionary<string, ArtableStage>();
			foreach (var stage in stages)
				stageById[stage.id] = stage;

			var capturedScreen = screen;
			foreach (var kv in buttons) {
				var mt = kv.Value;
				if (!mt.gameObject.activeSelf) continue;
				if (!stageById.TryGetValue(kv.Key, out var stage)) continue;

				var capturedMt = mt;
				var capturedStageId = kv.Key;
				var capturedStage = stage;
				items.Add(new ToggleWidget {
					Label = capturedStage.Name,
					Component = capturedMt,
					GameObject = capturedMt.gameObject,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string selectedId;
						try {
							selectedId = Traverse.Create(capturedScreen)
								.Field<string>("selectedStage").Value;
						} catch {
							selectedId = "";
						}
						bool isSelected = capturedStageId == selectedId;
						string decor = capturedStage.decor >= 0
							? $"+{capturedStage.decor}" : capturedStage.decor.ToString();
						string speech = capturedStage.Name;
						if (isSelected)
							speech += $", {(string)STRINGS.ONIACCESS.STATES.SELECTED}";
						speech += $", {decor} decor";
						return speech;
					}
				});
			}

			string applyLabel = (string)STRINGS.UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.APPLYBUTTON;
			var applyBtn = screen.applyButton;
			if (applyBtn != null && applyBtn.gameObject.activeSelf) {
				items.Add(new ButtonWidget {
					Label = applyLabel,
					Component = applyBtn,
					GameObject = applyBtn.gameObject,
					SpeechFunc = () => applyLabel
				});
			}

			string clearLabel = (string)STRINGS.UI.UISIDESCREENS
				.OWNABLESSECONDSIDESCREEN.NONE_ROW_LABEL;
			var clearBtn = screen.clearButton;
			if (clearBtn != null && clearBtn.gameObject.activeSelf) {
				items.Add(new ButtonWidget {
					Label = clearLabel,
					Component = clearBtn,
					GameObject = clearBtn.gameObject,
					SpeechFunc = () => clearLabel
				});
			}
		}

		static void WalkMonument(MonumentSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkMonument: Traverse create failed: {ex.Message}");
				return;
			}

			MonumentPart target;
			List<GameObject> buttons;
			try {
				target = tv.Field<MonumentPart>("target").Value;
				buttons = tv.Field<List<GameObject>>("buttons").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkMonument: field read failed: {ex.Message}");
				return;
			}
			if (target == null || buttons == null) return;

			var parts = Db.GetMonumentParts().GetParts(target.part);
			var capturedTarget = target;
			for (int i = 0; i < buttons.Count && i < parts.Count; i++) {
				var btnGO = buttons[i];
				if (!btnGO.activeSelf) continue;
				var kButton = btnGO.GetComponent<KButton>();
				if (kButton == null) continue;

				var capturedPart = parts[i];
				items.Add(new ButtonWidget {
					Label = capturedPart.Name,
					Component = kButton,
					GameObject = btnGO,
					SuppressTooltip = true,
					SpeechFunc = () => {
						string chosenState;
						try {
							chosenState = Traverse.Create(capturedTarget)
								.Field<string>("chosenState").Value;
						} catch {
							chosenState = "";
						}
						string speech = capturedPart.Name;
						if (capturedPart.Id == chosenState)
							speech += $", {(string)STRINGS.ONIACCESS.STATES.SELECTED}";
						return speech;
					}
				});
			}

			var flipBtn = screen.flipButton;
			if (flipBtn != null && flipBtn.gameObject.activeSelf) {
				var captured = flipBtn;
				string label = SideScreenWalker.GetButtonLabel(captured, captured.transform.name);
				items.Add(new ButtonWidget {
					Label = label,
					Component = captured,
					GameObject = captured.gameObject,
					SpeechFunc = () => SideScreenWalker.GetButtonLabel(captured, captured.transform.name)
				});
			}
		}
		static void WalkCritterSensor(CritterSensorSideScreen screen, List<Widget> items) {
			var capturedScreen = screen;
			string crittersLabel = (string)STRINGS.BUILDINGS.PREFABS
				.LOGICCRITTERCOUNTSENSOR.COUNT_CRITTER_LABEL;
			items.Add(new ToggleWidget {
				Label = crittersLabel,
				Component = screen.countCrittersToggle,
				GameObject = screen.countCrittersToggle.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string state = capturedScreen.targetSensor.countCritters
						? (string)STRINGS.ONIACCESS.STATES.ON
						: (string)STRINGS.ONIACCESS.STATES.OFF;
					return $"{crittersLabel}, {state}";
				}
			});

			string eggsLabel = (string)STRINGS.BUILDINGS.PREFABS
				.LOGICCRITTERCOUNTSENSOR.COUNT_EGG_LABEL;
			items.Add(new ToggleWidget {
				Label = eggsLabel,
				Component = screen.countEggsToggle,
				GameObject = screen.countEggsToggle.gameObject,
				SuppressTooltip = true,
				SpeechFunc = () => {
					string state = capturedScreen.targetSensor.countEggs
						? (string)STRINGS.ONIACCESS.STATES.ON
						: (string)STRINGS.ONIACCESS.STATES.OFF;
					return $"{eggsLabel}, {state}";
				}
			});
		}

		static void WalkHEPDirection(
				HighEnergyParticleDirectionSideScreen screen, List<Widget> items) {
			string[] dirStrings;
			try {
				dirStrings = Traverse.Create(screen)
					.Field<string[]>("directionStrings").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkHEPDirection: directionStrings read failed: {ex.Message}");
				return;
			}
			if (dirStrings == null) return;

			for (int i = 0; i < screen.Buttons.Count && i < dirStrings.Length; i++) {
				var btn = screen.Buttons[i];
				var capturedBtn = btn;
				string dirLabel = dirStrings[i];
				items.Add(new ToggleWidget {
					Label = dirLabel,
					Component = capturedBtn,
					GameObject = capturedBtn.gameObject,
					SuppressTooltip = true,
					SpeechFunc = () => {
						if (!capturedBtn.isInteractable)
							return $"{dirLabel}, {(string)STRINGS.ONIACCESS.STATES.SELECTED}";
						return dirLabel;
					}
				});
			}
		}

		static void WalkTelepad(TelepadSideScreen screen, List<Widget> items) {
			Traverse tv;
			try { tv = Traverse.Create(screen); } catch (System.Exception ex) {
				Util.Log.Warn($"WalkTelepad: Traverse create failed: {ex.Message}");
				return;
			}

			LocText timeLabel;
			KButton viewImmigrantsBtn;
			KButton viewColonySummaryBtn;
			Image newAchievementsEarned;
			KButton openRolesScreenButton;
			Image skillPointsAvailable;
			GameObject victoryConditionsContainer;
			try {
				timeLabel = tv.Field<LocText>("timeLabel").Value;
				viewImmigrantsBtn = tv.Field<KButton>("viewImmigrantsBtn").Value;
				viewColonySummaryBtn = tv.Field<KButton>("viewColonySummaryBtn").Value;
				newAchievementsEarned = tv.Field<Image>("newAchievementsEarned").Value;
				openRolesScreenButton = tv.Field<KButton>("openRolesScreenButton").Value;
				skillPointsAvailable = tv.Field<Image>("skillPointsAvailable").Value;
				victoryConditionsContainer = tv.Field<GameObject>("victoryConditionsContainer").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"WalkTelepad: field read failed: {ex.Message}");
				return;
			}

			// timeLabel and viewImmigrantsBtn are mutually exclusive
			if (timeLabel != null && timeLabel.gameObject.activeSelf) {
				var capturedTime = timeLabel;
				items.Add(new LabelWidget {
					Label = capturedTime.GetParsedText(),
					GameObject = capturedTime.gameObject,
					SpeechFunc = () => capturedTime.GetParsedText()
				});
			}

			if (viewImmigrantsBtn != null && viewImmigrantsBtn.gameObject.activeSelf) {
				var captured = viewImmigrantsBtn;
				string label = (string)STRINGS.UI.IMMIGRANTSCREEN.IMMIGRANTSCREENTITLE;
				items.Add(new ButtonWidget {
					Label = label,
					Component = captured,
					GameObject = captured.gameObject,
					SpeechFunc = () => label
				});
			}

			if (viewColonySummaryBtn != null && viewColonySummaryBtn.gameObject.activeSelf) {
				var captured = viewColonySummaryBtn;
				var capturedBadge = newAchievementsEarned;
				string label = (string)STRINGS.UI.UISIDESCREENS
					.TELEPADSIDESCREEN.SUMMARY_TITLE;
				items.Add(new ButtonWidget {
					Label = label,
					Component = captured,
					GameObject = captured.gameObject,
					SpeechFunc = () => {
						string speech = label;
						if (capturedBadge != null
							&& capturedBadge.gameObject.activeSelf)
							speech += $", {(string)STRINGS.ONIACCESS.TELEPAD.NEW_ACHIEVEMENTS}";
						return speech;
					}
				});
			}

			if (openRolesScreenButton != null && openRolesScreenButton.gameObject.activeSelf) {
				var captured = openRolesScreenButton;
				var capturedBadge = skillPointsAvailable;
				string label = (string)STRINGS.UI.UISIDESCREENS
					.TELEPADSIDESCREEN.SKILLS_BUTTON;
				items.Add(new ButtonWidget {
					Label = label,
					Component = captured,
					GameObject = captured.gameObject,
					SpeechFunc = () => {
						string speech = label;
						if (capturedBadge != null
							&& capturedBadge.gameObject.activeSelf)
							speech += $", {(string)STRINGS.ONIACCESS.TELEPAD.SKILL_POINTS}";
						return speech;
					}
				});
			}

			// Victory conditions
			if (victoryConditionsContainer == null) return;
			var containerT = victoryConditionsContainer.transform;
			for (int g = 0; g < containerT.childCount; g++) {
				var groupT = containerT.GetChild(g);
				if (!groupT.gameObject.activeSelf) continue;
				var groupHref = groupT.GetComponent<HierarchyReferences>();
				if (groupHref == null) continue;

				LocText groupLabel;
				try {
					groupLabel = groupHref.GetReference<LocText>("Label");
				} catch { continue; }
				if (groupLabel == null) continue;

				var children = new List<Widget>();
				for (int r = 0; r < groupT.childCount; r++) {
					var rowT = groupT.GetChild(r);
					if (!rowT.gameObject.activeSelf) continue;
					var rowHref = rowT.GetComponent<HierarchyReferences>();
					if (rowHref == null) continue;

					LocText rowLabel;
					Image rowCheck;
					try {
						rowLabel = rowHref.GetReference<LocText>("Label");
						rowCheck = rowHref.GetReference<Image>("Check");
					} catch { continue; }
					if (rowLabel == null || rowCheck == null) continue;
					if (rowLabel == groupLabel) continue;

					var capturedLabel = rowLabel;
					var capturedCheck = rowCheck;
					children.Add(new LabelWidget {
						Label = capturedLabel.GetParsedText(),
						GameObject = rowT.gameObject,
						SpeechFunc = () => {
							string status = capturedCheck.enabled
								? (string)STRINGS.ONIACCESS.STATES.CONDITION_MET
								: (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
							return $"{status}, {capturedLabel.GetParsedText()}";
						}
					});
				}

				var capturedGroupLabel = groupLabel;
				items.Add(new LabelWidget {
					Label = capturedGroupLabel.GetParsedText(),
					GameObject = groupT.gameObject,
					SuppressTooltip = true,
					Children = children.Count > 0 ? children : null,
					SpeechFunc = () => capturedGroupLabel.GetParsedText()
				});
			}
		}

		static void WalkClusterDestination(
				ClusterDestinationSideScreen screen, List<Widget> items) {
			SideScreenWalker.WalkDefault(screen, items);

			var rocketSelector = Traverse.Create(screen)
				.Property<RocketClusterDestinationSelector>("targetRocketSelector").Value;
			if (rocketSelector == null) return;

			var pads = LaunchPad.GetLaunchPadsForDestination(
				rocketSelector.GetDestination());

			var members = new List<SideScreenWalker.RadioMember>();
			members.Add(new SideScreenWalker.RadioMember {
				Label = (string)STRINGS.UI.UISIDESCREENS
					.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE,
				OnSelect = () => rocketSelector.SetDestinationPad(null),
				IsActive = () => rocketSelector.GetDestinationPad() == null
			});
			foreach (var pad in pads) {
				var capturedPad = pad;
				members.Add(new SideScreenWalker.RadioMember {
					Label = capturedPad.GetProperName(),
					OnSelect = () => rocketSelector.SetDestinationPad(capturedPad),
					IsActive = () => rocketSelector.GetDestinationPad() == capturedPad
				});
			}

			var openButton = screen.launchPadDropDown.openButton;
			int openButtonIndex = -1;
			for (int i = 0; i < items.Count; i++) {
				if (items[i].Component == openButton) {
					openButtonIndex = i;
					break;
				}
			}

			var platformLabel = screen.landingPlatformInfoLabel;
			var dropdown = new DropdownWidget {
				Label = (string)STRINGS.UI.UISIDESCREENS
					.CLUSTERDESTINATIONSIDESCREEN.LANDING_PLATFORM_LABEL,
				Component = openButton,
				GameObject = screen.launchPadDropDown.gameObject,
				Tag = members,
				SpeechFunc = () => {
					string label = platformLabel.GetParsedText();
					var currentPad = rocketSelector.GetDestinationPad();
					string padName = currentPad != null
						? currentPad.GetProperName()
						: (string)STRINGS.UI.UISIDESCREENS
							.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE;
					return $"{label}, {padName}";
				}
			};

			if (openButtonIndex >= 0)
				items[openButtonIndex] = dropdown;
			else
				items.Add(dropdown);
		}
	}
}
