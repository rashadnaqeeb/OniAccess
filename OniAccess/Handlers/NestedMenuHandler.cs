using System.Collections.Generic;

using OniAccess.Speech;

namespace OniAccess.Handlers {
	/// <summary>
	/// Multi-level navigation on top of BaseMenuHandler's infrastructure.
	/// Level 0 is the root (e.g., subcategories), deeper levels are children
	/// (e.g., buildings within a subcategory). Navigation crosses parent
	/// boundaries at levels > 0: moving past the last child wraps into the
	/// next parent, and vice versa. Type-ahead searches a configurable level
	/// (SearchLevel) regardless of the current navigation level.
	/// </summary>
	public abstract class NestedMenuHandler: BaseMenuHandler, ISearchable {
		private int _level;
		private int[] _indices = new int[8];

		protected NestedMenuHandler(KScreen screen = null) : base(screen) { }

		protected override int CurrentIndex { get => _indices[_level]; set => _indices[_level] = value; }

		protected int Level { get => _level; set => _level = value; }
		protected virtual int StartLevel => 0;

		protected int GetIndex(int level) => _indices[level];
		protected void SetIndex(int level, int value) => _indices[level] = value;

		// ========================================
		// ABSTRACT: LEVEL-AWARE MEMBERS
		// ========================================

		/// <summary>
		/// Deepest navigable level (e.g., 1 for subcategory+building).
		/// </summary>
		protected abstract int MaxLevel { get; }

		/// <summary>
		/// Item count at a given level. Parent indices provide context
		/// (e.g., at level 1, indices[0] tells which subcategory).
		/// </summary>
		protected abstract int GetItemCount(int level, int[] indices);

		/// <summary>
		/// Label for the item at the given position.
		/// </summary>
		protected abstract string GetItemLabel(int level, int[] indices);

		/// <summary>
		/// Activate an item at MaxLevel.
		/// </summary>
		protected abstract void ActivateLeafItem(int[] indices);

		/// <summary>
		/// Which level type-ahead searches (e.g., 1 for buildings).
		/// </summary>
		protected abstract int SearchLevel { get; }

		/// <summary>
		/// Total searchable items across all parents at SearchLevel.
		/// </summary>
		protected abstract int GetSearchItemCount(int[] indices);

		/// <summary>
		/// Label for a flat search index at SearchLevel.
		/// </summary>
		protected abstract string GetSearchItemLabel(int flatIndex);

		/// <summary>
		/// Convert flat search index back to level indices.
		/// </summary>
		protected abstract void MapSearchIndex(int flatIndex, int[] outIndices);

		/// <summary>
		/// Label for the parent group at the given indices.
		/// Used when announcing group changes during cross-boundary navigation.
		/// </summary>
		protected abstract string GetParentLabel(int level, int[] indices);

		/// <summary>
		/// Returns the level to set when search lands on flatIndex.
		/// Defaults to SearchLevel. Override when some items are leaves
		/// at a shallower level (e.g., tools at level 1 in a 3-level menu).
		/// </summary>
		protected virtual int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) => SearchLevel;

		// ========================================
		// BASE CLASS BRIDGES
		// ========================================

		public sealed override int ItemCount => GetItemCount(_level, _indices);

