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

		protected int Level => _level;

		protected int GetIndex(int level) => _indices[level];
		protected void SetIndex(int level, int value) => _indices[level] = value;
		protected void SetLevel(int level) { _level = level; }

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
			// Base class methods (NavigateNext/Prev/First/Last) update
			// _currentIndex directly before calling SpeakCurrentItem.
			// Sync _indices so label lookup reads the correct position.
			_indices[_level] = _currentIndex;
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
			_level = 0;
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
				SyncFromCurrentIndex();
				return;
			}

			int count = GetItemCount(_level, _indices);
			if (count == 0) return;

			int nextIndex = _indices[_level] + 1;
			if (nextIndex < count) {
				_indices[_level] = nextIndex;
				SyncCurrentIndex();
				PlayHoverSound();
				SpeakCurrentItem();
			} else {
				JumpToNextParent(landOnLast: false);
			}
		}

		protected override void NavigatePrev() {
			if (_level == 0) {
				base.NavigatePrev();
				SyncFromCurrentIndex();
				return;
			}

			int prevIndex = _indices[_level] - 1;
			if (prevIndex >= 0) {
				_indices[_level] = prevIndex;
				SyncCurrentIndex();
				PlayHoverSound();
				SpeakCurrentItem();
			} else {
				JumpToPrevParent(landOnLast: true);
			}
		}

		protected override void NavigateFirst() {
			if (_level == 0) {
				base.NavigateFirst();
				SyncFromCurrentIndex();
				return;
			}

			int parentCount = GetItemCount(_level - 1, _indices);
			for (int i = 0; i < parentCount; i++) {
				_indices[_level - 1] = i;
				int childCount = GetItemCount(_level, _indices);
				if (childCount > 0) {
					_indices[_level] = 0;
					SyncCurrentIndex();
					PlayHoverSound();
					SpeakWithParentContext();
					return;
				}
			}
		}

		protected override void NavigateLast() {
			if (_level == 0) {
				base.NavigateLast();
				SyncFromCurrentIndex();
				return;
			}

			int parentCount = GetItemCount(_level - 1, _indices);
			for (int i = parentCount - 1; i >= 0; i--) {
				_indices[_level - 1] = i;
				int childCount = GetItemCount(_level, _indices);
				if (childCount > 0) {
					_indices[_level] = childCount - 1;
					SyncCurrentIndex();
					PlayHoverSound();
					SpeakWithParentContext();
					return;
				}
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

		public override bool HandleKeyDown(KButtonEvent e) {
			if (base.HandleKeyDown(e))
				return true;

			if (_level > 0 && e.TryConsume(Action.Escape)) {
				GoBack();
				return true;
			}

			return false;
		}

		// ========================================
		// SEARCH: explicit ISearchable re-implementation
		// TypeAheadSearch receives this as ISearchable. If these explicit
		// members are removed, the base class public members are called
		// instead, which only set _currentIndex without updating _indices
		// or _level, corrupting nested navigation state.
		// ========================================

		int ISearchable.SearchItemCount => GetSearchItemCount(_indices);

		int ISearchable.SearchCurrentIndex {
			get {
				if (_level == SearchLevel)
					return _indices[_level];
				return 0;
			}
		}

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
			_level = SearchLevel;
			SyncCurrentIndex();
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
				SyncCurrentIndex();
				if (next <= startParent) PlayWrapSound();
				else PlayHoverSound();
				if (next == startParent) SpeakCurrentItem();
				else SpeakWithParentContext();
				return true;
			}

			// Neighbor empty — wrap to first populated parent
			for (int i = 0; i < parentCount; i++) {
				_indices[_level - 1] = i;
				childCount = GetItemCount(_level, _indices);
				if (childCount > 0) {
					_indices[_level] = landOnLast ? childCount - 1 : 0;
					SyncCurrentIndex();
					PlayWrapSound();
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
				SyncCurrentIndex();
				if (prev >= startParent) PlayWrapSound();
				else PlayHoverSound();
				if (prev == startParent) SpeakCurrentItem();
				else SpeakWithParentContext();
				return true;
			}

			// Neighbor empty — wrap to last populated parent
			for (int i = parentCount - 1; i >= 0; i--) {
				_indices[_level - 1] = i;
				childCount = GetItemCount(_level, _indices);
				if (childCount > 0) {
					_indices[_level] = landOnLast ? childCount - 1 : 0;
					SyncCurrentIndex();
					PlayWrapSound();
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
			int levelBefore = _level;
			SkipSingleItemLevels();

			int count = GetItemCount(_level, _indices);
			if (count > 0) {
				if (_level > levelBefore)
					SpeakWithParentContext();
				else
					SpeakCurrentItem();
			}
		}

		protected void SkipSingleItemLevels() {
			while (_level < MaxLevel && GetItemCount(_level, _indices) == 1) {
				_level++;
				_indices[_level] = 0;
			}
			SyncCurrentIndex();
		}

		private void GoBack() {
			_level--;
			_search.Clear();
			SyncCurrentIndex();
			SpeakCurrentItem();
		}

		protected void SpeakWithParentContext() {
			string parentLabel = GetParentLabel(_level, _indices);
			SpeakCurrentItem(parentLabel);
		}

		/// <summary>
		/// Copy _indices[_level] to _currentIndex for base class compatibility.
		/// </summary>
		private void SyncCurrentIndex() {
			_currentIndex = _indices[_level];
		}

		/// <summary>
		/// Copy _currentIndex back to _indices[_level] after base class
		/// methods (NavigateNext/Prev/First/Last) modify _currentIndex directly.
		/// </summary>
		private void SyncFromCurrentIndex() {
			_indices[_level] = _currentIndex;
		}
	}
}
