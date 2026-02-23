using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the AdditionalDetailsPanel (Properties tab) into a flat widget list.
	/// Eight CollapsibleDetailContentPanel sections, each with a header and DetailLabel children.
	/// All widgets use SpeechFunc for live text since the game updates labels every frame.
	/// </summary>
	class PropertiesTab : IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.DETAILS.NAME;

		public bool IsAvailable(GameObject target) => true;

		private static readonly string[] PanelFieldNames = {
			"detailsPanel", "immuneSystemPanel", "diseaseSourcePanel",
			"currentGermsPanel", "overviewPanel", "generatorsPanel",
			"consumersPanel", "batteriesPanel"
		};

		public void Populate(GameObject target, List<WidgetInfo> widgets) {
			var panel = FindPanel();
			if (panel == null) {
				Util.Log.Warn("PropertiesTab.Populate: AdditionalDetailsPanel not found");
				return;
			}

			// Ensure the panel has been populated for this target.
			// Only call SetTarget when the panel is active to avoid interfering
			// with the game's tab lifecycle (inactive panels get SetTarget(null)
			// from DetailTabHeader.ChangeTab).
			if (panel.gameObject.activeSelf)
				panel.SetTarget(target);

			foreach (var fieldName in PanelFieldNames) {
				CollapsibleDetailContentPanel section;
				try {
					section = Traverse.Create(panel)
						.Field<CollapsibleDetailContentPanel>(fieldName).Value;
				} catch (System.Exception ex) {
					Util.Log.Warn($"PropertiesTab: field '{fieldName}' read failed: {ex.Message}");
					continue;
				}

				if (section == null || !section.gameObject.activeSelf) continue;

				AddSectionWidgets(section, widgets);
			}
		}

		private static void AddSectionWidgets(
				CollapsibleDetailContentPanel section, List<WidgetInfo> widgets) {
			var headerLabel = section.HeaderLabel;
			if (headerLabel != null && !string.IsNullOrEmpty(headerLabel.text)) {
				widgets.Add(new WidgetInfo {
					Label = headerLabel.text,
					Type = WidgetType.Label,
					GameObject = section.gameObject,
					SpeechFunc = () => headerLabel.text
				});
			}

			var content = section.Content;
			if (content == null) return;

			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;

				var detailLabel = child.GetComponent<DetailLabel>();
				if (detailLabel == null) continue;

				var captured = detailLabel;
				widgets.Add(new WidgetInfo {
					Label = captured.label.text,
					Type = WidgetType.Label,
					GameObject = child.gameObject,
					SpeechFunc = () => captured.label.text
				});
			}
		}

		private static AdditionalDetailsPanel FindPanel() {
			var ds = DetailsScreen.Instance;
			if (ds == null) return null;

			var tabHeader = Traverse.Create(ds)
				.Field<DetailTabHeader>("tabHeader").Value;
			if (tabHeader == null) return null;

			var tabPanels = Traverse.Create(tabHeader)
				.Field<Dictionary<string, TargetPanel>>("tabPanels").Value;
			if (tabPanels == null || !tabPanels.TryGetValue("DETAILS", out var panel))
				return null;

			return panel as AdditionalDetailsPanel;
		}

	}
}