		public sealed override string GetItemLabel(int index) {
			int saved = _indices[_level];
			_indices[_level] = index;
			string label = GetItemLabel(_level, _indices);
			_indices[_level] = saved;
			return label;
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			int count = GetItemCount(_level, _indices);
			if (count == 0) return;
			string label = GetItemLabel(_level, _indices);
			if (string.IsNullOrWhiteSpace(label)) return;
			if (!string.IsNullOrEmpty(parentContext))
				label = parentContext + ", " + label;
			SpeechPipeline.SpeakInterrupt(label);
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		public override void OnActivate() {
			_level = StartLevel;
			for (int i = 0; i < _indices.Length; i++)
				_indices[i] = 0;
			base.OnActivate();
		}

		public override void OnDeactivate() {
			_level = 0;
			for (int i = 0; i < _indices.Length; i++)
				_indices[i] = 0;
			base.OnDeactivate();
		}

		// ========================================
		// NAVIGATION OVERRIDES
		// ========================================

		protected override void NavigateNext() {
			if (_level == 0) {
				base.NavigateNext();
				return;
			}

			int count = GetItemCount(_level, _indices);
			if (count == 0) return;

			int nextIndex = _indices[_level] + 1;
			if (nextIndex < count) {
				_indices[_level] = nextIndex;
				PlaySound("HUD_Mouseover");
				SpeakCurrentItem();
			} else {
				JumpToNextParent(landOnLast: false);
			}
		}

		protected override void NavigatePrev() {
			if (_level == 0) {
				base.NavigatePrev();
				return;
			}

			int prevIndex = _indices[_level] - 1;
			if (prevIndex >= 0) {
				_indices[_level] = prevIndex;
				PlaySound("HUD_Mouseover");
				SpeakCurrentItem();
			} else {
				JumpToPrevParent(landOnLast: true);
			}
		}

		protected override void NavigateFirst() {
			if (_level == 0) {
				base.NavigateFirst();
				return;
			}

			int count = GetItemCount(_level, _indices);
			if (count > 0) {
				_indices[_level] = 0;
				PlaySound("HUD_Mouseover");
				SpeakCurrentItem();
			}
		}

		protected override void NavigateLast() {
			if (_level == 0) {
				base.NavigateLast();
				return;
			}

			int count = GetItemCount(_level, _indices);
			if (count > 0) {
				_indices[_level] = count - 1;
				PlaySound("HUD_Mouseover");
				SpeakCurrentItem();
			}
		}

		// ========================================
		// LEFT/RIGHT: DRILL DOWN / GO BACK
		// ========================================

		protected override void HandleLeftRight(int direction, int stepLevel) {
			if (direction > 0 && _level < MaxLevel && CanDrillDown()) {
				DrillDown();
			} else if (direction < 0 && _level > 0) {
				GoBack();
			}
		}

		// ========================================
		// ENTER / ESCAPE
		// ========================================

		protected override void ActivateCurrentItem() {
			if (ItemCount == 0) return;
			if (_level < MaxLevel && CanDrillDown())
				DrillDown();
			else
				ActivateLeafItem(_indices);
		}

		/// <summary>
		/// Whether the current item has children at the next level.
		/// </summary>
		private bool CanDrillDown() {
			return GetItemCount(_level + 1, _indices) > 0;
		}

		// ========================================
		// SEARCH: explicit ISearchable re-implementation
		// TypeAheadSearch receives this as ISearchable. These explicit
		// members route search to the correct level and update _indices
		// and _level, which the base class public members don't do.
		// ========================================

		int ISearchable.SearchItemCount => GetSearchItemCount(_indices);

		string ISearchable.GetSearchLabel(int index) {
			string label = GetSearchItemLabel(index);
			if (label == null) return null;
			return TextFilter.FilterForSpeech(label);
		}

		void ISearchable.SearchMoveTo(int index) {
			NestedSearchMoveTo(index, parentContext: false);
		}

		protected void NestedSearchMoveTo(int index, bool parentContext = true) {
			MapSearchIndex(index, _indices);
			_level = GetSearchTargetLevel(index, _indices);
			if (parentContext)
				SpeakWithParentContext();
			else
				SpeakCurrentItem();
		}

		// ========================================
		// HELP ENTRIES
		// ========================================

		protected static readonly List<HelpEntry> NestedNavHelpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Ctrl+Up/Down", STRINGS.ONIACCESS.HELP.JUMP_GROUP),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter/Right", STRINGS.ONIACCESS.HELP.OPEN_GROUP),
			new HelpEntry("Left", STRINGS.ONIACCESS.HELP.GO_BACK),
		};

		// ========================================
		// GROUP JUMPING
		// ========================================

		protected override void JumpNextGroup() {
			if (_level == 0) { NavigateNext(); return; }
			JumpToNextParent(landOnLast: false);
		}

		protected override void JumpPrevGroup() {
			if (_level == 0) { NavigatePrev(); return; }
			JumpToPrevParent(landOnLast: false);
		}

		private bool JumpToNextParent(bool landOnLast) {
			int parentCount = GetItemCount(_level - 1, _indices);
			if (parentCount == 0) return false;
			int startParent = _indices[_level - 1];

			int next = (startParent + 1) % parentCount;
			_indices[_level - 1] = next;
			int childCount = GetItemCount(_level, _indices);
			if (childCount > 0) {
				_indices[_level] = landOnLast ? childCount - 1 : 0;
				if (next <= startParent) PlaySound("HUD_Click");
				else PlaySound("HUD_Mouseover");
				if (next == startParent) SpeakCurrentItem();
				else SpeakWithParentContext();
				return true;
			}

			// Neighbor empty — scan forward for next populated parent
			for (int step = 2; step < parentCount; step++) {
				int i = (startParent + step) % parentCount;
				_indices[_level - 1] = i;
				childCount = GetItemCount(_level, _indices);
				if (childCount > 0) {
					_indices[_level] = landOnLast ? childCount - 1 : 0;
					if (i <= startParent) PlaySound("HUD_Click");
					else PlaySound("HUD_Mouseover");
					if (i == startParent) SpeakCurrentItem();
					else SpeakWithParentContext();
					return true;
				}
			}

			_indices[_level - 1] = startParent;
			return false;
		}

		private bool JumpToPrevParent(bool landOnLast) {
			int parentCount = GetItemCount(_level - 1, _indices);
			if (parentCount == 0) return false;
			int startParent = _indices[_level - 1];

			int prev = (startParent - 1 + parentCount) % parentCount;
			_indices[_level - 1] = prev;
			int childCount = GetItemCount(_level, _indices);
			if (childCount > 0) {
				_indices[_level] = landOnLast ? childCount - 1 : 0;
				if (prev >= startParent) PlaySound("HUD_Click");
				else PlaySound("HUD_Mouseover");
				if (prev == startParent) SpeakCurrentItem();
				else SpeakWithParentContext();
				return true;
			}

			// Neighbor empty — scan backward for next populated parent
			for (int step = 2; step < parentCount; step++) {
				int i = (startParent - step + parentCount) % parentCount;
				_indices[_level - 1] = i;
				childCount = GetItemCount(_level, _indices);
				if (childCount > 0) {
					_indices[_level] = landOnLast ? childCount - 1 : 0;
					if (i >= startParent) PlaySound("HUD_Click");
					else PlaySound("HUD_Mouseover");
					if (i == startParent) SpeakCurrentItem();
					else SpeakWithParentContext();
					return true;
				}
			}

			_indices[_level - 1] = startParent;
			return false;
		}

		// ========================================
		// PRIVATE HELPERS
		// ========================================

		private void DrillDown() {
			_level++;
			_indices[_level] = 0;
			_search.Clear();

			int count = GetItemCount(_level, _indices);
			if (count > 0)
				SpeakCurrentItem();
		}

		private void GoBack() {
			_level--;
			_search.Clear();
			SpeakCurrentItem();
		}

		protected void ResetState() {
			_level = StartLevel;
			for (int i = 0; i < _indices.Length; i++)
				_indices[i] = 0;
			_search.Clear();
			SuppressSearchThisFrame();
		}

		private void SpeakWithParentContext() {
			string parentLabel = GetParentLabel(_level, _indices);
			SpeakCurrentItem(parentLabel);
		}
	}
}
