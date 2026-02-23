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

		public struct SubcategoryGroup {
			public string Name;
			public List<BuildingEntry> Buildings;
		}

		public struct CategoryGroup {
			public HashedString Category;
			public string DisplayName;
			public List<SubcategoryGroup> Subcategories;
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
		/// Returns buildings in a category in game-definition order.
		/// Unresearched buildings are hidden, matching the sighted UI.
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
				if (state == PlanScreen.RequirementsState.Tech) continue;

				string label = BuildLabel(def, state);
				result.Add(new BuildingEntry { Def = def, State = state, Label = label });
			}

			return result;
		}

		/// <summary>
		/// Returns buildings grouped by subcategory. Each group has the
		/// subcategory display name from STRINGS.UI.NEWBUILDCATEGORIES and
		/// a list of visible buildings. Same filtering as GetVisibleBuildings.
		/// </summary>
		public static List<SubcategoryGroup> GetGroupedBuildings(HashedString category) {
			var result = new List<SubcategoryGroup>();
			PlanScreen.PlanInfo? found = null;
			foreach (var planInfo in TUNING.BUILDINGS.PLANORDER) {
				if (planInfo.category == category) {
					found = planInfo;
					break;
				}
			}
			if (found == null) return result;

			var planInfoVal = found.Value;
			// Preserve game ordering: iterate buildingAndSubcategoryData in order,
			// group buildings by their subcategory value.
			var groupMap = new Dictionary<string, SubcategoryGroup>();
			foreach (var kv in planInfoVal.buildingAndSubcategoryData) {
				var def = Assets.GetBuildingDef(kv.Key);
				if (def == null) continue;
				if (!def.IsAvailable() || !def.ShouldShowInBuildMenu()
					|| !Game.IsCorrectDlcActiveForCurrentSave(def)) continue;

				var state = PlanScreen.Instance.GetBuildableState(def);
				if (state == PlanScreen.RequirementsState.Invalid) continue;
				if (state == PlanScreen.RequirementsState.Tech) continue;

				string subcatKey = kv.Value;
				if (!groupMap.TryGetValue(subcatKey, out var group)) {
					group = new SubcategoryGroup {
						Name = GetSubcategoryDisplayName(subcatKey),
						Buildings = new List<BuildingEntry>()
					};
					groupMap[subcatKey] = group;
					result.Add(group);
				}
				string label = BuildLabel(def, state);
				group.Buildings.Add(new BuildingEntry { Def = def, State = state, Label = label });
			}

			return result;
		}

		/// <summary>
		/// Returns the full 3-level build tree: categories → subcategories → buildings.
		/// Same visibility filters as GetVisibleCategories and GetGroupedBuildings.
		/// Categories with no visible buildings are excluded.
		/// </summary>
		public static List<CategoryGroup> GetFullBuildTree() {
			var result = new List<CategoryGroup>();
			foreach (var planInfo in TUNING.BUILDINGS.PLANORDER) {
				if (!Game.IsCorrectDlcActiveForCurrentSave(planInfo))
					continue;
				if (planInfo.hideIfNotResearched && !HasAnyResearchedBuilding(planInfo))
					continue;
				var subcategories = GetGroupedBuildings(planInfo.category);
				if (subcategories.Count == 0) continue;
				string name = GetCategoryDisplayName(planInfo.category);
				result.Add(new CategoryGroup {
					Category = planInfo.category,
					DisplayName = name,
					Subcategories = subcategories
				});
			}
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
				var panel = PlanScreen.Instance.ProductInfoScreen.materialSelectionPanel;
				var firstTag = panel.CurrentSelectedElement;
				if (firstTag == null) return null;
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
		/// Returns the building name and facing direction (no material).
		/// Used for the immediate interrupt announcement; material is queued
		/// separately so the selection panel has time to initialize.
		/// </summary>
		public static string BuildNameAnnouncement(BuildingDef def) {
			string name = def.Name;
			if (def.PermittedRotations != PermittedRotations.Unrotatable) {
				string dir = GetOrientationName(GetCurrentOrientation());
				return name + ", " + string.Format(
					(string)STRINGS.ONIACCESS.BUILD_MENU.FACING, dir);
			}
			return name;
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

		private static string GetSubcategoryDisplayName(string subcategoryKey) {
			StringEntry entry;
			if (Strings.TryGet("STRINGS.UI.NEWBUILDCATEGORIES." + subcategoryKey.ToUpper() + ".BUILDMENUTITLE", out entry))
				return entry.String;
			return subcategoryKey;
		}

		private static string GetCategoryDisplayName(HashedString category) {
			string text = HashCache.Get().Get(category).ToUpper();
			string name = Strings.Get("STRINGS.UI.BUILDCATEGORIES." + text + ".NAME");
			return STRINGS.UI.StripLinkFormatting(name);
		}

		private static string BuildLabel(BuildingDef def, PlanScreen.RequirementsState state) {
			string name = def.Name;
			string effect = STRINGS.UI.StripLinkFormatting(def.Effect);
			string label = string.IsNullOrEmpty(effect) ? name : name + ", " + effect;

			if (state == PlanScreen.RequirementsState.Complete)
				return label;
			if (state == PlanScreen.RequirementsState.Materials)
				return label + ", " + FormatMissingMaterials(def);
			string reason = PlanScreen.GetTooltipForRequirementsState(def, state);
			if (string.IsNullOrEmpty(reason))
				return label;
			return label + ", " + reason;
		}

		private static string FormatMissingMaterials(BuildingDef def) {
			var ingredients = def.CraftRecipe.Ingredients;
			bool allSameAmount = true;
			float sharedAmount = ingredients.Count > 0 ? ingredients[0].amount : 0f;
			for (int i = 1; i < ingredients.Count; i++) {
				if (ingredients[i].amount != sharedAmount) {
					allSameAmount = false;
					break;
				}
			}

			var sb = new System.Text.StringBuilder();
			sb.Append((string)STRINGS.UI.PRODUCTINFO_MISSINGRESOURCES_HOVER);
			sb.Append(": ");
			for (int i = 0; i < ingredients.Count; i++) {
				if (i > 0) sb.Append(", ");
				sb.Append(ingredients[i].tag.ProperName());
				if (!allSameAmount) {
					sb.Append(' ');
					sb.Append(GameUtil.GetFormattedMass(ingredients[i].amount));
				}
			}
			if (allSameAmount && ingredients.Count > 0) {
				sb.Append(' ');
				sb.Append(GameUtil.GetFormattedMass(sharedAmount));
			}
			return sb.ToString();
		}
	}
}
