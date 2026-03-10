using System.Collections.Generic;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens.Details {
	/// <summary>
	/// Merges a fresh list of DetailSections into an existing list in place,
	/// preserving the order of matched items. This stabilizes navigation
	/// when the game reorders widgets with equal sort keys between frames.
	/// </summary>
	static class SectionMerger {
		public static void Merge(List<DetailSection> existing, List<DetailSection> fresh) {
			MergeList(existing, fresh, SectionKey, SectionTypesMatch, UpdateSection);
		}

		private static void UpdateSection(DetailSection old, DetailSection fresh) {
			old.Header = fresh.Header;
			MergeList(old.Items, fresh.Items, WidgetKey, WidgetTypesMatch, UpdateWidget);
		}

		private static void UpdateWidget(Widget old, Widget fresh) {
			old.UpdateFrom(fresh);
			var oldChildren = old.Children;
			var freshChildren = fresh.Children;
			if (freshChildren == null || freshChildren.Count == 0) {
				old.Children = null;
				return;
			}
			if (oldChildren == null) {
				old.Children = freshChildren;
				return;
			}
			MergeList(oldChildren, freshChildren, WidgetKey, WidgetTypesMatch, UpdateWidget);
		}

		/// <summary>
		/// Generic merge: match items by key, keep existing order for matches,
		/// insert new items relative to neighbors, remove gone items.
		/// </summary>
		private static void MergeList<T>(
				List<T> existing, List<T> fresh,
				System.Func<T, string> getKey,
				System.Func<T, T, bool> typesMatch,
				System.Action<T, T> update) {
			// Build key-to-indices map for existing items.
			// Handles duplicates: each key maps to a queue of indices.
			var oldMap = new Dictionary<string, Queue<int>>();
			for (int i = 0; i < existing.Count; i++) {
				string key = getKey(existing[i]);
				if (key == null) continue;
				if (!oldMap.TryGetValue(key, out var q)) {
					q = new Queue<int>();
					oldMap[key] = q;
				}
				q.Enqueue(i);
			}

			// Track which existing indices are matched.
			var matched = new HashSet<int>();

			// For each fresh item, find its match in existing.
			// matchedOldIndex[freshIdx] = old index, or -1 if new.
			var matchedOldIndex = new int[fresh.Count];
			for (int fi = 0; fi < fresh.Count; fi++) {
				string key = getKey(fresh[fi]);
				int oldIdx = -1;
				if (key != null && oldMap.TryGetValue(key, out var q)) {
					while (q.Count > 0) {
						int candidate = q.Dequeue();
						if (!matched.Contains(candidate)
								&& typesMatch(existing[candidate], fresh[fi])) {
							oldIdx = candidate;
							break;
						}
					}
				}

				// Fallback: if no key or key not found, try label matching
				// within unmatched items.
				if (oldIdx < 0 && key == null) {
					string label = FallbackLabel(fresh[fi]);
					if (label != null) {
						for (int oi = 0; oi < existing.Count; oi++) {
							if (matched.Contains(oi)) continue;
							if (FallbackLabel(existing[oi]) == label
									&& typesMatch(existing[oi], fresh[fi])) {
								oldIdx = oi;
								break;
							}
						}
					}
				}

				if (oldIdx >= 0) matched.Add(oldIdx);
				matchedOldIndex[fi] = oldIdx;
			}

			// Update matched items in place.
			for (int fi = 0; fi < fresh.Count; fi++) {
				int oi = matchedOldIndex[fi];
				if (oi >= 0)
					update(existing[oi], fresh[fi]);
			}

			// Build the merged result preserving existing order for matched
			// items and inserting new items relative to neighbors.
			var result = new List<T>(fresh.Count);

			// Map from old index to position in result (for matched items).
			// We process fresh items in order, emitting matched items at
			// their existing relative order and inserting new items.

			// Collect matched old indices in the order they appear in existing.
			var matchedInOrder = new List<int>();
			for (int oi = 0; oi < existing.Count; oi++) {
				if (matched.Contains(oi))
					matchedInOrder.Add(oi);
			}

			// Map each matched old index to its fresh index.
			var oldToFresh = new Dictionary<int, int>();
			for (int fi = 0; fi < fresh.Count; fi++) {
				if (matchedOldIndex[fi] >= 0)
					oldToFresh[matchedOldIndex[fi]] = fi;
			}

			// Build result: start with matched items in existing order.
			foreach (int oi in matchedInOrder)
				result.Add(existing[oi]);

			// Insert new (unmatched) fresh items relative to neighbors.
			for (int fi = 0; fi < fresh.Count; fi++) {
				if (matchedOldIndex[fi] >= 0) continue;

				// Find the last preceding fresh item that is matched.
				int insertAfterResultIdx = -1;
				for (int pi = fi - 1; pi >= 0; pi--) {
					int predOld = matchedOldIndex[pi];
					if (predOld >= 0) {
						// Find this item in result.
						for (int ri = 0; ri < result.Count; ri++) {
							if (ReferenceEquals(result[ri], existing[predOld])) {
								insertAfterResultIdx = ri;
								break;
							}
						}
						break;
					}
					// The predecessor is also new — find it in result.
					for (int ri = 0; ri < result.Count; ri++) {
						if (ReferenceEquals(result[ri], fresh[pi])) {
							insertAfterResultIdx = ri;
							break;
						}
					}
					if (insertAfterResultIdx >= 0) break;
				}

				result.Insert(insertAfterResultIdx + 1, fresh[fi]);
			}

			existing.Clear();
			existing.AddRange(result);
		}

		private static string SectionKey(DetailSection s) => s.Key ?? s.Header;
		private static bool SectionTypesMatch(DetailSection a, DetailSection b) => true;

		private static string WidgetKey(Widget w) => w.Key;
		private static bool WidgetTypesMatch(Widget a, Widget b) =>
			a.GetType() == b.GetType();

		private static string FallbackLabel<T>(T item) {
			if (item is Widget w) return w.Label;
			if (item is DetailSection s) return s.Header;
			return null;
		}
	}
}
