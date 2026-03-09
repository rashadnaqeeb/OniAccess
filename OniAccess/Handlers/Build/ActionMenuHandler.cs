using System.Collections.Generic;
using OniAccess.Handlers.Sandbox;
using OniAccess.Handlers.Tools;
using OniAccess.Speech;

namespace OniAccess.Handlers.Build {
	/// <summary>
	/// Unified action menu combining tools and build categories.
	/// Tools appear first at level 0 index 0 with individual tools at
	/// level 1 (leaf). When sandbox mode is active, Sandbox Tools appears
	/// at index 1 with sandbox tools at level 1 (leaf). Build categories
	/// follow at the remaining indices, with subcategories (level 1) and
	/// buildings (level 2).
	/// Type-ahead searches tools, sandbox tools (when active), and buildings.
	/// </summary>
	public class ActionMenuHandler: NestedMenuHandler {
		private readonly HashedString _initialCategory;
		private readonly BuildingDef _initialDef;
		private List<BuildMenuData.CategoryGroup> _tree;

		private const int ToolsCategoryIndex = 0;
		private bool _sandboxActive;
		private int _sandboxCategoryIndex = -1;
		// Number of fixed categories before build categories (1 or 2)
		private int _fixedCategoryCount = 1;

		private bool IsSandboxCategory(int catIndex) =>
			_sandboxActive && catIndex == _sandboxCategoryIndex;

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
		/// Open focused on a specific category (e.g., from tutorial notification).
		/// Cursor starts on the category at level 0.
		/// </summary>
		public ActionMenuHandler(HashedString category) {
			_initialCategory = category;
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
		/// Build categories start after the fixed categories (Tools, optionally Sandbox).
		/// </summary>
		private int TreeIndex(int catIndex) => catIndex - _fixedCategoryCount;

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int MaxLevel => 2;
		protected override int SearchLevel => 2;

		protected override int GetItemCount(int level, int[] indices) {
			if (_tree == null) return 0;
			if (level == 0) return _tree.Count + _fixedCategoryCount;

			if (IsToolsCategory(indices[0])) {
				if (level == 1) return ToolHandler.AllTools.Count;
				return 0;
			}

			if (IsSandboxCategory(indices[0])) {
				if (level == 1) return GetSandboxToolInfos().Count;
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
				if (IsSandboxCategory(indices[0]))
					return (string)STRINGS.ONIACCESS.SANDBOX.TOOLS_CATEGORY;
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

			if (IsSandboxCategory(indices[0])) {
				if (level == 1) {
					var sbTools = GetSandboxToolInfos();
					if (indices[1] < 0 || indices[1] >= sbTools.Count) return null;
					return sbTools[indices[1]].text;
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

			if (IsSandboxCategory(indices[0])) {
				if (level == 1)
					return (string)STRINGS.ONIACCESS.SANDBOX.TOOLS_CATEGORY;
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

			if (IsSandboxCategory(indices[0])) {
				ActivateSandboxToolItem(indices[1]);
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
				PlaySound("Negative");
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

		private void ActivateSandboxToolItem(int toolIndex) {
			var sbTools = GetSandboxToolInfos();
			if (toolIndex < 0 || toolIndex >= sbTools.Count) return;

			var ti = sbTools[toolIndex];
			ActivateSandboxTool(ti);
			HandlerStack.Replace(new SandboxToolHandler());
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
				PlaySound("HUD_Mouseover");
				SpeakCurrentItem();
				return;
			}

			// Try next subcategory in current category
			for (int s = sub + 1; s < subs.Count; s++) {
				if (subs[s].Buildings.Count > 0) {
					SetIndex(1, s);
					SetIndex(2, 0);
					PlaySound("HUD_Mouseover");
					SpeakWithSubcategoryContext();
					return;
				}
			}

			// Try next categories
			for (int c = ti + 1; c < _tree.Count; c++) {
				var nextSubs = _tree[c].Subcategories;
				for (int s = 0; s < nextSubs.Count; s++) {
					if (nextSubs[s].Buildings.Count > 0) {
						SetIndex(0, c + _fixedCategoryCount);
						SetIndex(1, s);
						SetIndex(2, 0);
						PlaySound("HUD_Mouseover");
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
						SetIndex(0, c + _fixedCategoryCount);
						SetIndex(1, s);
						SetIndex(2, 0);
						PlaySound("HUD_Click");
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
				PlaySound("HUD_Mouseover");
				SpeakCurrentItem();
				return;
			}

			// Try previous subcategory in current category
			var subs = _tree[ti].Subcategories;
			for (int s = sub - 1; s >= 0; s--) {
				if (subs[s].Buildings.Count > 0) {
					SetIndex(1, s);
					SetIndex(2, subs[s].Buildings.Count - 1);
					PlaySound("HUD_Mouseover");
					SpeakWithSubcategoryContext();
					return;
				}
			}

			// Try previous categories
			for (int c = ti - 1; c >= 0; c--) {
				var prevSubs = _tree[c].Subcategories;
				for (int s = prevSubs.Count - 1; s >= 0; s--) {
					if (prevSubs[s].Buildings.Count > 0) {
						SetIndex(0, c + _fixedCategoryCount);
						SetIndex(1, s);
						SetIndex(2, prevSubs[s].Buildings.Count - 1);
						PlaySound("HUD_Mouseover");
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
						SetIndex(0, c + _fixedCategoryCount);
						SetIndex(1, s);
						SetIndex(2, wrapSubs[s].Buildings.Count - 1);
						PlaySound("HUD_Click");
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
				SetIndex(0, nc + _fixedCategoryCount);
				SetIndex(1, ns);
				SetIndex(2, 0);
				if (nc < ti || (nc == ti && ns <= sub)) PlaySound("HUD_Click");
				else PlaySound("HUD_Mouseover");
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
				SetIndex(0, nc + _fixedCategoryCount);
				SetIndex(1, ns);
				SetIndex(2, 0);
				if (nc > ti || (nc == ti && ns >= sub)) PlaySound("HUD_Click");
				else PlaySound("HUD_Mouseover");
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

		private int SandboxSearchCount => _sandboxActive ? GetSandboxToolInfos().Count : 0;

		protected override int GetSearchItemCount(int[] indices) {
			return ToolHandler.AllTools.Count + SandboxSearchCount + GetBuildingSearchCount();
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			// Tools first
			var tools = ToolHandler.AllTools;
			if (flatIndex < tools.Count)
				return tools[flatIndex].Label;
			int remaining = flatIndex - tools.Count;

			// Then sandbox tools
			if (_sandboxActive) {
				var sbTools = GetSandboxToolInfos();
				if (remaining < sbTools.Count)
					return sbTools[remaining].text;
				remaining -= sbTools.Count;
			}

			// Then buildings
			if (_tree == null) return null;
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
			int remaining = flatIndex - tools.Count;

			// Then sandbox tools
			if (_sandboxActive) {
				var sbTools = GetSandboxToolInfos();
				if (remaining < sbTools.Count) {
					outIndices[0] = _sandboxCategoryIndex;
					outIndices[1] = remaining;
					outIndices[2] = 0;
					return;
				}
				remaining -= sbTools.Count;
			}

			// Then buildings
			if (_tree == null) return;
			for (int c = 0; c < _tree.Count; c++) {
				var subs = _tree[c].Subcategories;
				for (int s = 0; s < subs.Count; s++) {
					int count = subs[s].Buildings.Count;
					if (remaining < count) {
						outIndices[0] = c + _fixedCategoryCount;
						outIndices[1] = s;
						outIndices[2] = remaining;
						return;
					}
					remaining -= count;
				}
			}
		}

		protected override int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) {
			if (IsToolsCategory(mappedIndices[0]) || IsSandboxCategory(mappedIndices[0]))
				return 1;
			return SearchLevel;
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			PlaySound("HUD_Click_Open");
			_tree = BuildMenuData.GetFullBuildTree();
			_sandboxActive = Game.Instance != null && Game.Instance.SandboxModeActive;
			_sandboxCategoryIndex = _sandboxActive ? 1 : -1;
			_fixedCategoryCount = _sandboxActive ? 2 : 1;

			if (_initialDef != null && _restoreFlatIndex < 0)
				FindDefFlatIndex(_initialDef, _initialCategory);

			base.OnActivate();

			if (_restoreFlatIndex >= 0) {
				NestedSearchMoveTo(_restoreFlatIndex);
				_restoreFlatIndex = -1;
			} else if (_initialCategory.IsValid && _initialDef == null) {
				MoveToCategory(_initialCategory);
			} else {
				SpeechPipeline.SpeakQueued(
					(string)STRINGS.ONIACCESS.BUILD_MENU.TOOLS_CATEGORY);
			}
		}

		public override void OnDeactivate() {
			PlaySound("HUD_Click_Close");
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
			int flat = ToolHandler.AllTools.Count + SandboxSearchCount;
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

		private void MoveToCategory(HashedString category) {
			for (int c = 0; c < _tree.Count; c++) {
				if (_tree[c].Category == category) {
					SetIndex(0, c + _fixedCategoryCount);
					Level = 0;
					SpeakCurrentItem();
					return;
				}
			}
			// Category not found (e.g., all buildings behind unresearched tech).
			// Fall back to Tools.
			SpeechPipeline.SpeakQueued(
				(string)STRINGS.ONIACCESS.BUILD_MENU.TOOLS_CATEGORY);
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

		// ========================================
		// SANDBOX TOOL HELPERS
		// ========================================

		/// <summary>
		/// Build a flat list of ToolInfo from all sandbox tool collections.
		/// Re-queries ToolMenu each call to avoid caching game state.
		/// </summary>
		private static List<ToolMenu.ToolInfo> GetSandboxToolInfos() {
			var list = new List<ToolMenu.ToolInfo>();
			if (ToolMenu.Instance != null) {
				foreach (var collection in ToolMenu.Instance.sandboxTools)
					foreach (var ti in collection.tools)
						list.Add(ti);
			}
			return list;
		}

		private static void ActivateSandboxTool(ToolMenu.ToolInfo ti) {
			ToolPickerHandler.ActivateSandboxTool(ti);
		}

	}
}
