using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	class BlueprintTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.COSMETICS.NAME;
		public int StartLevel => 1;
		public string GameTabId => null;

		public bool IsAvailable(GameObject target) {
			var tab = DetailsScreen.Instance?.GetTabOfType(
				DetailsScreen.SidescreenTabTypes.Blueprints);
			return tab != null && tab.IsVisible;
		}

		public void OnTabSelected() {
			var tab = DetailsScreen.Instance?.GetTabOfType(
				DetailsScreen.SidescreenTabTypes.Blueprints);
			if (tab?.tabInstance != null)
				WidgetOps.ClickMultiToggle(tab.tabInstance);
		}

		public void Populate(GameObject target, List<DetailSection> sections) {
			var panel = FindPanel();
			if (panel == null) return;

			bool isDupe = target.GetComponent<MinionIdentity>() != null;
			if (isDupe)
				PopulateDupeMode(panel, sections);
			else
				PopulateBuildingMode(panel, sections);
		}

		private static CosmeticsPanel FindPanel() {
			var ds = DetailsScreen.Instance;
			if (ds == null) return null;
			return ds.GetComponentInChildren<CosmeticsPanel>(true);
		}

		// =============================================
		// BUILDING MODE
		// =============================================

		private static void PopulateBuildingMode(
				CosmeticsPanel panel, List<DetailSection> sections) {
			LocText nameLabel;
			LocText descriptionLabel;
			FacadeSelectionPanel selectionPanel;
			try {
				var t = Traverse.Create(panel);
				nameLabel = t.Field<LocText>("nameLabel").Value;
				descriptionLabel = t.Field<LocText>("descriptionLabel").Value;
				selectionPanel = t.Field<FacadeSelectionPanel>("selectionPanel").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"BlueprintTab: field access failed: {ex.Message}");
				return;
			}

			AddBuildingSection(nameLabel, descriptionLabel, selectionPanel, sections);
		}

		// =============================================
		// DUPE MODE
		// =============================================

		private static void PopulateDupeMode(
				CosmeticsPanel panel, List<DetailSection> sections) {
			LocText nameLabel;
			KButton editButton;
			FacadeSelectionPanel selectionPanel;
			Dictionary<ClothingOutfitUtility.OutfitType, GameObject> outfitCategories;
			ClothingOutfitUtility.OutfitType selectedCategory;
			try {
				var t = Traverse.Create(panel);
				nameLabel = t.Field<LocText>("nameLabel").Value;
				editButton = t.Field<KButton>("editButton").Value;
				selectionPanel = t.Field<FacadeSelectionPanel>("selectionPanel").Value;
				outfitCategories = t.Field<Dictionary<ClothingOutfitUtility.OutfitType, GameObject>>(
					"outfitCategories").Value;
				selectedCategory = t.Field<ClothingOutfitUtility.OutfitType>(
					"selectedOutfitCategory").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"BlueprintTab: dupe field access failed: {ex.Message}");
				return;
			}

			AddDupeInfoSection(nameLabel, editButton, outfitCategories, sections);
			AddFacadeGridSection(selectionPanel, sections,
				GetCategoryName(selectedCategory));
		}

		// =============================================
		// SHARED SECTIONS
		// =============================================

		private static void AddBuildingSection(
				LocText nameLabel, LocText descriptionLabel,
				FacadeSelectionPanel selectionPanel,
				List<DetailSection> sections) {
			var section = new DetailSection();
			section.Header = (string)STRINGS.UI.DETAILTABS.COSMETICS.NAME;

			if (nameLabel != null) {
				var label = nameLabel;
				section.Items.Add(new LabelWidget {
					Label = label.GetParsedText(),
					GameObject = label.gameObject,
					SpeechFunc = () => label.GetParsedText()
				});
			}

			if (descriptionLabel != null) {
				string descText = descriptionLabel.GetParsedText();
				if (!string.IsNullOrEmpty(descText)) {
					var desc = descriptionLabel;
					section.Items.Add(new LabelWidget {
						Label = descText,
						GameObject = desc.gameObject,
						SpeechFunc = () => desc.GetParsedText()
					});
				}
			}

			AddFacadeToggles(selectionPanel, section);

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddDupeInfoSection(
				LocText nameLabel, KButton editButton,
				Dictionary<ClothingOutfitUtility.OutfitType, GameObject> outfitCategories,
				List<DetailSection> sections) {
			var section = new DetailSection();
			section.Header = (string)STRINGS.UI.DETAILTABS.COSMETICS.NAME;

			if (nameLabel != null) {
				var label = nameLabel;
				section.Items.Add(new LabelWidget {
					Label = label.GetParsedText(),
					GameObject = label.gameObject,
					SpeechFunc = () => label.GetParsedText()
				});
			}

			if (editButton != null && editButton.gameObject.activeSelf) {
				var btn = editButton;
				var btnLocText = btn.GetComponentInChildren<LocText>();
				string btnLabel = btnLocText != null ? btnLocText.GetParsedText() : "";
				section.Items.Add(new ButtonWidget {
					Component = btn,
					GameObject = btn.gameObject,
					Label = btnLabel,
					SpeechFunc = () => btnLocText != null ? btnLocText.GetParsedText() : btnLabel
				});
			}

			AddCategoryToggles(outfitCategories, section);

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddCategoryToggles(
				Dictionary<ClothingOutfitUtility.OutfitType, GameObject> outfitCategories,
				DetailSection section) {
			if (outfitCategories == null) return;

			foreach (var kvp in outfitCategories) {
				var catType = kvp.Key;
				var catGO = kvp.Value;
				if (catGO == null || !catGO.activeSelf) continue;

				var multiToggle = catGO.GetComponent<MultiToggle>();
				if (multiToggle == null) continue;

				string catName = GetCategoryName(catType);
				var capturedToggle = multiToggle;
				var capturedName = catName;
				section.Items.Add(new ToggleWidget {
					Component = capturedToggle,
					GameObject = catGO,
					Label = capturedName,
					SpeechFunc = () => {
						bool selected = capturedToggle.CurrentState == 1;
						return selected
							? $"{capturedName}, {(string)STRINGS.ONIACCESS.STATES.SELECTED}"
							: capturedName;
					}
				});
			}
		}

		private static void AddFacadeGridSection(
				FacadeSelectionPanel selectionPanel, List<DetailSection> sections,
				string header) {
			if (selectionPanel == null) return;

			var section = new DetailSection();
			section.Header = header;
			AddFacadeToggles(selectionPanel, section);

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddFacadeToggles(
				FacadeSelectionPanel selectionPanel, DetailSection section) {
			if (selectionPanel == null) return;

			System.Collections.IDictionary toggles;
			try {
				toggles = Traverse.Create(selectionPanel)
					.Field("activeFacadeToggles").GetValue()
					as System.Collections.IDictionary;
			} catch (System.Exception ex) {
				Util.Log.Warn($"BlueprintTab: facade toggles read failed: {ex.Message}");
				return;
			}

			if (toggles == null || toggles.Count == 0) return;

			foreach (System.Collections.DictionaryEntry entry in toggles) {
				var facadeId = (string)entry.Key;
				var facadeToggle = entry.Value;

				GameObject toggleGO;
				MultiToggle multiToggle;
				try {
					var ft = Traverse.Create(facadeToggle);
					toggleGO = ft.Property<GameObject>("gameObject").Value;
					multiToggle = ft.Property<MultiToggle>("multiToggle").Value;
				} catch (System.Exception ex) {
					Util.Log.Warn($"BlueprintTab: toggle field access failed for {facadeId}: {ex.Message}");
					continue;
				}

				if (toggleGO == null || !toggleGO.activeSelf) continue;

				string name = ReadToggleName(toggleGO);
				var capturedId = facadeId;
				var capturedPanel = selectionPanel;
				var capturedGO = toggleGO;
				section.Items.Add(new ToggleWidget {
					Component = multiToggle,
					GameObject = capturedGO,
					Label = name,
					SpeechFunc = () => {
						string n = ReadToggleName(capturedGO);
						bool selected = capturedId == capturedPanel.SelectedFacade;
						return selected
							? $"{n}, {(string)STRINGS.ONIACCESS.STATES.SELECTED}"
							: n;
					}
				});
			}
		}

		private static string GetCategoryName(ClothingOutfitUtility.OutfitType type) {
			switch (type) {
				case ClothingOutfitUtility.OutfitType.Clothing:
					return (string)STRINGS.UI.UISIDESCREENS.BLUEPRINT_TAB.SUBCATEGORY_OUTFIT;
				case ClothingOutfitUtility.OutfitType.AtmoSuit:
					return (string)STRINGS.UI.UISIDESCREENS.BLUEPRINT_TAB.SUBCATEGORY_ATMOSUIT;
				case ClothingOutfitUtility.OutfitType.JetSuit:
					return (string)STRINGS.UI.UISIDESCREENS.BLUEPRINT_TAB.SUBCATEGORY_JETSUIT;
				default:
					return type.ToString();
			}
		}

		private static string ReadToggleName(GameObject toggleGO) {
			var tooltip = toggleGO.GetComponent<ToolTip>();
			if (tooltip != null) {
				string text = WidgetOps.ReadAllTooltipText(tooltip);
				if (!string.IsNullOrEmpty(text)) {
					int nl = text.IndexOf('\n');
					return nl > 0 ? text.Substring(0, nl).Trim() : text;
				}
			}
			return toggleGO.name;
		}
	}
}
