using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Handlers.Sandbox {
	/// <summary>
	/// Modal nested menu for sandbox selectors. Adapts to categorized selectors
	/// (Element, Entity: MaxLevel=1, SearchLevel=1) or flat selectors
	/// (Disease, Story: MaxLevel=0, SearchLevel=0).
	///
	/// Categorized: level 0 = filter categories, level 1 = items within category.
	/// Flat: level 0 = all items.
	///
	/// Enter selects the item and pops back to the parameter menu.
	/// </summary>
	public class SandboxSelectorHandler: NestedMenuHandler {
		private readonly SandboxToolParameterMenu.SelectorValue _selector;
		private readonly bool _hasCategories;

		// Filtered option lists per category (or one list for flat selectors)
		private List<List<object>> _categoryOptions;
		private List<string> _categoryNames;

		public override string DisplayName => _selector.labelText;

		private static readonly IReadOnlyList<HelpEntry> _helpEntries;

		static SandboxSelectorHandler() {
			var list = new List<HelpEntry>();
			list.AddRange(NestedNavHelpEntries);
			list.Add(new HelpEntry("Escape", STRINGS.ONIACCESS.HELP.CLOSE));
			_helpEntries = list.AsReadOnly();
		}

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		public SandboxSelectorHandler(SandboxToolParameterMenu.SelectorValue selector)
			: base(null) {
			_selector = selector;
			_hasCategories = selector.filters != null && selector.filters.Length > 0;
		}

		// ========================================
		// NESTED MENU ABSTRACTS
		// ========================================

		protected override int MaxLevel => _hasCategories ? 1 : 0;
		protected override int SearchLevel => _hasCategories ? 1 : 0;

		protected override int GetItemCount(int level, int[] indices) {
			if (_categoryOptions == null) return 0;

			if (!_hasCategories)
				return _categoryOptions.Count > 0 ? _categoryOptions[0].Count : 0;

			if (level == 0) return _categoryOptions.Count;
			if (indices[0] < 0 || indices[0] >= _categoryOptions.Count) return 0;
			return _categoryOptions[indices[0]].Count;
		}

		protected override string GetItemLabel(int level, int[] indices) {
			if (_categoryOptions == null) return null;

			if (!_hasCategories) {
				if (_categoryOptions.Count == 0) return null;
				var items = _categoryOptions[0];
				int idx = indices[0];
				if (idx < 0 || idx >= items.Count) return null;
				return _selector.getOptionName(items[idx]);
			}

			if (level == 0) {
				if (indices[0] < 0 || indices[0] >= _categoryNames.Count) return null;
				return _categoryNames[indices[0]];
			}

			int cat = indices[0];
			if (cat < 0 || cat >= _categoryOptions.Count) return null;
			var catItems = _categoryOptions[cat];
			int itemIdx = indices[1];
			if (itemIdx < 0 || itemIdx >= catItems.Count) return null;
			return _selector.getOptionName(catItems[itemIdx]);
		}

		protected override string GetParentLabel(int level, int[] indices) {
			if (!_hasCategories || level == 0) return null;
			if (indices[0] < 0 || indices[0] >= _categoryNames.Count) return null;
			return _categoryNames[indices[0]];
		}

		protected override void ActivateLeafItem(int[] indices) {
			object selected = GetSelectedOption(indices);
			if (selected == null) return;

			_selector.onValueChanged(selected);
			SpeechPipeline.SpeakInterrupt((string)STRINGS.ONIACCESS.STATES.SELECTED);
			HandlerStack.Pop();
		}

		// ========================================
		// SEARCH
		// ========================================

		private int _flatSearchCount = -1;

		protected override int GetSearchItemCount(int[] indices) {
			if (_flatSearchCount >= 0) return _flatSearchCount;
			int total = 0;
			if (_categoryOptions != null)
				foreach (var list in _categoryOptions)
					total += list.Count;
			_flatSearchCount = total;
			return total;
		}

		protected override string GetSearchItemLabel(int flatIndex) {
			if (_categoryOptions == null) return null;
			int remaining = flatIndex;
			foreach (var list in _categoryOptions) {
				if (remaining < list.Count)
					return _selector.getOptionName(list[remaining]);
				remaining -= list.Count;
			}
			return null;
		}

		protected override void MapSearchIndex(int flatIndex, int[] outIndices) {
			if (_categoryOptions == null) return;

			if (!_hasCategories) {
				outIndices[0] = flatIndex;
				return;
			}

			int remaining = flatIndex;
			for (int c = 0; c < _categoryOptions.Count; c++) {
				int count = _categoryOptions[c].Count;
				if (remaining < count) {
					outIndices[0] = c;
					outIndices[1] = remaining;
					return;
				}
				remaining -= count;
			}
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			BuildOptionLists();
			PlaySound("HUD_Click_Open");
			base.OnActivate();
			if (!_hasCategories && _categoryOptions.Count > 0 && _categoryOptions[0].Count > 0)
				SpeechPipeline.SpeakQueued(GetItemLabel(0, new int[] { 0 }));
			else if (_hasCategories && _categoryOptions.Count > 0)
				SpeechPipeline.SpeakQueued(_categoryNames[0]);
		}

		public override void OnDeactivate() {
			PlaySound("HUD_Click_Close");
			base.OnDeactivate();
		}

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

		private void BuildOptionLists() {
			_categoryOptions = new List<List<object>>();
			_categoryNames = new List<string>();
			_flatSearchCount = -1;

			if (!_hasCategories) {
				// Flat list: all options in one list
				var all = new List<object>();
				if (_selector.options != null) {
					foreach (var opt in _selector.options)
						all.Add(opt);
				}
				_categoryOptions.Add(all);
				return;
			}

			// Categorized: build one list per top-level filter
			// Only include top-level filters (parentFilter == null).
			// Sub-filters (e.g., creature species under Creatures) are
			// treated as separate categories for flat navigation.
			foreach (var filter in _selector.filters) {
				var items = new List<object>();
				if (_selector.options != null) {
					foreach (var opt in _selector.options) {
						if (filter.condition(opt))
							items.Add(opt);
					}
				}
				// Include category even if empty (for consistent indexing)
				// but NestedMenuHandler will skip empty categories during navigation
				_categoryOptions.Add(items);
				_categoryNames.Add(filter.Name);
			}
		}

		private object GetSelectedOption(int[] indices) {
			if (_categoryOptions == null) return null;

			if (!_hasCategories) {
				if (_categoryOptions.Count == 0) return null;
				var items = _categoryOptions[0];
				int idx = indices[0];
				if (idx < 0 || idx >= items.Count) return null;
				return items[idx];
			}

			int cat = indices[0];
			if (cat < 0 || cat >= _categoryOptions.Count) return null;
			var catItems = _categoryOptions[cat];
			int itemIdx = indices[1];
			if (itemIdx < 0 || itemIdx >= catItems.Count) return null;
			return catItems[itemIdx];
		}
	}
}
