using System.Collections.Generic;
using OniAccess.Handlers.Tools;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Unified action menu combining tools and build categories.
	/// Tools appear first at level 0 index 0 with individual tools at
	/// level 1 (leaf). Build categories follow at indices 1+, with
	/// subcategories (level 1) and buildings (level 2).
	/// Type-ahead searches both tools and buildings.
	/// </summary>
	public class ActionMenuHandler: NestedMenuHandler {
		private readonly HashedString _initialCategory;
		private readonly BuildingDef _initialDef;
		private List<BuildMenuData.CategoryGroup> _tree;

		private const int ToolsCategoryIndex = 0;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries;

		static ActionMenuHandler() {
			var list = new List<HelpEntry>();
			list.AddRange(NestedNavHelpEntries);
			list.Add(new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE));
			_helpEntries = list.AsReadOnly();
		}

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public override string DisplayName => (string)STRINGS.ONIACCESS.BUILD_MENU.ACTION_MENU;

		/// <summary>
		/// Open fresh from the tile cursor.
		/// </summary>
		public ActionMenuHandler() {
			_initialCategory = HashedString.Invalid;
			_initialDef = null;
		}

		/// <summary>
		/// Return from placement (Tab in BuildToolHandler). Cursor starts on
		/// the building matching initialDef within the given category.
		/// </summary>
		public ActionMenuHandler(HashedString category, BuildingDef initialDef) {
			_initialCategory = category;
			_initialDef = initialDef;
		}

		private static bool IsToolsCategory(int catIndex) => catIndex == ToolsCategoryIndex;

		/// <summary>
		/// Convert level-0 category index to _tree index.
		/// Build categories occupy indices 1..N, mapping to _tree[0..N-1].
		/// </summary>
		private static int TreeIndex(int catIndex) => catIndex - 1;

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 2;

		protected override int GetItemCount(int level, int[] indices) {
			if (_tree == null) return 0;
			if (level == 0) return _tree.Count + 1;

			if (IsToolsCategory(indices[0])) {
				if (level == 1) return ToolHandler.AllTools.Count;
				return 0;
			}

			int ti = TreeIndex(indices[0]);
			if (ti < 0 || ti >= _tree.Count) return 0;
			var subs = _tree[ti].Subcategories;
			if (level == 1) return subs.Count;
			if (indices[1] < 0 || indices[1] >= subs.Count) return 0;
			return subs[indices[1]].Buildings.Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (_tree == null) return null;
			if (level == 0) {
				if (IsToolsCategory(indices[0]))
					return (string)STRINGS.ONIACCESS.BUILD_MENU.TOOLS_CATEGORY;
				int ti = TreeIndex(indices[0]);
				if (ti < 0 || ti >= _tree.Count) return null;
				return _tree[ti].DisplayName;
			}

			if (IsToolsCategory(indices[0])) {
				if (level == 1) {
					var tools = ToolHandler.AllTools;
					if (indices[1] < 0 || indices[1] >= tools.Count) return null;
					return tools[indices[1]].Label;
				}
				return null;
			}

			int treeIdx = TreeIndex(indices[0]);
			if (treeIdx < 0 || treeIdx >= _tree.Count) return null;
			var subs = _tree[treeIdx].Subcategories;
			if (level == 1) {
				if (indices[1] < 0 || indices[1] >= subs.Count) return null;
				return subs[indices[1]].Name;
			}
			if (indices[1] < 0 || indices[1] >= subs.Count) return null;
			var buildings = subs[indices[1]].Buildings;
			if (indices[2] < 0 || indices[2] >= buildings.Count) return null;
			return buildings[indices[2]].Label;
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (_tree == null) return null;

			if (IsToolsCategory(indices[0])) {
				if (level == 1)
					return (string)STRINGS.ONIACCESS.BUILD_MENU.TOOLS_CATEGORY;
				return null;
			}

			int ti = TreeIndex(indices[0]);
			if (level == 1) {
				if (ti < 0 || ti >= _tree.Count) return null;
				return _tree[ti].DisplayName;
			}
			if (level == 2) {
				if (ti < 0 || ti >= _tree.Count) return null;
				var subs = _tree[ti].Subcategories;
				if (indices[1] < 0 || indices[1] >= subs.Count) return null;
				return subs[indices[1]].Name;
			}
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			if (_tree == null) return;

			if (IsToolsCategory(indices[0])) {
				ActivateToolItem(indices[1]);
				return;
			}

			int ti = TreeIndex(indices[0]);
			if (ti < 0 || ti >= _tree.Count) return;
			var subs = _tree[ti].Subcategories;
			if (indices[1] < 0 || indices[1] >= subs.Count) return;
			var buildings = subs[indices[1]].Buildings;
			if (indices[2] < 0 || indices[2] >= buildings.Count) return;

			var entry = buildings[indices[2]];
			var category = _tree[ti].Category;

			var handler = new BuildToolHandler(category, entry.Def);
			HandlerStack.Replace(handler);
			handler.SuppressToolEvents = true;
			if (!BuildMenuData.SelectBuilding(entry.Def, category)) {
				handler.SuppressToolEvents = false;
				HandlerStack.Pop();
				PlayNegativeSound();
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.BUILD_MENU.NOT_BUILDABLE);
				return;
			}
			handler.SuppressToolEvents = false;
			handler.AnnounceInitialState();
		}

		private void ActivateToolItem(int toolIndex) {
			var tools = ToolHandler.AllTools;
			if (toolIndex < 0 || toolIndex >= tools.Count) return;

			var tool = tools[toolIndex];
			if (tool.RequiresModeFirst) {
				HandlerStack.Replace(new ToolFilterHandler(tool));
			} else {
				ToolPickerHandler.ActivateTool(tool);
				HandlerStack.Replace(new ToolHandler());
			}
		}

		// ========================================
		// LEVEL 2 CROSS-BOUNDARY NAVIGATION
		// ========================================

		protected override void NavigateNext() {
			if (Level < 2) {
				base.NavigateNext();
				return;
			}

			int cat = GetIndex(0);
			int ti = TreeIndex(cat);
			int sub = GetIndex(1);
			int bld = GetIndex(2);

			var subs = _tree[ti].Subcategories;
			int bldCount = subs[sub].Buildings.Count;

			if (bld + 1 < bldCount) {
				SetIndex(2, bld + 1);
				SyncCurrentIndex();
				PlayHoverSound();
				SpeakCurrentItem();
				return;
			}

			// Try next subcategory in current category
			for (int s = sub + 1; s < subs.Count; s++) {
				if (subs[s].Buildings.Count > 0) {
					SetIndex(1, s);
					SetIndex(2, 0);
					SyncCurrentIndex();
					PlayHoverSound();
					SpeakWithSubcategoryContext();
					return;
				}
			}

			// Try next categories
			for (int c = ti + 1; c < _tree.Count; c++) {
				var nextSubs = _tree[c].Subcategories;
				for (int s = 0; s < nextSubs.Count; s++) {
					if (nextSubs[s].Buildings.Count > 0) {
						SetIndex(0, c + 1);
						SetIndex(1, s);
						SetIndex(2, 0);
						SyncCurrentIndex();
						PlayHoverSound();
						SpeakWithCategoryContext();
						return;
					}
				}
			}

			// Wrap to first building in the entire tree
			for (int c = 0; c < _tree.Count; c++) {
				var wrapSubs = _tree[c].Subcategories;
				for (int s = 0; s < wrapSubs.Count; s++) {
					if (wrapSubs[s].Buildings.Count > 0) {
						SetIndex(0, c + 1);
						SetIndex(1, s);
						SetIndex(2, 0);
						SyncCurrentIndex();
						PlayWrapSound();
						if (c == ti && s == sub)
							SpeakCurrentItem();
						else if (c == ti)
							SpeakWithSubcategoryContext();
						else
							SpeakWithCategoryContext();
						return;
					}
				}
			}
		}

		protected override void NavigatePrev() {
			if (Level < 2) {
				base.NavigatePrev();
				return;
			}

			int cat = GetIndex(0);
			int ti = TreeIndex(cat);
			int sub = GetIndex(1);
			int bld = GetIndex(2);

			if (bld - 1 >= 0) {
				SetIndex(2, bld - 1);
				SyncCurrentIndex();
				PlayHoverSound();
				SpeakCurrentItem();
				return;
			}

			// Try previous subcategory in current category
			var subs = _tree[ti].Subcategories;
			for (int s = sub - 1; s >= 0; s--) {
				if (subs[s].Buildings.Count > 0) {
					SetIndex(1, s);
					SetIndex(2, subs[s].Buildings.Count - 1);
					SyncCurrentIndex();
					PlayHoverSound();
					SpeakWithSubcategoryContext();
					return;
				}
			}

			// Try previous categories
			for (int c = ti - 1; c >= 0; c--) {
				var prevSubs = _tree[c].Subcategories;
				for (int s = prevSubs.Count - 1; s >= 0; s--) {
					if (prevSubs[s].Buildings.Count > 0) {
						SetIndex(0, c + 1);
						SetIndex(1, s);
						SetIndex(2, prevSubs[s].Buildings.Count - 1);
						SyncCurrentIndex();
						PlayHoverSound();
						SpeakWithCategoryContext();
						return;
					}
				}
			}

			// Wrap to last building in the entire tree
			for (int c = _tree.Count - 1; c >= 0; c--) {
				var wrapSubs = _tree[c].Subcategories;
				for (int s = wrapSubs.Count - 1; s >= 0; s--) {
					if (wrapSubs[s].Buildings.Count > 0) {
						SetIndex(0, c + 1);
						SetIndex(1, s);
						SetIndex(2, wrapSubs[s].Buildings.Count - 1);
						SyncCurrentIndex();
						PlayWrapSound();
						if (c == ti && s == sub)
							SpeakCurrentItem();
						else if (c == ti)
							SpeakWithSubcategoryContext();
						else
							SpeakWithCategoryContext();
						return;
					}
				}
			}
		}

		protected override void NavigateFirst() {
			if (Level < 2) {
				base.NavigateFirst();
				return;
			}

			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++) {
					if (subs[s].Buildings.Count > 0) {
						SetIndex(0, c + 1);
						SetIndex(1, s);
						SetIndex(2, 0);
						SyncCurrentIndex();
						PlayHoverSound();
						SpeakWithCategoryContext();
						return;
					}
				}
			}
		}

		protected override void NavigateLast() {
			if (Level < 2) {
				base.NavigateLast();
				return;
			}

			for (int c = _tree.Count - 1; c >= 0; c--) {
				var subs = _tree[c].Subcategories;
				for (int s = subs.Count - 1; s >= 0; s--) {
					if (subs[s].Buildings.Count > 0) {
						SetIndex(0, c + 1);
						SetIndex(1, s);
						SetIndex(2, subs[s].Buildings.Count - 1);
						SyncCurrentIndex();
						PlayHoverSound();
						SpeakWithCategoryContext();
						return;
					}
				}
			}
		}

		// ========================================
		// LEVEL 2 GROUP JUMPING
		// ========================================

		protected override void JumpNextGroup() {
			if (Level < 2) {
				base.JumpNextGroup();
				return;
			}

			int ti = TreeIndex(GetIndex(0));
			int sub = GetIndex(1);

			if (FindNextSubcategory(ti, sub, out int nc, out int ns)) {
				SetIndex(0, nc + 1);
				SetIndex(1, ns);
				SetIndex(2, 0);
				SyncCurrentIndex();
				if (nc < ti || (nc == ti && ns <= sub)) PlayWrapSound();
				else PlayHoverSound();
				if (nc == ti)
					SpeakWithSubcategoryContext();
				else
					SpeakWithCategoryContext();
				return;
			}
		}

		protected override void JumpPrevGroup() {
			if (Level < 2) {
				base.JumpPrevGroup();
				return;
			}

			int ti = TreeIndex(GetIndex(0));
			int sub = GetIndex(1);

			if (FindPrevSubcategory(ti, sub, out int nc, out int ns)) {
				SetIndex(0, nc + 1);
				SetIndex(1, ns);
				SetIndex(2, 0);
				SyncCurrentIndex();
				if (nc > ti || (nc == ti && ns >= sub)) PlayWrapSound();
				else PlayHoverSound();
				if (nc == ti)
					SpeakWithSubcategoryContext();
				else
					SpeakWithCategoryContext();
				return;
			}
		}

		/// <summary>
		/// Find next non-empty subcategory scanning _tree indices.
		/// outCat/outSub are _tree indices (not level-0 indices).
		/// </summary>
		private bool FindNextSubcategory(int cat, int sub, out int outCat, out int outSub) {
			outCat = cat;
			outSub = sub;

			int c = cat;
			int s = sub + 1;
			while (true) {
				if (c >= _tree.Count) break;
				var subs = _tree[c].Subcategories;
				if (s < subs.Count) {
					if (subs[s].Buildings.Count > 0) {
						outCat = c;
						outSub = s;
						return true;
					}
					s++;
				} else {
					c++;
					s = 0;
				}
			}

			for (int wc = 0; wc < _tree.Count; wc++) {
				var subs = _tree[wc].Subcategories;
				for (int ws = 0; ws < subs.Count; ws++) {
					if (subs[ws].Buildings.Count > 0) {
						outCat = wc;
						outSub = ws;
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Find previous non-empty subcategory scanning _tree indices.
		/// outCat/outSub are _tree indices (not level-0 indices).
		/// </summary>
		private bool FindPrevSubcategory(int cat, int sub, out int outCat, out int outSub) {
			outCat = cat;
			outSub = sub;

			int c = cat;
			int s = sub - 1;
			while (true) {
				if (c < 0) break;
				if (s >= 0) {
					var subs = _tree[c].Subcategories;
					if (subs[s].Buildings.Count > 0) {
						outCat = c;
						outSub = s;
						return true;
					}
					s--;
				} else {
					c--;
					if (c >= 0)
						s = _tree[c].Subcategories.Count - 1;
				}
			}

			for (int wc = _tree.Count - 1; wc >= 0; wc--) {
				var subs = _tree[wc].Subcategories;
				for (int ws = subs.Count - 1; ws >= 0; ws--) {
					if (subs[ws].Buildings.Count > 0) {
						outCat = wc;
						outSub = ws;
						return true;
					}
				}
			}

			return false;
		}

		// ========================================
		// SEARCH
		// ========================================

		private int GetBuildingSearchCount() {
			if (_tree == null) return 0;
			int total = 0;
			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++)
					total += subs[s].Buildings.Count;
			}
			return total;
		}

		protected override int GetSearchItemCount(int[] indices) {
			return ToolHandler.AllTools.Count + GetBuildingSearchCount();
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			// Tools first
			var tools = ToolHandler.AllTools;
			if (flatIndex < tools.Count)
				return tools[flatIndex].Label;

			// Then buildings
			if (_tree == null) return null;
			int remaining = flatIndex - tools.Count;
			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++) {
					int count = subs[s].Buildings.Count;
					if (remaining < count)
						return subs[s].Buildings[remaining].Label;
					remaining -= count;
				}
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			// Tools first
			var tools = ToolHandler.AllTools;
			if (flatIndex < tools.Count) {
				outIndices[0] = ToolsCategoryIndex;
				outIndices[1] = flatIndex;
				outIndices[2] = 0;
				return;
			}

			// Then buildings
			if (_tree == null) return;
			int remaining = flatIndex - tools.Count;
			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++) {
					int count = subs[s].Buildings.Count;
					if (remaining < count) {
						outIndices[0] = c + 1;
						outIndices[1] = s;
						outIndices[2] = remaining;
						return;
					}
					remaining -= count;
				}
			}
		}

		protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) {
			if (IsToolsCategory(mappedIndices[0]))
				return 1;
			return SearchLevel;
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			PlayOpenSound();
			_tree = BuildMenuData.GetFullBuildTree();

			if (_initialDef != null && _restoreFlatIndex < 0)
				FindDefFlatIndex(_initialDef, _initialCategory);

			base.OnActivate();

			if (_restoreFlatIndex >= 0) {
				NestedSearchMoveTo(_restoreFlatIndex);
				_restoreFlatIndex = -1;
			} else {
				SpeechPipeline.SpeakQueued(
					(string)STRINGS.ONIACCESS.BUILD_MENU.TOOLS_CATEGORY);
			}
		}

		public override void OnDeactivate() {
			PlayCloseSound();
			base.OnDeactivate();
		}

		// ========================================
		// ESCAPE
		// ========================================

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;
			if (e.TryConsume(Action.Escape)) {
				SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.TOOLTIP.CLOSED);
				HandlerStack.Pop();
				return true;
			}
			return false;
		}

		// ========================================
		// PRIVATE HELPERS
		// ========================================

		private int _restoreFlatIndex = -1;

		private void FindDefFlatIndex(BuildingDef def, HashedString category) {
			if (_tree == null) return;
			int flat = ToolHandler.AllTools.Count;
			for (int c = 0; c < _tree.Count; c++) {
				bool categoryMatch = category.IsValid && _tree[c].Category == category;
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++) {
					var buildings = subs[s].Buildings;
					for (int b = 0; b < buildings.Count; b++) {
						if (buildings[b].Def == def && (categoryMatch || !category.IsValid)) {
							_restoreFlatIndex = flat;
							return;
						}
						flat++;
					}
				}
			}
			if (category.IsValid) {
				FindDefFlatIndex(def, HashedString.Invalid);
			}
		}

		private void SpeakWithSubcategoryContext() {
			int ti = TreeIndex(GetIndex(0));
			int sub = GetIndex(1);
			string subName = _tree[ti].Subcategories[sub].Name;
			SpeakCurrentItem(subName);
		}

		private void SpeakWithCategoryContext() {
			int ti = TreeIndex(GetIndex(0));
			int sub = GetIndex(1);
			string catName = _tree[ti].DisplayName;
			string subName = _tree[ti].Subcategories[sub].Name;
			SpeakCurrentItem(catName + ", " + subName);
		}

	}
}
