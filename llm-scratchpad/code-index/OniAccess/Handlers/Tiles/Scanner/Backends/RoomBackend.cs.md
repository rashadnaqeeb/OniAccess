# RoomBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for `Zones > Rooms`. Iterates `Game.Instance.roomProber.rooms` directly; no pre-processed grid data. Skips `Neutral` room type. Teleport target is the cell in the room's cavity nearest to the cursor.

```
class RoomBackend : IScannerBackend (line 9)

  IEnumerable<ScanEntry> Scan(int worldId) (line 11)
    -- Iterates roomProber.rooms; skips rooms without a cavity, empty cavities,
    -- rooms in a different world, invisible rooms, and rooms of the Neutral type.
    -- BackendData = Room object.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 34)
    -- Checks the room still exists in roomProber.rooms.
    -- Updates entry.Cell to the cavity cell nearest to cursorCell.

  string FormatName(ScanEntry entry) (line 53)
    -- Returns room.roomType.Name.
```
