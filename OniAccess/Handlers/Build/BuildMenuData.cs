using System.Collections.Generic;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Static helpers for querying PlanScreen categories, buildings,
	/// materials, and programmatically selecting buildings in the game.
	/// All methods re-query live game data on every call.
	/// </summary>
	public static class BuildMenuData {
		public struct CategoryEntry {
			public HashedString Category;
			public string DisplayName;
		}

		public struct BuildingEntry {
			public BuildingDef Def;
			public PlanScreen.RequirementsState State;
			public string Label;
		}

		/// <summary>
		/// Returns visible categories from TUNING.BUILDINGS.PLANORDER.
		/// Categories with hideIfNotResearched are hidden when no building
		/// in the category has been researched, matching the sighted UI.
		/// </summary>
		public static List<CategoryEntry> GetVisibleCategories() {
			var result = new List<CategoryEntry>();
			foreach (var planInfo in TUNING.BUILDINGS.PLANORDER) {
				if (!Game.IsCorrectDlcActiveForCurrentSave(planInfo))
					continue;
				if (planInfo.hideIfNotResearched && !HasAnyResearchedBuilding(planInfo))
					continue;
				string name = GetCategoryDisplayName(planInfo.category);
				result.Add(new CategoryEntry {
					Category = planInfo.category,
					DisplayName = name
				});
			}
			return result;
		}

		/// <summary>
		/// Returns buildings in a category. Complete (buildable) buildings
		/// are sorted to the front since a sighted player visually skips
		/// greyed-out icons but a blind player must arrow through the list.
		/// </summary>
		public static List<BuildingEntry> GetVisibleBuildings(HashedString category) {
			var result = new List<BuildingEntry>();
			PlanScreen.PlanInfo? found = null;
			foreach (var planInfo in TUNING.BUILDINGS.PLANORDER) {
				if (planInfo.category == category) {
					found = planInfo;
					break;
				}
			}
			if (found == null) return result;

			var planInfoVal = found.Value;
			foreach (var kv in planInfoVal.buildingAndSubcategoryData) {
				var def = Assets.GetBuildingDef(kv.Key);
				if (def == null) continue;
				if (!def.IsAvailable() || !def.ShouldShowInBuildMenu()
					|| !Game.IsCorrectDlcActiveForCurrentSave(def)) continue;

				var state = PlanScreen.Instance.GetBuildableState(def);
				if (state == PlanScreen.RequirementsState.Invalid) continue;

				string label = BuildLabel(def, state);
				result.Add(new BuildingEntry { Def = def, State = state, Label = label });
			}

			result.Sort((a, b) => {
				bool aComplete = a.State == PlanScreen.RequirementsState.Complete;
				bool bComplete = b.State == PlanScreen.RequirementsState.Complete;
				if (aComplete != bComplete) return aComplete ? -1 : 1;
				return 0;
			});
			return result;
		}

		/// <summary>
		/// Programmatically select a building in PlanScreen, triggering the
		/// full game chain: ProductInfoScreen configuration, material
		/// auto-selection, and build tool activation.
		/// </summary>
		public static bool SelectBuilding(BuildingDef def, HashedString category) {
			try {
				string categoryName = HashCache.Get().Get(category);
				PlanScreen.Instance.OpenCategoryByName(categoryName);
				if (!PlanScreen.Instance.activeCategoryBuildingToggles.TryGetValue(def, out var toggle)) {
					Util.Log.Warn($"BuildMenuData.SelectBuilding: no toggle for {def.PrefabID}");
					return false;
				}
				PlanScreen.Instance.OnSelectBuilding(toggle.gameObject, def);
				return true;
			} catch (System.Exception ex) {
				Util.Log.Error($"BuildMenuData.SelectBuilding: {ex}");
				return false;
			}
		}

		public static bool IsUtilityBuilding(BuildingDef def) {
			return def.isKAnimTile && def.isUtility;
		}

		public static string GetOrientationName(Orientation orientation) {
			switch (orientation) {
				case Orientation.Neutral: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_UP;
				case Orientation.R90: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_RIGHT;
				case Orientation.R180: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_DOWN;
				case Orientation.R270: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_LEFT;
				case Orientation.FlipH: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_LEFT;
				case Orientation.FlipV: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_DOWN;
				default: return (string)STRINGS.ONIACCESS.BUILD_MENU.ORIENT_UP;
			}
		}

		/// <summary>
		/// Builds a brief material summary for the building's auto-selected
		/// materials. Returns e.g. "copper, 25 kg".
		/// </summary>
		public static string GetMaterialSummary(BuildingDef def) {
			try {
				var elements = PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel.GetSelectedElementAsList;
				if (elements == null || elements.Count == 0) return null;
				var firstTag = elements[0];
				var element = ElementLoader.GetElement(firstTag);
				string name = element != null ? element.name : firstTag.ProperName();
				float mass = def.Mass != null && def.Mass.Length > 0 ? def.Mass[0] : 0f;
				return $"{name}, {GameUtil.GetFormattedMass(mass)}";
			} catch (System.Exception ex) {
				Util.Log.Warn($"BuildMenuData.GetMaterialSummary: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Returns the brief placement announcement:
		/// "name, material mass, facing dir" (or without dir if unrotatable).
		/// </summary>
		public static string BuildPlacementAnnouncement(BuildingDef def) {
			string name = def.Name;
			string material = GetMaterialSummary(def);
			if (material == null) material = "";

			if (def.PermittedRotations != PermittedRotations.Unrotatable) {
				var orientation = GetCurrentOrientation();
				string dir = GetOrientationName(orientation);
				return string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.PLACEMENT, name, material, dir);
			}
			return string.Format(
				(string)STRINGS.ONIACCESS.BUILD_MENU.PLACEMENT_NO_ORIENT, name, material);
		}

		/// <summary>
		/// Reads the current build tool orientation from the visualizer.
		/// </summary>
		public static Orientation GetCurrentOrientation() {
			if (BuildTool.Instance != null && BuildTool.Instance.visualizer != null) {
				var rot = BuildTool.Instance.visualizer.GetComponent<Rotatable>();
				if (rot != null) return rot.GetOrientation();
			}
			return Orientation.Neutral;
		}

		private static bool HasAnyResearchedBuilding(PlanScreen.PlanInfo planInfo) {
			foreach (var kv in planInfo.buildingAndSubcategoryData) {
				var def = Assets.GetBuildingDef(kv.Key);
				if (def == null) continue;
				if (!def.IsAvailable() || !def.ShouldShowInBuildMenu()
					|| !Game.IsCorrectDlcActiveForCurrentSave(def)) continue;
				var state = PlanScreen.Instance.GetBuildableState(def);
				if (state != PlanScreen.RequirementsState.Tech)
					return true;
			}
			return false;
		}

		private static string GetCategoryDisplayName(HashedString category) {
			string text = HashCache.Get().Get(category).ToUpper();
			string name = Strings.Get("STRINGS.UI.BUILDCATEGORIES." + text + ".NAME");
			return STRINGS.UI.StripLinkFormatting(name);
		}

		private static string BuildLabel(BuildingDef def, PlanScreen.RequirementsState state) {
			string name = def.Name;
			if (state == PlanScreen.RequirementsState.Complete)
				return name;
			string reason = PlanScreen.GetTooltipForRequirementsState(def, state);
			if (string.IsNullOrEmpty(reason))
				return name;
			return name + ", " + reason;
		}
	}
}
