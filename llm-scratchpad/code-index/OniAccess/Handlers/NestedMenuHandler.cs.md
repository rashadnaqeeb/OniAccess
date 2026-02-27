# NestedMenuHandler.cs

## File-level comment
Multi-level navigation on top of `BaseMenuHandler`'s infrastructure. Level 0
is the root (e.g., subcategories); deeper levels are children (e.g., buildings
within a subcategory). Navigation crosses parent boundaries at levels > 0:
moving past the last child wraps into the next parent, and vice versa.
Type-ahead searches a configurable level (`SearchLevel`) regardless of the
current navigation level.

---

```
abstract class NestedMenuHandler : BaseMenuHandler, ISearchable (line 14)

  // Private fields
  private int _level                                      (line 15)
  private int[] _indices                                  (line 16)
  // Per-level cursor indices. Array size 8. _indices[_level] is the active cursor.

  // Constructor
  protected NestedMenuHandler(KScreen screen = null)      (line 18)

  // Protected properties
  protected int Level { get; }                            (line 20)
  protected virtual int StartLevel { get; }               (line 21)
  // Default 0. Override to start at a deeper level.

  protected int GetIndex(int level)                       (line 23)
  protected void SetIndex(int level, int value)           (line 24)

  // Abstract: level-aware members
  protected abstract int MaxLevel { get; }                (line 33)
  // Deepest navigable level (e.g., 1 for a two-level subcategory+building menu).

  protected abstract int GetItemCount(int level, int[] indices) (line 39)
  // Item count at a given level. Parent indices provide context.

  protected abstract string GetItemLabel(int level, int[] indices) (line 44)
  protected abstract void ActivateLeafItem(int[] indices) (line 49)
  // Activate an item at MaxLevel (the leaf action, e.g., place a building).

  protected abstract int SearchLevel { get; }             (line 54)
  // Which level type-ahead search targets.

  protected abstract int GetSearchItemCount(int[] indices) (line 59)
  // Total searchable items across all parents at SearchLevel (flat count).

  protected abstract string GetSearchItemLabel(int flatIndex) (line 64)
  // Label for a flat search index at SearchLevel.

  protected abstract void MapSearchIndex(int flatIndex, int[] outIndices) (line 69)
  // Converts a flat search index back into the full indices array.

  protected abstract string GetParentLabel(int level, int[] indices) (line 75)
  // Label for the parent group at the given indices. Used when announcing group
  // changes during cross-boundary navigation.

  protected virtual int GetSearchTargetLevel(int flatIndex, int[] mappedIndices) (line 82)
  // Returns the level to set when search lands on flatIndex. Default: SearchLevel.
  // Override when some items are leaves at a shallower level.

  // Base class bridges (sealed — must not be overridden)
  sealed override int ItemCount { get; }                  (line 88)
  // Returns GetItemCount(_level, _indices).

  sealed override string GetItemLabel(int index)          (line 90)
  // Temporarily sets _indices[_level] = index, calls GetItemLabel(_level, _indices),
  // then restores. Adapts the level-aware abstract to the flat base class API.

  override void SpeakCurrentItem(string parentContext = null) (line 98)
  // Syncs _indices[_level] from _currentIndex, then speaks the label.
  // Prepends parentContext if provided.

  // Lifecycle
  override void OnActivate()                              (line 116)
  // Resets _level to StartLevel, zeroes all _indices, then calls base.

  override void OnDeactivate()                            (line 123)
  // Resets _level to 0 and zeroes all _indices, then calls base.

  // Navigation overrides
  protected override void NavigateNext()                  (line 134)
  // At level 0: delegates to base. At deeper levels: increments index within
  // current parent. If past end of parent, calls JumpToNextParent(landOnLast: false).

  protected override void NavigatePrev()                  (line 155)
  // At level 0: delegates to base. At deeper levels: decrements index within
  // current parent. If before start of parent, calls JumpToPrevParent(landOnLast: true).

  protected override void NavigateFirst()                 (line 173)
  // At level 0: delegates to base. At deeper levels: scans parents forward to
  // find first parent with children and lands on index 0.

  protected override void NavigateLast()                  (line 194)
  // At level 0: delegates to base. At deeper levels: scans parents backward to
  // find last parent with children and lands on last child.

  // Left/Right: drill down / go back
  protected override void HandleLeftRight(int direction, int stepLevel) (line 219)
  // Right: drills down if _level < MaxLevel and current item has children.
  // Left: goes back one level if _level > 0.

  // Enter / Escape
  protected override void ActivateCurrentItem()           (line 231)
  // At max level or leaf: calls ActivateLeafItem.
  // Otherwise: drills down (same as Right arrow).

  private bool CanDrillDown()                             (line 242)
  // Returns true if current item has children at the next level.

  override bool HandleKeyDown(KButtonEvent e)             (line 246)
  // Delegates to base (search clear on Escape).

  // ISearchable — explicit re-implementation
  // NOTE: explicit to prevent base class members from being called by TypeAheadSearch,
  // which would update _currentIndex without updating _indices or _level.
  int ISearchable.SearchItemCount { get; }                (line 258)
  string ISearchable.GetSearchLabel(int index)            (line 260)
  void ISearchable.SearchMoveTo(int index)                (line 266)
  // Calls NestedSearchMoveTo(index, parentContext: false).

  protected void NestedSearchMoveTo(int index, bool parentContext = true) (line 270)
  // Maps flat index to _indices + _level via MapSearchIndex/GetSearchTargetLevel,
  // then speaks with or without parent context.

  // Help entries
  protected static readonly List<HelpEntry> NestedNavHelpEntries (line 284)
  // Contains: A-Z search, Up/Down, Ctrl+Up/Down (jump group), Home/End,
  // Enter/Right (open group), Left (go back).

  // Group jumping
  protected override void JumpNextGroup()                 (line 297)
  // At level 0: NavigateNext. At deeper levels: JumpToNextParent(landOnLast: false).

  protected override void JumpPrevGroup()                 (line 303)
  // At level 0: NavigatePrev. At deeper levels: JumpToPrevParent(landOnLast: false).

  private bool JumpToNextParent(bool landOnLast)          (line 307)
  // Advances parent index by 1 (wrapping). Skips empty parents. Announces with
  // parent context when the parent changes. Returns false if no populated sibling
  // parent exists.

  private bool JumpToPrevParent(bool landOnLast)          (line 345)
  // Same as JumpToNextParent but in reverse direction.

  // Private helpers
  private void DrillDown()                                (line 387)
  // Increments _level, resets child index to 0, clears search, syncs cursor,
  // and speaks current item.

  private void GoBack()                                   (line 398)
  // Decrements _level, clears search, syncs cursor, and speaks current item.

  protected void ResetState()                             (line 405)
  // Resets _level to StartLevel, zeroes all indices and _currentIndex,
  // clears search, and suppresses search for this frame.

  private void SpeakWithParentContext()                   (line 414)
  // Gets parent label via GetParentLabel and calls SpeakCurrentItem with it.

  protected void SyncCurrentIndex()                       (line 421)
  // Copies _indices[_level] to _currentIndex for base class compatibility.
  // Called after any direct mutation of _indices.

  private void SyncFromCurrentIndex()                     (line 429)
  // Copies _currentIndex back to _indices[_level] after base class navigation
  // methods (NavigateNext/Prev/First/Last) modify _currentIndex directly.
```
