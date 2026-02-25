using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	class MaterialTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.MATERIAL.NAME;
		public int StartLevel => 1;
		public string GameTabId => null;

		public bool IsAvailable(GameObject target) {
			var tab = DetailsScreen.Instance?.GetTabOfType(
				DetailsScreen.SidescreenTabTypes.Material);
			return tab != null && tab.IsVisible;
		}

		public void OnTabSelected() {
			var tab = DetailsScreen.Instance?.GetTabOfType(
				DetailsScreen.SidescreenTabTypes.Material);
			if (tab?.tabInstance != null)
				WidgetOps.ClickMultiToggle(tab.tabInstance);
		}

		public void Populate(GameObject target, List<DetailSection> sections) {
			var panel = FindPanel();
			if (panel == null) return;

			AddCurrentMaterialSection(panel, sections);
			AddChangeMaterialSection(panel, sections);
		}

		private static DetailsScreenMaterialPanel FindPanel() {
			var ds = DetailsScreen.Instance;
			if (ds == null) return null;
			return ds.GetComponentInChildren<DetailsScreenMaterialPanel>(true);
		}

		private static void AddCurrentMaterialSection(
				DetailsScreenMaterialPanel panel, List<DetailSection> sections) {
			LocText materialLabel;
			LocText materialDescription;
			DescriptorPanel descriptorPanel;
			try {
				var t = Traverse.Create(panel);
				materialLabel = t.Field<LocText>("currentMaterialLabel").Value;
				materialDescription = t.Field<LocText>("currentMaterialDescription").Value;
				descriptorPanel = t.Field<DescriptorPanel>("descriptorPanel").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"MaterialTab: field access failed: {ex.Message}");
				return;
			}

			var section = new DetailSection();
			section.Header = (string)STRINGS.UI.DETAILTABS.MATERIAL.NAME;

			if (materialLabel != null) {
				var label = materialLabel;
				section.Items.Add(new LabelWidget {
					Label = label.GetParsedText(),
					GameObject = label.gameObject,
					SpeechFunc = () => label.GetParsedText()
				});
			}

			if (materialDescription != null) {
				var desc = materialDescription;
				section.Items.Add(new LabelWidget {
					Label = desc.GetParsedText(),
					GameObject = desc.gameObject,
					SpeechFunc = () => desc.GetParsedText()
				});
			}

			if (descriptorPanel != null && descriptorPanel.gameObject.activeSelf) {
				AddDescriptorLabels(descriptorPanel, section);
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddDescriptorLabels(
				DescriptorPanel descriptorPanel, DetailSection section) {
			List<GameObject> labels;
			try {
				labels = Traverse.Create(descriptorPanel)
					.Field<List<GameObject>>("labels").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"MaterialTab: descriptorPanel labels read failed: {ex.Message}");
				return;
			}
			if (labels == null) return;

			string effectsHeader = (string)STRINGS.ELEMENTS.MATERIAL_MODIFIERS.EFFECTS_HEADER;
			for (int i = 0; i < labels.Count; i++) {
				var labelObj = labels[i];
				if (!labelObj.activeSelf) continue;

				var locText = labelObj.GetComponent<LocText>();
				if (locText == null) continue;

				string text = locText.text;
				if (string.IsNullOrEmpty(text)) continue;

				// Skip the "Effects Header" — it's implicit from the section structure
				if (text.Contains(effectsHeader)) continue;

				var captured = locText;
				section.Items.Add(new LabelWidget {
					Label = text,
					GameObject = labelObj,
					SpeechFunc = () => captured.text
				});
			}
		}

		private static void AddChangeMaterialSection(
				DetailsScreenMaterialPanel panel, List<DetailSection> sections) {
			MaterialSelectionPanel selectionPanel;
			KButton openButton;
			KButton orderButton;
			try {
				var t = Traverse.Create(panel);
				selectionPanel = t.Field<MaterialSelectionPanel>("materialSelectionPanel").Value;
				openButton = t.Field<KButton>("openChangeMaterialPanelButton").Value;
				orderButton = t.Field<KButton>("orderChangeMaterialButton").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"MaterialTab: change material field access failed: {ex.Message}");
				return;
			}

			var section = new DetailSection();
			section.Header = (string)STRINGS.UI.DETAILTABS.MATERIAL.BUTTON_CHANGE_MATERIAL;

			if (selectionPanel != null && selectionPanel.gameObject.activeSelf) {
				AddMaterialToggles(selectionPanel, section);
				AddOrderButton(orderButton, section);
			} else if (openButton != null && openButton.gameObject.activeSelf
					&& openButton.isInteractable) {
				var btn = openButton;
				var btnLocText = btn.GetComponentInChildren<LocText>();
				section.Items.Add(new ButtonWidget {
					Component = btn,
					GameObject = btn.gameObject,
					Label = btnLocText != null ? btnLocText.GetParsedText()
						: (string)STRINGS.UI.USERMENUACTIONS.RECONSTRUCT.REQUEST_RECONSTRUCT,
					SpeechFunc = () => btnLocText != null ? btnLocText.GetParsedText()
						: (string)STRINGS.UI.USERMENUACTIONS.RECONSTRUCT.REQUEST_RECONSTRUCT
				});
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddMaterialToggles(
				MaterialSelectionPanel selectionPanel, DetailSection section) {
			List<MaterialSelector> selectors;
			try {
				selectors = Traverse.Create(selectionPanel)
					.Field<List<MaterialSelector>>("materialSelectors").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"MaterialTab: materialSelectors read failed: {ex.Message}");
				return;
			}
			if (selectors == null || selectors.Count == 0) return;

			var selector = selectors[0];
			if (!selector.gameObject.activeSelf) return;

			var currentTag = selector.CurrentSelectedElement;
			foreach (var kvp in selector.ElementToggles) {
				var tag = kvp.Key;
				var toggle = kvp.Value;
				if (!toggle.gameObject.activeSelf) continue;

				var capturedTag = tag;
				var capturedToggle = toggle;
				section.Items.Add(new ToggleWidget {
					Component = capturedToggle,
					GameObject = capturedToggle.gameObject,
					Label = capturedTag.ProperName(),
					SpeechFunc = () => BuildToggleSpeech(capturedTag, selector)
				});
			}
		}

		private static string BuildToggleSpeech(Tag tag, MaterialSelector selector) {
			string name = tag.ProperName();
			float amount = ClusterManager.Instance.activeWorld.worldInventory
				.GetAmount(tag, includeRelatedWorlds: true);
			string mass = GameUtil.GetFormattedMass(amount);
			bool selected = tag == selector.CurrentSelectedElement;
			return selected
				? $"{name}, {mass}, {(string)STRINGS.ONIACCESS.STATES.SELECTED}"
				: $"{name}, {mass}";
		}

		private static void AddOrderButton(KButton orderButton, DetailSection section) {
			if (orderButton == null) return;
			if (!orderButton.isInteractable) return;

			var btn = orderButton;
			var btnLocText = btn.GetComponentInChildren<LocText>();
			section.Items.Add(new ButtonWidget {
				Component = btn,
				GameObject = btn.gameObject,
				Label = btnLocText != null ? btnLocText.GetParsedText() : "",
				SpeechFunc = () => btnLocText != null ? btnLocText.GetParsedText() : ""
			});
		}
	}
}
