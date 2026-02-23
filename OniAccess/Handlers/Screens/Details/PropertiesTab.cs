using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the AdditionalDetailsPanel (Properties tab) into structured sections.
	/// Eight CollapsibleDetailContentPanel sections, each with a header and DetailLabel children.
	/// All widgets use SpeechFunc for live text since the game updates labels every frame.
	/// </summary>
	class PropertiesTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.DETAILS.NAME;
		public string GameTabId => "DETAILS";

		public bool IsAvailable(GameObject target) => true;

		private static readonly string[] SectionFields = {
			"detailsPanel",
			"immuneSystemPanel",
			"diseaseSourcePanel",
			"currentGermsPanel",
			"overviewPanel",
			"generatorsPanel",
			"consumersPanel",
			"batteriesPanel",
		};

		public void Populate(GameObject target, List<DetailSection> sections) {
			var panel = FindPanel();
			if (panel == null) {
				Util.Log.Warn("PropertiesTab.Populate: AdditionalDetailsPanel not found");
				return;
			}

			// The handler switches the game's visual tab before calling Populate,
			// so the panel is already active with SetTarget called by the game.
			// Guard against edge cases where the panel hasn't been refreshed yet.
			if (panel.gameObject.activeSelf)
				panel.SetTarget(target);

			foreach (var fieldName in SectionFields) {
				CollapsibleDetailContentPanel gameSection;
				try {
					gameSection = Traverse.Create(panel)
						.Field<CollapsibleDetailContentPanel>(fieldName).Value;
				} catch (System.Exception ex) {
					Util.Log.Warn($"PropertiesTab: field '{fieldName}' read failed: {ex.Message}");
					continue;
				}

				if (gameSection == null || !gameSection.gameObject.activeSelf) continue;

				var section = BuildSection(gameSection);
				if (section.Items.Count > 0)
					sections.Add(section);
			}
		}

		private static DetailSection BuildSection(CollapsibleDetailContentPanel gameSection) {
			var section = new DetailSection();

			var headerLabel = gameSection.HeaderLabel;
			if (headerLabel != null && !string.IsNullOrEmpty(headerLabel.text))
				section.Header = headerLabel.text;

			var content = gameSection.Content;
			if (content == null) return section;

			// Collect active DetailLabel children. Indented entries (leading whitespace)
			// are children of the preceding non-indented header entry.
			var activeLabels = new List<DetailLabel>();
			for (int i = 0; i < content.childCount; i++) {
				var child = content.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var detailLabel = child.GetComponent<DetailLabel>();
				if (detailLabel != null)
					activeLabels.Add(detailLabel);
			}

			int idx = 0;
			while (idx < activeLabels.Count) {
				var header = activeLabels[idx];
				var children = new List<DetailLabel>();
				int next = idx + 1;
				while (next < activeLabels.Count) {
					string nextText = activeLabels[next].label.text;
					if (string.IsNullOrEmpty(nextText) || nextText[0] != ' ')
						break;
					children.Add(activeLabels[next]);
					next++;
				}

				if (children.Count == 0) {
					var captured = header;
					section.Items.Add(new WidgetInfo {
						Label = captured.label.text,
						Type = WidgetType.Label,
						GameObject = captured.gameObject,
						SpeechFunc = () => captured.label.text
					});
				} else {
					var capturedHeader = header;
					var capturedChildren = children.ToArray();
					section.Items.Add(new WidgetInfo {
						Label = capturedHeader.label.text,
						Type = WidgetType.Label,
						GameObject = capturedHeader.gameObject,
						SpeechFunc = () => {
							string text = capturedHeader.label.text;
							foreach (var child in capturedChildren) {
								string childText = child.label.text?.Trim();
								if (!string.IsNullOrEmpty(childText))
									text = $"{text} {childText}";
							}
							return text;
						}
					});
				}
				idx = next;
			}

			return section;
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
