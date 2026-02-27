# IScannerBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner`

Backend interface. All backends are stateless between refreshes — they query game state fresh each time `Scan()` is called. Distance sorting happens in `ScannerSnapshot`, not here.

```
interface IScannerBackend (line 9)
  IEnumerable<ScanEntry> Scan(int worldId) (line 15)
    -- Query game state and return entries for the given world.
    -- Cursor position is not passed; distance sorting is done in ScannerSnapshot after all backends return.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 22)
    -- Called when the user navigates to an instance.
    -- Returns false if the entry is stale and should be removed from the snapshot.
    -- Implementations also update entry.Cell to the closest valid cell to the cursor.

  string FormatName(ScanEntry entry) (line 27)
    -- Return the spoken label for this instance (may include tile count prefix for clusters).
```
