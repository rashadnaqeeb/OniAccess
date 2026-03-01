using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Codex {
	/// <summary>
	/// Categories tab: 3-level NestedMenuHandler.
	/// Level 0 = top categories from HOME.entriesInCategory
	/// Level 1 = entries or sub-categories within a category
	/// Level 2 = entries within a sub-category (only when level 1 is a CategoryEntry)
	///
	/// Leaf activation calls ChangeArticle on the game screen and
	/// switches to the content tab.
	/// </summary>
	internal class CategoriesTab: NestedMenuHandler, ICodexTab {
		private readonly CodexScreenHandler _parent;

		internal CategoriesTab(CodexScreenHandler parent) : base(screen: null) {
			_parent = parent;
		}

		public string TabName => (string)STRINGS.ONIACCESS.CODEX.CATEGORIES_TAB;

		public override string DisplayName => TabName;

		public override IReadOnlyList<HelpEntry> HelpEntries => NestedNavHelpEntries;

		// ========================================
		// ICodexTab
		// ========================================

		public void OnTabActivated(bool announce) {
			OnTabActivatedOnEntry(announce, entryId: null);
		}

		/// <summary>
		/// Activate and optionally position the cursor on a specific entry.
		/// Used when returning from the content tab to land on the article
		/// the user was reading.
		/// </summary>
		internal void OnTabActivatedOnEntry(bool announce, string entryId) {
			if (entryId == null || !NavigateToEntry(entryId))
				ResetState();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			if (ItemCount > 0) {
				string label = GetItemLabel(_currentIndex);
				if (!string.IsNullOrEmpty(label))
					SpeechPipeline.SpeakQueued(label);
			}
		}

		public void OnTabDeactivated() {
			_search.Clear();
		}

		public bool HandleInput() {
			return base.Tick();
		}

		public new bool HandleKeyDown(KButtonEvent e) {
			return base.HandleKeyDown(e);
		}

		// ========================================
		// NestedMenuHandler abstracts
		// ========================================

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 1;

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return CodexHelper.GetTopCategories().Count;

			var topCats = CodexHelper.GetTopCategories();
			if (indices[0] < 0 || indices[0] >= topCats.Count) return 0;

			if (level == 1)
				return CodexHelper.GetEntriesInCategory(topCats[indices[0]]).Count;

			// Level 2: only if level 1 item is a CategoryEntry
			var level1Entries = CodexHelper.GetEntriesInCategory(topCats[indices[0]]);
			if (indices[1] < 0 || indices[1] >= level1Entries.Count) return 0;
			var level1Item = level1Entries[indices[1]];
			if (level1Item is CategoryEntry)
				return CodexHelper.GetEntriesInCategory(level1Item).Count;
			return 0;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			var topCats = CodexHelper.GetTopCategories();
			if (indices[0] < 0 || indices[0] >= topCats.Count) return null;
			if (level == 0)
				return CodexHelper.GetEntryName(topCats[indices[0]]);

			var entries = CodexHelper.GetEntriesInCategory(topCats[indices[0]]);

			if (level == 1) {
				if (indices[1] < 0 || indices[1] >= entries.Count) return null;
				return CodexHelper.GetEntryName(entries[indices[1]]);
			}

			// Level 2
			if (indices[1] < 0 || indices[1] >= entries.Count) return null;
			var subEntries = CodexHelper.GetEntriesInCategory(entries[indices[1]]);
			if (indices[2] < 0 || indices[2] >= subEntries.Count) return null;
			return CodexHelper.GetEntryName(subEntries[indices[2]]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			var topCats = CodexHelper.GetTopCategories();
			if (indices[0] < 0 || indices[0] >= topCats.Count) return null;

			if (level == 2) {
				var entries = CodexHelper.GetEntriesInCategory(topCats[indices[0]]);
				if (indices[1] >= 0 && indices[1] < entries.Count)
					return CodexHelper.GetEntryName(entries[indices[1]]);
			}
			if (level >= 1)
				return CodexHelper.GetEntryName(topCats[indices[0]]);
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			var entry = ResolveEntry(indices);
			if (entry == null) return;

			var codexScreen = _parent.CodexScreen;
			if (codexScreen == null) return;

			PlayOpenSound();
			codexScreen.ChangeArticle(entry.id);
			_parent.JumpToContentTab();
		}

		protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) {
			// If the mapped level-1 entry is a CategoryEntry (sub-category),
			// search should land at level 2. Otherwise level 1.
			var topCats = CodexHelper.GetTopCategories();
			if (mappedIndices[0] < 0 || mappedIndices[0] >= topCats.Count) return 1;
			var entries = CodexHelper.GetEntriesInCategory(topCats[mappedIndices[0]]);
			if (mappedIndices[1] < 0 || mappedIndices[1] >= entries.Count) return 1;
			if (entries[mappedIndices[1]] is CategoryEntry)
				return 2;
			return 1;
		}

		// ========================================
		// Search across all leaf entries
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			return GetAllLeafEntries().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var all = GetAllLeafEntries();
			if (flatIndex < 0 || flatIndex >= all.Count) return null;
			return CodexHelper.GetEntryName(all[flatIndex].entry);
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			var all = GetAllLeafEntries();
			if (flatIndex < 0 || flatIndex >= all.Count) return;
			var item = all[flatIndex];
			outIndices[0] = item.catIdx;
			outIndices[1] = item.entryIdx;
			outIndices[2] = item.subIdx;
		}

		// ========================================
		// Helpers
		// ========================================

		/// <summary>
		/// Position the cursor on the leaf entry matching entryId.
		/// Returns false if the entry isn't found in the category tree.
		/// </summary>
		private bool NavigateToEntry(string entryId) {
			var all = GetAllLeafEntries();
			for (int i = 0; i < all.Count; i++) {
				if (all[i].entry.id == entryId) {
					var item = all[i];
					SetIndex(0, item.catIdx);
					SetIndex(1, item.entryIdx);
					SetIndex(2, item.subIdx);

					var topCats = CodexHelper.GetTopCategories();
					var entries = CodexHelper.GetEntriesInCategory(topCats[item.catIdx]);
					Level = (entries[item.entryIdx] is CategoryEntry) ? 2 : 1;

					_currentIndex = GetIndex(Level);
					_search.Clear();
					SuppressSearchThisFrame();
					return true;
				}
			}
			return false;
		}

		private CodexEntry ResolveEntry(int[] indices) {
			var topCats = CodexHelper.GetTopCategories();
			if (indices[0] < 0 || indices[0] >= topCats.Count) return null;
			var entries = CodexHelper.GetEntriesInCategory(topCats[indices[0]]);
			if (indices[1] < 0 || indices[1] >= entries.Count) return null;

			var level1 = entries[indices[1]];
			if (level1 is CategoryEntry) {
				var subEntries = CodexHelper.GetEntriesInCategory(level1);
				if (indices[2] < 0 || indices[2] >= subEntries.Count) return null;
				return subEntries[indices[2]];
			}
			return level1;
		}

		private struct FlatEntry {
			internal CodexEntry entry;
			internal int catIdx;
			internal int entryIdx;
			internal int subIdx;
		}

		private List<FlatEntry> GetAllLeafEntries() {
			var result = new List<FlatEntry>();
			var topCats = CodexHelper.GetTopCategories();
			for (int c = 0; c < topCats.Count; c++) {
				var entries = CodexHelper.GetEntriesInCategory(topCats[c]);
				for (int e = 0; e < entries.Count; e++) {
					if (entries[e] is CategoryEntry subCat) {
						var subEntries = CodexHelper.GetEntriesInCategory(subCat);
						for (int s = 0; s < subEntries.Count; s++) {
							result.Add(new FlatEntry {
								entry = subEntries[s],
								catIdx = c,
								entryIdx = e,
								subIdx = s
							});
						}
					} else {
						result.Add(new FlatEntry {
							entry = entries[e],
							catIdx = c,
							entryIdx = e,
							subIdx = 0
						});
					}
				}
			}
			return result;
		}
	}
}
