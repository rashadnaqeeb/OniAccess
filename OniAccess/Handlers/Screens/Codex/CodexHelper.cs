using System.Collections.Generic;

namespace OniAccess.Handlers.Screens.Codex {
	/// <summary>
	/// Static helpers for Codex data access: category ordering, entry filtering,
	/// and link extraction. All methods re-query CodexCache at call time.
	/// </summary>
	internal static class CodexHelper {
		/// <summary>
		/// Priority ordering for level-0 categories. Last entry in the game's
		/// SetupCategory calls = first in navigator = first here.
		/// Categories not in this list sort alphabetically after these.
		/// </summary>
		private static readonly string[] CategoryOrder = {
			"Root", "LESSONS", "CREATURES::GeneralInfo", "MYLOG",
			"INVESTIGATIONS", "EMAILS", "JOURNALS", "RESEARCHNOTES",
			"NOTICES", "CREATURES", "PLANTS",
		};

		/// <summary>
		/// Get top-level categories, combining HOME's programmatic CategoryEntry
		/// children with YAML-based categories discovered from entry.category
		/// strings. The game discovers categories dynamically from the category
		/// field in CategorizeEntries; programmatic categories use the
		/// CategoryEntry.entriesInCategory tree.
		/// </summary>
		internal static List<CodexEntry> GetTopCategories() {
			var seen = new HashSet<string>();
			var filtered = new List<CodexEntry>();

			// Programmatic categories from HOME.entriesInCategory
			if (CodexCache.entries.TryGetValue("HOME", out var home)) {
				var cat = home as CategoryEntry;
				if (cat != null) {
					foreach (var entry in cat.entriesInCategory) {
						if (!IsTopCategoryVisible(entry)) continue;
						seen.Add(entry.id);
						filtered.Add(entry);
					}
				}
			}

			// YAML-based categories: scan all entries for category strings
			// that aren't already reachable in the programmatic tree.
			// Skip categories whose ID maps to a CategoryEntry — those
			// are nested sub-categories (e.g. VIDEOTUTORIALS under LESSONS).
			var yamlCatIds = new HashSet<string>();
			foreach (var kvp in CodexCache.entries) {
				var entry = kvp.Value;
				if (entry.searchOnly) continue;
				if (string.IsNullOrEmpty(entry.category)) continue;
				string catId = entry.category;
				if (catId == "Root") continue;
				if (seen.Contains(catId)) continue;
				if (CodexCache.entries.TryGetValue(catId, out var existing)
					&& existing is CategoryEntry) continue;
				if (!IsEntryVisible(entry)) continue;
				yamlCatIds.Add(catId);
			}

			foreach (string catId in yamlCatIds) {
				if (seen.Contains(catId)) continue;
				seen.Add(catId);
				// Use the named CodexEntry for this category if one exists.
				// YAML directories often have an entry whose ID matches
				// the folder name (e.g. MYLOG, EMAILS).
				if (CodexCache.entries.TryGetValue(catId, out var catEntry)) {
					filtered.Add(catEntry);
				} else {
					// No dedicated entry for this category. Create a
					// lightweight stand-in entry so the category appears.
					// The stand-in has no content of its own.
					var standIn = new CodexEntry(catId, new List<ContentContainer>(),
						ResolveCategoryName(catId));
					standIn.id = catId;
					filtered.Add(standIn);
				}
			}

			filtered.Sort((a, b) => {
				int ia = GetCategoryPriority(a.id);
				int ib = GetCategoryPriority(b.id);
				if (ia != ib) return ia.CompareTo(ib);
				return string.Compare(a.sortString ?? a.name, b.sortString ?? b.name);
			});

			return filtered;
		}

