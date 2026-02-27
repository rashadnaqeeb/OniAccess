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
		/// Get top-level categories from HOME, filtered and ordered.
		/// </summary>
		internal static List<CodexEntry> GetTopCategories() {
			if (!CodexCache.entries.TryGetValue("HOME", out var home)) return new List<CodexEntry>();
			var cat = home as CategoryEntry;
			if (cat == null) return new List<CodexEntry>();

			var filtered = new List<CodexEntry>();
			foreach (var entry in cat.entriesInCategory) {
				if (IsEntryVisible(entry))
					filtered.Add(entry);
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
		/// Get entries within a category, filtered. Returns entries from
		/// CategoryEntry.entriesInCategory if the entry is a CategoryEntry.
		/// </summary>
		internal static List<CodexEntry> GetEntriesInCategory(CodexEntry category) {
			var cat = category as CategoryEntry;
			if (cat == null) return new List<CodexEntry>();

			var filtered = new List<CodexEntry>();
			foreach (var entry in cat.entriesInCategory) {
				if (IsEntryVisible(entry))
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
