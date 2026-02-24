using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Reads the BuildingChoresPanel (Errands tab) into structured sections.
	/// Each chore is a section (level 0) with dupe rows as items (level 1).
	/// Dupe rows are Button widgets — Enter pans the camera to the dupe.
	///
	/// Section headers are enriched beyond the game's generic ChoreLabel text
	/// by reading the actual Chore objects from GlobalChoreProvider. For fetch
	/// chores, the header includes the fetch target name (e.g., "Cook Supply:
	/// Gristle Berry" instead of just "Cook Supply").
	/// </summary>
	class ChoresTab: IDetailTab {
		public string DisplayName => (string)STRINGS.UI.DETAILTABS.BUILDING_CHORES.NAME;
		public int StartLevel => 0;
		public string GameTabId => "BUILDINGCHORES";

		public bool IsAvailable(GameObject target) => true;

		public void OnTabSelected() { }

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

			// Collect the chores for this building in the same order the game
			// processes them in RefreshDetails: choreWorldMap first, then fetchMap.
			// This order matches the active entries in EntriesContainer.
			var chores = CollectChores(target);

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

			int choreIndex = 0;
			for (int i = 0; i < container.childCount; i++) {
				var choreChild = container.GetChild(i);
				if (!choreChild.gameObject.activeSelf) continue;

				var entry = choreChild.GetComponent<HierarchyReferences>();
				if (entry == null) continue;

				var section = new DetailSection();

				// Use enriched header from the Chore object when available,
				// fall back to the game's ChoreLabel text.
				if (choreIndex < chores.Count) {
					section.Header = GetChoreHeader(chores[choreIndex]);
				} else {
					var choreLabel = entry.GetReference<LocText>("ChoreLabel");
					section.Header = choreLabel != null ? choreLabel.text : "";
				}
				choreIndex++;

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

		/// <summary>
		/// Build a descriptive header for a chore. For FetchChore types with a
		/// known fetch target, appends the target name (e.g., "Cook Supply:
		/// Gristle Berry"). For operation chores on buildings with a
		/// ComplexFabricator, appends the recipe name (e.g., "Cook: Stuffed
		/// Berry"). Otherwise uses the game's standard chore name.
		/// </summary>
		private static string GetChoreHeader(Chore chore) {
			string baseName = GameUtil.GetChoreName(chore, null);
			var fetchChore = chore as FetchChore;
			if (fetchChore != null) {
				// fetchTarget is only set when a dupe picks up the item.
				// For pending chores, use tagsFirst — the requested material tag.
				var fetchTarget = fetchChore.fetchTarget;
				if (fetchTarget != null) {
					string targetName = fetchTarget.GetProperName();
					if (!string.IsNullOrEmpty(targetName))
						return $"{baseName}: {targetName}";
				}
				string tagName = fetchChore.tagsFirst.ProperName();
				if (!string.IsNullOrEmpty(tagName))
					return $"{baseName}: {tagName}";
			} else {
				string recipeName = GetRecipeName(chore.gameObject);
				if (recipeName != null)
					return $"{baseName}: {recipeName}";
			}
			return baseName;
		}

		/// <summary>
		/// If the building has a ComplexFabricator, return the name of the
		/// current or next recipe. Returns null if no fabricator or no order.
		/// </summary>
		private static string GetRecipeName(GameObject building) {
			var fabricator = building.GetComponent<ComplexFabricator>();
			if (fabricator == null) return null;
			var recipe = fabricator.CurrentWorkingOrder ?? fabricator.NextOrder;
			if (recipe == null) return null;
			return recipe.GetUIName(false);
		}

		/// <summary>
		/// Collect the Chore objects for a building in the same order the game's
		/// BuildingChoresPanel.RefreshDetails processes them: first from
		/// choreWorldMap, then from fetchMap.
		/// </summary>
		private static List<Chore> CollectChores(GameObject target) {
			var result = new List<Chore>();
			int worldId = target.GetMyParentWorldId();

			List<Chore> worldChores = null;
			GlobalChoreProvider.Instance.choreWorldMap.TryGetValue(worldId, out worldChores);
			if (worldChores != null) {
				for (int i = 0; i < worldChores.Count; i++) {
					var chore = worldChores[i];
					if (!chore.isNull && chore.gameObject == target)
						result.Add(chore);
				}
			}

			List<FetchChore> fetchChores = null;
			GlobalChoreProvider.Instance.fetchMap.TryGetValue(worldId, out fetchChores);
			if (fetchChores != null) {
				for (int i = 0; i < fetchChores.Count; i++) {
					var fc = fetchChores[i];
					if (!fc.isNull && fc.gameObject == target)
						result.Add(fc);
				}
			}

			return result;
		}

		private static void AddNoErrandsSection(List<DetailSection> sections) {
			var section = new DetailSection();
			section.Header = (string)STRINGS.ONIACCESS.DETAILS.NO_ERRANDS;
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
