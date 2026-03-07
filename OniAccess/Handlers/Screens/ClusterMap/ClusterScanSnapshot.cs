using System.Collections.Generic;

namespace OniAccess.Handlers.Screens.ClusterMap {
	public class ClusterScanItem {
		public string ItemName;
		public List<ClusterScanEntry> Instances;
	}

	public class ClusterScanCategory {
		public string Name;
		public List<ClusterScanItem> Items;
	}

	/// <summary>
	/// 3-level hierarchy: Category -> Item -> Instance.
	/// Built from a flat list of ClusterScanEntry objects.
	/// Categories follow ClusterMapTaxonomy ordering.
	/// The "All" category at index 0 holds shared ClusterScanItem references.
	/// </summary>
	public class ClusterScanSnapshot {
		public readonly List<ClusterScanCategory> Categories;
		public readonly AxialI Origin;

		public ClusterScanSnapshot(List<ClusterScanEntry> entries, AxialI origin) {
			Origin = origin;
			Categories = Build(entries, origin);
		}

		public int CategoryCount => Categories.Count;
		public ClusterScanCategory GetCategory(int ci) => Categories[ci];

		/// <summary>
		/// Remove a ClusterScanEntry from its item's instance list.
		/// If the item becomes empty, prune it from all categories.
		/// </summary>
		public void RemoveInstance(ClusterScanItem item, ClusterScanEntry entry) {
			item.Instances.Remove(entry);
			if (item.Instances.Count > 0) return;
			PruneEmptyItem(item);
		}

		private static List<ClusterScanCategory> Build(
				List<ClusterScanEntry> entries, AxialI origin) {
			// Group: category -> itemName -> instances
			var grouped = new Dictionary<string,
				Dictionary<string, List<ClusterScanEntry>>>();

			foreach (var entry in entries) {
				if (!grouped.TryGetValue(entry.Category, out var byCat))
					grouped[entry.Category] = byCat =
						new Dictionary<string, List<ClusterScanEntry>>();
				if (!byCat.TryGetValue(entry.ItemName, out var instances))
					byCat[entry.ItemName] = instances = new List<ClusterScanEntry>();
				instances.Add(entry);
			}

			// Build named categories
			var namedCategories = new List<ClusterScanCategory>();
			foreach (var catKvp in grouped) {
				var items = new List<ClusterScanItem>();
				foreach (var itemKvp in catKvp.Value) {
					var instances = itemKvp.Value;
					instances.Sort((a, b) =>
						AxialUtil.GetDistance(origin, a.Location)
							.CompareTo(AxialUtil.GetDistance(origin, b.Location)));
					items.Add(new ClusterScanItem {
						ItemName = itemKvp.Key,
						Instances = instances,
					});
				}
				items.Sort((a, b) => CompareItems(a, b, origin));
				namedCategories.Add(new ClusterScanCategory {
					Name = catKvp.Key,
					Items = items,
				});
			}

			namedCategories.Sort((a, b) =>
				ClusterMapTaxonomy.CategorySortIndex(a.Name)
					.CompareTo(ClusterMapTaxonomy.CategorySortIndex(b.Name)));

			// Build "All" category from shared item references
			var allItems = new List<ClusterScanItem>();
			foreach (var cat in namedCategories)
				allItems.AddRange(cat.Items);
			allItems.Sort((a, b) => CompareItems(a, b, origin));

			var categories = new List<ClusterScanCategory>(namedCategories.Count + 1) {
				new ClusterScanCategory {
					Name = ClusterMapTaxonomy.Categories.All,
					Items = allItems,
				}
			};
			categories.AddRange(namedCategories);

			return categories;
		}

		private static int CompareItems(
				ClusterScanItem a, ClusterScanItem b, AxialI origin) {
			int sk = a.Instances[0].SortKey.CompareTo(b.Instances[0].SortKey);
			if (sk != 0) return sk;
			return AxialUtil.GetDistance(origin, a.Instances[0].Location)
				.CompareTo(AxialUtil.GetDistance(origin, b.Instances[0].Location));
		}

		private void PruneEmptyItem(ClusterScanItem item) {
			for (int ci = Categories.Count - 1; ci >= 0; ci--) {
				var cat = Categories[ci];
				if (cat.Items.Remove(item) && cat.Items.Count == 0)
					Categories.RemoveAt(ci);
			}
		}
	}
}
