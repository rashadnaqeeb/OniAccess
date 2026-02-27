# ScannerSnapshot.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Contains three plain data classes for the snapshot hierarchy nodes, and the `ScannerSnapshot` class that builds and owns the full 4-level tree.

---

## Plain hierarchy node classes (lines 4-17)

```
class ScannerItem (line 4)
  string ItemName (line 5)
  List<ScanEntry> Instances (line 6)

class ScannerSubcategory (line 9)
  string Name (line 10)
  List<ScannerItem> Items (line 11)

class ScannerCategory (line 14)
  string Name (line 15)
  List<ScannerSubcategory> Subcategories (line 16)
```

---

## class ScannerSnapshot (line 26)

Frozen 4-level hierarchy built from a flat list of `ScanEntry` objects. Categories and subcategories follow `ScannerTaxonomy` ordering. The "all" subcategory at index 0 of each category holds **shared** `ScannerItem` references — removing an instance from a named subcategory's item automatically removes it from "all" as well.

```
  List<ScannerCategory> Categories (line 27)   -- public readonly

  ScannerSnapshot(List<ScanEntry> entries, int cursorCell) (line 29)

  int CategoryCount { get; } (line 33)
  ScannerCategory GetCategory(int ci) (line 35)
  ScannerSubcategory GetSubcategory(int ci, int si) (line 37)
  ScannerItem GetItem(int ci, int si, int ii) (line 40)
  ScanEntry GetInstance(int ci, int si, int ii, int ni) (line 43)

  void RemoveInstance(ScannerItem item, ScanEntry entry) (line 52)
    -- Removes entry from item.Instances. If item becomes empty, calls PruneEmptyItem which
    -- removes the item from all subcategory lists and prunes empty subcategories and categories.

  private static List<ScannerCategory> Build(List<ScanEntry> entries, int cursorCell) (line 58)
    -- Groups entries by category/subcategory/itemName, sorts instances by distance to cursor,
    -- sorts items by nearest-instance distance, sorts subcategories by ScannerTaxonomy order,
    -- prepends an "all" subcategory containing shared ScannerItem references.

  private void PruneEmptyItem(ScannerItem item) (line 138)
    -- Removes item from every subcategory in every category, then prunes empty subcategories
    -- and categories. Iterates in reverse to avoid index shifting.
```
