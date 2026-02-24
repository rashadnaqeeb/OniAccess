using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the AdditionalDetailsPanel (Properties tab) into structured sections.
	/// Eight CollapsibleDetailContentPanel sections, each with a header and DetailLabel children.
	/// All widgets use SpeechFunc for live text since the game updates labels every frame.
	/// </summary>
	class PropertiesTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.DETAILS.NAME;
		public int StartLevel => 0;
		public string GameTabId => "DETAILS";

		public bool IsAvailable(GameObject target) => true;

		public void OnTabSelected() { }

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

				var section = CollapsiblePanelReader.BuildSection(gameSection);
				if (section.Items.Count > 0)
					sections.Add(section);
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
