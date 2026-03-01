using System;
using System.Collections.Generic;

namespace OniAccess.Handlers.Tiles.Scanner {
	public static class ScannerSearch {
		public static List<ScanEntry> Filter(List<ScanEntry> allEntries, string query) {
			var results = new List<ScanEntry>();
			string q = query.ToLowerInvariant();

			foreach (var entry in allEntries) {
				int sortKey = MatchSortKey(entry.ItemName, q);
				if (sortKey < 0) continue;

				results.Add(new ScanEntry {
					Cell = entry.Cell,
					Backend = entry.Backend,
					BackendData = entry.BackendData,
					ItemName = entry.ItemName,
					Category = (string)STRINGS.ONIACCESS.SCANNER.CATEGORIES.SEARCH,
					Subcategory = entry.Category,
					SortKey = sortKey,
				});
			}

			return results;
		}

		/// <summary>
		/// Returns sort key (0=string prefix, 1=whole word at word boundary,
		/// 2=word-start at word boundary) or -1 for no match.
		/// Scans all positions to find the best (lowest) sort key.
		/// </summary>
		internal static int MatchSortKey(string itemName, string query) {
			string lower = itemName.ToLowerInvariant();
			query = query.ToLowerInvariant();

			if (lower.StartsWith(query, StringComparison.Ordinal))
				return 0;

			int best = -1;
			int idx = 0;
			while (true) {
				int pos = lower.IndexOf(query, idx, StringComparison.Ordinal);
				if (pos < 0) break;

				if (pos > 0 && lower[pos - 1] == ' ') {
					int end = pos + query.Length;
					if (end >= lower.Length || lower[end] == ' ') {
						return 1; // whole word — can't improve past prefix
					}
					if (best < 0) best = 2;
				}

				idx = pos + 1;
			}

			return best;
		}
	}
}
