# ElementClusterBackend.cs

Namespace: `OniAccess.Handlers.Tiles.Scanner.Backends`

Backend for natural elements (`Solids`, `Liquids`, `Gases` categories). Receives pre-clustered `ElementCluster` objects from `GridScanner`.

```
class ElementClusterBackend : IScannerBackend (line 8)

  private List<ElementCluster> _clusters (line 9)

  void SetGridData(List<ElementCluster> clusters) (line 11)
    -- Called by ScannerNavigator.Refresh() before Scan() to inject grid scan results.

  IEnumerable<ScanEntry> Scan(int worldId) (line 15)
    -- Yields one ScanEntry per cluster. Category/Subcategory come from the cluster.

  bool ValidateEntry(ScanEntry entry, int cursorCell) (line 29)
    -- Iterates cluster.Cells in reverse; removes cells where element ID no longer matches.
    -- Updates entry.Cell to the remaining cell nearest to cursorCell. Returns false if no cells survive.

  string FormatName(ScanEntry entry) (line 53)
    -- Returns cluster.ElementName for single-cell clusters;
    -- "N elementName" (CLUSTER_LABEL) for multi-cell clusters.
```
