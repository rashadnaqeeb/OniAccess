using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the SimpleInfoScreen (Status tab) into structured sections.
	/// The most information-dense tab: status items, storage, vitals, effects,
	/// requirements, stress, fertility, process conditions, world panels, etc.
	/// All widgets use SpeechFunc for live text — the game updates labels every frame.
	/// </summary>
	class StatusTab : IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.SIMPLEINFO.NAME;
		public string GameTabId => "SIMPLEINFO";

		public bool IsAvailable(GameObject target) => true;

		public void Populate(GameObject target, List<DetailSection> sections) {
			var panel = FindPanel();
			if (panel == null) {
				Util.Log.Warn("StatusTab.Populate: SimpleInfoScreen not found");
				return;
			}

			if (panel.gameObject.activeSelf)
				panel.SetTarget(target);

			AddProcessConditions(panel, sections);
			AddStatusItems(panel, sections);
			AddCollapsibleSection(panel, "spacePOIPanel", sections);
			AddCollapsibleSection(panel, "spaceHexCellStoragePanel", sections);
			AddCollapsibleSection(panel, "rocketStatusContainer", sections);
			AddVitals(panel, sections);
			AddCollapsibleSection(panel, "fertilityPanel", sections);
			AddCollapsibleSection(panel, "mooFertilityPanel", sections);
			AddCollapsibleSection(panel, "infoPanel", sections);
			AddDescriptorSection(panel, "requirementsPanel", "requirementContent", sections);
			AddDescriptorSection(panel, "effectsPanel", "effectsContent", sections);
			AddWorldPanel(panel, "worldMeteorShowersPanel", sections);
			AddWorldPanel(panel, "worldElementsPanel", sections);
			AddWorldPanel(panel, "worldGeysersPanel", sections);
			AddWorldPanel(panel, "worldTraitsPanel", sections);
			AddWorldPanel(panel, "worldBiomesPanel", sections);
			AddWorldPanel(panel, "worldLifePanel", sections);
			AddStorage(panel, sections);
			AddCollapsibleSection(panel, "stressPanel", sections);
			AddCollapsibleSection(panel, "movePanel", sections);
		}

		// ========================================
		// STATUS ITEMS
		// ========================================

		private static void AddStatusItems(SimpleInfoScreen panel, List<DetailSection> sections) {
			List<SimpleInfoScreen.StatusItemEntry> statusItems;
			try {
				statusItems = Traverse.Create(panel)
					.Field<List<SimpleInfoScreen.StatusItemEntry>>("statusItems").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"StatusTab: statusItems read failed: {ex.Message}");
				return;
			}
			if (statusItems == null || statusItems.Count == 0) return;

			var statusPanel = Traverse.Create(panel)
				.Field<CollapsibleDetailContentPanel>("statusItemPanel").Value;
			if (statusPanel == null || !statusPanel.gameObject.activeSelf) return;

			var section = new DetailSection();
			section.Header = statusPanel.HeaderLabel != null
				? statusPanel.HeaderLabel.text
				: (string)STRINGS.UI.DETAILTABS.SIMPLEINFO.GROUPNAME_STATUS;

			foreach (var entry in statusItems) {
				var captured = entry;
				var text = Traverse.Create(captured)
					.Field<LocText>("text").Value;
				if (text == null) {
					Util.Log.Warn("StatusTab: StatusItemEntry.text is null");
					continue;
				}
				var button = Traverse.Create(captured)
					.Field<KButton>("button").Value;

				bool isClickable = button != null && button.enabled;

				section.Items.Add(new WidgetInfo {
					Label = text.text,
					Type = isClickable ? WidgetType.Button : WidgetType.Label,
					Component = isClickable ? (Component)button : null,
					GameObject = text.gameObject,
					SpeechFunc = () => text.text
				});
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		// ========================================
		// STORAGE
		// ========================================

		private static void AddStorage(SimpleInfoScreen panel, List<DetailSection> sections) {
			var storagePanel = panel.StoragePanel;
			if (storagePanel == null || !storagePanel.gameObject.activeSelf) return;

			var section = new DetailSection();
			section.Header = storagePanel.HeaderLabel != null
				&& !string.IsNullOrEmpty(storagePanel.HeaderLabel.text)
				? storagePanel.HeaderLabel.text
				: (string)STRINGS.UI.DETAILTABS.DETAILS.GROUPNAME_CONTENTS;

			var content = storagePanel.Content;
			if (content == null) return;

			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;

				var collapsable = child.GetComponent<DetailCollapsableLabel>();
				if (collapsable != null) {
					AddStorageGroup(collapsable, section);
					continue;
				}

				var withButton = child.GetComponent<DetailLabelWithButton>();
				if (withButton != null) {
					AddStorageItem(withButton, section);
					continue;
				}

				var plain = child.GetComponent<DetailLabel>();
				if (plain != null) {
					var captured = plain;
					section.Items.Add(new WidgetInfo {
						Label = captured.label.text,
						Type = WidgetType.Label,
						GameObject = child.gameObject,
						SpeechFunc = () => captured.label.text
					});
				}
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddStorageItem(DetailLabelWithButton item, DetailSection section) {
			var captured = item;
			section.Items.Add(new WidgetInfo {
				Label = captured.label.text,
				Type = WidgetType.Button,
				Component = captured.button,
				GameObject = captured.gameObject,
				SpeechFunc = () => BuildStorageItemText(captured)
			});
		}

		private static void AddStorageGroup(DetailCollapsableLabel group, DetailSection section) {
			var captured = group;
			var widget = new WidgetInfo {
				Label = $"{captured.nameLabel.text}, {captured.valueLabel.text}",
				Type = WidgetType.Label,
				GameObject = captured.gameObject,
				SpeechFunc = () => $"{captured.nameLabel.text}, {captured.valueLabel.text}"
			};

			if (captured.IsExpanded) {
				var children = new List<WidgetInfo>();
				foreach (var row in captured.contentRows) {
					if (!row.inUse) continue;
					var childLabel = row.label;
					children.Add(new WidgetInfo {
						Label = childLabel.label.text,
						Type = WidgetType.Button,
						Component = childLabel.button,
						GameObject = childLabel.gameObject,
						SpeechFunc = () => BuildStorageItemText(childLabel)
					});
				}
				if (children.Count > 0)
					widget.Children = children;
			}

			section.Items.Add(widget);
		}

		private static string BuildStorageItemText(DetailLabelWithButton item) {
			string text = item.label.text;
			if (!string.IsNullOrEmpty(item.label2.text))
				text = $"{text}, {item.label2.text}";
			if (!string.IsNullOrEmpty(item.label3.text))
				text = $"{text}, {item.label3.text}";
			return text;
		}

		// ========================================
		// VITALS
		// ========================================

		private static void AddVitals(SimpleInfoScreen panel, List<DetailSection> sections) {
			MinionVitalsPanel vitals;
			try {
				vitals = Traverse.Create(panel)
					.Field<MinionVitalsPanel>("vitalsPanel").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"StatusTab: vitalsPanel read failed: {ex.Message}");
				return;
			}
			if (vitals == null || !vitals.gameObject.activeSelf) return;

			var section = new DetailSection();
			section.Header = vitals.HeaderLabel != null
				&& !string.IsNullOrEmpty(vitals.HeaderLabel.text)
				? vitals.HeaderLabel.text
				: (string)STRINGS.UI.DETAILTABS.SIMPLEINFO.GROUPNAME_CONDITION;

			AddVitalLines(vitals.amountsLines, section);
			AddVitalLines(vitals.attributesLines, section);

			foreach (var line in vitals.checkboxLines) {
				if (!line.go.activeSelf) continue;
				var captured = line;
				var checkRef = captured.go.GetComponent<HierarchyReferences>();
				section.Items.Add(new WidgetInfo {
					Label = captured.locText.text,
					Type = WidgetType.Label,
					GameObject = captured.go,
					SpeechFunc = () => {
						bool met = checkRef.GetReference("Check").gameObject.activeSelf;
						string prefix = met
							? (string)STRINGS.ONIACCESS.STATES.CONDITION_MET
							: (string)STRINGS.ONIACCESS.STATES.CONDITION_NOT_MET;
						return $"{prefix}, {captured.locText.text}";
					}
				});
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

	private static void AddVitalLines(
				IEnumerable<MinionVitalsPanel.AmountLine> lines, DetailSection section) {
			foreach (var line in lines) {
				if (!line.go.activeSelf) continue;
				var captured = line;
				section.Items.Add(new WidgetInfo {
					Label = captured.locText.text,
					Type = WidgetType.Label,
					GameObject = captured.go,
					SpeechFunc = () => captured.locText.text
				});
			}
		}

		private static void AddVitalLines(
				IEnumerable<MinionVitalsPanel.AttributeLine> lines, DetailSection section) {
			foreach (var line in lines) {
				if (!line.go.activeSelf) continue;
				var captured = line;
				section.Items.Add(new WidgetInfo {
					Label = captured.locText.text,
					Type = WidgetType.Label,
					GameObject = captured.go,
					SpeechFunc = () => captured.locText.text
				});
			}
		}

		// ========================================
		// COLLAPSIBLE SECTION (generic)
		// ========================================

		private static void AddCollapsibleSection(
				SimpleInfoScreen panel, string fieldName, List<DetailSection> sections) {
			CollapsibleDetailContentPanel gameSection;
			try {
				gameSection = Traverse.Create(panel)
					.Field<CollapsibleDetailContentPanel>(fieldName).Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"StatusTab: field '{fieldName}' read failed: {ex.Message}");
				return;
			}
			if (gameSection == null || !gameSection.gameObject.activeSelf) return;

			var section = BuildCollapsibleSection(gameSection);
			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static DetailSection BuildCollapsibleSection(
				CollapsibleDetailContentPanel gameSection) {
			var section = new DetailSection();

			var headerLabel = gameSection.HeaderLabel;
			if (headerLabel != null && !string.IsNullOrEmpty(headerLabel.text))
				section.Header = headerLabel.text;

			var content = gameSection.Content;
			if (content == null) return section;

			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;

				var withButton = child.GetComponent<DetailLabelWithButton>();
				if (withButton != null) {
					var captured = withButton;
					section.Items.Add(new WidgetInfo {
						Label = captured.label.text,
						Type = WidgetType.Button,
						Component = captured.button,
						GameObject = child.gameObject,
						SpeechFunc = () => BuildStorageItemText(captured)
					});
					continue;
				}

				var detailLabel = child.GetComponent<DetailLabel>();
				if (detailLabel != null) {
					var captured = detailLabel;
					section.Items.Add(new WidgetInfo {
						Label = captured.label.text,
						Type = WidgetType.Label,
						GameObject = child.gameObject,
						SpeechFunc = () => captured.label.text
					});
				}
			}

			return section;
		}

		// ========================================
		// DESCRIPTOR SECTIONS (effects, requirements)
		// ========================================

		private static void AddDescriptorSection(
				SimpleInfoScreen panel, string panelFieldName,
				string contentFieldName, List<DetailSection> sections) {
			CollapsibleDetailContentPanel wrapper;
			try {
				wrapper = Traverse.Create(panel)
					.Field<CollapsibleDetailContentPanel>(panelFieldName).Value;
			} catch (System.Exception ex) {
				Util.Log.Warn(
					$"StatusTab: field '{panelFieldName}' read failed: {ex.Message}");
				return;
			}
			if (wrapper == null || !wrapper.gameObject.activeSelf) return;

			DescriptorPanel descriptorPanel;
			try {
				descriptorPanel = Traverse.Create(panel)
					.Field<DescriptorPanel>(contentFieldName).Value;
			} catch (System.Exception ex) {
				Util.Log.Warn(
					$"StatusTab: field '{contentFieldName}' read failed: {ex.Message}");
				return;
			}
			if (descriptorPanel == null || !descriptorPanel.gameObject.activeSelf) return;

			var section = new DetailSection();
			var headerLabel = wrapper.HeaderLabel;
			if (headerLabel != null && !string.IsNullOrEmpty(headerLabel.text))
				section.Header = headerLabel.text;

			for (int i = 0; i < descriptorPanel.transform.childCount; i++) {
				var child = descriptorPanel.transform.GetChild(i);
				if (!child.gameObject.activeSelf) continue;

				var locText = child.GetComponent<LocText>();
				if (locText == null) continue;

				var captured = locText;
				section.Items.Add(new WidgetInfo {
					Label = captured.text,
					Type = WidgetType.Label,
					GameObject = child.gameObject,
					SpeechFunc = () => captured.text
				});
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		// ========================================
		// PROCESS CONDITIONS
		// ========================================

		private static void AddProcessConditions(
				SimpleInfoScreen panel, List<DetailSection> sections) {
			CollapsibleDetailContentPanel container;
			try {
				container = Traverse.Create(panel)
					.Field<CollapsibleDetailContentPanel>("processConditionContainer").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn(
					$"StatusTab: processConditionContainer read failed: {ex.Message}");
				return;
			}
			if (container == null || !container.gameObject.activeSelf) return;

			var content = container.Content;
			if (content == null) return;

			DetailSection currentSection = null;

			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;

				var refs = child.GetComponent<HierarchyReferences>();
				if (refs == null) continue;

				// Headers have a "Label" reference and are the processConditionHeader prefab.
				// Condition rows also have "Label". Distinguish by checking for "Box" reference
				// (condition rows have a checkbox Box, headers don't).
				if (!refs.HasReference("Box")) {
					// Section header
					var headerText = refs.GetReference<LocText>("Label");
					if (headerText == null) continue;

					if (currentSection != null && currentSection.Items.Count > 0)
						sections.Add(currentSection);

					currentSection = new DetailSection();
					var capturedHeader = headerText;
					currentSection.Header = capturedHeader.text;
				} else {
					// Condition row
					if (currentSection == null)
						currentSection = new DetailSection();

					var labelText = refs.GetReference<LocText>("Label");
					if (labelText == null) continue;

					var captured = labelText;
					currentSection.Items.Add(new WidgetInfo {
						Label = captured.text,
						Type = WidgetType.Label,
						GameObject = child.gameObject,
						SpeechFunc = () => captured.text
					});
				}
			}

			if (currentSection != null && currentSection.Items.Count > 0)
				sections.Add(currentSection);
		}

		// ========================================
		// WORLD PANELS
		// ========================================

		private static void AddWorldPanel(
				SimpleInfoScreen panel, string fieldName, List<DetailSection> sections) {
			CollapsibleDetailContentPanel gameSection;
			try {
				gameSection = Traverse.Create(panel)
					.Field<CollapsibleDetailContentPanel>(fieldName).Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"StatusTab: field '{fieldName}' read failed: {ex.Message}");
				return;
			}
			if (gameSection == null || !gameSection.gameObject.activeSelf) return;

			var section = new DetailSection();
			var headerLabel = gameSection.HeaderLabel;
			if (headerLabel != null && !string.IsNullOrEmpty(headerLabel.text))
				section.Header = headerLabel.text;

			var content = gameSection.Content;
			if (content == null) return;

			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;

				var refs = child.GetComponent<HierarchyReferences>();
				if (refs != null) {
					AddWorldRow(refs, child.gameObject, section);
					continue;
				}

				// Fall back to plain DetailLabel (some world panels may use SetLabel)
				var detailLabel = child.GetComponent<DetailLabel>();
				if (detailLabel != null) {
					var captured = detailLabel;
					section.Items.Add(new WidgetInfo {
						Label = captured.label.text,
						Type = WidgetType.Label,
						GameObject = child.gameObject,
						SpeechFunc = () => captured.label.text
					});
				}
			}

			if (section.Items.Count > 0)
				sections.Add(section);
		}

		private static void AddWorldRow(
				HierarchyReferences refs, GameObject go, DetailSection section) {
			LocText nameLabel = refs.HasReference("NameLabel")
				? refs.GetReference<LocText>("NameLabel") : null;
			LocText valueLabel = refs.HasReference("ValueLabel")
				? refs.GetReference<LocText>("ValueLabel") : null;
			LocText descLabel = refs.HasReference("DescriptionLabel")
				? refs.GetReference<LocText>("DescriptionLabel") : null;

			if (nameLabel == null) return;

			var capturedName = nameLabel;
			var capturedValue = valueLabel;
			var capturedDesc = descLabel;

			section.Items.Add(new WidgetInfo {
				Label = capturedName.text,
				Type = WidgetType.Label,
				GameObject = go,
				SpeechFunc = () => {
					string text = capturedName.text;
					if (capturedValue != null
							&& capturedValue.gameObject.activeSelf
							&& !string.IsNullOrEmpty(capturedValue.text))
						text = $"{text}, {capturedValue.text}";
					if (capturedDesc != null
							&& capturedDesc.gameObject.activeSelf
							&& !string.IsNullOrEmpty(capturedDesc.text))
						text = $"{text}, {capturedDesc.text}";
					return text;
				}
			});
		}

		// ========================================
		// PANEL LOOKUP
		// ========================================

		private static SimpleInfoScreen FindPanel() {
			var ds = DetailsScreen.Instance;
			if (ds == null) return null;

			var tabHeader = Traverse.Create(ds)
				.Field<DetailTabHeader>("tabHeader").Value;
			if (tabHeader == null) return null;

			var tabPanels = Traverse.Create(tabHeader)
				.Field<Dictionary<string, TargetPanel>>("tabPanels").Value;
			if (tabPanels == null || !tabPanels.TryGetValue("SIMPLEINFO", out var panel))
				return null;

			return panel as SimpleInfoScreen;
		}
	}
}
