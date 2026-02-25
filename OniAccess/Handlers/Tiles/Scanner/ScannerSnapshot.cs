using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner {
	public class ScannerItem {
		public string ItemName;
		public List<ScanEntry> Instances;
	}

	public class ScannerSubcategory {
		public string Name;
		public List<ScannerItem> Items;
	}

	public class ScannerCategory {
		public string Name;
		public List<ScannerSubcategory> Subcategories;
	}

	/// <summary>
	/// Frozen 4-level hierarchy built from a flat list of ScanEntry objects.
	/// Categories and subcategories follow ScannerTaxonomy ordering.
	/// The "all" subcategory at index 0 of each category holds shared
	/// ScannerItem references, so removing an instance from a named
	/// subcategory's item automatically removes it from "all".
	/// </summary>
	public class ScannerSnapshot {
		public readonly List<ScannerCategory> Categories;

		public ScannerSnapshot(List<ScanEntry> entries, int cursorCell) {
			Categories = Build(entries, cursorCell);
		}

		public int CategoryCount => Categories.Count;

		public ScannerCategory GetCategory(int ci) => Categories[ci];

		public ScannerSubcategory GetSubcategory(int ci, int si) =>
			Categories[ci].Subcategories[si];

		public ScannerItem GetItem(int ci, int si, int ii) =>
			Categories[ci].Subcategories[si].Items[ii];

		public ScanEntry GetInstance(int ci, int si, int ii, int ni) =>
			Categories[ci].Subcategories[si].Items[ii].Instances[ni];

		/// <summary>
		/// Remove a ScanEntry from its item's instance list. Because "all"
		/// holds shared ScannerItem references, the entry disappears from
		/// both named and "all" subcategories. If the item becomes empty,
		/// prune it from all subcategory lists and clean up empty containers.
		/// </summary>
		public void RemoveInstance(ScannerItem item, ScanEntry entry) {
			item.Instances.Remove(entry);
			if (item.Instances.Count > 0) return;
			PruneEmptyItem(item);
		}

		private static List<ScannerCategory> Build(
				List<ScanEntry> entries, int cursorCell) {
			// Group entries: category -> subcategory -> itemName -> instances
			var grouped = new Dictionary<string,
				Dictionary<string, Dictionary<string, List<ScanEntry>>>>();

			foreach (var entry in entries) {
				if (!grouped.TryGetValue(entry.Category, out var byCat))
					grouped[entry.Category] = byCat =
						new Dictionary<string, Dictionary<string, List<ScanEntry>>>();
				if (!byCat.TryGetValue(entry.Subcategory, out var bySub))
					byCat[entry.Subcategory] = bySub =
						new Dictionary<string, List<ScanEntry>>();
				if (!bySub.TryGetValue(entry.ItemName, out var instances))
					bySub[entry.ItemName] = instances = new List<ScanEntry>();
				instances.Add(entry);
			}

			var categories = new List<ScannerCategory>();

			foreach (var catKvp in grouped) {
				string catName = catKvp.Key;

				// Build named subcategories
				var namedSubcats = new List<ScannerSubcategory>();
				foreach (var subKvp in catKvp.Value) {
					var items = new List<ScannerItem>();
					foreach (var itemKvp in subKvp.Value) {
						var instances = itemKvp.Value;
						instances.Sort((a, b) =>
							GridUtil.CellDistance(cursorCell, a.Cell)
								.CompareTo(GridUtil.CellDistance(cursorCell, b.Cell)));
						items.Add(new ScannerItem {
							ItemName = itemKvp.Key,
							Instances = instances,
						});
					}
					items.Sort((a, b) =>
						GridUtil.CellDistance(cursorCell, a.Instances[0].Cell)
							.CompareTo(GridUtil.CellDistance(cursorCell, b.Instances[0].Cell)));

					namedSubcats.Add(new ScannerSubcategory {
						Name = subKvp.Key,
						Items = items,
					});
				}

				namedSubcats.Sort((a, b) =>
					ScannerTaxonomy.SubcategorySortIndex(catName, a.Name)
						.CompareTo(ScannerTaxonomy.SubcategorySortIndex(catName, b.Name)));

				// Build "all" from shared item references
				var allItems = new List<ScannerItem>();
				foreach (var sub in namedSubcats)
					allItems.AddRange(sub.Items);
				allItems.Sort((a, b) =>
					GridUtil.CellDistance(cursorCell, a.Instances[0].Cell)
						.CompareTo(GridUtil.CellDistance(cursorCell, b.Instances[0].Cell)));

				var subcats = new List<ScannerSubcategory>(namedSubcats.Count + 1) {
					new ScannerSubcategory {
						Name = ScannerTaxonomy.Subcategories.All,
						Items = allItems,
					}
				};
				subcats.AddRange(namedSubcats);

				categories.Add(new ScannerCategory {
					Name = catName,
					Subcategories = subcats,
				});
			}

			categories.Sort((a, b) =>
				ScannerTaxonomy.CategorySortIndex(a.Name)
					.CompareTo(ScannerTaxonomy.CategorySortIndex(b.Name)));

			return categories;
		}

		private void PruneEmptyItem(ScannerItem item) {
			for (int ci = Categories.Count - 1; ci >= 0; ci--) {
				var cat = Categories[ci];
				for (int si = cat.Subcategories.Count - 1; si >= 0; si--)
					cat.Subcategories[si].Items.Remove(item);
				for (int si = cat.Subcategories.Count - 1; si >= 0; si--) {
					if (cat.Subcategories[si].Items.Count == 0)
						cat.Subcategories.RemoveAt(si);
				}
				if (cat.Subcategories.Count == 0)
					Categories.RemoveAt(ci);
			}
		}

	}
}
