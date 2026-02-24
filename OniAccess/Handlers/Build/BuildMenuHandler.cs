using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Three-level navigation: categories (level 0), subcategories (level 1),
	/// and buildings (level 2). Enter/Right drills down; Left goes back.
	/// Up/Down at level 2 crosses both subcategory and category boundaries.
	/// Type-ahead always searches buildings (level 2).
	/// </summary>
	public class BuildMenuHandler: NestedMenuHandler {
		private readonly HashedString _initialCategory;
		private readonly BuildingDef _initialDef;
		private List<BuildMenuData.CategoryGroup> _tree;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries;

		static BuildMenuHandler() {
			var list = new List<HelpEntry>();
			list.AddRange(NestedNavHelpEntries);
			list.Add(new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE));
			_helpEntries = list.AsReadOnly();
		}

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public override string DisplayName => (string)STRINGS.ONIACCESS.BUILD_MENU.CATEGORY_LIST;

		/// <summary>
		/// Open fresh from the tile cursor.
		/// </summary>
		public BuildMenuHandler() {
			_initialCategory = HashedString.Invalid;
			_initialDef = null;
		}

		/// <summary>
		/// Return from placement (Tab in BuildToolHandler). Cursor starts on
		/// the building matching initialDef within the given category.
		/// </summary>
		public BuildMenuHandler(HashedString category, BuildingDef initialDef) {
			_initialCategory = category;
			_initialDef = initialDef;
		}

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 2;

		protected override int GetItemCount(int level, int[] indices) {
			if (_tree == null) return 0;
			if (level == 0) return _tree.Count;
			if (indices[0] < 0 || indices[0] >= _tree.Count) return 0;
			var subs = _tree[indices[0]].Subcategories;
			if (level == 1) return subs.Count;
			if (indices[1] < 0 || indices[1] >= subs.Count) return 0;
			return subs[indices[1]].Buildings.Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (_tree == null) return null;
			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= _tree.Count) return null;
				return _tree[indices[0]].DisplayName;
			}
			if (indices[0] < 0 || indices[0] >= _tree.Count) return null;
			var subs = _tree[indices[0]].Subcategories;
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
			if (level == 1) {
				if (indices[0] < 0 || indices[0] >= _tree.Count) return null;
				return _tree[indices[0]].DisplayName;
			}
			if (level == 2) {
				if (indices[0] < 0 || indices[0] >= _tree.Count) return null;
				var subs = _tree[indices[0]].Subcategories;
				if (indices[1] < 0 || indices[1] >= subs.Count) return null;
				return subs[indices[1]].Name;
			}
			return null;
		}

		protected override void ActivateLeafItem(int[] indices) {
			if (_tree == null) return;
			if (indices[0] < 0 || indices[0] >= _tree.Count) return;
			var subs = _tree[indices[0]].Subcategories;
			if (indices[1] < 0 || indices[1] >= subs.Count) return;
			var buildings = subs[indices[1]].Buildings;
			if (indices[2] < 0 || indices[2] >= buildings.Count) return;

			var entry = buildings[indices[2]];
			var category = _tree[indices[0]].Category;

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

		// ========================================
		// LEVEL 2 CROSS-BOUNDARY NAVIGATION
		// ========================================

		protected override void NavigateNext() {
			if (Level < 2) {
				base.NavigateNext();
				return;
			}

			int cat = GetIndex(0);
			int sub = GetIndex(1);
			int bld = GetIndex(2);

			var subs = _tree[cat].Subcategories;
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
			for (int c = cat + 1; c < _tree.Count; c++) {
				var nextSubs = _tree[c].Subcategories;
				for (int s = 0; s < nextSubs.Count; s++) {
					if (nextSubs[s].Buildings.Count > 0) {
						SetIndex(0, c);
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
						SetIndex(0, c);
						SetIndex(1, s);
						SetIndex(2, 0);
						SyncCurrentIndex();
						PlayWrapSound();
						if (c == cat && s == sub)
							SpeakCurrentItem();
						else if (c == cat)
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
			var subs = _tree[cat].Subcategories;
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
			for (int c = cat - 1; c >= 0; c--) {
				var prevSubs = _tree[c].Subcategories;
				for (int s = prevSubs.Count - 1; s >= 0; s--) {
					if (prevSubs[s].Buildings.Count > 0) {
						SetIndex(0, c);
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
						SetIndex(0, c);
						SetIndex(1, s);
						SetIndex(2, wrapSubs[s].Buildings.Count - 1);
						SyncCurrentIndex();
						PlayWrapSound();
						if (c == cat && s == sub)
							SpeakCurrentItem();
						else if (c == cat)
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
						SetIndex(0, c);
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
						SetIndex(0, c);
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

			int cat = GetIndex(0);
			int sub = GetIndex(1);

			// Check immediate next subcategory (may cross category boundary)
			if (FindNextSubcategory(cat, sub, out int nc, out int ns)) {
				SetIndex(0, nc);
				SetIndex(1, ns);
				SetIndex(2, 0);
				SyncCurrentIndex();
				if (nc < cat || (nc == cat && ns <= sub)) PlayWrapSound();
				else PlayHoverSound();
				if (nc == cat)
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

			int cat = GetIndex(0);
			int sub = GetIndex(1);

			// Check immediate previous subcategory (may cross category boundary)
			if (FindPrevSubcategory(cat, sub, out int nc, out int ns)) {
				SetIndex(0, nc);
				SetIndex(1, ns);
				SetIndex(2, 0);
				SyncCurrentIndex();
				if (nc > cat || (nc == cat && ns >= sub)) PlayWrapSound();
				else PlayHoverSound();
				if (nc == cat)
					SpeakWithSubcategoryContext();
				else
					SpeakWithCategoryContext();
				return;
			}
		}

		private bool FindNextSubcategory(int cat, int sub, out int outCat, out int outSub) {
			outCat = cat;
			outSub = sub;

			// Scan forward from current position
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

			// Wrap: scan from the beginning
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

		private bool FindPrevSubcategory(int cat, int sub, out int outCat, out int outSub) {
			outCat = cat;
			outSub = sub;

			// Scan backward from current position
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

			// Wrap: scan from the end
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

		protected override int GetSearchItemCount(int[] indices) {
			if (_tree == null) return 0;
			int total = 0;
			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++)
					total += subs[s].Buildings.Count;
			}
			return total;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			if (_tree == null) return null;
			int remaining = flatIndex;
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
			if (_tree == null) return;
			int remaining = flatIndex;
			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++) {
					int count = subs[s].Buildings.Count;
					if (remaining < count) {
						outIndices[0] = c;
						outIndices[1] = s;
						outIndices[2] = remaining;
						return;
					}
					remaining -= count;
				}
			}
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
			} else if (_tree != null && _tree.Count > 0) {
				SpeechPipeline.SpeakQueued(_tree[0].DisplayName);
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
			int flat = 0;
			for (int c = 0; c < _tree.Count; c++) {
				// When returning from placement, prefer the exact category
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
			// If exact category not found, try any category
			if (category.IsValid) {
				FindDefFlatIndex(def, HashedString.Invalid);
			}
		}

		private void SpeakWithSubcategoryContext() {
			int cat = GetIndex(0);
			int sub = GetIndex(1);
			string subName = _tree[cat].Subcategories[sub].Name;
			SpeakCurrentItem(subName);
		}

		private void SpeakWithCategoryContext() {
			int cat = GetIndex(0);
			int sub = GetIndex(1);
			string catName = _tree[cat].DisplayName;
			string subName = _tree[cat].Subcategories[sub].Name;
			SpeakCurrentItem(catName + ", " + subName);
		}

		private static void PlayOpenSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Open");
		}

		private static void PlayCloseSound() {
			Tools.ToolPickerHandler.PlaySound("HUD_Click_Close");
		}

		private static void PlayNegativeSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Negative"));
			} catch (System.Exception ex) {
				Util.Log.Error($"BuildMenuHandler.PlayNegativeSound: {ex.Message}");
			}
		}
	}
}
