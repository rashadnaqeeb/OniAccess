# GeyserBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for `Buildings > Geysers`. Iterates `Components.Geysers` and `Components.GeothermalVents`. Each geyser/vent is one instance. No pre-processed grid data needed; the backend queries the game's component lists directly.

```
class GeyserBackend : IScannerBackend (line 9)

  IEnumerable<ScanEntry> Scan(int worldId) (line 11)
    -- Yields one entry per visible geyser and geothermal vent.
    -- Category = Buildings, Subcategory = Geysers.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 27)
    -- Checks the GameObject is not null/destroyed and the stored cell is still visible.
    -- Does not update entry.Cell (geysers don't move).

  string FormatName(ScanEntry entry) (line 33)
    -- Returns UserNameable.savedName if set; otherwise KSelectable.GetName(); falls back to entry.ItemName.

  private ScanEntry MakeEntry(GameObject go, int cell) (line 38)
    -- Constructs a ScanEntry with name from GetGeyserName.

  private static string GetGeyserName(GameObject go) (line 50)
    -- Prefers UserNameable.savedName (player-assigned name), falls back to KSelectable.GetName().
```
