using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Codex {
	/// <summary>
	/// Categories tab: 4-level NestedMenuHandler.
	/// Level 0 = top categories from HOME.entriesInCategory
	/// Level 1 = entries or sub-categories within a category
	/// Level 2 = entries within a sub-category (CategoryEntry children)
	///           OR SubEntries of a level 1 entry (critter morphs, etc.)
	/// Level 3 = SubEntries of a level 2 entry inside a sub-category
	///
	/// Enter on a CodexEntry with SubEntries opens the article directly.
	/// Right arrow drills into SubEntries. Enter on a SubEntry opens the
	/// parent article with the content cursor at that SubEntry's section.
	/// </summary>
	internal class CategoriesTab: NestedMenuHandler, IScreenTab {
		private readonly CodexScreenHandler _parent;

		internal CategoriesTab(CodexScreenHandler parent) : base(screen: null) {
			_parent = parent;
			_search.GroupOf = GetSearchGroup;
		}

		public string TabName => (string)STRINGS.ONIACCESS.CODEX.CATEGORIES_TAB;

		public override string DisplayName => TabName;

		public override IReadOnlyList<HelpEntry> HelpEntries => NestedNavHelpEntries;

		// ========================================
		// IScreenTab
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
				string label = GetItemLabel(CurrentIndex);
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

		protected override int MaxLevel => 3;
		protected override int SearchLevel => 1;

		protected override bool ShouldDrillOnActivate() {
			var entry = ResolveEntryAtLevel(Level);
			return entry is CategoryEntry;
		}

		protected override int GetItemCount(int level, int[] indices) {
			if (level == 0)
				return CodexHelper.GetTopCategories().Count;

			var parent = ResolveEntryAtLevel(level - 1, indices);
			if (parent == null) return 0;

			if (parent is CategoryEntry)
				return CodexHelper.GetEntriesInCategory(parent).Count;

			return CodexHelper.GetVisibleSubEntries(parent).Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (level == 0) {
				var topCats = CodexHelper.GetTopCategories();
				if (indices[0] < 0 || indices[0] >= topCats.Count) return null;
				return CodexHelper.GetEntryName(topCats[indices[0]]);
			}

			var parent = ResolveEntryAtLevel(level - 1, indices);
			if (parent == null) return null;

			if (parent is CategoryEntry) {
				var children = CodexHelper.GetEntriesInCategory(parent);
				if (indices[level] < 0 || indices[level] >= children.Count) return null;
				return CodexHelper.GetEntryName(children[indices[level]]);
			}

			var subs = CodexHelper.GetVisibleSubEntries(parent);
			if (indices[level] < 0 || indices[level] >= subs.Count) return null;
			return CodexHelper.GetSubEntryName(subs[indices[level]]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (level <= 0) return null;
			var parent = ResolveEntryAtLevel(level - 1, indices);
			if (parent == null) return null;
			return CodexHelper.GetEntryName(parent);
		}

		protected override void ActivateLeafItem(int[] indices) {
			var sub = ResolveSubEntry(indices);
			if (sub != null) {
				_parent.ContentTabRef.SetPendingSubEntryId(sub.id);
				var screen = _parent.CodexScreen;
				if (screen == null) return;
				PlaySound("HUD_Click_Open");
				screen.ChangeArticle(sub.id);
				_parent.JumpToContentTab();
				return;
			}

			var entry = ResolveEntryAtLevel(Level, indices);
			if (entry == null) return;

			var codexScreen = _parent.CodexScreen;
			if (codexScreen == null) return;

			PlaySound("HUD_Click_Open");
			codexScreen.ChangeArticle(entry.id);
			_parent.JumpToContentTab();
		}

		protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) {
			var all = GetAllSearchableEntries();
			if (flatIndex < 0 || flatIndex >= all.Count) return 1;
			var item = all[flatIndex];
			if (item.isCategory) return 0;
			return item.targetLevel;
		}

		// ========================================
		// Search across all leaf entries and SubEntries
		// ========================================

		protected override int GetSearchItemCount(int[] indices) {
			return GetAllSearchableEntries().Count;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			var all = GetAllSearchableEntries();
			if (flatIndex < 0 || flatIndex >= all.Count) return null;
			var item = all[flatIndex];
			if (item.subEntryName != null) return item.subEntryName;
			return CodexHelper.GetEntryName(item.entry);
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			var all = GetAllSearchableEntries();
			if (flatIndex < 0 || flatIndex >= all.Count) return;
			var item = all[flatIndex];
			outIndices[0] = item.catIdx;
			outIndices[1] = item.entryIdx;
			outIndices[2] = item.subCatIdx;
			outIndices[3] = item.subEntryIdx;
		}

		// ========================================
		// Helpers
		// ========================================

		private int GetSearchGroup(int flatIndex) {
			var all = GetAllSearchableEntries();
			return (flatIndex >= 0 && flatIndex < all.Count && all[flatIndex].isCategory) ? 1 : 0;
		}

		/// <summary>
		/// Resolve the CodexEntry at a given level using the current live indices.
		/// </summary>
		private CodexEntry ResolveEntryAtLevel(int level) {
			// Build a snapshot of live indices for ResolveEntryAtLevel(int, int[])
			int[] snap = new int[4];
			for (int i = 0; i <= level && i < 4; i++)
				snap[i] = GetIndex(i);
			return ResolveEntryAtLevel(level, snap);
		}

		/// <summary>
		/// Walk the category tree to resolve the CodexEntry at a given level.
		/// Only traverses CategoryEntry children (not SubEntries).
		/// </summary>
		private static CodexEntry ResolveEntryAtLevel(int level, int[] indices) {
			var topCats = CodexHelper.GetTopCategories();
			if (indices[0] < 0 || indices[0] >= topCats.Count) return null;
			if (level == 0) return topCats[indices[0]];

			CodexEntry current = topCats[indices[0]];
			for (int l = 1; l <= level; l++) {
				if (!(current is CategoryEntry)) return null;
				var children = CodexHelper.GetEntriesInCategory(current);
				if (indices[l] < 0 || indices[l] >= children.Count) return null;
				current = children[indices[l]];
			}
			return current;
		}

		/// <summary>
		/// If the current level points to a SubEntry (parent is non-CategoryEntry
		/// with visible SubEntries), return that SubEntry. Otherwise null.
		/// </summary>
		private SubEntry ResolveSubEntry(int[] indices) {
			if (Level < 1) return null;
			var parent = ResolveEntryAtLevel(Level - 1, indices);
			if (parent == null || parent is CategoryEntry) return null;
			var subs = CodexHelper.GetVisibleSubEntries(parent);
			if (subs.Count == 0) return null;
			if (indices[Level] < 0 || indices[Level] >= subs.Count) return null;
			return subs[indices[Level]];
		}

		/// <summary>
		/// Position the cursor on the leaf entry matching entryId.
		/// Returns false if the entry isn't found in the category tree.
		/// </summary>
		private bool NavigateToEntry(string entryId) {
			var all = GetAllSearchableEntries();
			for (int i = 0; i < all.Count; i++) {
				if (all[i].isCategory) continue;
				if (all[i].subEntryName != null) continue;
				if (all[i].entry.id == entryId) {
					var item = all[i];
					SetIndex(0, item.catIdx);
					SetIndex(1, item.entryIdx);
					SetIndex(2, item.subCatIdx);
					SetIndex(3, 0);
					Level = item.targetLevel;
					_search.Clear();
					SuppressSearchThisFrame();
					return true;
				}
			}
			return false;
		}

		private struct FlatEntry {
			internal CodexEntry entry;
			internal int catIdx;
			internal int entryIdx;
			internal int subCatIdx;
			internal int subEntryIdx;
			internal int targetLevel;
			internal bool isCategory;
			internal string subEntryName;
		}

		private List<FlatEntry> GetAllSearchableEntries() {
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
								subCatIdx = s,
								targetLevel = 2
							});
							// SubEntries of entries within a sub-category (level 3)
							AddSubEntrySearchItems(result, subEntries[s], c, e, s, 3);
						}
					} else {
						result.Add(new FlatEntry {
							entry = entries[e],
							catIdx = c,
							entryIdx = e,
							subCatIdx = 0,
							targetLevel = 1
						});
						// SubEntries of direct entries (level 2)
						AddSubEntrySearchItems(result, entries[e], c, e, 0, 2);
					}
				}
			}
			for (int c = 0; c < topCats.Count; c++) {
				result.Add(new FlatEntry {
					entry = topCats[c],
					catIdx = c,
					isCategory = true
				});
			}
			return result;
		}

		private static void AddSubEntrySearchItems(
			List<FlatEntry> result, CodexEntry entry,
			int catIdx, int entryIdx, int subCatIdx, int targetLevel
		) {
			var subs = CodexHelper.GetVisibleSubEntries(entry);
			for (int i = 0; i < subs.Count; i++) {
				result.Add(new FlatEntry {
					entry = entry,
					catIdx = catIdx,
					entryIdx = entryIdx,
					subCatIdx = subCatIdx,
					subEntryIdx = i,
					targetLevel = targetLevel,
					subEntryName = CodexHelper.GetSubEntryName(subs[i])
				});
			}
		}
	}
}
