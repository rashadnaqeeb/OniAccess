using System.Collections.Generic;

namespace OniAccess.Handlers.Resources {
	/// <summary>
	/// Live data queries for the resource browser. All data is read fresh
	/// from game singletons on every call — nothing is cached.
	/// </summary>
	internal static class ResourceHelper {

		internal struct CategoryEntry {
			internal Tag Tag;
			internal ResourceCategoryHeader Header;
		}

		/// <summary>
		/// Returns discovered categories sorted alphabetically by name.
		/// Only includes categories with at least one discovered resource.
		/// </summary>
		internal static List<CategoryEntry> GetCategories() {
			var result = new List<CategoryEntry>();
			if (ResourceCategoryScreen.Instance == null) return result;

			foreach (var kvp in ResourceCategoryScreen.Instance.DisplayedCategories) {
				if (kvp.Value.ResourcesDiscovered.Count > 0)
					result.Add(new CategoryEntry { Tag = kvp.Key, Header = kvp.Value });
			}

			result.Sort((a, b) =>
				a.Header.elements.LabelText.text.CompareTo(
					b.Header.elements.LabelText.text));
			return result;
		}

		/// <summary>
		/// Returns discovered resources within a category, sorted alphabetically.
		/// </summary>
		internal static List<Tag> GetResources(Tag categoryTag) {
			if (ResourceCategoryScreen.Instance == null) return new List<Tag>();
			if (!ResourceCategoryScreen.Instance.DisplayedCategories
				.TryGetValue(categoryTag, out var header))
				return new List<Tag>();

			var result = new List<Tag>(header.ResourcesDiscovered.Keys);
			result.Sort((a, b) =>
				a.ProperNameStripLink().CompareTo(b.ProperNameStripLink()));
			return result;
		}

		/// <summary>
		/// Builds a speech label for a category at level 0.
		/// Format: "{name}: {total available}[, rising/falling]"
		/// </summary>
		internal static string BuildCategoryLabel(Tag categoryTag, ResourceCategoryHeader header) {
			string name = header.elements.LabelText.text;
			string amount = header.elements.QuantityText.text;
			string label = name + ": " + amount;

			string trend = GetTrendWord(categoryTag);
			if (trend != null)
				label += ", " + trend;

			return label;
		}

		/// <summary>
		/// Builds a speech label for a resource at level 1.
		/// Format: "{name}: {available}[, {reserved} reserved][, rising/falling]"
		/// When available is negative (overdrawn): "available {amount}"
		/// </summary>
		internal static string BuildResourceLabel(Tag resourceTag, GameUtil.MeasureUnit measure) {
			string name = resourceTag.ProperNameStripLink();
			var worldInventory = ClusterManager.Instance.activeWorld.worldInventory;
			float available = worldInventory.GetAmount(resourceTag, false);
			float reserved = MaterialNeeds.GetAmount(resourceTag,
				ClusterManager.Instance.activeWorld.id, false);

			string amountStr = FormatAmount(available, measure);

			string label;
			if (available < 0f)
				label = name + ": " +
					(string)STRINGS.ONIACCESS.STATES.AVAILABLE + " " + amountStr;
			else
				label = name + ": " + amountStr;

			if (reserved > 0f) {
				string reservedStr = FormatAmount(reserved, measure);
				label += ", " + reservedStr + " " +
					(string)STRINGS.ONIACCESS.RESOURCES.RESERVED;
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
		/// Returns "rising", "falling", or null (stable).
		/// </summary>
		internal static string GetTrendWord(Tag tag) {
			var tracker = TrackerTool.Instance.GetResourceStatistic(
				ClusterManager.Instance.activeWorldId, tag);
			if (tracker == null) return null;
			float delta = tracker.GetDelta(150f);
			if (delta > 0f) return (string)STRINGS.ONIACCESS.RESOURCES.RISING;
			if (delta < 0f) return (string)STRINGS.ONIACCESS.RESOURCES.FALLING;
			return null;
		}

		/// <summary>
		/// Returns the measure unit for a category tag.
		/// </summary>
		internal static GameUtil.MeasureUnit GetMeasure(Tag categoryTag) {
			if (ResourceCategoryScreen.Instance != null
				&& ResourceCategoryScreen.Instance.DisplayedCategories
					.TryGetValue(categoryTag, out var header))
				return header.Measure;
			return GameUtil.MeasureUnit.mass;
		}

		/// <summary>
		/// Builds a speech block for reading pinned resources.
		/// Returns null if no resources are pinned.
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
				float available = worldInventory.GetAmount(tag, false);
				string amount = FormatAmount(available, measure);
				sb.Append(name).Append(": ").Append(amount);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Determine the measure unit for a specific resource tag
		/// by finding which category it belongs to.
		/// </summary>
		internal static GameUtil.MeasureUnit GetMeasureForResource(Tag resourceTag) {
			if (ResourceCategoryScreen.Instance == null) return GameUtil.MeasureUnit.mass;
			foreach (var kvp in ResourceCategoryScreen.Instance.DisplayedCategories) {
				if (kvp.Value.ResourcesDiscovered.ContainsKey(resourceTag))
					return kvp.Value.Measure;
			}
			return GameUtil.MeasureUnit.mass;
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
					UnityEngine.GameObject building = null;
					if (p.storage != null)
						building = p.storage.gameObject;
					entry = new InstanceEntry(cell, 0f, building);
					cellMap[cell] = entry;
				}
				entry.Amount += p.TotalAmount;
			}

			result.AddRange(cellMap.Values);
			result.Sort((a, b) => a.Cell.CompareTo(b.Cell));
			return result;
		}

		internal class InstanceEntry {
			internal int Cell;
			internal float Amount;
			internal UnityEngine.GameObject Building;

			internal InstanceEntry(int cell, float amount, UnityEngine.GameObject building) {
				Cell = cell;
				Amount = amount;
				Building = building;
			}
		}
	}
}
