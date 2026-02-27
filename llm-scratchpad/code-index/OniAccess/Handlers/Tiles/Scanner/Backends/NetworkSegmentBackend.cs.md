# NetworkSegmentBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for wire/conduit segment clusters and bridge instances. Segments are clustered (from `GridScanner`); bridges are individual instances. Both are emitted into the same scanner subcategory (e.g., `Networks > Power`).

```
class NetworkSegmentBackend : IScannerBackend (line 9)

  private List<NetworkSegmentCluster> _segments (line 10)
  private List<BridgeInstance> _bridges (line 11)

  void SetGridData(List<NetworkSegmentCluster> segments, List<BridgeInstance> bridges) (line 13)
    -- Called by ScannerNavigator.Refresh() before Scan().

  IEnumerable<ScanEntry> Scan(int worldId) (line 20)
    -- Yields entries for all segment clusters (BackendData = NetworkSegmentCluster)
    -- followed by all bridge instances (BackendData = BridgeInstance).

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 51)
    -- For bridges: checks GameObject is not null/destroyed.
    -- For segment clusters: iterates cells in reverse, removes cells where segment is no longer present,
    --   updates entry.Cell to closest remaining cell to cursor.

  string FormatName(ScanEntry entry) (line 77)
    -- For segment clusters: returns entry.ItemName if single-cell; "N name" (CLUSTER_LABEL) if multi-cell.
    -- For bridges: returns entry.ItemName unchanged.

  private static string GetSegmentName(NetworkSegmentCluster cluster) (line 87)
    -- Finds the segment's object layer via FindSegmentLayer, reads KSelectable.GetName()
    -- from the first cell that has a matching object. Falls back to cluster.PrefabId.

  private static int FindSegmentLayer(NetworkSegmentCluster cluster) (line 100)
    -- Looks up NetworkLayerConfig.Types to find the SegmentLayer matching the cluster's
    -- scanner category and subcategory. Returns -1 if not found.

  private static bool IsSegmentStillPresent(int cell, NetworkSegmentCluster cluster) (line 110)
    -- Checks all matching network types in NetworkLayerConfig for a building at the cell
    -- with the expected PrefabId.
```