		/// <summary>
		/// Whether a top-level category should appear. CategoryEntry objects
		/// are always visible (they have ungated title containers). Other
		/// entries use the standard IsEntryVisible check.
		/// </summary>
		private static bool IsTopCategoryVisible(CodexEntry entry) {
			if (entry is CategoryEntry) {
				if (entry.disabled) return false;
				if (!Game.IsCorrectDlcActiveForCurrentSave(entry)) return false;
				return true;
			}
			return IsEntryVisible(entry);
		}

		/// <summary>
		/// Whether an entry acts as a category with drillable children.
		/// True for CategoryEntry objects and for YAML-based categories
		/// that have child entries with matching category strings.
		/// </summary>
		internal static bool IsCategory(CodexEntry entry) {
			if (entry is CategoryEntry) return true;
			return GetEntriesForYAMLCategory(entry.id).Count > 0;
		}

		/// <summary>
		/// Get entries within a category, filtered. For programmatic
		/// CategoryEntry objects, returns entriesInCategory. For YAML-based
		/// categories, returns all entries whose category field matches.
		/// </summary>
		internal static List<CodexEntry> GetEntriesInCategory(CodexEntry category) {
			var cat = category as CategoryEntry;
			if (cat != null) {
				var filtered = new List<CodexEntry>();
				foreach (var entry in cat.entriesInCategory) {
					if (IsEntryVisible(entry))
						filtered.Add(entry);
				}
				return filtered;
			}

			return GetEntriesForYAMLCategory(category.id);
		}

		/// <summary>
		/// Find all visible entries whose category field matches a given
		/// category ID. Excludes searchOnly entries and the category
		/// entry itself.
		/// </summary>
		private static List<CodexEntry> GetEntriesForYAMLCategory(string categoryId) {
			var filtered = new List<CodexEntry>();
			foreach (var kvp in CodexCache.entries) {
				var entry = kvp.Value;
				if (entry.id == categoryId) continue;
				if (entry.category != categoryId) continue;
				if (entry.searchOnly) continue;
				if (!IsEntryVisible(entry)) continue;
				filtered.Add(entry);
			}
			return filtered;
		}

		/// <summary>
		/// Whether an entry should appear in the navigator.
		/// Replicates CodexScreen.HasUnlockedCategoryEntries + DLC + disabled checks.
		/// </summary>
		internal static bool IsEntryVisible(CodexEntry entry) {
			if (entry.disabled) return false;
			if (!Game.IsCorrectDlcActiveForCurrentSave(entry)) return false;
			return HasUnlockedContent(entry);
		}

		/// <summary>
		/// Whether an entry has at least one unlocked or ungated content container.
		/// </summary>
		private static bool HasUnlockedContent(CodexEntry entry) {
			if (entry.contentContainers == null) return false;
			foreach (var cc in entry.contentContainers) {
				if (string.IsNullOrEmpty(cc.lockID) || Game.Instance.unlocks.IsUnlocked(cc.lockID))
					return true;
			}
			return false;
		}

		private static int GetCategoryPriority(string id) {
			for (int i = 0; i < CategoryOrder.Length; i++) {
				if (CategoryOrder[i] == id) return i;
			}
			return CategoryOrder.Length;
		}

		/// <summary>
		/// Resolve category display name from STRINGS.UI.CODEX.CATEGORYNAMES
		/// or the named CodexEntry.
		/// </summary>
		private static string ResolveCategoryName(string categoryId) {
			if (CodexCache.entries.TryGetValue(categoryId, out var catEntry))
				return GetEntryName(catEntry);

			string loc = Strings.Get("STRINGS.UI.CODEX.CATEGORYNAMES." + categoryId.ToUpper());
			if (!string.IsNullOrEmpty(loc)) return loc;

			return categoryId;
		}

		/// <summary>
		/// Resolve category display name for a top-level entry. For
		/// CategoryEntry objects, uses the entry name. For YAML categories,
		/// looks up the localized name from STRINGS.UI.CODEX.CATEGORYNAMES.
		/// </summary>
		internal static string GetCategoryDisplayName(CodexEntry entry) {
			if (entry is CategoryEntry)
				return GetEntryName(entry);
			return ResolveCategoryName(entry.id);
		}

