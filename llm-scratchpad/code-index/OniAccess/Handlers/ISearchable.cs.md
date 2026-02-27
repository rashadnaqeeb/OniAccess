# ISearchable.cs

## File-level comment
Interface for handlers that support type-ahead search via `TypeAheadSearch.HandleKey`.
Handlers implement this to describe their searchable list at the current navigation level.

---

```
interface ISearchable (line 6)

  int SearchItemCount { get; }          (line 11)
  // Number of searchable items at the current navigation level.
  // Return 0 to disable search (A-Z keys pass through to handler).

  string GetSearchLabel(int index)      (line 17)
  // Searchable label for the item at index.
  // Return null to skip an item in search results.

  void SearchMoveTo(int index)          (line 23)
  // Move cursor to index and announce. Called during search navigation
  // and when search results are found. The move is permanent.
```
