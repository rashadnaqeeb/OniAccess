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

			// TargetPanel.SetTarget guards with selectedTarget != target, so
			// it's a no-op when the target hasn't changed. Call Refresh directly
			// to force the panel to repopulate its pooled chore entries.
			Traverse.Create(panel).Method("Refresh").GetValue();

			// The game's activeChoreEntries counter resets to 0 at the end of
			// every Refresh(), so we can't use it. Walk choreGroup's
			// EntriesContainer children and check activeSelf instead — the game
			// deactivates excess pooled entries before resetting the counter.
			HierarchyReferences choreGroup;
			try {
				choreGroup = Traverse.Create(panel)
					.Field<HierarchyReferences>("choreGroup").Value;
			} catch (System.Exception ex) {
				Util.Log.Warn($"ChoresTab: choreGroup read failed: {ex.Message}");
				return;
			}
			if (choreGroup == null) {
				AddNoErrandsSection(sections);
				return;
			}

			var container = choreGroup.GetReference<RectTransform>("EntriesContainer");
			if (container == null) {
				AddNoErrandsSection(sections);
				return;
			}

			for (int i = 0; i < container.childCount; i++) {
				var choreChild = container.GetChild(i);
				if (!choreChild.gameObject.activeSelf) continue;

				var entry = choreChild.GetComponent<HierarchyReferences>();
				if (entry == null) continue;

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