		/// <summary>
		/// Resolve entry name, falling back to title LocString key.
		/// </summary>
		internal static string GetEntryName(CodexEntry entry) {
			if (!string.IsNullOrEmpty(entry.name)) return entry.name;
			if (!string.IsNullOrEmpty(entry.title)) return Strings.Get(entry.title);
			return entry.id;
		}

		/// <summary>
		/// Whether a content container should be included in the content reader.
		/// </summary>
		internal static bool IsContainerVisible(ContentContainer cc) {
			return Game.IsCorrectDlcActiveForCurrentSave(cc);
		}

		/// <summary>
		/// Whether a content container is locked (has a lockID that isn't unlocked).
		/// </summary>
		internal static bool IsContainerLocked(ContentContainer cc) {
			return !string.IsNullOrEmpty(cc.lockID)
				&& !Game.Instance.unlocks.IsUnlocked(cc.lockID);
		}

		/// <summary>
		/// Whether an ICodexWidget should be included (DLC filter).
		/// </summary>
		internal static bool IsWidgetVisible(ICodexWidget widget) {
			return Game.IsCorrectDlcActiveForCurrentSave(widget as IHasDlcRestrictions);
		}

		/// <summary>
		/// Whether a SubEntry should appear in the navigator.
		/// </summary>
		internal static bool IsSubEntryVisible(SubEntry sub) {
			if (sub.disabled) return false;
			if (!Game.IsCorrectDlcActiveForCurrentSave(sub)) return false;
			if (!string.IsNullOrEmpty(sub.lockID) && !Game.Instance.unlocks.IsUnlocked(sub.lockID))
				return false;
			return true;
		}

		/// <summary>
		/// Get visible SubEntries for drilling. Returns empty if count &lt;= 1
		/// (a single SubEntry IS the entry content, not worth drilling into).
		/// Categories never have SubEntries worth drilling.
		/// </summary>
		internal static List<SubEntry> GetVisibleSubEntries(CodexEntry entry) {
			var result = new List<SubEntry>();
			if (entry == null || IsCategory(entry)) return result;
			if (entry.subEntries == null || entry.subEntries.Count <= 1) return result;
			foreach (var sub in entry.subEntries)
				if (IsSubEntryVisible(sub)) result.Add(sub);
			return result;
		}

		/// <summary>
		/// Resolve SubEntry display name, falling back to title LocString key.
		/// </summary>
		internal static string GetSubEntryName(SubEntry sub) {
			if (!string.IsNullOrEmpty(sub.name)) return sub.name;
			if (!string.IsNullOrEmpty(sub.title)) return Strings.Get(sub.title);
			return sub.id;
		}

		/// <summary>
		/// Extract link IDs from a CodexText's raw text (TMP link markup).
		/// Returns (linkID, displayText) pairs.
		/// </summary>
		internal static List<(string id, string text)> ExtractTextLinks(string rawText) {
			var links = new List<(string, string)>();
			if (string.IsNullOrEmpty(rawText)) return links;

			int searchStart = 0;
			while (true) {
				int linkStart = rawText.IndexOf("<link=\"", searchStart, System.StringComparison.Ordinal);
				if (linkStart < 0) break;

				int idStart = linkStart + 7;
				int idEnd = rawText.IndexOf("\">", idStart, System.StringComparison.Ordinal);
				if (idEnd < 0) break;

				string linkId = rawText.Substring(idStart, idEnd - idStart);

				int textStart = idEnd + 2;
				int textEnd = rawText.IndexOf("</link>", textStart, System.StringComparison.Ordinal);
				if (textEnd < 0) break;

				string displayText = rawText.Substring(textStart, textEnd - textStart);
				links.Add((linkId, displayText));
				searchStart = textEnd + 7;
			}

			return links;
		}
	}
}
