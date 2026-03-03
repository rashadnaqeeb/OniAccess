using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Handlers.Resources {
	/// <summary>
	/// Live data queries for the resource browser. All data is read fresh
	/// from game singletons on every call — nothing is cached.
	///
	/// Data comes from DiscoveredResources.Instance and WorldInventory,
	/// not from ResourceCategoryScreen (which is only alive when the
	/// sidebar panel is visible).
	/// </summary>
	internal static class ResourceHelper {

		internal struct CategoryEntry {
			internal Tag Tag;
		}

		/// <summary>
		/// Returns discovered categories sorted alphabetically by name.
		/// Only includes categories with at least one discovered resource.
		/// Uses the same three tag sets as AllResourcesScreen:
		/// MaterialCategories, CalorieCategories, UnitCategories.
		///
		/// Resources can be discovered under multiple categories (e.g. food
		/// under both Edible and Compostable). We assign each resource to
		/// its primary category only — the first match in priority order:
		/// CalorieCategories, UnitCategories, MaterialCategories.
		/// </summary>
		internal static List<CategoryEntry> GetCategories() {
			var result = new List<CategoryEntry>();
			if (DiscoveredResources.Instance == null) return result;

			var seen = new HashSet<Tag>();
			AddDiscoveredFrom(GameTags.CalorieCategories, result, seen);
			AddDiscoveredFrom(GameTags.UnitCategories, result, seen);
			AddDiscoveredFrom(GameTags.MaterialCategories, result, seen);

			result.Sort((a, b) =>
				a.Tag.ProperNameStripLink().CompareTo(
					b.Tag.ProperNameStripLink()));
			return result;
		}

		private static void AddDiscoveredFrom(
			TagSet tagSet, List<CategoryEntry> result, HashSet<Tag> seen)
		{
			foreach (Tag tag in tagSet) {
				var resources = DiscoveredResources.Instance
					.GetDiscoveredResourcesFromTag(tag);
				bool hasUnseen = false;
				foreach (var r in resources) {
					if (seen.Add(r))
						hasUnseen = true;
				}
				if (hasUnseen)
					result.Add(new CategoryEntry { Tag = tag });
			}
		}

		/// <summary>
		/// Returns discovered resources within a category, sorted alphabetically.
		/// Excludes resources that belong to a higher-priority category
		/// (calorie > unit > material) to prevent duplicates across categories.
		/// Also deduplicates by display name within a category.
		/// </summary>
		internal static List<Tag> GetResources(Tag categoryTag) {
			if (DiscoveredResources.Instance == null) return new List<Tag>();
			var discovered = DiscoveredResources.Instance
				.GetDiscoveredResourcesFromTag(categoryTag);

			var measure = GetMeasure(categoryTag);
			var result = new List<Tag>();
			var seenNames = new HashSet<string>();
			foreach (var tag in discovered) {
				if (GetPrimaryMeasure(tag) != measure) continue;
				string name = tag.ProperNameStripLink();
				if (seenNames.Add(name))
					result.Add(tag);
			}

			result.Sort((a, b) =>
				a.ProperNameStripLink().CompareTo(b.ProperNameStripLink()));
			return result;
		}

		/// <summary>
		/// Returns the primary measure unit for a resource tag by checking
		/// category sets in priority order: calorie, unit, mass.
		/// </summary>
		private static GameUtil.MeasureUnit GetPrimaryMeasure(Tag resourceTag) {
			if (DiscoveredResources.Instance == null) return GameUtil.MeasureUnit.mass;
			foreach (Tag cat in GameTags.CalorieCategories) {
				if (DiscoveredResources.Instance
					.GetDiscoveredResourcesFromTag(cat).Contains(resourceTag))
					return GameUtil.MeasureUnit.kcal;
			}
			foreach (Tag cat in GameTags.UnitCategories) {
				if (DiscoveredResources.Instance
					.GetDiscoveredResourcesFromTag(cat).Contains(resourceTag))
					return GameUtil.MeasureUnit.quantity;
			}
			return GameUtil.MeasureUnit.mass;
		}

		/// <summary>
		/// Builds a speech label for a category at level 0.
		/// Format: "{name}: {total}[, rising/falling]"
		/// </summary>
		internal static string BuildCategoryLabel(Tag categoryTag) {
			string name = categoryTag.ProperNameStripLink();
			var measure = GetMeasure(categoryTag);
			var worldInventory = ClusterManager.Instance.activeWorld.worldInventory;

			float total;
			if (measure == GameUtil.MeasureUnit.kcal) {
				total = WorldResourceAmountTracker<RationTracker>.Get()
					.CountAmount(null, worldInventory);
			} else {
				total = 0f;
				var resources = GetResources(categoryTag);
				foreach (var tag in resources)
					total += worldInventory.GetTotalAmount(tag, false);
			}

			string amount = FormatAmount(total, measure);
			string label = name + ": " + amount;

			string trend = GetTrendWord(categoryTag);
			if (trend != null)
				label += ", " + trend;

			return label;
		}

		/// <summary>
		/// Builds a speech label for a resource at level 1.
		///
		/// For mass/quantity: shows total amount (what exists), then reserved
		/// if any. Uses GetTotalAmount (not GetAmount which clamps to 0).
		/// Format: "{name}: {total}[, {reserved} reserved][, trend]"
		///
		/// For kcal: shows calories from RationTracker. Reserved is in mass
		/// units (mixed with calories is confusing), so it's omitted.
		/// Format: "{name}: {calories}[, trend]"
		/// </summary>
		internal static string BuildResourceLabel(Tag resourceTag, GameUtil.MeasureUnit measure) {
			string name = resourceTag.ProperNameStripLink();
			var worldInventory = ClusterManager.Instance.activeWorld.worldInventory;

			float total;
			float reserved;
			if (measure == GameUtil.MeasureUnit.kcal) {
				var foodInfo = EdiblesManager.GetFoodInfo(resourceTag.Name);
				float calsPerUnit = foodInfo != null ? foodInfo.CaloriesPerUnit : 1f;
				total = worldInventory.GetTotalAmount(resourceTag, false) * calsPerUnit;
				reserved = MaterialNeeds.GetAmount(resourceTag,
					ClusterManager.Instance.activeWorld.id, false) * calsPerUnit;
			} else {
				total = worldInventory.GetTotalAmount(resourceTag, false);
				reserved = MaterialNeeds.GetAmount(resourceTag,
					ClusterManager.Instance.activeWorld.id, false);
			}

			string label = name + ": " + FormatAmount(total, measure);
			if (reserved > 0f) {
				label += ", " + FormatAmount(reserved, measure) + " " +
					(string)STRINGS.ONIACCESS.RESOURCES.RESERVED;
				float available = total - reserved;
				string word = available < 0f
					? (string)STRINGS.ONIACCESS.RESOURCES.OVERDRAWN
					: (string)STRINGS.ONIACCESS.RESOURCES.AVAILABLE;
				label += ", " + FormatAmount(available, measure) + " " + word;
			}

			string trend = GetTrendWord(resourceTag);
			if (trend != null)
				label += ", " + trend;

			return label;
		}

		/// <summary>
		/// Format a float amount according to measure unit.
		/// </summary>
		internal static string FormatAmount(float amount, GameUtil.MeasureUnit measure) {
			switch (measure) {
				case GameUtil.MeasureUnit.mass:
					return GameUtil.GetFormattedMass(amount);
				case GameUtil.MeasureUnit.kcal:
					return GameUtil.GetFormattedCalories(amount);
				case GameUtil.MeasureUnit.quantity:
					return GameUtil.GetFormattedUnits(amount);
				default:
					return amount.ToString();
			}
		}

		/// <summary>
		/// Returns "rising", "falling", or null (stable/insufficient data).
		///
		/// Avoids Tracker.GetDelta which has a -1 sentinel that produces
		/// bogus positive deltas when the tracker lacks 150s of history.
		/// Instead compares recent average (last 30s) to longer-term
		/// average (last 150s). A meaningful divergence indicates a trend.
		/// </summary>
		internal static string GetTrendWord(Tag tag) {
			var tracker = TrackerTool.Instance.GetResourceStatistic(
				ClusterManager.Instance.activeWorldId, tag);
			if (tracker == null) return null;

			float recent = tracker.GetAverageValue(30f);
			float longer = tracker.GetAverageValue(150f);
			float diff = recent - longer;

			// Ignore differences smaller than 1% of the longer-term average,
			// with a floor of 0.5 to filter noise on small amounts.
			float threshold = Mathf.Max(0.5f, Mathf.Abs(longer) * 0.01f);
			if (diff > threshold) return (string)STRINGS.ONIACCESS.RESOURCES.RISING;
			if (diff < -threshold) return (string)STRINGS.ONIACCESS.RESOURCES.FALLING;
			return null;
		}

		/// <summary>
		/// Returns the measure unit for a category tag using the same
		/// tag sets the game uses (GameTags.DisplayAsCalories, DisplayAsUnits).
		/// </summary>
		internal static GameUtil.MeasureUnit GetMeasure(Tag categoryTag) {
			if (GameTags.DisplayAsCalories.Contains(categoryTag))
				return GameUtil.MeasureUnit.kcal;
			if (GameTags.DisplayAsUnits.Contains(categoryTag))
				return GameUtil.MeasureUnit.quantity;
			return GameUtil.MeasureUnit.mass;
		}

		/// <summary>
		/// Builds a speech block for reading pinned resources.
		/// Returns NO_PINNED message if no resources are pinned.
		/// </summary>
		internal static string BuildPinnedSpeech() {
			var worldInventory = ClusterManager.Instance.activeWorld.worldInventory;
			if (worldInventory.pinnedResources.Count == 0)
				return (string)STRINGS.ONIACCESS.RESOURCES.NO_PINNED;

			var sb = new System.Text.StringBuilder();
			foreach (var tag in worldInventory.pinnedResources) {
				if (sb.Length > 0) sb.Append(". ");
				string name = tag.ProperNameStripLink();
				var measure = GetMeasureForResource(tag);
				float amount;
				if (measure == GameUtil.MeasureUnit.kcal) {
					amount = WorldResourceAmountTracker<RationTracker>.Get()
						.CountAmountForItemWithID(tag.Name, worldInventory);
				} else {
					amount = worldInventory.GetTotalAmount(tag, false);
				}
				sb.Append(name).Append(": ").Append(FormatAmount(amount, measure));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Determine the measure unit for a specific resource tag
		/// by finding which category it belongs to.
		/// </summary>
		internal static GameUtil.MeasureUnit GetMeasureForResource(Tag resourceTag) {
			return GetPrimaryMeasure(resourceTag);
		}

		/// <summary>
		/// Returns the current pinned resources sorted alphabetically.
		/// </summary>
		internal static List<Tag> GetPinnedResources() {
			var pinned = ClusterManager.Instance.activeWorld.worldInventory.pinnedResources;
			var result = new List<Tag>(pinned);
			result.Sort((a, b) =>
				a.ProperNameStripLink().CompareTo(b.ProperNameStripLink()));
			return result;
		}

		/// <summary>
		/// Collects pickupable instances for a resource tag, deduplicated by cell.
		/// Excludes StoredPrivate items. Returns entries sorted by cell.
		/// </summary>
		internal static List<InstanceEntry> GetInstances(Tag resourceTag) {
			var result = new List<InstanceEntry>();
			var worldInventory = ClusterManager.Instance.activeWorld.worldInventory;
			var pickupables = worldInventory.GetPickupables(resourceTag);
			if (pickupables == null) return result;

			var cellMap = new Dictionary<int, InstanceEntry>();
			int worldId = ClusterManager.Instance.activeWorld.id;

			foreach (var p in pickupables) {
				if (p == null) continue;
				if (p.KPrefabID.HasTag(GameTags.StoredPrivate)) continue;
				if (p.GetMyWorldId() != worldId) continue;

				int cell = Grid.PosToCell(p.transform.GetPosition());
				if (!Grid.IsValidCell(cell)) continue;

				if (!cellMap.TryGetValue(cell, out var entry)) {
					GameObject building = null;
					if (p.storage != null)
						building = p.storage.gameObject;
					entry = new InstanceEntry(cell, 0f, building);
					cellMap[cell] = entry;
				}
				entry.Amount += p.TotalAmount;
			}

			result.AddRange(cellMap.Values);
			result.Sort((a, b) => b.Amount.CompareTo(a.Amount));
			return result;
		}

		internal class InstanceEntry {
			internal int Cell;
			internal float Amount;
			internal GameObject Building;

			internal InstanceEntry(int cell, float amount, GameObject building) {
				Cell = cell;
				Amount = amount;
				Building = building;
			}
		}
	}
}
