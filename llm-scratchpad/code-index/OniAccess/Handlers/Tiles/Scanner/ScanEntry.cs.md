# ScanEntry.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

One instance in the 4-level scanner hierarchy: Category > Subcategory > ItemName > instance.
Multiple `ScanEntry` objects with the same Category/Subcategory/ItemName form the instance list for that item.

```
class ScanEntry (line 7)
  int Cell (line 8)                   -- representative cell; updated by ValidateEntry to closest valid cell
  IScannerBackend Backend (line 9)
  object BackendData (line 10)        -- backend-specific payload (cluster, GameObject, Room, etc.)
  string Category (line 11)
  string Subcategory (line 12)
  string ItemName (line 13)
```
