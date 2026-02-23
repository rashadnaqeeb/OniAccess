using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the BuildingChoresPanel (Errands tab) into structured sections.
	/// Each chore is a section (level 0) with dupe rows as items (level 1).
	/// Dupe rows are Button widgets — Enter pans the camera to the dupe.
	/// </summary>
	class ChoresTab : IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME;
		public string GameTabId => "BUILDINGCHORES";

		public bool IsAvailable(GameObject target) => true;

		public void Populate(GameObject target, List<DetailSection> sections) {
			var panel = FindPanel();
			if (panel == null) {
				Util.Log.Warn("ChoresTab.Populate: BuildingChoresPanel not found");
				return;
			}

			List<HierarchyReferences> choreEntries;
			int activeChoreCount;

			try {
				choreEntries = Traverse.Create(panel)
					.Field<List<HierarchyReferences>>("choreEntries").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"ChoresTab: choreEntries read failed: {ex.Message}");
				return;
			}

			try {
				activeChoreCount = Traverse.Create(panel)
					.Field<int>("activeChoreEntries").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"ChoresTab: activeChoreEntries read failed: {ex.Message}");
				return;
			}

			if (choreEntries == null || activeChoreCount == 0) {
				AddNoErrandsSection(sections);
				return;
			}

			for (int i = 0; i < activeChoreCount && i < choreEntries.Count; i++) {
				var entry = choreEntries[i];
				if (!entry.gameObject.activeSelf) continue;

				var section = new DetailSection();

				var choreLabel = entry.GetReference<LocText>("ChoreLabel");
				var choreSubLabel = entry.GetReference<LocText>("ChoreSubLabel");
				string choreName = choreLabel != null ? choreLabel.text : "";
				string choreGroups = choreSubLabel != null ? choreSubLabel.text : "";
				section.Header = string.IsNullOrEmpty(choreGroups)
					? choreName
					: $"{choreName}, {choreGroups}";

				var dupeContainer = entry.GetReference<RectTransform>("DupeContainer");
				if (dupeContainer != null) {
					for (int j = 0; j < dupeContainer.childCount; j++) {
						var child = dupeContainer.GetChild(j);
						if (!child.gameObject.activeSelf) continue;
						var row = child.GetComponent<BuildingChoresPanelDupeRow>();
						if (row == null) continue;

						var capturedRow = row;
						section.Items.Add(new WidgetInfo {
							Label = capturedRow.label.text,
							Type = WidgetType.Button,
							Component = capturedRow.button,
							GameObject = capturedRow.gameObject,
							SpeechFunc = () => capturedRow.label.text
						});
					}
				}

				if (section.Items.Count > 0)
					sections.Add(section);
			}

			if (sections.Count == 0)
				AddNoErrandsSection(sections);
		}

		private static void AddNoErrandsSection(List<DetailSection> sections) {
			var section = new DetailSection();
			section.Items.Add(new WidgetInfo {
				Label = (string)STRINGS.ONIACCESS.DETAILS.NO_ERRANDS,
				Type = WidgetType.Label,
				SpeechFunc = () => (string)STRINGS.ONIACCESS.DETAILS.NO_ERRANDS
			});
			sections.Add(section);
		}

		private static BuildingChoresPanel FindPanel() {
			var ds = DetailsScreen.Instance;
			if (ds == null) return null;

			var tabHeader = Traverse.Create(ds)
				.Field<DetailTabHeader>("tabHeader").Value;
			if (tabHeader == null) return null;

			var tabPanels = Traverse.Create(tabHeader)
				.Field<Dictionary<string, TargetPanel>>("tabPanels").Value;
			if (tabPanels == null || !tabPanels.TryGetValue("BUILDINGCHORES", out var panel))
				return null;

			return panel as BuildingChoresPanel;
		}
	}
}
