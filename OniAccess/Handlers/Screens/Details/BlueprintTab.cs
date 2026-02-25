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

			AddInfoSection(nameLabel, descriptionLabel, sections);
			AddFacadeGridSection(selectionPanel, sections);
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
			try {
				var t = Traverse.Create(panel);
				nameLabel = t.Field<LocText>("nameLabel").Value;
				editButton = t.Field<KButton>("editButton").Value;
				selectionPanel = t.Field<FacadeSelectionPanel>("selectionPanel").Value;
				outfitCategories = t.Field<Dictionary<ClothingOutfitUtility.OutfitType, GameObject>>(
					"outfitCategories").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"BlueprintTab: dupe field access failed: {ex.Message}");
				return;
			}

			AddDupeInfoSection(nameLabel, editButton, sections);
			AddCategorySection(outfitCategories, sections);
			AddFacadeGridSection(selectionPanel, sections);
		}

		// =============================================
		// SHARED SECTIONS
		// =============================================

		private static void AddInfoSection(
				LocText nameLabel, LocText descriptionLabel,
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

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddDupeInfoSection(
				LocText nameLabel, KButton editButton,
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

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddCategorySection(
				Dictionary<ClothingOutfitUtility.OutfitType, GameObject> outfitCategories,
				List<DetailSection> sections) {
			if (outfitCategories == null || outfitCategories.Count == 0) return;

			var section = new DetailSection();
			section.Header = (string)STRINGS.UI.DETAILTABS.COSMETICS.NAME;

			foreach (var kvp in outfitCategories) {
				var catGO = kvp.Value;
				if (catGO == null || !catGO.activeSelf) continue;

				var hierRefs = catGO.GetComponent<HierarchyReferences>();
				var multiToggle = catGO.GetComponent<MultiToggle>();
				if (multiToggle == null) continue;

				LocText labelText = null;
				if (hierRefs != null)
					labelText = hierRefs.GetReference<LocText>("Label");

				var capturedToggle = multiToggle;
				var capturedLabel = labelText;
				section.Items.Add(new ToggleWidget {
					Component = capturedToggle,
					GameObject = catGO,
					Label = capturedLabel != null ? capturedLabel.text : "",
					SpeechFunc = () => {
						string name = capturedLabel != null ? capturedLabel.text : "";
						bool selected = capturedToggle.CurrentState == 1;
						return selected
							? $"{name}, {(string)STRINGS.ONIACCESS.STATES.SELECTED}"
							: name;
					}
				});
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddFacadeGridSection(
				FacadeSelectionPanel selectionPanel, List<DetailSection> sections) {
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

			var section = new DetailSection();
			section.Header = (string)STRINGS.UI.DETAILTABS.COSMETICS.NAME;

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

			if (section.Items.Count > 0)
				sections.Add(section);
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
