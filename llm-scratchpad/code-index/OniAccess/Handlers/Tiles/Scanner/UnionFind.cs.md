# UnionFind.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Array-based union-find with path compression (path halving) and union by rank. Allocated once per clustering domain in `GridScanner` and reset per scan to avoid reallocation when the grid size is unchanged.

```
class UnionFind (line 6)

  private int[] _parent (line 7)
  private int[] _rank (line 8)
  private int _size (line 9)

  UnionFind(int size) (line 11)

  void Reset(int size) (line 22)
    -- Re-initializes for a new scan. Reuses existing arrays if size matches; reallocates if different.

  void Union(int a, int b) (line 31)
    -- Merges the sets containing a and b using union by rank.

  int Find(int cell) (line 45)
    -- Returns the root of cell's set, using path halving for compression.

  private void InitArrays() (line 53)
    -- Sets parent[i] = i and rank[i] = 0 for all i.
```
