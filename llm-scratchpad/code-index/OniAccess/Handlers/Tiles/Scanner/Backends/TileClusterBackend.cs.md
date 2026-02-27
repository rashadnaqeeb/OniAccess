# TileClusterBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for constructed tiles (`Solids > Tiles`, plus routed exceptions such as `FarmTile` which goes to `Buildings > Farming`). Receives pre-clustered `TileCluster` objects from `GridScanner`.

```
class TileClusterBackend : IScannerBackend (line 9)

  private List<TileCluster> _clusters (line 10)

  void SetGridData(List<TileCluster> clusters) (line 12)
    -- Called by ScannerNavigator.Refresh() before Scan().

  IEnumerable<ScanEntry> Scan(int worldId) (line 16)
    -- Yields one ScanEntry per cluster. ItemName is read via GetTileName.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 31)
    -- Iterates cluster.Cells in reverse; removes cells where the tile is no longer present
    -- or has a different prefab ID. Updates entry.Cell to the closest remaining cell.

  string FormatName(ScanEntry entry) (line 54)
    -- Returns entry.ItemName for single-cell clusters; "N name" (CLUSTER_LABEL) for multi-cell.

  private static string GetTileName(TileCluster cluster) (line 62)
    -- Reads KSelectable.GetName() from the first cell in the cluster that has a valid object.
    -- Falls back to cluster.PrefabId.

  private static bool IsTileStillPresent(int cell, string expectedPrefabId) (line 73)
    -- Checks FoundationTile or LadderTile layer for a Building with the expected prefab ID.
```
