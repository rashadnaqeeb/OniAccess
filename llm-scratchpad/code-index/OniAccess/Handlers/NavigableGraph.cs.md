# NavigableGraph.cs

## File-level comment
Reusable cursor for navigating a directed acyclic graph. Up/Down follows edges
between parents and children. Left/Right cycles among siblings (nodes sharing
the same origin from the last Up/Down move).

All neighbor lookups are computed on demand via caller-supplied lambdas. No
graph structure is cached internally.

---

```
class NavigableGraph<T> where T : class (line 16)

  // Private fields
  private readonly Func<T, IReadOnlyList<T>> _getParents  (line 17)
  private readonly Func<T, IReadOnlyList<T>> _getChildren (line 18)
  private readonly Func<IReadOnlyList<T>> _getRoots       (line 19)
  private T _current                                      (line 21)
  private IReadOnlyList<T> _siblings                      (line 22)
  // Sibling list established by the most recent Up/Down move, used for Left/Right cycling.

  private int _siblingIndex                               (line 23)

  // Properties
  T Current { get; }                                      (line 25)

  // Constructor
  NavigableGraph(
    Func<T, IReadOnlyList<T>> getParents,
    Func<T, IReadOnlyList<T>> getChildren,
    Func<IReadOnlyList<T>> getRoots = null)                (line 27)

  // Methods
  void MoveTo(T node)                                     (line 40)
  // Sets current node without establishing sibling context.
  // Left/Right does nothing until the first Up or Down.

  void MoveToWithSiblings(T node, IReadOnlyList<T> siblings) (line 50)
  // Sets current node with an explicit sibling list for immediate Left/Right use.

  T NavigateDown()                                        (line 61)
  // Moves to first child of current node. Sets siblings to children list.
  // Returns new current node, or null if no children.

  T NavigateUp()                                          (line 79)
  // Moves to first parent. Sets siblings based on context:
  // - If at root and getRoots provided: establishes root sibling context.
  // - If landing on a root node: uses roots as siblings.
  // - Otherwise: uses the parents list as siblings.
  // Returns new current node, or null if already at root.

  T CycleSibling(int direction, out bool wrapped)         (line 114)
  // Cycles among current sibling list (wraps around).
  // Returns new current node, or null if no sibling context or only one sibling.
  // 'wrapped' is true if the cycle crossed a boundary.

  bool HasChildren { get; }                               (line 126)
  bool HasParents { get; }                                (line 134)
  bool HasSiblings { get; }                               (line 142)

  private static int IndexOf(IReadOnlyList<T> list, T item) (line 144)
  // Returns the index of item in list by reference equality. Returns 0 if not found.
```
