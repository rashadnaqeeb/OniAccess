# BiomeBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for `Zones > Biomes`. Receives pre-clustered `BiomeCluster` objects from `GridScanner`. Each contiguous same-`ZoneType` region is one scanner instance.

```
class BiomeBackend : IScannerBackend (line 9)

  private List<BiomeCluster> _clusters (line 10)

  void SetGridData(List<BiomeCluster> clusters) (line 12)
    -- Called by ScannerNavigator.Refresh() before Scan() to inject grid scan results.

  IEnumerable<ScanEntry> Scan(int worldId) (line 16)
    -- Yields one ScanEntry per cluster. Category = Zones, Subcategory = Biomes.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 30)
    -- Iterates cluster.Cells in reverse; removes cells where the ZoneType no longer matches.
    -- Updates entry.Cell to the remaining cell nearest to cursorCell. Returns false if no cells survive.

  string FormatName(ScanEntry entry) (line 54)
    -- Returns cluster.DisplayName for single-cell clusters;
    -- "N displayName" (CLUSTER_LABEL) for multi-cell clusters.
```
